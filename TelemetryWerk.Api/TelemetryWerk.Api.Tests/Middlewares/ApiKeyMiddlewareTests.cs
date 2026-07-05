using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TelemetryWerk.Api.Application.Configurations;
using TelemetryWerk.Api.Host.Middlewares;

namespace TelemetryWerk.Api.Tests.Middlewares;

public class ApiKeyMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly DefaultHttpContext _context;

    public ApiKeyMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<ApiKeyMiddleware>>();
        _context = new DefaultHttpContext();
    }

    private async Task InvokeMiddlewareAsync(string apiKey)
    {
        var options = Options.Create(new ApiServiceOptions { ApiKey = apiKey });
        var sut = new ApiKeyMiddleware(_next);
        await sut.InvokeAsync(_context, options, _logger);
    }

    [Fact]
    public async Task ShouldReturn401_WhenNoApiKeyProvided()
    {
        // Arrange
        var expectedKey = "secret123";
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ShouldReturn401_WhenWrongApiKeyProvided()
    {
        // Arrange
        var expectedKey = "secret123";
        _context.Request.Headers["X-Api-Key"] = "wrongKey";
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectApiKeyInHeader()
    {
        // Arrange
        var expectedKey = "secret123";
        _context.Request.Headers["X-Api-Key"] = expectedKey;
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectApiKeyInQueryString()
    {
        // Arrange
        var expectedKey = "secret123";
        _context.Request.QueryString = new QueryString($"?access_token={expectedKey}");
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenNoApiKeyConfigured()
    {
        // Arrange
        var expectedKey = ""; // Empty implies disabled
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        await _next.Received(1).Invoke(_context);
    }

    [Fact]
    public async Task ShouldPassThrough_WhenCorrectApiKeyInAuthorizationHeader()
    {
        // Arrange
        var expectedKey = "secret123";
        _context.Request.Headers["Authorization"] = $"Bearer {expectedKey}";
        
        // Act
        await InvokeMiddlewareAsync(expectedKey);

        // Assert
        await _next.Received(1).Invoke(_context);
    }
}
