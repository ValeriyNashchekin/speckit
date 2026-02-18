using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Enums;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Validates dependencies before applying system types during Pull Update.
/// Checks that required baluster families, curtain panels, mullions, and child WallTypes exist.
/// </summary>
public class DependencyValidationService
{
    private readonly RailingSerializer _railingSerializer;
    private readonly CurtainSerializer _curtainSerializer;
    private readonly StackedWallSerializer _stackedWallSerializer;

    public DependencyValidationService()
        : this(new RailingSerializer(), new CurtainSerializer(), new StackedWallSerializer())
    {
    }

    public DependencyValidationService(
        RailingSerializer railingSerializer,
        CurtainSerializer curtainSerializer,
        StackedWallSerializer stackedWallSerializer)
    {
        _railingSerializer = railingSerializer
            ?? throw new ArgumentNullException(nameof(railingSerializer));
        _curtainSerializer = curtainSerializer
            ?? throw new ArgumentNullException(nameof(curtainSerializer));
        _stackedWallSerializer = stackedWallSerializer
            ?? throw new ArgumentNullException(nameof(stackedWallSerializer));
    }

    /// <summary>
    /// Validates dependencies for a system type before Pull Update.
    /// Returns validation result with any missing dependencies.
    /// </summary>
    public DependencyValidationResult Validate(SystemTypePullDto dto, Document document)
    {
        var result = new DependencyValidationResult
        {
            TypeName = dto.TypeName,
            Group = (SystemFamilyGroup)dto.Group
        };

        if (document == null || string.IsNullOrEmpty(dto.Json))
        {
            result.IsValid = true;
            return result;
        }

        switch ((SystemFamilyGroup)dto.Group)
        {
            case SystemFamilyGroup.GroupC:
                ValidateGroupC(dto, document, result);
                break;

            case SystemFamilyGroup.GroupD:
                ValidateGroupD(dto, document, result);
                break;
        }

        return result;
    }

    /// <summary>
    /// Validates Group C dependencies (RailingType balusters).
    /// </summary>
    private void ValidateGroupC(SystemTypePullDto dto, Document document, DependencyValidationResult result)
    {
        try
        {
            var railingJson = JsonConvert.DeserializeObject<RailingStructureJson>(dto.Json);
            if (railingJson == null)
            {
                result.IsValid = true;
                return;
            }

            var missing = _railingSerializer.ValidateDependencies(railingJson, document);

            if (missing.Count > 0)
            {
                result.IsValid = false;
                result.MissingDependencies.AddRange(missing);
                result.ErrorMessage = FormatMissingDependenciesMessage("baluster families", missing);
                result.Suggestion = "Load the required baluster families from the library before applying this railing type.";
            }
            else
            {
                result.IsValid = true;
            }
        }
        catch (Exception ex)
        {
            result.IsValid = true; // Allow on parse error
            result.Warnings.Add("Could not validate dependencies: " + ex.Message);
        }
    }

    /// <summary>
    /// Validates Group D dependencies (Curtain panels/mullions, Stacked wall children).
    /// </summary>
    private void ValidateGroupD(SystemTypePullDto dto, Document document, DependencyValidationResult result)
    {
        try
        {
            // Try parsing as CurtainWall
            var curtainJson = JsonConvert.DeserializeObject<CurtainWallJson>(dto.Json);
            if (curtainJson != null && "Curtain".Equals(curtainJson.Kind, StringComparison.OrdinalIgnoreCase))
            {
                ValidateCurtainWall(curtainJson, document, result);
                return;
            }

            // Try parsing as StackedWall
            var stackedJson = JsonConvert.DeserializeObject<StackedWallJson>(dto.Json);
            if (stackedJson != null && "Stacked".Equals(stackedJson.Kind, StringComparison.OrdinalIgnoreCase))
            {
                ValidateStackedWall(stackedJson, document, result);
                return;
            }

            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.IsValid = true; // Allow on parse error
            result.Warnings.Add("Could not validate dependencies: " + ex.Message);
        }
    }

