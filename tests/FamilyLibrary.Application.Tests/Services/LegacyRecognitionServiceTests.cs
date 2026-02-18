using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace FamilyLibrary.Application.Tests.Services;

/// <summary>
/// Unit tests for LegacyRecognitionService matching logic.
/// Tests the recognition rule evaluation algorithm.
/// </summary>
public class LegacyRecognitionServiceTests
{
    private readonly RecognitionRuleMatcher _matcher;

    public LegacyRecognitionServiceTests()
    {
        _matcher = new RecognitionRuleMatcher();
    }

    [Fact]
    public void MatchRole_ContainsCondition_ReturnsRole()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("DoorRole", CreateCondition("Door", "Contains"))
        };

        var result = _matcher.MatchRole(rules, "MyDoorFamily");

        result.Should().Be("DoorRole");
    }

    [Fact]
    public void MatchRole_NotContainsCondition_ReturnsRole()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("NotWindowRole", CreateCondition("Window", "NotContains"))
        };

        var result = _matcher.MatchRole(rules, "MyDoorFamily");

        result.Should().Be("NotWindowRole");
    }

    [Fact]
    public void MatchRole_NotContainsCondition_WhenContains_ReturnsNull()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("NotWindowRole", CreateCondition("Window", "NotContains"))
        };

        var result = _matcher.MatchRole(rules, "MyWindowFamily");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_MultipleMatchingRules_ReturnsFirstMatch()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("FirstRole", CreateCondition("Door")),
            CreateRule("SecondRole", CreateCondition("Door"))
        };

        var result = _matcher.MatchRole(rules, "MyDoorFamily");

        result.Should().Be("FirstRole");
    }

    [Fact]
    public void MatchRole_NoMatchingRule_ReturnsNull()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("DoorRole", CreateCondition("Door")),
            CreateRule("WindowRole", CreateCondition("Window"))
        };

        var result = _matcher.MatchRole(rules, "MyWallFamily");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_AndGroup_AllConditionsMatch_ReturnsRole()
    {
        var andGroup = CreateGroup("AND",
            CreateCondition("FB"),
            CreateCondition("Wired")
        );
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("WiredDeskRole", andGroup)
        };

        var result = _matcher.MatchRole(rules, "FB_Wired_Desk");

        result.Should().Be("WiredDeskRole");
    }

    [Fact]
    public void MatchRole_AndGroup_OneConditionFails_ReturnsNull()
    {
        var andGroup = CreateGroup("AND",
            CreateCondition("FB"),
            CreateCondition("Wired")
        );
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("WiredDeskRole", andGroup)
        };

        var result = _matcher.MatchRole(rules, "FB_Wireless_Desk");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_OrGroup_AnyConditionMatches_ReturnsRole()
    {
        var orGroup = CreateGroup("OR",
            CreateCondition("FB"),
            CreateCondition("Desk")
        );
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("FurnitureRole", orGroup)
        };

        var result = _matcher.MatchRole(rules, "MyDeskFamily");

        result.Should().Be("FurnitureRole");
    }

    [Fact]
    public void MatchRole_OrGroup_NoConditionMatches_ReturnsNull()
    {
        var orGroup = CreateGroup("OR",
            CreateCondition("FB"),
            CreateCondition("Desk")
        );
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("FurnitureRole", orGroup)
        };

        var result = _matcher.MatchRole(rules, "MyChairFamily");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_EmptyRules_ReturnsNull()
    {
        var rules = new List<TestRecognitionRule>();

        var result = _matcher.MatchRole(rules, "MyDoorFamily");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_EmptyFamilyName_ReturnsNull()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("DoorRole", CreateCondition("Door"))
        };

        var result = _matcher.MatchRole(rules, "");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_NullFamilyName_ReturnsNull()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("DoorRole", CreateCondition("Door"))
        };

        var result = _matcher.MatchRole(rules, null!);

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_CaseInsensitiveMatch_ReturnsRole()
    {
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("DoorRole", CreateCondition("DOOR"))
        };

        var result = _matcher.MatchRole(rules, "mydoorfamily");

        result.Should().Be("DoorRole");
    }

    [Fact]
    public void MatchRole_NestedAndOrGroup_ReturnsRole()
    {
        var innerOr = CreateGroup("OR",
            CreateCondition("FB"),
            CreateCondition("Desk")
        );
        var outerAnd = CreateGroup("AND",
            innerOr,
            CreateCondition("Wired")
        );
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("WiredFurnitureRole", outerAnd)
        };

        var result = _matcher.MatchRole(rules, "FB_Wired_Type");

        result.Should().Be("WiredFurnitureRole");
    }

    [Fact]
    public void MatchRole_EmptyGroupChildren_ReturnsNull()
    {
        var emptyGroup = CreateGroup("AND");
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("EmptyGroupRole", emptyGroup)
        };

        var result = _matcher.MatchRole(rules, "AnyFamily");

        result.Should().BeNull();
    }

    [Fact]
    public void MatchRole_EmptyConditionValue_ReturnsNull()
    {
        var emptyCondition = CreateCondition("");
        var rules = new List<TestRecognitionRule>
        {
            CreateRule("EmptyConditionRole", emptyCondition)
        };

        var result = _matcher.MatchRole(rules, "AnyFamily");

        result.Should().BeNull();
    }

    #region Helper Methods

    private TestRecognitionRule CreateRule(string roleName, TestRecognitionNode rootNode)
    {
        return new TestRecognitionRule
        {
            Id = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            RoleName = roleName,
            RootNode = rootNode
        };
    }

    private TestRecognitionNode CreateCondition(string value, string op = "Contains")
    {
        return new TestRecognitionNode
        {
            Type = "condition",
            Operator = op,
            Value = value
        };
    }

    private TestRecognitionNode CreateGroup(string op, params TestRecognitionNode[] children)
    {
        return new TestRecognitionNode
        {
            Type = "group",
            Operator = op,
            Children = new List<TestRecognitionNode>(children)
        };
    }

    #endregion
}

