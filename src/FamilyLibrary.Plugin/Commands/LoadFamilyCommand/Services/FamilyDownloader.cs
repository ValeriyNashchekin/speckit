using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;

/// <summary>
/// Downloads family files from backend API.
/// Uses HttpClient for API calls and blob storage downloads.
/// </summary>
public class FamilyDownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _tempFolder;

    public FamilyDownloader()
    {
        _httpClient = new HttpClient();
        _apiBaseUrl = "https://localhost:5001/api";
        _tempFolder = Path.Combine(Path.GetTempPath(), "FamilyLibrary");
        Directory.CreateDirectory(_tempFolder);
    }

    /// <summary>
    /// Downloads family file from the library.
    /// Returns local file path after download.
    /// </summary>
    public async Task<DownloadResult> DownloadFamilyAsync(Guid familyId, int? version = null)
    {
        var url = version.HasValue
            ? $"{_apiBaseUrl}/families/{familyId}/download/{version}"
            : $"{_apiBaseUrl}/families/{familyId}/download";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var downloadInfo = JsonConvert.DeserializeObject<FamilyDownloadResponse>(json);

        if (downloadInfo == null)
        {
            throw new InvalidOperationException("Failed to parse download response");
        }

        var tempPath = Path.Combine(_tempFolder, downloadInfo.OriginalFileName);
        await DownloadFileAsync(downloadInfo.DownloadUrl, tempPath);

        return new DownloadResult
        {
            LocalPath = tempPath,
            OriginalFileName = downloadInfo.OriginalFileName,
            Hash = downloadInfo.Hash,
            Version = downloadInfo.Version
        };
    }

    private async Task DownloadFileAsync(string url, string destinationPath)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(destinationPath);
        await stream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// Cleans up temporary files.
    /// </summary>
    public void CleanupTempFiles()
    {
        try
        {
            if (Directory.Exists(_tempFolder))
            {
                foreach (var file in Directory.GetFiles(_tempFolder))
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

/// <summary>
/// Result of family download operation.
/// </summary>
public class DownloadResult
{
    public string LocalPath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Version { get; set; }
}

/// <summary>
/// Response from download API endpoint.
/// </summary>
public class FamilyDownloadResponse
{
    public string DownloadUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Version { get; set; }
}
