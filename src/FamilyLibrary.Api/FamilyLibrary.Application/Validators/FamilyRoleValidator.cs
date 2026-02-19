using FluentValidation;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Interfaces;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Business-rule validator for CreateFamilyIdDto.
/// </summary>
public class CreateFamilyIdBusinessValidator : AbstractValidator<CreateFamilyIdDto>
{
    public CreateFamilyIdBusinessValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, ct) =>
            {
                if (categoryId is null || categoryId == Guid.Empty)
                    return true;

                return await categoryRepository.ExistsAsync(categoryId.Value, ct);
            })
            .WithMessage("The specified category does not exist")
            .When(x => x.CategoryId.HasValue && x.CategoryId != Guid.Empty);
    }
}

/// <summary>
/// Business-rule validator for UpdateFamilyIdDto.
/// </summary>
public class UpdateFamilyIdBusinessValidator : AbstractValidator<UpdateFamilyIdDto>
{
    public UpdateFamilyIdBusinessValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, ct) =>
            {
                if (categoryId is null || categoryId == Guid.Empty)
                    return true;

                return await categoryRepository.ExistsAsync(categoryId.Value, ct);
            })
            .WithMessage("The specified category does not exist")
            .When(x => x.CategoryId.HasValue && x.CategoryId != Guid.Empty);
    }
}
