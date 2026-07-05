using System.Threading.Channels;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Exceptions;

namespace TelemetryWerk.Api.Application.Services;

public class MachineService(IMachineRepository machineRepository, ChannelWriter<MachineStateUpdateMessage> channelWriter) : IMachineService
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

        if (updatedNode == null) throw new NotFoundException($"Machine with ID {id} not found.");

        await channelWriter.WriteAsync(new MachineStateUpdateMessage("AddOrUpdate", updatedNode));

        return new MachineNodeDto 
        { 
            Id = updatedNode.Id, 
            Status = updatedNode.Status, 
            CoreTemperature = updatedNode.CoreTemperature 
        };
    }
}
