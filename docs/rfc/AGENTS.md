# Standards Adherence Review — Agent Instructions

This folder holds one document per RFC or specification that applies to this
project, plus an index (`README.md`). Each document states what the standard
requires, where the code complies, where it diverges, and which divergences are
intentional — always backed by evidence taken from the code itself.

This file is the process to **regenerate or revise** that analysis. It is
project-agnostic: copy this file into `docs/rfc/` of any repository and use the
request prompt below.

---

## How to request a review

Paste this to the agent, adjusting the settings if needed:

```text
Read docs/rfc/AGENTS.md and execute the full standards adherence review it
describes for this repository. Do not rely on previous conversations, memory,
or the claims inside the existing docs/rfc/ documents: read the code and verify
every statement yourself. Update the existing documents in place, create
documents for newly applicable standards, retire documents that no longer
apply, refresh the index and the review timestamps, then commit following the
repository conventions.

Settings:
- Output language: English
- Scope: whole repository
- Mode: revision (regenerate if docs/rfc/ is empty)
```

### Settings

| Setting | Default | Meaning |
|---|---|---|
| Output language | English | Language of every generated document. If existing documents are in another language, rewrite them in the configured language during the revision. |
| Scope | whole repository | Can be narrowed to a domain (e.g. "OAuth only") or a path. Documents outside the scope are left untouched, but the index still lists them. |
| Mode | revision | `revision` updates existing documents; `regenerate` discards them and starts from zero. Both follow the same verification rules. |

---

## Non-negotiable rules

1. **Zero prior knowledge.** Treat existing documents, commit messages, chat
   history and memory as *hypotheses*, never as evidence. A claim survives only
   if you re-verify it against the current code.
2. **Every citation is opened.** Each `path/to/file:line` reference must point to
   a line you actually read in this run. Line numbers drift; re-check them.
3. **Absence must be proven.** Saying "not implemented" requires stating what you
   searched for (patterns, file types, directories). Search for synonyms and
   framework idioms, not just the literal header or keyword.
4. **State the project's role.** Many standards have several roles (e.g. OAuth
   authorization server, resource server, client). Evaluate each role the
   project actually plays, separately.
5. **Distinguish verified from inferred.** If a conclusion depends on runtime
   configuration, deployment files outside the repository, or behavior you did
   not execute, say so explicitly ("inferred from configuration, not tested").
6. **Never copy secrets.** Do not paste credentials, tokens, connection strings
   or private keys into any document. Mention only the file and the kind of
   secret.
7. **No code changes.** This process only reads code and writes documentation.
   Bugs found are recorded, not fixed.
8. **Concise and concrete.** Each gap has a concrete impact and a concrete
   recommendation. No generic advice, no filler.

---

## Process

### Phase 0 — Baseline

- Record the review timestamp in UTC (`date -u +%Y-%m-%dT%H:%MZ`) and the short
  commit hash (`git rev-parse --short HEAD`). Note uncommitted changes, if any.
- Identify directories to ignore: build outputs, vendored dependencies, git
  worktrees, publish artifacts, temporary folders, generated code.
- Read the repository's own agent instructions (`CLAUDE.md`, `AGENTS.md`,
  `CONTRIBUTING.md`) for commit and documentation conventions.

### Phase 1 — Map the surface (from code, not docs)

Build a factual inventory before choosing standards:

- **Inbound interfaces:** HTTP routes and controllers, minimal APIs, gRPC,
  WebSocket, SSE, JSON-RPC, CLI, message consumers.
- **Emulated or public protocols:** compatibility layers for third-party APIs.
- **Authentication and authorization:** token validation, sessions, cookies, API
  keys, OAuth/OIDC flows (as server, proxy or client), metadata endpoints.
- **Outbound connections:** HTTP clients, connection handlers, DNS/IP handling,
  TLS settings, proxies, retries, timeouts.
- **Edge and deployment:** reverse proxy configuration, forwarded headers, HSTS,
  CORS, container/service files present in the repository.
- **Data formats and identifiers:** JSON serialization, dates, UUIDs, Base64,
  data URLs, pagination cursors.
- **Observability:** trace and correlation headers, logging of sensitive data.

### Phase 2 — Select applicable standards

Start from the candidate catalog below, add anything the inventory reveals, and
decide for each candidate: **applies** (gets a document) or **does not apply**
(listed in the index with a one-line justification and the searches that
support it). Related standards may share one document (e.g. RFC 6749 + 6750).

