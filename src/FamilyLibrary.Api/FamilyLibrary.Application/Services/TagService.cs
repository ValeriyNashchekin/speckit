using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Domain.Interfaces;
using Mapster;

namespace FamilyLibrary.Application.Services;

/// <summary>
/// Service for Tag operations.
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(ITagRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Adapt<List<TagDto>>();
    }

    public async Task<TagDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(TagEntity), id);

        return entity.Adapt<TagDto>();
    }

    public async Task<Guid> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        var existingTag = await _repository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new ValidationException(nameof(dto.Name), $"A tag with name '{dto.Name}' already exists.");
        }

        var entity = new TagEntity(dto.Name, dto.Color);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(TagEntity), id);

        // Check for duplicate name if name is being changed
        if (entity.Name != dto.Name)
        {
            var existingTag = await _repository.GetByNameAsync(dto.Name, cancellationToken);
            if (existingTag != null)
            {
                throw new ValidationException(nameof(dto.Name), $"A tag with name '{dto.Name}' already exists.");
            }
        }

        entity.Update(dto.Name, dto.Color);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(TagEntity), id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
