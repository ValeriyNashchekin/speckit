using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateRecognitionRuleDto.
/// </summary>
public class CreateRecognitionRuleDtoValidator : AbstractValidator<CreateRecognitionRuleDto>
{
    public CreateRecognitionRuleDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("RoleId is required and must be a valid GUID");

        RuleFor(x => x.RootNode)
            .NotEmpty()
                .WithMessage("RootNode is required")
            .MaximumLength(100)
                .WithMessage("RootNode must not exceed 100 characters");

        RuleFor(x => x.Formula)
            .NotEmpty()
                .WithMessage("Formula is required")
            .MaximumLength(500)
                .WithMessage("Formula must not exceed 500 characters");
    }
}
