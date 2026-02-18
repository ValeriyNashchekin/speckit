namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Labels for filtering and categorization.
/// </summary>
public class TagEntity : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Color { get; private set; }
    
    // Navigation properties (many-to-many with FamilyRole)
    private readonly List<FamilyRoleEntity> _familyRoles = [];
    public IReadOnlyCollection<FamilyRoleEntity> FamilyRoles => _familyRoles.AsReadOnly();
    
    // Private constructor for EF Core
    private TagEntity() { }
    
    public TagEntity(string name, string? color = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color;
    }
    
    public void Update(string name, string? color)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Color = color;
        SetUpdated();
    }
}
