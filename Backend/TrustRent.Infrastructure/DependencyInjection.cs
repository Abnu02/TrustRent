using Microsoft.Extensions.DependencyInjection;
using TrustRent.Application.Common;
using TrustRent.Infrastructure.Data;

namespace TrustRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Singleton central data store for CQRS commands and queries
        services.AddSingleton<ITrustRentDataStore, TrustRentDataStore>();

        return services;
    }
}
