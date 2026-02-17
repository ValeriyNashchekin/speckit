namespace FamilyLibrary.Plugin.Core.Entities;

/// <summary>
/// Extensible Storage stamp data attached to family elements.
/// Matches the ES schema definition.
/// </summary>
public class EsStampData
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public DateTime StampedAt { get; set; }
    public string StampedBy { get; set; } = string.Empty;

    public bool IsValid => RoleId != Guid.Empty && !string.IsNullOrEmpty(RoleName);
}
