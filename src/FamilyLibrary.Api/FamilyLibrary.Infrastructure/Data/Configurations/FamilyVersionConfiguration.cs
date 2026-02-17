using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Infrastructure.Data.Configurations;

public class FamilyVersionConfiguration : IEntityTypeConfiguration<FamilyVersionEntity>
{
    public void Configure(EntityTypeBuilder<FamilyVersionEntity> builder)
    {
        builder.ToTable("FamilyVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.FamilyId)
            .IsRequired();

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.Hash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(v => v.PreviousHash)
            .HasMaxLength(64);

        builder.Property(v => v.BlobPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.CatalogBlobPath)
            .HasMaxLength(500);

        builder.Property(v => v.CatalogHash)
            .HasMaxLength(64);

        builder.Property(v => v.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.OriginalCatalogName)
            .HasMaxLength(255);

        builder.Property(v => v.CommitMessage)
            .HasMaxLength(1000);

        builder.Property(v => v.SnapshotJson)
            .IsRequired();

        builder.Property(v => v.PublishedAt)
            .IsRequired();

        builder.Property(v => v.PublishedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(v => new { v.FamilyId, v.Version })
            .IsUnique();

        builder.HasIndex(v => v.Hash);
    }
}
