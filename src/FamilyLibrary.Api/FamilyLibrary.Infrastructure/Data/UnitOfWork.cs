using FamilyLibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyLibrary.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation using EF Core DbContext.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
