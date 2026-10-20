---
name: code-review
description: Review changed code (uncommitted by default, or a branch) against the repo conventions and for concrete quality bugs. Report-only. Triggers - "code review", "review the code", "review the changes", "review the branch", "check my changes".
---

# Code Review

Reviews changed code against CLAUDE.md conventions, for architecture and code quality, and for concrete defects. Business-rule correctness is left to spec-check, which runs next (see Handoff). When both backend and frontend changed, first checks the contract between them, then reviews each file independently.

-

## Triggers

- "code review", "review the code", "review the changes", "review the branch", "check my changes". A code object is required; "review the design" is not a code review.
- Fixing a finding is a separate, explicit request.

-

## Procedure

### Flow

Split between the main thread and one fresh subagent, so the review reads the change without the context of the session that wrote it:

1. **Main thread:** resolve the scope (Scope), then spawn one fresh general-purpose subagent (`model: opus`, per CLAUDE.md 'Subagent policy': a review is a high-stakes judgment pass) and hand it the scope.
2. **Subagent:** runs Collect, Contract review, Per-file review and Confidence filter, and returns only the findings text. No further subagent. If none can be spawned, the main thread runs the same steps inline.
3. **Main thread:** prints the findings verbatim, then runs the Handoff.

Spawn prompt:

```text
Perform a code review. Scope: <uncommitted (default) | branch <name> vs origin/<base> | PR>.
Areas changed: <backend | frontend | both>. Follow the Procedure subsections Collect, Contract review,
Per-file review and Confidence filter, and the Output section, of .claude/skills/code-review/SKILL.md.
Read .claude/CLAUDE.md in full, docs/CODING-STANDARDS.md, and the file in docs/layers/ of each changed layer (docs/layers/domain.md, docs/layers/application.md, docs/layers/persistence.md, docs/layers/infrastructure.md or docs/layers/api.md). Read-only: no checkout, no git
writes, no file edits. Return only findings.
```

### Scope

- **Default: uncommitted changes** (staged, unstaged, and new untracked files vs HEAD). This is the usual "check before I commit".
- **Branch / PR:** only when the user names one: committed changes vs the base (`main` or `master`, whichever exists; `origin/<base>` when a remote exists, else the local `<base>`).
- **Fallback:** if the working tree is clean, fall back to the branch scope and say so; never report "nothing to review" when the branch has committed work.
- **No repository or no commit yet:** `git diff HEAD` fails, so treat every file not excluded by `.gitignore` as added, say so, and review the files whole.
- The changed areas (backend, frontend, or both) decide whether Contract review runs.

### Collect

```bash
git diff HEAD --name-only                  # staged + unstaged (tracked)
git ls-files --others --exclude-standard   # new untracked files
git diff --name-only origin/<base>...<tip> # branch scope
```

Filter to `.cs`, `.csproj`, `.ts`, `.html`, `.scss`, `.json` (not lock files, `bin/`, `obj/`, `node_modules/`, `dist/`, `.claude/`, or `docs/`). Files under `Migrations/` are listed but skimmed for hand edits only. If nothing matches after the fallback, say so and stop.

### Contract review

Only when backend and frontend both changed. Read the changed files from both sides, then check the seam (structural analysis, not syntax):

- a route, HTTP method, or parameter shape the Angular service calls that does not match the controller;
- a DTO or result shape (including the error mapping) that differs between the API response and the frontend model;
- an enum value or status string added on one side and missing or different on the other;
- a new required request field with no form control, or a form field the API does not accept;
- a frontend call whose endpoint is not in the changed files: search the repo for the controller, and if none exists report `endpoint not found`.

### Per-file review

Get each file's diff for the same scope (`git diff HEAD -- "<file>"`, or `git diff origin/<base>...<tip> -- "<file>"`; a new untracked file is read whole as added). Flag findings only on added or modified lines, but read past the diff to judge them: the whole method a line sits in, the handler a controller calls, the configuration a changed entity maps through, the other code paths that write the same field. The deep bugs live in how a change interacts with code that did not change.

The structural C# standards apply to the backend. For frontend files apply syntax hygiene, the frontend checks, and the concrete-bug classes.

**CLAUDE.md standards** (a checklist, not a restatement; read the sections there):

- 'Newest syntax & APIs': collection expressions, pattern matching, `required` members over older forms
- 'No null': a null return or null-initialized field in Domain/Application (preferred elsewhere); null not converted at a boundary
- 'Extract logic into `private static` methods': methods reading no instance state and having no side effects but not static
- 'Cyclomatic complexity <= 4 per method': a method over 4
- 'API results go through one result abstraction': a controller hand-building error JSON, or Application returning HTTP types
- 'Syntax hygiene': missing braces; `==` on strings where `StringComparison` matters; `==` on reference types
- 'Code style': a line over the line length; a LINQ chain not laid out one call per line; a wrapped call nested in another call; layout, declaration (`var`, explicit empty values, `foreach` with `var`) or member-order rules broken
- 'Code shapes': a rule, operation, transition, entity, root method, details record, closed set, event, options class or extension class that departs from its shape
- 'Files and folders': more than one type per file or a file name that differs from its type; a block-scoped namespace; an interface outside Application when Infrastructure or Persistence implements it, or not named `I<Name>`; a file outside the folder 'Domain folders' prescribes
- 'Naming': a type whose name does not say its role; a bool member without `Is`, `Has`, `Can` or `Should`; a single-letter or type-in-name identifier; a method name that breaks the `Create`, `From`, `As`, `Get` conventions; `Async`, `CancellationToken` or `sealed` conventions broken
- 'Domain modeling', 'Domain engine' and 'Extending the Domain': a rule not named `Is`/`Has` or without a `Code` constant; a precondition inline in an operation instead of a rule; the Domain reading a clock outside `Ids`; a child entity changed from outside its aggregate; a state change outside a transition; a layer referencing a higher layer
- 'Tests': a test class not mirroring its source folder; a name that is not `<Subject>_<Condition>_<Result>`; a test with more than one act, an act that is not one statement, the input built inside the act block, a result not named `outcome`, `result` or `act`; a test asserting a message instead of a code; a missing or misplaced `Trait`
- 'No explanatory comments': comments added (exceptions: user-requested, XML docs on public API)
- 'Migrations': a hand-edited migration
- 'Architecture': a type in the wrong project, a project reference pointing outward; state change not a Command, or a read not a Query; Unit of Work bypassed; domain event not raised on a significant transition; missing FluentValidation validator for a command; validation not wired as a MediatR pipeline behavior; AutoMapper profile outside Application; storage or audit reached without `IStorageService` / `IAuditLogService`; audit written straight to the `DbContext` from a handler
- 'Data & naming conventions': entity configuration via data annotations instead of Fluent API; decimal without `HasPrecision(19, 4)`; missing soft-delete or tenant query filter, audit columns, or `RowVer` on an aggregate root; seed data applied at startup; hard-coded GUID in logic; command, query, DTO, or route names off-pattern

