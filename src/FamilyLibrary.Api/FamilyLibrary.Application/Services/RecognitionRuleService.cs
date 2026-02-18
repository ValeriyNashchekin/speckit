using System.Globalization;
using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Domain.Interfaces;
using Mapster;

namespace FamilyLibrary.Application.Services;

public class RecognitionRuleService : IRecognitionRuleService
{
    private readonly IRecognitionRuleRepository _repository;
    private readonly IFamilyRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecognitionRuleService(
        IRecognitionRuleRepository repository,
        IFamilyRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<RecognitionRuleDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var allRules = await _repository.GetAllAsync(cancellationToken);
        var ruleList = allRules.ToList();
        var totalCount = ruleList.Count;
        var pagedItems = ruleList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var dtos = pagedItems.Adapt<List<RecognitionRuleDto>>();
        return new PagedResult<RecognitionRuleDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<RecognitionRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity?.Adapt<RecognitionRuleDto>();
    }

    public async Task<Guid> CreateAsync(CreateRecognitionRuleDto dto, CancellationToken cancellationToken = default)
    {
        var roleExists = await _roleRepository.ExistsAsync(dto.RoleId, cancellationToken);
        if (!roleExists)
            throw new NotFoundException(nameof(FamilyRoleEntity), dto.RoleId);

        if (!await ValidateFormulaAsync(dto.Formula, cancellationToken))
            throw new Domain.Exceptions.ValidationException(nameof(dto.Formula), "Invalid formula syntax.");

        var existingRule = await _repository.GetByRoleIdAsync(dto.RoleId, cancellationToken);
        if (existingRule != null)
            throw new Domain.Exceptions.ValidationException(nameof(dto.RoleId), "A recognition rule already exists for this role.");

        var entity = new RecognitionRuleEntity(dto.RoleId, dto.RootNode, dto.Formula);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateRecognitionRuleDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(RecognitionRuleEntity), id);

        if (dto.RoleId != Guid.Empty && dto.RoleId != entity.RoleId)
        {
            var roleExists = await _roleRepository.ExistsAsync(dto.RoleId, cancellationToken);
            if (!roleExists)
                throw new NotFoundException(nameof(FamilyRoleEntity), dto.RoleId);

            var existingRule = await _repository.GetByRoleIdAsync(dto.RoleId, cancellationToken);
            if (existingRule != null && existingRule.Id != id)
                throw new Domain.Exceptions.ValidationException(nameof(dto.RoleId), "A recognition rule already exists for this role.");
        }

        if (dto.Formula is not null && !await ValidateFormulaAsync(dto.Formula, cancellationToken))
            throw new Domain.Exceptions.ValidationException(nameof(dto.Formula), "Invalid formula syntax.");

