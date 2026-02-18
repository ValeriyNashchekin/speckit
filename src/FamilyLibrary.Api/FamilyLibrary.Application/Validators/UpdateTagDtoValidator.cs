using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for UpdateTagDto.
/// </summary>
public class UpdateTagDtoValidator : AbstractValidator<UpdateTagDto>
{
    public UpdateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is required")
            .MaximumLength(50)
                .WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Color)
            .MaximumLength(20)
                .WithMessage("Color must not exceed 20 characters")
            .When(x => x.Color is not null);
    }
}
