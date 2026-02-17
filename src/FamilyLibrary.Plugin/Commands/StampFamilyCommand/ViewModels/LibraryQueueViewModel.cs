using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

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

    private Autodesk.Revit.DB.Document? _document;

    public string StatusSummary => GetStatusSummary();

    public LibraryQueueViewModel()
    {
        _scannerService = new FamilyScannerService();
        _stampService = new StampService();
        _publishService = new PublishService();
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
            QueueItems.Add(new FamilyQueueItem
            {
                UniqueId = item.UniqueId,
                FamilyName = item.FamilyName,
                CategoryName = item.CategoryName,
                SourcePath = item.SourcePath,
                HasStamp = item.HasStamp,
                StampData = item.StampData,
                Status = QueueItemStatus.Pending
            });
        }
    }

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

    public string GetStatusSummary()
    {
        return $"Scanned: {ScannedCount} | Stamped: {StampedCount} | Published: {PublishedCount} | Failed: {FailedCount}";
    }
}
