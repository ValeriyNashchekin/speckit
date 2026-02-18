using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Services;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services
{
    public class MaterialMappingIntegration
    {
        private readonly MaterialMappingClient _mappingClient;
        private readonly IWebViewBridge _webViewBridge;
        private readonly Dictionary<string, string> _sessionMappings;
        private TaskCompletionSource<MaterialMappingDecision> _mappingDecisionTcs;
        private static readonly TimeSpan DecisionTimeout = TimeSpan.FromMinutes(5);

        public Guid ProjectId { get; set; }

        public MaterialMappingIntegration(IWebViewBridge? webViewBridge = null, string apiBaseUrl = "https://localhost:5001/api")
        {
            _mappingClient = new MaterialMappingClient(apiBaseUrl);
            _webViewBridge = webViewBridge;
            _sessionMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> LookupMaterialAsync(
            Document document,
            string templateMaterialName,
            string systemTypeId,
            string systemTypeName,
            int? layerIndex = null)
        {
            if (string.IsNullOrEmpty(templateMaterialName))
                return templateMaterialName;

            if (_sessionMappings.TryGetValue(templateMaterialName, out var cachedMapping))
                return cachedMapping;

            if (ProjectId != Guid.Empty)
            {
                var apiResult = await _mappingClient.LookupAsync(ProjectId, templateMaterialName).ConfigureAwait(false);
                if (apiResult != null)
                {
                    _sessionMappings[templateMaterialName] = apiResult.ProjectMaterialName;
                    return apiResult.ProjectMaterialName;
                }
            }

            if (MaterialExistsInDocument(document, templateMaterialName))
                return templateMaterialName;

            var decision = await SendFallbackEventAsync(document, templateMaterialName, systemTypeId, systemTypeName, layerIndex).ConfigureAwait(false);

            if (decision != null)
            {
                _sessionMappings[templateMaterialName] = decision.ProjectMaterialName;
                return decision.ProjectMaterialName;
            }

            return templateMaterialName;
        }

        private async Task<MaterialMappingDecision> SendFallbackEventAsync(Document document, string templateMaterialName, string systemTypeId, string systemTypeName, int? layerIndex)
        {
            if (_webViewBridge == null)
                return null;

            var options = BuildMaterialOptions(document, templateMaterialName);
            var fallbackEvent = new MaterialFallbackEvent
            {
                SystemTypeId = systemTypeId,
                SystemTypeName = systemTypeName,
                MissingMaterial = new MissingMaterialInfo { TemplateMaterialName = templateMaterialName, LayerIndex = layerIndex },
                AvailableOptions = options
            };

            _webViewBridge.SendEvent("revit:material:fallback", fallbackEvent);
            _mappingDecisionTcs = new TaskCompletionSource<MaterialMappingDecision>();

            using (var cts = new System.Threading.CancellationTokenSource(DecisionTimeout))
            {
                cts.Token.Register(() => _mappingDecisionTcs?.TrySetResult(null));
                try { return await _mappingDecisionTcs.Task.ConfigureAwait(false); }
                finally { _mappingDecisionTcs = null; }
            }
        }

        public async Task HandleMappingSaveEventAsync(MaterialMappingSaveEvent eventPayload)
        {
            if (eventPayload == null) return;

            Guid projectId;
            if (!Guid.TryParse(eventPayload.ProjectId, out projectId)) projectId = ProjectId;

            var request = new CreateMaterialMappingRequest
            {
                ProjectId = projectId,
                TemplateMaterialName = eventPayload.TemplateMaterialName,
                ProjectMaterialName = eventPayload.ProjectMaterialName
            };

            await _mappingClient.SaveMappingAsync(request).ConfigureAwait(false);
            _sessionMappings[eventPayload.TemplateMaterialName] = eventPayload.ProjectMaterialName;

            if (eventPayload.ApplyToCurrent && _mappingDecisionTcs != null)
            {
                _mappingDecisionTcs.TrySetResult(new MaterialMappingDecision
                {
                    TemplateMaterialName = eventPayload.TemplateMaterialName,
                    ProjectMaterialName = eventPayload.ProjectMaterialName
                });
            }
        }

        private bool MaterialExistsInDocument(Document document, string materialName)
        {
            if (document == null || string.IsNullOrEmpty(materialName)) return false;
            var materials = new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>();
            return materials.Any(m => m.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase));
        }

        private List<MaterialOption> BuildMaterialOptions(Document document, string templateMaterialName)
        {
            var options = new List<MaterialOption>();
            if (document != null)
            {
                var materials = new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>().OrderBy(m => m.Name);
                foreach (var material in materials)
                    options.Add(new MaterialOption { Id = material.Id.ToString(), Name = material.Name, OptionType = "existing" });
            }
            options.Add(new MaterialOption { Id = "create", Name = "Create '" + templateMaterialName + "'", OptionType = "create" });
            options.Add(new MaterialOption { Id = "default", Name = "Use Default Material", OptionType = "default" });
            options.Add(new MaterialOption { Id = "skip", Name = "Skip Layer", OptionType = "skip" });
            return options;
        }

        public void ClearSessionCache() { _sessionMappings.Clear(); }
    }

    public class MaterialMappingDecision
    {
        public string TemplateMaterialName { get; set; } = string.Empty;
        public string ProjectMaterialName { get; set; } = string.Empty;
        public bool CreateNew { get; set; }
        public bool Skip { get; set; }
    }
}
