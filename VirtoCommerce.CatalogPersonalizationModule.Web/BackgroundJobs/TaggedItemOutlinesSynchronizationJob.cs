using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs
{
    public class TaggedItemOutlinesSynchronizationJob
	{
		private const int BatchCount = 50;
		private readonly ITaggedItemOutlinesSynchronizator _taggedOutlineSync;
        private readonly ITaggedItemSearchService _taggedItemSearchService;
        public const string JobId = "TagOutlinesSynchronization";

        public TaggedItemOutlinesSynchronizationJob(ITaggedItemSearchService taggedItemSearchService, ITaggedItemOutlinesSynchronizator taggedOutlineSync)
		{
            _taggedItemSearchService = taggedItemSearchService;
            _taggedOutlineSync = taggedOutlineSync;		
		}

		public Task Run()
		{
            var criteria = new TaggedItemSearchCriteria
            {
                Skip = 0,
                Take = 0
            };
            var result = _taggedItemSearchService.SearchTaggedItems(criteria);
            for (var i = 0; i < result.TotalCount; i += BatchCount)
            {
                criteria.Skip = i;
                criteria.Take = BatchCount;
                var searchResponse = _taggedItemSearchService.SearchTaggedItems(criteria);
                if(searchResponse.Results.Any())
                {
                    _taggedOutlineSync.SynchronizeOutlines(searchResponse.Results.ToArray());
                }            
            }
            return Task.CompletedTask;
		}

	
		
	}
}