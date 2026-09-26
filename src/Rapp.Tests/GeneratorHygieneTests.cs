using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rapp.Gen;
using Xunit;

namespace Rapp.Tests;

/// <summary>
/// The generator runs inside every consumer's compilation and IDE session, so anything it emits
/// or registers beyond real output is a cost paid by every user.
/// </summary>
public class GeneratorHygieneTests
{
    [Fact]
    public void Analyzer_assembly_registers_only_the_real_generators()
    {
        var generators = typeof(RappGenerator).Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<GeneratorAttribute>() is not null)
            .Select(t => t.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { nameof(RappGenerator), nameof(RappGhostGenerator) }, generators);
    }

    [Fact]
    public void Generator_emits_nothing_for_a_compilation_without_cached_types()
    {
        var compilation = CSharpCompilation.Create("Empty",
            new[] { CSharpSyntaxTree.ParseText("namespace App; public class Plain { }") },
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new RappGenerator(), new RappGhostGenerator());
        var generated = driver.RunGenerators(compilation).GetRunResult().GeneratedTrees;

        Assert.Empty(generated);
    }
}
