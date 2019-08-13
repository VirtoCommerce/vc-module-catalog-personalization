using System.Linq;
using System.Net;
using System.Web.Http;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [RoutePrefix("api/personalization")]
    public class PersonalizationModuleController : ApiController
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedItemSearchService _searchService;
        private readonly ITaggedItemOutlinesSynchronizator _taggedItemOutlineSync;
        public PersonalizationModuleController(ITaggedItemService taggedItemService, ITaggedItemSearchService searchService, ITaggedItemOutlinesSynchronizator taggedItemOutlineSync)
        {
            _taggedItemService = taggedItemService;
            _searchService = searchService;
            _taggedItemOutlineSync = taggedItemOutlineSync;
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}")]
        public IHttpActionResult GetTaggedItem(string id)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                EntityId = id,
                Take = 1
            };
            var taggedItem = _searchService.SearchTaggedItems(criteria).Results.FirstOrDefault();
            return Ok(new { taggedItem });
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}/tags/count
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}/tags/count")]
        public IHttpActionResult GetTagsCount(string id)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                EntityId = id,
                Take = 1
            };
            var result = _searchService.SearchTaggedItems(criteria).Results.FirstOrDefault();
            var count = result?.Tags.Count ?? 0;

            return Ok(new { count });
        }

        /// <summary>
        /// PUT: api/personalization/taggeditem
        /// </summary>
        [HttpPut]
        [Route("taggeditem")]
        public IHttpActionResult UpdateTaggedItem(TaggedItem taggedItem)
        {
            _taggedItemService.SaveTaggedItems(new[] { taggedItem });
            _taggedItemOutlineSync.SynchronizeOutlines(new[] { taggedItem });
            return StatusCode(HttpStatusCode.NoContent);
        }
    
    }
}
