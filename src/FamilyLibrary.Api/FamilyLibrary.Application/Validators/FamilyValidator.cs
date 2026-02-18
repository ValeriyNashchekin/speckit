using FluentValidation;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Business-rule validator for CreateFamilyDto.
/// Validates business rules that require database access (e.g., RoleId existence).
/// This validator complements CreateFamilyDtoValidator which handles structural validation.
/// </summary>
public class CreateFamilyBusinessValidator : AbstractValidator<CreateFamilyDto>
{
    private static readonly Regex FamilyNameRegex = new(@"^[A-Za-z0-9_\-]+$", RegexOptions.Compiled);

    public CreateFamilyBusinessValidator(IFamilyRoleRepository roleRepository)
    {
        // Validate RoleId exists in database
        RuleFor(x => x.RoleId)
            .MustAsync(async (roleId, ct) =>
            {
                if (roleId == Guid.Empty)
                    return false;

                return await roleRepository.ExistsAsync(roleId, ct);
            })
            .WithMessage("The specified role does not exist")
            .When(x => x.RoleId != Guid.Empty);

        // Validate FamilyName format (alphanumeric + underscores/hyphens)
        RuleFor(x => x.FamilyName)
            .Must(name => FamilyNameRegex.IsMatch(name))
            .WithMessage("FamilyName must contain only alphanumeric characters, underscores, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.FamilyName));
    }
}

/// <summary>
/// Business-rule validator for UpdateFamilyDto.
/// Validates business rules that require database access (e.g., RoleId existence).
/// This validator complements UpdateFamilyDtoValidator which handles structural validation.
/// </summary>
public class UpdateFamilyBusinessValidator : AbstractValidator<UpdateFamilyDto>
{
    private static readonly Regex FamilyNameRegex = new(@"^[A-Za-z0-9_\-]+$", RegexOptions.Compiled);

    public UpdateFamilyBusinessValidator(IFamilyRoleRepository roleRepository)
    {
        // Validate RoleId exists in database when provided
        RuleFor(x => x.RoleId)
            .MustAsync(async (roleId, ct) =>
            {
                if (!roleId.HasValue || roleId.Value == Guid.Empty)
                    return false;

                return await roleRepository.ExistsAsync(roleId.Value, ct);
            })
            .WithMessage("The specified role does not exist")
            .When(x => x.RoleId.HasValue && x.RoleId != Guid.Empty);

        // Validate FamilyName format when provided
        RuleFor(x => x.FamilyName)
            .Must(name => FamilyNameRegex.IsMatch(name!))
            .WithMessage("FamilyName must contain only alphanumeric characters, underscores, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.FamilyName));
    }
}
