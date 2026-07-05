namespace TelemetryWerk.Api.Application.Exceptions;

public class ValidationException(string message) : Exception(message)
{
}
