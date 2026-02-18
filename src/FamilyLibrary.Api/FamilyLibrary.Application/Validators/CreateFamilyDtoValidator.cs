using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateFamilyDto.
/// Handles structural validation (format, length, required fields).
/// For business-rule validation (database existence checks), use CreateFamilyBusinessValidator.
/// </summary>
public class CreateFamilyDtoValidator : AbstractValidator<CreateFamilyDto>
{
    public CreateFamilyDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("RoleId is required and must be a valid GUID");

        RuleFor(x => x.FamilyName)
            .NotEmpty()
                .WithMessage("FamilyName is required")
            .MaximumLength(200)
                .WithMessage("FamilyName must not exceed 200 characters");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
                .WithMessage("OriginalFileName is required")
            .Must(fileName => fileName!.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
                .WithMessage("OriginalFileName must end with .rfa extension")
            .When(x => x.OriginalFileName is not null);
    }
}
