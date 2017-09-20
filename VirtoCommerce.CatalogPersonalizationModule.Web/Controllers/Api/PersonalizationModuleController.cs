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

        // GET: api/personalization/taggeditem/{id}
        [HttpGet]
        [Route("taggeditem/{id}")]
        public IHttpActionResult GetTaggedItem(string id)
        {
            var taggedItem = _taggedItemService.GetTaggedItemsByIds(new[] { id }).FirstOrDefault();
            return Ok(new { taggedItem });
        }

        // GET: api/personalization/taggeditem/{id}/tags/count
        [HttpGet]
        [Route("taggeditem/{id}/tags/count")]
        public IHttpActionResult GetTagsCount(string id)
        {
            var taggedItem = _taggedItemService.GetTaggedItemsByIds(new[] { id }).FirstOrDefault();
            var count = taggedItem?.Tags.Count() ?? 0;

            return Ok(new { count });
        }

        // PUT: api/personalization/taggeditem
        [HttpPut]
        [Route("taggeditem")]
        public IHttpActionResult UpdateTaggedItem(TaggedItem taggedItem)
        {
            _taggedItemService.SaveTaggedItems(new[] { taggedItem });
            return StatusCode(HttpStatusCode.NoContent);
        }


    }
}
