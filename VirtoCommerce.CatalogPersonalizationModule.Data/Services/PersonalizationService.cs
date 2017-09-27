using System;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class PersonalizationService : ServiceBase, ITaggedItemService, ITaggedItemSearchService
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;

        public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public TaggedItem[] GetTaggedItemsByIds(string[] ids)
        {
            TaggedItem[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetTaggedItemsByIds(ids).Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
                }
            }
            return retVal;
        }

        public TaggedItem[] GetTaggedItemsByObjectIds(string[] ids)
        {
            TaggedItem[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    retVal = repository.GetTaggedItemsByObjectIds(ids).Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
                }
            }
            return retVal;
        }

        public void SaveTaggedItems(TaggedItem[] taggedItems)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                using (var changeTracker = GetChangeTracker(repository))
                {
                    var ids = taggedItems.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                    var alreadyExistEntities = repository.TaggedItems.Where(x => ids.Contains(x.Id)).ToArray();
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
                var query = repository.TaggedItems;
                if (!string.IsNullOrWhiteSpace(criteria.EntityId))
                {
                    query = query.Where(x => x.ObjectId == criteria.EntityId);
                }

                if (!string.IsNullOrWhiteSpace(criteria.EntityType))
                {
                    query = query.Where(x => x.ObjectType == criteria.EntityType);
                }

                if (!criteria.Ids.IsNullCollection())
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
                retVal.Results = GetTaggedItemsByIds(ids).AsQueryable().OrderBySortInfos(sortInfos).ToList();
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
