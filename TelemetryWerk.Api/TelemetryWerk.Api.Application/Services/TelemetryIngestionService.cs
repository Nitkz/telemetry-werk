using Microsoft.Extensions.Logging;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Interfaces;

namespace TelemetryWerk.Api.Application.Services;

public class TelemetryIngestionService(ITelemetryPublisher telemetryPublisher, ILogger<TelemetryIngestionService> logger) : ITelemetryIngestionService
{
    public async Task ProcessTelemetryAsync(IEnumerable<MachineTelemetryDto> metrics, CancellationToken cancellationToken = default)
    {
        var metricsList = metrics.ToList();
        
        if (!metricsList.Any())
        {
            return;
        }

        var packet = new TelemetryPackageFrame
        {
            Timestamp = DateTime.UtcNow,
            // Generate pseudo-hex generator for now
            FrameHex = $"0x{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper()}",
            Metrics = metricsList
        };

        logger.LogDebug("Processed {MetricsCount} metrics in batch", metricsList.Count);

        await telemetryPublisher.PublishAsync(packet, cancellationToken);
    }
}
