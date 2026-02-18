namespace FamilyLibrary.Plugin.Core.Interfaces;

/// <summary>
/// Service for computing content hash of RFA files.
/// Implements PartAtom normalization per research.md R1-R2.
/// </summary>
public interface IContentHashService
{
    /// <summary>
    /// Compute content hash for an RFA file.
    /// Uses PartAtom normalization (MVP implementation).
    /// </summary>
    string ComputeHash(string rfaFilePath);

    /// <summary>
    /// Compute content hash from file bytes.
    /// </summary>
    string ComputeHash(byte[] fileContent);
}
