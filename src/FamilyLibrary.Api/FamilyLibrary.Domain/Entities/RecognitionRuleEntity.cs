using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Defines how to match family names to roles.
/// </summary>
public class RecognitionRuleEntity : BaseEntity
{
    public Guid RoleId { get; private set; }
    public string RootNode { get; private set; } = null!;
    public string Formula { get; private set; } = null!;
    
    // Navigation property
    public FamilyRoleEntity Role { get; private set; } = null!;
    
    // Private constructor for EF Core
    private RecognitionRuleEntity() { }
    
    public RecognitionRuleEntity(Guid roleId, string rootNode, string formula)
    {
        RoleId = roleId;
        RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        Formula = formula ?? throw new ArgumentNullException(nameof(formula));
    }
    
    public void Update(string rootNode, string formula)
    {
        RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        Formula = formula ?? throw new ArgumentNullException(nameof(formula));
        SetUpdated();
    }
}
