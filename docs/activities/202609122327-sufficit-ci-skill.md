# Sufficit CI operations in the GitHub CLI skill

## Objective

Teach Genius agents to inspect, diagnose, rerun and monitor Sufficit CI autonomously through GitHub and `gh`, based on the reviewed CI configuration under `/mnt/aireset/servers/contigencia`.

## Starting state

The public `github-cli` skill covered GitHub Actions at a general level, but it did not explain Sufficit runner labels, queued jobs, evidence required to classify a failure as pre-existing, or how to retrieve job logs when the normal `gh run view --log-failed` path returns no useful output. This allowed an agent to ask a human to paste CI output even when the connected GitHub account could obtain it directly.

## Delivered

- Released `github-cli` skill version `1.1.0` with a focused Sufficit CI reference.
- Defined the evidence order: repository instructions, workflow YAML, local CI scripts, GitHub runs and logs, artifacts, then runner state.
- Documented the current Linux, Windows and macOS runner labels, while keeping each workflow's `runs-on` declaration as the source of truth.
- Distinguished queued jobs from failures and documented the one-job-per-runner capacity model.
- Added autonomous commands for check discovery, job-level log retrieval, artifact inspection, reruns and bounded monitoring.
- Required comparison against the base branch or equivalent evidence before calling a failure pre-existing.
- Explicitly prohibited asking the user to paste logs that the connected account can retrieve.
- Kept passwords, tokens, private addresses, Vault content, credential paths and host administration commands out of the public package.

## Validation

- `quick_validate.py skills/github-cli`: valid.
- Sensitive-value scan against the reviewed CI source: no private infrastructure values copied.
- `git diff --check`: passed.
- `dotnet restore Sufficit.Blazor.UI.slnx`: passed.
- `dotnet build Sufficit.Blazor.UI.slnx --configuration Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test tests/Sufficit.Blazor.UI.Tests/Sufficit.Blazor.UI.Tests.csproj --configuration Release --no-restore`: 668 passed.
- Pull request CI covered build, component tests, package inspection, CSS budget, C# analysis, CodeQL, Chromium, Firefox, WebKit and Lighthouse.

The first build attempt with `--no-restore` in the fresh worktree correctly failed because `project.assets.json` did not exist. Restoring dependencies resolved the setup condition; no product defect was involved.

## Delivery references

- Source issue: `sufficit/sufficit-blazor-ui#24`.
- Source pull request: `sufficit/sufficit-blazor-ui#25`.
- Genius catalog issue: `sufficit/sufficit-ai-genius#754`.
- Regression example reviewed: `Hermes-SRV/hermes-premium#536`.

The merged source commit is the immutable revision consumed by the Genius public catalog.
