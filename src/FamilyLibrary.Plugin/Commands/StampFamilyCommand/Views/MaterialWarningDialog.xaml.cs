using FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Views;

/// <summary>
/// Code-behind for MaterialWarningDialog.
/// DataContext is set in constructor, following MVVM pattern.
/// </summary>
public sealed partial class MaterialWarningDialog
{
    public MaterialWarningDialog(MaterialWarningViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
