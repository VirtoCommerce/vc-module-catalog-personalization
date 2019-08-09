using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs
{
	public class TaggedItemOutlinesSynchronizationJob
	{
		private const int BatchCount = 50;
		private const string LastIndexationDateSettingName = @"VirtoCommerce.CatalogPersonalizationModule.SynchronizationJob.IndexationDate";

		public const string JobId = "TagOutlinesSynchronization";

		private readonly IChangeLogService _changeLogService;
		private readonly ITaggedItemSearchService _taggedItemSearchService;
		private readonly ITaggedItemOutlineService _taggedItemOutlineService;
		private readonly ICategoryService _categoryService;
		private readonly IItemService _itemService;
		private readonly ISettingsManager _settingsManager;

		public TaggedItemOutlinesSynchronizationJob(IChangeLogService changeLogService,
			ITaggedItemSearchService taggedItemSearchService,
			ITaggedItemOutlineService taggedItemOutlineService,
			ICategoryService categoryService,
			IItemService itemService,
			ISettingsManager settingsManager)
		{
			_changeLogService = changeLogService;
			_taggedItemSearchService = taggedItemSearchService;
			_taggedItemOutlineService = taggedItemOutlineService;
			_categoryService = categoryService;
			_itemService = itemService;
			_settingsManager = settingsManager;
		}

		public Task Run()
		{
			var lastExecutionTime = GetLastExecutionDate();
			var newExecutionDate = DateTime.UtcNow;

			if (lastExecutionTime == null)
			{
				UpdateCategoryOutlines();
				UpdateProductOutlines();
			}
			else
			{
				UpdateOutlinesForChangedObjects(lastExecutionTime, typeof(CategoryEntity).Name, ids => UpdateCategoryOutlines(ids));
				UpdateOutlinesForChangedObjects(lastExecutionTime, typeof(ItemEntity).Name, ids => UpdateProductOutlines(ids));
			}

			SetLastExecutionDate(lastExecutionTime, newExecutionDate);

			return Task.CompletedTask;
		}

		private void UpdateOutlinesForChangedObjects(DateTime? lastExecutionTime, string changesTypeName, Action<string[]> updateOutlinesAction)
		{
			var changeLogOperations = _changeLogService.FindChangeHistory(changesTypeName, lastExecutionTime, null);

			var changedIds = Array.Empty<string>();

			if (!changeLogOperations.IsNullOrEmpty())
			{
				changedIds = changeLogOperations.Where(x => x.OperationType != EntryState.Deleted).Select(x => x.ObjectId).Distinct().ToArray();
			}

			if (!changedIds.IsNullOrEmpty())
			{
				for (int i = 0; i < changedIds.Length; i += BatchCount)
				{
					updateOutlinesAction(changedIds.Skip(i).Take(BatchCount).ToArray());
				}
			}
		}

		private DateTime? GetLastExecutionDate()
		{
			return _settingsManager.GetValue(LastIndexationDateSettingName, (DateTime?)null);
		}

		private void SetLastExecutionDate(DateTime? oldValue, DateTime newValue)
		{
			var currentValue = GetLastExecutionDate();
			if (currentValue == oldValue)
			{
				_settingsManager.SetValue(LastIndexationDateSettingName, newValue);
			}
		}

		private void UpdateCategoryOutlines(string[] entityIds = null)
		{
			UpdateOutlines("Category", ids => _categoryService.GetByIds(ids, CategoryResponseGroup.WithOutlines), entityIds);
		}

		private void UpdateProductOutlines(string[] entityIds = null)
		{
			UpdateOutlines("Product", ids => _itemService.GetByIds(ids, ItemResponseGroup.Outlines), entityIds);
		}

		private void UpdateOutlines(string entityType, Func<string[], IHasOutlines[]> outlineGetter, string[] entityIds = null)
		{
			var searchCriteria = new TaggedItemSearchCriteria()
			{
				ResponseGroup = TaggedItemResponseGroup.WithOutlines.ToString(),
				EntityType = entityType,
				EntityIds = entityIds,
				Take = 0,
				Skip = 0,
			};

			var categoriesCount = _taggedItemSearchService.SearchTaggedItems(searchCriteria).TotalCount;

			for (int i = 0; i < categoriesCount; i += BatchCount)
			{
				searchCriteria.Skip = i;
				searchCriteria.Take = BatchCount;

				var taggedItems = _taggedItemSearchService.SearchTaggedItems(searchCriteria).Results;
				var objectIds = taggedItems.Select(x => x.EntityId).ToArray();
				var objectsWithOutlines = Array.Empty<IHasOutlines>();

				if (!objectIds.IsNullOrEmpty())
				{
					objectsWithOutlines = outlineGetter(objectIds);
				}

				if (!objectsWithOutlines.IsNullOrEmpty())
				{
					var taggedItemOutlinesMap = taggedItems.ToDictionary(x => x.Id, x => objectsWithOutlines.FirstOrDefault(obj => ((IEntity)obj).Id == x.EntityId)?.Outlines);
					var outlines = taggedItemOutlinesMap
						.Where(x => !x.Value.IsNullOrEmpty())
						.SelectMany(x =>
							x.Value.Select(outline =>
							{
								var instance = AbstractTypeFactory<TaggedItemOutline>.TryCreateInstance();

								instance.Outline = string.Join("/", outline.Items.Take(outline.Items.Count - 1).Select(item => item.Id).ToArray());
								instance.TaggedItemId = x.Key;

								return instance;
							}))
						.ToArray();

					_taggedItemOutlineService.SaveTaggedItemOutlines(outlines);
				}
			}
		}
	}
}