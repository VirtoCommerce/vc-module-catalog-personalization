using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using Category = VirtoCommerce.CatalogModule.Web.Model.Category;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class PersonalizationService : ServiceBase, ITaggedItemService, ITaggedItemSearchService
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITagPropagationPolicy _tagPropagationPolicy;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;

        public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory, ITagPropagationPolicy tagPropagationPolicy, IItemService itemService, ICategoryService categoryService)
        {
            _repositoryFactory = repositoryFactory;
            _tagPropagationPolicy = tagPropagationPolicy;
            _itemService = itemService;
            _categoryService = categoryService;
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

        private List<TaggedItem> EvaluateEffectiveTags(string[] ids)
        {
            var result = new List<TaggedItem>();
            var entities = new List<IEntity>();
            entities.AddRange(_itemService.GetByIds(ids, ItemResponseGroup.Outlines));
            entities.AddRange(_categoryService.GetByIds(ids, CategoryResponseGroup.WithOutlines));

            if (!entities.IsNullOrEmpty())
            {
                var effectiveTagsMap = _tagPropagationPolicy.GetResultingTags(entities.ToArray());
                foreach (var entity in entities)
                {
                    if (effectiveTagsMap.TryGetValue(entity.Id, out List<EffectiveTag> effectiveTags))
                    {
                        var taggedItem = result.FirstOrDefault(x => x.EntityId == entity.Id);

                        if (taggedItem == null)
                        {
                            var label = string.Empty;
                            if (entity is Category category)
                            {
                                label = category.Name;
                            }
                            else if (entity is CatalogProduct product)
                            {
                                label = product.Name;
                            }

                            taggedItem = new TaggedItem
                            {
                                EntityId = entity.Id,
                                EntityType = KnownDocumentTypes.Category,
                                Label = label
                            };
                            result.Add(taggedItem);
                        }

                        taggedItem.Tags = effectiveTags.Where(x => !x.IsInherited)
                            .Select(x => x.Tag)
                            .ToArray();
                        taggedItem.InheritedTags = effectiveTags.Where(x => x.IsInherited)
                            .Select(y => y.Tag)
                            .ToArray();
                    }
                }
            }

            return result;
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

            if (criteria.ResponseGroup.Contains(TaggedItemResponseGroup.WithInheritedTags.ToString()))
            {
                var taggedItems = EvaluateEffectiveTags(criteria.EntityIds);
                retVal.TotalCount = taggedItems.Count;
                retVal.Results = taggedItems;
            }
            else
            {
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

    }
}
