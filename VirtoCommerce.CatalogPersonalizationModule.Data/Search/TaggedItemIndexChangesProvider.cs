using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class TaggedItemIndexChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(TaggedItemEntity);

        private readonly ITaggedItemSearchService _taggedItemSearchService;
        private readonly IChangeLogService _changeLogService;

        public TaggedItemIndexChangesProvider(ITaggedItemSearchService taggedItemSearchService, IChangeLogService changeLogService)
        {
            _taggedItemSearchService = taggedItemSearchService;
            _changeLogService = changeLogService;
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

        protected virtual IDictionary<string, string> GetObjectIds(string[] taggedItemIds)
        {
            var searchCriteria = new TaggedItemSearchCriteria
            {
                Ids = taggedItemIds,
                Take = int.MaxValue
            };
            var searchResult = _taggedItemSearchService.SearchTaggedItems(searchCriteria);
            var result = searchResult.Results.ToDictionary(e => e.Id, e => e.EntityId);
            return result;
        }
    }
}
