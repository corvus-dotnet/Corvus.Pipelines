// <copyright file="HandlerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reqnroll;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class HandlerSteps(ScenarioCodeGenerationBindings syntaxBindings)
{
    [Given("I produce the handlers")]
    public void IProduceTheHandlers(Table table)
    {
        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{syntaxBindings.Namespace}}
            {
                public static partial class Handlers
                {

            """;

        string handlers = string.Join(
            Environment.NewLine,
            table.Rows.Select(
                s =>
                    $$"""
                    public static readonly {{SyncAsyncPrefix(s)}}PipelineStep<HandlerState<{{s["Input type"]}}, {{s["Output type"]}}>> {{s["Handler name"]}} = {{s["Handler definition"]}};
                    """));

        const string codeSuffix =
            """

                }
            }
            """;

        string code = codePrefix + handlers + codeSuffix;
        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Handlers.cs"));

        static string SyncAsyncPrefix(DataTableRow row)
        {
            return row["Sync or async"] == "sync" ? "Sync" : string.Empty;
        }
    }

    [Then("the (.*) output of \"(.*)\" should be handled with result '([^']*)'")]
    public async Task TheOutputShouldBe(string syncOrAsync, string stepName, string output)
    {
        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            using NUnit.Framework;            
            
            namespace {{syntaxBindings.Namespace}}
            {
                public static partial class Expectations
                {
                    {{SyncOrAsync(syncOrAsync, stepName, output)}}
                }
            }
            """;

        List<SyntaxTree> syntaxTrees =
            [];

        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: $"Expectations.{stepName}.cs"));

        Assembly assembly = syntaxBindings.BuildAndLoadAssembly(syntaxTrees);

        if (syncOrAsync == "sync")
        {
            Action assert = GetSyncAssertion(syntaxBindings.Namespace, assembly, stepName);
            assert();
        }
        else
        {
            Func<ValueTask> assert = GetAsyncAssertion(syntaxBindings.Namespace, assembly, stepName);
            await assert().ConfigureAwait(false);
        }

        static Action GetSyncAssertion(string namespaceName, Assembly assembly, string stepName)
        {
            Type? type = assembly.GetType($"{namespaceName}.Expectations");

            object? expectationObject = type?.GetField($"{stepName}OutputShouldBe", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

            return expectationObject is Action expectation
                ? expectation
                : throw new InvalidOperationException("The assertion could not be loaded.");
        }

        static Func<ValueTask> GetAsyncAssertion(string namespaceName, Assembly assembly, string stepName)
        {
            Type? type = assembly.GetType($"{namespaceName}.Expectations");

            object? expectationObject = type?.GetField($"{stepName}OutputShouldBe", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

            return expectationObject is Func<ValueTask> expectation
                ? expectation
                : throw new InvalidOperationException("The assertion could not be loaded.");
        }

        static string SyncOrAsync(string syncOrAsync, string stepName, string expectedOutput)
        {
            if (syncOrAsync == "sync")
            {
                return $$"""
                    public static Action {{stepName}}OutputShouldBe = () =>
                    {
                        var state = Executions.Execute{{stepName}}();
                        Assert.IsTrue(state.WasHandled(out var result), "WasHandled");
                        Assert.AreEqual({{expectedOutput}}, result, "Result");
                    };
                    """;
            }

            return $$"""
                public static Func<ValueTask> {{stepName}}OutputShouldBe = async () =>
                {
                    var state = await Executions.Execute{{stepName}}().ConfigureAwait(false);
                    Assert.IsTrue(state.WasHandled(out var result), "WasHandled");
                    Assert.AreEqual({{expectedOutput}}, result, "Result");
                };
                """;
        }
    }
}