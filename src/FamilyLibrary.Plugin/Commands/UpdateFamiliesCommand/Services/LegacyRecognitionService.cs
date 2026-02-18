using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services
{
    /// <summary>
    /// Service for matching unstamped families to roles using recognition rules.
    /// Rules are cached for session duration.
    /// </summary>
    public class LegacyRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private List<RecognitionRuleDto>? _cachedRules;
        private DateTime _cacheTime;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

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
        /// Initialize by loading recognition rules from API.
        /// Uses cache to avoid repeated API calls.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_cachedRules != null && DateTime.Now - _cacheTime < _cacheDuration)
                return;

            _cachedRules = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/recognition-rules").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<List<RecognitionRuleDto>>(json);
            }).ConfigureAwait(false);

            _cacheTime = DateTime.Now;
        }

        /// <summary>
        /// Match a family name to a role using recognition rules.
        /// Returns null if no match found.
        /// </summary>
        public string? MatchRole(string familyName)
        {
            if (_cachedRules == null || string.IsNullOrEmpty(familyName))
                return null;

            foreach (var rule in _cachedRules)
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
