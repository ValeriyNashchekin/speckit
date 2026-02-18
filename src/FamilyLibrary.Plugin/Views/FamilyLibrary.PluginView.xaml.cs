using FamilyLibrary.Plugin.ViewModels;

namespace FamilyLibrary.Plugin.Views;

public sealed partial class FamilyLibrary_PluginView
{
    public FamilyLibrary_PluginView(FamilyLibrary_PluginViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}