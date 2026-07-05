using System.Linq;
using System.Threading.Tasks;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Services;

public class MachineService(IMachineRepository machineRepository) : IMachineService
{
    public async Task<PagedCollection<MachineNodeDto>> GetNodesAsync(int limit, string? afterId)
    {
        // Fetch limit + 1 to determine if there are more records
        var nodes = await machineRepository.GetAllAsync(limit + 1, afterId);
        
        var hasMore = nodes.Count() > limit;
        var pagedNodes = nodes.Take(limit).Select(m => new MachineNodeDto
        {
            Id = m.Id,
            Status = m.Status,
            CoreTemperature = m.CoreTemperature
        }).ToList();

        return new PagedCollection<MachineNodeDto>
        {
            Items = pagedNodes,
            LastId = pagedNodes.LastOrDefault()?.Id ?? "",
            HasMore = hasMore
        };
    }

    public async Task<MachineNodeDto> AddNodeAsync(MachineNodeDto request)
    {
        var newNode = new MachineNode
        {
            Id = request.Id,
            Status = request.Status ?? "Stopped",
            CoreTemperature = request.CoreTemperature
        };

        await machineRepository.AddAsync(newNode);

        return new MachineNodeDto 
        { 
            Id = newNode.Id, 
            Status = newNode.Status, 
            CoreTemperature = newNode.CoreTemperature 
        };
    }

    public async Task<MachineNodeDto?> UpdateNodeAsync(string id, UpdateNodeRequestDto request)
    {
        var updatedNode = await machineRepository.UpdateAsync(id, request.Status, request.CoreTemperature);

        if (updatedNode == null) return null;

        return new MachineNodeDto 
        { 
            Id = updatedNode.Id, 
            Status = updatedNode.Status, 
            CoreTemperature = updatedNode.CoreTemperature 
        };
    }
}
