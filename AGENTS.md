# Agent Instructions — Sufficit.Blazor.UI

## Consumers that reference this repository as a local sibling

Some consumer repositories (currently `sufficit-identity`) switch between two
ways of consuming SUI, decided in their `Directory.Build.props` by the
`SufficitUseLocalSui` property:

- **Sibling mode** — when `../sufficit-blazor-ui/src/Sufficit.Blazor.UI.csproj`
  exists next to the consumer checkout, the consumer uses a `ProjectReference`
  to this repository (fast inner loop against unreleased component changes).
- **Package mode** — otherwise (CI, Docker, fresh clones) the consumer uses the
  published `Sufficit.Blazor.UI` NuGet package, floating on `2.*`.

The consumer's committed `packages.lock.json` files **must stay in package
mode**. CI restores with `--locked-mode` and has no sibling checkout. A restore
in sibling mode rewrites the lock entry to `"sufficit.blazor.ui": { "type":
"Project" }`. If that file is committed, CI fails with `NU1004` ("The package
references have changed … Lock file's package references: None, project's
package references: Sufficit.Blazor.UI >= 2.*").

After working against this repository as a sibling, regenerate the consumer's
locks in package mode before committing:

```bash
dotnet restore <Consumer>.sln --force-evaluate -p:SufficitUseLocalSui=false
dotnet restore <Consumer>.sln --locked-mode -p:SufficitUseLocalSui=false   # verify
```

Then check that each lock lists `Sufficit.Blazor.UI` as `"type": "Direct"`
with a `resolved` version and `contentHash`, and never as `"type": "Project"`.

Example incident: `sufficit-identity` commit `3c457ca` committed sibling-mode
locks, which broke CodeQL/CI restore. Commit `996f4ec` fixed it by regenerating
the locks in package mode.
