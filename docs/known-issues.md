# Known issues

Rapp's defects were found during a cross-project review that prepared this repository and four
others for submission, and most of them were found by building and packing the repository from a
clean clone rather than by its own test suite. That is the pattern worth noting: the tests passed
throughout. Entries are kept after they are resolved so that anyone evaluating a specific released
version can see what applied to it.

| # | Issue | Severity | Status |
|---|---|---|---|
| 1 | The solution could not be restored from a clean clone | High | **Fixed** |
| 2 | A redundant Source Link reference pulled a vulnerable package | High | **Fixed** |
| 3 | Tests ran only on `net10.0` while the package shipped `net8.0` | High | **Fixed** |
| 4 | Benchmark and dashboard were published as NuGet packages | Moderate | **Fixed** |
| 5 | Two unused locals in the generator | Low | **Fixed** |
| 6 | `dotnet pack --no-build` failed on the solution | Moderate | **Fixed** |
| 7 | Test generators shipped to every consumer | Moderate | **Fixed** |
| 8 | Generator pipeline defeats incremental caching | Low | **Fixed** |
| 9 | Shipped library serialized every value twice, once to JSON by reflection | High | **Fixed** |
| 10 | Wall-clock assertions made the test suite fail under load | Moderate | **Fixed** |
| 11 | Samples' JSON size comparison no longer populated | Low | **Fixed** |
| 12 | The three sample projects were in no solution the build ever compiled | Moderate | **Fixed** |
| 13 | Six project files defined `RAPP_TELEMETRY`, where it does nothing | Low | **Fixed** |

---

# Fixed

## 1. The solution could not be restored from a clean clone

**Severity: high as an advisory, low as exposure. Fixed in 1.3.0.**

`dotnet restore Rapp.sln` failed outright:

```
error NU1903: Package 'Microsoft.OpenApi' 2.0.0 has a known high severity vulnerability
(GHSA-v5pm-xwqc-g5wc)
```

`Rapp.Playground` referenced `Microsoft.AspNetCore.OpenApi` 10.0.3, which pulls `Microsoft.OpenApi`
2.0.0 transitively. The shipped `Rapp` package was never affected — the dependency reached only a
sample project — but the practical consequence was worse than the exposure: nobody could build the
repository from a clean clone, and CI would have failed on its first run.

`CentralPackageTransitivePinningEnabled` was already on, so declaring the fixed version in
`Directory.Packages.props` was sufficient:

```xml
<PackageVersion Include="Microsoft.OpenApi" Version="2.12.2" />
```

This is the same advisory that was found in Sannr. Meeting it twice in one review is the argument
for a shared dependency policy across the repositories rather than five independent ones.

## 2. A redundant Source Link reference pulled a vulnerable package

**Severity: high. Fixed in 1.3.0.**

`Rapp.csproj` referenced `Microsoft.SourceLink.GitHub` explicitly. Source Link has been built into
the .NET SDK since .NET 8, so the reference added nothing — but it dragged in
`Microsoft.Build.Tasks.Git`, which carries GHSA-23fw-v26w-5fgq.

The reference is removed. Source Link still works, because the SDK provides it.

Worth stating because it generalises: this was a dependency that was *only* a liability. It
contributed no capability the SDK did not already supply, and it was carried because it had once
been necessary. A dependency that is no longer doing anything is not neutral.

## 3. Tests ran only on `net10.0` while the package shipped `net8.0`

**Severity: high. Fixed in 1.3.0.**

`Rapp.Tests.csproj` declared no target framework of its own, so it inherited
`<TargetFramework>net10.0</TargetFramework>` from `Directory.Build.props`. The library used the
plural `<TargetFrameworks>`, so `net8.0` was built, packed and published — and never executed.
Rapp shipped a target framework on which not one test had ever run.

The mechanism is an MSBuild trap worth knowing: **a singular `<TargetFramework>` silently defeats a
plural `<TargetFrameworks>`**, and it does so without a warning. MSBuild prefers `TargetFramework`
whenever it is non-empty, so a repository-wide default in `Directory.Build.props` quietly collapses
every project's multi-targeting.

The global property is removed. Test projects now use `$(ToolkitTestTargetFrameworks)` and
executables `$(ToolkitAppTargetFrameworks)`, both defined in `Directory.TargetFrameworks.props`, so
the shipped framework list and the tested framework list are derived from the same place and cannot
drift apart.

