using Microsoft.AspNetCore.Identity;


using TrustRent.Application.Abstractions.Authentication;
using TrustRent.Application.DTOs.Auth;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(bool Success, string? Error, UserResponse? User)>
        RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
    {
        var role = request.Role?.Trim();

        // Only Tenant and Landlord can register themselves.
        if (!string.Equals(role, "Tenant", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Landlord", StringComparison.OrdinalIgnoreCase))
        {
            return (
                false,
                "Only Tenant and Landlord roles are allowed for registration.",
                null
            );
        }

        // Check duplicate email.
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return (
                false,
                "A user with this email already exists.",
                null
            );
        }

        var normalizedRole =
            string.Equals(
                role,
                "Tenant",
                StringComparison.OrdinalIgnoreCase)
                ? "Tenant"
                : "Landlord";

        // Make sure the role exists.
        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            return (
                false,
                $"The role '{normalizedRole}' does not exist.",
                null
            );
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            IsVerified = false
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(e => e.Description));

            return (
                false,
                errors,
                null
            );
        }

        // Assign selected role.
        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                normalizedRole);

        if (!roleResult.Succeeded)
        {
            // Roll back user creation if role assignment fails.
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(e => e.Description));

            return (
                false,
                errors,
                null
            );
        }

        var response = new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = normalizedRole,
            IsVerified = user.IsVerified
        };

        return (
            true,
            null,
            response
        );
    }

    public async Task<(bool Success, string? Error, LoginResponse? Response)>
        LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return (
                false,
                "Invalid email or password.",
                null
            );
        }

        var signInResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return (
                false,
                "Invalid email or password.",
                null
            );
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var role = roles.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role))
        {
            return (
                false,
                "User does not have a valid role.",
                null
            );
        }

        var tokenData = new UserTokenData
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles
        };

        var accessToken =
            await _jwtTokenService.CreateTokenAsync(tokenData);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = role,
            IsVerified = user.IsVerified
        };

        return (
            true,
            null,
            response
        );
    }

    public async Task LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // JWT authentication is stateless.
        // The client removes its access token.
        await Task.CompletedTask;
    }
}