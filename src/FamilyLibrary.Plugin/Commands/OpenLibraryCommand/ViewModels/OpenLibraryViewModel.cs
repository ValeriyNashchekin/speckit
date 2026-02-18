using System;
using FamilyLibrary.Plugin.Infrastructure.WebView2;

namespace FamilyLibrary.Plugin.Commands.OpenLibraryCommand.ViewModels;

/// <summary>
/// ViewModel for OpenLibraryView.
/// Manages WebView2 host and bridge communication.
/// </summary>
public sealed class OpenLibraryViewModel : ObservableObject
{
    private readonly WebViewHost _webViewHost;
    private readonly RevitBridge _bridge;

    /// <summary>
    /// WebViewHost instance for Angular UI.
    /// </summary>
    public WebViewHost WebViewHost => _webViewHost;

    /// <summary>
    /// Bridge for WebView2 communication.
    /// </summary>
    public RevitBridge Bridge => _bridge;

    public OpenLibraryViewModel()
    {
        _webViewHost = new WebViewHost();
        _bridge = new RevitBridge(_webViewHost);
    }

    /// <summary>
    /// Initialize WebView2 and navigate to library URL.
    /// </summary>
    /// <param name="libraryUrl">URL to navigate to (defaults to localhost).</param>
    public void Initialize(string libraryUrl = "http://localhost:4200/library")
    {
        _bridge.Initialize();
        _bridge.Navigate(libraryUrl);
    }

    /// <summary>
    /// Clean up resources.
    /// </summary>
    public void Cleanup()
    {
        // WebView2 cleanup is handled by the view
    }
}
