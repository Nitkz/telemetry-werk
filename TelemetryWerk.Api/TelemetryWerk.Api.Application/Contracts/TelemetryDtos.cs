namespace TelemetryWerk.Api.Application.Contracts;

public class MachineNodeDto
{
    public string Id { get; set; } = string.Empty;
    public double CoreTemperature { get; set; }
    public string Status { get; set; } = "Unknown";
}

public class UpdateNodeRequestDto
{
    public double? CoreTemperature { get; set; }
    public string? Status { get; set; }
}

public class MachineTelemetryDto
{
    public string Id { get; set; } = string.Empty;
    public double CoreTemperature { get; set; }
    public double PressurePercentage { get; set; }
    public double FlowRate { get; set; }
    public string Status { get; set; } = "Unknown";
}

public class TelemetryPackageFrame
{
    public DateTime Timestamp { get; set; }
    public string FrameHex { get; set; } = string.Empty;
    public List<MachineTelemetryDto> Metrics { get; set; } = new();
}
