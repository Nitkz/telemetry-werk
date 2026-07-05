using Microsoft.AspNetCore.SignalR;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Host.Hubs;

namespace TelemetryWerk.Api.Host.Publishers;

public class SignalRTelemetryPublisher(IHubContext<TelemetryHub> hubContext) : ITelemetryPublisher
{
    public async Task PublishAsync(TelemetryPackageFrame package, CancellationToken cancellationToken = default)
    {
        // Push raw data into WebSockets pipeline to all active Client dashboards
        await hubContext.Clients.All.SendAsync("ReceiveTelemetryPackage", package, cancellationToken);
    }
}