Rapp's test count went from 88 to 176 without a single new test being written. The other 88 had
always existed; nothing had ever run them.

The same trap hid a framework leg in Prova, which is why it is described here in full rather than
just recorded as fixed.

## 4. Benchmark and dashboard were published as NuGet packages

**Severity: moderate. Fixed in 1.3.0.**

`dotnet pack Rapp.sln` produced `Rapp.Benchmark.nupkg` and `Rapp.Dashboard.nupkg` alongside
`Rapp.nupkg`. Neither is a library, and neither was intended for consumers.

Rapp keeps every project under `src/`, so the path-based rule used in the sibling repositories does
not work here; the non-shipping projects are named explicitly in `Directory.Build.props` instead.

`dotnet pack Rapp.sln` now produces exactly `Rapp.1.3.0.nupkg`, and the contents of that package
were inspected directly rather than inferred from the build succeeding.

## 5. Two unused locals in the generator

**Severity: low. Fixed in 1.3.0.**

`RappGenerator` assigned `attrName` and `attrDisplay` and then compared against
`attr.AttributeClass?.Name` and `.ToDisplayString()` again in the conditions below, leaving both
locals unused. The comparisons now use the locals, which is what the code was evidently written to
do.

---

## 6. `dotnet pack --no-build` failed on the solution

**Severity: moderate. Fixed in 1.3.0.**

`src/Rapp/Rapp.csproj` packs its project references into its own package through a
`CopyProjectReferencesToPackage` target that depends on `ResolveReferences`. Under
`dotnet pack --no-build` that dependency still invoked `Build` on the referenced projects, which the
SDK forbids (`NETSDK1085`), so packing the solution after building it failed. It is exactly what
the CI "Pack" step does, so CI could not have produced a package. The project also set
`GeneratePackageOnBuild`, which packed on every build and hid the problem locally.

**Fix:** `BuildProjectReferences` is `false` when `NoBuild` is set, and `GeneratePackageOnBuild`
is removed (the publish workflow packs explicitly). `dotnet pack --no-build` now produces
exactly `Rapp.1.3.0.nupkg`. The package contents were inspected: the library for `net8.0` and `net10.0` plus the
generator under `analyzers/dotnet/cs`.

---

## 7. Test generators shipped to every consumer

**Severity: moderate. Fixed in 1.3.0.**

`Rapp.Gen` contained a second `[Generator]` class, `TestGenerator`, which added
`TestGenerated.g.cs` to every compilation that referenced the package. `RappGenerator` itself also
added a `TestGenerator.g.cs`. Neither had any function; both were packed into the shipped analyzer
and appeared in every consumer's compilation. It is the same defect Sannr shipped (its entries 6
and 11).

**Fix:** both are deleted. `GeneratorHygieneTests` asserts that `Rapp.Gen` registers exactly
`RappGenerator` and `RappGhostGenerator`, and that a compilation with no `[RappCache]` type gets no
generated source at all. Both tests failed before the fix.

---

## 9. The shipped library serialized every value twice, once to JSON by reflection

**Severity: high. Fixed in 1.3.0. Affects 1.1.0 and 1.2.0.**

`RappBaseSerializer` contains a size-comparison block under `#if RAPP_TELEMETRY` that, on every
`Serialize`, serializes the value a second time with MemoryPack and a third time to JSON with
`System.Text.Json` reflection; and on every `Deserialize`, serializes the result to JSON. The
documentation described this as opt-in: "the NuGet package you install has zero telemetry
overhead", enabled by defining the symbol "in your project".

Neither half was true. `Directory.Build.props` defined `RAPP_TELEMETRY` for every project in the
repository, including `Rapp` itself, from 1.1.0 onwards, so the shipped package always contained the
block. And because the symbol is evaluated when Rapp is compiled, defining it in a consuming project
could never have switched it on or off. A 1.3.0 package built before this fix contained references to
`JsonSerializer.SerializeToUtf8Bytes`.

The consequences, for a library whose purpose is fast binary caching:

* **Allocation.** `Serialize` into a reused buffer allocated 568 bytes per call; it now allocates 0.
  `Deserialize` allocated 736 bytes where the result graph is 272.
* **Latency and CPU.** Every cache write and every cache hit paid for a reflection-based JSON
  serialization that the caller never asked for and whose output was discarded.
