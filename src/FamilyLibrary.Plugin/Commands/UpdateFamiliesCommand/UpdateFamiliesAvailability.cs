using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using JetBrains.Annotations;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand
{
    /// <summary>
    /// Availability check for UpdateFamiliesCommand.
    /// Command is available when a project document is active.
    /// </summary>
    [UsedImplicitly]
    public class UpdateFamiliesAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            if (applicationData == null) return false;

            var document = applicationData.ActiveUIDocument?.Document;
            if (document == null) return false;

            // Available for project documents, not family documents
            return document.IsFamilyDocument == false;
        }
    }
}
