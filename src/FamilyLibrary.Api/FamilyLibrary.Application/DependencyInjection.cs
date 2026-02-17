using Microsoft.Extensions.DependencyInjection;

namespace FamilyLibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application layer DI registration
        // Services, Validators, Mappers will be registered here

        return services;
    }
}