* **Native AOT.** The JSON calls suppressed `IL2026` and `IL3050` inside a package marked
  `IsAotCompatible`. Under AOT, reflection serialization fails; the failure was swallowed by an empty
  `catch`, which is why nobody saw it — but the work up to the failure still ran.
* **The benchmarks.** `Rapp.Benchmark` inherited the same define, so published figures measured
  Rapp with this overhead included.

**Fix:** the repository-wide define is removed, so the library is compiled without the block. The
projects that want it (tests, dashboard, playground, samples) still define it for their own code.
`IRappMetricsCollector` and `RappMetricsCollector`, which had been public only under the symbol, are
now unconditional so that 1.3.0 does not remove types 1.2.0 shipped.

`PerformanceRegressionTests` now asserts allocation per operation: 0 bytes to serialize, and exactly
the result graph to deserialize. Built with the old define, all three tests fail with the numbers
above; without it, they pass on `net8.0`, `net10.0` and `net11.0`.

---

## 10. Wall-clock assertions made the test suite fail under load

**Severity: moderate. Fixed in 1.3.0. Affected the build only.**

Five tests asserted elapsed time: 1,000 serializations in under 50 ms, 100 parallel round-trips in
under 500 ms, and so on. Such thresholds measure the machine, not the code. The parallel test failed
at 515 ms when three target frameworks were tested at once on one machine, and hosted CI runners are
slower and noisier still. They also did not catch entry 9: the serialization and deserialization thresholds passed with the
reflection JSON overhead present.

**Fix:** allocation per operation replaces elapsed time where the test is about cost (entry 9);
the two tests about correctness under concurrency and through `HybridCache` keep their correctness
assertions and are renamed to say so. Timing belongs in `Rapp.Benchmark`.

---

## 8. The generator pipeline defeated incremental caching

**Severity: low. Fixed. Affected IDE responsiveness only; build output was always correct.**

`RappGenerator`'s syntax transform returned an `INamedTypeSymbol`, and `RappGhostGenerator`'s
returned a `ClassDeclarationSyntax`. Neither is equatable across compilations, so the incremental
pipeline treated every edit as a change: it re-ran code generation for every `[RappCache]` and
`[RappGhost]` type on every keystroke, and each cached step kept the previous compilation alive.
Roslyn's incremental-generator guidance is explicit that pipeline values must be equatable models,
not symbols or syntax nodes.

Both generators now use `ForAttributeWithMetadataName` and project into equatable value models
(`CacheTypeModel`, `GhostModel`) built from strings and an `EquatableArray<T>` wrapper —
`ImmutableArray<T>` compares by the identity of the underlying array, so it is not sufficient on
its own. A `netstandard2.0` generator also needs an `IsExternalInit` polyfill before it can declare
records at all.

`GeneratorIncrementalityTests` pins this by running each generator against two separately
constructed but identical compilations and asserting that every tracked output step reports
`Cached` or `Unchanged`. The suite includes a deliberately defective symbol-carrying generator as a
control, so the harness is known to be capable of failing rather than merely observed to pass.

## 11. The samples' JSON size comparison was no longer populated

**Severity: low. Fixed. Affected the samples only.**

The ASP.NET Core and gRPC samples displayed Rapp bytes against JSON-equivalent bytes. Those numbers
came from the block removed in entry 9, so the JSON figure read zero.

The measurement is now `Rapp.Dashboard`'s `RappSizeComparison`: opt-in, called from the sample's
own cache-miss path where the cost is visible and chosen, and annotated
`[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` because the JSON half of the comparison is
reflection-based, with a `JsonTypeInfo` overload for callers who need it to stay Native-AOT-safe.
It was deliberately not restored to the library.

`TelemetryOverheadTests` guards the boundary by listening to the `Rapp` meter across a hundred
round trips and asserting that the library emits no `rapp_bytes_total` or `json_bytes_equivalent`
measurement. Building the same test with `-p:DefineConstants=RAPP_TELEMETRY` fails it with 400
recorded measurements, which is the regression it exists to catch. The listener filters by the
calling thread, because a `MeterListener` is process-global and `RappMetricsTests` exercises the
public `RecordSerializationSize` API on a parallel xUnit thread — without that filter the test was
intermittently red for a reason that had nothing to do with the library.

## 12. The three sample projects were in no solution the build ever compiled

**Severity: moderate. Fixed.**

