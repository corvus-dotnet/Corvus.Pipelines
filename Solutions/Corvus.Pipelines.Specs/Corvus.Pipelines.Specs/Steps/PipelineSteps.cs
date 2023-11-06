// <copyright file="PipelineSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class PipelineSteps
{
    private const string SyntaxTreesKey = "SyntaxTrees";
    private const string NamespaceKey = "AssemblyName";

    private readonly ScenarioContext scenarioContext;

    public PipelineSteps(ScenarioContext scenarioContext)
    {
        this.scenarioContext = scenarioContext;
    }

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
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(this.scenarioContext);
        string namespaceName = GetNamespace(this.scenarioContext);

        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
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

    [Then("the (.*) output of \"(.*)\" should be (.*)")]
    public async Task TheOutputShouldBe(string syncOrAsync, string stepName, string output)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(this.scenarioContext);
        string namespaceName = GetNamespace(this.scenarioContext);

        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            using NUnit.Framework;            
            
            namespace {{namespaceName}}
            {
                public static partial class Expectations
                {
                    {{SyncOrAsync(syncOrAsync, stepName, output)}}
                }
            }
            """;

        List<SyntaxTree> cloneTrees =
            [.. syntaxTrees];

        cloneTrees.Add(CSharpSyntaxTree.ParseText(code, path: $"Expectations.{stepName}.cs"));

        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');

        Assembly assembly = BuildAndLoadAssembly(cloneTrees, assemblyName) ?? throw new InvalidOperationException("The assembly could not be built.");

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

        static Assembly BuildAndLoadAssembly(IEnumerable<SyntaxTree> syntaxTrees, string assemblyName)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees,
                BuildMetadataReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);
            if (!result.Success)
            {
                throw new InvalidOperationException("Compilation failed.");
            }

            // Load the assembly into the current AppDomain
            return Assembly.Load(ms.ToArray());
        }

        static IEnumerable<MetadataReference> BuildMetadataReferences()
        {
            return from l in DependencyContext.Default.CompileLibraries
                   from r in l.ResolveReferencePaths()
                   select MetadataReference.CreateFromFile(r);
        }
    }

    [Given("I produce the steps")]
    public void IProduceTheSteps(Table table)
    {
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(this.scenarioContext);
        string namespaceName = GetNamespace(this.scenarioContext);

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
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
        IList<SyntaxTree> syntaxTrees = GetSyntaxTrees(this.scenarioContext);
        string namespaceName = GetNamespace(this.scenarioContext);

        string pipelineStepType = selectorType == "a sync" ? $"SyncPipelineStep<{stateType}>>" : $"PipelineStep<{stateType}>>";

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
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
}