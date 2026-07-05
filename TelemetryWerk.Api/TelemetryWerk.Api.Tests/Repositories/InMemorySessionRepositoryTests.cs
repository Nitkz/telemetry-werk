using FluentAssertions;
using TelemetryWerk.Api.Infrastructure.Repositories;
using Xunit;

namespace TelemetryWerk.Api.Tests.Repositories;

public class InMemorySessionRepositoryTests
{
    private readonly InMemorySessionRepository _repository;

    public InMemorySessionRepositoryTests()
    {
        _repository = new InMemorySessionRepository();
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldReturnValidSessionKey()
    {
        // Act
        var sessionKey = await _repository.CreateSessionAsync("user1");

        // Assert
        sessionKey.Should().NotBeNullOrWhiteSpace();
        var isValid = await _repository.ValidateSessionAsync(sessionKey);
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSessionAsync_ExceedingMaxConcurrentLimit_ShouldEvictOldestSession()
    {
        // Arrange
        var userId = "user1";
        var sessions = new List<string>();

        // Act - Create 10 sessions (the max limit)
        for (int i = 0; i < 10; i++)
        {
            var key = await _repository.CreateSessionAsync(userId);
            sessions.Add(key);
            // Small delay to ensure LastAccessedTime is slightly different for ordering
            await Task.Delay(10);
        }

        var oldestSessionKey = sessions.First();
        
        // Act - Create the 11th session (exceeding limit)
        var newSessionKey = await _repository.CreateSessionAsync(userId);
        sessions.Add(newSessionKey);

        var isOldestValidAfter = await _repository.ValidateSessionAsync(oldestSessionKey);
        var isNewestValid = await _repository.ValidateSessionAsync(newSessionKey);

        // Assert after exceeding
        isOldestValidAfter.Should().BeFalse("The oldest session should be evicted when concurrent limit is exceeded.");
        isNewestValid.Should().BeTrue("The newest session should be valid.");
        
        // Verify other existing sessions are still valid (e.g. the second oldest)
        var secondOldestKey = sessions[1];
        var isSecondOldestValid = await _repository.ValidateSessionAsync(secondOldestKey);
        isSecondOldestValid.Should().BeTrue("Other sessions within the limit should remain valid.");
    }

    [Fact]
    public async Task RemoveSessionAsync_ShouldInvalidateSession()
    {
        // Arrange
        var sessionKey = await _repository.CreateSessionAsync("user1");

        // Act
        await _repository.RemoveSessionAsync(sessionKey);

        // Assert
        var isValid = await _repository.ValidateSessionAsync(sessionKey);
        isValid.Should().BeFalse();
    }
}
