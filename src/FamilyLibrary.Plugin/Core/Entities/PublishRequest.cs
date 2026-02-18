namespace FamilyLibrary.Plugin.Core.Entities;

/// <summary>
/// Request to publish a family to the library.
/// </summary>
public class PublishRequest
{
    public string FamilyUniqueId { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string? CommitMessage { get; set; }
    public string? CatalogFilePath { get; set; }
}
