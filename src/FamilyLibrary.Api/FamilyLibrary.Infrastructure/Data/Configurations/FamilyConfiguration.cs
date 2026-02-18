using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<FamilyEntity>
{
    public void Configure(EntityTypeBuilder<FamilyEntity> builder)
    {
        builder.ToTable("Families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FamilyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.CurrentVersion)
            .IsRequired();

        builder.Property(f => f.RoleId)
            .IsRequired();

        // Performance indexes per data-model.md
        // Covering index for batch check optimization - includes CurrentVersion to avoid key lookup
        builder.HasIndex(f => f.RoleId)
            .IncludeProperties(f => f.CurrentVersion);
        builder.HasIndex(f => f.FamilyName);

        // Unique constraint: one family name per role
        builder.HasIndex(f => new { f.RoleId, f.FamilyName })
            .IsUnique();

        builder.HasOne(f => f.Role)
            .WithMany(r => r.Families)
            .HasForeignKey(f => f.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Versions)
            .WithOne(v => v.Family)
            .HasForeignKey(v => v.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
