using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.BackgroundJobs
{
    public class TaggedItemOutlinesSynchronizationJobTests
    {
        private readonly Mock<ITaggedItemSearchService> _searchService = new();
        private readonly Mock<ISettingsManager> _settingsManager = new();
        private readonly Mock<IJobExecutionContext> _context = new();
        private readonly TaggedItemOutlinesSynchronizationJob _job;

        public TaggedItemOutlinesSynchronizationJobTests()
        {
            _searchService
                .Setup(x => x.SearchTaggedItemsAsync(It.IsAny<TaggedItemSearchCriteria>()))
                .ReturnsAsync(new TaggedItemSearchResult { TotalCount = 0, Results = new List<TaggedItem>() });
            _context.SetupGet(x => x.JobId).Returns("job-1");

            _job = new TaggedItemOutlinesSynchronizationJob(
                _searchService.Object,
                Mock.Of<ITaggedItemOutlinesSynchronizer>(),
                Mock.Of<IPushNotificationManager>(),
                _settingsManager.Object);
        }

        [Theory]
        [InlineData("UpTree", 1)]
        [InlineData("uptree", 1)]
        [InlineData("DownTree", 0)]
        public async Task Execute_ScheduledRun_SynchronizesOnlyUnderUpTreePolicy(string policy, int expectedSearches)
        {
            SetPolicy(policy);

            await _job.Execute(new TaggedItemOutlinesSynchronizationJobPayload(), _context.Object, TestContext.Current.CancellationToken);

            _searchService.Verify(x => x.SearchTaggedItemsAsync(It.IsAny<TaggedItemSearchCriteria>()), Times.Exactly(expectedSearches));
        }

        [Fact]
        public async Task Execute_ManualRun_SynchronizesAndReportsJobId()
        {
            // The controller only enqueues a manual run under UpTree, so the job does not re-check the policy.
            SetPolicy("DownTree");
            var notification = new TaggedItemOutlineSyncPushNotification("admin");

            await _job.Execute(new TaggedItemOutlinesSynchronizationJobPayload { Notification = notification }, _context.Object, TestContext.Current.CancellationToken);

            _searchService.Verify(x => x.SearchTaggedItemsAsync(It.IsAny<TaggedItemSearchCriteria>()), Times.Once);
            Assert.Equal("job-1", notification.JobId);
            Assert.NotNull(notification.Finished);
        }

        private void SetPolicy(string policy)
        {
            _settingsManager
                .Setup(x => x.GetObjectSettingAsync(ModuleConstants.Settings.General.TagsInheritancePolicy.Name, null, null))
                .ReturnsAsync(new ObjectSettingEntry { Value = policy });
        }
    }
}
