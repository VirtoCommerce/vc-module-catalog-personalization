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
    public class TaggedItemProductDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IItemService _itemService;
        private readonly ITaggedItemService _taggedItemService;

        public TaggedItemProductDocumentBuilder(IItemService itemService, ITaggedItemService taggedItemService)
        {
            _itemService = itemService;
            _taggedItemService = taggedItemService;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var products = GetProducts(documentIds);
            var outlineIds = products.GetObjectsOutlineIds();
            var taggedItems = _taggedItemService.GetTaggedItemsByObjectIds(outlineIds);

            IDictionary<string, string[]> combinedData = products.CombineObjectsWithTags(taggedItems);

            IList<IndexDocument> result = products
                .Select(c => CreateDocument(c, combinedData[c.Id]))
                .ToArray();

            return Task.FromResult(result);
        }

        protected virtual IList<CatalogProduct> GetProducts(IList<string> productIds)
        {
            return _itemService.GetByIds(productIds.ToArray(), ItemResponseGroup.ItemLarge);
        }

        protected virtual IndexDocument CreateDocument(CatalogProduct product, string[] tags)
        {
            var document = new IndexDocument(product.Id);
            
            if (tags.IsNullOrEmpty())
            {
                tags = new[] { "__any" };
            }

            foreach (var tag in tags)
            {
                document.Add(new IndexDocumentField("user_groups", tag) { IsRetrievable = true, IsFilterable = true });
            }

            return document;
        }
    }
}
