using MudBlazor;

namespace TelemetryWerk.Ui.Client.Extensions;

public static class MachineStatusExtensions
{
    public static readonly IReadOnlyList<string> AllStatuses = new[] { "Running", "Warning", "Stopped" };

    public static string GetStatusHex(this string status) => status switch
    {
        "Running" => "#00E676",
        "Warning" => "#FFB300",
        "Stopped" => "#FF1744",
        _ => "#8B949E"
    };

    public static Color GetStatusColor(this string status) => status switch
    {
        "Running" => Color.Secondary,
        "Warning" => Color.Warning,
        "Stopped" => Color.Error,
        _ => Color.Default
    };
}
