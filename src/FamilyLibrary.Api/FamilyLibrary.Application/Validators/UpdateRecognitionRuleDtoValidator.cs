using System.Text.RegularExpressions;
using FluentValidation;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Validators;

/// <summary>
/// Validator for UpdateRecognitionRuleDto.
/// Handles structural validation of recognition rule update data.
/// </summary>
public class UpdateRecognitionRuleDtoValidator : AbstractValidator<UpdateRecognitionRuleDto>
{
    private static readonly Regex OperandRegex = new(@"^[A-Za-z][A-Za-z0-9_\-]*$", RegexOptions.Compiled);

    public UpdateRecognitionRuleDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEqual(Guid.Empty)
                .WithMessage("RoleId must be a valid GUID")
            .When(x => x.RoleId != Guid.Empty);

        RuleFor(x => x.RootNode)
            .NotEmpty()
                .WithMessage("RootNode cannot be empty when provided")
            .MaximumLength(100)
                .WithMessage("RootNode must not exceed 100 characters")
            .When(x => x.RootNode is not null);

        RuleFor(x => x.Formula)
            .NotEmpty()
                .WithMessage("Formula cannot be empty when provided")
            .MaximumLength(500)
                .WithMessage("Formula must not exceed 500 characters")
            .Must(BeValidFormula)
                .WithMessage("Formula contains invalid syntax. Allowed operators: AND, OR, NOT. Example: '(FB OR Desk) AND Wired'")
            .When(x => x.Formula is not null);
    }

    /// <summary>
    /// Validates formula syntax.
    /// Allowed operators: AND, OR, NOT (case-insensitive)
    /// Parentheses must be balanced
    /// Operands can contain letters, numbers, underscores, hyphens
    /// </summary>
    private bool BeValidFormula(string? formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return false;

        // Check balanced parentheses
        int balance = 0;
        foreach (char c in formula)
        {
            if (c == '(') balance++;
            else if (c == ')') balance--;

            if (balance < 0) return false; // Closing before opening
        }
        if (balance != 0) return false; // Unbalanced

        // Normalize operators (case-insensitive)
        string normalized = formula;

        // Replace operators with placeholders (case-insensitive)
        normalized = Regex.Replace(normalized, @"\bAND\b", "&&", RegexOptions.IgnoreCase);
        normalized = Regex.Replace(normalized, @"\bOR\b", "||", RegexOptions.IgnoreCase);
        normalized = Regex.Replace(normalized, @"\bNOT\b", "!", RegexOptions.IgnoreCase);

        // After removing operators and parentheses, check remaining tokens
        string tokensOnly = Regex.Replace(normalized, @"[()&&||!]", " ");
        string[] tokens = tokensOnly.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string token in tokens)
        {
            if (!OperandRegex.IsMatch(token))
                return false;
        }

        return true;
    }
}
