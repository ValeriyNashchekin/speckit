using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FamilyLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Infrastructure layer DI registration
        // DbContext, Repositories, External Services will be registered here

        return services;
    }
}
