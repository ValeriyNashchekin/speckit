using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyRoleConfiguration : IEntityTypeConfiguration<FamilyRoleEntity>
{
    public void Configure(EntityTypeBuilder<FamilyRoleEntity> builder)
    {
        builder.ToTable("FamilyRoles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Performance indexes per data-model.md
        builder.HasIndex(r => r.Name)
            .IsUnique();
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.CategoryId);

        builder.HasOne(r => r.Category)
            .WithMany(c => c.FamilyRoles)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Tags)
            .WithMany(t => t.FamilyRoles)
            .UsingEntity<FamilyRoleTagEntity>(
                j => j.HasOne<TagEntity>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<FamilyRoleEntity>().WithMany().HasForeignKey("FamilyRoleId"),
                j =>
                {
                    j.HasKey("FamilyRoleId", "TagId");
                    j.ToTable("FamilyRoleTags");
                });
    }
}
