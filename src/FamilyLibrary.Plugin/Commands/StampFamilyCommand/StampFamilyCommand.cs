using Autodesk.Revit.Attributes;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Views;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand;

/// <summary>
/// Command for stamping and publishing families to the library.
/// Opens LibraryQueueView for user interaction.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StampFamilyCommand : ExternalCommand
{
    public override void Execute()
    {
        var viewModel = new LibraryQueueViewModel();
        viewModel.SetDocument(Context.ActiveDocument);

        var view = new LibraryQueueView(viewModel);
        view.ShowDialog();
    }
}
