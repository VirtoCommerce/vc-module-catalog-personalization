using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Caching;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class PersonalizationService : ITaggedItemService, ITaggedItemSearchService
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITaggedEntitiesServiceFactory _taggedEntitiesServiceFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly ITagPropagationPolicy _tagPropagationPolicy;

        public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory,
            ITagPropagationPolicy tagPropagationPolicy,
            ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory,
            IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _tagPropagationPolicy = tagPropagationPolicy;
            _taggedEntitiesServiceFactory = taggedEntitiesServiceFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<TaggedItemSearchResult> SearchTaggedItemsAsync(TaggedItemSearchCriteria criteria)
        {

            var cacheKey = CacheKey.With(GetType(), nameof(SearchTaggedItemsAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey,
                async (cacheEntry) =>
                {
                    cacheEntry.AddExpirationToken(PersonalizationCacheRegion.CreateChangeToken());
                    var result = new TaggedItemSearchResult();

                    using (var repository = _repositoryFactory())
                    {
                        repository.DisableChangesTracking();

                        IQueryable<TaggedItemEntity> query = repository.TaggedItems.Include(x => x.Tags);

                        if (!criteria.EntityIds.IsNullOrEmpty())
                        {
                            query = query.Where(x => criteria.EntityIds.Contains(x.ObjectId));
                        }

                        if (!string.IsNullOrWhiteSpace(criteria.EntityType))
                        {
                            query = query.Where(x => x.ObjectType == criteria.EntityType);
                        }

                        if (!criteria.Ids.IsNullOrEmpty())
                        {
                            query = query.Where(x => criteria.Ids.Contains(x.Id));
                        }

                        if (criteria.ChangedFrom.HasValue)
                        {
                            query = query.Where(x => x.ModifiedDate.HasValue && x.ModifiedDate.Value.Date >= criteria.ChangedFrom.Value.Date);
                        }

                        var sortInfos = criteria.SortInfos;
                        if (sortInfos.IsNullOrEmpty())
                        {
                            sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<TaggedItem>(x => x.Label) } };
                        }

                        query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                        result.TotalCount = query.Count();
                        query = query.Skip(criteria.Skip).Take(criteria.Take);

                        if (criteria.Take > 0)
                        {
                            var ids = query.Select(x => x.Id).ToArray();
                            var selectedItems = await GetByIdsAsync(ids, criteria.ResponseGroup);
                            result.Results = selectedItems.OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();

                        }

                        var taggedItemResponseGroup = EnumUtility.SafeParseFlags(criteria.ResponseGroup, TaggedItemResponseGroup.Info);

                        if (taggedItemResponseGroup.HasFlag(TaggedItemResponseGroup.WithInheritedTags) && !criteria.EntityIds.IsNullOrEmpty())
                        {
                            var taggedItems = await FillInheritedTags(criteria.EntityIds, result.Results);
                            result.TotalCount = taggedItems.Count;
                            result.Results = taggedItems;
                        }
                    }

                    return result;
                });
        }

        public async Task<TaggedItem[]> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            var cacheKey = CacheKey.With(GetType(), "GetByIdsAsync", string.Join("-", ids));

            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey,
                async (cacheEntry) =>
                {
                    cacheEntry.AddExpirationToken(PersonalizationCacheRegion.CreateChangeToken());
                    using (var repository = _repositoryFactory())
                    {
                        repository.DisableChangesTracking();

                        var selectedItems = await repository.GetTaggedItemsByIdsAsync(ids, responseGroup);
                        return selectedItems.Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
                    }
                });
        }

        public async Task SaveChangesAsync(TaggedItem[] taggedItems)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                var ids = taggedItems.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = await repository.GetTaggedItemsByIdsAsync(ids, null);
                foreach (var taggedItem in taggedItems)
                {
                    var sourceEntity = AbstractTypeFactory<TaggedItemEntity>.TryCreateInstance().FromModel(taggedItem, pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == taggedItem.Id);
                    if (targetEntity != null)
                    {
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();
            }

            PersonalizationCacheRegion.ExpireRegion();
        }

        public async Task DeleteAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.DeleteTaggedItemsAsync(ids);
                await repository.UnitOfWork.CommitAsync();
            }

            PersonalizationCacheRegion.ExpireRegion();
        }


        private async Task<List<TaggedItem>> FillInheritedTags(string[] entityIds, ICollection<TaggedItem> assignedTags)
        {
            var result = new List<TaggedItem>(assignedTags);
            var entityIdsWithAssignedTags = new List<string>();

            if (!result.IsNullOrEmpty())
            {
                entityIdsWithAssignedTags = result.Select(x => x.EntityId).Distinct().ToList();

                foreach (var taggedItem in result)
                {
                    var entities = await _taggedEntitiesServiceFactory.Create(taggedItem.EntityType).GetEntitiesByIdsAsync(new[] { taggedItem.EntityId });
                    var evaluatedItems = await EvaluateEffectiveTags(entities.ToList());

                    taggedItem.InheritedTags = evaluatedItems.FirstOrDefault(x => x.EntityId == taggedItem.EntityId)?.InheritedTags;
                }
            }

            var entityIdsWithoutAssignedTags = entityIds.Except(entityIdsWithAssignedTags).ToArray();

            if (!entityIdsWithoutAssignedTags.IsNullOrEmpty())
            {
                var entityTypesWithInheritance = new[] { KnownDocumentTypes.Product, KnownDocumentTypes.Category };
                var entitiesWithoutAssignedTags = new List<IEntity>();

                foreach (var entityType in entityTypesWithInheritance)
                {
                    entitiesWithoutAssignedTags.AddRange(await _taggedEntitiesServiceFactory.Create(entityType).GetEntitiesByIdsAsync(entityIdsWithoutAssignedTags));
                }

                if (!entitiesWithoutAssignedTags.IsNullOrEmpty())
                {
                    result.AddRange(await EvaluateEffectiveTags(entitiesWithoutAssignedTags));
                }
            }

            return result;
        }

        private async Task<List<TaggedItem>> EvaluateEffectiveTags(List<IEntity> entities)
        {
            var result = new List<TaggedItem>();

            var effectiveTagsMap = await _tagPropagationPolicy.GetResultingTagsAsync(entities.ToArray());
            foreach (var entity in entities)
            {
                if (effectiveTagsMap.TryGetValue(entity.Id, out var effectiveTags))
                {
                    var taggedItem = new TaggedItem
                    {
                        EntityId = entity.Id,
                        EntityType = entity.GetType().Name,
                        Tags = effectiveTags.Where(x => !x.IsInherited).Select(x => x.Tag).ToArray(),
                        InheritedTags = effectiveTags.Where(x => x.IsInherited).Select(y => y.Tag).ToArray()
                    };
                    result.Add(taggedItem);
                }
            }

            return result;
        }
    }
}
