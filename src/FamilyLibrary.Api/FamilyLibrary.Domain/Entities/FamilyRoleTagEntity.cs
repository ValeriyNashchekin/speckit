namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Junction table for many-to-many relationship between FamilyRole and Tag.
/// </summary>
public class FamilyRoleTagEntity
{
    public Guid FamilyRoleId { get; set; }
    public Guid TagId { get; set; }
    
    public FamilyRoleEntity FamilyRole { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
