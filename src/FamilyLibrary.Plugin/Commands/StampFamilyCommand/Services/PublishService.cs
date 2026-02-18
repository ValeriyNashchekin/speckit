using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
/// </summary>
public class PublishService
{
    private readonly IContentHashService _hashService;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _blobConnectionString;

    public PublishService() : this(new ContentHashService())
    {
    }

    public PublishService(IContentHashService hashService)
    {
        _hashService = hashService;
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
