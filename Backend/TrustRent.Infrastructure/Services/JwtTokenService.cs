using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TrustRent.Application.Abstractions.Authentication;
using TrustRent.Application.DTOs.Auth;

namespace TrustRent.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public Task<string> CreateTokenAsync(UserTokenData user)
{
    if (string.IsNullOrWhiteSpace(_jwtOptions.Key))
    {
        throw new InvalidOperationException(
            "JWT Key is missing or empty. Check the Jwt configuration section."
        );
    }

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.FullName),
        new(ClaimTypes.Email, user.Email)
    };

    foreach (var role in user.Roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_jwtOptions.Key));

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer,
        audience: _jwtOptions.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(
            _jwtOptions.ExpirationMinutes),
        signingCredentials: credentials);

    var accessToken = new JwtSecurityTokenHandler()
        .WriteToken(token);

    return Task.FromResult(accessToken);
}
}