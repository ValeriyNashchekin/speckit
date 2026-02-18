using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class RecognitionRuleConfiguration : IEntityTypeConfiguration<RecognitionRuleEntity>
{
    public void Configure(EntityTypeBuilder<RecognitionRuleEntity> builder)
    {
        builder.ToTable("RecognitionRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoleId)
            .IsRequired();

        builder.Property(r => r.RootNode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Formula)
            .IsRequired()
            .HasMaxLength(500);

        // Performance indexes per data-model.md
        builder.HasIndex(r => r.RoleId)
            .IsUnique();
        // Note: IsActive property does not exist on RecognitionRuleEntity, skipping IsActive index

        builder.HasOne(r => r.Role)
            .WithOne(role => role.RecognitionRule)
            .HasForeignKey<RecognitionRuleEntity>(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
