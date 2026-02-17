using FluentValidation;
using FamilyLibrary.Application.DTOs;
using System.Text.RegularExpressions;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for CreateFamilyRoleDto.
/// </summary>
public class CreateFamilyRoleDtoValidator : AbstractValidator<CreateFamilyRoleDto>
{
    private static readonly Regex NameRegex = new(@"^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public CreateFamilyRoleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is required")
            .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters")
            .Must(name => NameRegex.IsMatch(name))
                .WithMessage("Name must start with a letter and contain only letters, digits, and underscores");

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
