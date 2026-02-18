using System;

namespace FamilyLibrary.Plugin.Services;

/// <summary>
/// Represents a choice for loading a nested family during family load operation.
/// </summary>
/// <remarks>
/// When a family contains nested shared families, this record determines
/// which version of each nested family should be used during load.
/// </remarks>
public sealed class NestedLoadChoice
{
    /// <summary>
    /// Gets the name of the nested family.
    /// </summary>
    public string FamilyName { get; }

    /// <summary>
    /// Gets a value indicating whether to use the library version of the family.
    /// If true, uses the version from the project (FamilySource.Project).
    /// If false, uses the version embedded in the RFA file (FamilySource.Family).
    /// </summary>
    public bool UseLibraryVersion { get; }

    /// <summary>
    /// Gets the target version of the family to load from library.
    /// Null means use the latest version.
    /// </summary>
    public int? TargetVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedLoadChoice"/> class.
    /// </summary>
    /// <param name="familyName">The name of the nested family.</param>
    /// <param name="useLibraryVersion">Whether to use library version (true) or RFA version (false).</param>
    /// <param name="targetVersion">Optional target version for library family.</param>
    public NestedLoadChoice(string familyName, bool useLibraryVersion, int? targetVersion = null)
    {
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
        UseLibraryVersion = useLibraryVersion;
        TargetVersion = targetVersion;
    }
}
