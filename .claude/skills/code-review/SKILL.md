---
name: code-review
description: Review changed code (uncommitted by default, or a branch) against the repo conventions and for concrete quality bugs. Report-only. Triggers - "code review", "review the code", "review the changes", "review the branch", "check my changes".
---

# Code Review

Reviews changed code against the repo conventions, for architecture and code quality, and for concrete defects. Business-rule correctness is left to spec-check, which runs next (see Handoff). When backend and frontend both changed, it first checks the contract between them.

-

## Triggers

- "code review", "review the code", "review the changes", "review the branch", "check my changes". A code object is required; "review the design" is not a code review.
- Fixing a finding is a separate, explicit request.

-

## Procedure

### Flow

The review runs in one fresh subagent, so it reads the change without the context of the session that wrote it:

1. **Main thread:** resolve the scope (Scope), then spawn one general-purpose subagent with `model: opus` (CLAUDE.md 'Subagent policy') and the prompt below. If none can be spawned, run the same steps inline.
2. **Subagent:** runs Collect, Contract review, Per-file review and Confidence filter, and returns only the findings text. It spawns nothing.
3. **Main thread:** prints the findings verbatim, then runs the Handoff.

```text
Perform a code review. Scope: <uncommitted (default) | branch <name> vs origin/<base> | PR>.
Areas changed: <backend | frontend | infra | several>. Follow the Procedure subsections Collect, Contract review,
Per-file review and Confidence filter, and the Output section, of .claude/skills/code-review/SKILL.md.
Read .claude/CLAUDE.md in full, the standards file of each changed area (docs/BACKEND-STANDARDS.md,
docs/FRONTEND-STANDARDS.md or docs/DEPLOYMENT-STANDARDS.md), and the file of each changed layer:
docs/layers/domain.md, docs/layers/application.md, docs/layers/persistence.md, docs/layers/infrastructure.md, docs/layers/api.md,
docs/layers/frontend.md or docs/layers/deployment.md.
Read-only: no checkout, no git writes, no file edits. Return only findings.
```

### Scope

- **Default:** uncommitted changes (staged, unstaged and new untracked files vs HEAD), the usual "check before I commit".
- **Branch or PR:** only when the user names one: committed changes vs the base (`origin/<base>` when a remote exists, else the local `<base>`; `<base>` is `main` or `master`, whichever exists).
- **Clean working tree:** fall back to the branch scope and say so; never report "nothing to review" when the branch has committed work.
- **No repository or no commit yet:** `git diff HEAD` fails, so treat every file not excluded by `.gitignore` as added, say so, and review the files whole.

### Collect

```bash
git diff HEAD --name-only                  # staged + unstaged (tracked)
git ls-files --others --exclude-standard   # new untracked files
git diff --name-only origin/<base>...<tip> # branch scope
```

Keep `.cs`, `.csproj`, `.ts`, `.html`, `.scss`, `.json`, `.bicep`, `.bicepparam`, `.sql`, `.yml`, `.ps1`; drop lock files, `bin/`, `obj/`, `node_modules/`, `dist/`, `.claude/` and `docs/`. Files under `Migrations/` are skimmed for hand edits only. If nothing is left after the fallback, say so and stop. The areas present (backend, frontend, infra) decide which layer files to read and whether Contract review runs.

### Contract review

Only when backend and frontend both changed. Read the changed files from both sides and check the seam:

- a route, HTTP method or parameter shape the Angular service calls that the controller does not match;
- a DTO or result shape (including the error mapping) that differs between the API response and the frontend model;
- an enum value or status string added on one side and missing or different on the other;
- a new required request field with no form control, or a form field the API does not accept;
- a frontend call whose endpoint is not in the changed files: search for the controller, and if none exists report `endpoint not found`.

### Per-file review

Get each file's diff for the same scope (`git diff HEAD -- "<file>"`, or `git diff origin/<base>...<tip> -- "<file>"`; a new untracked file is read whole). Flag findings only on added or modified lines, but read past the diff to judge them: the whole method, the handler a controller calls, the configuration an entity maps through, the other paths that write the same field. The deep bugs live in how a change meets code that did not change.

The C# standards apply to the backend. For frontend files apply `docs/FRONTEND-STANDARDS.md`, 'Syntax hygiene', the Frontend checks and the bug classes.

**Standards** (a checklist; the rules are in CLAUDE.md 'Core rules', the standards file of the area and the layer file):

