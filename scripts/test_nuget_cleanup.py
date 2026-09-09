import os
import unittest
from contextlib import redirect_stdout
from io import StringIO
from unittest.mock import patch

import unlist_legacy_packages as cleanup


class NuGetCleanupTests(unittest.TestCase):
    def test_reads_inline_registration_without_waiting_for_version_leaf(self):
        metadata = {"version": "1.26.908.1200", "listed": True}
        index = {"items": [{"items": [{"catalogEntry": metadata}]}]}
        with patch.object(cleanup, "get_json", return_value=index) as request:
            self.assertEqual(metadata, cleanup.catalog(metadata["version"]))
            self.assertEqual(1, request.call_count)

    def test_reads_registration_page_when_not_inlined(self):
        metadata = {"version": "1.28.0", "listed": False}
        responses = [{"items": [{"@id": "https://api.nuget.org/page"}]},
                     {"items": [{"catalogEntry": metadata}]}]
        with patch.object(cleanup, "get_json", side_effect=responses):
            self.assertEqual(metadata, cleanup.catalog(metadata["version"]))

    def run_cleanup(self, apply=False, listed=True, key="test-key"):
        args = ["cleanup", "--replacement", "1.26.908.1200"]
        if apply:
            args.append("--apply")
        with patch("sys.argv", args), patch.dict(os.environ, {"NUGET_API_KEY": key}), \
             patch.object(cleanup, "catalog", return_value={"id": cleanup.PACKAGE, "listed": listed}), \
             patch.object(cleanup.urllib.request, "urlopen") as request, \
             redirect_stdout(StringIO()):
            cleanup.main()
            return request.call_args_list

    def test_default_does_not_delete(self):
        self.assertEqual([], self.run_cleanup())

    def test_apply_deletes_only_the_five_approved_versions(self):
        calls = self.run_cleanup(apply=True)
        self.assertEqual(5, len(calls))
        for call, version in zip(calls, cleanup.LEGACY_VERSIONS):
            request = call.args[0]
            self.assertEqual("DELETE", request.method)
            self.assertEqual(f"https://www.nuget.org/api/v2/package/Sufficit.Blazor.UI/{version}",
                             request.full_url)

    def test_missing_key_stops_before_deletion(self):
        with self.assertRaisesRegex(SystemExit, "NUGET_API_KEY"):
            self.run_cleanup(apply=True, key="")

    def test_unlisted_replacement_stops_before_deletion(self):
        with self.assertRaisesRegex(SystemExit, "replacement"):
            self.run_cleanup(apply=True, listed=False)


if __name__ == "__main__":
    unittest.main()
