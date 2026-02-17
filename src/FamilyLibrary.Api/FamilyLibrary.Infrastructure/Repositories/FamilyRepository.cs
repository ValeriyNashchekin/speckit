using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class FamilyRepository : Repository<FamilyEntity>, IFamilyRepository
{
    public FamilyRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<FamilyEntity>> GetAllWithRolesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Role)
                .ThenInclude(r => r!.Category)
            .Include(f => f.Role)
                .ThenInclude(r => r!.Tags)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public override async Task<FamilyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Role)
            .Include(f => f.Versions.OrderByDescending(v => v.Version).Take(5))
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FamilyEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FamilyName == name, cancellationToken);
    }

    public async Task<FamilyEntity?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Role)
            .Include(f => f.Versions.OrderByDescending(v => v.Version))
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyEntity>> GetWithVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Versions.OrderByDescending(v => v.Version).Take(1))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyEntity>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Versions.OrderByDescending(v => v.Version).Take(1))
            .Where(f => f.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HashExistsAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await Context.FamilyVersions.AnyAsync(v => v.Hash == hash, cancellationToken);
    }

    public async Task<FamilyEntity?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        var version = await Context.FamilyVersions
            .Include(v => v.Family)
            .ThenInclude(f => f!.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Hash == hash, cancellationToken);

        return version?.Family;
    }
}
