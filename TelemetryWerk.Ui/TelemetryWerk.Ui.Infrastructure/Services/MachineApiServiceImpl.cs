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

    public async Task<UiMachineNode?> AddNodeAsync(UiMachineNode node)
    {
        var dto = new MachineNodeDto
        {
            Id = node.Id,
            CoreTemperature = node.CoreTemperature,
            Status = node.Status
        };

        var response = await apiClient.NodesPOSTAsync(dto);

        if (response?.Data == null)
            return null;

        return new UiMachineNode
        {
            Id = response.Data.Id ?? string.Empty,
            CoreTemperature = response.Data.CoreTemperature,
            Status = response.Data.Status ?? "Unknown"
        };
    }

    public async Task<UiMachineNode?> UpdateNodeAsync(string id, UiMachineNode node)
    {
        var dto = new UpdateNodeRequestDto
        {
            CoreTemperature = node.CoreTemperature,
            Status = node.Status
        };

        var response = await apiClient.NodesPATCHAsync(id, dto);

        if (response?.Data == null)
            return null;

        return new UiMachineNode
        {
            Id = response.Data.Id ?? string.Empty,
            CoreTemperature = response.Data.CoreTemperature,
            Status = response.Data.Status ?? "Unknown"
        };
    }

    public async Task DeleteNodeAsync(string id)
    {
        await apiClient.NodesDELETEAsync(id);
    }
}
