# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-09-25

### Changed
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
