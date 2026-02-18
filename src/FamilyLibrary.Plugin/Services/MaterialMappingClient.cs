using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Services
{
    /// <summary>
    /// HTTP client for material mapping API operations.
    /// T062: Provides methods for looking up and saving material mappings.
    /// </summary>
    public class MaterialMappingClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        /// <summary>
        /// Initializes a new instance of the MaterialMappingClient.
        /// </summary>
        /// <param name="apiBaseUrl">The base URL for the API (default: https://localhost:5001/api).</param>
        public MaterialMappingClient(string apiBaseUrl = "https://localhost:5001/api")
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = apiBaseUrl;
        }

        /// <summary>
        /// Looks up a material mapping by template material name.
        /// T062: Used during Pull Update to find project material for a template material.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="templateMaterialName">The template material name to look up.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The material mapping if found, null otherwise.</returns>
        public async Task<MaterialMappingResult?> LookupAsync(
            Guid projectId,
            string templateMaterialName,
            CancellationToken cancellationToken = default)
        {
            if (projectId == Guid.Empty || string.IsNullOrEmpty(templateMaterialName))
                return null;

            try
            {
                return await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var url = string.Format("{0}/material-mappings/lookup?projectId={1}&templateName={2}",
                        _apiBaseUrl,
                        projectId,
                        Uri.EscapeDataString(templateMaterialName));

                    var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<MaterialMappingResult>(json);
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Material mapping lookup failed: {0}", ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Gets all material mappings for a project.
        /// T062: Retrieves cached mappings for the session.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of material mappings for the project.</returns>
        public async Task<List<MaterialMappingResult>> GetAllAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            if (projectId == Guid.Empty)
                return new List<MaterialMappingResult>();

            try
            {
                return await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var url = string.Format("{0}/material-mappings?projectId={1}",
                        _apiBaseUrl,
                        projectId);

                    var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<MaterialMappingResult>>(json)
                        ?? new List<MaterialMappingResult>();
                }).ConfigureAwait(false);
            }
            catch
            {
                return new List<MaterialMappingResult>();
            }
        }

        /// <summary>
        /// Saves a new material mapping.
        /// T062, T065: Saves user mapping decision from UI.
        /// </summary>
        /// <param name="request">The create mapping request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created mapping ID if successful, null otherwise.</returns>
        public async Task<Guid?> SaveMappingAsync(
            CreateMaterialMappingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.ProjectId == Guid.Empty)
                return null;

            if (string.IsNullOrEmpty(request.TemplateMaterialName) ||
                string.IsNullOrEmpty(request.ProjectMaterialName))
                return null;

            try
            {
                return await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var url = string.Format("{0}/material-mappings", _apiBaseUrl);
                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<Guid>(responseJson);
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Save material mapping failed: {0}", ex.Message));
                return null;
            }
        }
    }

    /// <summary>
    /// Result of a material mapping lookup.
    /// Matches MaterialMappingDto from API.
    /// </summary>
    public class MaterialMappingResult
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("projectId")]
        public Guid ProjectId { get; set; }

        [JsonProperty("templateMaterialName")]
        public string TemplateMaterialName { get; set; } = string.Empty;

        [JsonProperty("projectMaterialName")]
        public string ProjectMaterialName { get; set; } = string.Empty;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("lastUsedAt")]
        public DateTime? LastUsedAt { get; set; }
    }

    /// <summary>
    /// Request to create a new material mapping.
    /// Matches CreateMaterialMappingRequest from API.
    /// </summary>
    public class CreateMaterialMappingRequest
    {
        [JsonProperty("projectId")]
        public Guid ProjectId { get; set; }

        [JsonProperty("templateMaterialName")]
        public string TemplateMaterialName { get; set; } = string.Empty;

        [JsonProperty("projectMaterialName")]
        public string ProjectMaterialName { get; set; } = string.Empty;
    }
}
