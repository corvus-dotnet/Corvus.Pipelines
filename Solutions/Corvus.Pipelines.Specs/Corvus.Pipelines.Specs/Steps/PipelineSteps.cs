// <copyright file="PipelineSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class PipelineSteps(ScenarioContext scenarioContext)
{
    private const string SyntaxTreesKey = "SyntaxTrees";
    private const string NamespaceKey = "Namespace";
    private const string AssemblyMetadataReferenceKey = "AssemblyMetadataReference";

    /// <summary>
    /// Gets the syntax trees from the scenario context, creating it if it does not exist.
    /// </summary>
    /// <param name="context">The context from which to get the syntax trees.</param>
    /// <returns>The list of syntax trees being built for the context.</returns>
    public static IList<SyntaxTree> GetSyntaxTrees(ScenarioContext context)
    {
        if (!context.TryGetValue(SyntaxTreesKey, out List<SyntaxTree> syntaxTrees))
        {
            syntaxTrees = new List<SyntaxTree>();
            context.Add(SyntaxTreesKey, syntaxTrees);
        }

        return syntaxTrees;
    }

    /// <summary>
    /// Gets the string to be used as the assembly name and namespace.
    /// </summary>
    /// <param name="context">The context from which to get the assembly name and namespace.</param>
    /// <returns>The string to use as the assembly name and namespace.</returns>
    public static string GetNamespace(ScenarioContext context)
    {
        if (!context.TryGetValue(NamespaceKey, out string namespaceName))
        {
            namespaceName = $"GeneratedTest_{Guid.NewGuid().ToString().Replace('-', '_')}";
            context.Add(NamespaceKey, namespaceName);
        }

        return namespaceName;
    }

    [When("I execute the (.*) step \"(.*)\" with the input of type \"(.*)\" (.*)")]
    public void IExecuteTheStep(string syncOrAsync, string stepName, string type, string input)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(scenarioContext);
        string namespaceName = GetNamespace(scenarioContext);

        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{namespaceName}}
            {
                public static partial class Executions
                {
                    {{SyncOrAsync(syncOrAsync, stepName, type, input)}}
                }
            }
            """;

        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: $"Executions.{stepName}.cs"));

        static string SyncOrAsync(string syncOrAsync, string stepName, string type, string input)
        {
            if (syncOrAsync == "sync")
            {
                return $"public static Func<{type}> Execute{stepName} = () => Steps.{stepName}({input});";
            }

            return $"public static Func<ValueTask<{type}>> Execute{stepName} = () => Steps.{stepName}({input});";
        }
    }

    [Then("the timer (.*) should show (.*) within (.*)")]
    public void TheTimerShouldShow(string timerServiceName, string timeSpan, string deltaTimeSpan)
    {
        this.BuildTestCode();
        string namespaceName = GetNamespace(scenarioContext);

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

        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');

        Assembly assembly = this.BuildAndLoadAssembly(syntaxTrees, assemblyName) ?? throw new InvalidOperationException("The assembly could not be built.");

        Type? type = assembly.GetType($"{namespaceName}.TimerExpectations");

        object? expectationObject = type?.GetField($"Assert", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

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
        this.BuildTestCode();
        string namespaceName = GetNamespace(scenarioContext);

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
                public static partial class LogExpectations
                {
                    public static Action Assert = () => {{logServiceName}}.Validate({{GetMessages(entries)}});
                }
            }
            """;

        List<SyntaxTree> syntaxTrees =
            [
                CSharpSyntaxTree.ParseText(code, path: "LogExpectations.cs"),
            ];

        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');

        Assembly assembly = this.BuildAndLoadAssembly(syntaxTrees, assemblyName) ?? throw new InvalidOperationException("The assembly could not be built.");

        Type? type = assembly.GetType($"{namespaceName}.LogExpectations");

        object? expectationObject = type?.GetField($"Assert", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

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
                    s => $"(LogLevel.{s["Log level"]}, \"{Escape(s["Message"])}\")"));
        }

        static string Escape(string v)
        {
            return v.Replace("\"", "\\\"").Replace("{", "\\{").Replace("}", "\\}");
        }
    }

    [Then("the (.*) output of \"(.*)\" should be (.*)")]
    public async Task TheOutputShouldBe(string syncOrAsync, string stepName, string output)
    {
        this.BuildTestCode();

        string namespaceName = GetNamespace(scenarioContext);

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
                public static partial class Expectations
                {
                    {{SyncOrAsync(syncOrAsync, stepName, output)}}
                }
            }
            """;

        List<SyntaxTree> syntaxTrees =
            [];

        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: $"Expectations.{stepName}.cs"));

        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');

        Assembly assembly = this.BuildAndLoadAssembly(syntaxTrees, assemblyName) ?? throw new InvalidOperationException("The assembly could not be built.");

        if (syncOrAsync == "sync")
        {
            Action assert = GetSyncAssertion(namespaceName, assembly, stepName);
            assert();
        }
        else
        {
            Func<ValueTask> assert = GetAsyncAssertion(namespaceName, assembly, stepName);
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
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(scenarioContext);
        string namespaceName = GetNamespace(scenarioContext);

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;

            using Microsoft.Extensions.Logging;
            
            namespace {{namespaceName}}
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
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Services.cs"));
    }

    [Given("I define the functions")]
    public void IDefineTheFunctions(Table table)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(scenarioContext);
        string namespaceName = GetNamespace(scenarioContext);

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{namespaceName}}
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
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Functions.cs"));
    }

    [Given("I produce the steps")]
    public void IProduceTheSteps(Table table)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(scenarioContext);
        string namespaceName = GetNamespace(scenarioContext);

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{namespaceName}}
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
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "Steps.cs"));

        static string SyncAsyncPrefix(TableRow row)
        {
            return row["Sync or async"] == "sync" ? "Sync" : string.Empty;
        }
    }

    [Given("I create (.*) match selector called \"(.*)\" for state of type \"(.*)\" with the following configuration")]
    public void ICreateAMatchSelectorCalledForStateOfType(string selectorType, string selectorName, string stateType, Table table)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(scenarioContext);
        string namespaceName = GetNamespace(scenarioContext);

        string pipelineStepType = selectorType == "a sync" ? $"SyncPipelineStep<{stateType}>>" : $"PipelineStep<{stateType}>>";

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using Corvus.Pipelines.Specs.Models;
            
            using Microsoft.Extensions.Logging;
            
            namespace {{namespaceName}}
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
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code));
    }

    private static IEnumerable<MetadataReference> BuildMetadataReferences()
    {
        return from l in DependencyContext.Default.CompileLibraries
               from r in l.ResolveReferencePaths()
               select MetadataReference.CreateFromFile(r);
    }

    private static string BuildError(ImmutableArray<Diagnostic> diagnostics)
    {
        StringBuilder builder = new();
        builder.AppendLine();

        foreach (Diagnostic diagnostic in diagnostics)
        {
            FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
            SourceText text = diagnostic.Location.SourceTree?.GetText() ?? throw new InvalidOperationException("No text available");

            for (int i = Math.Max(0, lineSpan.StartLinePosition.Line - 2); i <= lineSpan.StartLinePosition.Line; ++i)
            {
                builder.AppendLine(text.Lines[i].ToString());
            }

            // Append a number of spaces equal to the column number of the error
            int indentAmount = Math.Max(0, lineSpan.StartLinePosition.Character - 1);
            string indent = new('_', indentAmount);
            string indentSpace = new(' ', indentAmount);
            builder.Append(indent);
            builder.AppendLine("^");
            builder.Append(diagnostic.GetMessage());
            builder.Append(' ');
            builder.Append(lineSpan.StartLinePosition.Line);
            builder.Append(',');
            builder.Append(lineSpan.StartLinePosition.Character);
            builder.AppendLine();
            builder.AppendLine();
        }

        builder.AppendLine();

        return builder.ToString();
    }

    private void BuildTestCode()
    {
        if (scenarioContext.ContainsKey(AssemblyMetadataReferenceKey))
        {
            return;
        }

        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');
        _ = this.BuildAndLoadAssembly(GetSyntaxTrees(scenarioContext), assemblyName, true) ?? throw new InvalidOperationException("The assembly could not be built.");
    }

    private Assembly BuildAndLoadAssembly(IEnumerable<SyntaxTree> syntaxTrees, string assemblyName, bool isTestAssembly = false)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            BuildMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (!isTestAssembly && this.TryGetTestAssemblyMetadataReference(out MetadataReference testAssemblyMetadataReference))
        {
            compilation = compilation.AddReferences(testAssemblyMetadataReference);
        }

        var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);
        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        if (!result.Success)
        {
            throw new InvalidOperationException(BuildError(result.Diagnostics));
        }

        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

        ms.Seek(0, SeekOrigin.Begin);

        if (isTestAssembly)
        {
            scenarioContext.Set(MetadataReference.CreateFromStream(ms), AssemblyMetadataReferenceKey);
        }

        return assembly;
    }

    private bool TryGetTestAssemblyMetadataReference(out MetadataReference testAssemblyMetadataReference)
    {
        return scenarioContext.TryGetValue(AssemblyMetadataReferenceKey, out testAssemblyMetadataReference);
    }
}