using System.Web.Http;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [RoutePrefix("api/personalization")]
    public class PersonalizationModuleController : ApiController
    {
        // GET: api/managedModule
        //api/personalization/taggeditems/{id}/{type}
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
