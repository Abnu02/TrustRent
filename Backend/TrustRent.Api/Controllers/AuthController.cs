using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Repositories;

namespace TrustRent.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Tags("Authentication")]
[Produces("application/json")]
public class AuthController(IUserRepository userRepository) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("User login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or credentials." });
        }

        // Mock JWT Token for hackathon / development
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{user.Role}")) +
                    ".signature";

        var response = new AuthResponse(
            token,
            new UserDto(user.Id, user.FullName, user.Email, user.Role)
        );

        return Ok(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("User registration")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            return BadRequest(new { message = "A user with this email already exists." });
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = request.Role.Trim(),
            IsVerified = request.Role == "Tenant" // Tenants auto-verified; Landlords require admin verification
        };

        await userRepository.AddAsync(newUser);

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{newUser.Id}:{newUser.Role}")) +
                    ".signature";

        return Created("", new AuthResponse(
            token,
            new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Role)
        ));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("User logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Successfully logged out." });
    }
}
