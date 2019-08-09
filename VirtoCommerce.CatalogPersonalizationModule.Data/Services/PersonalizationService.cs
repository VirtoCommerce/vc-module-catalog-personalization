using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
	public class PersonalizationService : ServiceBase, ITaggedItemService, ITaggedItemSearchService, ITaggedItemOutlineService
	{
		private readonly Func<IPersonalizationRepository> _repositoryFactory;

		public PersonalizationService(Func<IPersonalizationRepository> repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public TaggedItem[] GetTaggedItemsByIds(string[] ids) => GetTaggedItemsByIds(ids, null);

		public TaggedItem[] GetTaggedItemsByIds(string[] ids, string responseGroup)
		{
			TaggedItem[] retVal = null;
			if (ids != null)
			{
				using (var repository = _repositoryFactory())
				{
					retVal = repository.GetTaggedItemsByIds(ids, responseGroup).Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
				}
			}
			return retVal;
		}

		public TaggedItem[] GetTaggedItemsByObjectIds(string[] ids)
		{
			var retVal = new List<TaggedItem>();
			if (ids != null)
			{
				using (var repository = _repositoryFactory())
				{
					foreach (var chunkIds in ids.SplitList(50))
					{
						retVal.AddRange(repository.GetTaggedItemsByObjectIds(chunkIds).Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())));
					}
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

				string responseGroup = criteria.ResponseGroup;

				retVal.TotalCount = query.Count();
				query = query.Skip(criteria.Skip).Take(criteria.Take);

				var ids = query.Select(x => x.Id).ToArray();
				retVal.Results = GetTaggedItemsByIds(ids, responseGroup).AsQueryable().OrderBySortInfos(sortInfos).ToList();
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

		public string[] GetTagsByOutlinePart(string outlinePart)
		{
			var result = Array.Empty<string>();
			if (!string.IsNullOrEmpty(outlinePart))
			{
				using (var repository = _repositoryFactory())
				{
					result = repository.GetTagsByOutlinePart(outlinePart);
				}
			}
			return result;
		}


		public void SaveTaggedItemOutlines(TaggedItemOutline[] taggedItemOutlines)
		{
			var pkMap = new PrimaryKeyResolvingMap();
			using (var repository = _repositoryFactory())
			{
				using (var changeTracker = GetChangeTracker(repository))
				{
					// As we construct outlines without loading them from database = they do not have Id, we need to:
					// - Assign Id to model which have existing entity by corresponding TaggedItemId and Outline
					// - Add new entities ones for those nodels which do not have corresponding TaggedItemId and Outline
					// - Delete existing entities with no TaggedItemId and Outline presented in taggedItemOutlines

					var outlinesByTaggedItemId = taggedItemOutlines.ToLookup(x => x.TaggedItemId);
					var taggedItemIds = taggedItemOutlines.Select(x => x.TaggedItemId).Where(x => x != null).Distinct().ToArray();
					var alreadyExistEntities = repository.TaggedItemOutlines.Where(x => taggedItemIds.Contains(x.TaggedItemId)).ToArray();
					var existentEntitiesByTaggedItemId = alreadyExistEntities.ToLookup(x => x.TaggedItemId);

					foreach (var grouping in outlinesByTaggedItemId)
					{
						var taggedItemId = grouping.Key;
						var itemOutlines = grouping.ToArray();
						var existentItemOutlines = existentEntitiesByTaggedItemId[taggedItemId];
						var existentToDelete = existentItemOutlines.Where(existent => !itemOutlines.Any(x => x.Outline == existent.Outline)).ToArray();

						foreach (var itemOutline in itemOutlines)
						{
							var sourceEntity = AbstractTypeFactory<TaggedItemOutlineEntity>.TryCreateInstance().FromModel(itemOutline, pkMap);
							var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Outline == itemOutline.Outline);

							if (targetEntity != null)
							{
								changeTracker.Attach(targetEntity);
								sourceEntity.Id = targetEntity.Id;
							}
							else
							{
								repository.Add(sourceEntity);
							}
						}

						foreach (var toDelete in existentToDelete)
						{
							repository.Remove(toDelete);
						}
					}
				}
				CommitChanges(repository);
				pkMap.ResolvePrimaryKeys();
			}

		}

		public void DeleteTaggedItemOutlines(string[] ids)
		{
			using (var repository = _repositoryFactory())
			{
				repository.DeleteTaggedItemOutlines(ids);
				CommitChanges(repository);
			}
		}
	}
}
