using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs;
using VirtoCommerce.CatalogPersonalizationModule.Web.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [Route("api/personalization")]
    public class PersonalizationModuleController : Controller
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedItemSearchService _searchService;
        private readonly ITaggedItemOutlinesSynchronizer _taggedItemOutlineSync;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ISettingsManager _settingsManager;

        private const int _duplicateKey = 2601;
        private const int _duplicatePrimaryKey = 2627;

        public PersonalizationModuleController(ITaggedItemService taggedItemService,
            ITaggedItemSearchService searchService,
            ITaggedItemOutlinesSynchronizer taggedItemOutlineSync,
            IUserNameResolver userNameResolver,
            IPushNotificationManager pushNotificationManager,
            ISettingsManager settingsManager)
        {
            _taggedItemService = taggedItemService;
            _searchService = searchService;
            _taggedItemOutlineSync = taggedItemOutlineSync;
            _userNameResolver = userNameResolver;
            _pushNotificationManager = pushNotificationManager;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}")]
        [ProducesResponseType(typeof(TaggedItem), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaggedItem>> GetTaggedItem(string id)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                EntityId = id,
                Take = 1,
                ResponseGroup = TaggedItemResponseGroup.WithInheritedTags.ToString()
            };
            var taggedItem = (await _searchService.SearchTaggedItemsAsync(criteria)).Results.FirstOrDefault();

            return Ok(taggedItem);
        }

        /// <summary>
        /// GET: api/personalization/taggeditem/{id}/tags/count
        /// </summary>
        [HttpGet]
        [Route("taggeditem/{id}/tags/count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetTagsCount(string id)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                EntityId = id,
                Take = 1,
                ResponseGroup = TaggedItemResponseGroup.WithInheritedTags.ToString()
            };

            var result = (await _searchService.SearchTaggedItemsAsync(criteria)).Results.FirstOrDefault();
            var count = result?.Tags.Union(result.InheritedTags).Distinct().Count() ?? 0;

            return Ok(count);
        }

        /// <summary>
        /// PUT: api/personalization/taggeditem
        /// </summary>
        [HttpPut]
        [Route("taggeditem")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateTaggedItem([FromBody] TaggedItem taggedItem)
        {

            try
            {
                await _taggedItemService.SaveChangesAsync(new[] { taggedItem });
            }
            // VP-4690: Handling concurrent update exception - e.g. adding 2 tagged for 1 entity
            catch (DbUpdateException e) when (e.InnerException is Microsoft.Data.SqlClient.SqlException sqlException && (sqlException.Number == _duplicateKey || sqlException.Number == _duplicatePrimaryKey))
            {
                throw new InvalidOperationException($"Tagged item for the entity (id:\"{taggedItem.EntityId}\" type:\"{taggedItem.EntityType}\") already exists." +
                    $" Please refresh the entity and execute the operation again.");
            }

            await _taggedItemOutlineSync.SynchronizeOutlinesAsync(new[] { taggedItem });
            return NoContent();
        }

        /// <summary>
        /// POST: api/personalization/search
        /// </summary>
        [HttpPost]
        [Route("search")]
        [ProducesResponseType(typeof(TaggedItemSearchResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaggedItemSearchResult>> Search([FromBody] TaggedItemSearchCriteria criteria)
        {
            var searchResult = await _searchService.SearchTaggedItemsAsync(criteria);
            return Ok(searchResult);
        }

        /// <summary>
        /// PUT: api/personalization/outlines/synchronize
        /// </summary>
        [HttpPost]
        [Route("outlines/synchronize")]
        [ProducesResponseType(typeof(TaggedItemOutlineSyncPushNotification), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaggedItemOutlineSyncPushNotification>> RunOutlinesSynchronization()
        {
            var tagsInheritancePolicy = await _settingsManager.GetValueAsync<string>(ModuleConstants.Settings.General.TagsInheritancePolicy);
            var notification = new TaggedItemOutlineSyncPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Synchronizing tagged items outlines",
                Description = "Starting…"
            };

            if (tagsInheritancePolicy.EqualsInvariant("UpTree"))
            {
                var jobId = BackgroundJob.Enqueue<TaggedItemOutlinesSynchronizationJob>(x => x.Run(notification, JobCancellationToken.Null, null));
                notification.JobId = jobId;
            }
            else
            {
                notification.Description = "Unable to perform synchronization";
                notification.Errors.Add("To perform tagged items outlines synchronization TagsInheritancePolicy should be \"UpTree\"");
                notification.Finished = DateTime.UtcNow;
            }

            await _pushNotificationManager.SendAsync(notification);

            return Ok(notification);
        }

        /// <summary>
        /// PUT: api/personalization/outlines/synchronization/cancel
        /// </summary>
        [HttpPost]
        [Route("outlines/synchronization/cancel")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public ActionResult CancelSynchronization([FromBody] TaggedItemOutlinesSynchronizationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return NoContent();
        }
    }
}
