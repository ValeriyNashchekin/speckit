using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class FamilyVersionRepository : Repository<FamilyVersionEntity>, IFamilyVersionRepository
{
    public FamilyVersionRepository(AppDbContext context) : base(context) { }

    public override async Task<FamilyVersionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.Family)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyVersionEntity>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.Family)
            .Where(v => v.FamilyId == familyId)
            .OrderByDescending(v => v.Version)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<FamilyVersionEntity?> GetByVersionAsync(Guid familyId, int version, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.Family)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.FamilyId == familyId && v.Version == version, cancellationToken);
    }

    public async Task<FamilyVersionEntity?> GetLatestVersionAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(v => v.FamilyId == familyId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
