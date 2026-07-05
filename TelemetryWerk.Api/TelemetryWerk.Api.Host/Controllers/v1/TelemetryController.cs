using Microsoft.AspNetCore.Mvc;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Interfaces;

namespace TelemetryWerk.Api.Host.Controllers.v1;

/// <summary>
/// Telemetry Controller
/// Pattern: Clean Architecture / Thin Controller
/// This controller is responsible ONLY for HTTP concerns (Routing, Model Binding, HTTP Status Codes).
/// All business logic, data fetching, and DTO mapping have been delegated to the Application layer (IMachineService).
/// </summary>
[ApiController] [Route("api/v1/[controller]")]
public class TelemetryController(IMachineService machineService) : ControllerBase
{
    // GET: api/v1/telemetry/nodes?afterId=TK-004&limit=20
    [HttpGet] [Route("nodes")]
    public async Task<ActionResult<UnifiedResponse<PagedCollection<MachineNodeDto>>>> GetNodes([FromQuery] string? afterId, [FromQuery] int limit = 20)
    {
        var result = await machineService.GetNodesAsync(limit, afterId);

        var response = new UnifiedResponse<PagedCollection<MachineNodeDto>>
        {
            Data = result
        };

        return Ok(response);
    }

    // POST: api/v1/telemetry/nodes
    [HttpPost] [Route("nodes")]
    public async Task<ActionResult<UnifiedResponse<MachineNodeDto>>> AddNode([FromBody] MachineNodeDto request)
    {
        var result = await machineService.AddNodeAsync(request);

        var response = new UnifiedResponse<MachineNodeDto>
        {
            Data = result
        };

        return CreatedAtAction(nameof(GetNodes), new { }, response);
    }

    // PATCH: api/v1/telemetry/nodes/TK-005
    [HttpPatch] [Route("nodes/{id}")]
    public async Task<ActionResult<UnifiedResponse<MachineNodeDto>>> UpdateNodePartial([FromRoute] string id, [FromBody] UpdateNodeRequestDto request)
    {
        var result = await machineService.UpdateNodeAsync(id, request);

        var response = new UnifiedResponse<MachineNodeDto>
        {
            Data = result
        };

        return Ok(response);
    }
}
