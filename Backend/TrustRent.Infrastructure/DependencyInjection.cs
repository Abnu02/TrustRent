using Microsoft.Extensions.DependencyInjection;
using TrustRent.Application.Interfaces;
using TrustRent.Application.Services;
using TrustRent.Domain.Repositories;
using TrustRent.Infrastructure.Repositories;

namespace TrustRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Singleton repositories so in-memory changes persist while the app is running
        services.AddSingleton<IPropertyRepository, InMemoryPropertyRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        // Application Services
        services.AddScoped<ILandlordPropertyService, LandlordPropertyService>();

        return services;
    }
}
