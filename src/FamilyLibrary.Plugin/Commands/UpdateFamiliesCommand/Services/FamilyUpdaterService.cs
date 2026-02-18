using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Models;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services
{
    /// <summary>
    /// Service for updating families from the library.
    /// Downloads and loads new family versions.
    /// </summary>
    public class FamilyUpdaterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly string _tempFolder;
        private readonly UpdatePreviewService? _previewService;

        public FamilyUpdaterService() : this(null)
        {
        }

        public FamilyUpdaterService(UpdatePreviewService? previewService)
        {
            _previewService = previewService;
            _httpClient = new HttpClient();
            _apiBaseUrl = "https://localhost:5001/api";
            _tempFolder = Path.Combine(Path.GetTempPath(), "FamilyLibrary", "Updates");
            Directory.CreateDirectory(_tempFolder);
        }

        public FamilyUpdaterService(HttpClient httpClient, string apiBaseUrl, UpdatePreviewService? previewService = null)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
            _previewService = previewService;
            _tempFolder = Path.Combine(Path.GetTempPath(), "FamilyLibrary", "Updates");
            Directory.CreateDirectory(_tempFolder);
        }

        /// <summary>
        /// Computes a preview of changes before updating a family.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="uniqueId">The unique ID of the family to update.</param>
        /// <param name="librarySnapshot">The library snapshot to compare against.</param>
        /// <returns>ChangeSet with detected modifications, or null if preview service not available.</returns>
        public ChangeSet? ComputeUpdatePreview(Document document, string uniqueId, FamilySnapshot librarySnapshot)
        {
            if (_previewService == null)
                return null;

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var family = new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(f => f.UniqueId == uniqueId);

            if (family == null)
                return null;

            return _previewService.ComputePreview(family, document, librarySnapshot);
        }

        public async Task<UpdateResult> UpdateFamilyAsync(
            Document document,
            string uniqueId,
            string roleName,
            int targetVersion)
        {
            if (document == null)
                return new UpdateResult { Success = false, FamilyName = roleName, Error = "No document" };

            string? tempPath = null;
            try
            {
                var downloadUrl = await GetDownloadUrlAsync(roleName, targetVersion).ConfigureAwait(false);
                if (string.IsNullOrEmpty(downloadUrl))
                    return new UpdateResult { Success = false, FamilyName = roleName, Error = "Failed to get download URL" };

                tempPath = Path.Combine(_tempFolder, $"{roleName}_v{targetVersion}_{Guid.NewGuid():N}.rfa");
                await DownloadFileAsync(downloadUrl, tempPath).ConfigureAwait(false);
                var result = LoadFamilyToDocument(document, uniqueId, tempPath, roleName);
                return result;
            }
            catch (Exception ex)
            {
                return new UpdateResult { Success = false, FamilyName = roleName, Error = ex.Message };
            }
            finally
            {
                if (tempPath != null && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); }
                    catch { }
                }
            }
        }

        private async Task<string?> GetDownloadUrlAsync(string roleName, int version)
        {
            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                var url = $"{_apiBaseUrl}/families/by-role/{Uri.EscapeDataString(roleName)}/download/{version}";
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<DownloadUrlResponse>(json);
                return result?.DownloadUrl;
            }).ConfigureAwait(false);
        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var fileStream = File.Create(destinationPath);
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private UpdateResult LoadFamilyToDocument(Document document, string uniqueId, string rfaPath, string roleName)
        {
            Family? newFamily = null;
            bool loadSucceeded = false;

            using (var transaction = new Transaction(document, "Update " + roleName))
            {
                transaction.Start();
                try
                {
                    var existingFamily = new FilteredElementCollector(document)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.UniqueId == uniqueId);

                    if (existingFamily != null)
                    {
                        var loadOptions = new FamilyLoadOptionsHandler();
                        loadSucceeded = document.LoadFamily(rfaPath, loadOptions, out newFamily);
                    }
                    else
                    {
                        loadSucceeded = document.LoadFamily(rfaPath, out newFamily);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                    throw;
                }
            }

            return new UpdateResult
            {
                Success = loadSucceeded,
                FamilyName = newFamily?.Name ?? roleName,
                Error = loadSucceeded ? null : "Failed to load family"
            };
        }

        public void CleanupTempFiles()
        {
            try
            {
                if (Directory.Exists(_tempFolder))
                {
                    foreach (var file in Directory.GetFiles(_tempFolder))
                        File.Delete(file);
                }
            }
            catch { }
        }
    }

    internal class FamilyLoadOptionsHandler : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, 
            out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    internal class DownloadUrlResponse
    {
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
