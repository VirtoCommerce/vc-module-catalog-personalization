using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using static VirtoCommerce.CatalogPersonalizationModule.Core.ModuleConstants.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Handlers
{
    public class ObjectSettingEntryChangedEventHandler : IEventHandler<Platform.Core.Settings.Events.ObjectSettingChangedEvent>
    {
        private readonly ModuleConfigurator _moduleConfigurator;


        public ObjectSettingEntryChangedEventHandler(ModuleConfigurator moduleConfigurator)
        {
            _moduleConfigurator = moduleConfigurator;

        }

        public virtual async Task Handle(Platform.Core.Settings.Events.ObjectSettingChangedEvent message)
        {
            var tagsInheritancePolicyChanged = message.ChangedEntries.Any(x => (x.EntryState == EntryState.Modified
                                                                               || x.EntryState == EntryState.Added)
                                                            && (x.NewEntry.Name == General.TagsInheritancePolicy.Name));

            var cronExpressionChanged = message.ChangedEntries.Any(x => (x.EntryState == EntryState.Modified
                                                                       || x.EntryState == EntryState.Added)
                                                            && (x.NewEntry.Name == General.CronExpression.Name));

            if (tagsInheritancePolicyChanged || cronExpressionChanged)
            {
                await _moduleConfigurator.ConfigureOutlinesSynchronizationJob();
            }

            if (tagsInheritancePolicyChanged)
            {
                _moduleConfigurator.ConfigureSearch();
            }
        }
    }
}
