using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Infrastructure.Hashing;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Service for publishing families to backend API and blob storage.
/// Uses Azure.Storage.Blobs for file upload.
/// Includes nested family detection and dependency tracking.
/// </summary>
public class PublishService
{
    private readonly IContentHashService _hashService;
    private readonly SnapshotService _snapshotService;
    private readonly NestedDetectionService _nestedDetectionService;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _blobConnectionString;

    // Event for WebView2 notification
    public event Action<NestedDetectedEvent>? NestedFamiliesDetected;

    public PublishService() : this(new ContentHashService(), new SnapshotService(), new NestedDetectionService())
    {
    }

    public PublishService(IContentHashService hashService) : this(hashService, new SnapshotService(), new NestedDetectionService())
    {
    }

    public PublishService(IContentHashService hashService, SnapshotService snapshotService)
        : this(hashService, snapshotService, new NestedDetectionService())
    {
    }

    public PublishService(
        IContentHashService hashService,
        SnapshotService snapshotService,
        NestedDetectionService nestedDetectionService)
    {
        _hashService = hashService;
        _snapshotService = snapshotService ?? throw new ArgumentNullException(nameof(snapshotService));
        _nestedDetectionService = nestedDetectionService ?? throw new ArgumentNullException(nameof(nestedDetectionService));
        _httpClient = new HttpClient();
        // MVP: Configuration should be injected
        _apiBaseUrl = "https://localhost:5001/api";
        _blobConnectionString = string.Empty;
    }

    /// <summary>
    /// Publish multiple families to the library.
    /// Returns count of successfully published families.
    /// </summary>
    public int PublishFamilies(List<FamilyQueueItem> items)
    {
        if (items == null || items.Count == 0)
            return 0;

        var publishedCount = 0;

        foreach (var item in items)
        {
            if (PublishSingleFamily(item))
            {
                item.Status = QueueItemStatus.Published;
                publishedCount++;
            }
        }

        return publishedCount;
    }

    /// <summary>
    /// Publish a family with document context for snapshot creation and nested detection.
    /// </summary>
    public int PublishFamilies(List<FamilyQueueItem> items, Document document)
    {
        if (items == null || items.Count == 0)
            return 0;
        if (document == null)
            return PublishFamilies(items);

        var publishedCount = 0;

        foreach (var item in items)
        {
            if (PublishSingleFamilyWithSnapshotAndNested(item, document))
            {
                item.Status = QueueItemStatus.Published;
                publishedCount++;
            }
        }

        return publishedCount;
    }

