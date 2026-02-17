using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.ViewModels;
using FamilyLibrary.Plugin.Views;

namespace FamilyLibrary.Plugin.Commands;

/// <summary>
///     External command entry point.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        var viewModel = new FamilyLibrary_PluginViewModel();
        var view = new FamilyLibrary_PluginView(viewModel);
        view.ShowDialog();
    }
}