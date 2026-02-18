using FluentValidation;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Business-rule validator for CreateRecognitionRuleDto.
/// Validates business rules that require database access (e.g., RoleId existence).
/// This validator complements CreateRecognitionRuleDtoValidator which handles structural validation.
/// </summary>
public class CreateRecognitionRuleBusinessValidator : AbstractValidator<CreateRecognitionRuleDto>
{
    private static readonly Regex OperandRegex = new(@"^[A-Za-z][A-Za-z0-9_\-]*$", RegexOptions.Compiled);

    public CreateRecognitionRuleBusinessValidator(IFamilyRoleRepository roleRepository)
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
    }
}

/// <summary>
/// Business-rule validator for UpdateRecognitionRuleDto.
/// Validates business rules that require database access (e.g., RoleId existence).
/// This validator complements UpdateRecognitionRuleDtoValidator which handles structural validation.
/// </summary>
public class UpdateRecognitionRuleBusinessValidator : AbstractValidator<UpdateRecognitionRuleDto>
{
    public UpdateRecognitionRuleBusinessValidator(IFamilyRoleRepository roleRepository)
    {
        // Validate RoleId exists in database when provided
        RuleFor(x => x.RoleId)
            .MustAsync(async (roleId, ct) =>
            {
                if (roleId == Guid.Empty)
                    return false;

                return await roleRepository.ExistsAsync(roleId, ct);
            })
            .WithMessage("The specified role does not exist")
            .When(x => x.RoleId != Guid.Empty);
    }
}
