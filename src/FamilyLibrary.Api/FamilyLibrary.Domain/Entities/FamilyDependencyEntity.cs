namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Represents a nested family dependency detected within a parent family.
/// Tracks whether the nested family is shared and if it exists in the library.
/// </summary>
public class FamilyDependencyEntity : BaseEntity
{
    /// <summary>
    /// The parent family that contains this nested family.
    /// </summary>
    public Guid ParentFamilyId { get; private set; }
    
    /// <summary>
    /// Name of the nested family as it appears in the RFA file.
    /// </summary>
    public string NestedFamilyName { get; private set; } = null!;
    
    /// <summary>
    /// Role name if the nested family is Shared and has been stamped with a role.
    /// Null if not shared or not yet stamped.
    /// </summary>
    public string? NestedRoleName { get; private set; }
    
    /// <summary>
    /// Indicates whether the nested family is a Shared family.
    /// </summary>
    public bool IsShared { get; private set; }
    
    /// <summary>
    /// Indicates whether the nested family has been published to the library.
    /// </summary>
    public bool InLibrary { get; private set; }
    
    /// <summary>
    /// Current version number in the library, if InLibrary is true.
    /// Null if not yet published.
    /// </summary>
    public int? LibraryVersion { get; private set; }
    
    /// <summary>
    /// Timestamp when this dependency was detected during family processing.
    /// </summary>
    public DateTime DetectedAt { get; private set; }
    
    // Navigation property
    public FamilyEntity ParentFamily { get; private set; } = null!;
    
    // Private constructor for EF Core
    private FamilyDependencyEntity() { }
    
    /// <summary>
    /// Creates a new family dependency record.
    /// </summary>
    /// <param name="parentFamilyId">ID of the parent family containing the nested family</param>
    /// <param name="nestedFamilyName">Name of the nested family from the RFA file</param>
    /// <param name="isShared">Whether the nested family is shared</param>
    /// <param name="nestedRoleName">Role name if shared and stamped (optional)</param>
    /// <param name="inLibrary">Whether the nested family exists in the library</param>
    /// <param name="libraryVersion">Library version if published (optional)</param>
    public FamilyDependencyEntity(
        Guid parentFamilyId,
        string nestedFamilyName,
        bool isShared,
        string? nestedRoleName = null,
        bool inLibrary = false,
        int? libraryVersion = null)
    {
        ParentFamilyId = parentFamilyId;
        NestedFamilyName = nestedFamilyName ?? throw new ArgumentNullException(nameof(nestedFamilyName));
        IsShared = isShared;
        NestedRoleName = nestedRoleName;
        InLibrary = inLibrary;
        LibraryVersion = libraryVersion;
        DetectedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates the dependency information when re-scanning or after library changes.
    /// </summary>
    /// <param name="nestedRoleName">Updated role name if shared and stamped</param>
    /// <param name="inLibrary">Updated library status</param>
    /// <param name="libraryVersion">Updated library version</param>
    public void Update(
        string? nestedRoleName,
        bool inLibrary,
        int? libraryVersion)
    {
        NestedRoleName = nestedRoleName;
        InLibrary = inLibrary;
        LibraryVersion = libraryVersion;
        SetUpdated();
    }
    
    /// <summary>
    /// Marks this dependency as published to the library with the specified version.
    /// </summary>
    /// <param name="version">The version number assigned in the library</param>
    public void MarkAsPublished(int version)
    {
        InLibrary = true;
        LibraryVersion = version;
        SetUpdated();
    }
}
