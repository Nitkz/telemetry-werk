using System.Threading.Tasks;
using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Interfaces;

public interface IMachineService
{
    Task<PagedCollection<MachineNodeDto>> GetNodesAsync(int limit, string? afterId);
    Task<MachineNodeDto> AddNodeAsync(MachineNodeDto request);
    Task<MachineNodeDto> UpdateNodeAsync(string id, UpdateNodeRequestDto request);
}
