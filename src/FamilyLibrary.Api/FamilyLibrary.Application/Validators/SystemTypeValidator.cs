using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FamilyLibrary.Application.DTOs;
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
