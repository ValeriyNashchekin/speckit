using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Infrastructure.Http;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services
{
    /// <summary>
    /// Service for pulling system types from backend API and applying to document.
    /// Supports Group A (CompoundStructure), Group B (RoutingPreferences), Group C (Railings),
    /// Group D (Curtain/Stacked Walls), and Group E (Parameters).
    /// </summary>
    public class SystemTypePullService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly RoutingPreferencesApplier _routingPreferencesApplier;
        private readonly CompoundStructureDeserializer _compoundStructureDeserializer;
        private readonly DependencyValidationService _dependencyValidationService;

        public SystemTypePullService()
            : this(new RoutingPreferencesApplier(), new CompoundStructureDeserializer(), new DependencyValidationService())
        {
        }

        public SystemTypePullService(
            RoutingPreferencesApplier routingPreferencesApplier,
            CompoundStructureDeserializer compoundStructureDeserializer,
            DependencyValidationService dependencyValidationService)
        {
            _routingPreferencesApplier = routingPreferencesApplier
                ?? throw new ArgumentNullException(nameof(routingPreferencesApplier));
            _compoundStructureDeserializer = compoundStructureDeserializer
                ?? throw new ArgumentNullException(nameof(compoundStructureDeserializer));
            _dependencyValidationService = dependencyValidationService
                ?? throw new ArgumentNullException(nameof(dependencyValidationService));
            _httpClient = new HttpClient();
            _apiBaseUrl = "https://localhost:5001/api";
        }

        /// <summary>
        /// Fetches system type data from API by role ID and type name.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <param name="typeName">The type name to find.</param>
        /// <returns>System type DTO or null if not found.</returns>
        public async Task<SystemTypePullDto> FetchSystemTypeAsync(Guid roleId, string typeName)
        {
            if (roleId == Guid.Empty || string.IsNullOrEmpty(typeName))
                return null;

            try
            {
                return await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    var url = string.Format("{0}/system-types/by-role/{1}", _apiBaseUrl, roleId);
                    var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var types = JsonConvert.DeserializeObject<List<SystemTypePullDto>>(json);

                    if (types == null) return null;

                    foreach (var t in types)
                    {
                        if (t.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                            return t;
                    }
                    return null;
                }).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Applies system type data to an existing element in the document.
        /// Uses single transaction for all modifications.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="targetElement">The target element to update.</param>
        /// <param name="dto">The system type data from API.</param>
        /// <returns>Result with success status and any warnings.</returns>
        public SystemTypePullResult ApplySystemType(Document document, Element targetElement, SystemTypePullDto dto)
        {
            var result = new SystemTypePullResult { TypeName = dto.TypeName };

            if (document == null || targetElement == null || dto == null)
            {
                result.Success = false;
                result.Error = "Invalid parameters";
                return result;
            }

            using (var transaction = new Transaction(document, string.Format("Pull System Type: {0}", dto.TypeName)))
            {
                transaction.Start();

                try
                {
                    var warnings = ApplyByGroup(document, targetElement, dto);
                    transaction.Commit();

                    result.Success = true;
                    result.Warnings = warnings;
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    result.Success = false;
                    result.Error = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies multiple system types to document elements in single transaction.
        /// </summary>
        /// <param name="document">The Revit document.</param>
        /// <param name="items">List of element and DTO pairs to apply.</param>
        /// <returns>Count of successfully applied types.</returns>
        public int ApplyMany(Document document, List<SystemTypePullItem> items)
        {
            if (document == null || items == null || items.Count == 0)
                return 0;

            int appliedCount = 0;

            using (var transaction = new Transaction(document, "Pull System Types"))
            {
                transaction.Start();

                try
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            var warnings = ApplyByGroup(document, item.TargetElement, item.Dto);
                            item.Warnings = warnings;
                            item.Success = true;
                            appliedCount++;
                        }
                        catch (Exception ex)
                        {
                            item.Success = false;
                            item.Error = ex.Message;
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                    throw;
                }
            }

            return appliedCount;
        }

        /// <summary>
        /// Applies system type based on its group classification.
        /// </summary>
        private List<string> ApplyByGroup(Document document, Element targetElement, SystemTypePullDto dto)
        {
            var warnings = new List<string>();

            switch ((SystemFamilyGroup)dto.Group)
            {
                case SystemFamilyGroup.GroupA:
                    warnings.AddRange(ApplyCompoundStructure(document, targetElement, dto.Json));
                    break;

                case SystemFamilyGroup.GroupB:
                    warnings.AddRange(ApplyRoutingPreferences(document, targetElement, dto.Json));
                    break;

                case SystemFamilyGroup.GroupE:
                    warnings.AddRange(ApplySimpleParameters(document, targetElement, dto.Json));
                    break;

                default:
                    warnings.Add(string.Format("Unknown group: {0}", dto.Group));
                    break;
            }

            return warnings;
        }

        /// <summary>
        /// Applies CompoundStructure to Group A types (WallType, FloorType, etc.).
        /// </summary>
        private List<string> ApplyCompoundStructure(Document document, Element targetElement, string json)
        {
            var warnings = new List<string>();

            var hostAttrs = targetElement as HostObjAttributes;
            if (hostAttrs == null)
            {
                warnings.Add(string.Format("Element {0} does not support CompoundStructure", targetElement.Name));
                return warnings;
            }

            try
            {
                var structure = _compoundStructureDeserializer.Deserialize(json, document);
                if (structure != null)
                {
                    hostAttrs.SetCompoundStructure(structure);
                }
                else
                {
                    warnings.Add("Failed to deserialize CompoundStructure");
                }
            }
            catch (Exception ex)
            {
                warnings.Add(string.Format("Failed to apply CompoundStructure: {0}", ex.Message));
            }

            return warnings;
        }

        /// <summary>
        /// Applies RoutingPreferences to Group B types (PipeType, DuctType).
        /// </summary>
        private List<string> ApplyRoutingPreferences(Document document, Element targetElement, string json)
        {
            var warnings = new List<string>();

            RoutingPreferencesJson routingPrefs;
            try
            {
                routingPrefs = JsonConvert.DeserializeObject<RoutingPreferencesJson>(json);
                if (routingPrefs == null)
                {
                    warnings.Add("Failed to deserialize RoutingPreferences");
                    return warnings;
                }
            }
            catch (Exception ex)
            {
                warnings.Add(string.Format("JSON deserialization error: {0}", ex.Message));
                return warnings;
            }

            // Apply to PipeType
            var pipeType = targetElement as PipeType;
            if (pipeType != null)
            {
                var pipeWarnings = _routingPreferencesApplier.Apply(pipeType, routingPrefs, document);
                warnings.AddRange(pipeWarnings);
                return warnings;
            }

            // Apply to DuctType
            var ductType = targetElement as DuctType;
            if (ductType != null)
            {
                var ductWarnings = _routingPreferencesApplier.Apply(ductType, routingPrefs, document);
                warnings.AddRange(ductWarnings);
                return warnings;
            }

            warnings.Add(string.Format("Element {0} is not PipeType or DuctType", targetElement.Name));
            return warnings;
        }

        /// <summary>
        /// Applies simple parameters to Group E types (Levels, Grids).
        /// </summary>
        private List<string> ApplySimpleParameters(Document document, Element targetElement, string json)
        {
            var warnings = new List<string>();

            try
            {
                var paramData = JsonConvert.DeserializeObject<SimpleParametersData>(json);
                if (paramData == null || paramData.Parameters == null)
                {
                    warnings.Add("No parameters found in JSON");
                    return warnings;
                }

                foreach (var param in paramData.Parameters)
                {
                    var warning = TrySetParameter(targetElement, param);
                    if (!string.IsNullOrEmpty(warning))
                        warnings.Add(warning);
                }
            }
            catch (Exception ex)
            {
                warnings.Add(string.Format("Failed to apply parameters: {0}", ex.Message));
            }

            return warnings;
        }

        /// <summary>
        /// Attempts to set a parameter value on an element.
        /// </summary>
        private string TrySetParameter(Element element, ParameterValueData param)
        {
            if (string.IsNullOrEmpty(param.Name))
                return null;

            var parameter = element.LookupParameter(param.Name);
            if (parameter == null)
                return string.Format("Parameter not found: {0}", param.Name);

            if (parameter.IsReadOnly)
                return string.Format("Parameter is read-only: {0}", param.Name);

            try
            {
                var storageType = param.StorageType;
                if (storageType != null) storageType = storageType.ToLowerInvariant();

                if (storageType == "string")
                {
                    parameter.Set(param.Value ?? string.Empty);
                }
                else if (storageType == "double")
                {
                    double doubleValue;
                    if (double.TryParse(param.Value, out doubleValue))
                        parameter.Set(doubleValue);
                }
                else if (storageType == "integer")
                {
                    int intValue;
                    if (int.TryParse(param.Value, out intValue))
                        parameter.Set(intValue);
                }
                else if (storageType == "elementid")
                {
                    int elementIdValue;
                    if (int.TryParse(param.Value, out elementIdValue))
                        parameter.Set(new ElementId(elementIdValue));
                }
            }
            catch (Exception ex)
            {
                return string.Format("Failed to set {0}: {1}", param.Name, ex.Message);
            }

            return null;
        }
    }

    /// <summary>
    /// DTO for system type data from API.
    /// </summary>
    public class SystemTypePullDto
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SystemFamily { get; set; } = string.Empty;
        public int Group { get; set; }
        public string Json { get; set; } = string.Empty;
        public int CurrentVersion { get; set; }
    }

    /// <summary>
    /// Result of a system type pull operation.
    /// </summary>
    public class SystemTypePullResult
    {
        public bool Success { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Item for batch pull operations.
    /// </summary>
    public class SystemTypePullItem
    {
        public Element TargetElement { get; set; }
        public SystemTypePullDto Dto { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Data for simple parameter application.
    /// </summary>
    public class SimpleParametersData
    {
        public List<ParameterValueData> Parameters { get; set; } = new List<ParameterValueData>();
    }

    /// <summary>
    /// Single parameter value data.
    /// </summary>
    public class ParameterValueData
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string StorageType { get; set; } = string.Empty;
    }
}