using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class RecognitionRuleRepository : Repository<RecognitionRuleEntity>, IRecognitionRuleRepository
{
    public RecognitionRuleRepository(AppDbContext context) : base(context) { }

    public override async Task<RecognitionRuleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<RecognitionRuleEntity?> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
    }
}
