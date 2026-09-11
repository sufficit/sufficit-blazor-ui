#!/usr/bin/env python3
"""Generate/validate Sufficit's UTC 2.yy.MMdd.HHmm NuGet versions.

Major 2: the legacy SemVer releases (1.27.0, 1.28.0, 2.0.0, 2.1.1, 2.2.1)
cannot be deleted from NuGet.org and would outrank any 1.yy calendar version
in a floating range. 2.yy.MMdd.HHmm orders above all of them.
"""

import re
import sys
from datetime import datetime, timezone


def normalize(value: str) -> str:
    match = re.fullmatch(r"v?2\.([0-9]{2})\.([0-9]{3,4})\.([0-9]{1,4})", value)
    if not match:
        raise ValueError("expected Sufficit version 2.yy.MMdd.HHmm (optional v tag prefix)")
    year, month_day, hour_minute = map(int, match.groups())
    # Validate the actual calendar, including leap years and the UTC clock.
    datetime(2000 + year, month_day // 100, month_day % 100,
             hour_minute // 100, hour_minute % 100, tzinfo=timezone.utc)
    # NuGet removes leading zeroes in each numeric segment.
    return f"2.{year}.{month_day}.{hour_minute}"


def generate(now: datetime | None = None) -> str:
    now = now or datetime.now(timezone.utc)
    if now.tzinfo is None:
        raise ValueError("version timestamp must include a timezone")
    return normalize(now.astimezone(timezone.utc).strftime("2.%y.%m%d.%H%M"))


if __name__ == "__main__":
    try:
        if len(sys.argv) > 2:
            raise ValueError("usage: release_version.py [version-or-tag]")
        print(normalize(sys.argv[1]) if len(sys.argv) == 2 else generate())
    except ValueError as error:
        print(f"Invalid release version: {error}", file=sys.stderr)
        sys.exit(1)
