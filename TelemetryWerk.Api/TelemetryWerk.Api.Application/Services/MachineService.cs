using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Exceptions;

namespace TelemetryWerk.Api.Application.Services;

public class MachineService(IMachineRepository machineRepository, ChannelWriter<MachineStateUpdateMessage> channelWriter, ILogger<MachineService> logger) : IMachineService
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
        var existing = await machineRepository.GetByIdAsync(request.Id);
        if (existing != null)
        {
            throw new ConflictException($"Machine with ID {request.Id} already exists.");
        }

        var newNode = new MachineNode
        {
            Id = request.Id,
            Status = request.Status ?? "Stopped",
            CoreTemperature = request.CoreTemperature
        };

        await machineRepository.AddAsync(newNode);

        logger.LogInformation("Node {MachineId} added", newNode.Id);

        await channelWriter.WriteAsync(new MachineStateUpdateMessage("AddOrUpdate", newNode));

        return new MachineNodeDto 
        { 
            Id = newNode.Id, 
            Status = newNode.Status, 
            CoreTemperature = newNode.CoreTemperature 
        };
    }

    public async Task<MachineNodeDto> UpdateNodeAsync(string id, UpdateNodeRequestDto request)
    {
        var updatedNode = await machineRepository.UpdateAsync(id, request.Status, request.CoreTemperature);

        if (updatedNode == null) 
        {
            logger.LogWarning("Attempted to update non-existent node {MachineId}", id);
            throw new NotFoundException($"Machine with ID {id} not found.");
        }

        logger.LogInformation("Node {MachineId} updated", updatedNode.Id);

        await channelWriter.WriteAsync(new MachineStateUpdateMessage("AddOrUpdate", updatedNode));

        return new MachineNodeDto 
        { 
            Id = updatedNode.Id, 
            Status = updatedNode.Status, 
            CoreTemperature = updatedNode.CoreTemperature 
        };
    }

    public async Task DeleteNodeAsync(string id)
    {
        var protectedNodes = new HashSet<string> { "TK-001", "TK-002", "TK-003", "TK-004", "TK-005", "TK-006", "TK-007", "TK-008" };
        if (protectedNodes.Contains(id))
        {
            throw new ConflictException($"Machine with ID {id} is protected and cannot be deleted.");
        }

        var deleted = await machineRepository.DeleteAsync(id);
        if (!deleted)
        {
            throw new NotFoundException($"Machine with ID {id} not found.");
        }

        // Emit a delete event to SignalR
        await channelWriter.WriteAsync(new MachineStateUpdateMessage("Delete", new MachineNode { Id = id }));
    }
}