        var newRootNode = dto.RootNode ?? entity.RootNode;
        var newFormula = dto.Formula ?? entity.Formula;
        entity.Update(newRootNode, newFormula);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new NotFoundException(nameof(RecognitionRuleEntity), id);
        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ValidateFormulaAsync(string formula, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return Task.FromResult(false);
        try
        {
            var tokens = TokenizeFormula(formula);
            var isValid = ValidateTokens(tokens);
            return Task.FromResult(isValid);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> TestRuleAsync(Guid id, string familyName, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(RecognitionRuleEntity), id);
        return EvaluateFormula(entity.Formula, familyName);
    }

    public async Task<List<ConflictDto>> CheckConflictsAsync(Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var allRules = await _repository.GetAllAsync(cancellationToken);
        var ruleList = allRules.ToList();
        var conflicts = new List<ConflictDto>();

        for (var i = 0; i < ruleList.Count; i++)
        {
            for (var j = i + 1; j < ruleList.Count; j++)
            {
                var rule1 = ruleList[i];
                var rule2 = ruleList[j];
                if (excludeId.HasValue && (rule1.Id == excludeId.Value || rule2.Id == excludeId.Value))
                    continue;
                if (HasPotentialConflict(rule1.Formula, rule2.Formula))
                {
                    conflicts.Add(new ConflictDto
                    {
                        RuleId1 = rule1.Id,
                        RuleId2 = rule2.Id,
                        RoleName1 = rule1.Role?.Name ?? "Unknown",
                        RoleName2 = rule2.Role?.Name ?? "Unknown",
                        Description = "Rules for roles may match the same family name."
                    });
                }
            }
        }
        return conflicts;
    }

    #region Formula Parsing

    private enum TokenType
    {
        Pattern,
        And,
        Or,
        Not,
        LeftParen,
        RightParen,
        Eof
    }

    private readonly record struct Token(TokenType Type, string Value);

    private static List<Token> TokenizeFormula(string formula)
    {
        var tokens = new List<Token>();
        var i = 0;
        var span = formula.AsSpan();

        while (i < span.Length)
        {
            // Skip whitespace
            while (i < span.Length && char.IsWhiteSpace(span[i]))
                i++;

            if (i >= span.Length)
                break;

            var c = span[i];

            if (c == '(')
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                i++;
            }
            else if (c == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                i++;
            }
            else if (IsOperatorStart(span, i, out var operatorType, out var operatorLength))
            {
                tokens.Add(new Token(operatorType, span.Slice(i, operatorLength).ToString()));
                i += operatorLength;
            }
            else
            {
                // Read pattern (text to match)
                var start = i;
                while (i < span.Length && !char.IsWhiteSpace(span[i]) && span[i] != '(' && span[i] != ')')
                    i++;

                if (i > start)
                {
                    var potentialOperator = span.Slice(start, i - start).ToString();
                    var upperOperator = potentialOperator.ToUpperInvariant();

                    if (upperOperator == "AND")
                        tokens.Add(new Token(TokenType.And, potentialOperator));
                    else if (upperOperator == "OR")
                        tokens.Add(new Token(TokenType.Or, potentialOperator));
                    else if (upperOperator == "NOT")
                        tokens.Add(new Token(TokenType.Not, potentialOperator));
                    else
                        tokens.Add(new Token(TokenType.Pattern, potentialOperator));
                }
            }
        }

        tokens.Add(new Token(TokenType.Eof, string.Empty));
        return tokens;
    }

    private static bool IsOperatorStart(ReadOnlySpan<char> span, int index, out TokenType type, out int length)
    {
        type = TokenType.Pattern;
        length = 0;

        if (index >= span.Length)
            return false;

        var remaining = span.Slice(index);

        // Check AND
        if (remaining.Length >= 3 &&
            char.ToUpperInvariant(remaining[0]) == 'A' &&
            char.ToUpperInvariant(remaining[1]) == 'N' &&
            char.ToUpperInvariant(remaining[2]) == 'D' &&
            (remaining.Length == 3 || !char.IsLetterOrDigit(remaining[3])))
        {
            type = TokenType.And;
            length = 3;
            return true;
        }

        // Check OR
        if (remaining.Length >= 2 &&
            char.ToUpperInvariant(remaining[0]) == 'O' &&
            char.ToUpperInvariant(remaining[1]) == 'R' &&
            (remaining.Length == 2 || !char.IsLetterOrDigit(remaining[2])))
        {
            type = TokenType.Or;
            length = 2;
            return true;
        }

        // Check NOT
        if (remaining.Length >= 3 &&
            char.ToUpperInvariant(remaining[0]) == 'N' &&
            char.ToUpperInvariant(remaining[1]) == 'O' &&
            char.ToUpperInvariant(remaining[2]) == 'T' &&
            (remaining.Length == 3 || !char.IsLetterOrDigit(remaining[3])))
        {
            type = TokenType.Not;
            length = 3;
            return true;
        }

        return false;
    }

