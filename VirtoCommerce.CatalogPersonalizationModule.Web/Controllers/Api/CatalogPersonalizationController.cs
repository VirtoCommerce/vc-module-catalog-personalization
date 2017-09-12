using System.Web.Http;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [RoutePrefix("api/personalization")]
    public class CatalogPersonalizationController : ApiController
    {
        // GET: api/managedModule
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
