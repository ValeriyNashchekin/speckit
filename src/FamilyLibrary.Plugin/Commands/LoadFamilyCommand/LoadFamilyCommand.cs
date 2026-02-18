using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand;

/// <summary>
/// Command for loading families from the library into the current document.
/// Typically triggered from WebView2 via RevitBridge.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class LoadFamilyCommand : ExternalCommand
{
    public override void Execute()
    {
        try
        {
            var document = Context.ActiveDocument;

            if (document == null)
            {
                TaskDialog.Show("Load Family", "No active document found.");
                return;
            }

            if (document.IsFamilyDocument)
            {
                TaskDialog.Show("Load Family", "Cannot load families into a family document.");
                return;
            }

            // This command is typically called from WebView2 with family ID
            // The actual load is triggered via LoadFamilyFromLibrary method
            TaskDialog.Show("Load Family", "Select a family from the library to load.");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Load Family Error", ex.Message);
        }
    }

    /// <summary>
    /// Loads a family from the library by ID.
    /// Called from RevitBridge when user clicks Load in WebView2.
    /// </summary>
    public static LoadFamilyResult LoadFamilyFromLibrary(Document document, Guid familyId, int? version = null)
    {
        if (document == null)
        {
            return new LoadFamilyResult
            {
                Success = false,
                Message = "No active document"
            };
        }

        try
        {
            var downloader = new FamilyDownloader();
            var downloadResult = downloader.DownloadFamilyAsync(familyId, version).Result;

            var loader = new FamilyLoader();
            var loadResult = loader.LoadFamily(document, downloadResult.LocalPath);

            return loadResult;
        }
        catch (Exception ex)
        {
            return new LoadFamilyResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
