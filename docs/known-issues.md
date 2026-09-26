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

# Open

Nothing is open in Rapp.

Two caveats belong here rather than in the table, because neither is a defect and both bound what
the entries above are worth:

* Every result recorded here was produced on a single Windows ARM64 machine. CI has never executed
  on a GitHub-hosted runner, so nothing above is confirmed on x64 or on Linux.
* The `net11.0` preview leg is opt-in via `IncludePreviewTargetFramework` and has not been
  exercised recently, because the preview SDK is not installed on the machine used for this work.

A record of five fixed defects measures how hard this repository was looked at. It is not a claim
that there is nothing left to find.

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
