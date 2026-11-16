# Native contracts for the ongoing application migration

This library delivery completes reusable-contract checkpoint 2 of
`/mnt/sufficit/sufficit-blazor/docs/PLAN-MUDBLAZOR-REMOVAL.md`; the application-wide
migration and production deployment remain pending in that plan.

- Dialog presentation exposes width, backdrop/Escape policies and host close control.
  Existing providers cannot silently ignore restrictive dismissal policies.
- Grid supports explicit four-pixel gap steps; vertical separators use native semantics.
- Linear progress distinguishes unknown progress from numeric completion, provides
  accessible loading text in tables and a static reduced-motion fallback.
- Typed native date/time fields preserve civil date and clock values without UTC
  conversion, accept nullable clearing and reject invalid values without model writes.
  Native range formatting remains invariant. No Mud compatibility wrapper was added.
- Updated public API baseline, copied examples, documentation, CSS mirrors and budget
  measurements. Existing size ceilings remain 63,232 B raw, 11,776 B gzip and 10,240 B
  Brotli; actual global CSS is 63,084 / 11,523 / 10,027 B respectively.

Validation: complete Release solution build with warnings as errors; 1,227 component
tests passed; all 84 standalone catalog examples compile; complete Chromium suite
134 passed (including root/pathbase and showcase checks). New checks cover pt-BR /
America/Sao_Paulo date/time edits, controlled dialogs, narrow/wide geometry, vertical
separator, progress semantics and reduced motion. Final local package
Sufficit.Blazor.UI.2.26.1116.2231 validates executable net10.0 consumers at root and
pathbase. No NuGet publication or production deployment is claimed.
