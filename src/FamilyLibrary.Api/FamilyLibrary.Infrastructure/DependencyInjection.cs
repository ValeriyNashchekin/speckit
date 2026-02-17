using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Infrastructure.Data;
using FamilyLibrary.Infrastructure.Repositories;
using FamilyLibrary.Infrastructure.Services;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Application.Interfaces;

namespace FamilyLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IFamilyRoleRepository, FamilyRoleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyVersionRepository, FamilyVersionRepository>();
        services.AddScoped<ISystemTypeRepository, SystemTypeRepository>();
        services.AddScoped<IDraftRepository, DraftRepository>();
        services.AddScoped<IRecognitionRuleRepository, RecognitionRuleRepository>();
        services.AddScoped<IFamilyNameMappingRepository, FamilyNameMappingRepository>();

        // Services
        services.AddScoped<IBlobStorageService, BlobStorageService>();

        return services;
    }
}
