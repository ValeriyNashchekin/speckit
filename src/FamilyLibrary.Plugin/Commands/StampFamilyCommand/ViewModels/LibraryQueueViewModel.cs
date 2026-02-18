using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;

/// <summary>
/// ViewModel for the Library Queue window.
/// Manages family scanning, stamping, and publishing operations.
/// </summary>
public sealed partial class LibraryQueueViewModel : ObservableObject
{
    private readonly FamilyScannerService _scannerService;
    private readonly StampService _stampService;
    private readonly PublishService _publishService;
    private readonly LocalChangeDetector _changeDetector;
    private readonly SnapshotService _snapshotService;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    [ObservableProperty]
    private ObservableCollection<FamilyQueueItem> _families = new ObservableCollection<FamilyQueueItem>();

    [ObservableProperty]
    private ObservableCollection<FamilyQueueItem> _queueItems = new ObservableCollection<FamilyQueueItem>();

    [ObservableProperty]
    private string _statusMessage = "Ready. Click Scan to find families.";

    [ObservableProperty]
    private int _scannedCount;

    [ObservableProperty]
    private int _stampedCount;

    [ObservableProperty]
    private int _publishedCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isFamilyEditorMode;

    [ObservableProperty]
    private bool _showScanTab = true;

    [ObservableProperty]
    private bool _showStatusTab = true;

    private Autodesk.Revit.DB.Document? _document;

    public string StatusSummary => GetStatusSummary();

    public LibraryQueueViewModel() : this(isFamilyEditorMode: false, currentFamily: null)
    {
    }

    public LibraryQueueViewModel(bool isFamilyEditorMode, Autodesk.Revit.DB.Family? currentFamily)
    {
        _scannerService = new FamilyScannerService();
        _stampService = new StampService();
        _publishService = new PublishService();
        _changeDetector = new LocalChangeDetector();
        _snapshotService = new SnapshotService();
        _httpClient = new HttpClient();
        _apiBaseUrl = "https://localhost:5001/api";

        _isFamilyEditorMode = isFamilyEditorMode;

        if (isFamilyEditorMode)
        {
            _showScanTab = false;
            _showStatusTab = false;
            _statusMessage = "Family Editor mode. Current family is ready for processing.";

            // Auto-add current family to queue (T155)
            if (currentFamily != null)
            {
                AddCurrentFamilyToQueue(currentFamily);
            }
        }
    }

    private void AddCurrentFamilyToQueue(Autodesk.Revit.DB.Family family)
    {
        var item = new FamilyQueueItem
        {
            UniqueId = family.UniqueId,
            FamilyName = family.Name,
            CategoryName = family.FamilyCategory?.Name ?? "Unknown",
            SourcePath = null,
            IsSystemFamily = false,
            Status = QueueItemStatus.Pending
        };

        QueueItems.Add(item);
    }

    public void SetDocument(Autodesk.Revit.DB.Document? document)
    {
        _document = document;
    }

