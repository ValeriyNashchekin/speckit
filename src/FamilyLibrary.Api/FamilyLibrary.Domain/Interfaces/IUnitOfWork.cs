namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for atomic database operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
