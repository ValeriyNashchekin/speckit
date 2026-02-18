using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Views;
using JetBrains.Annotations;

namespace FamilyLibrary.Plugin.Commands.PublishFromEditorCommand;

/// <summary>
/// Command to publish current family from Family Editor.
/// Only available when document is a FamilyDocument.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.ReadOnly)]
public class PublishFromEditorCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiApp = commandData.Application;
            var document = uiApp.ActiveUIDocument.Document;

            // Verify we're in Family Editor
            if (!document.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "This command is only available in Family Editor.");
                return Result.Failed;
            }

            // Get current family info
            var family = document.OwnerFamily;
            if (family == null)
            {
                TaskDialog.Show("Error", "Could not access current family.");
                return Result.Failed;
            }

            // Show queue view in Family Editor mode
            var viewModel = new LibraryQueueViewModel(isFamilyEditorMode: true, currentFamily: family);
            viewModel.SetDocument(document);

            var view = new LibraryQueueView(viewModel);
            view.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}
