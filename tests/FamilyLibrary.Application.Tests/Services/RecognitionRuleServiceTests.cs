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

public class RecognitionRuleServiceTests
{
    private readonly Mock<IRecognitionRuleRepository> _repositoryMock;
    private readonly Mock<IFamilyRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RecognitionRuleService _sut;

    public RecognitionRuleServiceTests()
    {
        _repositoryMock = new Mock<IRecognitionRuleRepository>();
        _roleRepositoryMock = new Mock<IFamilyRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new RecognitionRuleService(_repositoryMock.Object, _roleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region ValidateFormulaAsync Tests

    [Fact]
    public async Task ValidateFormulaAsync_WithValidSimplePattern_ShouldReturnTrue()
    {
        // Arrange
        var formula = "Door";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithValidAndFormula_ShouldReturnTrue()
    {
        // Arrange
        var formula = "Door AND Window";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithValidOrFormula_ShouldReturnTrue()
    {
        // Arrange
        var formula = "Door OR Window";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithValidNotFormula_ShouldReturnTrue()
    {
        // Arrange
        var formula = "NOT Window";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithValidParentheses_ShouldReturnTrue()
    {
        // Arrange
        var formula = "(Door OR Window) AND Wall";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithComplexFormula_ShouldReturnTrue()
    {
        // Arrange
        var formula = "(FB OR Desk) AND Wired";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithEmptyFormula_ShouldReturnFalse()
    {
        // Arrange
        var formula = "";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithWhitespaceFormula_ShouldReturnFalse()
    {
        // Arrange
        var formula = "   ";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithMismatchedParentheses_ShouldReturnFalse()
    {
        // Arrange
        var formula = "(Door AND Window";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithConsecutiveOperators_ShouldReturnFalse()
    {
        // Arrange
        var formula = "Door AND AND Window";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFormulaAsync_WithOperatorAtEnd_ShouldReturnFalse()
    {
        // Arrange
        var formula = "Door AND";

        // Act
        var result = await _sut.ValidateFormulaAsync(formula);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult_WhenRulesExist()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rules = new List<RecognitionRuleEntity>
        {
            new(roleId, "Root1", "Door"),
            new(Guid.NewGuid(), "Root2", "Window")
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _sut.GetAllAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoRulesExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecognitionRuleEntity>());

        // Act
        var result = await _sut.GetAllAsync(1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ShouldApplyPagingCorrectly()
    {
        // Arrange
        var rules = new List<RecognitionRuleEntity>();
        for (int i = 0; i < 25; i++)
        {
            rules.Add(new RecognitionRuleEntity(Guid.NewGuid(), $"Root{i}", $"Pattern{i}"));
        }
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _sut.GetAllAsync(2, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRule_WhenRuleExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rule = new RecognitionRuleEntity(roleId, "RootNode", "Door AND Window");
        _repositoryMock.Setup(r => r.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _sut.GetByIdAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Formula.Should().Be("Door AND Window");
        result.RootNode.Should().Be("RootNode");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRuleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecognitionRuleEntity?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateRule_WhenValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var dto = new CreateRecognitionRuleDto
        {
            RoleId = roleId,
            RootNode = "RootNode",
            Formula = "Door"
        };
        _roleRepositoryMock.Setup(r => r.ExistsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecognitionRuleEntity?)null);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<RecognitionRuleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<RecognitionRuleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var dto = new CreateRecognitionRuleDto
        {
            RoleId = roleId,
            RootNode = "RootNode",
            Formula = "Door"
        };
        _roleRepositoryMock.Setup(r => r.ExistsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var act = () => _sut.CreateAsync(dto);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRuleAlreadyExistsForRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var dto = new CreateRecognitionRuleDto
        {
            RoleId = roleId,
            RootNode = "RootNode",
            Formula = "Door"
        };
        var existingRule = new RecognitionRuleEntity(roleId, "ExistingRoot", "Window");
        _roleRepositoryMock.Setup(r => r.ExistsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        // Act & Assert
        var act = () => _sut.CreateAsync(dto);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.PropertyName == nameof(dto.RoleId));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenFormulaIsInvalid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var dto = new CreateRecognitionRuleDto
        {
            RoleId = roleId,
            RootNode = "RootNode",
            Formula = ""
        };
        _roleRepositoryMock.Setup(r => r.ExistsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = () => _sut.CreateAsync(dto);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.PropertyName == nameof(dto.Formula));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRule_WhenRuleExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenRuleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var act = () => _sut.DeleteAsync(id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region TestRuleAsync Tests

    [Fact]
    public async Task TestRuleAsync_ShouldReturnTrue_WhenFormulaMatches()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rule = new RecognitionRuleEntity(roleId, "Root", "Door");
        _repositoryMock.Setup(r => r.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _sut.TestRuleAsync(rule.Id, "MyDoorFamily");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestRuleAsync_ShouldReturnFalse_WhenFormulaDoesNotMatch()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rule = new RecognitionRuleEntity(roleId, "Root", "Door");
        _repositoryMock.Setup(r => r.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _sut.TestRuleAsync(rule.Id, "MyWindowFamily");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestRuleAsync_ShouldThrowNotFoundException_WhenRuleDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecognitionRuleEntity?)null);

        // Act & Assert
        var act = () => _sut.TestRuleAsync(id, "TestFamily");
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TestRuleAsync_ShouldEvaluateComplexFormulaCorrectly()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rule = new RecognitionRuleEntity(roleId, "Root", "(FB OR Desk) AND Wired");
        _repositoryMock.Setup(r => r.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var resultMatch = await _sut.TestRuleAsync(rule.Id, "FB_Wired_Type");
        var resultNoMatch = await _sut.TestRuleAsync(rule.Id, "FB_Wireless_Type");

        // Assert
        resultMatch.Should().BeTrue();
        resultNoMatch.Should().BeFalse();
    }

    #endregion

    #region CheckConflictsAsync Tests

    [Fact]
    public async Task CheckConflictsAsync_ShouldReturnEmpty_WhenNoConflicts()
    {
        // Arrange
        var role1 = new FamilyRoleEntity("Doors", RoleType.Loadable, null, null);
        var role2 = new FamilyRoleEntity("Windows", RoleType.Loadable, null, null);
        var rules = new List<RecognitionRuleEntity>
        {
            new(role1.Id, "Root1", "Door"),
            new(role2.Id, "Root2", "Window")
        };
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[0], role1);
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[1], role2);
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _sut.CheckConflictsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckConflictsAsync_ShouldDetectConflict_WhenPatternsOverlap()
    {
        // Arrange
        var role1 = new FamilyRoleEntity("Doors", RoleType.Loadable, null, null);
        var role2 = new FamilyRoleEntity("SpecialDoors", RoleType.Loadable, null, null);
        var rules = new List<RecognitionRuleEntity>
        {
            new(role1.Id, "Root1", "Door"),
            new(role2.Id, "Root2", "DoorSpecial")
        };
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[0], role1);
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[1], role2);
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _sut.CheckConflictsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].RuleId1.Should().Be(rules[0].Id);
        result[0].RuleId2.Should().Be(rules[1].Id);
    }

    [Fact]
    public async Task CheckConflictsAsync_ShouldExcludeSpecifiedRule()
    {
        // Arrange
        var role1 = new FamilyRoleEntity("Doors", RoleType.Loadable, null, null);
        var role2 = new FamilyRoleEntity("SpecialDoors", RoleType.Loadable, null, null);
        var rules = new List<RecognitionRuleEntity>
        {
            new(role1.Id, "Root1", "Door"),
            new(role2.Id, "Root2", "DoorSpecial")
        };
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[0], role1);
        typeof(RecognitionRuleEntity).GetProperty("Role")!
            .SetValue(rules[1], role2);
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _sut.CheckConflictsAsync(rules[0].Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
