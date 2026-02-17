using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class FamilyNameMappingRepository : Repository<FamilyNameMappingEntity>, IFamilyNameMappingRepository
{
    public FamilyNameMappingRepository(AppDbContext context) : base(context) { }

    public async Task<FamilyNameMappingEntity?> GetByFamilyNameAndProjectAsync(string familyName, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.FamilyName == familyName && m.ProjectId == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyNameMappingEntity>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyNameMappingEntity>> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.RoleName == roleName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
