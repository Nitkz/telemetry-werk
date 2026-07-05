using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Application.Services;

namespace TelemetryWerk.Api.Tests.Services;

public class TelemetryIngestionServiceTests
{
    private readonly ITelemetryPublisher _telemetryPublisher;
    private readonly ILogger<TelemetryIngestionService> _logger;
    private readonly TelemetryIngestionService _sut;

    public TelemetryIngestionServiceTests()
    {
        _telemetryPublisher = Substitute.For<ITelemetryPublisher>();
        _logger = Substitute.For<ILogger<TelemetryIngestionService>>();
        
        _sut = new TelemetryIngestionService(_telemetryPublisher, _logger);
    }

    [Fact]
    public async Task ProcessTelemetryAsync_WithMetrics_ShouldPublishPackage()
    {
        // Arrange
        var metrics = new List<MachineTelemetryDto>
        {
            new() { Id = "Node1", CoreTemperature = 40.5 }
        };

        // Act
        await _sut.ProcessTelemetryAsync(metrics);

        // Assert
        await _telemetryPublisher.Received(1).PublishAsync(Arg.Is<TelemetryPackageFrame>(p => p.Metrics.Count == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessTelemetryAsync_WithEmptyMetrics_ShouldNotPublish()
    {
        // Arrange
        var metrics = new List<MachineTelemetryDto>();

        // Act
        await _sut.ProcessTelemetryAsync(metrics);

        // Assert
        await _telemetryPublisher.DidNotReceive().PublishAsync(Arg.Any<TelemetryPackageFrame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessTelemetryAsync_ShouldGenerateFrameHex()
    {
        // Arrange
        var metrics = new List<MachineTelemetryDto>
        {
            new() { Id = "Node1", CoreTemperature = 40.5 }
        };

        // Act
        await _sut.ProcessTelemetryAsync(metrics);

        // Assert
        await _telemetryPublisher.Received(1).PublishAsync(Arg.Is<TelemetryPackageFrame>(p => !string.IsNullOrEmpty(p.FrameHex) && p.FrameHex.StartsWith("0x")), Arg.Any<CancellationToken>());
    }
}
