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
/// Service for Draft operations.
/// </summary>
public class DraftService : IDraftService
{
    private readonly IDraftRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DraftService(IDraftRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<DraftDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? templateId = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DraftEntity> drafts;

        if (templateId.HasValue)
        {
            drafts = await _repository.GetByTemplateIdAsync(templateId.Value, cancellationToken);
        }
        else
        {
            drafts = await _repository.GetAllAsync(cancellationToken);
        }

        var totalCount = drafts.Count;

        // Apply pagination
        var pagedItems = drafts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedItems.Adapt<List<DraftDto>>();

        return new PagedResult<DraftDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<DraftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity?.Adapt<DraftDto>();
    }

    public async Task<Guid> CreateAsync(CreateDraftDto dto, CancellationToken cancellationToken = default)
    {
        // Check if draft with same FamilyUniqueId already exists
        var existing = await _repository.GetByFamilyUniqueIdAsync(dto.FamilyUniqueId, cancellationToken);
        if (existing != null)
        {
            throw new ValidationException(
                nameof(dto.FamilyUniqueId),
                $"A draft with FamilyUniqueId '{dto.FamilyUniqueId}' already exists.");
        }

        var entity = new DraftEntity(dto.FamilyName, dto.FamilyUniqueId, dto.TemplateId);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateDraftDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(DraftEntity), id);

        // Only SelectedRoleId can be updated via UpdateDraftDto
        if (dto.SelectedRoleId != entity.SelectedRoleId)
        {
            entity.SetSelectedRole(dto.SelectedRoleId);
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(DraftEntity), id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, DraftStatus status, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(DraftEntity), id);

        switch (status)
        {
            case DraftStatus.RoleSelected:
                // Role must be selected first
                if (!entity.SelectedRoleId.HasValue)
                {
                    throw new ValidationException(
                        nameof(entity.SelectedRoleId),
                        "Cannot set status to RoleSelected without selecting a role.");
                }
                entity.SetSelectedRole(entity.SelectedRoleId);
                break;

            case DraftStatus.Stamped:
                entity.MarkAsStamped();
                break;

            case DraftStatus.Published:
                entity.MarkAsPublished();
                break;

            case DraftStatus.New:
                throw new ValidationException(
                    nameof(status),
                    "Cannot reset draft status to New.");

            default:
                throw new ValidationException(
                    nameof(status),
                    $"Unknown status: {status}");
        }

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BatchCreateOrUpdateAsync(
        IReadOnlyList<CreateDraftDto> drafts,
        CancellationToken cancellationToken = default)
    {
        foreach (var dto in drafts)
        {
            var existing = await _repository.GetByFamilyUniqueIdAsync(dto.FamilyUniqueId, cancellationToken);

            if (existing != null)
            {
                // Update existing draft - update LastSeen
                existing.UpdateLastSeen();
                await _repository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                // Create new draft
                var entity = new DraftEntity(dto.FamilyName, dto.FamilyUniqueId, dto.TemplateId);
                await _repository.AddAsync(entity, cancellationToken);
            }
        }

        // Save all changes atomically at the end of batch operation
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
