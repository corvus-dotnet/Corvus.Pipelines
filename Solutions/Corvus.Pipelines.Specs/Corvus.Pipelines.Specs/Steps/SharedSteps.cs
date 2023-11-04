// <copyright file="SharedSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
internal class SharedSteps(ScenarioContext scenarioContext)
{
    public const string Output = "Output";

    private readonly ScenarioContext scenarioContext = scenarioContext;

    public static void SetOutput(ScenarioContext scenarioContext, int output)
    {
        scenarioContext.Set(output, Output);
    }

    public static Func<T, PipelineStep<T>> BuildSelector<T>(IEnumerable<(string Match, string Lambda)> selectorsAndSteps)
        where T : struct
    {
        // Format the guid as a valid C# identifier
        string assemblyName = GetAssemblyName();

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
            namespace {{assemblyName}}
            {
                public static class Generated
                {
                    public static readonly Func<{{typeof(T).Name}}, PipelineStep<{{typeof(T).Name}}>> Selector = input =>
                    {
                        return input switch
                        {
            """;

        // Build the match cases from the match and lambda
        string matchCases =
            string.Join(
                Environment.NewLine,
                selectorsAndSteps.Select(
                    s =>
                    $$"""
                                    {{s.Match}} => {{s.Lambda}},
                    """));

        const string codeSuffix =
            """   

                        };
                    };
                }
            }
            """;

        string code = codePrefix + matchCases + codeSuffix;

        Assembly assembly = BuildAndLoadAssembly(code, assemblyName);

        // Get the type from the assembly
        Type? type = assembly.GetType($"{assemblyName}.Generated");

        object? selectorInstanceObject = type?.GetField("Selector", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        return selectorInstanceObject is Func<T, PipelineStep<T>> stepInstance
            ? stepInstance
            : throw new InvalidOperationException("The selector could not be loaded.");
    }

    public static Func<T, SyncPipelineStep<T>> BuildSyncSelector<T>(IEnumerable<(string Match, string Lambda)> selectorsAndSteps)
    where T : struct
    {
        // Format the guid as a valid C# identifier
        string assemblyName = GetAssemblyName();

        string codePrefix =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
            namespace {{assemblyName}}
            {
                public static class Generated
                {
                    public static readonly Func<{{typeof(T).Name}}, SyncPipelineStep<{{typeof(T).Name}}>> Selector = input =>
                    {
                        return input switch
                        {
            """;

        // Build the match cases from the match and lambda
        string matchCases =
            string.Join(
                Environment.NewLine,
                selectorsAndSteps.Select(
                    s =>
                    $$"""
                                    {{s.Match}} => {{s.Lambda}},
                    """));

        const string codeSuffix =
            """   

                        };
                    };
                }
            }
            """;

        string code = codePrefix + matchCases + codeSuffix;

        Assembly assembly = BuildAndLoadAssembly(code, assemblyName);

        // Get the type from the assembly
        Type? type = assembly.GetType($"{assemblyName}.Generated");

        object? selectorInstanceObject = type?.GetField("Selector", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        return selectorInstanceObject is Func<T, SyncPipelineStep<T>> stepInstance
            ? stepInstance
            : throw new InvalidOperationException("The selector could not be loaded.");
    }

    /// <summary>
    /// Dynamically builds a step from a string lambda.
    /// </summary>
    /// <typeparam name="T">The type of the step.</typeparam>
    /// <param name="step">The lambda in the form state => result.</param>
    /// <returns>The pipeline step.</returns>
    /// <exception cref="InvalidOperationException">The lambda could not be compiled and loaded.</exception>
    public static PipelineStep<T> BuildStep<T>(string step)
        where T : struct
    {
        // Format the guid as a valid C# identifier
        string assemblyName = GetAssemblyName();

        string code =
            $$"""
            using System;
            using System.Threading.Tasks;
            
            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;
            
            namespace {{assemblyName}}
            {
                public static class Steps
                {
                    public static readonly PipelineStep<{{typeof(T).Name}}> Step = {{step}};
                }
            }
            """;

        Assembly assembly = BuildAndLoadAssembly(code, assemblyName) ?? throw new InvalidOperationException("The type could not be loaded.");

        // Get the type from the assembly
        Type? type = assembly.GetType($"{assemblyName}.Steps");

        object? stepInstanceObject = type?.GetField("Step", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        return stepInstanceObject is PipelineStep<T> stepInstance
            ? stepInstance
            : throw new InvalidOperationException("The step could not be loaded.");
    }

    /// <summary>
    /// Dynamically builds a step from a string lambda.
    /// </summary>
    /// <typeparam name="T">The type of the step.</typeparam>
    /// <param name="step">The lambda in the form state => result.</param>
    /// <returns>The pipeline step.</returns>
    /// <exception cref="InvalidOperationException">The lambda could not be compiled and loaded.</exception>
    public static SyncPipelineStep<T> BuildSyncStep<T>(string step)
        where T : struct
    {
        string assemblyName = GetAssemblyName();

        string code =
            $$"""
            using System;
            using System.Threading.Tasks;

            using Corvus.Pipelines;
            using Corvus.Pipelines.Handlers;

            namespace {{assemblyName}}
            {
                public static class Steps
                {
                    public static readonly SyncPipelineStep<{{typeof(T).Name}}> Step = {{step}};
                }
            }
            """;

        Assembly assembly = BuildAndLoadAssembly(code, assemblyName) ?? throw new InvalidOperationException("The type could not be loaded.");

        // Get the type from the assembly
        Type? type = assembly.GetType($"{assemblyName}.Steps");

        object? stepInstanceObject = type?.GetField("Step", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

        return stepInstanceObject is SyncPipelineStep<T> stepInstance
            ? stepInstance
            : throw new InvalidOperationException("The step could not be loaded.");
    }

    [Then("the output should be (.*)")]
    public void ThenTheOutputShouldBe(int expectedOutput)
    {
        Assert.AreEqual(expectedOutput, this.scenarioContext.Get<int>(Output));
    }

    private static string GetAssemblyName()
    {
        return $"PipelineStepAssembly_{Guid.NewGuid().ToString().Replace("-", "_")}";
    }

    private static Assembly BuildAndLoadAssembly(string code, string assemblyName)
    {
        // Get the syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
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

    private static IEnumerable<MetadataReference> BuildMetadataReferences()
    {
        return from l in DependencyContext.Default.CompileLibraries
               from r in l.ResolveReferencePaths()
               select MetadataReference.CreateFromFile(r);
    }
}