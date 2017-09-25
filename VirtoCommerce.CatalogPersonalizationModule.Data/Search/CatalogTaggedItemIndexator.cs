using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class CatalogTaggedItemIndexator: IIndexDocumentChangesProvider, IIndexDocumentBuilder
    {
        public const string ChangeLogObjectType = nameof(TaggedItemEntity);

        private readonly ITaggedItemService _taggedItemService;
        private readonly IChangeLogService _changeLogService;

        public CatalogTaggedItemIndexator(ITaggedItemService taggedItemService, IChangeLogService changeLogService)
        {
            _taggedItemService = taggedItemService;
            _changeLogService = changeLogService;
        }

        #region IIndexDocumentChangesProvider

        public Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                var operations = _changeLogService.FindChangeHistory(ChangeLogObjectType, startDate, endDate)
                    .Skip((int)skip)
                    .Take((int)take)
                    .ToArray();

                var taggedItemIds = operations.Select(o => o.ObjectId).ToArray();
                var objectIds = GetObjectIds(taggedItemIds);

                result = operations
                    .Where(o => objectIds.ContainsKey(o.ObjectId))
                    .Select(o => new IndexDocumentChange
                    {
                        DocumentId = objectIds[o.ObjectId],
                        ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                        ChangeType = IndexDocumentChangeType.Modified,
                    })
                    .ToArray();
            }

            return Task.FromResult(result);
        }

        public Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // We don't know the total products count
                result = 0L;
            }
            else
            {
                // Get changes count from operation log
                result = _changeLogService.FindChangeHistory(ChangeLogObjectType, startDate, endDate).Count();
            }

            return Task.FromResult(result);
        }

        #endregion

        #region IIndexDocumentBuilder

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var taggedItems = _taggedItemService.GetTaggedItemsByIds(documentIds.ToArray()).ToList();

            IList<IndexDocument> result = taggedItems
                .GroupBy(x => new { x.EntityId, x.EntityType})
                .Select(g => CreateDocument(g.Key.EntityId, g.Key.EntityType, g.ToArray()))
                .ToArray();

            return Task.FromResult(result);
        }

        #endregion

        protected virtual IDictionary<string, string> GetObjectIds(string[] taggedItemIds)
        {
            var taggedItems = _taggedItemService.GetTaggedItemsByIds(taggedItemIds);
            var result = taggedItems.ToDictionary(e => e.Id, e => e.EntityId);
            return result;
        }

        protected virtual IndexDocument CreateDocument(string entityId, string entityType, IEnumerable<TaggedItem> taggedItems)
        {
            var document = new IndexDocument(entityId);

            if (taggedItems != null)
            {
                foreach (var tag in taggedItems.SelectMany(t => t.Tags).Distinct())
                {
                    document.Add(new IndexDocumentField($"tag_{entityId}_{entityType}".ToLowerInvariant(), tag) { IsRetrievable = true, IsFilterable = true });
                }
            }

            return document;
        }
    }
}
