# Independent evaluation prompt

Perform a complete, independent evaluation and comparison of the project at
/mnt/sufficit/sufficit-blazor-ui (a .NET Razor Class Library publishing the
"Sufficit User Interface" component library and design system for Blazor —
components, theming, a CSS bundle and ES-module JavaScript interop — consumed
by several Sufficit web applications and distributed as a NuGet package).
Assume nothing going in — investigate the code from scratch, as if seeing it
for the first time. Do not assume any specific file, folder, component or
class in advance; discover the actual structure, entry points and boundaries
yourself by exploring the repository.

> **HARD RULE — source of truth is the code, not the docs.** For THIS project
> (sufficit-blazor-ui), you must NOT read its own documentation (README,
> /docs, DESIGN.md, PRODUCT.md, CHANGELOG.md, AGENTS.md, skills/, component
> descriptions written for the catalog, XML doc comments claiming what
> something does, design notes, previous evaluations, etc.) as a source of
> truth. Project docs may be stale, aspirational, or simply wrong, and
> relying on them would give you a false picture of the real system. Build
> your understanding exclusively by reading and reasoning over the actual
> source code — the Razor components and their C# code-behind, the theme
> classes, the CSS sources and the generated bundle, the JavaScript modules,
> the project/package configuration, the sample and catalog applications,
> the tests, the scripts and the CI workflows — and form your own independent
> judgment of what the library truly does and how it truly behaves. The ONLY
> documentation you may read is for THIRD-PARTY / PUBLIC projects
> (competing component libraries, official ASP.NET Core / Blazor / .NET
> platform docs, W3C / WAI-ARIA / WCAG specifications, NuGet and npm
> documentation, etc.) during the market comparison step. If project docs
> and code ever disagree, the code wins, full stop — and note the
> discrepancy as a finding.

1. RECOGNITION — map the architecture entirely on your own: discover the
   component taxonomy and how components are organized, composed and
   related (base classes, shared parameters, cascading values, render
   fragments, generic components), how the theming system works end to end
   (theme model, provider, CSS custom properties, light/dark handling,
   per-consumer overrides), how styles are authored, built and shipped (CSS
   sources, build pipeline, bundle, static web assets, scoping strategy),
   how JavaScript interop is structured and loaded (modules, lifecycle,
   disposal, what runs on the client versus the circuit), how services are
   registered and consumed, what the public API surface actually is and
   how it is guarded (baselines, analyzers, warnings-as-errors), how the
   package is versioned, packed and published (project properties, CI
   workflows, trusted publishing, version scheme and its consequences for
   consumers using floating ranges), how the sample / catalog / showcase
   applications exercise the library, and how it is tested (unit,
   rendering, browser, visual baselines, budgets). Read the entire source
   code, do not summarize based on file, folder or component names alone,
   and do not read this project's own documentation to shortcut this step.

