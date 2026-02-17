namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Represents a specific version of a family.
/// </summary>
public class FamilyVersionEntity : BaseEntity
{
    public Guid FamilyId { get; private set; }
    public int Version { get; private set; }
    public string Hash { get; private set; } = null!;
    public string? PreviousHash { get; private set; }
    public string BlobPath { get; private set; } = null!;
    public string? CatalogBlobPath { get; private set; }
    public string OriginalFileName { get; private set; } = null!;
    public string? OriginalCatalogName { get; private set; }
    public string? CommitMessage { get; private set; }
    public string SnapshotJson { get; private set; } = null!;
    public DateTime PublishedAt { get; private set; }
    public string PublishedBy { get; private set; } = null!;
    
    // Navigation property
    public FamilyEntity Family { get; private set; } = null!;
    
    // Private constructor for EF Core
    private FamilyVersionEntity() { }
    
    public FamilyVersionEntity(
        Guid familyId,
        int version,
        string hash,
        string blobPath,
        string originalFileName,
        string snapshotJson,
        string publishedBy,
        string? previousHash = null,
        string? catalogBlobPath = null,
        string? originalCatalogName = null,
        string? commitMessage = null)
    {
        FamilyId = familyId;
        Version = version;
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        PreviousHash = previousHash;
        BlobPath = blobPath ?? throw new ArgumentNullException(nameof(blobPath));
        CatalogBlobPath = catalogBlobPath;
        OriginalFileName = originalFileName ?? throw new ArgumentNullException(nameof(originalFileName));
        OriginalCatalogName = originalCatalogName;
        CommitMessage = commitMessage;
        SnapshotJson = snapshotJson ?? throw new ArgumentNullException(nameof(snapshotJson));
        PublishedAt = DateTime.UtcNow;
        PublishedBy = publishedBy ?? throw new ArgumentNullException(nameof(publishedBy));
    }
}