| Domain | Candidates |
|---|---|
| HTTP semantics | RFC 9110, 9111, 9112, 9113, 6585, 9457, 8288, 5789, 7240, 9205, draft-ietf-httpapi-idempotency-key-header, draft-ietf-httpapi-ratelimit-headers, RFC 9745 + 8594 |
| Data formats | RFC 8259, 7493, 9562, 4648, 2397, 3986, 3339 |
| OAuth and identity | RFC 6749, 6750, 7636, 8628, 8414, 9728, 8707, 7591, 7009, 7662, 9700, 7519, 8725, 9068, 9449, 8693, 7617, 6265, OpenID Connect Core/Discovery |
| Transport and network security | RFC 6797, 7239, 8305, 6724, 6890, 1918, 4193, 6598, 8446, 6455, 9116, WHATWG Fetch (CORS), OWASP SSRF guidance |
| Streaming, APIs and observability | WHATWG Server-Sent Events, JSON-RPC 2.0, Model Context Protocol, OpenAPI 3.1, W3C Trace Context, OpenTelemetry semantic conventions, Standard Webhooks, de facto vendor APIs the project emulates or consumes |

### Phase 3 — Verify in parallel by domain

Split the applicable standards into independent domains and review them in
parallel (one sub-agent per domain when available). Each reviewer receives:

- the repository path and directories to ignore;
- the list of standards in its domain;
- the rules above and the document template below;
- the instruction to treat any existing document as an unverified hypothesis;
- the instruction to write only its own files and never edit code;
- the required return value: a table `file | status | main gap`, the standards
  judged not applicable with justification, corrections to previous documents,
  and out-of-scope findings (security issues, bugs) noticed along the way.

Reviewers must re-open the code for every claim, including claims they are
merely carrying over from the previous revision.

### Phase 4 — Write or update the documents

File naming: `rfc-NNNN-short-slug.md`, `rfc-NNNN-MMMM-short-slug.md` for pairs,
`draft-<name>.md` for drafts, `<org>-<spec>-short-slug.md` for non-IETF
specifications (e.g. `whatwg-sse-server-sent-events.md`).

When revising an existing document:

- keep the file name, rewrite the content from the fresh verification;
- update status, evidence and gaps; remove gaps that were fixed and note them in
  the revision history;
- append one row to the revision history.

When a standard no longer applies, delete its document and list it under
"Evaluated without document" in the index, with the reason.

### Phase 5 — Coordinator verification

Before publishing, the coordinating agent must itself:

- open and confirm the evidence for every high-priority gap (🔴) and a sample of
  the remaining claims; correct or drop anything that does not hold;
- resolve contradictions between documents (two reviewers describing the same
  code differently);
- check that relative links between documents resolve;
- scan the generated files for secrets (passwords, tokens, keys, connection
  strings) and remove any occurrence;
- confirm every document has the current timestamp and commit hash.

### Phase 6 — Index and delivery

Rewrite `README.md` in this folder with:

1. last review timestamp and commit;
2. legend for status and priority;
3. **highest-risk findings** table (ranked, each linking to its document);
4. one table per domain: document, standard, status;
5. evaluated standards without a document, with justification;
6. out-of-scope findings noticed during the review;
7. intentional divergences preserved across documents.

Link the index from the repository's main documentation index if one exists.
Commit only the documentation files, following the repository conventions, and
report to the user: what changed since the previous review, the top risks, and
anything that could not be verified.

---

## Document template

```markdown
# RFC NNNN — Title

**Status:** ✅ compliant | 🟡 partial | 🔴 missing | ⚪ partially not applicable — one sentence
**Last reviewed:** YYYY-MM-DDTHH:MMZ · commit `abc1234`
**Project role:** (only when the standard has roles, e.g. authorization server / resource server / client)

## What the standard requires
Only the requirements that touch this project, each with its section number.

## Where the project complies
- Requirement → evidence `path/to/file.ext:line` (verified in this review).

## Gaps
| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| 🔴 / 🔶 / 🔵 | ... | ... | ... |

Searches performed for claimed absences: `pattern` in `*.ext` under `dir/`.

## Intentional divergences
Deviations that are deliberate design decisions (with the reason), so future
reviews do not report them as gaps.

## Revision history
| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| YYYY-MM-DDTHH:MMZ | `abc1234` | Initial analysis / what changed |

## References
- Standard URL
- Related documents in this folder
```

**Status legend:** ✅ compliant · 🟡 partial · 🔴 missing · ⚪ partially not applicable
**Priority legend:** 🔴 high (security, data loss, broken clients) · 🔶 medium (interoperability, reliability) · 🔵 low (polish, future-proofing)
