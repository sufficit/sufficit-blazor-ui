import unittest
from datetime import datetime, timedelta, timezone

from release_version import generate, is_prerelease, normalize


class ReleaseVersionTests(unittest.TestCase):
    def test_normalizes_msbuild_and_nuget_versions(self):
        for value in ("2.26.0908.0005", "v2.26.908.5", "2.26.908.5"):
            with self.subTest(value=value):
                self.assertEqual("2.26.908.5", normalize(value))

    def test_rejects_legacy_versions_and_debug_sentinel(self):
        for value in ("1.27.0", "1.28.0", "2.0.0", "2.1.1", "2.2.1",
                      "1.26.908.2020", "1.99.0.0", "2.99.0.0", "0.0.0-local",
                      "2.6.908.1200"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                normalize(value)

    def test_accepts_prerelease_labels(self):
        for value, expected in (
            ("2.26.0908.1200-preview.1", "2.26.908.1200-preview.1"),
            ("v2.26.908.1200-rc.10", "2.26.908.1200-rc.10"),
            ("2.26.908.1200-preview.01", "2.26.908.1200-preview.1"),
        ):
            with self.subTest(value=value):
                self.assertEqual(expected, normalize(value))
                self.assertTrue(is_prerelease(value))

    def test_rejects_malformed_prerelease_labels(self):
        # An unnumbered or upper-case label would sort unpredictably against its
        # siblings on NuGet, and a bare dash is a typo, not a channel.
        for value in ("2.26.908.1200-preview", "2.26.908.1200-", "2.26.908.1200-1.1",
                      "2.26.908.1200-Preview.1", "2.26.908.1200-preview.1234"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                normalize(value)

    def test_stable_versions_are_not_prerelease(self):
        self.assertFalse(is_prerelease("2.26.0908.1200"))

    def test_rejects_impossible_dates_and_times(self):
        for value in ("2.26.229.1200", "2.26.431.1200", "2.26.1301.0",
                      "2.26.908.2400", "2.26.908.1260", "2.26.908.9999"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                normalize(value)

    def test_accepts_leap_day_and_midnight(self):
        self.assertEqual("2.24.229.0", normalize("v2.24.0229.0000"))

    def test_converts_to_utc_across_year_boundary(self):
        local = datetime(2026, 12, 31, 23, 5, tzinfo=timezone(timedelta(hours=-3)))
        self.assertEqual("2.27.101.205", generate(local))

    def test_requires_timezone(self):
        with self.assertRaises(ValueError):
            generate(datetime(2026, 9, 8))


if __name__ == "__main__":
    unittest.main()
