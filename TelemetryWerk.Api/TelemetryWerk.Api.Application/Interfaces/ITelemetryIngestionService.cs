using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Interfaces;

public interface ITelemetryIngestionService
{
    Task ProcessTelemetryAsync(IEnumerable<MachineTelemetryDto> metrics, CancellationToken cancellationToken = default);
}