    [RelayCommand]
    private void Scan()
    {
        if (_document == null || IsProcessing) return;

        IsProcessing = true;
        StatusMessage = "Scanning document for families...";

        try
        {
            var families = _scannerService.ScanLoadableFamilies(_document);
            Families.Clear();

            foreach (var family in families)
            {
                Families.Add(new FamilyQueueItem
                {
                    UniqueId = family.UniqueId,
                    FamilyName = family.Name,
                    CategoryName = family.CategoryName,
                    SourcePath = family.SourcePath,
                    HasStamp = family.HasStamp,
                    StampData = family.StampData,
                    Status = QueueItemStatus.Scanned
                });
            }

            ScannedCount = Families.Count;
            StatusMessage = $"Found {ScannedCount} loadable families.";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void AddToQueue()
    {
        var selected = Families.Where(f => f.IsSelected && !QueueItems.Any(q => q.UniqueId == f.UniqueId));
        foreach (var item in selected)
        {
            var queueItem = new FamilyQueueItem
            {
                UniqueId = item.UniqueId,
                FamilyName = item.FamilyName,
                CategoryName = item.CategoryName,
                SourcePath = item.SourcePath,
                HasStamp = item.HasStamp,
                StampData = item.StampData,
                Status = QueueItemStatus.Pending
            };

            // Detect local modifications for stamped families
            if (_document != null && item.HasStamp && item.StampData?.RoleId != Guid.Empty)
            {
                queueItem.HasLocalChanges = DetectLocalChangesAsync(item).Result;
            }

            QueueItems.Add(queueItem);
        }
    }

    /// <summary>
    /// Detects local modifications by comparing current family state with library snapshot.
    /// Uses API to fetch library snapshot for comparison.
    /// </summary>
    private async Task<bool> DetectLocalChangesAsync(FamilyQueueItem item)
    {
        if (_document == null || item.StampData?.RoleId == null || item.StampData.RoleId == Guid.Empty)
            return false;

        try
        {
            // Get family element from document
            var element = _document.GetElement(item.UniqueId);
            if (!(element is Autodesk.Revit.DB.Family family))
                return false;

            // Create current local snapshot
            var localSnapshot = _snapshotService.CreateSnapshot(family, _document);

            // Fetch library snapshot via API
            var librarySnapshot = await FetchLibrarySnapshotAsync(item.StampData.RoleName);
            if (librarySnapshot == null)
            {
                // If no library snapshot available, cannot detect changes
                return false;
            }

            // Use local change detector to compare
            return _changeDetector.HasLocalChanges(family, _document, librarySnapshot);
        }
        catch
        {
            // If detection fails, assume no changes
            return false;
        }
    }

    /// <summary>
    /// Fetches the latest family snapshot from the library API.
    /// Uses existing API endpoint to get family details including latest version.
    /// </summary>
    private async Task<Core.Models.FamilySnapshot?> FetchLibrarySnapshotAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return null;

        try
        {
            // Find family by role name
            var response = await _httpClient.GetAsync(
                $"{_apiBaseUrl}/families?searchTerm={Uri.EscapeDataString(roleName)}&pageSize=1");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonConvert.DeserializeObject<PagedFamilyResult>(json);

            if (pagedResult?.Items == null || pagedResult.Items.Count == 0)
                return null;

            var familyId = pagedResult.Items[0].Id;

            // Get family details with versions
            var detailResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/families/{familyId}");
            if (!detailResponse.IsSuccessStatusCode)
                return null;

            var detailJson = await detailResponse.Content.ReadAsStringAsync();
            var familyDetail = JsonConvert.DeserializeObject<FamilyDetailResponse>(detailJson);

            // MVP: API doesn't expose SnapshotJson in version DTO yet
            // Return null for now - when API is enhanced, fetch actual snapshot
            return null;
        }
        catch
        {
            return null;
        }
    }

    #region API DTOs

    private class PagedFamilyResult
    {
        [JsonProperty("items")]
        public List<FamilyResult>? Items { get; set; }
    }

    private class FamilyResult
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    private class FamilyDetailResponse
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int CurrentVersion { get; set; }
        public List<FamilyVersionDto>? Versions { get; set; }
    }

    private class FamilyVersionDto
    {
        public int Version { get; set; }
        public string? Hash { get; set; }
    }

    #endregion

    [RelayCommand]
    private void Stamp()
    {
        if (_document == null || IsProcessing) return;

        IsProcessing = true;
        StatusMessage = "Stamping families...";

        try
        {
            var toStamp = QueueItems.Where(q => q.Status == QueueItemStatus.Pending || q.Status == QueueItemStatus.Scanned).ToList();
            var stamped = _stampService.StampFamilies(_document, toStamp);
            StampedCount = stamped;
            StatusMessage = $"Stamped {stamped} families.";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Stamp failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void Publish()
    {
        if (IsProcessing) return;

        IsProcessing = true;
        StatusMessage = "Publishing families...";

        try
        {
            var toPublish = QueueItems.Where(q => q.Status == QueueItemStatus.Stamped).ToList();
            var published = _publishService.PublishFamilies(toPublish);
            PublishedCount = published;
            StatusMessage = $"Published {published} families.";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Publish failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void RemoveFromQueue(FamilyQueueItem item)
    {
        if (item != null)
        {
            QueueItems.Remove(item);
        }
    }

    [RelayCommand]
    private void ClearQueue()
    {
        QueueItems.Clear();
        StatusMessage = "Queue cleared.";
    }

    [RelayCommand]
    private void ViewChanges(FamilyQueueItem item)
    {
        if (item == null || _document == null)
            return;

        if (!item.HasLocalChanges || item.StampData?.RoleId == null || item.StampData.RoleId == Guid.Empty)
        {
            System.Windows.MessageBox.Show(
                "No local changes detected for this family.",
                "Changes",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        try
        {
            var element = _document.GetElement(item.UniqueId);
            if (!(element is Autodesk.Revit.DB.Family family))
            {
                System.Windows.MessageBox.Show(
                    "Unable to access family element.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Fetch library snapshot synchronously for MVP
            var librarySnapshot = FetchLibrarySnapshotAsync(item.StampData.RoleName).Result;
            if (librarySnapshot == null)
            {
                System.Windows.MessageBox.Show(
                    "Unable to fetch library snapshot for comparison.",
                    "Changes",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            var changeSet = _changeDetector.DetectChanges(family, _document, librarySnapshot);
            var summary = _changeDetector.GetChangeSummary(changeSet);

            System.Windows.MessageBox.Show(
                summary,
                $"Changes: {item.FamilyName}",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error detecting changes: {ex.Message}",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    public string GetStatusSummary()
    {
        return $"Scanned: {ScannedCount} | Stamped: {StampedCount} | Published: {PublishedCount} | Failed: {FailedCount}";
    }
}
