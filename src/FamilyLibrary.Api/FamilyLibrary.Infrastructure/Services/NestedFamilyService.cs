using Microsoft.EntityFrameworkCore;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Infrastructure.Data;

namespace FamilyLibrary.Infrastructure.Services;

/// <summary>
/// Service for nested family dependency operations.
/// Manages analysis and retrieval of nested family dependencies within parent families.
/// </summary>
public class NestedFamilyService : INestedFamilyService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the NestedFamilyService.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public NestedFamilyService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<NestedFamilyDto>> GetDependenciesAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        // Verify the family exists
        var familyExists = await _context.Families
            .AsNoTracking()
            .AnyAsync(f => f.Id == familyId, cancellationToken);

        if (!familyExists)
        {
            throw new NotFoundException(nameof(FamilyEntity), familyId);
        }

        // Get all dependencies for the parent family
        var dependencies = await _context.FamilyDependencies
            .AsNoTracking()
            .Where(d => d.ParentFamilyId == familyId)
            .OrderBy(d => d.NestedFamilyName)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        // Note: RfaVersion and ProjectVersion require plugin coordination
        // These are currently null/placeholder values
        return dependencies.Select(d => new NestedFamilyDto
        {
            FamilyName = d.NestedFamilyName,
            RoleName = d.NestedRoleName,
            IsShared = d.IsShared,
            InLibrary = d.InLibrary,
            LibraryVersion = d.LibraryVersion,
            RfaVersion = null, // Requires plugin coordination
            ProjectVersion = null // Requires plugin coordination
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<PreLoadSummaryDto> GetPreLoadSummaryAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        // Get the parent family with its role
        var family = await _context.Families
            .AsNoTracking()
            .Include(f => f.Role)
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyEntity), familyId);

        // Get all dependencies for the parent family
        var dependencies = await _context.FamilyDependencies
            .AsNoTracking()
            .Where(d => d.ParentFamilyId == familyId)
            .OrderBy(d => d.NestedFamilyName)
            .ToListAsync(cancellationToken);

        // Map dependencies to nested family summaries with recommended actions
        var nestedSummaries = dependencies.Select(d => new NestedFamilySummaryDto
        {
            FamilyName = d.NestedFamilyName,
            RoleName = d.NestedRoleName,
            RfaVersion = null, // Requires plugin coordination
            LibraryVersion = d.LibraryVersion,
            ProjectVersion = null, // Requires plugin coordination
            RecommendedAction = DetermineLoadAction(d)
        }).ToList();

        return new PreLoadSummaryDto
        {
            ParentFamilyName = family.FamilyName,
            ParentVersion = family.CurrentVersion,
            NestedFamilies = nestedSummaries
        };
    }

    /// <inheritdoc />
    public async Task<UsedInDto> GetUsedInAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ValidationException(nameof(roleName), "Role name cannot be empty.");
        }

        // Find all dependencies where the nested role name matches
        var dependencies = await _context.FamilyDependencies
            .AsNoTracking()
            .Where(d => d.NestedRoleName == roleName)
            .Include(d => d.ParentFamily)
                .ThenInclude(f => f!.Role)
            .ToListAsync(cancellationToken);

        // Get the nested family name from the first dependency (if any)
        var nestedFamilyName = dependencies.FirstOrDefault()?.NestedFamilyName ?? roleName;

        // Map to parent references
        var parentFamilies = dependencies
            .Where(d => d.ParentFamily != null)
            .Select(d => new ParentReferenceDto
            {
                FamilyId = d.ParentFamilyId,
                FamilyName = d.ParentFamily!.FamilyName,
                RoleName = d.ParentFamily.Role?.Name,
                NestedVersionInParent = d.LibraryVersion ?? 0,
                ParentLatestVersion = d.ParentFamily.CurrentVersion
            })
            .OrderBy(p => p.FamilyName)
            .ToList();

        return new UsedInDto
        {
            NestedFamilyName = nestedFamilyName,
            ParentFamilies = parentFamilies
        };
    }

    /// <summary>
    /// Determines the recommended load action for a nested family dependency.
    /// </summary>
    /// <param name="dependency">The family dependency to evaluate.</param>
    /// <returns>The recommended action for loading the nested family.</returns>
    /// <remarks>
    /// Current implementation provides basic logic based on library status.
    /// Full implementation requires plugin coordination for RFA and project versions.
    /// </remarks>
    private static NestedLoadAction DetermineLoadAction(FamilyDependencyEntity dependency)
    {
        // Non-shared families are embedded in the parent - no action needed
        if (!dependency.IsShared)
        {
            return NestedLoadAction.NoAction;
        }

        // If not in library, load from RFA
        if (!dependency.InLibrary)
        {
            return NestedLoadAction.LoadFromRfa;
        }

        // Shared and in library - default to update from library
        // Full logic would compare RFA version, library version, and project version
        // This is a stub implementation until plugin coordination is available
        return NestedLoadAction.UpdateFromLibrary;
    }
}
