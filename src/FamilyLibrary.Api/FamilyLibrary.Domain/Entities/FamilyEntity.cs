namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Represents a loadable family in the library.
/// </summary>
public class FamilyEntity : BaseEntity
{
    public Guid RoleId { get; private set; }
    public string FamilyName { get; private set; } = null!;
    public int CurrentVersion { get; private set; } = 1;
    
    // Navigation properties
    public FamilyRoleEntity Role { get; private set; } = null!;
    private readonly List<FamilyVersionEntity> _versions = [];
    public IReadOnlyCollection<FamilyVersionEntity> Versions => _versions.AsReadOnly();
    
    // Private constructor for EF Core
    private FamilyEntity() { }
    
    public FamilyEntity(Guid roleId, string familyName)
    {
        RoleId = roleId;
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
    }
    
    public void IncrementVersion()
    {
        CurrentVersion++;
        SetUpdated();
    }
    
    public void AddVersion(FamilyVersionEntity version)
    {
        _versions.Add(version);
    }
}
