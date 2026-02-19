using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for UpdateFamilyIdDto.
/// </summary>
public class UpdateFamilyIdDtoValidator : AbstractValidator<UpdateFamilyIdDto>
{
    public UpdateFamilyIdDtoValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
                .WithMessage("CategoryId must be a valid GUID")
            .When(x => x.CategoryId.HasValue);
    }
}
