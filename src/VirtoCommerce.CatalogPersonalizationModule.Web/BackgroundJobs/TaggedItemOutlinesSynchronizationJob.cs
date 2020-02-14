using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs
{
    public class TaggedItemOutlinesSynchronizationJob
    {
        public const string JobId = "TagOutlinesSynchronization";

        private const int BatchCount = 50;

        private readonly ITaggedItemOutlinesSynchronizer _taggedOutlineSync;
        private readonly ITaggedItemSearchService _taggedItemSearchService;
        private readonly IPushNotificationManager _pushNotificationManager;

        public TaggedItemOutlinesSynchronizationJob(ITaggedItemSearchService taggedItemSearchService, ITaggedItemOutlinesSynchronizer taggedOutlineSync, IPushNotificationManager pushNotificationManager)
        {
            _taggedItemSearchService = taggedItemSearchService;
            _taggedOutlineSync = taggedOutlineSync;
            _pushNotificationManager = pushNotificationManager;
        }

        public async Task Run()
        {
            void progressCallback(TaggedItemOutlineSyncProgressInfo x)
            {
            }

            await PerformSynchronization(progressCallback, null);
        }


        public async Task Run(TaggedItemOutlineSyncPushNotification notification, IJobCancellationToken cancellationToken, PerformContext context)
        {
            async void progressCallback(TaggedItemOutlineSyncProgressInfo x)
            {
                notification.Description = x.Description;
                notification.Errors = x.Errors;
                notification.ProcessedCount = x.ProcessedCount;
                notification.TotalCount = x.TotalCount;
                notification.JobId = context.BackgroundJob.Id;

                await _pushNotificationManager.SendAsync(notification);
            }

            try
            {
                await PerformSynchronization(progressCallback, cancellationToken);
            }
            catch (JobAbortedException)
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

        public async Task PerformSynchronization(Action<TaggedItemOutlineSyncProgressInfo> progressCallback, IJobCancellationToken cancellationToken)
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

            cancellationToken?.ThrowIfCancellationRequested();

            for (var i = 0; i < result.TotalCount; i += BatchCount)
            {
                cancellationToken?.ThrowIfCancellationRequested();

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
