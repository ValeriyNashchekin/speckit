using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;
using FamilyLibrary.Plugin.Infrastructure.Hashing;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services
{
    public class ProjectScannerService
    {
        private readonly IEsService _esService;
        private readonly IContentHashService _hashService;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly LegacyRecognitionService _legacyRecognitionService;
        private readonly FamilyScannerService _familyScanner;

        public ProjectScannerService()
            : this(new EsService(), new ContentHashService(), new HttpClient(), "https://localhost:5001/api", new LegacyRecognitionService())
        {
        }

        public ProjectScannerService(
            IEsService esService,
            IContentHashService hashService,
            HttpClient httpClient,
            string apiBaseUrl,
            LegacyRecognitionService legacyRecognitionService)
        {
            _esService = esService;
            _hashService = hashService;
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
            _legacyRecognitionService = legacyRecognitionService;
            _familyScanner = new FamilyScannerService(esService);
        }

        public async Task<List<ScannedFamily>> ScanAsync(Document document)
        {
            if (document == null)
                return new List<ScannedFamily>();

            await _legacyRecognitionService.InitializeAsync().ConfigureAwait(false);

            var familyInfos = _familyScanner.ScanLoadableFamilies(document);
            var results = new List<ScannedFamily>();

            var stampedFamilies = familyInfos
                .Where(f => f.HasStamp && f.StampData != null && !string.IsNullOrEmpty(f.StampData.RoleName))
                .Select(f => new StampedFamilyInfo
                {
                    UniqueId = f.UniqueId,
                    Name = f.Name,
                    CategoryName = f.CategoryName,
                    RoleName = f.StampData.RoleName,
                    Hash = f.StampData.ContentHash ?? string.Empty
                })
                .ToList();

            var statusMap = await BatchCheckFamiliesAsync(stampedFamilies).ConfigureAwait(false);

            foreach (var info in stampedFamilies)
            {
                var status = FamilyScanStatus.Unmatched;
                int? libraryVersion = null;
                string libraryHash = null;

                if (statusMap.TryGetValue(info.RoleName, out var result))
                {
                    status = result.Status;
                    libraryVersion = result.LibraryVersion;
                    libraryHash = result.LibraryHash;
                }

                results.Add(new ScannedFamily
                {
                    UniqueId = info.UniqueId,
                    FamilyName = info.Name,
                    Category = info.CategoryName,
                    RoleName = info.RoleName,
                    IsAutoRole = false,
                    Status = status,
                    LibraryVersion = libraryVersion,
                    LibraryHash = libraryHash,
                    LocalHash = info.Hash
                });
            }

            foreach (var info in familyInfos.Where(f => !f.HasStamp))
            {
                var roleName = _legacyRecognitionService.MatchRole(info.Name);
                var status = !string.IsNullOrEmpty(roleName) ? FamilyScanStatus.LegacyMatch : FamilyScanStatus.Unmatched;

                results.Add(new ScannedFamily
                {
                    UniqueId = info.UniqueId,
                    FamilyName = info.Name,
                    Category = info.CategoryName,
                    RoleName = roleName,
                    IsAutoRole = !string.IsNullOrEmpty(roleName),
                    Status = status
                });
            }

            return results;
        }

        private async Task<Dictionary<string, BatchCheckResult>> BatchCheckFamiliesAsync(List<StampedFamilyInfo> stampedFamilies)
        {
            if (stampedFamilies.Count == 0)
                return new Dictionary<string, BatchCheckResult>();

            try
            {
                var request = new BatchCheckRequest
                {
                    Families = stampedFamilies.Select(f => new FamilyCheckItem
                    {
                        RoleName = f.RoleName,
                        Hash = f.Hash
                    }).ToList()
                };

                var response = await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var httpResponse = await _httpClient.PostAsync(_apiBaseUrl + "/families/batch-check", content).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                    var responseJson = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<BatchCheckResponse>(responseJson);
                }).ConfigureAwait(false);

                if (response != null && response.Results != null)
                    return response.Results.ToDictionary(r => r.RoleName);
                return new Dictionary<string, BatchCheckResult>();
            }
            catch
            {
                return new Dictionary<string, BatchCheckResult>();
            }
        }

        private class StampedFamilyInfo
        {
            public string UniqueId { get; set; }
            public string Name { get; set; }
            public string CategoryName { get; set; }
            public string RoleName { get; set; }
            public string Hash { get; set; }
        }
    }

    public class ScannedFamily
    {
        public string UniqueId { get; set; }
        public string FamilyName { get; set; }
        public string Category { get; set; }
        public string RoleName { get; set; }
        public bool IsAutoRole { get; set; }
        public FamilyScanStatus Status { get; set; }
        public int? LibraryVersion { get; set; }
        public string LibraryHash { get; set; }
        public string LocalHash { get; set; }
    }

    public class BatchCheckRequest
    {
        public List<FamilyCheckItem> Families { get; set; } = new List<FamilyCheckItem>();
    }

    public class FamilyCheckItem
    {
        public string RoleName { get; set; }
        public string Hash { get; set; }
    }

    public class BatchCheckResponse
    {
        public List<BatchCheckResult> Results { get; set; } = new List<BatchCheckResult>();
    }

    public class BatchCheckResult
    {
        public string RoleName { get; set; }
        public FamilyScanStatus Status { get; set; }
        public int? LibraryVersion { get; set; }
        public string LibraryHash { get; set; }
    }
}
