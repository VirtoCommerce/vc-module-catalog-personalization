using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
    public class TaggedItemCategoryDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly ICategoryService _categoryService;
        private readonly ITaggedItemService _taggedItemService;

        public TaggedItemCategoryDocumentBuilder(ICategoryService categoryService, ITaggedItemService taggedItemService)
        {
            _categoryService = categoryService;
            _taggedItemService = taggedItemService;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var categories = GetCategories(documentIds);
            var outlineIds = categories.GetObjectsOutlineIds();
            var taggedItems = _taggedItemService.GetTaggedItemsByObjectIds(outlineIds);

            IDictionary<string, string[]> combinedData = categories.CombineObjectsWithTags(taggedItems);

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

            foreach (var tag in tags)
            {
                document.Add(new IndexDocumentField("user_groups", tag) { IsRetrievable = true, IsFilterable = true });
            }

            return document;
        }
    }
}
