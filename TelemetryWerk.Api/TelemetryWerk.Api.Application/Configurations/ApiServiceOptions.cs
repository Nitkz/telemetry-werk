namespace TelemetryWerk.Api.Application.Configurations;

public class ApiServiceOptions
{
    public const string SectionName = "Api";
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultApiEndpoint { get; set; } = "http://localhost:5000";
}
