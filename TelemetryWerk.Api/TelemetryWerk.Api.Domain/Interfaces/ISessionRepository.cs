namespace TelemetryWerk.Api.Domain.Interfaces;

public interface ISessionRepository
{
    Task<string> CreateSessionAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(string sessionKey, CancellationToken cancellationToken = default);
}
