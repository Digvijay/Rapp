using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rapp.Gen;
using Xunit;

namespace Rapp.Tests;

/// <summary>
/// An incremental generator that cannot reuse its previous results re-runs in full on every
/// keystroke in the IDE. These tests pin the two properties that make reuse possible.
/// </summary>
public class GeneratorIncrementalityTests
{
    private const string Source = """
        using Rapp;

        namespace App;

        [RappCache]
        public partial class Order
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        [RappGhost]
        public partial class Telemetry
        {
            public int Count { get; set; }
            public string Source { get; set; } = "";
        }

        public class Unrelated
        {
            public int Untouched { get; set; }
        }
        """;

    private static CSharpCompilation CreateCompilation(string source) =>
        CSharpCompilation.Create("Incremental",
            new[] { CSharpSyntaxTree.ParseText(source) },
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p))
                .Append(MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "Rapp.dll"))),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static CSharpGeneratorDriver CreateDriver() =>
        (CSharpGeneratorDriver)CSharpGeneratorDriver.Create(
            new ISourceGenerator[] { new RappGenerator().AsSourceGenerator(), new RappGhostGenerator().AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    /// <summary>
    /// Re-running against an equivalent compilation must reuse every cached step. This fails if any
    /// value in the pipeline compares by reference — a symbol, a syntax node, or an
    /// <c>ImmutableArray</c> — because a fresh compilation produces fresh instances.
    /// </summary>
    [Fact]
    public void Second_run_over_an_equivalent_compilation_is_fully_cached()
    {
        var driver = CreateDriver().RunGenerators(CreateCompilation(Source));

        // A new compilation with identical content, as if the user had typed and undone a character.
        var rerun = driver.RunGenerators(CreateCompilation(Source));

        var steps = rerun.GetRunResult().Results
            .SelectMany(r => r.TrackedSteps)
            .Where(s => s.Key is "SourceOutput" or "GetClassToGenerate" or "GetTarget")
            .SelectMany(s => s.Value)
            .SelectMany(s => s.Outputs)
            .ToArray();

        Assert.NotEmpty(steps);
        Assert.All(steps, output =>
            Assert.True(
                output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                $"A pipeline step re-ran with reason '{output.Reason}'. Every value carried between " +
                "steps must have structural equality; symbols and syntax nodes do not."));
    }

    /// <summary>
    /// Editing code unrelated to any attributed type must not re-run the output stage.
    /// </summary>
    [Fact]
    public void Editing_unrelated_code_does_not_re_run_the_output_stage()
    {
        var driver = CreateDriver().RunGenerators(CreateCompilation(Source));

        var edited = Source.Replace("public int Untouched { get; set; }", "public int Untouched { get; set; } public int Added { get; set; }");
        var rerun = driver.RunGenerators(CreateCompilation(edited));

        var outputs = rerun.GetRunResult().Results
            .SelectMany(r => r.TrackedSteps)
            .Where(s => s.Key == "SourceOutput")
            .SelectMany(s => s.Value)
            .SelectMany(s => s.Outputs)
            .ToArray();

        Assert.NotEmpty(outputs);
        Assert.All(outputs, output =>
            Assert.True(
                output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                $"Editing an unrelated class re-ran source output with reason '{output.Reason}'."));
    }

    /// <summary>
    /// The caching must not be achieved by ignoring real changes.
    /// </summary>
    [Fact]
    public void Changing_a_cached_type_regenerates_it()
    {
        var driver = CreateDriver().RunGenerators(CreateCompilation(Source));
        var before = driver.GetRunResult().GeneratedTrees.Single(t => t.FilePath.Contains("Order")).ToString();

        var edited = Source.Replace("public int Id { get; set; }", "public long Id { get; set; }");
        var after = driver.RunGenerators(CreateCompilation(edited)).GetRunResult()
            .GeneratedTrees.Single(t => t.FilePath.Contains("Order")).ToString();

        Assert.NotEqual(before, after);
    }

    /// <summary>
    /// The control. A test that cannot fail proves nothing, so this runs the same assertion against
    /// a generator written the way Rapp's were before this fix — carrying a Roslyn symbol through
    /// the pipeline — and requires that the harness catches it.
    /// </summary>
    [Fact]
    public void The_harness_detects_a_pipeline_that_carries_a_symbol()
    {
        var driver = (CSharpGeneratorDriver)CSharpGeneratorDriver.Create(
            new ISourceGenerator[] { new SymbolCarryingGenerator().AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = (CSharpGeneratorDriver)driver.RunGenerators(CreateCompilation(Source));
        var rerun = driver.RunGenerators(CreateCompilation(Source));

        var reasons = rerun.GetRunResult().Results
            .SelectMany(r => r.TrackedSteps)
            .Where(s => s.Key == "SourceOutput")
            .SelectMany(s => s.Value)
            .SelectMany(s => s.Outputs)
            .Select(o => o.Reason)
            .ToArray();

        Assert.NotEmpty(reasons);
        Assert.Contains(reasons, r => r is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged));
    }

    /// <summary>
    /// Deliberately defective: carries an <see cref="INamedTypeSymbol"/>, which compares by
    /// reference across compilations. Used only by the control test above.
    /// </summary>
    private sealed class SymbolCarryingGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Rapp.RappCacheAttribute",
                predicate: static (s, _) => true,
                transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol);

            context.RegisterSourceOutput(provider, static (spc, symbol) =>
                spc.AddSource($"{symbol.Name}.Control.g.cs", $"// {symbol.Name}"));
        }
    }
}
