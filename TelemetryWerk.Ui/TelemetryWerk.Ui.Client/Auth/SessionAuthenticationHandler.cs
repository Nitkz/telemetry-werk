using System.Net.Http.Headers;

namespace TelemetryWerk.Ui.Client.Auth;

public class SessionAuthenticationHandler : DelegatingHandler
{
    private readonly PersistentAuthenticationStateProvider _authStateProvider;

    public SessionAuthenticationHandler(PersistentAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sessionKey = _authStateProvider.SessionKey;

        if (!string.IsNullOrEmpty(sessionKey))
        {
            request.Headers.Add("X-Session-Key", sessionKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
