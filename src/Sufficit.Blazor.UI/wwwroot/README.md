# Web assets (vendored)

Compiled CSS/JS taken from the MudBlazor NuGet package, version
`9.8.0`, rather than built from SCSS and
TypeScript here — importing that toolchain would add node and sass to a
project that otherwise needs only the .NET SDK.

MIT, like the rest of the vendored source. See
`../Vendor/LICENSE-MudBlazor.txt`.

Regenerate with the "Vendor web assets" workflow, giving the version to
sync to. Do not edit these files by hand: the next sync overwrites them.