`Samples/AspNetCoreMinimalApi`, `Samples/GrpcService` and `Samples/ConsoleApp` were referenced only
by `Samples/Rapp.Samples.sln`. CI built `Rapp.sln`, so no pipeline had ever compiled them. They
were the repository's only demonstration of how the library is meant to be used, and they were
outside the audit.

Building them surfaced two `CA1873` warnings in `GrpcService/Program.cs` immediately — a logging
call whose arguments were evaluated before the log level was checked. That is a small defect; the
point is that nothing would have reported it. The samples are now projects in `Rapp.sln`, so
`TreatWarningsAsErrors` applies to them, and `CA1873` is fixed with source-generated
`[LoggerMessage]` partial methods.

This is the third instance in this review of the same theme: a project outside the build graph is
outside the audit.

## 13. Six project files defined `RAPP_TELEMETRY`, where it does nothing

**Severity: low. Fixed.**

The three samples, the test project, the playground and the dashboard each set
`<DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>`. A `#if` is evaluated when
the file containing it is compiled, and every `#if RAPP_TELEMETRY` block lives in `Rapp`'s own
sources. Defining the symbol downstream compiled nothing differently in any of the six projects.

That misunderstanding is what entry 9 was: the symbol appeared to be opt-in while the cost it
guarded was actually being paid unconditionally by everyone. The defines are removed. `Rapp.csproj`
carries a comment recording that the symbol must not be defined there when packing.

---

Two caveats belong here rather than in the table, because neither is a defect and both bound what
the entries above are worth:

* Every result recorded here was produced on a single Windows ARM64 machine, **except** where noted
  below: the suite now also runs on GitHub-hosted x64 Linux and Windows runners, and the CI, OSPO
  compliance and benchmark workflows are green there.
* The `net11.0` leg is opt-in via `IncludePreviewTargetFramework`. It has been exercised on the
  same machine with SDK `11.0.100-rc.1.26425.128` (restore, build and every test, `net8.0`,
  `net10.0` and `net11.0`), with no failures. A release candidate is not a release; the leg
  should be re-run against the GA SDK.

# Found by running CI on GitHub-hosted x64 runners for the first time

The caveat above — "CI has never executed on a GitHub-hosted runner" — was retired by opening a
pull request. Doing so immediately produced two failures that a Windows ARM64 machine cannot
produce, because both are properties of how the workflow invokes the CLI rather than of the code.

## 14. `-p:PublishAot=true` on the command line broke the analyzer projects (NETSDK1207)

`aot-validation.yml` passed `-p:PublishAot=true` to `dotnet publish`. Two things were wrong with
that at once.

It was redundant: `src/Rapp/Rapp.csproj` already declares `PublishAot`, so the flag restated a
setting the project owns.

It was also harmful. A `-p:` switch on the command line creates a **global property**, and MSBuild
propagates global properties into every `ProjectReference` it builds. `Rapp.Gen` targets
`netstandard2.0`, which cannot be AOT-compiled, so the build stopped with:

```
error NETSDK1207: Ahead-of-time compilation is not supported for the target framework.
```

The distinction matters beyond this repository: the same property set in a project file does *not*
flow across a `ProjectReference`, which is why this never reproduced locally.

**Fixed.** The flag is removed. AOT is configured where it belongs, in the project file, and the
workflow simply publishes.

## 15. The IL-warning list was split on its commas (MSB1006)

The same step passed:

```
-p:WarningsAsErrors=IL2026,IL2046,IL2062,...
```

The dotnet CLI splits `-p:` values on commas, so everything after the first code was parsed as a
separate switch and the run failed before compiling anything:

```
MSBUILD : error MSB1006: Property is not valid. Switch: IL2046
```

The step had therefore never enforced a single one of those warnings-as-errors. A bare `;` is no
better, because it is the property separator.

**Fixed.** The codes are joined with `%3B`, the escaped semicolon, which reaches MSBuild as one
property value. `dotnet publish` on a multi-targeted project also requires `--framework`
(NETSDK1129), so the publish step now names `net10.0` — on the publish step only, since adding it
to the solution-wide build would break the `netstandard2.0` generator.

## 16. The AOT gate measured the dependencies, and the README overstated the result

Once defects 14 and 15 were fixed, the AOT job ran to completion for the first time — and failed.
It published `Samples/ConsoleApp` with `TreatWarningsAsErrors=true` and `TrimmerSingleWarn=false`,
which asks ILC to report every trim and AOT warning in the whole program, from every assembly, and
to treat each as fatal. That gate cannot distinguish "Rapp is AOT-safe" from "everything Rapp
depends on is AOT-safe", and the second is not true.

