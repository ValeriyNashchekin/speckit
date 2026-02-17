namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for FamilyVersion entity.
/// </summary>
public record FamilyVersionDto
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; init; }
    public int Version { get; init; }
    public required string Hash { get; init; }
    public string? PreviousHash { get; init; }
    public required string BlobPath { get; init; }
    public string? CatalogBlobPath { get; init; }
    public string? CatalogHash { get; init; }
    public required string OriginalFileName { get; init; }
    public string? OriginalCatalogName { get; init; }
    public string? CommitMessage { get; init; }
    public DateTime PublishedAt { get; init; }
    public required string PublishedBy { get; init; }
}

/// <summary>
/// DTO for publishing a new Family version.
/// </summary>
public record PublishFamilyVersionDto
{
    public required string Hash { get; init; }
    public required string BlobPath { get; init; }
    public string? CatalogBlobPath { get; init; }
    public required string OriginalFileName { get; init; }
    public string? OriginalCatalogName { get; init; }
    public string? CommitMessage { get; init; }
    public required string SnapshotJson { get; init; }
    public required string PublishedBy { get; init; }
}
