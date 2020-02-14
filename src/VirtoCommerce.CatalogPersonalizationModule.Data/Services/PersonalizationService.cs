using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class PersonalizationService : ITaggedItemService, ITaggedItemSearchService
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITaggedEntitiesServiceFactory _taggedEntitiesServiceFactory;
        private readonly ITagPropagationPolicy _tagPropagationPolicy;

        public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory,
            ITagPropagationPolicy tagPropagationPolicy,
            ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory)
        {
            _repositoryFactory = repositoryFactory;
            _tagPropagationPolicy = tagPropagationPolicy;
            _taggedEntitiesServiceFactory = taggedEntitiesServiceFactory;
        }

        public async Task<GenericSearchResult<TaggedItem>> SearchTaggedItemsAsync(TaggedItemSearchCriteria criteria)
        {
            var result = new GenericSearchResult<TaggedItem>();


            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = repository.TaggedItems;

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
                    sortInfos = new[] {new SortInfo {SortColumn = ReflectionUtility.GetPropertyName<TaggedItem>(x => x.Label)}};
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                result.TotalCount = query.Count();
                query = query.Skip(criteria.Skip).Take(criteria.Take);

                if (criteria.Take > 0)
                {
                    var ids = query.Select(x => x.Id).ToArray();
                    var selectedItems = await GetTaggedItemsByIdsAsync(ids, criteria.ResponseGroup);
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
        }

        public async Task<TaggedItem[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup = null)
        {
            var result = new List<TaggedItem>();
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var selectedItems = await repository.GetTaggedItemsByIdsAsync(ids, responseGroup);
                    result = selectedItems.Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance()))
                        .ToList();
                }
            }

            return result.ToArray();
        }

        public async Task SaveTaggedItemsAsync(TaggedItem[] taggedItems)
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
        }

        public async Task DeleteTaggedItemsAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.DeleteTaggedItemsAsync(ids);
                await repository.UnitOfWork.CommitAsync();
            }
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
                    var entities = await _taggedEntitiesServiceFactory.Create(taggedItem.EntityType).GetEntitiesByIdsAsync(new[] {taggedItem.EntityId});
                    var evaluatedItems = await EvaluateEffectiveTags(entities.ToList());

                    taggedItem.InheritedTags = evaluatedItems.FirstOrDefault(x => x.EntityId == taggedItem.EntityId)?.InheritedTags;
                }
            }

            var entityIdsWithoutAssignedTags = entityIds.Except(entityIdsWithAssignedTags).ToArray();

            if (!entityIdsWithoutAssignedTags.IsNullOrEmpty())
            {
                var entityTypesWithInheritance = new[] {KnownDocumentTypes.Product, KnownDocumentTypes.Category};
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