namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Server-side fallback for ES data recovery.
/// When ES is lost, lookup by (FamilyName, ProjectId) to recover role association.
/// </summary>
public class FamilyNameMappingEntity : BaseEntity
{
    public string FamilyName { get; private set; } = null!;
    public string RoleName { get; private set; } = null!;
    public Guid ProjectId { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    
    // Private constructor for EF Core
    private FamilyNameMappingEntity() { }
    
    public FamilyNameMappingEntity(string familyName, string roleName, Guid projectId)
    {
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
        RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
        ProjectId = projectId;
        LastSeenAt = DateTime.UtcNow;
    }
    
    public void UpdateLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
    }
}
