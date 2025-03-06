// <copyright file="QueryStringManipulationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.YarpPipelines;

using NUnit.Framework;

using Reqnroll;

namespace Corvus.YarpPipelines.Specs.Steps;

[Binding]
public class QueryStringManipulationSteps
{
    private string? queryStringIn;
    private ReadOnlyMemory<char> queryStringOut;
    private List<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)> nameValuePairs = [];

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

    [When("I enumerate the query string")]
    public void WhenIEnumerateTheQueryString()
    {
        this.nameValuePairs = this.queryStringIn.AsMemory().EnumerateQueryNameValues().ToList();
    }

    [Then("the enumerated query string results should be")]
    public void ThenTheEnumeratedQueryStringResultsShouldBe(DataTable dataTable)
    {
        Assert.AreEqual(dataTable.RowCount, this.nameValuePairs.Count, "Number of name value pairs");

        int rowIndex = 0;
        foreach (NameValueRow expectedRow in dataTable.CreateSet<NameValueRow>())
        {
            (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) resultRow = this.nameValuePairs[rowIndex++];
            Assert.AreEqual(expectedRow.Name, resultRow.Name.ToString());
            Assert.AreEqual(expectedRow.Value, resultRow.Value.ToString());
        }
    }

    private record NameValueRow(string Name, string Value);
}