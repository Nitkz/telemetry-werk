using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Infrastructure.Repositories;

public class InMemoryMachineRepository : IMachineRepository
{
    // Thread-safe dictionary for in-memory storage
    private readonly ConcurrentDictionary<string, MachineNode> _machines = new();

    public InMemoryMachineRepository()
    {
        // Pre-seed some mock data
        for (int i = 1; i <= 8; i++)
        {
            var id = $"TK-00{i}";
            _machines.TryAdd(id, new MachineNode
            {
                Id = id,
                Status = i == 4 ? "Warning" : i == 7 ? "Stopped" : "Running",
                CoreTemperature = 36.5
            });
        }
    }

    public Task<IEnumerable<MachineNode>> GetAllAsync(int limit, string? afterId = null)
    {
        var query = _machines.Values.OrderBy(m => m.Id).AsQueryable();
        
        if (!string.IsNullOrEmpty(afterId))
        {
            query = query.Where(m => string.Compare(m.Id, afterId, StringComparison.Ordinal) > 0);
        }

        return Task.FromResult(query.Take(limit).AsEnumerable());
    }

    public Task<MachineNode?> GetByIdAsync(string id)
    {
        _machines.TryGetValue(id, out var machine);
        return Task.FromResult(machine);
    }

    public Task<MachineNode> AddAsync(MachineNode machine)
    {
        _machines.AddOrUpdate(machine.Id, machine, (_, __) => machine);
        return Task.FromResult(machine);
    }

    public Task<MachineNode?> UpdateAsync(string id, string? status, double? coreTemperature)
    {
        if (_machines.TryGetValue(id, out var machine))
        {
            if (status != null) machine.Status = status;
            if (coreTemperature.HasValue) machine.CoreTemperature = coreTemperature.Value;
            return Task.FromResult<MachineNode?>(machine);
        }
        return Task.FromResult<MachineNode?>(null);
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_machines.TryRemove(id, out _));
    }
}
