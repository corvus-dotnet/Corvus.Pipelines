// <copyright file="NonNullableStringSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class NonNullableStringSteps(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext scenarioContext = scenarioContext;

    [Given("I create the non-nullable strings")]
    public void ICreateTheNonNullableStrings(Table stringTable)
    {
        foreach (TableRow row in stringTable.Rows)
        {
            this.AddNonNullableString(row["Name"], row["Constructor parameter"]);
        }
    }

    [Then("the NonNullableString (.*) should not equal (.*)")]
    public void TheNonNullableStringShouldNotEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreNotEqual(default(NonNullableString), sut);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreNotEqual(expected[1..^1], sut);
        }
        else if (expected == "String.Empty")
        {
            Assert.AreNotEqual(string.Empty, sut);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreNotEqual(new NonNullableString(string.Empty), sut);
        }
        else
                {
            Assert.AreNotEqual(new NonNullableString(expected), sut);
        }
    }

    [Then("the NonNullableString (.*) should equal (.*)")]
    public void TheNonNullableStringShouldEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreEqual(default(NonNullableString), sut);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreEqual(expected[1..^1], (string)sut);
        }
        else if (expected == "String.Empty")
        {
            Assert.AreEqual(string.Empty, (string)sut);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreEqual(new NonNullableString(string.Empty), sut);
        }
        else
        {
            Assert.AreEqual(new NonNullableString(expected), sut);
        }
    }

    [Then("the NonNullableString (.*) NotEqualsHashCode (.*)")]
    public void TheNonNullableStringNotEqualsHashCode(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreNotEqual(default(NonNullableString).GetHashCode(), sut.GetHashCode());
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreNotEqual(expected[1..^1].GetHashCode(), sut.GetHashCode());
        }
        else if (expected == "String.Empty")
        {
            Assert.AreNotEqual(string.Empty.GetHashCode(), sut.GetHashCode());
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreNotEqual(new NonNullableString(string.Empty).GetHashCode(), sut.GetHashCode());
        }
        else
        {
            Assert.AreNotEqual(new NonNullableString(expected).GetHashCode(), sut.GetHashCode());
        }
    }

    [Then("the NonNullableString (.*) EqualsHashCode (.*)")]
    public void TheNonNullableStringEqualsHashCode(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.AreEqual(default(NonNullableString).GetHashCode(), sut.GetHashCode());
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreEqual(expected[1..^1].GetHashCode(), sut.GetHashCode());
        }
        else if (expected == "String.Empty")
        {
            Assert.AreEqual(string.Empty.GetHashCode(), sut.GetHashCode());
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreEqual(new NonNullableString(string.Empty).GetHashCode(), sut.GetHashCode());
        }
        else
        {
            Assert.AreEqual(new NonNullableString(expected).GetHashCode(), sut.GetHashCode());
        }
    }

    [Then("the NonNullableString (.*) ObjectNotEquals (.*)")]
    public void TheNonNullableStringObjectNotEquals(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsFalse(sut.Equals((object)default(NonNullableString)));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsFalse(sut.Equals((object)expected[1..^1]));
        }
        else if (expected == "String.Empty")
        {
            Assert.IsFalse(sut.Equals((object)string.Empty));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsFalse(sut.Equals((object)new NonNullableString(string.Empty)));
        }
        else if (expected == "null")
        {
            Assert.IsFalse(sut.Equals((object?)null));
        }
        else
        {
            Assert.IsFalse(sut.Equals((object)new NonNullableString(expected)));
        }
    }

    [Then("the NonNullableString (.*) ObjectEquals (.*)")]
    public void TheNonNullableStringObjectEquals(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut.Equals((object)default(NonNullableString)));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.Equals((object)expected[1..^1]));
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.Equals((object)string.Empty));
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.Equals((object)new NonNullableString(string.Empty)));
        }
        else if (expected == "null")
        {
            Assert.IsTrue(sut.Equals((object?)null));
        }
        else
        {
            Assert.IsTrue(sut.Equals((object)new NonNullableString(expected)));
        }
    }

    [Then("the NonNullableString (.*) == (.*)")]
    public void TheNonNullableStringShouldEqualOperator(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut == default);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut == expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut == string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut == new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut == new NonNullableString(expected));
        }
    }

    [Then("the NonNullableString (.*) != (.*)")]
    public void TheNonNullableStringShouldNotEqualOperator(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut != default);
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut != expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut != string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut != new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut != new NonNullableString(expected));
        }
    }

    [Then("the NonNullableString.Value (.*) should not equal (.*)")]
    public void TheNonNullableStringValueShouldNotEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            // No assertion for default
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreNotEqual(expected[1..^1], sut.Value);
        }
        else if (expected == "String.Empty")
        {
            Assert.AreNotEqual(string.Empty, sut.Value);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreNotEqual(string.Empty, sut.Value);
        }
        else
        {
            Assert.AreNotEqual(expected, sut.Value);
        }
    }

    [Then("the NonNullableString.Value (.*) should equal (.*)")]
    public void TheNonNullableStringValueShouldEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            // No assertion for default
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.AreEqual(expected[1..^1], sut.Value);
        }
        else if (expected == "String.Empty")
        {
            Assert.AreEqual(string.Empty, sut.Value);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.AreEqual(string.Empty, sut.Value);
        }
        else
        {
            Assert.AreEqual(expected, sut.Value);
        }
    }

    [Then("the NonNullableString.Value (.*) == (.*)")]
    public void TheNonNullableStringValueShouldEqualOperator(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            // No assertion for default
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.Value == expected[1..^1]);
        }
