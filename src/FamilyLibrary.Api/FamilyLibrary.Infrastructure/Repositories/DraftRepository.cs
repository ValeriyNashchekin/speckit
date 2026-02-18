using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class DraftRepository : Repository<DraftEntity>, IDraftRepository
{
    public DraftRepository(AppDbContext context) : base(context) { }

    public override async Task<DraftEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.SelectedRole)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DraftEntity?> GetByFamilyUniqueIdAsync(string familyUniqueId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.SelectedRole)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.FamilyUniqueId == familyUniqueId, cancellationToken);
    }

    public async Task<IReadOnlyList<DraftEntity>> GetByStatusAsync(DraftStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.SelectedRole)
            .Where(d => d.Status == status)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DraftEntity>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.SelectedRole)
            .Where(d => d.TemplateId == templateId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
