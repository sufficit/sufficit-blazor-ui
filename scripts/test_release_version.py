import unittest
from datetime import datetime, timedelta, timezone

from release_version import generate, normalize


class ReleaseVersionTests(unittest.TestCase):
    def test_normalizes_msbuild_and_nuget_versions(self):
        for value in ("1.26.0908.0005", "v1.26.908.5", "1.26.908.5"):
            with self.subTest(value=value):
                self.assertEqual("1.26.908.5", normalize(value))

    def test_rejects_legacy_versions_and_debug_sentinel(self):
        for value in ("1.27.0", "1.28.0", "2.0.0", "2.1.1", "2.2.1",
                      "1.99.0.0", "0.0.0-local", "1.26.908.1200-preview.1"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                normalize(value)

    def test_rejects_impossible_dates_and_times(self):
        for value in ("1.26.229.1200", "1.26.431.1200", "1.26.1301.0",
                      "1.26.908.2400", "1.26.908.1260", "1.26.908.9999"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                normalize(value)

    def test_accepts_leap_day_and_midnight(self):
        self.assertEqual("1.24.229.0", normalize("v1.24.0229.0000"))

    def test_converts_to_utc_across_year_boundary(self):
        local = datetime(2026, 12, 31, 23, 5, tzinfo=timezone(timedelta(hours=-3)))
        self.assertEqual("1.27.101.205", generate(local))

    def test_requires_timezone(self):
        with self.assertRaises(ValueError):
            generate(datetime(2026, 9, 8))


if __name__ == "__main__":
    unittest.main()
