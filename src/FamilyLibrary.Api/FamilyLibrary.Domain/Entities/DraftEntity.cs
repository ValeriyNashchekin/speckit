using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Tracks families being prepared for publish.
/// </summary>
public class DraftEntity : BaseEntity
{
    public string FamilyName { get; private set; } = null!;
    public string FamilyUniqueId { get; private set; } = null!;
    public Guid? SelectedRoleId { get; private set; }
    public Guid? TemplateId { get; private set; }
    public DraftStatus Status { get; private set; } = DraftStatus.New;
    public DateTime LastSeen { get; private set; }
    
    // Navigation property
    public FamilyRoleEntity? SelectedRole { get; private set; }
    
    // Private constructor for EF Core
    private DraftEntity() { }
    
    public DraftEntity(string familyName, string familyUniqueId, Guid? templateId = null)
    {
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
        FamilyUniqueId = familyUniqueId ?? throw new ArgumentNullException(nameof(familyUniqueId));
        TemplateId = templateId;
        LastSeen = DateTime.UtcNow;
    }
    
    public void SetSelectedRole(Guid? roleId)
    {
        SelectedRoleId = roleId;
        if (roleId.HasValue && Status == DraftStatus.New)
        {
            Status = DraftStatus.RoleSelected;
        }
        SetUpdated();
    }
    
    public void MarkAsStamped()
    {
        if (Status == DraftStatus.RoleSelected)
        {
            Status = DraftStatus.Stamped;
        }
        SetUpdated();
    }
    
    public void MarkAsPublished()
    {
        Status = DraftStatus.Published;
        SetUpdated();
    }
    
    public void UpdateLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }
}
