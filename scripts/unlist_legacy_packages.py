#!/usr/bin/env python3
"""One-time, bounded NuGet cleanup. Default is inspection only."""

import argparse
import gzip
import json
import os
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
    url = f"https://api.nuget.org/v3/registration5-gz-semver2/{PACKAGE.lower()}/{version}.json"
    registration = get_json(url)
    return get_json(registration["catalogEntry"])


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
        with urllib.request.urlopen(request, timeout=60) as response:
            print(f"Unlist accepted for {version}: HTTP {response.status}", flush=True)
    print("Verify listed=false after NuGet indexing completes. Exact restores remain available.")


if __name__ == "__main__":
    main()
