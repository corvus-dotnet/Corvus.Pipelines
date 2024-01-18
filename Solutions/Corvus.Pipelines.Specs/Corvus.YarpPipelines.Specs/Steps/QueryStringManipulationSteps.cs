// <copyright file="QueryStringManipulationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.YarpPipelines;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.YarpPipelines.Specs.Steps;

[Binding]
public class QueryStringManipulationSteps
{
    private string? queryStringIn;
    private ReadOnlyMemory<char> queryStringOut;

    [Given("the query string '([^']*)'")]
    public void GivenTheQueryString(string queryString)
    {
        this.queryStringIn = queryString;
    }

    [When("I remove '([^']*)' from the query string")]
    public void WhenIRemoveFromTheQueryString(string parameterName)
    {
        this.queryStringOut = this.queryStringIn.AsMemory().Without(parameterName);
    }

    [Then("the modified query string should be '([^']*)'")]
    public void ThenTheModifiedQueryStringShouldBe(string expectedQueryString)
    {
        Assert.AreEqual(expectedQueryString, this.queryStringOut.ToString());
    }
}