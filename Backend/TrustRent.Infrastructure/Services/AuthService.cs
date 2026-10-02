using Microsoft.AspNetCore.Identity;
using TrustRent.Application.Auth.DTOs;
using TrustRent.Application.Interfaces;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    public async Task RegisterAsync(RegisterRequest request)
    {
        if (!string.Equals(request.Role, Roles.Landlord, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only landlord accounts can be registered here.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new IdentityOperationException(createResult.Errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, Roles.Landlord);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new IdentityOperationException(roleResult.Errors);
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!await userManager.IsInRoleAsync(user, Roles.Landlord))
        {
            throw new UnauthorizedAccessException("This account cannot access the landlord workspace.");
        }

        var userDto = new UserDto
        {
            Id = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = Roles.Landlord,
            IsVerified = user.IsVerified,
        };

        return new AuthResponse { AccessToken = tokenGenerator.GenerateToken(userDto), User = userDto };
    }
}

public sealed class IdentityOperationException(IEnumerable<IdentityError> errors)
    : Exception(string.Join(" ", errors.Select(error => error.Description)));