using System.Linq;
using System.Net;
using System.Web.Http;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [RoutePrefix("api/personalization")]
    public class PersonalizationModuleController : ApiController
    {
        private readonly ITaggedItemService _taggedItemService;

        public PersonalizationModuleController(ITaggedItemService taggedItemService)
        {
            _taggedItemService = taggedItemService;
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}")]
        public IHttpActionResult GetTaggedItem(string id)
        {
            var taggedItem = _taggedItemService.GetTaggedItemsByObjectIds(new[] { id }).FirstOrDefault();
            return Ok(new { taggedItem });
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}/tags/count
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}/tags/count")]
        public IHttpActionResult GetTagsCount(string id)
        {
            var taggedItem = _taggedItemService.GetTaggedItemsByObjectIds(new[] { id }).FirstOrDefault();
            var count = taggedItem?.Tags.Count ?? 0;

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
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
