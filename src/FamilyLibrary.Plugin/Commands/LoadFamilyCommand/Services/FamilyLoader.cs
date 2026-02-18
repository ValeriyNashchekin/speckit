using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Services;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;

/// <summary>
/// Loads family files into Revit document.
/// Handles family conflicts and existing families.
/// </summary>
public class FamilyLoader
{
    /// <summary>
    /// Loads family file into document.
    /// Returns load result with family reference if successful.
    /// </summary>
    public LoadFamilyResult LoadFamily(Document document, string filePath)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Family file not found", filePath);
        }

        Family family;
        bool wasLoaded;

        using var transaction = new Transaction(document, "Load Family");
        transaction.Start();

        try
        {
            var loadOptions = new SimpleFamilyLoadOptions();
            wasLoaded = document.LoadFamily(filePath, loadOptions, out family);

            if (wasLoaded)
            {
                transaction.Commit();
                return new LoadFamilyResult
                {
                    Success = true,
                    Family = family,
                    WasNewlyLoaded = true,
                    Message = "Family loaded successfully"
                };
            }

            // Family already exists - rollback and get existing
            transaction.RollBack();

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            family = FindExistingFamily(document, fileName);

            if (family != null)
            {
                return new LoadFamilyResult
                {
                    Success = true,
                    Family = family,
                    WasNewlyLoaded = false,
                    Message = "Family already exists in document"
                };
            }

            return new LoadFamilyResult
            {
                Success = false,
                Family = null,
                WasNewlyLoaded = false,
                Message = "Failed to load family"
            };
        }
        catch
        {
            transaction.RollBack();
            throw;
        }
    }

    private static Family FindExistingFamily(Document document, string familyName)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Result of family load operation.
/// </summary>
public class LoadFamilyResult
{
    public bool Success { get; set; }
    public Family? Family { get; set; }
    public bool WasNewlyLoaded { get; set; }
    public string Message { get; set; } = string.Empty;
}
