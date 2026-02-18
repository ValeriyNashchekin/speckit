using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyRoleTagConfiguration : IEntityTypeConfiguration<FamilyRoleTagEntity>
{
    public void Configure(EntityTypeBuilder<FamilyRoleTagEntity> builder)
    {
        builder.ToTable("FamilyRoleTags");

        builder.HasKey(rt => new { rt.FamilyRoleId, rt.TagId });

        builder.HasOne(rt => rt.FamilyRole)
            .WithMany()
            .HasForeignKey(rt => rt.FamilyRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.Tag)
            .WithMany()
            .HasForeignKey(rt => rt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
