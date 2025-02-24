// <copyright file="ScenarioCodeGenerationBindings.cs" company="Endjin Limited">
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

namespace Corvus.Pipelines.Specs.Steps;

/// <summary>
/// Provides code generation services for scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This presumes the test will generate two kinds of code:
/// </para>
/// <list type="number">
/// <item>An example of the kind of code people will write when using a library feature</item>
/// <item>Code that verifies that this code behaves as expected</item>
/// </list>
/// </remarks>
public class ScenarioCodeGenerationBindings
{
    private MetadataReference? metadataReferenceToTestCode;

    private IList<SyntaxTree> syntaxTrees = new List<SyntaxTree>();

    /// <summary>
    /// Gets the syntax trees that represent the test code. Populate this with the code that
    /// needs to be accessible to code supplied to <see cref="BuildAndLoadAssembly(IEnumerable{SyntaxTree})"/>.
    /// </summary>
    public IList<SyntaxTree> TestCodeSyntaxTrees => this.IsTestCodeBuilt
        ? throw new InvalidOperationException("Test code already built. You can't add more syntax.")
        : this.syntaxTrees;

    public string Namespace { get; } = $"GeneratedTest_{Guid.NewGuid().ToString().Replace('-', '_')}";

    private bool IsTestCodeBuilt => this.metadataReferenceToTestCode is not null;

    /// <summary>
    /// Compiles code references the code supplied in <see cref="TestCodeSyntaxTrees"/>.
    /// </summary>
    /// <param name="syntaxTrees">The code to compile.</param>
    /// <returns>An assembly containing the compiled code.</returns>
    public Assembly BuildAndLoadAssembly(IEnumerable<SyntaxTree> syntaxTrees)
    {
        this.EnsureTestCodeBuilt();
        if (this.metadataReferenceToTestCode is null)
        {
            throw new InvalidOperationException("EnsureTestCodeBuilt failed to make a metadata reference available");
        }

        (Assembly assembly, _) = this.BuildAndLoadAssembly(syntaxTrees, this.metadataReferenceToTestCode);

        return assembly;
    }

    private static IEnumerable<MetadataReference> BuildMetadataReferences()
    {
        return from l in DependencyContext.Default!.CompileLibraries
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

    private void EnsureTestCodeBuilt()
    {
        if (this.IsTestCodeBuilt)
        {
            return;
        }

        (_, this.metadataReferenceToTestCode) = this.BuildAndLoadAssembly(this.TestCodeSyntaxTrees, null);
    }

    private (Assembly Assembly, MetadataReference? MetadataReference) BuildAndLoadAssembly(
        IEnumerable<SyntaxTree> syntaxTrees, MetadataReference? metadataReference)
    {
        string assemblyName = "GeneratedTestAssembly_" + Guid.NewGuid().ToString().Replace('-', '_');

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            BuildMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (metadataReference is not null)
        {
            compilation = compilation.AddReferences(metadataReference);
        }

        var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);
        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        if (!result.Success)
        {
            throw new InvalidOperationException(BuildError(result.Diagnostics));
        }

        Assembly outputAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);
        ms.Seek(0, SeekOrigin.Begin);
        PortableExecutableReference outputMetadataReference = MetadataReference.CreateFromStream(ms);

        return (outputAssembly, outputMetadataReference);
    }
}