using System.Windows;
using FamilyLibrary.Plugin.Commands.OpenLibraryCommand.ViewModels;

namespace FamilyLibrary.Plugin.Commands.OpenLibraryCommand.Views;

/// <summary>
/// Code-behind for OpenLibraryView.
/// Hosts WebView2 control for Angular UI.
/// </summary>
public sealed partial class OpenLibraryView
{
    private readonly OpenLibraryViewModel _viewModel;

    public OpenLibraryView(OpenLibraryViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initialize WebView2
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();
            
            // Initialize bridge for JS interop
            _viewModel.Initialize("http://localhost:4200/library");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebView2 initialization failed: {ex.Message}");
            MessageBox.Show(
                $"Failed to initialize WebView2. Please ensure WebView2 runtime is installed.\n\nError: {ex.Message}",
                "WebView2 Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnClosed(object sender, EventArgs e)
    {
        _viewModel.Cleanup();
    }
}
