using TelemetryWerk.Ui.Core.Entities;

namespace TelemetryWerk.Ui.Core.Interfaces;

public interface IMachineApiService
{
    Task<IEnumerable<UiMachineNode>> GetNodesAsync(int limit = 100);
    Task<UiMachineNode?> AddNodeAsync(UiMachineNode node);
    Task<UiMachineNode?> UpdateNodeAsync(string id, UiMachineNode node);
    Task DeleteNodeAsync(string id);
}
