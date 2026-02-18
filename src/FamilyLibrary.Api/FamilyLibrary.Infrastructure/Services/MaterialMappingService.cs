using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Infrastructure.Data;
using Mapster;

namespace FamilyLibrary.Infrastructure.Services;

/// <summary>
/// Service for material mapping operations.
/// Maps template/library material names to project-specific material names.
/// </summary>
public class MaterialMappingService : IMaterialMappingService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the MaterialMappingService.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MaterialMappingService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<MaterialMappingDto>> GetAllAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.MaterialMappings
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.LastUsedAt ?? DateTime.MinValue)
            .ThenBy(m => m.TemplateMaterialName)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<MaterialMappingDto>>();
    }

    /// <inheritdoc />
    public async Task<MaterialMappingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MaterialMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MaterialMappingEntity), id);

        return entity.Adapt<MaterialMappingDto>();
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateMaterialMappingRequest dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate mapping in the same project
        var exists = await _context.MaterialMappings
            .AnyAsync(m => m.ProjectId == dto.ProjectId &&
                          m.TemplateMaterialName == dto.TemplateMaterialName, cancellationToken);

        if (exists)
        {
            throw new ValidationException(
                nameof(dto.TemplateMaterialName),
                $"A mapping for template material '{dto.TemplateMaterialName}' already exists in this project.");
        }

        var entity = new MaterialMappingEntity(
            dto.ProjectId,
            dto.TemplateMaterialName,
            dto.ProjectMaterialName);

        _context.MaterialMappings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, UpdateMaterialMappingRequest dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MaterialMappings
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MaterialMappingEntity), id);

        entity.Update(dto.ProjectMaterialName);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MaterialMappings
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MaterialMappingEntity), id);

        _context.MaterialMappings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MaterialMappingDto?> LookupAsync(Guid projectId, string templateMaterialName, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MaterialMappings
            .FirstOrDefaultAsync(m => m.ProjectId == projectId &&
                                     m.TemplateMaterialName == templateMaterialName, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        // Update LastUsedAt timestamp
        entity.MarkAsUsed();
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Adapt<MaterialMappingDto>();
    }
}
