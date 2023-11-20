// <copyright file="NonNullableStringIgnoreCaseSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class NonNullableStringIgnoreCaseSteps(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext scenarioContext = scenarioContext;

    [Then("ignoring case the NonNullableString (.*) should not equal (.*)")]
    public void TheNonNullableStringShouldNotEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsFalse(NonNullableString.Equals(default, sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsFalse(NonNullableString.Equals(expected[1..^1], sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "String.Empty")
        {
            Assert.IsFalse(NonNullableString.Equals(string.Empty, sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsFalse(NonNullableString.Equals(new NonNullableString(string.Empty), sut, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Assert.IsFalse(NonNullableString.Equals(new NonNullableString(expected), sut, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Then("ignoring case the NonNullableString (.*) should equal (.*)")]
    public void TheNonNullableStringShouldEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(NonNullableString.Equals(default, sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(NonNullableString.Equals(expected[1..^1], sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(NonNullableString.Equals(string.Empty, sut, StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(NonNullableString.Equals(new NonNullableString(string.Empty), sut, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Assert.IsTrue(NonNullableString.Equals(new NonNullableString(expected), sut, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Then("ignoring case the NonNullableString (.*) == (.*)")]
    public void TheNonNullableStringValueShouldBeEqualCompareTo(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.CompareTo(default, StringComparison.OrdinalIgnoreCase) == 0);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.CompareTo(expected[1..^1], StringComparison.OrdinalIgnoreCase) == 0);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.CompareTo(string.Empty, StringComparison.OrdinalIgnoreCase) == 0);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(string.Empty), StringComparison.OrdinalIgnoreCase) == 0);
        }
        else
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(expected), StringComparison.OrdinalIgnoreCase) == 0);
        }
    }

    [Then("ignoring case the NonNullableString (.*) >= (.*)")]
    public void TheNonNullableStringValueShouldBeGreaterThanOrEqualCompareTo(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.CompareTo(default, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.CompareTo(expected[1..^1], StringComparison.OrdinalIgnoreCase) >= 0);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.CompareTo(string.Empty, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
        }
        else
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(expected), StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    [Then("ignoring case the NonNullableString (.*) > (.*)")]
    public void TheNonNullableStringValueShouldBeGreaterThanCompareTo(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.CompareTo(default, StringComparison.OrdinalIgnoreCase) == 1);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.CompareTo(expected[1..^1], StringComparison.OrdinalIgnoreCase) == 1);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.CompareTo(string.Empty, StringComparison.OrdinalIgnoreCase) == 1);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(string.Empty), StringComparison.OrdinalIgnoreCase) == 1);
        }
        else
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(expected), StringComparison.OrdinalIgnoreCase) == 1);
        }
    }

    [Then("ignoring case the NonNullableString (.*) < (.*)")]
    public void TheNonNullableStringValueShouldBeLessThanCompareTo(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.CompareTo(default, StringComparison.OrdinalIgnoreCase) == -1);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.CompareTo(expected[1..^1], StringComparison.OrdinalIgnoreCase) == -1);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.CompareTo(string.Empty, StringComparison.OrdinalIgnoreCase) == -1);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(string.Empty), StringComparison.OrdinalIgnoreCase) == -1);
        }
        else
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(expected), StringComparison.OrdinalIgnoreCase) == -1);
        }
    }

    [Then("ignoring case the NonNullableString (.*) <= (.*)")]
    public void TheNonNullableStringValueShouldBeLessThanOrEqualCompareTo(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.CompareTo(default, StringComparison.OrdinalIgnoreCase) < 1);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.CompareTo(expected[1..^1], StringComparison.OrdinalIgnoreCase) < 1);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.CompareTo(string.Empty, StringComparison.OrdinalIgnoreCase) < 1);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(string.Empty), StringComparison.OrdinalIgnoreCase) < 1);
        }
        else
        {
            Assert.IsTrue(sut.CompareTo(new NonNullableString(expected), StringComparison.OrdinalIgnoreCase) < 1);
        }
    }

    [Then("ignoring case the NonNullableString (.*) NotEqualsHashCode (.*)")]
    public void TheNonNullableStringNotEqualsHashCode(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreNotEqual(default(NonNullableString).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreNotEqual(expected[1..^1].GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "String.Empty")
        {
            Assert.AreNotEqual(string.Empty.GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreNotEqual(new NonNullableString(string.Empty).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Assert.AreNotEqual(new NonNullableString(expected).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
    }

    [Then("ignoring case the NonNullableString (.*) EqualsHashCode (.*)")]
    public void TheNonNullableStringEqualsHashCode(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreEqual(default(NonNullableString).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreEqual(expected[1..^1].GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "String.Empty")
        {
            Assert.AreEqual(string.Empty.GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreEqual(new NonNullableString(string.Empty).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Assert.AreEqual(new NonNullableString(expected).GetHashCode(StringComparison.OrdinalIgnoreCase), sut.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
    }
}