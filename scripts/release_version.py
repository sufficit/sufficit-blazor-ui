#!/usr/bin/env python3
"""Generate/validate Sufficit's UTC 2.yy.MMdd.HHmm NuGet versions.

Major 2: the legacy SemVer releases (1.27.0, 1.28.0, 2.0.0, 2.1.1, 2.2.1)
cannot be deleted from NuGet.org and would outrank any 1.yy calendar version
in a floating range. 2.yy.MMdd.HHmm orders above all of them.
"""

import re
import sys
from datetime import datetime, timezone


# An optional SemVer pre-release label, so a release can be tried before it
# reaches everyone. Consumers pin floating ranges like "2.*", and NuGet excludes
# pre-release versions from those, so a labelled build is installable only by
# asking for it explicitly. Without this every tag was a GA release and there was
# no way to test a package as consumers would actually receive it.
_PRERELEASE = r"(?:-(?P<label>[a-z][0-9a-z]*)\.(?P<iteration>[0-9]{1,3}))?"


def normalize(value: str) -> str:
    match = re.fullmatch(r"v?2\.([0-9]{2})\.([0-9]{3,4})\.([0-9]{1,4})" + _PRERELEASE, value)
    if not match:
        raise ValueError(
            "expected Sufficit version 2.yy.MMdd.HHmm with an optional -label.N "
            "pre-release suffix (optional v tag prefix)")
    year, month_day, hour_minute = (int(group) for group in match.groups()[:3])
    # Validate the actual calendar, including leap years and the UTC clock.
    datetime(2000 + year, month_day // 100, month_day % 100,
             hour_minute // 100, hour_minute % 100, tzinfo=timezone.utc)
    label, iteration = match.group("label"), match.group("iteration")
    # NuGet removes leading zeroes in each numeric segment.
    suffix = f"-{label}.{int(iteration)}" if label else ""
    return f"2.{year}.{month_day}.{hour_minute}{suffix}"


def is_prerelease(value: str) -> bool:
    """Whether a normalized or raw version carries a pre-release label."""
    return "-" in normalize(value)


def generate(now: datetime | None = None) -> str:
    now = now or datetime.now(timezone.utc)
    if now.tzinfo is None:
        raise ValueError("version timestamp must include a timezone")
    return normalize(now.astimezone(timezone.utc).strftime("2.%y.%m%d.%H%M"))


if __name__ == "__main__":
    try:
        argv = sys.argv[1:]
        # --channel reports "prerelease" or "stable" so the workflow can route a
        # labelled tag without re-implementing the parsing in bash.
        wants_channel = "--channel" in argv
        if wants_channel:
            argv.remove("--channel")
        if len(argv) > 1:
            raise ValueError("usage: release_version.py [--channel] [version-or-tag]")
        version = normalize(argv[0]) if argv else generate()
        if wants_channel:
            print("prerelease" if is_prerelease(version) else "stable")
        else:
            print(version)
    except ValueError as error:
        print(f"Invalid release version: {error}", file=sys.stderr)
        sys.exit(1)
