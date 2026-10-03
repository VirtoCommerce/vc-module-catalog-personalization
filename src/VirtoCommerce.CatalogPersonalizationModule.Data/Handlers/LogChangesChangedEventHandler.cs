using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Events;
using VirtoCommerce.CatalogPersonalizationModule.Data.Jobs;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
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

            var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
            payload.OperationLogs = logOperations;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            await BackgroundJob.Enqueue<LogEntityChangesJobHandler>(payload);
        }
        else
        {
            _lastModifiedDateTime.Reset();
        }
    }

    /// <summary>
    /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
    /// New work goes through <see cref="LogEntityChangesJobHandler"/>; remove this once no such job
    /// can still be pending.
    /// </summary>
    // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
    // parameter types + serialized args, so changing any of them would strand already-queued entries as Failed.
    [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses LogEntityChangesJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public async Task LogEntityChangesInBackgroundAsync(OperationLog[] operationLogs)
    {
        await _changeLogService.SaveChangesAsync(operationLogs);
    }
}
