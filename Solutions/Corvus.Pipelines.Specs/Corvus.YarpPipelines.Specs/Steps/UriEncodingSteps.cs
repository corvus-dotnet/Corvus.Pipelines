// <copyright file="UriEncodingSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.YarpPipelines.Internal;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.YarpPipelines.Specs.Steps;

[Binding]
public class UriEncodingSteps
{
    private string? prefix;
    private string? path;
    private string? queryString;

    private string? result;

    [Given("the prefix, path, and query string '([^']*)', '([^']*)', and '([^']*)'")]
    public void GivenThePrefixPathAndQueryString(string prefix, string path, string queryString)
    {
        this.prefix = prefix;
        this.path = path;
        this.queryString = queryString;
    }

    [When("I append the prefix, encoded path, and query")]
    public void WhenIAppendThePrefixEncodedPathAndQuery()
    {
        this.result = LowAllocUriUtilities.EncodePathAndAppendEncodedQueryString(this.prefix, this.path, this.queryString);
    }

    [Then(@"the result URL should be '([^']*)'")]
    public void ThenTheResultURLShouldBe(string expectedResult)
    {
        Assert.AreEqual(expectedResult, this.result);
    }
}