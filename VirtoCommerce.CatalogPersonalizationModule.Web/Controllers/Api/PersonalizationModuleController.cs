using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs;
using VirtoCommerce.CatalogPersonalizationModule.Web.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api
{
    [RoutePrefix("api/personalization")]
    public class PersonalizationModuleController : ApiController
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedItemSearchService _searchService;
        private readonly ITaggedItemOutlinesSynchronizator _taggedItemOutlineSync;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ISettingsManager _settingsManager;


        public PersonalizationModuleController(ITaggedItemService taggedItemService,
            ITaggedItemSearchService searchService,
            ITaggedItemOutlinesSynchronizator taggedItemOutlineSync,
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
        public IHttpActionResult GetTaggedItem(string id)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                EntityId = id,
                Take = 1,
                ResponseGroup = TaggedItemResponseGroup.WithInheritedTags.ToString()
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
                Take = 1,
                ResponseGroup = TaggedItemResponseGroup.WithInheritedTags.ToString()
            };

            var result = _searchService.SearchTaggedItems(criteria).Results.FirstOrDefault();
            var count = result?.Tags.Union(result.InheritedTags).Distinct().Count();

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

        /// <summary>
        /// POST: api/personalization/search
        /// </summary>
        [HttpPost]
        [Route("search")]
        [ResponseType(typeof(GenericSearchResult<TaggedItem>))]
        public IHttpActionResult Search(TaggedItemSearchCriteria criteria)
        {
            var searchResult = _searchService.SearchTaggedItems(criteria);
            return Ok(searchResult);
        }

        /// <summary>
        /// PUT: api/personalization/outlines/synchronize
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(TaggedItemOutlineSyncPushNotification))]
        [Route("outlines/synchronize")]
        public IHttpActionResult RunOutlinesSynchronization()
        {
            var tagsInheritancePolicy = _settingsManager.GetValue("VirtoCommerce.Personalization.TagsInheritancePolicy", "DownTree");
            var notification = new TaggedItemOutlineSyncPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = $"Synchronizing tagged items outlines",
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

            _pushNotificationManager.Upsert(notification);

            return Ok(notification);
        }

        /// <summary>
        /// PUT: api/personalization/outlines/synchronization/cancel
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("outlines/synchronization/cancel")]
        public IHttpActionResult CancelSynchronization([FromBody]TaggedItemOutlinesSynchronizationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }
    }
}
