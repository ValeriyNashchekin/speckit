using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.ValueObjects;

namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Represents a functional role for a family (immutable name after creation).
/// </summary>
public class FamilyRoleEntity : BaseEntity
{
    public string Name { get; private set; } = null!;
    public RoleType Type { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    
    // Navigation properties
    public CategoryEntity? Category { get; private set; }
    private readonly List<TagEntity> _tags = [];
    public IReadOnlyCollection<TagEntity> Tags => _tags.AsReadOnly();
    public RecognitionRuleEntity? RecognitionRule { get; private set; }
    private readonly List<FamilyEntity> _families = [];
    public IReadOnlyCollection<FamilyEntity> Families => _families.AsReadOnly();
    private readonly List<SystemTypeEntity> _systemTypes = [];
    public IReadOnlyCollection<SystemTypeEntity> SystemTypes => _systemTypes.AsReadOnly();
    
    // Private constructor for EF Core
    private FamilyRoleEntity() { }
    
    public FamilyRoleEntity(string name, RoleType type, string? description = null, Guid? categoryId = null)
    {
        ValidateName(name);
        Name = name;
        Type = type;
        Description = description;
        CategoryId = categoryId;
    }
    
    public void Update(string? description, Guid? categoryId)
    {
        Description = description;
        CategoryId = categoryId;
        SetUpdated();
    }
    
    // Name is read-only after creation - no setter
    
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9_]*$"))
            throw new ArgumentException("Name must start with a letter and contain only letters, numbers, and underscores.", nameof(name));
    }
    
    public void AddTag(TagEntity tag)
    {
        if (!_tags.Contains(tag))
        {
            _tags.Add(tag);
        }
    }
    
    public void RemoveTag(TagEntity tag)
    {
        _tags.Remove(tag);
    }
    
    public void SetRecognitionRule(RecognitionRuleEntity rule)
    {
        RecognitionRule = rule;
        SetUpdated();
    }
}
