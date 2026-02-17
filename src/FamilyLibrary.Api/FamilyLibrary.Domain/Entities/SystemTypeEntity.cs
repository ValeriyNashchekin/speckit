using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Represents a system family type (WallType, FloorType, etc.).
/// </summary>
public class SystemTypeEntity : BaseEntity
{
    public Guid RoleId { get; private set; }
    public string TypeName { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string SystemFamily { get; private set; } = null!;
    public SystemFamilyGroup Group { get; private set; }
    public string Json { get; private set; } = null!;
    public int CurrentVersion { get; private set; } = 1;
    public string Hash { get; private set; } = null!;
    
    // Navigation property
    public FamilyRoleEntity Role { get; private set; } = null!;
    
    // Private constructor for EF Core
    private SystemTypeEntity() { }
    
    public SystemTypeEntity(
        Guid roleId,
        string typeName,
        string category,
        string systemFamily,
        SystemFamilyGroup group,
        string json,
        string hash)
    {
        RoleId = roleId;
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        SystemFamily = systemFamily ?? throw new ArgumentNullException(nameof(systemFamily));
        Group = group;
        Json = json ?? throw new ArgumentNullException(nameof(json));
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));
    }
    
    public void Update(string json, string hash)
    {
        Json = json ?? throw new ArgumentNullException(nameof(json));
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        CurrentVersion++;
        SetUpdated();
    }
}
