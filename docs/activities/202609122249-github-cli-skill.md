# Public GitHub CLI skill

## Objective

Publish the canonical public skill package that teaches Sufficit AI Genius agents to use the structured GitHub CLI and attachment tools introduced by `sufficit-ai-genius#749`.

## Starting state

The public Genius skill catalog contained only `sufficit-frontend` from this repository. GitHub guidance was not packaged for lazy installation, and no reference described issue attachments or the host-managed authorization boundary.

## Delivered

- Added `skills/github-cli/SKILL.md` with the normal discovery description, structured argument contract, authorization behavior, state verification, and untrusted-content boundary.
- Added on-demand references for issues and pull requests, Actions and API calls, releases and workflow artifacts, and GitHub issue attachments.
- Added package metadata, UI metadata, card, and MIT-0 license at version `1.0.0`.
- Kept access tokens out of the package. The skill requires the host's `github_cli` and `github_attachment_download` tools and directs disconnected users to the Genius integration screen.

## Validation

- `quick_validate.py skills/github-cli`: valid.
- `git diff --check`: passed.
- `dotnet build Sufficit.Blazor.UI.slnx --configuration Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test tests/Sufficit.Blazor.UI.Tests/Sufficit.Blazor.UI.Tests.csproj --configuration Release --no-restore`: 668 passed.
- The unhosted aggregate browser run reached 58 tests and every attempted page failed with `ERR_CONNECTION_REFUSED` because the required catalog at `127.0.0.1:5180` was not running. This package changes no UI runtime code; the Release build and unit suite are green.

## Delivery references

- Source issue: `sufficit/sufficit-blazor-ui#22`.
- Runtime and catalog integration: `sufficit/sufficit-ai-genius#749`.

The merged commit is the immutable revision consumed by the Genius public catalog.
