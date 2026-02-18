using FluentValidation;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Interfaces;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Business-rule validator for CreateFamilyRoleDto.
/// Validates business rules that require database access (e.g., CategoryId existence).
/// This validator complements CreateFamilyRoleDtoValidator which handles structural validation.
/// </summary>
public class CreateFamilyRoleBusinessValidator : AbstractValidator<CreateFamilyRoleDto>
{
    public CreateFamilyRoleBusinessValidator(ICategoryRepository categoryRepository)
    {
        // Validate CategoryId exists when provided
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, ct) =>
            {
                if (categoryId is null || categoryId == Guid.Empty)
                    return true; // Null/Empty is valid (optional field)

                return await categoryRepository.ExistsAsync(categoryId.Value, ct);
            })
            .WithMessage("The specified category does not exist")
            .When(x => x.CategoryId.HasValue && x.CategoryId != Guid.Empty);
    }
}

/// <summary>
/// Business-rule validator for UpdateFamilyRoleDto.
/// Validates business rules that require database access (e.g., CategoryId existence).
/// This validator complements UpdateFamilyRoleDtoValidator which handles structural validation.
/// </summary>
public class UpdateFamilyRoleBusinessValidator : AbstractValidator<UpdateFamilyRoleDto>
{
    public UpdateFamilyRoleBusinessValidator(ICategoryRepository categoryRepository)
    {
        // Validate CategoryId exists when provided
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, ct) =>
            {
                if (categoryId is null || categoryId == Guid.Empty)
                    return true; // Null/Empty is valid (optional field)

                return await categoryRepository.ExistsAsync(categoryId.Value, ct);
            })
            .WithMessage("The specified category does not exist")
            .When(x => x.CategoryId.HasValue && x.CategoryId != Guid.Empty);
    }
}
