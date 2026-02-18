using System.Text;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;
using FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Core.Models;

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

    // TaskCompletionSource for async UI confirmation
    private TaskCompletionSource<bool>? _updateConfirmationTcs;
    private static readonly TimeSpan ConfirmationTimeout = TimeSpan.FromMinutes(5);

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

            case "ui:get-changes":
                HandleGetChangesAsync(message.Payload as JObject).ConfigureAwait(false);
                break;

            // Update confirmation events from UI
            case "ui:update:confirm":
                HandleUpdateConfirm();
                break;

            case "ui:update:cancel":
                HandleUpdateCancel();
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
            var showPreview = payload?.Value<bool?>("showPreview") ?? false;

            if (familiesArray == null || familiesArray.Count == 0)
            {
                SendUpdateResult(false, "No families to update", new List<UpdateResult>());
                return;
            }

            var previewService = new UpdatePreviewService();
            var updaterService = new FamilyUpdaterService(previewService);
            var results = new List<UpdateResult>();

            // Compute previews for all families if requested
            if (showPreview)
            {
                var previews = ComputePreviews(familiesArray, updaterService);

                // Send preview event to UI
                SendEvent("revit:update:preview", new
                {
                    previews,
                    totalCount = previews.Count,
                    hasChanges = previews.Any(p => p.ChangeSet?.HasChanges == true)
                });

                // Wait for UI confirmation
                var confirmed = await WaitForConfirmationAsync().ConfigureAwait(false);
                if (!confirmed)
                {
                    SendUpdateResult(false, "Update cancelled by user", new List<UpdateResult>());
                    return;
                }
            }

            // Proceed with updates
            results = await ExecuteUpdatesAsync(familiesArray, updaterService).ConfigureAwait(false);

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

    private List<UpdatePreviewItem> ComputePreviews(JArray familiesArray, FamilyUpdaterService updaterService)
    {
        var previews = new List<UpdatePreviewItem>();

        foreach (var item in familiesArray)
        {
            var uniqueId = item["uniqueId"]?.ToString();
            var roleName = item["roleName"]?.ToString();
            var targetVersion = item.Value<int?>("targetVersion");
            var librarySnapshotToken = item["librarySnapshot"] as JObject;

            if (string.IsNullOrEmpty(uniqueId) || string.IsNullOrEmpty(roleName))
                continue;

            FamilySnapshot? librarySnapshot = null;
            if (librarySnapshotToken != null)
            {
                librarySnapshot = librarySnapshotToken.ToObject<FamilySnapshot>();
            }

            var changeSet = updaterService.ComputeUpdatePreview(
                _activeDocument!, uniqueId, librarySnapshot);

            previews.Add(new UpdatePreviewItem
            {
                UniqueId = uniqueId,
                FamilyName = GetFamilyNameByUniqueId(uniqueId),
                RoleName = roleName,
                TargetVersion = targetVersion,
                ChangeSet = changeSet
            });
        }

        return previews;
    }

    private async System.Threading.Tasks.Task<List<UpdateResult>> ExecuteUpdatesAsync(
        JArray familiesArray, FamilyUpdaterService updaterService)
    {
        var results = new List<UpdateResult>();

        foreach (var item in familiesArray)
        {
            var uniqueId = item["uniqueId"]?.ToString();
            var roleName = item["roleName"]?.ToString();
            var targetVersion = item.Value<int?>("targetVersion");

            if (string.IsNullOrEmpty(uniqueId) || string.IsNullOrEmpty(roleName) || !targetVersion.HasValue)
                continue;

            // Send progress update
            SendEvent("revit:update:progress", new
            {
                completed = results.Count,
                total = familiesArray.Count,
                currentFamily = roleName,
                success = results.Count(r => r.Success),
                failed = results.Count(r => !r.Success)
            });

            var result = await updaterService.UpdateFamilyAsync(
                _activeDocument!, uniqueId, roleName, targetVersion.Value).ConfigureAwait(false);

            results.Add(result);
        }

        return results;
    }

    private async System.Threading.Tasks.Task<bool> WaitForConfirmationAsync()
    {
        _updateConfirmationTcs = new TaskCompletionSource<bool>();

        using var cts = new System.Threading.CancellationTokenSource(ConfirmationTimeout);
        cts.Token.Register(() => _updateConfirmationTcs?.TrySetResult(false));

        try
        {
            return await _updateConfirmationTcs.Task.ConfigureAwait(false);
        }
        finally
        {
            _updateConfirmationTcs = null;
        }
    }

    private void HandleUpdateConfirm()
    {
        _updateConfirmationTcs?.TrySetResult(true);
    }

    private void HandleUpdateCancel()
    {
        _updateConfirmationTcs?.TrySetResult(false);
    }

    private string GetFamilyNameByUniqueId(string uniqueId)
    {
        if (_activeDocument == null || string.IsNullOrEmpty(uniqueId))
            return string.Empty;

        var family = new FilteredElementCollector(_activeDocument)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(f => f.UniqueId == uniqueId);

        return family?.Name ?? string.Empty;
    }

    private class UpdatePreviewItem
    {
        public string UniqueId { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int? TargetVersion { get; set; }
        public ChangeSet? ChangeSet { get; set; }
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

    private async System.Threading.Tasks.Task HandleGetChangesAsync(JObject? payload)
    {
        var uniqueId = payload?["uniqueId"]?.ToString();

        if (string.IsNullOrEmpty(uniqueId))
        {
            SendChangesResult(uniqueId ?? string.Empty, new ChangeSet());
            return;
        }

        if (_activeDocument == null)
        {
            SendChangesResult(uniqueId, new ChangeSet());
            return;
        }

        try
        {
            // Find family by UniqueId
            var family = new FilteredElementCollector(_activeDocument)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(f => f.UniqueId == uniqueId);

            if (family == null)
            {
                SendChangesResult(uniqueId, new ChangeSet());
                return;
            }

            // Try to get library snapshot from payload (UI may pass it)
            var librarySnapshotToken = payload?["librarySnapshot"] as JObject;
            FamilySnapshot? librarySnapshot = null;

            if (librarySnapshotToken != null)
            {
                librarySnapshot = librarySnapshotToken.ToObject<FamilySnapshot>();
            }

            // If no snapshot provided, return empty ChangeSet
            // (snapshot would need to be fetched from API in real implementation)
            if (librarySnapshot == null)
            {
                SendChangesResult(uniqueId, new ChangeSet());
                return;
            }

            // Use LocalChangeDetector to compute changes
            var changeDetector = new LocalChangeDetector();
            var changeSet = changeDetector.DetectChanges(family, _activeDocument, librarySnapshot);

            SendChangesResult(uniqueId, changeSet);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Get changes error: {ex.Message}");
            SendChangesResult(uniqueId, new ChangeSet());
        }
    }

    private void SendChangesResult(string familyUniqueId, ChangeSet changes)
    {
        SendEvent("revit:changes:result", new
        {
            familyUniqueId,
            changes
        });
    }

    #endregion
}
