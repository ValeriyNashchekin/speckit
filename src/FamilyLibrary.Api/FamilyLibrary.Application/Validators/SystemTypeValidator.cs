using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Interfaces;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateSystemTypeDto.
/// Handles structural validation (format, length, required fields).
/// For business-rule validation (database existence checks), use CreateSystemTypeBusinessValidator.
/// </summary>
public class CreateSystemTypeDtoValidator : AbstractValidator<CreateSystemTypeDto>
{
    private static readonly Regex Sha256Regex = new(@"^[a-fA-F0-9]{64}$", RegexOptions.Compiled);

    public CreateSystemTypeDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("RoleId is required and must be a valid GUID");

        RuleFor(x => x.TypeName)
            .NotEmpty()
                .WithMessage("TypeName is required")
            .MaximumLength(200)
                .WithMessage("TypeName must not exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
                .WithMessage("Category is required")
            .MaximumLength(100)
                .WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.SystemFamily)
            .NotEmpty()
                .WithMessage("SystemFamily is required")
            .MaximumLength(100)
                .WithMessage("SystemFamily must not exceed 100 characters");

        RuleFor(x => x.Json)
            .NotEmpty()
                .WithMessage("Json is required")
            .Must(BeValidJson)
                .WithMessage("Json must be a valid JSON string");

        // GroupB-specific validation for RoutingPreferences
        RuleFor(x => x.Json)
            .Must((dto, json) => BeValidRoutingPreferencesJson(json, dto.Group))
                .WithMessage("Json must contain valid RoutingPreferences structure for GroupB (MEP) types")
            .When(x => x.Group == SystemFamilyGroup.GroupB);

