using TelemetryWerk.Api.Client;
using TelemetryWerk.Ui.Core.Entities;
using TelemetryWerk.Ui.Core.Interfaces;

namespace TelemetryWerk.Ui.Infrastructure.Services;

public class MachineApiServiceImpl(ITelemetryApiClient apiClient) : IMachineApiService
{
    public async Task<IEnumerable<UiMachineNode>> GetNodesAsync(int limit = 100)
    {
        var response = await apiClient.NodesGETAsync(null, limit);

        if (response?.Data?.Items == null)
        {
            return Enumerable.Empty<UiMachineNode>();
        }

        // Anti-Corruption Layer: Map Generated Swagger DTO to UI Domain Model
        return response.Data.Items.Select(dto => new UiMachineNode
        {
            Id = dto.Id ?? string.Empty,
            CoreTemperature = dto.CoreTemperature,
            Status = dto.Status ?? "Unknown"
        });
    }
}
