namespace TelemetryWerk.Ui.Core.Entities;

public class UiMachineNode
{
    public string Id { get; set; } = string.Empty;
    public double CoreTemperature { get; set; }
    public string Status { get; set; } = "Unknown";
    
    // UI-specific computed properties can also be added here in the future
    // e.g. public bool IsCritical => Status == "Critical";
}
