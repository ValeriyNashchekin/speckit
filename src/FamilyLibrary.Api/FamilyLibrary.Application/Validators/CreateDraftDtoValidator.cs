using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateDraftDto.
/// </summary>
public class CreateDraftDtoValidator : AbstractValidator<CreateDraftDto>
{
    public CreateDraftDtoValidator()
    {
        RuleFor(x => x.FamilyName)
            .NotEmpty()
                .WithMessage("FamilyName is required")
            .MaximumLength(200)
                .WithMessage("FamilyName must not exceed 200 characters");

        RuleFor(x => x.FamilyUniqueId)
            .NotEmpty()
                .WithMessage("FamilyUniqueId is required")
            .MaximumLength(100)
                .WithMessage("FamilyUniqueId must not exceed 100 characters");

        RuleFor(x => x.TemplateId)
            .NotEqual(Guid.Empty)
                .WithMessage("TemplateId must be a valid GUID")
            .When(x => x.TemplateId.HasValue);
    }
}
