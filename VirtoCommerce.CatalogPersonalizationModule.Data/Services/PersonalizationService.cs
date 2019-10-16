using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class PersonalizationService : ServiceBase, ITaggedItemService, ITaggedItemSearchService
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITagPropagationPolicy _tagPropagationPolicy;
        private readonly ITaggedEntitiesServiceFactory _taggedEntitiesServiceFactory;

        public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory,
            ITagPropagationPolicy tagPropagationPolicy,
            ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory)
        {
            _repositoryFactory = repositoryFactory;
            _tagPropagationPolicy = tagPropagationPolicy;
            _taggedEntitiesServiceFactory = taggedEntitiesServiceFactory;
        }

        public TaggedItem[] GetTaggedItemsByIds(string[] ids, string responseGroup = null)
        {
            var retVal = new List<TaggedItem>();
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    retVal = repository.GetTaggedItemsByIds(ids, responseGroup)
                        .Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance()))
                        .ToList();
                }
            }
            return retVal.ToArray();
        }

        public void SaveTaggedItems(TaggedItem[] taggedItems)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                using (var changeTracker = GetChangeTracker(repository))
                {
                    var ids = taggedItems.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                    var alreadyExistEntities = repository.GetTaggedItemsByIds(ids, null).ToArray();
                    foreach (var taggedItem in taggedItems)
                    {
                        var sourceEntity = AbstractTypeFactory<TaggedItemEntity>.TryCreateInstance().FromModel(taggedItem, pkMap);
                        var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == taggedItem.Id);
                        if (targetEntity != null)
                        {
                            changeTracker.Attach(targetEntity);
                            sourceEntity.Patch(targetEntity);
                        }
                        else
                        {
                            repository.Add(sourceEntity);
                        }
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }
        }

        public GenericSearchResult<TaggedItem> SearchTaggedItems(TaggedItemSearchCriteria criteria)
        {
            var retVal = new GenericSearchResult<TaggedItem>();


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
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<TaggedItem>(x => x.Label) } };
                }
                query = query.OrderBySortInfos(sortInfos);

                retVal.TotalCount = query.Count();
                query = query.Skip(criteria.Skip).Take(criteria.Take);

                var ids = query.Select(x => x.Id).ToArray();
                retVal.Results = GetTaggedItemsByIds(ids, criteria.ResponseGroup).AsQueryable().OrderBySortInfos(sortInfos).ToList();

                if (criteria.ResponseGroup.Contains(TaggedItemResponseGroup.WithInheritedTags.ToString()) &&
                    !criteria.EntityIds.IsNullOrEmpty())
                {
                    var taggedItems = FillInheritedTags(criteria.EntityIds, retVal.Results.ToList());
                    retVal.TotalCount = taggedItems.Count;
                    retVal.Results = taggedItems;
                }
            }

            return retVal;
        }

        public void DeleteTaggedItems(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DeleteTaggedItems(ids);
                CommitChanges(repository);
            }
        }


        private List<TaggedItem> FillInheritedTags(string[] entityIds, List<TaggedItem> taggedItems = null)
        {
            var result = taggedItems ?? new List<TaggedItem>();
            var evaluatedEntityIds = new List<string>();

            if (!result.IsNullOrEmpty())
            {
                evaluatedEntityIds = result.Select(x => x.EntityId).ToList();
                foreach (var taggedItem in result)
                {
                    var entities = _taggedEntitiesServiceFactory.Create(taggedItem.EntityType).GetEntitiesByIds(new[] { taggedItem.EntityId }).ToList();
                    var evaluatedItems = EvaluateEffectiveTags(entities);
                    taggedItem.InheritedTags = evaluatedItems.FirstOrDefault(x => x.EntityId == taggedItem.EntityId)?.InheritedTags;
                }
            }

            entityIds = entityIds.Where(ids => evaluatedEntityIds.All(evaluatedIds => evaluatedIds != ids)).ToArray();

            var items = _taggedEntitiesServiceFactory.Create(KnownDocumentTypes.Product).GetEntitiesByIds(entityIds).ToList();

            if (!items.IsNullOrEmpty())
            {
                result.AddRange(EvaluateEffectiveTags(items));
            }

            var categories = _taggedEntitiesServiceFactory.Create(KnownDocumentTypes.Category).GetEntitiesByIds(entityIds).ToList();

            if (!categories.IsNullOrEmpty())
            {
                result.AddRange(EvaluateEffectiveTags(categories));
            }
            return result;
        }

        private List<TaggedItem> EvaluateEffectiveTags(List<IEntity> entities)
        {
            var result = new List<TaggedItem>();

            var effectiveTagsMap = _tagPropagationPolicy.GetResultingTags(entities.ToArray());
            foreach (var entity in entities)
            {
                if (effectiveTagsMap.TryGetValue(entity.Id, out List<EffectiveTag> effectiveTags))
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
