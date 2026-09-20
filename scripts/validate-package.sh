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

# Reject accidental legacy SemVer/Debug packages before testing or publishing.
python3 "$(dirname "$0")/release_version.py" "$package_version" >/dev/null

required_entries=(
  "lib/net10.0/Sufficit.Blazor.UI.dll"
  "lib/net10.0/Sufficit.Blazor.UI.xml"
  "readme.md"
  "icon.png"
  "staticwebassets/sufficit-ui.css"
)

# Every collocated interop module must ship. This list used to be hand-written
# and named six of the twelve modules, so dropping one of the other six from the
# package would have passed validation and broken the component at runtime in
# the consumer. Derive it from the sources instead, so a new module is covered
# the day it is written.
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
while IFS= read -r module; do
  required_entries+=("staticwebassets/${module#"$repo_root/src/"}")
done < <(find "$repo_root/src" -name '*.razor.js' -type f | sort)

if (( ${#required_entries[@]} < 6 )); then
  echo "no interop modules discovered under src/; check the source layout" >&2
  exit 1
fi

for entry in "${required_entries[@]}"; do
  if ! grep -Fxq "$entry" <<<"$entries"; then
    echo "missing package entry: $entry" >&2
    exit 1
  fi
done

if grep -Eq '^lib/net9\.0/' <<<"$entries"; then
  echo "unexpected net9.0 asset in net10-only package" >&2
  exit 1
fi

if ! grep -Eq '^staticwebassets/Sufficit\.Blazor\.UI\.[^.]+\.bundle\.scp\.css$' <<<"$entries"; then
  echo "missing package entry: fingerprinted CSS-isolation bundle" >&2
  exit 1
fi

validation_root="$(mktemp -d)"
running_pids=()
cleanup() {
  for pid in "${running_pids[@]}"; do
    kill "$pid" 2>/dev/null || true
  done
  rm -rf -- "$validation_root"
}
trap cleanup EXIT
dotnet new nugetconfig --output "$validation_root" >/dev/null
dotnet nuget add source "$package_dir" \
  --name sui-local \
  --configfile "$validation_root/nuget.config" >/dev/null

# Read the servicing floor the package actually declares instead of repeating it
# here. The hardcoded copy meant that the next Dependabot bump of
# Microsoft.AspNetCore.Components turned this script red until someone edited it
# by hand, because the synthetic consumer asked for an older version than the
# package requires and NuGet refused the downgrade.
aspnet_version="$(unzip -p "$package_path" "$nuspec_entry" \
  | sed -n 's:.*<dependency id="Microsoft.AspNetCore.Components.Web" version="\([^"]*\)".*:\1:p' \
  | head -n 1)"
aspnet_version="${aspnet_version#[\[(]}"
aspnet_version="${aspnet_version%%[],)]*}"
aspnet_version="${aspnet_version%%,*}"

if [[ -z "$aspnet_version" ]]; then
  echo "unable to read the Microsoft.AspNetCore.Components.Web dependency version from $nuspec_entry" >&2
  exit 1
fi

for framework in net10.0; do
  consumer_dir="$validation_root/$framework"

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

framework_index=0
for framework in net10.0; do
  framework_index=$((framework_index + 1))
  framework_slug="${framework//./}"
  app_name="SuiPackageSmoke${framework_slug}"
  app_dir="$validation_root/app-$framework"
  app_project="$app_dir/$app_name.csproj"

  dotnet new blazor \
    --framework "$framework" \
    --output "$app_dir" \
    --name "$app_name" \
    --empty \
    --interactivity Server \
    --all-interactive \
    --no-restore \
    --no-https >/dev/null
  dotnet add "$app_project" package Sufficit.Blazor.UI \
    --version "$package_version" \
    --no-restore >/dev/null

  perl -0pi -e \
    's/using ([^;]+\.Components);/using $1;\nusing Sufficit.Blazor.UI;/' \
    "$app_dir/Program.cs"
  perl -0pi -e \
    's/(\.AddInteractiveServerComponents\(\);)/$1\n\nbuilder.Services.AddSufficitUI();/' \
    "$app_dir/Program.cs"
  perl -0pi -e \
    's/(var app = builder\.Build\(\);)/$1\n\nvar pathBase = Environment.GetEnvironmentVariable("SUI_TEST_PATHBASE");\nif (!string.IsNullOrWhiteSpace(pathBase))\n{\n    app.UsePathBase(pathBase);\n}/' \
    "$app_dir/Program.cs"
  perl -0pi -e \
    's#<base href="/" />#<base href="\@BaseHref" />#; s#(<link rel="stylesheet" href="\@Assets\["app\.css"\]" />)#$1\n    <link rel="stylesheet" href="_content/Sufficit.Blazor.UI/sufficit-ui.css" />#; s#</html>#</html>\n\n\@code {\n    private static string BaseHref {\n        get {\n            var pathBase = Environment.GetEnvironmentVariable("SUI_TEST_PATHBASE");\n            return string.IsNullOrWhiteSpace(pathBase) ? "/" : \$"{pathBase.TrimEnd(\x27/\x27)}/";\n        }\n    }\n}#' \
    "$app_dir/Components/App.razor"
  perl -0pi -e \
    's#\A.*\z#\@page "/"\n\@using Sufficit.Blazor.UI.Components\n\@using Sufficit.Blazor.UI.Themes\n\n<PageTitle>SUI package smoke</PageTitle>\n\n<SUIThemeProvider>\n    <main data-sui-package-smoke>\n        <h1>SUI package smoke</h1>\n        <SUIFormGrid Columns="2">\n            <SUITextField T="string" Label="Nome" />\n            <SUISelect T="string" Label="Região" />\n        </SUIFormGrid>\n        <SUIButton>Pacote SUI operacional</SUIButton>\n    </main>\n</SUIThemeProvider>\n#s' \
    "$app_dir/Components/Pages/Home.razor"

  dotnet restore "$app_project" \
    --configfile "$validation_root/nuget.config" \
    --verbosity minimal
  dotnet build "$app_project" \
    --no-restore \
    --configuration Release \
    --verbosity minimal \
    -warnaserror

  for path_base in "" "/app"; do
    if [[ -z "$path_base" ]]; then
      mode="root"
      app_port=$((5090 + framework_index))
    else
      mode="pathbase"
      app_port=$((5100 + framework_index))
    fi
    app_origin="http://127.0.0.1:$app_port"
    app_base="$app_origin$path_base/"
    app_log="$validation_root/$app_name-$mode.log"

    SUI_TEST_PATHBASE="$path_base" dotnet run \
      --project "$app_project" \
      --configuration Release \
      --no-build \
      --urls "$app_origin" >"$app_log" 2>&1 &
    app_pid=$!
    running_pids+=("$app_pid")

    for attempt in $(seq 1 30); do
      if curl --fail --silent "$app_base" >"$validation_root/$app_name-$mode.html"; then
        break
      fi
      if [[ "$attempt" -eq 30 ]]; then
        tail -80 "$app_log" >&2
        exit 1
      fi
      sleep 1
    done

    app_html="$validation_root/$app_name-$mode.html"
    grep -Fq 'data-sui-package-smoke' "$app_html"
    grep -Fq 'Pacote SUI operacional' "$app_html"
    grep -Fq 'sui-form-grid' "$app_html"

    global_css="$validation_root/$app_name-$mode.sufficit-ui.css"
    curl --fail --silent \
      "${app_base}_content/Sufficit.Blazor.UI/sufficit-ui.css" \
      >"$global_css"
    grep -Fq -- '--sui-color-primary' "$global_css"

    select_module="$validation_root/$app_name-$mode.SUISelect.razor.js"
    curl --fail --silent \
      "${app_base}_content/Sufficit.Blazor.UI/Components/Forms/SUISelect.razor.js" \
      >"$select_module"
    grep -Fq 'export function' "$select_module"

    date_module="$validation_root/$app_name-$mode.SUIDateField.razor.js"
    curl --fail --silent \
      "${app_base}_content/Sufficit.Blazor.UI/Components/Forms/SUIDateField.razor.js" \
      >"$date_module"
    grep -Fq 'export function' "$date_module"

    styles_href="$(grep -oE 'href="[^"]+\.styles[^" ]*\.css"' "$app_html" \
      | head -n 1 \
      | cut -d '"' -f 2)"
    if [[ -z "$styles_href" ]]; then
      echo "host CSS-isolation link not found in $framework $mode app" >&2
      exit 1
    fi
    if [[ "$styles_href" == /* ]]; then
      styles_url="$app_origin$styles_href"
    else
      styles_url="$app_base$styles_href"
    fi
    host_css="$validation_root/$app_name-$mode.styles.css"
    curl --fail --silent "$styles_url" >"$host_css"
    isolated_href="$(sed -n \
      "s#^@import ['\"]\([^'\"]*Sufficit\.Blazor\.UI\.[^'\"]*\.bundle\.scp\.css\)['\"].*#\1#p" \
      "$host_css" \
      | head -n 1)"
    if [[ -z "$isolated_href" ]]; then
      echo "SUI CSS-isolation import not found in $framework $mode host stylesheet" >&2
      exit 1
    fi
    isolated_css="$validation_root/$app_name-$mode.sui-isolated.css"
    curl --fail --silent "$app_base$isolated_href" >"$isolated_css"
    grep -Fq '.sui-form-grid' "$isolated_css"

    kill "$app_pid" 2>/dev/null || true
    wait "$app_pid" 2>/dev/null || true
    echo "validated executable package app: $framework ($mode)"
  done
done

echo "validated package contents: $(basename "$package_path")"
