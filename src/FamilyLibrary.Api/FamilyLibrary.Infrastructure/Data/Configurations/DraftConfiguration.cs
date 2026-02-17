using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class DraftConfiguration : IEntityTypeConfiguration<DraftEntity>
{
    public void Configure(EntityTypeBuilder<DraftEntity> builder)
    {
        builder.ToTable("Drafts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FamilyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.FamilyUniqueId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.SelectedRoleId);

        builder.Property(d => d.TemplateId);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.LastSeen)
            .IsRequired();

        builder.HasIndex(d => d.FamilyUniqueId)
            .IsUnique();

        builder.HasIndex(d => d.Status);

        builder.HasOne(d => d.SelectedRole)
            .WithMany()
            .HasForeignKey(d => d.SelectedRoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
