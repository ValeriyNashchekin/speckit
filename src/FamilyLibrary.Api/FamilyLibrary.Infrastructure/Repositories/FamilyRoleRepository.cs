using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class FamilyRoleRepository : Repository<FamilyRoleEntity>, IFamilyRoleRepository
{
    public FamilyRoleRepository(AppDbContext context) : base(context) { }

    public override async Task<FamilyRoleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Category)
            .Include(r => r.Tags)
            .Include(r => r.RecognitionRule)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<FamilyRoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyRoleEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Tags)
            .Where(r => r.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<bool> HasFamiliesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Families.AnyAsync(f => f.RoleId == id, cancellationToken);
    }
}
