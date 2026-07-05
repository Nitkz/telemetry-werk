namespace TelemetryWerk.Ui.Core.Configurations;

public class ApiServiceOptions
{
    public const string SectionName = "ApiService";
    public const string DefaultApiEndpoint = "http://localhost:5000";
    
    public string ApiEndpoint { get; set; } = DefaultApiEndpoint;
    public string ApiKey { get; set; } = string.Empty;
}
