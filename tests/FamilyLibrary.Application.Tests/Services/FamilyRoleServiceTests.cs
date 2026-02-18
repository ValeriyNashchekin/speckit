using FluentAssertions;
using Moq;
using Xunit;
using FamilyLibrary.Application.Services;
using FamilyLibrary.Domain.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Tests.Services;

public class FamilyRoleServiceTests
{
    private readonly Mock<IFamilyRoleRepository> _repositoryMock;
    private readonly FamilyRoleService _sut;

    public FamilyRoleServiceTests()
    {
        _repositoryMock = new Mock<IFamilyRoleRepository>();
        _sut = new FamilyRoleService(_repositoryMock.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult_WhenRolesExist()
    {
        // Arrange
        var roles = new List<FamilyRoleEntity>
        {
            new("Doors", RoleType.Loadable, "Door families", null),
            new("Windows", RoleType.Loadable, "Window families", null)
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.GetAllAsync(1, 10, null, null);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByType_WhenTypeIsProvided()
    {
        // Arrange
        var roles = new List<FamilyRoleEntity>
        {
            new("Doors", RoleType.Loadable, "Door families", null),
            new("SystemWalls", RoleType.System, "System walls", null),
            new("Windows", RoleType.Loadable, "Window families", null)
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.GetAllAsync(1, 10, RoleType.Loadable, null);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCategoryId_WhenCategoryIdIsProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var roles = new List<FamilyRoleEntity>
        {
            new("Doors", RoleType.Loadable, "Door families", categoryId),
            new("Windows", RoleType.Loadable, "Window families", null)
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.GetAllAsync(1, 10, null, categoryId);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldApplyPagingCorrectly()
    {
        // Arrange
        var roles = new List<FamilyRoleEntity>();
        for (int i = 0; i < 25; i++)
        {
            roles.Add(new FamilyRoleEntity($"Role{i}", RoleType.Loadable, null, null));
        }
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act - Page 2 with 10 items per page
        var result = await _sut.GetAllAsync(2, 10, null, null);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoRolesExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FamilyRoleEntity>());

        // Act
        var result = await _sut.GetAllAsync(1, 10, null, null);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var role = new FamilyRoleEntity("Doors", RoleType.Loadable, "Door families", null);
        _repositoryMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _sut.GetByIdAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Doors");
        result.Type.Should().Be(RoleType.Loadable);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyRoleEntity?)null);

        // Act & Assert
        var act = () => _sut.GetByIdAsync(id);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*FamilyRoleEntity*{id}*");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateRole_WhenNameIsUnique()
    {
        // Arrange
        var dto = new CreateFamilyRoleDto
        {
            Name = "Doors",
            Type = RoleType.Loadable,
            Description = "Door families"
        };
        _repositoryMock.Setup(r => r.NameExistsAsync(dto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenNameExists()
    {
        // Arrange
        var dto = new CreateFamilyRoleDto
        {
            Name = "Doors",
            Type = RoleType.Loadable
        };
        _repositoryMock.Setup(r => r.NameExistsAsync(dto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = () => _sut.CreateAsync(dto);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.PropertyName == nameof(dto.Name));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRole_WhenRoleExists()
    {
        // Arrange
        var role = new FamilyRoleEntity("Doors", RoleType.Loadable, "Door families", null);
        var categoryId = Guid.NewGuid();
        var dto = new UpdateFamilyRoleDto
        {
            Description = "Updated description",
            CategoryId = categoryId
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(role.Id, dto);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateFamilyRoleDto { Description = "Updated" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyRoleEntity?)null);

        // Act & Assert
        var act = () => _sut.UpdateAsync(id, dto);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRole_WhenRoleExistsAndHasNoFamilies()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.HasFamiliesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var act = () => _sut.DeleteAsync(id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenRoleHasFamilies()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.HasFamiliesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = () => _sut.DeleteAsync(id);
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.RuleName == "CannotDeleteFamilyRoleWithFamilies");
    }

    #endregion

    #region ImportAsync Tests

    [Fact]
    public async Task ImportAsync_ShouldCreateNonDuplicateRoles()
    {
        // Arrange
        var dtos = new List<CreateFamilyRoleDto>
        {
            new() { Name = "Doors", Type = RoleType.Loadable },
            new() { Name = "Windows", Type = RoleType.Loadable }
        };
        _repositoryMock.Setup(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ImportAsync(dtos);

        // Assert
        result.CreatedCount.Should().Be(2);
        result.SkippedCount.Should().Be(0);
        result.TotalProcessed.Should().Be(2);
    }

    [Fact]
    public async Task ImportAsync_ShouldSkipDuplicates()
    {
        // Arrange
        var dtos = new List<CreateFamilyRoleDto>
        {
            new() { Name = "Doors", Type = RoleType.Loadable },
            new() { Name = "ExistingRole", Type = RoleType.Loadable },
            new() { Name = "Windows", Type = RoleType.Loadable }
        };
        _repositoryMock.Setup(r => r.NameExistsAsync("Doors", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.NameExistsAsync("ExistingRole", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.NameExistsAsync("Windows", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<FamilyRoleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ImportAsync(dtos);

        // Assert
        result.CreatedCount.Should().Be(2);
        result.SkippedCount.Should().Be(1);
        result.SkippedNames.Should().Contain("ExistingRole");
    }

    #endregion
}