- Newest syntax: older forms where a collection expression, pattern or `required` member fits
- No null: a null return or null-initialized field in Domain or Application; a boundary null not converted
- Small methods: a method over complexity 4; a method reading no instance state with no side effects that is not `static`
- Result abstraction: a controller building error JSON by hand; Application returning HTTP types
- Syntax hygiene: missing braces; string comparison without `StringComparison`; `==` on reference types
- Code style: a line over the line length; layout, declaration or member-order rules broken
- Code shapes (`docs/layers/domain.md`): a rule, operation, transition, entity, root method, details record, closed set, event, options class or extension class that departs from its shape; a precondition inline in an operation; a state change outside a transition; the Domain reading a clock outside `Ids`; a child entity changed from outside its aggregate
- Files and folders: more than one type per file or a name that differs from its type; a block-scoped namespace; an interface in the wrong project or not named `I<Name>`; a file outside its prescribed folder
- Naming: a type whose name hides its role; a bool without `Is`, `Has`, `Can` or `Should`; a single-letter or type-in-name identifier; `Async`, `CancellationToken` or `sealed` conventions broken
- Tests: a class not mirroring its source folder; a name off `<Subject>_<Condition>_<Result>`; more than one act; input built inside the act; a message asserted instead of a code; a missing or misplaced `Trait`
- No explanatory comments: a comment added (unless user-requested or XML doc on public API)
- Migrations: a hand-edited migration
- Architecture: a type in the wrong project or a reference pointing outward; a state change that is not a Command or a read that is not a Query; Unit of Work bypassed; no domain event on a significant transition; a command without a validator; storage or audit reached without `IStorageService` or `IAuditLogService`
- Data conventions: data annotations instead of Fluent API; decimal without `HasPrecision(19, 4)`; missing soft-delete or tenant filter, audit columns or `RowVer`; seed data applied at startup; a hard-coded GUID in logic; command, query, DTO or route names off-pattern
- Frontend (`docs/FRONTEND-STANDARDS.md`): a file or class name off the role convention; a component that is not standalone and OnPush, or whose template reads state that is not a signal; a hex colour in a component style; `any`
- Deployment (`docs/DEPLOYMENT-STANDARDS.md`, `docs/layers/deployment.md`): an unpinned action; a secret or connection string in a template, script or workflow; a feature switch on outside the demo; a script that fails silently

**Concrete quality bugs** (reasoned, not pattern-matched; flag only when the problem and the fix are both unambiguous):

- **Data integrity:** nullability that breaks a unique index or join; a string column too small for the data written; a field set on one write path but missing on its sibling; an enum or status handled in one switch but not its counterpart.
- **Async:** `.Result` or `.Wait()`, `async void`, a missing `await`, a `CancellationToken` not passed through.
- **EF and lifetime:** a `DbContext` captured by a longer-lived service; N+1 queries in a loop; tracking on pure reads; materializing before filtering.
- **Resources:** `new HttpClient` per call (use `IHttpClientFactory`); state in instance fields of a transient service.
- **Error visibility:** a `catch` that swallows without a log, rethrow or result; a result written to a field no caller reads.
- **Background jobs:** a Hangfire job that is not idempotent, or whose retry duplicates a side effect.
- **Security:** a state-changing endpoint without authorization; identity or role taken from the request or hard-coded; secrets in code or config; user input concatenated into queries.
- **Design:** a handler doing more than one job; business logic in a controller, adapter or migration; EF or HTTP types leaking into Domain or Application; duplicated logic; static mutable state; a magic value that names a concept; new logic without a test, or a test that asserts nothing; a public member nothing uses.
- **Dead logic:** a reachable `NotImplementedException`; a leftover TODO, `Console.WriteLine`, `debugger`, hard-coded test ID or `DEV-LOG` line.
- **Frontend:** an unsubscribed `subscribe`; a hard-coded absolute API URL; a hidden form field keeping a stale value; `HttpClient` outside the service layer; an API call without loading state or error notification; a role-restricted action shown to all roles; a form step that advances without validating; `any` instead of a typed model.

### Confidence filter

Report a finding only when all three hold:

1. The violation is on a new or modified line.
2. The rule is explicit in CLAUDE.md or the layer file, or it is a concrete bug from the list above (not a style preference).
3. The fix is unambiguous.

Skip a pattern that exists unchanged in the surrounding code or whose context is genuinely ambiguous.

-

## Output

Per CLAUDE.md 'Output discipline': only the findings, as plain text outside any code block. A clean file or category produces zero lines; if everything is clean, one short sentence such as `changes are clean`.

Contract section first (only if both sides changed), then findings grouped by file. Each finding is its own paragraph with a blank line between findings; `Fix:` starts a new line. Repo-relative paths, no bold, no dividers, no per-file sub-headers. A clean contract section reads `no contract issues` under the connection line.

```text
contract - claim submission
connection: Angular claim.service posts to ClaimsController.Submit with the SubmitClaimCommand shape

frontend/src/app/claims/claim.service.ts:31 - route mismatch: posts to /api/claim, controller route is /api/claims.
Fix: change the path to /api/claims.

backend/ClaimsModule.Application/Claims/SubmitClaimCommandHandler.cs:44 - small methods: Handle has complexity 6.
Fix: extract the status mapping into a private static method.
```

-

## Handoff

After the findings are printed, the main thread invokes spec-check in passive mode with the changed-file list from Collect. Its output (a `rules` section, a one-line `rules: ... covered`, or nothing) follows the findings with no extra framing. Skip the handoff when no changed file is backend, domain-relevant frontend or a test.

-

## Guardrails

- Report-only: no file edits, no git writes, no checkout; never fix a finding unless the user asks in a separate request.
- Confidential document text stays out of findings (CLAUDE.md 'Domain sources').
- Approval-first actions follow CLAUDE.md 'Permissions model'.
