#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "usage: $0 <package.nupkg>" >&2
  exit 2
fi

package_path="$(realpath "$1")"
if [[ ! -f "$package_path" ]]; then
  echo "package not found: $package_path" >&2
  exit 2
fi

package_dir="$(dirname "$package_path")"
entries="$(unzip -Z1 "$package_path")"
nuspec_entry="$(grep -E '\.nuspec$' <<<"$entries" | head -n 1)"
package_version="$(unzip -p "$package_path" "$nuspec_entry" \
  | sed -n 's:.*<version>\([^<]*\)</version>.*:\1:p' \
  | head -n 1)"

if [[ -z "$package_version" ]]; then
  echo "unable to read package version from $nuspec_entry" >&2
  exit 1
fi

required_entries=(
  "lib/net9.0/Sufficit.Blazor.UI.dll"
  "lib/net10.0/Sufficit.Blazor.UI.dll"
  "readme.md"
  "icon.png"
  "staticwebassets/sufficit-ui.css"
  "staticwebassets/styles/sui-foundations.css"
  "staticwebassets/styles/sui-portals.css"
  "staticwebassets/Components/Forms/SUISelect.razor.js"
  "staticwebassets/Components/Navigation/SUINavGroup.razor.js"
  "staticwebassets/Components/Navigation/SUITabs.razor.js"
  "staticwebassets/Components/Overlays/SUIDialogHost.razor.js"
  "staticwebassets/Components/Overlays/SUITooltip.razor.js"
)

for entry in "${required_entries[@]}"; do
  if ! grep -Fxq "$entry" <<<"$entries"; then
    echo "missing package entry: $entry" >&2
    exit 1
  fi
done

if ! grep -Eq '^staticwebassets/Sufficit\.Blazor\.UI\.[^.]+\.bundle\.scp\.css$' <<<"$entries"; then
  echo "missing package entry: fingerprinted CSS-isolation bundle" >&2
  exit 1
fi

validation_root="$(mktemp -d)"
trap 'rm -rf -- "$validation_root"' EXIT
dotnet new nugetconfig --output "$validation_root" >/dev/null
dotnet nuget add source "$package_dir" \
  --name sui-local \
  --configfile "$validation_root/nuget.config" >/dev/null

for framework in net9.0 net10.0; do
  consumer_dir="$validation_root/$framework"
  if [[ "$framework" == "net9.0" ]]; then
    aspnet_version="9.0.19"
  else
    aspnet_version="10.0.11"
  fi

  dotnet new razorclasslib --framework "$framework" --output "$consumer_dir" --no-restore >/dev/null
  # The installed SDK template can lag behind the servicing floor required by
  # this package. Updating its direct reference prevents a deliberate NuGet
  # downgrade error and models the supported consumer baseline.
  dotnet add "$consumer_dir" package Microsoft.AspNetCore.Components.Web \
    --version "$aspnet_version" \
    --no-restore >/dev/null
  dotnet add "$consumer_dir" package Sufficit.Blazor.UI \
    --version "$package_version" \
    --no-restore >/dev/null
  dotnet restore "$consumer_dir" \
    --configfile "$validation_root/nuget.config" \
    --verbosity minimal
  dotnet build "$consumer_dir" \
    --no-restore \
    --configuration Release \
    --verbosity minimal \
    -warnaserror
  echo "validated package consumer: $framework"
done

echo "validated package contents: $(basename "$package_path")"
