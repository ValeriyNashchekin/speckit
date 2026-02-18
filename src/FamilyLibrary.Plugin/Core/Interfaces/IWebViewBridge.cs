namespace FamilyLibrary.Plugin.Core.Interfaces;

/// <summary>
/// Bridge for WebView2 communication with Angular frontend.
/// </summary>
public interface IWebViewBridge
{
    /// <summary>
    /// Send event to frontend.
    /// </summary>
    void SendEvent<T>(string eventType, T payload);

    /// <summary>
    /// Register handler for frontend events.
    /// </summary>
    void OnEvent<T>(string eventType, Action<T> handler);

    /// <summary>
    /// Initialize the bridge.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Navigate to URL.
    /// </summary>
    void Navigate(string url);
}
