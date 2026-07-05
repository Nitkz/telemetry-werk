using Microsoft.Extensions.Configuration;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IConfiguration _configuration;

    public AuthService(ISessionRepository sessionRepository, IConfiguration configuration)
    {
        _sessionRepository = sessionRepository;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string password, CancellationToken cancellationToken = default)
    {
        var expectedPassword = _configuration["AppPassword"];
        
        if (string.IsNullOrWhiteSpace(expectedPassword))
        {
            throw new InvalidOperationException("Password is not set in configuration.");
        }

        if (password == expectedPassword)
        {
            return await _sessionRepository.CreateSessionAsync("Admin", cancellationToken);
        }

        return null;
    }

    public async Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionKey)) return false;
        return await _sessionRepository.ValidateSessionAsync(sessionKey, cancellationToken);
    }

    public async Task LogoutAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(sessionKey))
        {
            await _sessionRepository.RemoveSessionAsync(sessionKey, cancellationToken);
        }
    }
}