#pragma warning disable RCS1156 // Use string.Length instead of comparison with empty string.
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.Value == string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.Value == string.Empty);
        }
#pragma warning restore RCS1156 // Use string.Length instead of comparison with empty string.
        else
        {
            Assert.IsTrue(sut.Value == expected);
        }
    }

    [Then("the NonNullableString.Value (.*) != (.*)")]
    public void TheNonNullableStringValueShouldNotEqualOperator(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            // No assertion for default
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut.Value != expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut.Value != string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut.Value != string.Empty);
        }
        else
        {
            Assert.IsTrue(sut.Value != expected);
        }
    }

    [Then("the NonNullableString.Value (.*) <= (.*)")]
    public void TheNonNullableStringValueShouldBeLessThanOrEqualOperator(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            // No assertion for default
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut <= expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut <= string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut <= string.Empty);
        }
        else
        {
            Assert.IsTrue(sut <= expected);
        }
    }

    [Then("the NonNullableString (.*) >= (.*)")]
    public void TheNonNullableStringShouldBeGreaterThanOrEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut >= default(NonNullableString));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut >= expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut >= string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut >= new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut >= new NonNullableString(expected));
        }
    }

    [Then("the NonNullableString (.*) > (.*)")]
    public void TheNonNullableStringShouldBeGreaterThan(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut > default(NonNullableString));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut > expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut > string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut > new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut > new NonNullableString(expected));
        }
    }

    [Then("the NonNullableString (.*) < (.*)")]
    public void TheNonNullableStringShouldBeLessThan(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut < default(NonNullableString));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut < expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut < string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut < new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut < new NonNullableString(expected));
        }
    }

    [Then("the NonNullableString (.*) <= (.*)")]
    public void TheNonNullableStringShouldBeLessThanOrEqual(string name, string expected)
    {
        NonNullableString sut = this.scenarioContext.Get<NonNullableString>(name);

        if (expected == "default(NonNullableString)")
        {
            Assert.IsTrue(sut <= default(NonNullableString));
        }
        else if (expected.StartsWith('"') && expected.EndsWith('"'))
        {
            Assert.IsTrue(sut <= expected[1..^1]);
        }
        else if (expected == "String.Empty")
        {
            Assert.IsTrue(sut <= string.Empty);
        }
        else if (expected == "new NonNullableString(String.Empty)")
        {
            Assert.IsTrue(sut <= new NonNullableString(string.Empty));
        }
        else
        {
            Assert.IsTrue(sut <= new NonNullableString(expected));
        }
    }

    private void AddNonNullableString(string name, string constructorParameter)
    {
        if (constructorParameter == "default(NonNullableString)")
        {
            this.scenarioContext.Add(name, default(NonNullableString));
        }
        else if (constructorParameter.StartsWith('"') && constructorParameter.EndsWith('"'))
        {
            this.scenarioContext.Add(name, new NonNullableString(constructorParameter[1..^1]));
        }
        else if (constructorParameter == "String.Empty")
        {
            this.scenarioContext.Add(name, new NonNullableString(string.Empty));
        }
        else
        {
            this.scenarioContext.Add(name, new NonNullableString(constructorParameter));
        }
    }
}