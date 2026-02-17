using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Domain.Interfaces;
using Mapster;

namespace FamilyLibrary.Application.Services;

/// <summary>
/// Service for Category operations.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetOrderedAsync(cancellationToken);
        return entities.Adapt<List<CategoryDto>>();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CategoryEntity), id);

        return entity.Adapt<CategoryDto>();
    }

    public async Task<Guid> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        var existingCategory = await _repository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingCategory != null)
        {
            throw new ValidationException(nameof(dto.Name), $"A category with name '{dto.Name}' already exists.");
        }

        var entity = new CategoryEntity(dto.Name, dto.Description, dto.SortOrder);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CategoryEntity), id);

        // Check for duplicate name if name is being changed
        if (entity.Name != dto.Name)
        {
            var existingCategory = await _repository.GetByNameAsync(dto.Name, cancellationToken);
            if (existingCategory != null)
            {
                throw new ValidationException(nameof(dto.Name), $"A category with name '{dto.Name}' already exists.");
            }
        }

        entity.Update(dto.Name, dto.Description, dto.SortOrder);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(CategoryEntity), id);
        }

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