2. VULNERABILITIES AND DEFECTS — actively audit on your own:
   - **Injection surfaces**: every place the library renders raw markup
     (MarkupString, unencoded parameters, `innerHTML` in JS, attribute
     splatting with `AdditionalAttributes`, user-controlled `style`/`class`
     values, URLs in `href`/`src`), and whether a consumer passing untrusted
     data can produce XSS, CSS injection or open-redirect behavior.
   - **JavaScript interop trust boundary**: what the JS modules accept from
     .NET and from the DOM, what they expose globally, whether element
     references and callbacks can be misused, event handler leaks and
     disposal correctness, behavior under prerendering and reconnection.
   - **Component correctness**: parameter/state synchronization bugs
     (two-way binding, `EventCallback` misuse, `ShouldRender`,
     `StateHasChanged` outside the sync context), rendering under Blazor
     Server, WebAssembly and static SSR / streaming rendering, disposal and
     memory leaks in long-lived circuits, thread-safety of shared services,
     culture/format handling in inputs.
   - **Accessibility as a defect class**: keyboard operability, focus
     management in overlays and menus, ARIA roles/states/names, contrast
     of the shipped palettes, reduced-motion handling, form field
     labeling and error association. Treat WCAG 2.2 AA and the WAI-ARIA
     Authoring Practices as the bar and report violations as findings.
   - **Supply chain and delivery**: the npm and NuGet dependencies (pinning,
     lockfiles, known CVEs, outdated packages), integrity of the generated
     CSS/JS assets versus their sources, what the CI workflows are allowed
     to do (permissions, secrets, OIDC trusted publishing, third-party
     actions pinned or not), what gets included in the package, and whether
     a compromised or malformed release could reach consumers unnoticed.
   - **Insecure or fragile defaults**: anything a consumer gets "for free"
     that weakens their application (global CSS leakage, global JS state,
     permissive defaults, hidden network calls, telemetry).
   Classify each finding by severity with a concrete exploitation or
   failure scenario.

   **For each finding (and for every architectural weakness found in step 1
   or 4), do not stop at describing the problem — propose a concrete
   solution when relevant.** A good remediation says *how* to fix it at the
   architecture / software-design level: which abstraction to introduce,
   which pattern to apply, where in the code the change lands, and what the
   trade-off is. Favor design-level fixes (introduce a base component,
   move a responsibility, isolate a boundary, add an indirection, add an
   analyzer or a public-API gate) over one-line patches. Cite the specific
   file/component/module the change targets so the proposal is actionable,
   not generic. If a finding is purely operational (dependency bump, CI
   permission, release hygiene) with no software-design lever, say so
   explicitly and keep the recommendation brief.

3. MARKET COMPARISON — research the web for the current (most recent
   possible) state of direct competitors and adjacent players: open-source
   Blazor component libraries such as MudBlazor, Radzen Blazor, Microsoft
   Fluent UI Blazor, Blazorise, Ant Design Blazor, Havit Blazor and
   BootstrapBlazor; commercial suites such as Telerik UI for Blazor,
   Syncfusion Blazor and DevExpress Blazor; and, as the design-system
   baseline outside Blazor, headless/primitives approaches (Radix, Base
   UI, Headless UI) and token-driven systems (Material 3, Fluent 2, the
   W3C Design Tokens Community Group format). Compare component
   architecture, theming and token strategy, CSS delivery and isolation,
   JS interop model, render-mode support (Server, WebAssembly, static SSR,
   auto), accessibility posture, public-API stability and versioning
   practice, package size and performance budgets, testing approach, and
   what is currently considered the "modern" baseline for a Blazor
   component library. For this step, and only for this step, you may read
   the public documentation of these third-party projects.

4. SCORING — give a score from 0 to 10 per dimension (security,
   architecture, code quality, accessibility, feature completeness,
   consumer experience / developer ergonomics, production readiness) and
   an overall score, with objective justification for each. Rank the
   project against the competitors researched.

   **Scoring is not the deliverable — the architecture-improvement proposals
   are.** Alongside the architecture score, include a dedicated
   "Architecture improvements" section that lists, prioritized by impact,
   the concrete software-design changes you recommend (introduce / refactor
   / extract / decouple / relocate which component, base class, service,
   theme layer, build step or gate, and why). This is where the evaluation
   earns its value: a number without a proposal is a complaint; a number
   with a design-level proposal is engineering feedback. If you award a low
   architecture score, the corresponding proposals must explain how to
   raise it.

5. VERDICT — direct conclusion: strengths, risks that block adoption as the
   single UI foundation for production applications, and whether you would
   recommend a new Sufficit application standardizing on this library
   today instead of a third-party suite. The verdict must reference the
   top architecture-improvement proposals from step 4 as the roadmap to
   close the gaps — i.e. "to reach a production-ready library, do X, Y, Z",
   not just "the architecture is weak".

Work with full autonomy: run commands, build the solution, run the test
suites, read any file in the repository, search the web, and use parallel
agents to speed up investigation and market research. Do not ask before
acting — decide and execute.

Save the result to
/mnt/sufficit/sufficit-blazor-ui/docs/evaluations/EVALUATION-<date>-<model-name>.md
(name of the model used for this evaluation in the file name). Do not commit
anything.
