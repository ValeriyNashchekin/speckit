using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FamilyLibrary.Plugin.Core.Interfaces;

namespace FamilyLibrary.Plugin.Infrastructure.WebView2;

/// <summary>
/// Bridge for WebView2 communication with Angular frontend.
/// Implements event protocol from contracts/webview-events.md
/// </summary>
public class RevitBridge : IWebViewBridge
{
    private readonly WebViewHost _webViewHost;
    private readonly Dictionary<string, List<Action<object>>> _handlers = new();
    private bool _isInitialized;

    public RevitBridge(WebViewHost webViewHost)
    {
        _webViewHost = webViewHost;
    }

    public void Initialize()
    {
        if (_isInitialized) return;

        _webViewHost.Initialize();
        
        // Register handler for web messages
        _webViewHost.OnEvent<WebViewMessage>("webmessage", OnWebMessage);

        _isInitialized = true;
    }

    public void Navigate(string url)
    {
        _webViewHost.Navigate(url);
    }

    public void SendEvent<T>(string eventType, T payload)
    {
        _webViewHost.SendEvent(eventType, payload);
    }

    public void OnEvent<T>(string eventType, Action<T> handler)
    {
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Action<object>>();
        }

        _handlers[eventType].Add(obj =>
        {
            try
            {
                if (obj is JObject jObj)
                {
                    var typed = jObj.ToObject<T>();
                    if (typed != null) handler(typed);
                }
                else if (obj is T typed)
                {
                    handler(typed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Handler error for {eventType}: {ex.Message}");
            }
        });
    }

    private void OnWebMessage(WebViewMessage message)
    {
        if (message?.Type == null) return;

        System.Diagnostics.Debug.WriteLine($"Received event: {message.Type}");

        if (_handlers.TryGetValue(message.Type, out var handlers))
        {
            foreach (var handler in handlers)
            {
                handler(message.Payload);
            }
        }

        // Handle built-in events
        HandleBuiltInEvents(message);
    }

    private void HandleBuiltInEvents(WebViewMessage message)
    {
        switch (message.Type)
        {
            case "ui:ready":
                // Send revit:ready response
                SendRevitReady();
                break;

            case "ui:scan-families":
                // TODO: Trigger document scan
                // Will be implemented in US3
                break;

            case "ui:stamp":
                // TODO: Handle stamp request
                // Will be implemented in US3
                break;

            case "ui:publish":
                // TODO: Handle publish request
                // Will be implemented in US3
                break;

            case "ui:load-family":
                // TODO: Handle load request
                // Will be implemented in US6
                break;

            case "ui:log":
                var logPayload = message.Payload as JObject;
                var level = logPayload?["level"]?.ToString() ?? "info";
                var logMessage = logPayload?["message"]?.ToString() ?? "";
                System.Diagnostics.Debug.WriteLine($"[UI {level.ToUpper()}] {logMessage}");
                break;
        }
    }

    private void SendRevitReady()
    {
        var payload = new
        {
            version = GetRevitVersion(),
            pluginVersion = "1.0.0",
            documentType = "None", // Will be updated by actual implementation
            documentPath = (string?)null
        };

        SendEvent("revit:ready", payload);
    }

    private static string GetRevitVersion()
    {
        // Return Revit version based on compile-time constant
#if REVIT2020
        return "2020";
#elif REVIT2021
        return "2021";
#elif REVIT2022
        return "2022";
#elif REVIT2023
        return "2023";
#elif REVIT2024
        return "2024";
#elif REVIT2025
        return "2025";
#elif REVIT2026
        return "2026";
#else
        return "Unknown";
#endif
    }
}
