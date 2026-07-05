namespace TelemetryWerk.Api.Application.Configurations;

public class ApiServiceOptions
{
    public const string SectionName = "Api";
    public string ApiEndpoint { get; set; } = "http://localhost:5000";
}
