using Microsoft.AspNetCore.SignalR;
using TelemetryWerk.Api.Host.Hubs;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Host.Workers;

public class TelemetryBackgroundIngester(IHubContext<TelemetryHub> hubContext, IMachineRepository machineRepository) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(60)); // 60ms Interval High-Speed Push

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Fetch all currently configured machines from the state repository
            var activeMachines = await machineRepository.GetAllAsync(100);

            var mockMetrics = new List<MachineTelemetryDto>();
            foreach (var machine in activeMachines)
            {
                // Simulate fluctuating machine data based on the machine's base properties
                mockMetrics.Add(new MachineTelemetryDto
                {
                    Id = machine.Id,
                    CoreTemperature = machine.CoreTemperature + (_random.NextDouble() * 5), // fluctuate around base temp
                    PressurePercentage = 40 + _random.Next(0, 40),
                    FlowRate = 200 + _random.NextDouble() * 60,
                    Status = machine.Status
                });
            }

            if (mockMetrics.Any())
            {
                var packet = new TelemetryPackageFrame
                {
                    Timestamp = DateTime.UtcNow,
                    FrameHex = $"0x{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper()}",
                    Metrics = mockMetrics
                };

                // Push raw data into WebSockets pipeline to all active Client dashboards
                await hubContext.Clients.All.SendAsync("ReceiveTelemetryPackage", packet, stoppingToken);
            }
        }
    }
}
