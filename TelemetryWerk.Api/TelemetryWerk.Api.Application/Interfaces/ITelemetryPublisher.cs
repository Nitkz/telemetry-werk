using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Interfaces;

public interface ITelemetryPublisher
{
    Task PublishAsync(TelemetryPackageFrame package, CancellationToken cancellationToken = default);
}
