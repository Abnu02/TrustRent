using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.Auth.Commands;
using TrustRent.Application.DTOs;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Tags("Authentication")]
[Produces("application/json")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("User login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            authResponse => Ok(authResponse),
            error => Unauthorized(new { message = error.Message })
        );
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("User registration")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.Password,
            request.Role
        );

        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            authResponse => Created("", authResponse),
            error => BadRequest(new { message = error.Message })
        );
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("User logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Successfully logged out." });
    }
}
