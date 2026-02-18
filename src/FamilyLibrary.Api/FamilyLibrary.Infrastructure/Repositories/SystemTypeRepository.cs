using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class SystemTypeRepository : Repository<SystemTypeEntity>, ISystemTypeRepository
{
    public SystemTypeRepository(AppDbContext context) : base(context) { }

    public override async Task<SystemTypeEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SystemTypeEntity?> GetByTypeNameAsync(string typeName, string category, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TypeName == typeName && s.Category == category, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemTypeEntity>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemTypeEntity>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Role)
            .Where(s => s.Category == category)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