**Concrete quality bugs** (reasoned, not pattern-matched; flag only when the problem and the fix are both unambiguous):

- **Data integrity:** a column or key whose nullability breaks a unique index or join; string columns too small for the data written; a field added on one write path (update) but missing on its sibling (create); an enum or status handled in one switch but not its counterpart.
- **Async:** blocking on async (`.Result`, `.Wait()`), `async void`, a missing `await`, a `CancellationToken` not passed through where one is available.
- **EF and lifetime:** `DbContext` captured by a longer-lived service or a singleton; N+1 queries in a loop; tracking queries on pure reads; materializing before filtering.
- **Resource management:** `new HttpClient` per call or in a constructor (use `IHttpClientFactory`); state held in instance fields of a transient service.
- **Error visibility:** `catch` that swallows without a log, rethrow, or result; an operation result written to a field no caller reads.
- **Background jobs:** a Hangfire job that is not idempotent, or whose retry duplicates a side effect.
- **Security:** missing authorization on a state-changing endpoint; identity or role taken from the request or hard-coded instead of the authenticated user; secrets or connection strings in code or config files; user input concatenated into queries.
- **Design quality:** a handler doing more than one job; business logic in a controller, an infrastructure adapter, or a migration instead of the domain or Application; EF or HTTP types leaking into Domain or Application; duplicated logic that belongs in one place; static or global mutable state; magic numbers or strings that name a concept; new logic with no test, or a test that asserts nothing meaningful; a public member nothing uses.
- **Dead logic:** `NotImplementedException` reachable from a public path; leftover TODO, `//test`, hardcoded test IDs, `Console.WriteLine`, `debugger`, `DEV-LOG` lines.
- **Frontend:** an unsubscribed `subscribe` in a component (leak); a hardcoded absolute API URL instead of the configured base; a hidden form field that keeps a stale value on submit; `HttpClient` used outside the service layer; an API call with no loading state or error notification; a role-restricted action shown to all roles (the backend must still enforce it); a reactive-form step that advances without validating; `any` types instead of typed models.

### Confidence filter

Report a finding only when all three hold:

1. The violation is on a new or modified line.
2. The rule is explicit in CLAUDE.md, or it is a concrete, unambiguous quality bug from the list above (not a style preference).
3. The fix is unambiguous.

Skip a finding if the pattern exists unchanged in surrounding code, the context is genuinely ambiguous, or it is a style preference rather than a defect.

-

## Output

Per CLAUDE.md 'Output discipline': only the findings. Do not list categories that passed and do not write "X - none"; a clean file or category produces zero lines. If everything is clean: one short sentence, e.g. `changes are clean`.

Contract section first (only if both sides changed), then per-file findings grouped by file. Each finding is its own paragraph with a blank line between findings; inside a finding, `Fix:` (and any other label) starts on a new line. Repo-relative paths, no bold, no dividers, no per-file sub-headers:

```text
contract - <feature>
connection: <one sentence on how the sides connect in this change>

<file>:<line> - <what is mismatched or missing>.
Fix: <fix>.

<file path>:<line> - <rule label>: <specific problem>.
Fix: <one-line fix>.

<next finding>
```

A clean contract section reads `no contract issues` under the connection line. Contract findings do not replace per-file findings.

Example:

```text
contract - claim submission
connection: Angular claim.service posts to ClaimsController.Submit with the SubmitClaimCommand shape

frontend/src/app/claims/claim.service.ts:31 - route mismatch: posts to /api/claim, controller route is /api/claims.
Fix: change the path to /api/claims.

backend/ClaimsModule.Application/Claims/SubmitClaimCommandHandler.cs:44 - complexity > 4: Handle has 6 branches.
Fix: extract status mapping into a private static method.

backend/ClaimsModule.API/Controllers/ClaimsController.cs:58 - result abstraction: builds error JSON by hand.
Fix: return the mapped Result.
```

-

## Handoff

After the findings are printed, the main thread invokes the spec-check skill in passive mode with the changed-file list from Collect. Its output (a `rules` section, a one-line `rules: ... covered`, or nothing) follows the findings with no extra framing. Skip the handoff only when no file in scope is backend, domain-relevant frontend, or a test.

-

## Guardrails

- Report-only: no file edits, no git writes, no checkout.
- Never fix a finding unless the user asks in a separate request.
- Confidential document text stays out of findings (CLAUDE.md 'Domain sources').
- Approval-first actions follow CLAUDE.md 'Permissions model'.
