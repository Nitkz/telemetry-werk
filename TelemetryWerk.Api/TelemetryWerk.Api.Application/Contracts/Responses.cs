namespace TelemetryWerk.Api.Application.Contracts;

public class StatusResponseDto
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "OK";
    public string? ServerTime { get; set; } = DateTime.UtcNow.ToString("o");
}

public class UnifiedResponse<T>
{
    public StatusResponseDto Status { get; set; } = new();
    public T? Data { get; set; }
}

public class PagedCollection<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public string? LastId { get; set; }
    public bool HasMore { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}