    /// <summary>
    /// Validates CurtainWall dependencies (panels and mullions).
    /// </summary>
    private void ValidateCurtainWall(CurtainWallJson curtainJson, Document document, DependencyValidationResult result)
    {
        var missing = _curtainSerializer.ValidateDependencies(curtainJson, document);

        if (missing.Count > 0)
        {
            result.IsValid = false;
            result.MissingDependencies.AddRange(missing);
            result.ErrorMessage = FormatMissingDependenciesMessage("curtain panels or mullions", missing);
            result.Suggestion = "Load the required curtain panels and mullions from the library before applying this curtain wall type.";
        }
        else
        {
            result.IsValid = true;
        }
    }

    /// <summary>
    /// Validates StackedWall dependencies (child WallTypes).
    /// </summary>
    private void ValidateStackedWall(StackedWallJson stackedJson, Document document, DependencyValidationResult result)
    {
        var missing = _stackedWallSerializer.GetMissingChildWallTypes(stackedJson, document);

        if (missing.Count > 0)
        {
            result.IsValid = false;
            result.MissingDependencies.AddRange(missing);
            result.ErrorMessage = FormatMissingStackedWallMessage(missing);
            result.Suggestion = FormatStackedWallSuggestion(missing);
        }
        else
        {
            result.IsValid = true;
        }
    }

    /// <summary>
    /// Validates dependencies for multiple system types.
    /// Returns combined result with all missing dependencies.
    /// </summary>
    public List<DependencyValidationResult> ValidateMany(List<SystemTypePullDto> dtos, Document document)
    {
        var results = new List<DependencyValidationResult>();

        if (dtos == null || document == null)
            return results;

        foreach (var dto in dtos)
        {
            var result = Validate(dto, document);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Checks if any of the validation results have missing dependencies.
    /// </summary>
    public bool HasMissingDependencies(List<DependencyValidationResult> results)
    {
        return results.Any(r => !r.IsValid && r.MissingDependencies.Count > 0);
    }

    /// <summary>
    /// Gets all unique missing dependencies from validation results.
    /// </summary>
    public List<string> GetAllMissingDependencies(List<DependencyValidationResult> results)
    {
        return results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.MissingDependencies)
            .Distinct()
            .ToList();
    }

    private static string FormatMissingDependenciesMessage(string dependencyType, List<string> missing)
    {
        if (missing.Count == 0)
            return string.Empty;

        if (missing.Count == 1)
        {
            return "Missing " + dependencyType + ": " + missing[0];
        }

        return "Missing " + dependencyType + ": " + string.Join(", ", missing.Take(5)) +
               (missing.Count > 5 ? " and " + (missing.Count - 5) + " more" : string.Empty);
    }

    private static string FormatMissingStackedWallMessage(List<string> missing)
    {
        if (missing.Count == 0)
            return string.Empty;

        if (missing.Count == 1)
        {
            return "Child wall type not found: " + missing[0];
        }

        return "Child wall types not found: " + string.Join(", ", missing);
    }

    private static string FormatStackedWallSuggestion(List<string> missing)
    {
        var inLibrary = missing.Where(m => !m.Contains("[not in library]")).ToList();

        if (inLibrary.Count > 0)
        {
            return "Load the following wall types from the library: " + string.Join(", ", inLibrary);
        }

        return "Create the missing child wall types in the project or load them from an external source.";
    }
}

/// <summary>
/// Result of dependency validation.
/// </summary>
public class DependencyValidationResult
{
    public string TypeName { get; set; } = string.Empty;
    public SystemFamilyGroup Group { get; set; }
    public bool IsValid { get; set; } = true;
    public List<string> MissingDependencies { get; set; } = new List<string>();
    public string ErrorMessage { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new List<string>();
}
