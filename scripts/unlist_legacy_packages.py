#!/usr/bin/env python3
"""One-time, bounded NuGet cleanup. Default is inspection only."""

import argparse
import gzip
import json
import os
import urllib.error
import urllib.request

from release_version import normalize

PACKAGE = "Sufficit.Blazor.UI"
LEGACY_VERSIONS = ("1.27.0", "1.28.0", "2.0.0", "2.1.1", "2.2.1")


def get_json(url):
    with urllib.request.urlopen(url, timeout=30) as response:
        data = response.read()
        if response.headers.get("Content-Encoding") == "gzip":
            data = gzip.decompress(data)
        return json.loads(data)


def catalog(version):
    # The package index can be ready before the individual version leaf has
    # propagated. It contains the same authoritative catalog metadata.
    index = get_json(f"https://api.nuget.org/v3/registration5-gz-semver2/{PACKAGE.lower()}/index.json")
    for page in index["items"]:
        entries = page.get("items")
        if entries is None:
            entries = get_json(page["@id"])["items"]
        for entry in entries:
            metadata = entry["catalogEntry"]
            if metadata["version"] == version:
                return metadata
    raise SystemExit(f"{PACKAGE} {version} is not yet in the NuGet registration index")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--replacement", required=True)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    replacement = normalize(args.replacement)
    metadata = catalog(replacement)
    if metadata.get("id", "").lower() != PACKAGE.lower() or not metadata.get("listed", False):
        raise SystemExit("The replacement must already be listed on NuGet.org")
    key = os.environ.get("NUGET_API_KEY")
    if args.apply and not key:
        raise SystemExit("NUGET_API_KEY is required to unlist; no packages changed")
    for version in LEGACY_VERSIONS:
        listed = catalog(version).get("listed", False)
        print(f"{PACKAGE} {version}: listed={listed}", flush=True)
        if not listed or not args.apply:
            continue
        request = urllib.request.Request(
            f"https://www.nuget.org/api/v2/package/{PACKAGE}/{version}",
            headers={"X-NuGet-ApiKey": key}, method="DELETE")
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                print(f"Unlist accepted for {version}: HTTP {response.status}", flush=True)
        except urllib.error.HTTPError as error:
            # urllib's default message for a 403 is the same generic sentence
            # whether the credential lacks the unlist scope or its glob pattern
            # matches no package, and the two need opposite fixes. NuGet returns
            # the distinguishing detail in the response body, which urlopen
            # discards unless it is read here.
            body = error.read().decode("utf-8", "replace").strip()
            print(f"Unlist refused for {version}: HTTP {error.code}", flush=True)
            if body:
                print(f"NuGet said: {body}", flush=True)
            raise SystemExit(
                f"Stopped at {version}; earlier versions in the list were already "
                "unlisted and the rest were left untouched.")
    print("Verify listed=false after NuGet indexing completes. Exact restores remain available.")


if __name__ == "__main__":
    main()
