using System.Reflection;
using FluentValidation;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyLibrary.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all validators from the Application assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Application services
        services.AddScoped<IFamilyRoleService, FamilyRoleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IRecognitionRuleService, RecognitionRuleService>();
        services.AddScoped<IFamilyService, FamilyService>();
        services.AddScoped<IDraftService, DraftService>();
        services.AddScoped<ISystemTypeService, SystemTypeService>();

        return services;
    }
}
