using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<FamilyRoleEntity> FamilyRoles => Set<FamilyRoleEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<FamilyEntity> Families => Set<FamilyEntity>();
    public DbSet<FamilyVersionEntity> FamilyVersions => Set<FamilyVersionEntity>();
    public DbSet<SystemTypeEntity> SystemTypes => Set<SystemTypeEntity>();
    public DbSet<DraftEntity> Drafts => Set<DraftEntity>();
    public DbSet<RecognitionRuleEntity> RecognitionRules => Set<RecognitionRuleEntity>();
    public DbSet<FamilyNameMappingEntity> FamilyNameMappings => Set<FamilyNameMappingEntity>();
    public DbSet<FamilyRoleTagEntity> FamilyRoleTags => Set<FamilyRoleTagEntity>();
    public DbSet<FamilyDependencyEntity> FamilyDependencies => Set<FamilyDependencyEntity>();
    public DbSet<MaterialMappingEntity> MaterialMappings => Set<MaterialMappingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=FamilyLibrary_Dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True");

        return new AppDbContext(optionsBuilder.Options);
    }
}
