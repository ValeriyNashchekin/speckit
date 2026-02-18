using FluentAssertions;
using Xunit;
using FamilyLibrary.Infrastructure.Services;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.Tests.Services;

public class ChangeDetectionServiceTests
{
    private readonly ChangeDetectionService _sut;

    public ChangeDetectionServiceTests()
    {
        _sut = new ChangeDetectionService();
    }

    #region Null Previous Snapshot (New Family) Tests

    [Fact]
    public void DetectChanges_WithNullPrevious_ReturnsNewFamilyChanges()
    {
        // Arrange
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "NewFamily",
            Category = "Doors",
            Types = ["Type1", "Type2"],
            Parameters =
            [
                new ParameterSnapshot { Name = "Width", Value = "1000" },
                new ParameterSnapshot { Name = "Height", Value = "2000" }
            ],
            TxtHash = "hash123"
        };

        // Act
        var result = _sut.DetectChanges(null, current);

        // Assert
        result.HasChanges.Should().BeTrue();
        result.Items.Should().HaveCount(5);

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Name
            && i.PreviousValue == null
            && i.CurrentValue == "NewFamily");

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Category
            && i.PreviousValue == null
            && i.CurrentValue == "Doors");

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Types
            && i.AddedItems!.Count == 2);

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Parameters
            && i.ParameterChanges!.Count == 2);

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Txt
            && i.PreviousValue == null
            && i.CurrentValue == "hash123");
    }

    [Fact]
    public void DetectChanges_WithNullPreviousAndEmptyCollections_ReturnsOnlyBasicChanges()
    {
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "EmptyFamily",
            Category = "Windows",
            Types = [],
            Parameters = [],
            TxtHash = null
        };

        var result = _sut.DetectChanges(null, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Name);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Category);
    }

    #endregion

    #region No Changes Tests

    [Fact]
    public void DetectChanges_WithNoChanges_ReturnsEmptyChangeSet()
    {
        var snapshot = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "SameFamily",
            Category = "Doors",
            Types = ["Type1", "Type2"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "sameHash"
        };

        var result = _sut.DetectChanges(snapshot, snapshot);

        result.HasChanges.Should().BeFalse();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_WithIdenticalSnapshots_ReturnsEmptyChangeSet()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "Family",
            Category = "Category",
            Types = ["A", "B"],
            Parameters = [new ParameterSnapshot { Name = "P1", Value = "V1" }],
            HasGeometryChanges = false,
            TxtHash = "hash"
        };

        var current = new FamilySnapshot
        {
            Version = 2,
            FamilyName = "Family",
            Category = "Category",
            Types = ["A", "B"],
            Parameters = [new ParameterSnapshot { Name = "P1", Value = "V1" }],
            HasGeometryChanges = false,
            TxtHash = "hash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeFalse();
        result.Items.Should().BeEmpty();
    }

    #endregion

    #region Name Change Tests

    [Fact]
    public void DetectChanges_WithNameChange_DetectsNameChange()
    {
        var previous = CreateBasicSnapshot();
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "NewFamilyName",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Name);

        var nameChange = result.Items.First(i => i.Category == ChangeCategory.Name);
        nameChange.PreviousValue.Should().Be("TestFamily");
        nameChange.CurrentValue.Should().Be("NewFamilyName");
    }

    #endregion

    #region Category Change Tests

    [Fact]
    public void DetectChanges_WithCategoryChange_DetectsCategoryChange()
    {
        var previous = CreateBasicSnapshot();
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "NewCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Category);

        var categoryChange = result.Items.First(i => i.Category == ChangeCategory.Category);
        categoryChange.PreviousValue.Should().Be("TestCategory");
        categoryChange.CurrentValue.Should().Be("NewCategory");
    }

    #endregion

    #region Types Change Tests

    [Fact]
    public void DetectChanges_WithTypesAdded_DetectsTypesAdded()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type1"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type1", "Type2", "Type3"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Types);

        var typesChange = result.Items.First(i => i.Category == ChangeCategory.Types);
        typesChange.AddedItems.Should().BeEquivalentTo(["Type2", "Type3"]);
        typesChange.RemovedItems.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_WithTypesRemoved_DetectsTypesRemoved()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type1", "Type2", "Type3"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type1"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Types);

        var typesChange = result.Items.First(i => i.Category == ChangeCategory.Types);
        typesChange.AddedItems.Should().BeEmpty();
        typesChange.RemovedItems.Should().BeEquivalentTo(["Type2", "Type3"]);
    }

    [Fact]
    public void DetectChanges_WithTypesAddedAndRemoved_DetectsBothChanges()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type1", "Type2"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["Type2", "Type3"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        var typesChange = result.Items.First(i => i.Category == ChangeCategory.Types);
        typesChange.AddedItems.Should().BeEquivalentTo(["Type3"]);
        typesChange.RemovedItems.Should().BeEquivalentTo(["Type1"]);
    }

    #endregion

    #region Parameters Change Tests

    [Fact]
    public void DetectChanges_WithParameterAdded_DetectsParameterAdded()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters =
            [
                new ParameterSnapshot { Name = "Width", Value = "1000" },
                new ParameterSnapshot { Name = "Height", Value = "2000" }
            ],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Parameters);

        var paramChange = result.Items.First(i => i.Category == ChangeCategory.Parameters);
        paramChange.ParameterChanges.Should().ContainSingle(p =>
            p.Name == "Height" && p.PreviousValue == null && p.CurrentValue == "2000");
    }

    [Fact]
    public void DetectChanges_WithParameterRemoved_DetectsParameterRemoved()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters =
            [
                new ParameterSnapshot { Name = "Width", Value = "1000" },
                new ParameterSnapshot { Name = "Height", Value = "2000" }
            ],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();

        var paramChange = result.Items.First(i => i.Category == ChangeCategory.Parameters);
        paramChange.ParameterChanges.Should().ContainSingle(p =>
            p.Name == "Height" && p.PreviousValue == "2000" && p.CurrentValue == null);
    }

    [Fact]
    public void DetectChanges_WithParameterModified_DetectsParameterChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1500" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();

        var paramChange = result.Items.First(i => i.Category == ChangeCategory.Parameters);
        paramChange.ParameterChanges.Should().ContainSingle(p =>
            p.Name == "Width" && p.PreviousValue == "1000" && p.CurrentValue == "1500");
    }

    [Fact]
    public void DetectChanges_WithMultipleParameterChanges_DetectsAllChanges()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters =
            [
                new ParameterSnapshot { Name = "Width", Value = "1000" },
                new ParameterSnapshot { Name = "Height", Value = "2000" },
                new ParameterSnapshot { Name = "Depth", Value = "500" }
            ],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters =
            [
                new ParameterSnapshot { Name = "Width", Value = "1500" },
                new ParameterSnapshot { Name = "Height", Value = "2000" },
                new ParameterSnapshot { Name = "NewParam", Value = "100" }
            ],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        var paramChange = result.Items.First(i => i.Category == ChangeCategory.Parameters);
        paramChange.ParameterChanges.Should().HaveCount(3);

        paramChange.ParameterChanges.Should().Contain(p =>
            p.Name == "Width" && p.PreviousValue == "1000" && p.CurrentValue == "1500");

        paramChange.ParameterChanges.Should().Contain(p =>
            p.Name == "Depth" && p.PreviousValue == "500" && p.CurrentValue == null);

        paramChange.ParameterChanges.Should().Contain(p =>
            p.Name == "NewParam" && p.PreviousValue == null && p.CurrentValue == "100");
    }

    [Fact]
    public void DetectChanges_WithUnchangedParameterValue_NoParameterChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.Items.Should().NotContain(i => i.Category == ChangeCategory.Parameters);
    }

    #endregion

    #region Geometry Change Tests

    [Fact]
    public void DetectChanges_WithGeometryChange_DetectsGeometryChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = true,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Geometry);

        var geometryChange = result.Items.First(i => i.Category == ChangeCategory.Geometry);
        geometryChange.PreviousValue.Should().BeNull();
        geometryChange.CurrentValue.Should().BeNull();
    }

    [Fact]
    public void DetectChanges_WithoutGeometryChange_NoGeometryChangeItem()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "testHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.Items.Should().NotContain(i => i.Category == ChangeCategory.Geometry);
    }

    #endregion

    #region TXT Hash Change Tests

    [Fact]
    public void DetectChanges_WithTxtHashChange_DetectsTxtChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "oldHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "newHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().ContainSingle(i => i.Category == ChangeCategory.Txt);

        var txtChange = result.Items.First(i => i.Category == ChangeCategory.Txt);
        txtChange.PreviousValue.Should().Be("oldHash");
        txtChange.CurrentValue.Should().Be("newHash");
    }

    [Fact]
    public void DetectChanges_WithTxtHashAdded_DetectsTxtChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = null
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "newHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Txt);

        var txtChange = result.Items.First(i => i.Category == ChangeCategory.Txt);
        txtChange.PreviousValue.Should().BeNull();
        txtChange.CurrentValue.Should().Be("newHash");
    }

    [Fact]
    public void DetectChanges_WithTxtHashRemoved_DetectsTxtChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "oldHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = null
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Txt);

        var txtChange = result.Items.First(i => i.Category == ChangeCategory.Txt);
        txtChange.PreviousValue.Should().Be("oldHash");
        txtChange.CurrentValue.Should().BeNull();
    }

    [Fact]
    public void DetectChanges_WithSameTxtHash_NoTxtChange()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "sameHash"
        };
        var current = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "TestFamily",
            Category = "TestCategory",
            Types = ["TestType"],
            Parameters = [],
            HasGeometryChanges = false,
            TxtHash = "sameHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.Items.Should().NotContain(i => i.Category == ChangeCategory.Txt);
    }

    #endregion

    #region Multiple Changes Tests

    [Fact]
    public void DetectChanges_WithMultipleChanges_DetectsAllChanges()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "OldFamily",
            Category = "OldCategory",
            Types = ["Type1"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1000" }],
            HasGeometryChanges = false,
            TxtHash = "oldHash"
        };

        var current = new FamilySnapshot
        {
            Version = 2,
            FamilyName = "NewFamily",
            Category = "NewCategory",
            Types = ["Type1", "Type2"],
            Parameters = [new ParameterSnapshot { Name = "Width", Value = "1500" }],
            HasGeometryChanges = true,
            TxtHash = "newHash"
        };

        var result = _sut.DetectChanges(previous, current);

        result.HasChanges.Should().BeTrue();
        result.Items.Should().HaveCount(6);

        result.Items.Should().Contain(i => i.Category == ChangeCategory.Name);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Category);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Types);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Parameters);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Geometry);
        result.Items.Should().Contain(i => i.Category == ChangeCategory.Txt);
    }

    [Fact]
    public void DetectChanges_WithAllFieldsChanged_DetectsAllChanges()
    {
        var previous = new FamilySnapshot
        {
            Version = 1,
            FamilyName = "A",
            Category = "A",
            Types = ["A"],
            Parameters = [new ParameterSnapshot { Name = "A", Value = "A" }],
            HasGeometryChanges = false,
            TxtHash = "A"
        };

        var current = new FamilySnapshot
        {
            Version = 2,
            FamilyName = "B",
            Category = "B",
            Types = ["B"],
            Parameters = [new ParameterSnapshot { Name = "B", Value = "B" }],
            HasGeometryChanges = true,
            TxtHash = "B"
        };

        var result = _sut.DetectChanges(previous, current);

        result.Items.Should().HaveCount(6);

        result.Items.First(i => i.Category == ChangeCategory.Name).CurrentValue.Should().Be("B");
        result.Items.First(i => i.Category == ChangeCategory.Category).CurrentValue.Should().Be("B");
        result.Items.First(i => i.Category == ChangeCategory.Types).AddedItems.Should().Contain("B");
        result.Items.First(i => i.Category == ChangeCategory.Types).RemovedItems.Should().Contain("A");
        result.Items.First(i => i.Category == ChangeCategory.Geometry).Should().NotBeNull();
        result.Items.First(i => i.Category == ChangeCategory.Txt).CurrentValue.Should().Be("B");
    }

    #endregion

    #region Helper Methods

    private static FamilySnapshot CreateBasicSnapshot() => new()
    {
        Version = 1,
        FamilyName = "TestFamily",
        Category = "TestCategory",
        Types = ["TestType"],
        Parameters = [],
        HasGeometryChanges = false,
        TxtHash = "testHash"
    };

    #endregion
}
