namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for Family entity.
/// </summary>
public record FamilyDto
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required string FamilyName { get; init; }
    public int CurrentVersion { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for Family with versions.
/// </summary>
public record FamilyDetailDto : FamilyDto
{
    public required List<FamilyVersionDto> Versions { get; init; }
}

/// <summary>
/// DTO for creating a new Family.
/// </summary>
public record CreateFamilyDto
{
    public Guid RoleId { get; init; }
    public required string FamilyName { get; init; }
    public string? OriginalFileName { get; init; }
    public string? TypeCatalogFileName { get; init; }
    public string? TypeCatalogHash { get; init; }
}

/// <summary>
/// DTO for updating a Family.
/// </summary>
public record UpdateFamilyDto
{
    public Guid? RoleId { get; init; }
    public string? FamilyName { get; init; }
    public string? OriginalFileName { get; init; }
}

/// <summary>
/// DTO for family status check result.
/// </summary>
public record FamilyStatusDto
{
    public required string Hash { get; init; }
    public bool Exists { get; init; }
    public Guid? FamilyId { get; init; }
    public string? FamilyName { get; init; }
    public int? CurrentVersion { get; init; }
}

/// <summary>
/// DTO for family download URL response.
/// </summary>
public record FamilyDownloadDto
{
    /// <summary>
    /// SAS URL for downloading the family file.
    /// </summary>
    public required string DownloadUrl { get; init; }

    /// <summary>
    /// Original file name of the family.
    /// </summary>
    public required string OriginalFileName { get; init; }

    /// <summary>
    /// Content hash of the family file.
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// Version number of the family.
    /// </summary>
    public int Version { get; init; }
}
