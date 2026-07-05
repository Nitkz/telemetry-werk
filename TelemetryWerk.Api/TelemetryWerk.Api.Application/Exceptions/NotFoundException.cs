namespace TelemetryWerk.Api.Application.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
