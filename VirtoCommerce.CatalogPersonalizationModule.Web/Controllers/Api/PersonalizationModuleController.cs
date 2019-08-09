using System.Linq;
using System.Net;
using System.Web.Http;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
	[RoutePrefix("api/personalization")]
	public class PersonalizationModuleController : ApiController
	{
		private readonly ITaggedItemService _taggedItemService;
		private readonly IChangeLogService _changeLogService;


		public PersonalizationModuleController(ITaggedItemService taggedItemService, IChangeLogService changeLogService)
		{
			_taggedItemService = taggedItemService;
			_changeLogService = changeLogService;
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

			// This is required as there is no changes logged when we adding tags
			_changeLogService.SaveChanges(new OperationLog()
			{
				ObjectId = taggedItem.EntityId,
				ObjectType = TaggedEntityTypeToObjectType(taggedItem.EntityType),
				OperationType = Platform.Core.Common.EntryState.Modified,
				Detail = $"Tags changed"
			});

			return StatusCode(HttpStatusCode.NoContent);
		}


		private string TaggedEntityTypeToObjectType(string entityType)
		{
			var result = entityType;

			switch (entityType)
			{
				case "Category":
					result = nameof(CategoryEntity);
					break;
				case "Product":
					result = nameof(ItemEntity);
					break;
			}

			return result;
		}
	}
}
