using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for UpdateDraftDto.
/// </summary>
public class UpdateDraftDtoValidator : AbstractValidator<UpdateDraftDto>
{
    public UpdateDraftDtoValidator()
    {
        RuleFor(x => x.SelectedRoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("SelectedRoleId must be a valid GUID")
            .When(x => x.SelectedRoleId.HasValue);
    }
}
