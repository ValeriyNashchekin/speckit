namespace FamilyLibrary.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : Exception
{
    public string RuleName { get; }
    
    public BusinessRuleException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
    
    public BusinessRuleException(string ruleName, string message, Exception innerException)
        : base(message, innerException)
    {
        RuleName = ruleName;
    }
}
