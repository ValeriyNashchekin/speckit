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
/// Service for FamilyRole operations.
/// </summary>
public class FamilyRoleService : IFamilyRoleService
{
    private readonly IFamilyRoleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FamilyRoleService(IFamilyRoleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FamilyRoleDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        RoleType? type = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var allRoles = await _repository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredRoles = allRoles.AsEnumerable();

        if (type.HasValue)
        {
            filteredRoles = filteredRoles.Where(r => r.Type == type.Value);
        }

        if (categoryId.HasValue)
        {
            filteredRoles = filteredRoles.Where(r => r.CategoryId == categoryId.Value);
        }

        var filteredList = filteredRoles.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var pagedItems = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedItems.Adapt<List<FamilyRoleDto>>();

        return new PagedResult<FamilyRoleDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<FamilyRoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRoleEntity), id);

        return entity.Adapt<FamilyRoleDto>();
    }

    public async Task<Guid> CreateAsync(CreateFamilyRoleDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        if (await _repository.NameExistsAsync(dto.Name, cancellationToken))
        {
            throw new ValidationException(nameof(dto.Name), $"A family role with name '{dto.Name}' already exists.");
        }

        var entity = new FamilyRoleEntity(dto.Name, dto.Type, dto.Description, dto.CategoryId);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateFamilyRoleDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRoleEntity), id);

        // Note: We need to get a tracked entity for update
        // Since GetByIdAsync uses AsNoTracking, we need to fetch without it
        // The repository should handle this, but let's use the entity we have
        entity.Update(dto.Description, dto.CategoryId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Check if entity exists
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(FamilyRoleEntity), id);
        }

        // Check if role has associated families
        if (await _repository.HasFamiliesAsync(id, cancellationToken))
        {
            throw new BusinessRuleException(
                "CannotDeleteFamilyRoleWithFamilies",
                "Cannot delete family role because it has associated families.");
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<BatchCreateResult> ImportAsync(
        IReadOnlyList<CreateFamilyRoleDto> dtos,
        CancellationToken cancellationToken = default)
    {
        var createdIds = new List<Guid>();
        var skippedNames = new List<string>();

        foreach (var dto in dtos)
        {
            // Skip duplicates
            if (await _repository.NameExistsAsync(dto.Name, cancellationToken))
            {
                skippedNames.Add(dto.Name);
                continue;
            }

            var entity = new FamilyRoleEntity(dto.Name, dto.Type, dto.Description, dto.CategoryId);
            await _repository.AddAsync(entity, cancellationToken);
            createdIds.Add(entity.Id);
        }

        // Save all changes atomically at the end of batch operation
        if (createdIds.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BatchCreateResult(createdIds, skippedNames, dtos.Count);
    }
}
