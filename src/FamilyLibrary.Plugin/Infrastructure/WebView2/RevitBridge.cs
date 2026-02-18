using System.Text;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand;
using FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services;
using FamilyLibrary.Plugin.Core.Enums;
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
    private Document? _activeDocument;

    public RevitBridge(WebViewHost webViewHost)
    {
        _webViewHost = webViewHost;
    }

    /// <summary>
    /// Sets the active Revit document for load operations.
    /// </summary>
    public void SetActiveDocument(Document document)
    {
        _activeDocument = document;
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
                HandleLoadFamilyRequest(message.Payload as JObject);
                break;

            case "ui:log":
                var logPayload = message.Payload as JObject;
                var level = logPayload?["level"]?.ToString() ?? "info";
                var logMessage = logPayload?["message"]?.ToString() ?? "";
                System.Diagnostics.Debug.WriteLine($"[UI {level.ToUpper()}] {logMessage}");
                break;

            // Phase 2 - Scanner events
            case "ui:scan-project":
                HandleScanProjectAsync(message.Payload as JObject).ConfigureAwait(false);
                break;

            case "ui:update-families":
                HandleUpdateFamiliesAsync(message.Payload as JObject).ConfigureAwait(false);
                break;

            case "ui:stamp-legacy":
                HandleStampLegacyAsync(message.Payload as JObject).ConfigureAwait(false);
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

    private void HandleLoadFamilyRequest(JObject? payload)
    {
        try
        {
            var familyIdStr = payload?["familyId"]?.ToString();
            var versionToken = payload?["version"];

            if (string.IsNullOrEmpty(familyIdStr))
            {
                SendLoadFamilyResult(success: false, message: "Family ID is required");
                return;
            }

            if (!Guid.TryParse(familyIdStr, out var familyId))
            {
                SendLoadFamilyResult(success: false, message: "Invalid Family ID format");
                return;
            }

            int? version = versionToken != null ? versionToken.Value<int?>() : null;

            if (_activeDocument == null)
            {
                SendLoadFamilyResult(success: false, message: "No active document");
                return;
            }

            var result = LoadFamilyCommand.LoadFamilyFromLibrary(_activeDocument, familyId, version);

            SendLoadFamilyResult(
                success: result.Success,
                message: result.Message,
                familyName: result.Family?.Name,
                wasNewlyLoaded: result.WasNewlyLoaded
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load family error: {ex.Message}");
            SendLoadFamilyResult(success: false, message: ex.Message);
        }
    }

    private void SendLoadFamilyResult(bool success, string message, string? familyName = null, bool? wasNewlyLoaded = null)
    {
        var payload = new
        {
            success,
            message,
            familyName,
            wasNewlyLoaded
        };

        SendEvent("revit:family-loaded", payload);
    }

    #region Phase 2 - Scanner Event Handlers

    private async System.Threading.Tasks.Task HandleScanProjectAsync(JObject? payload)
    {
        if (_activeDocument == null)
        {
            SendScanError("No active document");
            return;
        }

        try
        {
            var scannerService = new ProjectScannerService();
            var results = await scannerService.ScanAsync(_activeDocument).ConfigureAwait(false);

            var summary = new
            {
                upToDate = results.Count(r => r.Status == FamilyScanStatus.UpToDate),
                updateAvailable = results.Count(r => r.Status == FamilyScanStatus.UpdateAvailable),
                legacyMatch = results.Count(r => r.Status == FamilyScanStatus.LegacyMatch),
                unmatched = results.Count(r => r.Status == FamilyScanStatus.Unmatched),
                localModified = results.Count(r => r.Status == FamilyScanStatus.LocalModified)
            };

            SendEvent("revit:scan:result", new
            {
                families = results,
                totalCount = results.Count,
                summary
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Scan error: {ex.Message}");
            SendScanError(ex.Message);
        }
    }

    private async System.Threading.Tasks.Task HandleUpdateFamiliesAsync(JObject? payload)
    {
        if (_activeDocument == null)
        {
            SendUpdateResult(false, "No active document", new List<UpdateResult>());
            return;
        }

        try
        {
            var familiesArray = payload?["families"] as JArray;
            if (familiesArray == null || familiesArray.Count == 0)
            {
                SendUpdateResult(false, "No families to update", new List<UpdateResult>());
                return;
            }

            var updaterService = new FamilyUpdaterService();
            var results = new List<UpdateResult>();

            foreach (var item in familiesArray)
            {
                var uniqueId = item["uniqueId"]?.ToString();
                var roleName = item["roleName"]?.ToString();
                var targetVersion = item.Value<int?>("targetVersion");

                if (string.IsNullOrEmpty(uniqueId) || string.IsNullOrEmpty(roleName) || !targetVersion.HasValue)
                    continue;

                var result = await updaterService.UpdateFamilyAsync(
                    _activeDocument, uniqueId, roleName, targetVersion.Value).ConfigureAwait(false);

                results.Add(result);
            }

            var success = results.All(r => r.Success);
            var message = success
                ? $"Updated {results.Count} families"
                : $"Updated {results.Count(r => r.Success)}/{results.Count} families";

            SendUpdateResult(success, message, results);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update error: {ex.Message}");
            SendUpdateResult(false, ex.Message, new List<UpdateResult>());
        }
    }

    private async System.Threading.Tasks.Task HandleStampLegacyAsync(JObject? payload)
    {
        if (_activeDocument == null)
        {
            SendStampLegacyResult(false, "No active document", 0);
            return;
        }

        try
        {
            var familiesArray = payload?["families"] as JArray;
            if (familiesArray == null || familiesArray.Count == 0)
            {
                SendStampLegacyResult(false, "No families to stamp", 0);
                return;
            }

            var esService = new Infrastructure.ExtensibleStorage.EsService();
            int stampedCount = 0;

            using (var transaction = new Transaction(_activeDocument, "Stamp Legacy Families"))
            {
                transaction.Start();

                foreach (var item in familiesArray)
                {
                    var uniqueId = item["uniqueId"]?.ToString();
                    var roleId = item["roleId"]?.ToString();
                    var roleName = item["roleName"]?.ToString();

                    if (string.IsNullOrEmpty(uniqueId) || string.IsNullOrEmpty(roleName))
                        continue;

                    // Find family by UniqueId
                    var family = new FilteredElementCollector(_activeDocument)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.UniqueId == uniqueId);

                    if (family == null) continue;

                    // Create stamp data
                    var stampData = new Core.Entities.EsStampData
                    {
                        RoleId = Guid.TryParse(roleId, out var rid) ? rid : Guid.NewGuid(),
                        RoleName = roleName,
                        FamilyName = family.Name,
                        ContentHash = "", // Will be computed on next scan
                        StampedAt = DateTime.UtcNow,
                        StampedBy = Environment.UserName
                    };

                    esService.WriteStamp(family, stampData);
                    stampedCount++;
                }

                transaction.Commit();
            }

            SendStampLegacyResult(true, $"Stamped {stampedCount} families", stampedCount);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Stamp legacy error: {ex.Message}");
            SendStampLegacyResult(false, ex.Message, 0);
        }
    }

    private void SendScanError(string error)
    {
        SendEvent("revit:scan:error", new { error });
    }

    private void SendUpdateResult(bool success, string message, List<UpdateResult> results)
    {
        SendEvent("revit:update:result", new { success, message, results });
    }

    private void SendStampLegacyResult(bool success, string message, int stampedCount)
    {
        SendEvent("revit:stamp-legacy:result", new { success, message, stampedCount });
    }

    #endregion
}
