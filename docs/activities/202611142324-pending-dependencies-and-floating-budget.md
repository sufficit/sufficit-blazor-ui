# Dependency integration and floating-surface budget repair

Owner authorized pending main integration and Genius release. Composed SUI PRs
66–71 (AxeCore, browser Playwright, test SDK and pinned setup-dotnet/CodeQL updates).
Adjacent package-version conflicts were resolved preserving every requested
upgrade. Upstream action tags were verified, including the peeled CodeQL commit.

Integration and unchanged main both failed the same three existing tooltip asset
budgets: raw bytes, lines and total Brotli. Shared floating geometry is now
exported from the existing colocated popover module and reused by the tooltip,
preserving each component's fallback ordering/margins. No new global module,
component API, appearance change or raised budget. Concise comments retain touch
and focus constraints. Tooltip is 315 lines; popover is 209.

Validation: 1166 unit tests passed with zero skips; 15 version-contract tests
passed; JavaScript syntax passed; 62 original-versus-refactored placement/coordinate
comparisons passed. Three Chromium tests passed: tooltip behavior, multiple
instances/removal/recreation, and configured subpath module loading. Owned catalog
processes were stopped after testing. Browser-test/catalog builds passed.
The entire cross-browser/visual suite was not run locally; automatic main CI is
separate evidence. git diff --check passed. Main publication follows this record.
