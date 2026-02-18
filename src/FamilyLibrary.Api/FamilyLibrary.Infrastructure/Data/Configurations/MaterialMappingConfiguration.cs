using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class MaterialMappingConfiguration : IEntityTypeConfiguration<MaterialMappingEntity>
{
    public void Configure(EntityTypeBuilder<MaterialMappingEntity> builder)
    {
        builder.ToTable("MaterialMappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProjectId)
            .IsRequired();

        builder.Property(m => m.TemplateMaterialName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.ProjectMaterialName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.LastUsedAt)
            .IsRequired(false);

        // Unique index: one template material per project
        builder.HasIndex(m => new { m.ProjectId, m.TemplateMaterialName })
            .HasDatabaseName("IX_MaterialMapping_ProjectId_TemplateName")
            .IsUnique();

        // Regular index for querying by project
        builder.HasIndex(m => m.ProjectId)
            .HasDatabaseName("IX_MaterialMapping_ProjectId");
    }
}
