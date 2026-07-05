using MudBlazor;

namespace TelemetryWerk.Ui.Client.Services;

public static class IndustrialTheme
{
    public static MudTheme DashboardTheme => new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#00E5FF",          // Cyan for Interactive elements
            Secondary = "#00E676",        // Neon Green for Running state
            Warning = "#FFB300",          // Amber for Warning state
            Error = "#FF1744",            // Pure Red for Stopped/Critical state
            Info = "#29B6F6",             // Blue for System info
            Background = "#0D1117",       // Deep control room backdrop
            Surface = "#161B22",          // Module container fill
            AppbarBackground = "#090D10", // Header strip
            AppbarText = "#E6EDF2",       // Header text
            TextPrimary = "#E6EDF2",      // Crisp white
            TextSecondary = "#8B949E",    // Telemetry units gray
            DrawerBackground = "#090D10",
            ActionDefault = "#58A6FF",
            LinesDefault = "#21262D"       // Grid boundaries
        },
        PaletteLight = new PaletteLight()
        {
            Primary = "#0066CC",          // Blue for Interactive elements in light mode
            Secondary = "#00C853",        // Green for Running state
            Warning = "#FF9100",          // Orange for Warning state
            Error = "#D50000",            // Red for Stopped/Critical state
            Info = "#0288D1",             // Blue for System info
            Background = "#F4F6F8",       // Light gray backdrop
            Surface = "#FFFFFF",          // White module container
            AppbarBackground = "#FFFFFF", // White header strip
            AppbarText = "#24292E",       // Dark header text
            TextPrimary = "#24292E",      // Dark gray/black for crisp text
            TextSecondary = "#586069",    // Gray for telemetry units
            DrawerBackground = "#FFFFFF",
            ActionDefault = "#0366D6",
            LinesDefault = "#E1E4E8"       // Grid boundaries
        },
        LayoutProperties = new LayoutProperties()
    };
}