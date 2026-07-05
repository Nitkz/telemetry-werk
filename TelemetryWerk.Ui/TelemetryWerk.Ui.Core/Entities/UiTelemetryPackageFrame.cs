namespace TelemetryWerk.Ui.Core.Entities;

public class UiTelemetryPackageFrame
{
    public string ServerTime { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string FrameHex { get; set; } = string.Empty;
    public List<UiMachineTelemetry> Metrics { get; set; } = new();
}
