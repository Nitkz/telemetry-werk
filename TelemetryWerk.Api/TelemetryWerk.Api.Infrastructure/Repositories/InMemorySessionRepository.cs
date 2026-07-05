using System.Collections.Concurrent;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Infrastructure.Repositories;

public class InMemorySessionRepository : ISessionRepository
{
    private class SessionData
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime LastAccessedTime { get; set; }
    }

    private readonly ConcurrentDictionary<string, SessionData> _sessions = new();
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromHours(24);
    private const int MaxConcurrentSessions = 10;

    public Task<string> CreateSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        CleanupExpiredSessions();

        // Enforce max concurrent sessions limit
        var userSessions = _sessions.Where(kvp => kvp.Value.UserId == userId).ToList();
        if (userSessions.Count >= MaxConcurrentSessions)
        {
            var oldestSession = userSessions.OrderBy(kvp => kvp.Value.LastAccessedTime).First();
            _sessions.TryRemove(oldestSession.Key, out _);
        }

        var sessionKey = Guid.NewGuid().ToString("N");
        _sessions.TryAdd(sessionKey, new SessionData { UserId = userId, LastAccessedTime = DateTime.UtcNow });
        
        return Task.FromResult(sessionKey);
    }

    public Task<bool> ValidateSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(sessionKey, out var sessionData))
        {
            if (DateTime.UtcNow - sessionData.LastAccessedTime > SessionTimeout)
            {
                // Session expired
                _sessions.TryRemove(sessionKey, out _);
                return Task.FromResult(false);
            }
            else
            {
                // Extend session lifetime
                sessionData.LastAccessedTime = DateTime.UtcNow;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public Task RemoveSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        _sessions.TryRemove(sessionKey, out _);
        return Task.CompletedTask;
    }

    private void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _sessions.Where(kvp => now - kvp.Value.LastAccessedTime > SessionTimeout).Select(kvp => kvp.Key).ToList();
        foreach (var key in expiredKeys)
        {
            _sessions.TryRemove(key, out _);
        }
    }
}
