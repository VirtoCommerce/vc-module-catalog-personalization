using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Jobs;

/// <summary>
/// Payload of the background job that persists tagged item change-log entries.
/// </summary>
public class LogEntityChangesJobPayload
{
    public OperationLog[] OperationLogs { get; set; }
}
