namespace FamilyLibrary.Domain.Enums;

/// <summary>
/// Category of change detected between family versions.
/// </summary>
public enum ChangeCategory
{
    Name = 0,
    Category = 1,
    Types = 2,
    Parameters = 3,
    Geometry = 4,
    Txt = 5
}