        RuleFor(x => x.Hash)
            .NotEmpty()
                .WithMessage("Hash is required")
            .Must(hash => Sha256Regex.IsMatch(hash))
                .WithMessage("Hash must be a valid SHA256 hash (64 hexadecimal characters)");
    }

    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates RoutingPreferences JSON structure for GroupB (MEP) types.
    /// Required structure:
    /// - preferredJunctionType: non-empty string (e.g., "Tee", "Tap")
    /// - segments, elbows, junctions, etc.: arrays of routing rules (can be empty)
    /// </summary>
    private static bool BeValidRoutingPreferencesJson(string json, SystemFamilyGroup group)
    {
        if (group != SystemFamilyGroup.GroupB)
            return true;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // preferredJunctionType is required and must be non-empty
            if (!root.TryGetProperty("preferredJunctionType", out var junctionTypeProp))
                return false;

            if (junctionTypeProp.ValueKind != JsonValueKind.String)
                return false;

            var junctionType = junctionTypeProp.GetString();
            if (string.IsNullOrWhiteSpace(junctionType))
                return false;

            // Validate routing rule arrays exist and are arrays (can be empty)
            var routingRuleArrays = new[]
            {
                "segments", "elbows", "junctions", "transitions",
                "crosses", "unions", "mechanicalJoints", "caps",
                "transitionsRectangularToRound", "transitionsRectangularToOval", "transitionsOvalToRound"
            };

            foreach (var arrayName in routingRuleArrays)
            {
                if (root.TryGetProperty(arrayName, out var arrayProp))
                {
                    if (arrayProp.ValueKind != JsonValueKind.Array)
                        return false;

                    // Validate each routing rule in the array
                    foreach (var rule in arrayProp.EnumerateArray())
                    {
                        if (!IsValidRoutingRule(rule))
                            return false;
                    }
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a single routing rule structure.
    /// Required structure:
    /// - mepPartId: integer (can be -1 for invalid)
    /// - mepPartName: string (optional)
    /// - criteria: array of routing criteria (optional)
    /// </summary>
    private static bool IsValidRoutingRule(JsonElement rule)
    {
        if (rule.ValueKind != JsonValueKind.Object)
            return false;

        // mepPartId should be an integer (defaults to -1 if invalid)
        if (rule.TryGetProperty("mepPartId", out var mepPartIdProp))
        {
            if (mepPartIdProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        // criteria should be an array if present
        if (rule.TryGetProperty("criteria", out var criteriaProp))
        {
            if (criteriaProp.ValueKind != JsonValueKind.Array)
                return false;

            // Validate each criterion
            foreach (var criterion in criteriaProp.EnumerateArray())
            {
                if (!IsValidRoutingCriterion(criterion))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates a single routing criterion structure.
    /// Required structure:
    /// - type: non-empty string (e.g., "PrimarySizeCriterion")
    /// - minimumSize, maximumSize: numbers (optional, for size-based criteria)
    /// </summary>
    private static bool IsValidRoutingCriterion(JsonElement criterion)
    {
        if (criterion.ValueKind != JsonValueKind.Object)
            return false;

        // type is required and must be a non-empty string
        if (!criterion.TryGetProperty("type", out var typeProp))
            return false;

        if (typeProp.ValueKind != JsonValueKind.String)
            return false;

        var type = typeProp.GetString();
        if (string.IsNullOrWhiteSpace(type))
            return false;

        // minimumSize and maximumSize should be numbers if present
        if (criterion.TryGetProperty("minimumSize", out var minSizeProp))
        {
            if (minSizeProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        if (criterion.TryGetProperty("maximumSize", out var maxSizeProp))
        {
            if (maxSizeProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        return true;
    }
}

/// <summary>
/// Business-rule validator for CreateSystemTypeDto.
/// Validates business rules that require database access (e.g., RoleId existence).
/// This validator complements CreateSystemTypeDtoValidator which handles structural validation.
/// </summary>
public class CreateSystemTypeBusinessValidator : AbstractValidator<CreateSystemTypeDto>
{
    public CreateSystemTypeBusinessValidator(IFamilyRoleRepository familyRoleRepository)
    {
        RuleFor(x => x.RoleId)
            .MustAsync(async (roleId, ct) =>
            {
                if (roleId == Guid.Empty)
                    return false;

                return await familyRoleRepository.ExistsAsync(roleId, ct);
            })
            .WithMessage("The specified RoleId does not exist");
    }
}

/// <summary>
/// Validator for UpdateSystemTypeDto.
/// Handles structural validation (format, length, required fields).
/// </summary>
public class UpdateSystemTypeDtoValidator : AbstractValidator<UpdateSystemTypeDto>
{
    private static readonly Regex Sha256Regex = new(@"^[a-fA-F0-9]{64}$", RegexOptions.Compiled);

    public UpdateSystemTypeDtoValidator()
    {
        RuleFor(x => x.Json)
            .NotEmpty()
                .WithMessage("Json is required")
            .Must(BeValidJson)
                .WithMessage("Json must be a valid JSON string");

        RuleFor(x => x.Hash)
            .NotEmpty()
                .WithMessage("Hash is required")
            .Must(hash => Sha256Regex.IsMatch(hash))
                .WithMessage("Hash must be a valid SHA256 hash (64 hexadecimal characters)");
    }

    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

/// <summary>
/// Business-rule validator for UpdateSystemTypeDto.
/// Validates RoutingPreferences JSON structure for GroupB (MEP) types.
/// Requires database access to determine the SystemType's group.
/// </summary>
public class UpdateSystemTypeBusinessValidator : AbstractValidator<(Guid Id, UpdateSystemTypeDto Dto)>
{
    private readonly ISystemTypeRepository _systemTypeRepository;

    public UpdateSystemTypeBusinessValidator(ISystemTypeRepository systemTypeRepository)
    {
        _systemTypeRepository = systemTypeRepository;

        RuleFor(x => x.Dto.Json)
            .MustAsync(async (idAndDto, json, ct) =>
            {
                var entity = await _systemTypeRepository.GetByIdAsync(idAndDto.Id, ct);
                if (entity is null)
                    return false; // Entity not found - will be caught by service

                return BeValidRoutingPreferencesJson(json, entity.Group);
            })
                .WithMessage("Json must contain valid RoutingPreferences structure for GroupB (MEP) types");
    }

    /// <summary>
    /// Validates RoutingPreferences JSON structure for GroupB (MEP) types.
    /// </summary>
    private static bool BeValidRoutingPreferencesJson(string json, SystemFamilyGroup group)
    {
        if (group != SystemFamilyGroup.GroupB)
            return true;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // preferredJunctionType is required and must be non-empty
            if (!root.TryGetProperty("preferredJunctionType", out var junctionTypeProp))
                return false;

            if (junctionTypeProp.ValueKind != JsonValueKind.String)
                return false;

            var junctionType = junctionTypeProp.GetString();
            if (string.IsNullOrWhiteSpace(junctionType))
                return false;

            // Validate routing rule arrays exist and are arrays (can be empty)
            var routingRuleArrays = new[]
            {
                "segments", "elbows", "junctions", "transitions",
                "crosses", "unions", "mechanicalJoints", "caps",
                "transitionsRectangularToRound", "transitionsRectangularToOval", "transitionsOvalToRound"
            };

            foreach (var arrayName in routingRuleArrays)
            {
                if (root.TryGetProperty(arrayName, out var arrayProp))
                {
                    if (arrayProp.ValueKind != JsonValueKind.Array)
                        return false;

                    foreach (var rule in arrayProp.EnumerateArray())
                    {
                        if (!IsValidRoutingRule(rule))
                            return false;
                    }
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsValidRoutingRule(JsonElement rule)
    {
        if (rule.ValueKind != JsonValueKind.Object)
            return false;

        if (rule.TryGetProperty("mepPartId", out var mepPartIdProp))
        {
            if (mepPartIdProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        if (rule.TryGetProperty("criteria", out var criteriaProp))
        {
            if (criteriaProp.ValueKind != JsonValueKind.Array)
                return false;

            foreach (var criterion in criteriaProp.EnumerateArray())
            {
                if (!IsValidRoutingCriterion(criterion))
                    return false;
            }
        }

        return true;
    }

    private static bool IsValidRoutingCriterion(JsonElement criterion)
    {
        if (criterion.ValueKind != JsonValueKind.Object)
            return false;

        if (!criterion.TryGetProperty("type", out var typeProp))
            return false;

        if (typeProp.ValueKind != JsonValueKind.String)
            return false;

        var type = typeProp.GetString();
        if (string.IsNullOrWhiteSpace(type))
            return false;

        if (criterion.TryGetProperty("minimumSize", out var minSizeProp))
        {
            if (minSizeProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        if (criterion.TryGetProperty("maximumSize", out var maxSizeProp))
        {
            if (maxSizeProp.ValueKind != JsonValueKind.Number)
                return false;
        }

        return true;
    }
}
