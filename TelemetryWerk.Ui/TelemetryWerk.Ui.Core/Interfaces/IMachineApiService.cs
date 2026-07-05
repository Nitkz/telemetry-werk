using TelemetryWerk.Ui.Core.Entities;

namespace TelemetryWerk.Ui.Core.Interfaces;

public interface IMachineApiService
{
    Task<IEnumerable<UiMachineNode>> GetNodesAsync(int limit = 100);
}
