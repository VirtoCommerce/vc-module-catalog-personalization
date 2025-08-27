using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
    public class TaggedItemIndexChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(TaggedItem);

        private readonly ITaggedItemSearchService _taggedItemSearchService;
        private readonly IChangeLogSearchService _changeLogSearchService;

        public TaggedItemIndexChangesProvider(ITaggedItemSearchService taggedItemSearchService, IChangeLogSearchService changeLogSearchService)
        {
            _taggedItemSearchService = taggedItemSearchService;
            _changeLogSearchService = changeLogSearchService;
        }

        public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
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
                var searchResult = await _changeLogSearchService.SearchAsync(new ChangeLogSearchCriteria
                {
                    ObjectType = ChangeLogObjectType,
                    StartDate = startDate,
                    EndDate = endDate
                });

                result = searchResult.TotalCount;
            }

            return result;
        }

        public async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                var searchResult = await _changeLogSearchService.SearchAsync(new ChangeLogSearchCriteria
                {
                    ObjectType = ChangeLogObjectType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Skip = (int)skip,
                    Take = (int)take
                });

                var taggedItemIds = searchResult.Results.Select(o => o.ObjectId).ToArray();
                var objectIds = await GetObjectIdsAsync(taggedItemIds);

                result = searchResult.Results
                    .Where(o => objectIds.ContainsKey(o.ObjectId))
                    .Select(o => new IndexDocumentChange
                    {
                        DocumentId = objectIds[o.ObjectId],
                        ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                        ChangeType = IndexDocumentChangeType.Modified,
                    })
                    .ToArray();
            }

            return result;
        }

        protected virtual async Task<IDictionary<string, string>> GetObjectIdsAsync(string[] taggedItemIds)
        {
            var searchCriteria = new TaggedItemSearchCriteria
            {
                Ids = taggedItemIds,
                Take = int.MaxValue
            };
            var searchResult = await _taggedItemSearchService.SearchTaggedItemsAsync(searchCriteria);
            var result = searchResult.Results.ToDictionary(e => e.Id, e => e.EntityId);
            return result;
        }
    }
}
