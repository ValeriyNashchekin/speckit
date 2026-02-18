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

    public async Task<(IReadOnlyList<FamilyEntity> Items, int TotalCount)> GetFilteredAsync(
        Guid? roleId,
        string? searchTerm,
        Guid? categoryId,
        List<Guid>? tagIds,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(f => f.Role)
                .ThenInclude(r => r!.Category)
            .Include(f => f.Role)
                .ThenInclude(r => r!.Tags)
            .AsNoTracking()
            .AsQueryable();

        // Apply roleId filter at DB level
        if (roleId.HasValue)
        {
            query = query.Where(f => f.RoleId == roleId.Value);
        }

        // Apply searchTerm filter at DB level (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(f => f.FamilyName.Contains(searchTerm));
        }

        // Apply categoryId filter at DB level
        if (categoryId.HasValue)
        {
            query = query.Where(f => f.Role!.CategoryId == categoryId.Value);
        }

        // Apply tagIds filter at DB level
        if (tagIds is not null && tagIds.Count > 0)
        {
            query = query.Where(f => f.Role!.Tags.Any(t => tagIds.Contains(t.Id)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination at DB level
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<FamilyEntity?> GetByRoleAndNameAsync(Guid roleId, string familyName, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                f => f.RoleId == roleId && f.FamilyName.ToLower() == familyName.ToLower(),
                cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyEntity>> GetByHashesAsync(IEnumerable<string> hashes, CancellationToken cancellationToken = default)
    {
        var hashList = hashes.ToList();
        if (hashList.Count == 0)
            return [];

        // Get latest version for each family and filter by hash
        var familyIdsWithHashes = await Context.FamilyVersions
            .AsNoTracking()
            .Where(v => hashList.Contains(v.Hash))
            .GroupBy(v => v.FamilyId)
            .Select(g => new { FamilyId = g.Key, LatestHash = g.OrderByDescending(v => v.Version).First().Hash })
            .ToListAsync(cancellationToken);

        var filteredHashes = familyIdsWithHashes
            .Where(x => hashList.Contains(x.LatestHash))
            .Select(x => x.FamilyId)
            .ToList();

        return await DbSet
            .Include(f => f.Role)
            .Include(f => f.Versions.OrderByDescending(v => v.Version).Take(1))
            .AsNoTracking()
            .Where(f => filteredHashes.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }
}
