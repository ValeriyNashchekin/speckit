using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for Draft operations.
/// </summary>
public interface IDraftService
{
    /// <summary>
    /// Gets all drafts with pagination and optional filtering by template.
    /// </summary>
    Task<PagedResult<DraftDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? templateId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a draft by ID.
    /// </summary>
    Task<DraftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new draft.
    /// </summary>
    Task<Guid> CreateAsync(CreateDraftDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing draft.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateDraftDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a draft.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a draft.
    /// </summary>
    Task UpdateStatusAsync(Guid id, DraftStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates multiple drafts based on FamilyUniqueId.
    /// Existing drafts are updated, new ones are created.
    /// </summary>
    Task BatchCreateOrUpdateAsync(
        IReadOnlyList<CreateDraftDto> drafts,
        CancellationToken cancellationToken = default);
}
