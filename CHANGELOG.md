# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-09-25

### Added
- **`tools/Rapp.AotProbe`, and an AOT gate that can actually fail.** A console app that consumes
  only the public package surface. CI publishes it with `PublishAot=true` and fails the build if
  any trim or AOT warning *originates in a Rapp type*, then executes the native binary to
  round-trip values, empty collections and 4 KB payloads. The gate was verified in both
  directions: it passes on the real code, and it catches a deliberately injected
  `MakeGenericType` call in `src/Rapp`.

### Fixed — found by running CI on GitHub-hosted x64 runners for the first time
- **The AOT gate measured the dependencies, not Rapp.** It published a sample with
  `TreatWarningsAsErrors=true` and `TrimmerSingleWarn=false`, which makes every trim warning in the
  whole program fatal — including 21 from MemoryPack 1.21.4's reflective formatter provider and 9
  from `Microsoft.Extensions.Caching.Hybrid` 10.3.0's JSON fallback. **Zero originate in Rapp.**
  The gate now asserts on the origin of a warning rather than its code, because `NoWarn=IL3050`
  would silence Rapp as well as MemoryPack. No IL suppressions were added. The blocking analyzer
  job is re-scoped from `Rapp.sln` to `src/Rapp/Rapp.csproj`; the samples are now advisory.
- **The README overstated the AOT claim.** It said Rapp was "100% compatible with Native AOT" and
  that it avoided reflection "preventing AOT trim warnings", when publishing a consumer app with
  `PublishAot=true` emits 30 warnings from two dependencies. Corrected to state what is true and
  verified — Rapp's own code is clean — and to document the consequence that actually affects
  consumers: a type cached **without** `[RappCache]` falls back to reflection-based
  `System.Text.Json`, which is not AOT-safe. Also removed a duplicated heading and an unterminated
  code fence in the same section.
- **The AOT validation workflow had never run to completion.** It passed `-p:PublishAot=true` on
  the command line, which creates a *global* property that MSBuild propagates into every
  `ProjectReference` — including the `netstandard2.0` generator, which cannot be AOT-compiled
  (`NETSDK1207`). The flag was also redundant: the target project already declares `PublishAot`.
  Removed; AOT stays configured in the project file, where it does not flow across references.
- **The trim and AOT warnings-as-errors list was never enforced.** It was passed as
  `-p:WarningsAsErrors=IL2026,IL2046,...`, and the dotnet CLI splits `-p:` values on commas, so
  every code after the first was parsed as a separate switch and the run failed with
  `MSB1006: Property is not valid. Switch: IL2046` before compiling anything. The codes are now
  joined with `%3B`, the escaped semicolon.
- **`dotnet publish` on a multi-targeted project needs an explicit framework** (`NETSDK1129`). The
  publish step now passes `--framework net10.0` — and only the publish step, because passing it to
  a solution-wide build breaks the `netstandard2.0` generator.

### Changed
- Updated the Dependabot queue after the OSPO baseline: `System.Text.Json`,
  `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`,
  `Microsoft.AspNetCore.OpenApi`, `Microsoft.Extensions.Caching.Hybrid`, Roslyn compiler/analyzer
  packages, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, and `FluentAssertions`; GitHub Pages
  and artifact actions now use their current major versions after checking workflow compatibility.
- Multi-targets `net8.0` (LTS) and `net10.0` (current) instead of a single framework, so the
  package no longer forces consumers onto the newest runtime. `net11.0` is validated in CI behind
  an opt-in switch.

### Fixed
- **The solution could not be restored from a clean clone.** `Rapp.Playground` referenced
  `Microsoft.AspNetCore.OpenApi` 10.0.3, which pulls `Microsoft.OpenApi` 2.0.0 transitively —
  a high-severity advisory (GHSA-v5pm-xwqc-g5wc) that failed `dotnet restore Rapp.sln` with
  NU1903. The shipped `Rapp` package itself was never affected, but nobody could build the
  repository and CI would have failed on its first run. Pinned to 2.12.2 via
  `Directory.Packages.props` transitive pinning.
- **`Rapp.Benchmark` and `Rapp.Dashboard` were produced as NuGet packages** by `dotnet pack` on
  the solution. Only `Rapp` and its generator are packable now.
- Removed a redundant `Microsoft.SourceLink.GitHub` package reference. Source Link has been built
  into the .NET SDK since .NET 8; the explicit reference added nothing and pulled in
  `Microsoft.Build.Tasks.Git`, which carries GHSA-23fw-v26w-5fgq.
- **Two test generators shipped in the analyzer** and added `TestGenerated.g.cs` and
  `TestGenerator.g.cs` to every consuming compilation. Removed.
