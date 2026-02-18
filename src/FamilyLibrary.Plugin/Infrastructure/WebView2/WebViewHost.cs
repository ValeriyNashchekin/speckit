using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using FamilyLibrary.Plugin.Core.Interfaces;
using WpfWebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace FamilyLibrary.Plugin.Infrastructure.WebView2;

/// <summary>
/// WPF control that hosts WebView2 for Angular frontend.
/// URL: localhost:4200 (development) or production URL.
/// </summary>
public class WebViewHost : System.Windows.Controls.Control, IWebViewBridge, INotifyPropertyChanged
{
    private WpfWebView2? _webView;
    private bool _isInitialized;
    private readonly Dictionary<string, List<Action<object>>> _handlers = new();

    // Default URL for development
    private const string DefaultUrl = "http://localhost:4200";

    public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
        nameof(Url), typeof(string), typeof(WebViewHost),
        new PropertyMetadata(DefaultUrl, OnUrlChanged));

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public WebViewHost()
    {
        // Initialize WebView2 control
        _webView = new WpfWebView2
        {
            DefaultBackgroundColor = System.Drawing.Color.White
        };

        // Set as visual child
        AddVisualChild(_webView);
    }

    public async void Initialize()
    {
        if (_isInitialized || _webView == null) return;

        try
        {
            // Create WebView2 environment
            var env = await CoreWebView2Environment.CreateAsync();
            await _webView.EnsureCoreWebView2Async(env);

            // Subscribe to messages from Angular
            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            // Navigate to URL
            _webView.Source = new Uri(Url);

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"WebView2 init failed: {ex.Message}");
        }
    }

    public void Navigate(string url)
    {
        if (_webView?.CoreWebView2 != null)
        {
            _webView.CoreWebView2.Navigate(url);
        }
    }

    public void SendEvent<T>(string eventType, T payload)
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null) return;

        var eventObj = new
        {
            type = eventType,
            payload,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(eventObj);

        // Dispatch custom event to Angular
        var script = $"window.dispatchEvent(new CustomEvent('revit-message', {{ detail: {json} }}))";
        _webView.CoreWebView2.ExecuteScriptAsync(script);
    }

    public void OnEvent<T>(string eventType, Action<T> handler)
    {
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Action<object>>();
        }

        _handlers[eventType].Add(obj =>
        {
            if (obj is T typed)
            {
                handler(typed);
            }
        });
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            var json = args.WebMessageAsJson;
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<WebViewMessage>(json);

            if (message != null && _handlers.TryGetValue(message.Type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(message.Payload);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Message handling error: {ex.Message}");
        }
    }

    private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WebViewHost host && host._isInitialized)
        {
            host.Navigate((string)e.NewValue);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override int VisualChildrenCount => _webView != null ? 1 : 0;

    protected override Visual GetVisualChild(int index)
    {
        if (index != 0 || _webView == null)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _webView;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        _webView?.Arrange(new Rect(arrangeBounds));
        return arrangeBounds;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _webView?.Measure(constraint);
        return base.MeasureOverride(constraint);
    }
}

/// <summary>
/// Represents a message from WebView2.
/// </summary>
public class WebViewMessage
{
    [Newtonsoft.Json.JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonProperty("payload")]
    public object Payload { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}
