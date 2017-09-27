using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class TaggedItemIndexDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly ITaggedItemService _taggedItemService;

        public TaggedItemIndexDocumentBuilder(ITaggedItemService taggedItemService)
        {
            _taggedItemService = taggedItemService;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var taggedItems = _taggedItemService.GetTaggedItemsByObjectIds(documentIds.ToArray()).ToList();

            IList<IndexDocument> result = documentIds.Select(id => new
            {
                ObjectId = id,
                Tags = taggedItems.Where(i => i.EntityId == id).SelectMany(t => t.Tags).ToArray()
            })
                .GroupBy(x => x.ObjectId)
                .Select(g => CreateDocument(g.Key, g.SelectMany(t => t.Tags).Distinct().ToArray()))
                .ToArray();

            return Task.FromResult(result);
        }

        protected virtual IndexDocument CreateDocument(string entityId, string[] tags)
        {
            var document = new IndexDocument(entityId);

            if (tags.IsNullOrEmpty())
                tags = new[] { "__any" };

            foreach (var tag in tags.Distinct())
            {
                document.Add(new IndexDocumentField("user_groups", tag) { IsRetrievable = true, IsFilterable = true });
            }

            return document;
        }
    }
}
