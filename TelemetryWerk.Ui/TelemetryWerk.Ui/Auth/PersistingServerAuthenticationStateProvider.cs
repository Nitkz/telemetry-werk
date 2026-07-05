using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;

namespace TelemetryWerk.Ui.Auth;

/// <summary>
/// This provider runs ONLY on the Server during Prerendering.
/// Its job is to extract the SessionKey from the server's HTTP Context (from the encrypted cookie)
/// and "pack" it into the initial HTML payload (via PersistentComponentState).
/// When the WASM client boots up, it reads this packed data to reconstruct the authentication state.
/// </summary>
internal sealed class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private readonly ILogger<PersistingServerAuthenticationStateProvider> _logger;
    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(PersistentComponentState state, ILogger<PersistingServerAuthenticationStateProvider> logger)
    {
        _state = state;
        _logger = logger;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var task = base.GetAuthenticationStateAsync();
        _authenticationStateTask = task;
        return task;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    /// <summary>
    /// This method is called by the Blazor framework just before the HTML is sent to the client.
    /// We grab the current AuthenticationState (which was populated by the cookie middleware)
    /// and serialize the SessionKey into a JSON string embedded in a hidden HTML comment.
    /// </summary>
    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            _authenticationStateTask = GetAuthenticationStateAsync();
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var sessionKey = principal.FindFirst("SessionKey")?.Value;

            if (sessionKey != null)
            {
                _logger.LogDebug("Packing SessionKey: {SessionKey}...", sessionKey.Substring(0, Math.Min(5, sessionKey.Length)));
                _state.PersistAsJson("SessionInfo", new { SessionKey = sessionKey });
            }
            else 
            {
                _logger.LogWarning("SessionKey claim not found in authenticated user!");
            }
        }
        else 
        {
            _logger.LogDebug("User is NOT authenticated, skipping SessionKey packing.");
        }
    }

    public void Dispose()
    {
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        _subscription.Dispose();
    }
}
