using Autodesk.Revit.Attributes;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.Commands.OpenLibraryCommand.ViewModels;
using FamilyLibrary.Plugin.Commands.OpenLibraryCommand.Views;

namespace FamilyLibrary.Plugin.Commands.OpenLibraryCommand;

/// <summary>
/// Command to open the family library in a WebView2 browser inside Revit.
/// Displays Angular UI via WebView2 for family management.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.ReadOnly)]
public class OpenLibraryCommand : ExternalCommand
{
    public override void Execute()
    {
        var viewModel = new OpenLibraryViewModel();
        var view = new OpenLibraryView(viewModel);
        view.ShowDialog();
    }
}
