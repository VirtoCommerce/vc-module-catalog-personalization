using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
	public class TaggedItemCategoryDocumentBuilder : IIndexDocumentBuilder
	{
		private readonly ICategoryService _categoryService;
		private readonly ITaggedItemService _taggedItemService;
		private readonly ITaggedItemOutlineService _taggedItemOutlineService;

		public TaggedItemCategoryDocumentBuilder(ICategoryService categoryService, ITaggedItemService taggedItemService, ITaggedItemOutlineService taggedItemOutlineService)
		{
			_categoryService = categoryService;
			_taggedItemService = taggedItemService;
			_taggedItemOutlineService = taggedItemOutlineService;
		}

		public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
		{
			var categories = GetCategories(documentIds);
			var outlineIds = categories.GetObjectsOutlineIds();
			var taggedItems = _taggedItemService.GetTaggedItemsByObjectIds(outlineIds);

			// Getting self and parent objects tags (tags inheritence)
			var selfAndParentTagsMap = categories.CombineObjectsWithTags(taggedItems);

			// Getting children categories/products tags - we want to find categories by tag in case any of its children have it
			var childrenTagsMap = documentIds.ToDictionary(x => x, x => _taggedItemOutlineService.GetTagsByOutlinePart(x));

			// Joining this sequences for each category, removing dulicates
			var combinedData = selfAndParentTagsMap.Join(
				childrenTagsMap,
				x => x.Key,
				x => x.Key,
				(parentTags, childrenTags) => new KeyValuePair<string, string[]>(parentTags.Key, parentTags.Value.Union(childrenTags.Value, StringComparer.OrdinalIgnoreCase).ToArray())
			).ToDictionary(x => x.Key, x => x.Value);

			IList<IndexDocument> result = categories
				.Select(c => CreateDocument(c, combinedData[c.Id]))
				.ToArray();

			return Task.FromResult(result);
		}

		protected virtual IList<Category> GetCategories(IList<string> categoryIds)
		{
			return _categoryService.GetByIds(categoryIds.ToArray(), CategoryResponseGroup.WithProperties | CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithImages | CategoryResponseGroup.WithSeo | CategoryResponseGroup.WithLinks);
		}

		protected virtual IndexDocument CreateDocument(Category category, string[] tags)
		{
			var document = new IndexDocument(category.Id);

			if (tags.IsNullOrEmpty())
			{
				tags = new[] { Constants.UserGroupsAnyValue };
			}

			foreach (var tag in tags)
			{
				document.Add(new IndexDocumentField(Constants.UserGroupsFieldName, tag) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
			}

			return document;
		}
	}
}
