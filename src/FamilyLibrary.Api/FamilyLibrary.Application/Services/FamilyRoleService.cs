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
/// Service for FamilyId operations.
/// </summary>
public class FamilyIdService : IFamilyIdService
{
    private readonly IFamilyRoleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FamilyIdService(IFamilyRoleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FamilyIdDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        RoleType? type = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var allRoles = await _repository.GetAllAsync(cancellationToken);

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

        var pagedItems = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedItems.Adapt<List<FamilyIdDto>>();

        return new PagedResult<FamilyIdDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<FamilyIdDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRoleEntity), id);

        return entity.Adapt<FamilyIdDto>();
    }

    public async Task<Guid> CreateAsync(CreateFamilyIdDto dto, CancellationToken cancellationToken = default)
    {
        if (await _repository.NameExistsAsync(dto.Name, cancellationToken))
        {
            throw new ValidationException(nameof(dto.Name), $"A family id with name '{dto.Name}' already exists.");
        }

        var entity = new FamilyRoleEntity(dto.Name, dto.Type, dto.Description, dto.CategoryId);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateFamilyIdDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRoleEntity), id);

        entity.Update(dto.Description, dto.CategoryId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(FamilyRoleEntity), id);
        }

        if (await _repository.HasFamiliesAsync(id, cancellationToken))
        {
            throw new BusinessRuleException(
                "CannotDeleteFamilyIdWithFamilies",
                "Cannot delete family id because it has associated families.");
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<BatchCreateResult> ImportAsync(
        IReadOnlyList<CreateFamilyIdDto> dtos,
        CancellationToken cancellationToken = default)
    {
        var createdIds = new List<Guid>();
        var skippedNames = new List<string>();

        foreach (var dto in dtos)
        {
            if (await _repository.NameExistsAsync(dto.Name, cancellationToken))
            {
                skippedNames.Add(dto.Name);
                continue;
            }

            var entity = new FamilyRoleEntity(dto.Name, dto.Type, dto.Description, dto.CategoryId);
            await _repository.AddAsync(entity, cancellationToken);
            createdIds.Add(entity.Id);
        }

        if (createdIds.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BatchCreateResult(createdIds, skippedNames, dtos.Count);
    }
}
