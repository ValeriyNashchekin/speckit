namespace FamilyLibrary.Domain.Enums;

/// <summary>
/// Operators for recognition conditions.
/// </summary>
public enum RecognitionOperator
{
    Contains = 0,
    NotContains = 1
}

/// <summary>
/// Logical operators for combining conditions.
/// </summary>
public enum LogicalOperator
{
    And = 0,
    Or = 1
}
