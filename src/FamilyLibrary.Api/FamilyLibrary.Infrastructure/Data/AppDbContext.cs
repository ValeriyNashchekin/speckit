using Microsoft.EntityFrameworkCore;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
