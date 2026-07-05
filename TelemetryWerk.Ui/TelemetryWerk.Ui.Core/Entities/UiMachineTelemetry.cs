namespace TelemetryWerk.Ui.Core.Entities;

public class UiMachineTelemetry
{
    public string Id { get; set; } = string.Empty;
    public double CoreTemperature { get; set; }
    public double PressurePercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
}
