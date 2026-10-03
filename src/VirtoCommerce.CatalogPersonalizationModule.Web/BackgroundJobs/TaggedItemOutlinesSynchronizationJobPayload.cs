using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs
{
    /// <summary>
    /// Payload of the tagged item outlines synchronization job.
    /// </summary>
    public class TaggedItemOutlinesSynchronizationJobPayload
    {
        /// <summary>
        /// Push notification to report progress through, for a manual run started from the admin UI.
        /// <c>null</c> for a scheduled run.
        /// </summary>
        public TaggedItemOutlineSyncPushNotification Notification { get; set; }
    }
}