    private static bool ValidateTokens(List<Token> tokens)
    {
        var parenCount = 0;
        var expectOperand = true;

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            switch (token.Type)
            {
                case TokenType.Pattern:
                    if (!expectOperand)
                        return false;
                    expectOperand = false;
                    break;

                case TokenType.Not:
                    if (!expectOperand)
                        return false;
                    // NOT is prefix operator, still expect operand
                    break;

                case TokenType.And:
                case TokenType.Or:
                    if (expectOperand)
                        return false;
                    expectOperand = true;
                    break;

                case TokenType.LeftParen:
                    if (!expectOperand)
                        return false;
                    parenCount++;
                    break;

                case TokenType.RightParen:
                    if (expectOperand)
                        return false;
                    parenCount--;
                    if (parenCount < 0)
                        return false;
                    break;

                case TokenType.Eof:
                    if (expectOperand && i > 0)
                        return false;
                    break;
            }
        }

        return parenCount == 0 && !expectOperand;
    }

    /// <summary>
    /// Evaluates a formula against a family name.
    /// Formula examples: "(FB OR Desk) AND Wired", "NOT Window", "Chair AND Office"
    /// </summary>
    public static bool EvaluateFormula(string formula, string familyName)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return false;

        var tokens = TokenizeFormula(formula);
        var index = 0;
        return EvaluateExpression(tokens, ref index, familyName);
    }

    private static bool EvaluateExpression(List<Token> tokens, ref int index, string familyName)
    {
        return EvaluateOrExpression(tokens, ref index, familyName);
    }

    private static bool EvaluateOrExpression(List<Token> tokens, ref int index, string familyName)
    {
        var left = EvaluateAndExpression(tokens, ref index, familyName);

        while (index < tokens.Count && tokens[index].Type == TokenType.Or)
        {
            index++; // consume OR
            var right = EvaluateAndExpression(tokens, ref index, familyName);
            left = left || right;
        }

        return left;
    }

    private static bool EvaluateAndExpression(List<Token> tokens, ref int index, string familyName)
    {
        var left = EvaluateNotExpression(tokens, ref index, familyName);

        while (index < tokens.Count && tokens[index].Type == TokenType.And)
        {
            index++; // consume AND
            var right = EvaluateNotExpression(tokens, ref index, familyName);
            left = left && right;
        }

        return left;
    }

    private static bool EvaluateNotExpression(List<Token> tokens, ref int index, string familyName)
    {
        if (index < tokens.Count && tokens[index].Type == TokenType.Not)
        {
            index++; // consume NOT
            return !EvaluatePrimaryExpression(tokens, ref index, familyName);
        }

        return EvaluatePrimaryExpression(tokens, ref index, familyName);
    }

    private static bool EvaluatePrimaryExpression(List<Token> tokens, ref int index, string familyName)
    {
        if (index >= tokens.Count)
            return false;

        var token = tokens[index];

        if (token.Type == TokenType.LeftParen)
        {
            index++; // consume (
            var result = EvaluateExpression(tokens, ref index, familyName);

            if (index < tokens.Count && tokens[index].Type == TokenType.RightParen)
                index++; // consume )

            return result;
        }

        if (token.Type == TokenType.Pattern)
        {
            index++;
            // Case-insensitive contains check
            return familyName.Contains(token.Value, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool HasPotentialConflict(string formula1, string formula2)
    {
        var patterns1 = ExtractPatterns(formula1);
        var patterns2 = ExtractPatterns(formula2);

        // If any pattern from formula1 is a substring of a pattern from formula2 or vice versa
        foreach (var p1 in patterns1)
        {
            foreach (var p2 in patterns2)
            {
                if (p1.Contains(p2, StringComparison.OrdinalIgnoreCase) ||
                    p2.Contains(p1, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static HashSet<string> ExtractPatterns(string formula)
    {
        var tokens = TokenizeFormula(formula);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in tokens)
        {
            if (t.Type == TokenType.Pattern)
                result.Add(t.Value);
        }

        return result;
    }

    #endregion
}
