using System.Collections.Concurrent;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Infrastructure.Repositories;

public class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<string, string> _sessions = new();

    public Task<string> CreateSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var sessionKey = Guid.NewGuid().ToString("N");
        _sessions.TryAdd(sessionKey, userId);
        return Task.FromResult(sessionKey);
    }

    public Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sessions.ContainsKey(sessionKey));
    }

    public Task RemoveSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        _sessions.TryRemove(sessionKey, out _);
        return Task.CompletedTask;
    }
}
