using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace TelemetryWerk.Ui.Client.Auth;

/// <summary>
/// This provider runs ONLY on the WebAssembly client side.
/// Its job is to "unpack" the SessionKey that was embedded into the initial HTML payload
/// by the PersistingServerAuthenticationStateProvider during Prerendering.
/// By doing this, the WASM client immediately knows who is logged in without needing to make an extra API call.
/// </summary>
public class PersistentAuthenticationStateProvider(
    PersistentComponentState state, 
    ILogger<PersistentAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> DefaultUnauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly (string? SessionKey, Task<AuthenticationState> AuthTask) _stateInfo = Initialize(state, logger);

    public string? SessionKey => _stateInfo.SessionKey;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _stateInfo.AuthTask;

    /// <summary>
    /// Attempts to read the "SessionInfo" JSON that was embedded into the page by the server.
    /// If found, we create a ClaimsPrincipal locally so the UI knows we are authenticated.
    /// </summary>
    private static (string?, Task<AuthenticationState>) Initialize(
        PersistentComponentState state, 
        ILogger<PersistentAuthenticationStateProvider> logger)
    {
        if (!state.TryTakeFromJson<SessionInfo>("SessionInfo", out var sessionInfo))
        {
            logger.LogDebug("No SessionInfo found in PersistentComponentState.");
            return (null, DefaultUnauthenticatedTask);
        }
        
        if (sessionInfo?.SessionKey is null)
        {
            logger.LogWarning("SessionInfo was found but SessionKey is null.");
            return (null, DefaultUnauthenticatedTask);
        }

        logger.LogDebug("Successfully unpacked SessionKey: {SessionKey}...", sessionInfo.SessionKey.Substring(0, Math.Min(5, sessionInfo.SessionKey.Length)));
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Admin"),
            new Claim("SessionKey", sessionInfo.SessionKey)
        };
        
        var identity = new ClaimsIdentity(claims, nameof(PersistentAuthenticationStateProvider));
        var principal = new ClaimsPrincipal(identity);
        
        return (sessionInfo.SessionKey, Task.FromResult(new AuthenticationState(principal)));
    }

    public class SessionInfo
    {
        public string? SessionKey { get; set; }
    }
}
