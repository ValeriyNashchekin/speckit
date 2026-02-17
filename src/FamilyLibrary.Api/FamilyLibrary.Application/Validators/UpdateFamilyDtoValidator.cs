using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for UpdateFamilyDto.
/// Handles structural validation (format, length, required fields).
/// For business-rule validation (database existence checks), use UpdateFamilyBusinessValidator.
/// </summary>
public class UpdateFamilyDtoValidator : AbstractValidator<UpdateFamilyDto>
{
    public UpdateFamilyDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("RoleId must be a valid GUID when provided")
            .When(x => x.RoleId.HasValue);

        RuleFor(x => x.FamilyName)
            .NotEmpty()
                .WithMessage("FamilyName cannot be empty when provided")
            .MaximumLength(200)
                .WithMessage("FamilyName must not exceed 200 characters")
            .When(x => x.FamilyName is not null);

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
                .WithMessage("OriginalFileName cannot be empty when provided")
            .Must(fileName => fileName!.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
                .WithMessage("OriginalFileName must end with .rfa extension")
            .When(x => x.OriginalFileName is not null);
    }
}
