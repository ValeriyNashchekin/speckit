using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Organizational grouping for roles.
/// </summary>
public class CategoryEntity : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    
    // Navigation properties
    private readonly List<FamilyRoleEntity> _familyRoles = [];
    public IReadOnlyCollection<FamilyRoleEntity> FamilyRoles => _familyRoles.AsReadOnly();
    
    // Private constructor for EF Core
    private CategoryEntity() { }
    
    public CategoryEntity(string name, string? description = null, int sortOrder = 0)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        SortOrder = sortOrder;
    }
    
    public void Update(string name, string? description, int sortOrder)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        SortOrder = sortOrder;
        SetUpdated();
    }
}