    private bool PublishSingleFamily(FamilyQueueItem item)
    {
        try
        {
            // Validate source file exists
            if (string.IsNullOrEmpty(item.SourcePath) || !File.Exists(item.SourcePath))
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "Source file not found";
                return false;
            }

            // Compute content hash
            var contentHash = _hashService.ComputeHash(item.SourcePath!);
            if (item.StampData != null)
            {
                item.StampData.ContentHash = contentHash;
            }

            // Upload to blob storage (MVP: placeholder)
            var blobUrl = UploadToBlobStorage(item.SourcePath!, contentHash);

            // Call backend API
            var request = new PublishRequest
            {
                FamilyUniqueId = item.UniqueId,
                FamilyName = item.FamilyName,
                RoleId = item.StampData?.RoleId ?? Guid.Empty,
                RoleName = item.StampData?.RoleName ?? item.FamilyName,
                ContentHash = contentHash,
                CatalogFilePath = blobUrl
            };

            var success = CallPublishApi(request);
            if (!success)
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "API call failed";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            item.Status = QueueItemStatus.Failed;
            item.ErrorMessage = ex.Message;
            return false;
        }
    }

    private bool PublishSingleFamilyWithSnapshot(FamilyQueueItem item, Document document)
    {
        try
        {
            // Validate source file exists
            if (string.IsNullOrEmpty(item.SourcePath) || !File.Exists(item.SourcePath))
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "Source file not found";
                return false;
            }

            // Compute content hash
            var contentHash = _hashService.ComputeHash(item.SourcePath!);
            if (item.StampData != null)
            {
                item.StampData.ContentHash = contentHash;
            }

            // Create snapshot JSON
            string? snapshotJson = null;
            var element = document.GetElement(item.UniqueId);
            if (element is Family family)
            {
                snapshotJson = _snapshotService.CreateSnapshotJson(family, document);
            }

            // Upload to blob storage (MVP: placeholder)
            var blobUrl = UploadToBlobStorage(item.SourcePath!, contentHash);

            // Call backend API
            var request = new PublishRequest
            {
                FamilyUniqueId = item.UniqueId,
                FamilyName = item.FamilyName,
                RoleId = item.StampData?.RoleId ?? Guid.Empty,
                RoleName = item.StampData?.RoleName ?? item.FamilyName,
                ContentHash = contentHash,
                CatalogFilePath = blobUrl,
                SnapshotJson = snapshotJson
            };

            var success = CallPublishApi(request);
            if (!success)
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "API call failed";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            item.Status = QueueItemStatus.Failed;
            item.ErrorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Publishes a family with snapshot creation and nested family detection.
    /// Detects nested families, sends WebView2 event, and saves dependencies to API.
    /// </summary>
    private bool PublishSingleFamilyWithSnapshotAndNested(FamilyQueueItem item, Document document)
    {
        try
        {
            // Validate source file exists
            if (string.IsNullOrEmpty(item.SourcePath) || !File.Exists(item.SourcePath))
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "Source file not found";
                return false;
            }

            // Get family element for nested detection
            var element = document.GetElement(item.UniqueId);
            if (!(element is Family family))
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "Family element not found";
                return false;
            }

            // Detect nested families before publishing
            var nestedFamilies = _nestedDetectionService.Detect(document, family);

            // Send WebView2 event with detected nested families
            if (nestedFamilies.Count > 0)
            {
                var detectedEvent = new NestedDetectedEvent
                {
                    ParentFamilyId = item.UniqueId,
                    ParentFamilyName = item.FamilyName,
                    NestedFamilies = nestedFamilies
                };
                NestedFamiliesDetected?.Invoke(detectedEvent);
            }

            // Compute content hash
            var contentHash = _hashService.ComputeHash(item.SourcePath!);
            if (item.StampData != null)
            {
                item.StampData.ContentHash = contentHash;
            }

            // Create snapshot JSON
            var snapshotJson = _snapshotService.CreateSnapshotJson(family, document);

            // Upload to blob storage (MVP: placeholder)
            var blobUrl = UploadToBlobStorage(item.SourcePath!, contentHash);

            // Call backend API to publish
            var request = new PublishRequest
            {
                FamilyUniqueId = item.UniqueId,
                FamilyName = item.FamilyName,
                RoleId = item.StampData?.RoleId ?? Guid.Empty,
                RoleName = item.StampData?.RoleName ?? item.FamilyName,
                ContentHash = contentHash,
                CatalogFilePath = blobUrl,
                SnapshotJson = snapshotJson
            };

            var familyId = CallPublishApiAndGetId(request);
            if (familyId == null)
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorMessage = "API call failed";
                return false;
            }

            // Save dependencies to API if any nested families were detected
            if (nestedFamilies.Count > 0)
            {
                SaveDependenciesToApi(familyId.Value, nestedFamilies);
            }

            return true;
        }
        catch (Exception ex)
        {
            item.Status = QueueItemStatus.Failed;
            item.ErrorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Calls the publish API and returns the created family ID.
    /// </summary>
    private Guid? CallPublishApiAndGetId(PublishRequest request)
    {
        try
        {
            return RetryHelper.ExecuteWithRetryAsync(() =>
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return ExecutePostRequestAndGetIdAsync($"{_apiBaseUrl}/families/publish", content);
            }).Result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Executes POST request and extracts family ID from response.
    /// </summary>
    private async Task<Guid?> ExecutePostRequestAndGetIdAsync(string url, StringContent content)
    {
        var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API returned {(int)response.StatusCode} ({response.StatusCode})");
        }

        // Parse response to get family ID
        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<PublishResponse>(responseJson);
        return result?.Id;
    }

    /// <summary>
    /// Saves detected dependencies to the API.
    /// </summary>
    private void SaveDependenciesToApi(Guid familyId, List<NestedFamilyInfo> nestedFamilies)
    {
        try
        {
            var request = new SaveDependenciesRequest
            {
                Dependencies = nestedFamilies.Select(n => new SaveDependencyDto
                {
                    NestedFamilyName = n.FamilyName,
                    NestedRoleName = n.RoleName,
                    IsShared = n.IsShared,
                    InLibrary = n.InLibrary,
                    LibraryVersion = n.LibraryVersion
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Fire and forget - don't fail publish if dependency save fails
            _ = _httpClient.PostAsync($"{_apiBaseUrl}/families/{familyId}/dependencies", content);
        }
        catch
        {
            // Log but don't fail the publish operation
            System.Diagnostics.Debug.WriteLine("Failed to save dependencies to API");
        }
    }

    private string UploadToBlobStorage(string filePath, string contentHash)
    {
        // MVP: Placeholder - actual implementation would use Azure.Storage.Blobs
        // var blobServiceClient = new BlobServiceClient(_blobConnectionString);
        // var containerClient = blobServiceClient.GetBlobContainerClient("families");
        // var blobClient = containerClient.GetBlobClient($"{contentHash}.rfa");
        // await blobClient.UploadAsync(filePath, true);
        // return blobClient.Uri.ToString();

        return $"https://storage.example.com/families/{contentHash}.rfa";
    }

    private bool CallPublishApi(PublishRequest request)
    {
        try
        {
            return RetryHelper.ExecuteWithRetryAsync(() =>
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return ExecutePostRequestAsync($"{_apiBaseUrl}/families/publish", content);
            }).Result;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ExecutePostRequestAsync(string url, StringContent content)
    {
        var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API returned {(int)response.StatusCode} ({response.StatusCode})");
        }
        return true;
    }
}

/// <summary>
/// Response from publish API containing the created family ID.
/// </summary>
internal class PublishResponse
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
}
