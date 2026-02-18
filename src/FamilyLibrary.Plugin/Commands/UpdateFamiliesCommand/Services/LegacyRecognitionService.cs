using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services
{
    /// <summary>
    /// Service for matching unstamped families to roles using recognition rules.
    /// Rules are cached globally for session duration with thread-safe access.
    /// </summary>
    public class LegacyRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        // Static cache shared across all instances
        private static readonly List<RecognitionRuleDto> s_cachedRules;
        private static DateTime s_cacheTime;
        private static readonly TimeSpan s_cacheDuration = TimeSpan.FromMinutes(30);
        private static readonly SemaphoreSlim s_cacheLock = new SemaphoreSlim(1, 1);

        static LegacyRecognitionService()
        {
            s_cachedRules = new List<RecognitionRuleDto>();
        }

        public LegacyRecognitionService()
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = "https://localhost:5001/api";
        }

        public LegacyRecognitionService(HttpClient httpClient, string apiBaseUrl)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
        }

        /// <summary>
        /// Check if cache is valid (not expired).
        /// </summary>
        private static bool IsCacheValid => s_cachedRules.Count > 0 && DateTime.Now - s_cacheTime < s_cacheDuration;

        /// <summary>
        /// Initialize by loading recognition rules from API.
        /// Uses cache to avoid repeated API calls.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (IsCacheValid)
                return;

            await s_cacheLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Double-check after acquiring lock
                if (IsCacheValid)
                    return;

                var rules = await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/recognition-rules").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<RecognitionRuleDto>>(json);
                }).ConfigureAwait(false);

                s_cachedRules.Clear();
                if (rules != null)
                    s_cachedRules.AddRange(rules);

                s_cacheTime = DateTime.Now;
            }
            finally
            {
                s_cacheLock.Release();
            }
        }

        /// <summary>
        /// Force refresh cache from API regardless of expiration.
        /// </summary>
        public async Task RefreshCacheAsync()
        {
            await s_cacheLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var rules = await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/recognition-rules").ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<RecognitionRuleDto>>(json);
                }).ConfigureAwait(false);

                s_cachedRules.Clear();
                if (rules != null)
                    s_cachedRules.AddRange(rules);

                s_cacheTime = DateTime.Now;
            }
            finally
            {
                s_cacheLock.Release();
            }
        }

        /// <summary>
        /// Clear cache immediately without API call.
        /// </summary>
        public void InvalidateCache()
        {
            s_cacheLock.Wait();
            try
            {
                s_cachedRules.Clear();
                s_cacheTime = DateTime.MinValue;
            }
            finally
            {
                s_cacheLock.Release();
            }
        }

        /// <summary>
        /// Match a family name to a role using recognition rules.
        /// Returns null if no match found.
        /// </summary>
        public string? MatchRole(string familyName)
        {
            if (s_cachedRules.Count == 0 || string.IsNullOrEmpty(familyName))
                return null;

            foreach (var rule in s_cachedRules)
            {
                if (EvaluateRule(rule.RootNode, familyName))
                    return rule.RoleName;
            }

            return null;
        }

        private bool EvaluateRule(RecognitionNode node, string name)
        {
            if (node == null) return false;

            return node.Type == "group"
                ? EvaluateGroup(node, name)
                : EvaluateCondition(node, name);
        }

        private bool EvaluateGroup(RecognitionNode node, string name)
        {
            if (node.Children == null || node.Children.Count == 0)
                return false;

            return node.Operator == "AND"
                ? node.Children.All(c => EvaluateRule(c, name))
                : node.Children.Any(c => EvaluateRule(c, name));
        }

        private bool EvaluateCondition(RecognitionNode node, string name)
        {
            if (string.IsNullOrEmpty(node.Value))
                return false;

            var contains = name.IndexOf(node.Value, StringComparison.OrdinalIgnoreCase) >= 0;
            return node.Operator == "Contains" ? contains : !contains;
        }
    }

    /// <summary>
    /// DTO for recognition rule from API.
    /// </summary>
    public class RecognitionRuleDto
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public RecognitionNode RootNode { get; set; } = new RecognitionNode();
    }

    /// <summary>
    /// Node in recognition rule tree (group or condition).
    /// </summary>
    public class RecognitionNode
    {
        public string Type { get; set; } = "condition";
        public string? Operator { get; set; }
        public string? Value { get; set; }
        public List<RecognitionNode>? Children { get; set; }
    }
}
