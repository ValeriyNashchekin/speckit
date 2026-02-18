using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class SystemTypeConfiguration : IEntityTypeConfiguration<SystemTypeEntity>
{
    public void Configure(EntityTypeBuilder<SystemTypeEntity> builder)
    {
        builder.ToTable("SystemTypes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.RoleId)
            .IsRequired();

        builder.Property(s => s.TypeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.SystemFamily)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Group)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Json)
            .IsRequired();

        builder.Property(s => s.CurrentVersion)
            .IsRequired();

        builder.Property(s => s.Hash)
            .IsRequired()
            .HasMaxLength(64);

        // Performance indexes per data-model.md
        builder.HasIndex(s => s.RoleId);
        builder.HasIndex(s => new { s.Category, s.TypeName });

        // Unique constraint: one type name per role
        builder.HasIndex(s => new { s.RoleId, s.TypeName })
            .IsUnique();

        builder.HasIndex(s => s.Hash);

        builder.HasOne(s => s.Role)
            .WithMany(r => r.SystemTypes)
            .HasForeignKey(s => s.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
