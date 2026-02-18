using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using JetBrains.Annotations;

namespace FamilyLibrary.Plugin.Commands.PublishFromEditorCommand;

/// <summary>
/// Availability check - only available in Family Editor.
/// </summary>
[UsedImplicitly]
public class PublishFromEditorAvailability : IExternalCommandAvailability
{
    public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        if (applicationData == null) return false;

        var document = applicationData.ActiveUIDocument?.Document;
        if (document == null) return false;

        // Available only for family documents
        return document.IsFamilyDocument;
    }
}
