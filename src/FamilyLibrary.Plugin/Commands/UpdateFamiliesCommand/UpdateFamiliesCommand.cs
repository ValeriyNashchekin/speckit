using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Views;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand
{
    /// <summary>
    /// Command for updating families from the library.
    /// Opens scanner window for project analysis.
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class UpdateFamiliesCommand : ExternalCommand
    {
        public override void Execute()
        {
            var document = Context.ActiveDocument;

            if (document == null)
            {
                TaskDialog.Show("Update Families", "No active document found.");
                return;
            }

            if (document.IsFamilyDocument)
            {
                TaskDialog.Show("Update Families", "This command is not available in family editor.");
                return;
            }

            // Open scanner window with WebView2
            var window = new ScannerWindow(Context.UiApplication);
            window.Show();
        }
    }
}
