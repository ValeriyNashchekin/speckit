using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyDependencyConfiguration : IEntityTypeConfiguration<FamilyDependencyEntity>
{
    public void Configure(EntityTypeBuilder<FamilyDependencyEntity> builder)
    {
        builder.ToTable("FamilyDependencies");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ParentFamilyId)
            .IsRequired();

        builder.Property(d => d.NestedFamilyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.NestedRoleName)
            .HasMaxLength(100);

        builder.Property(d => d.IsShared)
            .IsRequired();

        builder.Property(d => d.InLibrary)
            .IsRequired();

        builder.Property(d => d.LibraryVersion);

        builder.Property(d => d.DetectedAt)
            .IsRequired();

        // Indexes per data-model.md
        builder.HasIndex(d => d.ParentFamilyId)
            .HasDatabaseName("IX_FamilyDependency_ParentFamilyId");

        builder.HasIndex(d => d.NestedRoleName)
            .HasDatabaseName("IX_FamilyDependency_NestedRoleName")
            .HasFilter("[NestedRoleName] IS NOT NULL");

        // Foreign key relationship to FamilyEntity
        builder.HasOne(d => d.ParentFamily)
            .WithMany()
            .HasForeignKey(d => d.ParentFamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
