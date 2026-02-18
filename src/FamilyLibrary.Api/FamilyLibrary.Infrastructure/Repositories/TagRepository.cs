using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Repositories;

public class TagRepository : Repository<TagEntity>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context) { }

    public async Task<TagEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}
