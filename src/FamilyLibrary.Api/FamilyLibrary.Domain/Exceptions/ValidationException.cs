namespace FamilyLibrary.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    public string PropertyName { get; }
    
    public ValidationException(string propertyName, string message)
        : base($"Validation failed for '{propertyName}': {message}")
    {
        PropertyName = propertyName;
    }
}
