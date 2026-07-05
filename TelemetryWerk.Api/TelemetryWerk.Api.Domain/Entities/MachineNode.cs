namespace TelemetryWerk.Api.Domain.Entities;

public class MachineNode
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "Running";
    public double CoreTemperature { get; set; } = 0;
    // We can add more domain-specific logic and properties here later
}
