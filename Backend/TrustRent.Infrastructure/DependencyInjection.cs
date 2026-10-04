using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TrustRent.Application.Abstractions.Authentication;
using TrustRent.Application.Abstractions.Persistence;
using TrustRent.Application.Abstractions.Storage;
using TrustRent.Application.Services;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Persistence;
using TrustRent.Infrastructure.Repositories;
using TrustRent.Infrastructure.Services;

namespace TrustRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TrustRentDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString(
                    "DefaultConnection")));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(10);
        })
        .AddRoles<ApplicationRole>()
        .AddSignInManager()
        .AddEntityFrameworkStores<TrustRentDbContext>()
        .AddDefaultTokenProviders();

        var jwtKey =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is missing.");

        services.AddAuthentication(
            JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            configuration["Jwt:Issuer"],

                        ValidAudience =
                            configuration["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtKey))
                    };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IPropertyRepository,
            PropertyRepository>();

        services.AddScoped<IPropertyService,
            PropertyService>();

        services.AddScoped<IAdminVerificationService,
            AdminVerificationService>();

        services.AddScoped<IFileStorageService,
            LocalFileStorageService>();

        return services;
    }
}