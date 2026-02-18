using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyNameMappingConfiguration : IEntityTypeConfiguration<FamilyNameMappingEntity>
{
    public void Configure(EntityTypeBuilder<FamilyNameMappingEntity> builder)
    {
        builder.ToTable("FamilyNameMappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FamilyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ProjectId)
            .IsRequired();

        builder.Property(m => m.LastSeenAt)
            .IsRequired();

        builder.HasIndex(m => new { m.FamilyName, m.ProjectId })
            .IsUnique();

        builder.HasIndex(m => m.RoleName);
    }
}
