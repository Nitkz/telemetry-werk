namespace TelemetryWerk.Api.Application.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string password, CancellationToken cancellationToken = default);
    Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken cancellationToken = default);
    Task LogoutAsync(string sessionKey, CancellationToken cancellationToken = default);
}
