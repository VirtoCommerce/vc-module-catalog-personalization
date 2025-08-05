using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Events;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Handlers;

public class LogChangesChangedEventHandler : IEventHandler<TaggedItemChangedEvent>
{
    private readonly IChangeLogService _changeLogService;
    private readonly ILastModifiedDateTime _lastModifiedDateTime;
    private readonly ISettingsManager _settingsManager;

    public LogChangesChangedEventHandler(IChangeLogService changeLogService, ILastModifiedDateTime lastModifiedDateTime, ISettingsManager settingsManager)
    {
        _changeLogService = changeLogService;
        _lastModifiedDateTime = lastModifiedDateTime;
        _settingsManager = settingsManager;
    }

    public virtual async Task Handle(TaggedItemChangedEvent message)
    {
        await InnerHandle(message);
    }

    protected virtual async Task InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
    {
        var changesEnabled = await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.LogTaggedItemsChanges);

        if (changesEnabled)
        {
            var logOperations = @event.ChangedEntries.Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x)).ToArray();
            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));
        }
        else
        {
            _lastModifiedDateTime.Reset();
        }
    }

    public void LogEntityChangesInBackground(OperationLog[] operationLogs)
    {
        _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
    }
}