#region Test Doubles

/// <summary>
/// Matcher that implements the same logic as LegacyRecognitionService.
/// This is extracted for testing without Revit dependencies.
/// </summary>
public class RecognitionRuleMatcher
{
    public string? MatchRole(List<TestRecognitionRule> rules, string familyName)
    {
        if (rules.Count == 0 || string.IsNullOrEmpty(familyName))
            return null;

        foreach (var rule in rules)
        {
            if (EvaluateRule(rule.RootNode, familyName))
                return rule.RoleName;
        }

        return null;
    }

    private bool EvaluateRule(TestRecognitionNode node, string name)
    {
        if (node == null) return false;

        return node.Type == "group"
            ? EvaluateGroup(node, name)
            : EvaluateCondition(node, name);
    }

    private bool EvaluateGroup(TestRecognitionNode node, string name)
    {
        if (node.Children == null || node.Children.Count == 0)
            return false;

        return node.Operator == "AND"
            ? node.Children.All(c => EvaluateRule(c, name))
            : node.Children.Any(c => EvaluateRule(c, name));
    }

    private bool EvaluateCondition(TestRecognitionNode node, string name)
    {
        if (string.IsNullOrEmpty(node.Value))
            return false;

        var contains = name.IndexOf(node.Value, StringComparison.OrdinalIgnoreCase) >= 0;
        return node.Operator == "Contains" ? contains : !contains;
    }
}

/// <summary>
/// Test double for RecognitionRuleDto.
/// </summary>
public class TestRecognitionRule
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public TestRecognitionNode RootNode { get; set; } = new TestRecognitionNode();
}

/// <summary>
/// Test double for RecognitionNode.
/// </summary>
public class TestRecognitionNode
{
    public string Type { get; set; } = "condition";
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public List<TestRecognitionNode>? Children { get; set; }
}

#endregion
