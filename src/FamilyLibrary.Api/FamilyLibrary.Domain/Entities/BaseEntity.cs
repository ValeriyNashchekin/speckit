namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Base entity with common properties for all entities.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    
    protected BaseEntity() { }
    
    protected void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
