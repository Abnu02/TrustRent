using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrustRent.Application.Auth;
using TrustRent.Application.Auth.DTOs;
using TrustRent.Application.Interfaces;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (request.Role != Roles.Tenant && request.Role != Roles.Landlord)
        {
            throw new Exception("Invalid role. Only Tenant or Landlord are allowed.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to assign role: {errors}");
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var userDto = new UserDto
        {
            Id = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email!,
            Role = role,
            IsVerified = user.IsVerified
        };

        var token = _jwtTokenGenerator.GenerateToken(userDto);

        return new AuthResponse
        {
            AccessToken = token,
            User = userDto
        };
    }
}
