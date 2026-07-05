using System.Collections.Concurrent;
using System.Threading.Channels;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Host.Workers;

public class PseudoTelemetryGeneratorWorker(IServiceScopeFactory serviceScopeFactory, ChannelReader<MachineStateUpdateMessage> channelReader) : BackgroundService
{
    private readonly Random _random = new();
    private readonly ConcurrentDictionary<string, MachineNode> _activeMachines = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Initial DB Load
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
            var machines = await machineRepository.GetAllAsync(100);
            foreach (var m in machines)
            {
                _activeMachines[m.Id] = m;
            }
        }

        // 2. Background Channel Listener for live updates
        _ = Task.Run(async () =>
        {
            await foreach (var msg in channelReader.ReadAllAsync(stoppingToken))
            {
                if (msg.Action == "AddOrUpdate")
                {
                    _activeMachines[msg.Machine.Id] = msg.Machine;
                }
                else if (msg.Action == "Remove")
                {
                    _activeMachines.TryRemove(msg.Machine.Id, out _);
                }
            }
        }, stoppingToken);

        // 3. High-speed generator loop
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(60)); // 60ms Interval High-Speed Push

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var mockMetrics = new List<MachineTelemetryDto>();
            foreach (var machine in _activeMachines.Values)
            {
                if (string.Equals(machine.Status, "Running", StringComparison.OrdinalIgnoreCase))
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
                else
                {
                    // If stopped or in maintenance, send base/zero values
                    mockMetrics.Add(new MachineTelemetryDto
                    {
                        Id = machine.Id,
                        CoreTemperature = machine.CoreTemperature, // Slowly cools down or stays at base
                        PressurePercentage = 0,
                        FlowRate = 0,
                        Status = machine.Status
                    });
                }
            }

            if (mockMetrics.Any())
            {
                // Send generated mock data to the Application layer for processing
                using var scope = serviceScopeFactory.CreateScope();
                var telemetryIngestionService = scope.ServiceProvider.GetRequiredService<ITelemetryIngestionService>();
                await telemetryIngestionService.ProcessTelemetryAsync(mockMetrics, stoppingToken);
            }
        }
    }
}
