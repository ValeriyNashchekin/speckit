using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateFamilyDto.
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
    }
}
