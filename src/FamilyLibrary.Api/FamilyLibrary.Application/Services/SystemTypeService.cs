using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Domain.Interfaces;
using Mapster;

namespace FamilyLibrary.Application.Services;

/// <summary>
/// Service for SystemType operations.
/// </summary>
public class SystemTypeService : ISystemTypeService
{
    private readonly ISystemTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SystemTypeService(ISystemTypeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SystemTypeDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? roleId = null,
        SystemFamilyGroup? group = null,
        CancellationToken cancellationToken = default)
    {
        var allTypes = await _repository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredTypes = allTypes.AsEnumerable();

        if (roleId.HasValue)
        {
            filteredTypes = filteredTypes.Where(t => t.RoleId == roleId.Value);
        }

        if (group.HasValue)
        {
            filteredTypes = filteredTypes.Where(t => t.Group == group.Value);
        }

        var filteredList = filteredTypes.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var pagedItems = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedItems.Adapt<List<SystemTypeDto>>();

        return new PagedResult<SystemTypeDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<SystemTypeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(SystemTypeEntity), id);

        return entity.Adapt<SystemTypeDto>();
    }

    public async Task<Guid> CreateAsync(CreateSystemTypeDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate typename + category combination
        var existing = await _repository.GetByTypeNameAsync(dto.TypeName, dto.Category, cancellationToken);
        if (existing != null)
        {
            throw new ValidationException(nameof(dto.TypeName), 
                $"A system type with name '{dto.TypeName}' and category '{dto.Category}' already exists.");
        }

        var entity = new SystemTypeEntity(
            dto.RoleId,
            dto.TypeName,
            dto.Category,
            dto.SystemFamily,
            dto.Group,
            dto.Json,
            dto.Hash);

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateSystemTypeDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(SystemTypeEntity), id);

        entity.Update(dto.Json, dto.Hash);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(SystemTypeEntity), id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemTypeDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByRoleIdAsync(roleId, cancellationToken);
        return entities.Adapt<List<SystemTypeDto>>();
    }
}
