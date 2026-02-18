using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;

namespace FamilyLibrary.Plugin.Services;

/// <summary>
/// Service for loading families with nested family version control.
/// Handles two-phase loading: parent family first, then nested families from library if needed.
/// </summary>
public class NestedFamilyLoadService
{
    private readonly FamilyLoader _familyLoader;
    private readonly FamilyDownloader _familyDownloader;

    /// <summary>
    /// Default constructor for production use.
    /// </summary>
    public NestedFamilyLoadService()
    {
        _familyLoader = new FamilyLoader();
        _familyDownloader = new FamilyDownloader();
    }

    /// <summary>
    /// Constructor for testing with injected dependencies.
    /// </summary>
    public NestedFamilyLoadService(FamilyLoader familyLoader, FamilyDownloader familyDownloader)
    {
        _familyLoader = familyLoader ?? throw new ArgumentNullException(nameof(familyLoader));
        _familyDownloader = familyDownloader ?? throw new ArgumentNullException(nameof(familyDownloader));
    }

    /// <summary>
    /// Gets pre-load summary showing nested family versions.
    /// Compares RFA embedded versions, library versions, and project versions.
    /// </summary>
    /// <param name="rfaPath">Path to the parent family RFA file.</param>
    /// <param name="document">The Revit document to check for existing families.</param>
    /// <returns>Pre-load summary with nested family version information.</returns>
    public PreLoadSummary GetPreLoadSummary(string rfaPath, Document document)
    {
        if (string.IsNullOrEmpty(rfaPath))
        {
            throw new ArgumentException("RFA path cannot be null or empty", nameof(rfaPath));
        }

        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var fileName = Path.GetFileNameWithoutExtension(rfaPath);
        var summary = new PreLoadSummary
        {
            ParentFamilyName = fileName,
            RfaPath = rfaPath
        };

        // Get existing families in project (one-time collector - not in loop)
        var existingFamilies = GetExistingFamilies(document);

        // TODO: Implement actual nested family extraction from RFA
        // This requires opening the family document and analyzing nested families
        // For now, returns stub data for integration testing
        summary.NestedFamilies = GetStubNestedFamilies(existingFamilies);

        return summary;
    }

    /// <summary>
    /// Loads parent family with nested family version control.
    /// Two-phase loading: parent first, then override nested from library if needed.
    /// </summary>
    /// <param name="document">The Revit document to load families into.</param>
    /// <param name="rfaPath">Path to the parent family RFA file.</param>
    /// <param name="choices">Dictionary of nested family choices keyed by family name.</param>
    /// <returns>Result of the load operation.</returns>
    public NestedLoadResult LoadWithNestedChoices(
        Document document,
        string rfaPath,
        Dictionary<string, NestedLoadChoice> choices)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (string.IsNullOrEmpty(rfaPath))
        {
            throw new ArgumentException("RFA path cannot be null or empty", nameof(rfaPath));
        }

        if (!File.Exists(rfaPath))
        {
            throw new FileNotFoundException("Family file not found", rfaPath);
        }

        var result = new NestedLoadResult
        {
            ParentFamilyName = Path.GetFileNameWithoutExtension(rfaPath)
        };

        try
        {
            // Phase 1: Load parent family with nested family options
            var options = new NestedFamilyLoadOptions(choices);
            using var transaction = new Transaction(document, "Load Family with Nested Options");
            transaction.Start();

            try
            {
                var loaded = document.LoadFamily(rfaPath, options, out Family parentFamily);

                if (loaded && parentFamily != null)
                {
                    result.ParentFamily = parentFamily;
                    result.ParentLoaded = true;

                    // Phase 2: Override nested families from library if needed
                    var libraryOverrides = LoadLibraryOverrides(document, choices);
                    result.NestedOverrides = libraryOverrides;

                    transaction.Commit();
                    result.Success = true;
                    result.Message = "Family loaded successfully";
                }
                else
                {
                    transaction.RollBack();
                    result.Message = "Failed to load family";
                }
            }
            catch
            {
                transaction.RollBack();
                throw;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = string.Format("Error loading family: {0}", ex.Message);
        }

        return result;
    }

    /// <summary>
    /// Loads nested families from library that need override.
    /// </summary>
    private List<NestedOverrideResult> LoadLibraryOverrides(
        Document document,
        Dictionary<string, NestedLoadChoice> choices)
    {
        var results = new List<NestedOverrideResult>();

        // Filter choices that need library update (UseLibraryVersion = true)
        var libraryUpdates = choices
            .Where(c => c.Value.UseLibraryVersion)
            .ToList();

        foreach (var kvp in libraryUpdates)
        {
            var familyName = kvp.Key;
            var choice = kvp.Value;

            try
            {
                // Download and load from library
                // Note: FamilyDownloader requires familyId - this is a stub
                // Full implementation needs familyId lookup by name
                // For now, skip library override if no familyId available
                results.Add(new NestedOverrideResult
                {
                    FamilyName = familyName,
                    Success = false,
                    ErrorMessage = "Library family ID lookup not implemented"
                });
            }
            catch (Exception ex)
            {
                results.Add(new NestedOverrideResult
                {
                    FamilyName = familyName,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Gets existing families in the document.
    /// Uses single collector call - not in loop.
    /// </summary>
    private static Dictionary<string, Family> GetExistingFamilies(Document document)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets stub nested families for initial integration.
    /// TODO: Replace with actual RFA analysis.
    /// </summary>
    private static List<NestedFamilySummary> GetStubNestedFamilies(
        Dictionary<string, Family> existingFamilies)
    {
        // Stub implementation - returns empty list
        // Full implementation would open the RFA and extract nested family info
        return new List<NestedFamilySummary>();
    }
}

/// <summary>
/// Pre-load summary showing nested family versions before loading.
/// </summary>
public class PreLoadSummary
{
    public string ParentFamilyName { get; set; } = string.Empty;
    public string RfaPath { get; set; } = string.Empty;
    public int ParentVersion { get; set; }
    public List<NestedFamilySummary> NestedFamilies { get; set; } = new List<NestedFamilySummary>();
}

/// <summary>
/// Summary of a nested family within a parent family.
/// </summary>
public class NestedFamilySummary
{
    public string FamilyName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int? RfaVersion { get; set; }
    public int? LibraryVersion { get; set; }
    public int? ProjectVersion { get; set; }
    public NestedLoadAction RecommendedAction { get; set; }
    public bool IsShared { get; set; }
    public Guid LibraryFamilyId { get; set; }
}

/// <summary>
/// Action to take for loading a nested family.
/// </summary>
public enum NestedLoadAction
{
    LoadFromRfa = 0,
    UpdateFromLibrary = 1,
    KeepProjectVersion = 2,
    NoAction = 3
}

/// <summary>
/// Result of loading a family with nested options.
/// </summary>
public class NestedLoadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ParentFamilyName { get; set; } = string.Empty;
    public bool ParentLoaded { get; set; }
    public Family ParentFamily { get; set; }
    public List<NestedOverrideResult> NestedOverrides { get; set; } = new List<NestedOverrideResult>();
}

/// <summary>
/// Result of overriding a nested family from library.
/// </summary>
public class NestedOverrideResult
{
    public string FamilyName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int Version { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}