using FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Views;

/// <summary>
/// Code-behind for LibraryQueueView.
/// DataContext is set in constructor, following MVVM pattern.
/// </summary>
public sealed partial class LibraryQueueView
{
    public LibraryQueueView(LibraryQueueViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}