Measured on the probe described below, `TrimmerSingleWarn=false`, .NET 10.0.12 ILC:

| Originating assembly | Warnings |
|---|---|
| MemoryPack 1.21.4 (reflective formatter provider) | 21 |
| Microsoft.Extensions.Caching.Hybrid 10.3.0 (`DefaultJsonSerializerFactory`) | 9 |
| **Rapp** | **0** |

So the claim in the README was directionally right and specifically wrong. Rapp's own code is
clean. But the README said Rapp was "100% compatible with Native AOT", that it "avoids
`System.Reflection` entirely … preventing AOT trim warnings", and that MemoryPack is "designed from
the ground up for AOT" — and a reader who set `PublishAot=true` and `TreatWarningsAsErrors=true`
would immediately have seen 30 warnings. The README also blamed the sample warnings solely on the
JSON comparison logic in the ASP.NET and gRPC demos, which was incomplete: `ConsoleApp` has no JSON
comparison and still fails a strict gate, because MemoryPack and HybridCache are enough on their
own.

Two smaller things fell out of the same investigation. `ILLinkTreatWarningsAsErrors=false` does not
work; ILC honours plain `TreatWarningsAsErrors`. And ILC's default single-warn mode collapses
inferred trim warnings into one `IL2104`/`IL3053` per assembly, but never collapses `IL2026`/`IL3050`,
because those come from explicit `RequiresUnreferencedCode`/`RequiresDynamicCode` annotations rather
than from inference — so "suppress the roll-ups" would have left HybridCache's three annotation
warnings behind anyway.

**Fixed, in the way that makes the claim checkable rather than the build green.**

The obvious repair is to add the offending codes to `NoWarn`. That was rejected: warning codes are
not owned by an assembly, so `NoWarn=IL3050` silences MemoryPack *and* Rapp, and the gate would then
pass by construction. There are no IL suppressions anywhere in this change.

Instead there is now `tools/Rapp.AotProbe`, a console app that consumes only the public package
surface — the `[RappCache]` attribute, the generated `UseRappFor…` registration, the generated
serializer and `HybridCache`. CI publishes it with `PublishAot=true`, leaves ILC warnings
non-fatal so that compilation completes, and then **fails if any warning names a Rapp type**:

```
grep -E 'IL[0-9]{4}: Rapp' ./ilc.log
```

That assertion says exactly what the README claims, and cannot be weakened by adding a code to a
list. It was verified in both directions: it passes on the real code, and when a `MakeGenericType`
call was temporarily added to `src/Rapp`, it caught all three resulting warnings (IL3050, IL2055,
IL2067) and failed.

Leaving ILC's warnings non-fatal also has a point beyond letting the gate read them: the native
binary now gets built, and CI runs it. It round-trips values, empty strings, empty collections,
default `DateTime`s and 4 KB payloads. Compiling proves only that ILC was willing to compile;
executing is what shows the reflective fallbacks are never reached, because under Native AOT a
reached fallback throws rather than degrading quietly.

The blocking analyzer job was also re-scoped from `Rapp.sln` to `src/Rapp/Rapp.csproj`, since the
benchmark, playground and dashboard projects are not shipped, and the samples now run as an
advisory, non-blocking job.

**Not fixed, because it is not ours to fix.** The 30 dependency warnings remain. They are now
documented in the README with their true cause and, more usefully, with the consequence that
matters to a consumer: a type cached through `HybridCache` **without** `[RappCache]` falls back to
reflection-based `System.Text.Json`, which is not AOT-safe. That caveat was previously undocumented
and is the one case where these warnings describe a real runtime risk.

A record of sixteen defects, all fixed, measures how hard this repository was looked at. It is not
a claim that there is nothing left to find.

---

## Supported frameworks

As of 1.3.0, Rapp multi-targets `net8.0` (LTS) and `net10.0` (current), rather than forcing
consumers onto the newest runtime. `net11.0` is built and tested in CI behind an opt-in switch, so
preview regressions surface during the preview window, but it is not shipped in the released
package until it is a supported release.

---

## Reporting

Security-relevant issues should follow [SECURITY.md](../SECURITY.md) rather than being filed as
public issues.
