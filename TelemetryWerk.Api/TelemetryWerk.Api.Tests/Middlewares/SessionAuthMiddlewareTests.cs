using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Host.Middlewares;

namespace TelemetryWerk.Api.Tests.Middlewares;

public class SessionAuthMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionAuthMiddleware> _logger;
    private readonly ISessionRepository _sessionRepository;
    private readonly DefaultHttpContext _context;

    public SessionAuthMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<SessionAuthMiddleware>>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _context = new DefaultHttpContext();
    }

    private async Task InvokeMiddlewareAsync()
    {
        var sut = new SessionAuthMiddleware(_next);
        await sut.InvokeAsync(_context, _logger, _sessionRepository);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenPathIsLogin()
    {
        // Arrange
        _context.Request.Path = "/api/v1/auth/login";
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldReturn401_WhenNoSessionKeyProvided()
    {
        // Arrange
        _context.Request.Path = "/api/telemetry";
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ShouldReturn401_WhenInvalidSessionKeyProvided()
    {
        // Arrange
        _context.Request.Path = "/api/telemetry";
        _context.Request.Headers["X-Session-Key"] = "invalidKey";
        _sessionRepository.ValidateSessionAsync("invalidKey").Returns(false);
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectSessionKeyInHeader()
    {
        // Arrange
        var expectedKey = "validKey";
        _context.Request.Path = "/api/telemetry";
        _context.Request.Headers["X-Session-Key"] = expectedKey;
        _sessionRepository.ValidateSessionAsync(expectedKey).Returns(true);
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectSessionKeyInQueryString()
    {
        // Arrange
        var expectedKey = "validKey";
        _context.Request.Path = "/api/telemetry";
        _context.Request.QueryString = new QueryString($"?access_token={expectedKey}");
        _sessionRepository.ValidateSessionAsync(expectedKey).Returns(true);
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectSessionKeyInAuthorizationHeader()
    {
        // Arrange
        var expectedKey = "validKey";
        _context.Request.Path = "/api/telemetry";
        _context.Request.Headers["Authorization"] = $"Bearer {expectedKey}";
        _sessionRepository.ValidateSessionAsync(expectedKey).Returns(true);
        
        // Act
        await InvokeMiddlewareAsync();

        // Assert
        await _next.Received(1).Invoke(_context);
    }
}