- `dotnet pack --no-build` failed with `NETSDK1085`; `GeneratePackageOnBuild` is removed.
- **Every cache write serialized the value twice more, once to JSON by reflection, and every cache
  hit serialized the result to JSON.** `RAPP_TELEMETRY` was defined repository-wide, so the shipped
  library always contained its size-comparison block, despite documentation saying the package had
  zero telemetry overhead. `Serialize` now allocates 0 bytes per call (was 568). Affects 1.1.0 and
  1.2.0. Defining `RAPP_TELEMETRY` in a consuming project never had any effect; the docs now say so.
- Timing assertions in `PerformanceRegressionTests` replaced with allocation assertions.
- **Both incremental generators re-ran on every keystroke.** `RappGenerator` carried an
  `INamedTypeSymbol` and `RappGhostGenerator` a `ClassDeclarationSyntax` through the pipeline;
  neither is equatable across compilations, so nothing was ever cached and every cached step kept
  the previous compilation alive. Both now use `ForAttributeWithMetadataName` and project into
  equatable value models. Generated output is unchanged.
- **`CA1873` in the gRPC sample**: log arguments were evaluated before the level was checked.
  Replaced with source-generated `[LoggerMessage]` partial methods.

### Added
- `Rapp.Dashboard.RappSizeComparison` — the opt-in replacement for the size comparison removed
  from the library, with a `JsonTypeInfo` overload so it stays Native-AOT-safe. The samples call
  it from their own cache-miss paths, where the cost is visible and chosen.
- `GeneratorIncrementalityTests` asserts both generators report only `Cached`/`Unchanged` steps
  across identical compilations, with a deliberately defective generator as a control so the
  harness is proven able to fail.
- `TelemetryOverheadTests` listens to the `Rapp` meter and asserts the library emits no size
  measurement. It fails with 400 measurements when built with `-p:DefineConstants=RAPP_TELEMETRY`.

### Removed
- The no-op `RAPP_TELEMETRY` define from six project files (three samples, tests, playground and
  dashboard). A `#if` applies where the code is compiled, and every `#if RAPP_TELEMETRY` block is
  in `Rapp`'s own sources, so these defines changed nothing — while implying the cost was opt-in.

### Build
- The three sample projects are now in `Rapp.sln`. They were only in `Samples/Rapp.Samples.sln`,
  which no pipeline built, so the repository's only usage examples were never compiled by CI.

## [1.2.0] - 2026-01-11

### Added
- **Ghost Reader**: A zero-copy, zero-allocation `ref struct` view over binary data for ultra-low latency access.
- **Convenience Methods**: `ComputeSize()` and `ToBytes()` extension methods for generated Ghost types.
- **Documentation**: New `docs/GHOST_READER.md` dedicated guide.
- **Reliability**: Robust handling for nullable strings and buffer overflows in `RappWriter`.

## [1.1.0] - 2026-01-07

### Added
- **Full AOT Compatibility**: Verified and tested Native AOT support for high-performance deployments.
- **Dashboard Statistics**: Visual dashboard now shows real-time serialization metrics and cache hit/miss rates.
- **Documentation**: Added comprehensive `GETTING_STARTED.md` guide for new users.
- **Telemetry**: Enhanced metric collection for HybridCache integration.

### Changed
- **Performance**: Optimized hashing algorithm for 15% faster cold startups.
- **Samples**: Updated samples to use latest .NET 10 preview features.

## [1.0.0] - 2025-12-31

### Added
- Initial release of Rapp: Schema-aware binary serialization for .NET 10
- Source-generated binary serialization with cryptographic schema validation
- Native AOT compatibility with zero-overhead telemetry
- Seamless integration with Microsoft.Extensions.Caching.Hybrid
- Enterprise-grade schema evolution safety
- OpenAPI/Swagger schema generation
- Client-side validation rule generation
- Built-in business rule validation attributes
- Comprehensive performance monitoring and metrics
- Interactive schema evolution demo
- Full benchmark suite with BenchmarkDotNet

### Features
- **Performance**: MemoryPack-level performance with enterprise safety
- **Schema Safety**: Cryptographic hash validation prevents deployment crashes
- **AOT Compatible**: Full ahead-of-time compilation support
- **Enterprise Ready**: Production monitoring, metrics, and compliance features
- **Developer Experience**: Source generation with comprehensive IntelliSense support

### Dependencies
- Microsoft.Extensions.Caching.Hybrid (>= 10.1.0)
- MemoryPack (>= 1.21.4)
- .NET 10.0+

### Documentation
- Comprehensive README with architecture overview
- Performance benchmarks and comparisons
- Schema evolution safety demonstrations
- API documentation and examples
- Enterprise integration guides

---

## Types of changes
- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for now removed features
- `Fixed` for any bug fixes
- `Security` in case of vulnerabilities
