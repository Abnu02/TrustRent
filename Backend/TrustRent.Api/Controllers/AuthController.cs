using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Auth.DTOs;
using TrustRent.Application.Interfaces;
using TrustRent.Infrastructure.Services;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            await authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { message = "Landlord account created." });
        }
        catch (IdentityOperationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Registration failed", detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid registration", detail: exception.Message);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            return Ok(await authService.LoginAsync(request));
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sign-in failed", detail: "The email or password is incorrect, or this account is not a landlord.");
        }
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout() => Ok(new { message = "Signed out." });
}