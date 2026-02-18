using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services
{
    /// <summary>
    /// Service for publishing system family types to backend API.
    /// Handles GroupA (CompoundStructure) and GroupE (Simple parameters).
    /// </summary>
    public class SystemTypePublisher
    {
        private readonly CompoundStructureSerializer _serializer;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        /// <summary>
        /// Default constructor for production use.
        /// </summary>
        public SystemTypePublisher() : this(new CompoundStructureSerializer())
        {
        }

        /// <summary>
        /// Constructor for testing with dependencies.
        /// </summary>
        public SystemTypePublisher(CompoundStructureSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _httpClient = new HttpClient();
            // MVP: Configuration should be injected
            _apiBaseUrl = "https://localhost:5001/api";
        }

        /// <summary>
        /// Publishes a single system type to the backend API.
        /// </summary>
        /// <param name="systemType">The system type information.</param>
        /// <param name="document">The Revit document.</param>
        /// <param name="roleId">The role ID to associate with this type.</param>
        /// <returns>True if published successfully, false otherwise.</returns>
        public bool Publish(SystemTypeInfo systemType, Document document, Guid roleId)
        {
            if (systemType == null)
                throw new ArgumentNullException(nameof(systemType));
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            try
            {
                var element = document.GetElement(systemType.ElementId);
                if (element == null)
                    return false;

                string json;
                if (systemType.Group == SystemFamilyGroup.GroupA)
                {
                    var compoundStructure = GetCompoundStructure(element);
                    json = compoundStructure != null
                        ? _serializer.Serialize(compoundStructure, document)
                        : "{}";
                }
                else
                {
                    json = SerializeSimpleParameters(element);
                }

                var hash = ComputeHashFromString(json);

                var request = new SystemTypePublishRequest
                {
                    RoleId = roleId,
                    TypeName = systemType.TypeName,
                    Category = systemType.Category,
                    SystemFamily = systemType.SystemFamily,
                    Group = (int)systemType.Group,
                    Json = json,
                    Hash = hash
                };

                return CallPublishApi(request);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Publishes multiple system types to the backend API.
        /// </summary>
        /// <param name="systemTypes">List of system types to publish.</param>
        /// <param name="document">The Revit document.</param>
        /// <param name="roleId">The role ID to associate with these types.</param>
        /// <returns>Count of successfully published types.</returns>
        public int PublishMany(List<SystemTypeInfo> systemTypes, Document document, Guid roleId)
        {
            if (systemTypes == null || systemTypes.Count == 0)
                return 0;

            int published = 0;
            foreach (var type in systemTypes)
            {
                if (Publish(type, document, roleId))
                {
                    published++;
                }
            }
            return published;
        }

        /// <summary>
        /// Gets the compound structure from a host object element type.
        /// Returns null if element does not have compound structure.
        /// </summary>
        private CompoundStructure GetCompoundStructure(Element element)
        {
            if (element is HostObjAttributes hostAttrs)
            {
                return hostAttrs.GetCompoundStructure();
            }
            return null;
        }

        /// <summary>
        /// Serializes element parameters to JSON for GroupE types (Levels, Grids, etc.).
        /// </summary>
        private string SerializeSimpleParameters(Element element)
        {
            var parameters = element.Parameters
                .Cast<Parameter>()
                .Select(p => new ParameterData
                {
                    Name = p.Definition?.Name ?? "",
                    Value = GetParameterValueAsString(p),
                    StorageType = p.StorageType.ToString()
                })
                .ToList();

            return JsonConvert.SerializeObject(new { Parameters = parameters }, Formatting.Indented);
        }

        /// <summary>
        /// Gets parameter value as string based on storage type.
        /// </summary>
        private string GetParameterValueAsString(Parameter param)
        {
            if (param == null)
                return "";

            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString() ?? "";
                case StorageType.Double:
                    return param.AsDouble().ToString();
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.ElementId:
                    return GetElementIdValue(param.AsElementId()).ToString();
                default:
                    return "";
            }
        }

        /// <summary>
        /// Computes SHA256 hash from a string content.
        /// </summary>
        private string ComputeHashFromString(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Calls the backend API to publish the system type.
        /// MVP: Synchronous HTTP call for simplicity.
        /// </summary>
        private bool CallPublishApi(SystemTypePublishRequest request)
        {
            try
            {
                return RetryHelper.ExecuteWithRetryAsync(() =>
                {
                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return ExecutePostRequestAsync(_apiBaseUrl + "/system-types", content);
                }).Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a POST request and returns success status.
        /// </summary>
        private async Task<bool> ExecutePostRequestAsync(string url, StringContent content)
        {
            var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API returned {(int)response.StatusCode} ({response.StatusCode})");
            }
            return true;
        }

        /// <summary>
        /// Gets the integer value of an ElementId in a version-compatible way.
        /// Revit 2024+ uses Value property, earlier versions use IntegerValue.
        /// </summary>
        private static int GetElementIdValue(ElementId elementId)
        {
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return -1;

#if REVIT2024 || REVIT2025 || REVIT2026
            return (int)elementId.Value;
#else
            return elementId.IntegerValue;
#endif
        }
    }

    /// <summary>
    /// Request model for publishing a system type to the backend API.
    /// </summary>
    public class SystemTypePublishRequest
    {
        public Guid RoleId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SystemFamily { get; set; } = string.Empty;
        public int Group { get; set; }
        public string Json { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for parameter serialization in GroupE types.
    /// </summary>
    public class ParameterData
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string StorageType { get; set; } = string.Empty;
    }
}
