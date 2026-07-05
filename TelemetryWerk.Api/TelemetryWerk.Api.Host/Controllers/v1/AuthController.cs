using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TelemetryWerk.Api.Application.Interfaces;

namespace TelemetryWerk.Api.Host.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginLimiter")]
    public async Task<IActionResult> Login([FromForm] string password, CancellationToken cancellationToken)
    {
        try
        {
            var sessionKey = await _authService.LoginAsync(password, cancellationToken);
            if (sessionKey != null)
            {
                return Ok(new { token = sessionKey });
            }
            return Unauthorized(new { message = "Invalid password." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader(Name = "X-Session-Key")] string sessionKey, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(sessionKey, cancellationToken);
        return Ok();
    }
}
