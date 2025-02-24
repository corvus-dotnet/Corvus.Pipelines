// <copyright file="PipelineSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable IDE0028 // Simplify collection initialization

using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

using Reqnroll;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class PipelineSteps(ScenarioCodeGenerationBindings syntaxBindings)
{
    [When("I execute the (.*) step \"(.*)\" with the input of type \"(.*)\" (.*)")]
    public void IExecuteTheStep(string syncOrAsync, string stepName, string type, string input)
    {
        this.IExecuteTheStepOrHandler(syncOrAsync, stepName, type, input, "Steps");
    }

    [When("I execute the (.*) handler \"(.*)\" with the input of type \"(.*)\" (.*)")]
    public void IExecuteTheHandler(string syncOrAsync, string stepName, string type, string input)
    {
        this.IExecuteTheStepOrHandler(syncOrAsync, stepName, type, input, "Handlers");
    }

    [Then("the timer (.*) should show (.*) within (.*)")]
    public void TheTimerShouldShow(string timerServiceName, string timeSpan, string deltaTimeSpan)
    {
        string namespaceName = syntaxBindings.Namespace;

        string code =
        $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            using NUnit.Framework;            
            
            namespace {{namespaceName}}
            {
                public static partial class TimerExpectations
                {
                    public static Action Assert = () => {{timerServiceName}}.AssertInRange({{timeSpan}}, {{deltaTimeSpan}});
                }
            }
            """;

        List<SyntaxTree> syntaxTrees =
            [
                CSharpSyntaxTree.ParseText(code, path: "TimerExpectations.cs"),
            ];

        Assembly assembly = syntaxBindings.BuildAndLoadAssembly(syntaxTrees);

        Type? type = assembly.GetType($"{namespaceName}.TimerExpectations");

        object? expectationObject = type?.GetField("Assert", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        if (expectationObject is Action expectation)
        {
            expectation();
        }
        else
        {
            throw new InvalidOperationException("The assertion could not be loaded.");
        }
    }

    [Then("the log (.*) should contain the following entries")]
    public void TheLogShouldContainTheFollowingEntries(string logServiceName, Table entries)
    {
        this.TheLogShouldContainTheFollowingEntriesAtOrAboveLevel(logServiceName, LogLevel.Trace, entries);
    }

    [Then("the log (.*) should contain the following entries at level (.*) or above")]
    public void TheLogShouldContainTheFollowingEntriesAtOrAboveLevel(string logServiceName, LogLevel minimumLogLevel, Table entries)
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
                public static partial class LogExpectations
                {
                    public static Action Assert = () => {{logServiceName}}.Validate(LogLevel.{{minimumLogLevel}}, {{GetMessages(entries)}});
                }
            }
            """;

        List<SyntaxTree> syntaxTrees =
            [
                CSharpSyntaxTree.ParseText(code, path: "LogExpectations.cs"),
            ];

        Assembly assembly = syntaxBindings.BuildAndLoadAssembly(syntaxTrees);

        Type? type = assembly.GetType($"{syntaxBindings.Namespace}.LogExpectations");

        object? expectationObject = type?.GetField("Assert", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        if (expectationObject is Action expectation)
        {
            expectation();
        }
        else
        {
            throw new InvalidOperationException("The assertion could not be loaded.");
        }

        static string GetMessages(Table table)
        {
            return string.Join(
                ", ",
                table.Rows.Select(
                    s => $"(LogLevel.{s["Log level"]}, \"{Escape(s["Message"])}\", \"{Escape(s["Scope"])}\")"));
        }

        static string Escape(string v)
        {
            return v.Replace("\"", "\\\"");
        }
    }

    [Then("the (.*) output of \"(.*)\" should be \"([^\"]*)\"")]
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
                return $"public static Action {stepName}OutputShouldBe = () => Assert.AreEqual({expectedOutput}, Executions.Execute{stepName}());";
            }

            return $"public static Func<ValueTask> {stepName}OutputShouldBe = async () => Assert.AreEqual({expectedOutput}, await Executions.Execute{stepName}().ConfigureAwait(false));";
        }
    }

    [Given("I create the service instances")]
    public void ICreateTheServiceInstances(Table table)
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
                public static partial class Services
                {

            """;

        string steps = string.Join(
            Environment.NewLine,
            table.Rows.Select(
                s =>
                    $$"""
                    public static readonly {{s["Service type"]}} {{s["Instance name"]}} = {{s["Factory method"]}};
                    """));

        const string codeSuffix =
            """

                }
            }
            """;

        string code = codePrefix + steps + codeSuffix;
        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Services.cs"));
    }

    [Given("I define the functions")]
    public void IDefineTheFunctions(Table table)
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
                public static partial class Functions
                {

            """;

        string steps = string.Join(
            Environment.NewLine,
            table.Rows.Select(
                s =>
                    $$"""
                    public static readonly {{s["Function type"]}} {{s["Function name"]}} = {{s["Function definition"]}};
                    """));

        const string codeSuffix =
            """

                }
            }
            """;

        string code = codePrefix + steps + codeSuffix;
        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Functions.cs"));
    }

    [Given("I produce the steps")]
    public void IProduceTheSteps(Table table)
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
                public static partial class Steps
                {

            """;

        string steps = string.Join(
            Environment.NewLine,
            table.Rows.Select(
                s =>
                    $$"""
                    public static readonly {{SyncAsyncPrefix(s)}}PipelineStep<{{s["State type"]}}> {{s["Step name"]}} = {{s["Step definition"]}};
                    """));

        const string codeSuffix =
            """

                }
            }
            """;

        string code = codePrefix + steps + codeSuffix;
        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Steps.cs"));

        static string SyncAsyncPrefix(DataTableRow row)
        {
            return row["Sync or async"] == "sync" ? "Sync" : string.Empty;
        }
    }

    [Given("I create (.*) match selector called \"(.*)\" for state of type \"(.*)\" with the following configuration")]
    public void ICreateAMatchSelectorCalledForStateOfType(string selectorType, string selectorName, string stateType, Table table)
    {
        string pipelineStepType = selectorType == "a sync" ? $"SyncPipelineStep<{stateType}>>" : $"PipelineStep<{stateType}>>";

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
                public static partial class Selectors
                {
                    public static readonly Func<{{stateType}}, {{pipelineStepType}} {{selectorName}} = input =>
                    {
                        return input switch
                        {
            
            """;

        // Build the match cases from the match and lambda
        string matchCases =
            string.Join(
                Environment.NewLine,
                table.Rows.Select(
                    s =>
                    $$"""
                                    {{s["Match"]}} => {{s["Step definition"]}},
                    """));

        const string codeSuffix =
            """   

                        };
                    };
                }
            }
            """;

        string code = codePrefix + matchCases + codeSuffix;
        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code));
    }

    private void IExecuteTheStepOrHandler(string syncOrAsync, string stepName, string type, string input, string stepsType)
    {
        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{syntaxBindings.Namespace}}
            {
                public static partial class Executions
                {
                    {{SyncOrAsync(syncOrAsync, stepName, type, input, stepsType)}}
                }
            }
            """;

        syntaxBindings.TestCodeSyntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: $"Executions.{stepName}.cs"));

        static string SyncOrAsync(string syncOrAsync, string stepName, string type, string input, string stepsType)
        {
            if (syncOrAsync == "sync")
            {
                return $"public static Func<{type}> Execute{stepName} = () => {stepsType}.{stepName}({input});";
            }

            return $"public static Func<ValueTask<{type}>> Execute{stepName} = () => {stepsType}.{stepName}({input});";
        }
    }
}