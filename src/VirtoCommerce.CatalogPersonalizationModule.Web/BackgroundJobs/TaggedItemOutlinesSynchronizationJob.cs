using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs
{
    public class TaggedItemOutlinesSynchronizationJob : IBackgroundJobHandler<TaggedItemOutlinesSynchronizationJobPayload>
    {
        public const string JobId = "TagOutlinesSynchronization";

        private const int BatchCount = 50;

        private readonly ITaggedItemOutlinesSynchronizer _taggedOutlineSync;
        private readonly ITaggedItemSearchService _taggedItemSearchService;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ISettingsManager _settingsManager;

        public TaggedItemOutlinesSynchronizationJob(ITaggedItemSearchService taggedItemSearchService, ITaggedItemOutlinesSynchronizer taggedOutlineSync, IPushNotificationManager pushNotificationManager, ISettingsManager settingsManager)
        {
            _taggedItemSearchService = taggedItemSearchService;
            _taggedOutlineSync = taggedOutlineSync;
            _pushNotificationManager = pushNotificationManager;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Runs a synchronization. A payload with a <see cref="TaggedItemOutlinesSynchronizationJobPayload.Notification"/>
        /// is a manual run started from the admin UI and reports progress through it; a payload without one is a
        /// scheduled run and reports nothing.
        /// </summary>
        public virtual async Task Execute(TaggedItemOutlinesSynchronizationJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            if (payload?.Notification is not null)
            {
                await Run(payload.Notification, context.JobId, cancellationToken);
                return;
            }

            // The schedule's enabler is a boolean setting (the engine supports nothing else), so the UpTree rule that
            // used to gate the Hangfire schedule is checked here: outlines only need syncing under that policy.
            var policy = await _settingsManager.GetValueAsync<string>(ModuleConstants.Settings.General.TagsInheritancePolicy);
            if (policy.EqualsIgnoreCase("UpTree"))
            {
                await PerformSynchronization(_ => { }, cancellationToken);
            }
        }

        public async Task Run()
        {
            void progressCallback(TaggedItemOutlineSyncProgressInfo x)
            {
            }

            await PerformSynchronization(progressCallback, CancellationToken.None);
        }

        protected virtual async Task Run(TaggedItemOutlineSyncPushNotification notification, string jobId, CancellationToken cancellationToken)
        {
            async void progressCallback(TaggedItemOutlineSyncProgressInfo x)
            {
                notification.Description = x.Description;
                notification.Errors = x.Errors;
                notification.ProcessedCount = x.ProcessedCount;
                notification.TotalCount = x.TotalCount;
                notification.JobId = jobId;

                await _pushNotificationManager.SendAsync(notification);
            }

            try
            {
                await PerformSynchronization(progressCallback, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                notification.Errors.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                notification.Description = "Synchronization finished";
                notification.Finished = DateTime.UtcNow;
                _pushNotificationManager.Send(notification);
            }
        }

        public async Task PerformSynchronization(Action<TaggedItemOutlineSyncProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var criteria = new TaggedItemSearchCriteria
            {
                Skip = 0,
                Take = 0
            };
            var result = await _taggedItemSearchService.SearchTaggedItemsAsync(criteria);
            var totalCount = result.TotalCount;

            var progressInfo = new TaggedItemOutlineSyncProgressInfo()
            {
                Description = "Reading orders...",
                TotalCount = totalCount,
                ProcessedCount = 0
            };

            progressCallback(progressInfo);

            cancellationToken.ThrowIfCancellationRequested();

            for (var i = 0; i < result.TotalCount; i += BatchCount)
            {
                cancellationToken.ThrowIfCancellationRequested();

                criteria.Skip = i;
                criteria.Take = BatchCount;

                var searchResponse = await _taggedItemSearchService.SearchTaggedItemsAsync(criteria);

                if (searchResponse.Results.Any())
                {
                    await _taggedOutlineSync.SynchronizeOutlinesAsync(searchResponse.Results.ToArray());
                }

                var processedCount = Math.Min(i + searchResponse.Results.Count, totalCount);

                progressInfo.ProcessedCount = processedCount;
                progressInfo.Description = $"Processed {processedCount} of {totalCount} tagged items";
                progressCallback(progressInfo);
            }

            progressInfo.Description = "Tagged items outlines synchronization completed.";
            progressCallback(progressInfo);
        }
    }
}
