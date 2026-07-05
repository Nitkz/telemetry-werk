using TelemetryWerk.Api.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelemetryWerk.Api.Domain.Interfaces;

/// <summary>
/// Interface for managing machine state.
/// This abstraction allows us to easily migrate from in-memory storage 
/// to a real Database (e.g. SQL Server, MongoDB) in the future without changing business logic.
/// </summary>
public interface IMachineRepository
{
    Task<IEnumerable<MachineNode>> GetAllAsync(int limit, string? afterId = null);
    Task<MachineNode?> GetByIdAsync(string id);
    Task<MachineNode> AddAsync(MachineNode machine);
    Task<MachineNode?> UpdateAsync(string id, string? status, double? coreTemperature);
    Task<bool> DeleteAsync(string id);
}
