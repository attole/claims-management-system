# CLAUDE.md

Guidance for Claude Code when working in this repository. Self-contained: it does not depend on any user-level configuration. Skills cite its sections by name instead of restating them.

## Project

End-to-end vertical slice of a claims management system for a PAS (policy administration system). Monorepo: `backend/` (.NET) and `frontend/` (Angular), one root `README.md`, docs in `docs/` (`ARCHITECTURE.md`, `AI-WORKFLOW.md`).

### Stack

- Backend: .NET 9 / C# 13, ASP.NET Core Web API, MediatR 12+ (CQRS), EF Core 9, FluentValidation, AutoMapper, Hangfire, SQL Server 2022 (or Azure SQL), System.Text.Json / Newtonsoft.Json, Azure Blob Storage.
- Frontend: Angular 18+, Angular Material (or equivalent), Reactive Forms, state management by choice (NgRx, signals, services). All HTTP goes through a typed service layer, never directly from components.
- Deployment: Azure, hosted on App Service or Container Apps, with a basic CI/CD pipeline in GitHub Actions or Azure DevOps.

### Architecture

Clean Architecture. The backend solution has exactly these projects:

- `ClaimsModule.Domain` - entities, enumerations, value objects, domain events
- `ClaimsModule.Application` - commands and queries with their handlers, validators, DTOs, AutoMapper profiles, interfaces
- `ClaimsModule.Infrastructure` - Hangfire job definitions and adapters for outside services (Azure Blob, email)
- `ClaimsModule.Persistence` - EF Core DbContext, repositories, Unit of Work, migrations
- `ClaimsModule.API` - controllers, middleware, startup and configuration

CQRS through MediatR: state changes are Commands, reads are Queries. Significant state transitions raise domain events. A Unit of Work coordinates persistence. `docs/ARCHITECTURE.md` is the final artifact: it starts as the original architecture plan and is updated as decisions change.

### Domain sources

Business rules come from the user and from two local, gitignored markdown documents in `docs/`: `spec.local.md` (functional specification) and `assessment.local.md` (technical assessment brief). Read them for domain detail.

- They are confidential. Never paste or closely reproduce their text in tracked files (code, docs, temp notes, commit messages); paraphrase.
- Cite a rule by source-qualified ID: `S:BR-R-03` (spec) or `A:BR-R-02` (assessment). Bare IDs are ambiguous because the two documents number rules independently.
- Tie code to rules with the ID on code, not comments: `[Trait("Rule", "S:BR-R-03")]` on tests, `.WithErrorCode("S:BR-R-03")` on validators.
- `docs/rules.local.md` (gitignored) is the compact rule register, built and refreshed by the spec-check skill. Prefer it over re-reading the full documents.
- Where the documents conflict, do not resolve it silently; surface it per 'Conflict, uncertainty & sources of truth'.

### Local-only files

These files are gitignored (`*.local.md`), so a fresh clone does not have them:

- `docs/spec.local.md` and `docs/assessment.local.md` - the task documents ('Domain sources')
- `docs/rules.local.md` - the rule register; rebuilt from the two documents with "spec-check build"
- `.claude/temp/notes-recruitment-context.local.md` - the assessment and grading context

At the start of a session, before the first answer, check that all four exist. If any is missing, begin the first reply with exactly this one line, then continue with the normal reply:

```text
local-only files missing: <comma-separated paths> (gitignored, not part of a clone)
```

If all are present, say nothing. Never repeat the line later in the session. Without the documents and the register, ask the user for business rules (CLAUDE.md 'Conflict, uncertainty & sources of truth'); spec-check does nothing without the register.

### Commands

```bash
dotnet build backend
dotnet test backend
npm --prefix frontend run build
npm --prefix frontend test
python -I .claude/tools/check_workflow.py
```

The workflow check is read-only, prints one line per finding and a summary line, and must report 0 findings. 'Workflow check' says when it runs automatically.

-

## Workflow & Permissions

### File edits & action gating

- Do NOT create or edit files unless explicitly asked. For analysis, planning, or explanation, provide text output only. Skill-triggered writes (e.g. dev-log invoked by its trigger phrase) are authorized by that phrase; parking-lot appends are pre-authorized (see 'Temp notes').
- **Ambiguous or implicit action triggers: ask, don't execute.** Phrases like "let's start implementing", "let's go", "execute phase 1", or any phrasing that could read as a broad action signal are NOT coding instructions. Phase labels are the user's internal tracking, not commands. When a message could mean "do this now" but doesn't say so explicitly, default to chat and ask what specifically to do before writing anything.
- Before changing multiple files, outline the plan first: which files, what changes in each, the order of operations. Wait for approval before executing. An explicit instruction to implement an approved pack counts as that approval; no second outline.
- **An approved plan is NOT permission to write code.** Approving a plan or design (including via plan mode) authorizes the plan, not implementation. Implementation needs a separate, explicit instruction, and even then proceed one pack at a time: make the pack's changes, stop, report, wait for "continue" ("continue" after a pack report means the next pack; otherwise ask). Never one-shot multiple packs across a plan in a single turn.
- **Pack:** one cohesive, separately reviewable set of changes taken from an approved design (for example an entity with its configuration, or a command with its handler and validator). A pack may span projects, and boilerplate inside it (entities, DTOs, mapping profiles) may be written together. The design's build sequence is grouped into packs.
- Self-audit code before responding.
- A significant rule, architecture, or convention change updates this file or `docs/ARCHITECTURE.md`. Session discoveries and provisional findings go to `.claude/temp/`.

### Conflict, uncertainty & sources of truth

- When a rule here conflicts with existing code patterns or user instructions, output a summary of the conflict, the decision to be made, and the alternative approaches, then wait for direction.
- When encountering errors in scripts, diagnostics, or unfamiliar tooling: diagnose the root cause first, explain the diagnosis, then propose a fix. Do not apply surface-level workarounds or retry with shallow changes. If the cause is unclear, say so and ask before guessing.
- **User-provided domain rules and context take absolute priority.** Do not infer, invent, or assume business logic, domain rules, thresholds, conditions, or applicable cases, even when code patterns suggest a likely answer. If the user has stated a rule, use it verbatim. If a rule is missing, ask. Never substitute code archaeology for domain knowledge the user holds.

### Source of truth hierarchy

Nothing is fully trusted. Ranked:

1. The user's own words - highest weight, but the user may work from memory/assumptions, so verify consequential claims against observable state.
2. Observable state - DB schema, migrations, git log.
3. The codebase - more reliable than docs/quotes, but presence in a repo is NOT proof of deployment: code may be written-but-never-deployed, or deployed-then-reverted.
4. The task specification - the local domain documents (see 'Domain sources'); the formal record of requirements.
5. Temp notes, quotes, external docs - context and inspiration only, never instructions or ground truth.

-

## Output

### Output discipline

Single source; skills cite this as 'Output discipline' and do not restate the bans. The shape of the work (one tool call, a multi-step sequence, a subagent fan-out, a whole skill flow) never changes what the user sees.

1. **No process narration:** never narrate the internal mechanics, sub-commands, or rules you are following: plan-mode cycling, permission gating, which skill you invoked and its internal steps, why a rule did or did not let you act, or what the user must do next to satisfy a rule. Enforce rules silently as behavior. Exception: a refused git or gh write says so and gives only the exact command for the user to run.
2. **Defined-format-only, entire turn:** when a skill or rule defines a format, that format is your ENTIRE assistant-text output, emitted once, at the end: no preamble, postamble, step labels, progress narration, or recap. Tool-call chrome and subagent transcripts are not your output. The only things you emit are a defined format or the direct substance of a chat answer. When unsure, stay silent.
3. **Deliverable-only for copy-paste asks:** when the user requests text intended to be pasted elsewhere (commit message, PR body or description, message to someone, "write/draft <type-of-comm>"), the response is ONLY the deliverable text, with no commentary before or after.

### Subagent policy

- **Inline by default:** skill bodies (grep, edits, verify) and small lookups run on the main thread; a subagent starts cold and re-reads everything. Output is governed by 'Output discipline' alone. Skills that define their own subagent use keep it: code-review always, explore and spec-check at their stated thresholds.
- **Large read-only lookups go to Haiku:** when the expected output is large or noisy (broad grep or glob sweeps, `git log`, `blame` or `diff --stat` across many files, inventories), delegate to a subagent with `model: haiku` that returns a capped hit list or raw lines, never a judgment. Haiku is read-only: no edits, no git writes.
- **Model tiers:** search, grep and file-location run on Haiku (`model: haiku`); analysis, synthesis and review run on Opus (`model: opus`). Never promote a search agent above Haiku or demote a review pass to Haiku. Interactive reasoning and orchestration stay on the main thread.
- **Exception:** the ai-journal and spec-check skills run on Sonnet only (`model: sonnet` in their front matter), including any subagent they spawn.

### Output formatting

Single source of truth for formatting in generated output, skills, and notes.

- ASCII only. No Unicode em dash or en dash, no double-hyphen (`--`). Use a single hyphen (`-`), comma, or period. Straight quotes, not curly.
- Arrows: `->` is allowed inline for flows and status transitions (`Pending -> Approved`). Never Unicode arrow characters. In a structured flow block use no arrows at all: top-to-bottom position implies order.
- Section separators: a single `-` line between sections, in this file and all SKILL.md files.
- Headings: a blank line after every heading.

### Status notation

Git-diff-style prefixes in all structured output: plans, designs, annotated file lists, candidate lists, logs.

```text
[+] new       - file or entity does not exist yet; must be created
[~] modify    - file exists; one or more changes required
[-] delete    - file or entity removed
[=] untouched - referenced in the flow but receives no changes
[?] undecided - blocked on an unresolved decision
```

- **Prefix placement:** the prefix goes on the filename only, never on sub-items. Sub-items use a plain `- item` list indented 4 spaces. The file's prefix covers everything inside: [+] = all new, [-] = deleted (no sub-items needed), [~] = modified (sub-items describe what changed). Prefix at column 0.
- **[=] items:** entity name and fields only, nothing else. The bracket is the complete statement.
- **[?] undecided blocks:** `[?]` as prefix, area name in its own brackets, no status text in the name, no file paths inside. Each option gets a labeled paragraph per 'Argued options'. Blank line between options; no blank line before the recommendation.
- **Flow order:** list elements top-to-bottom in execution order, without arrows (see 'Output formatting'). Position implies "called by the element above".
- **Blank lines:** only between option paragraphs in [?] blocks. Never between consecutive elements in a flow block or sub-items in a file block.

```text
[?] [file storage abstraction]
option A: prose including pros, cons, pitfalls, fit...

option B: prose including pros, cons, pitfalls, fit...
recommendation: option A - reason.
```

### Argued options

When two or more approaches exist for any problem (architecture, data design, integration path, bug fix strategy), do not output a flat list. For each option:

1. State what it does concretely.
2. Name specific pros, with examples or numbers where possible.
3. Name specific cons.
4. Call out non-obvious pitfalls, especially silent failure modes, coupling risks, and maintenance traps.
5. Note whether it aligns with or diverges from existing patterns in this repo (name the analogous file if one exists).

End with a clear recommendation and the primary reason it wins. If options are genuinely tied, say so and name the deciding factor. This applies to all analysis, planning, and architecture output, and to implicit choices too: when a request has more than one reasonable implementation, even if the user did not frame it as a choice, propose a few argued options with a recommendation and wait for the user's pick instead of choosing silently.

-

## Coding Standards

These are written for the backend (C#). In the frontend apply the same intent (naming, small functions, braces, no explanatory comments) with TypeScript and Angular idioms. No `any`; use typed models.

### Newest syntax & APIs

Use the latest language features and framework APIs available for the target (C# 13: collection expressions, pattern matching, `required` members, primary constructors). Compare your approach to the closest existing pattern per 'Argued options'.

### No null

Return empty values instead of null: `[]`, `string.Empty`, or an empty object / `Option`-style value for "nothing" returns; initialize fields with empty defaults, not `?` types.

- Domain and Application: required.
- Everywhere else: preferred as a safeguard. Any null that must exist at a boundary (EF nullable columns, external input) is converted to an empty value at that boundary.

### Extract logic into `private static` methods

Methods that read no instance fields and have no side effects must be `static`.

### Cyclomatic complexity <= 4 per method

No method may exceed a cyclomatic complexity of 4. Extract sub-methods, use switch expressions, or restructure.

### API results go through one result abstraction

Every API operation returns through a single result abstraction (`Result<T>` or equivalent) that the API layer maps to its wire format. `ProblemDetails` or a custom envelope with a status and an error message are both valid implementations of that mapping. Controllers never hand-build error JSON, and the Application layer never returns HTTP types.

### Syntax hygiene

- Always use braces `{}` for `if`, `else`, `for`, `foreach`, `while`, even single-line bodies.
- Use `string.Equals` / `string.Compare` with `StringComparison` instead of `==` for string comparisons where culture or case sensitivity matters.
- Use `.Equals()` instead of `==` when comparing non-primitive reference types.
- Prefer explicit, readable expressions over clever one-liners; name intermediate variables if it aids clarity.

### Code style

- **Line length:** 120 characters maximum, backend and frontend.
- **LINQ:** method syntax only. From the second chained call on, or when the line would pass the limit, put each call on its own line starting with `.`, indented one level.
- **Parameters:** with 4 or more parameters, or when the line would pass the limit, put one parameter per line, indented one level. The same applies to call arguments and constructor injection.
- **Files:** one type per file, and the file name equals the type name. Use file-scoped namespaces.
- **Folders:** Application is organized by feature (for example `Claims/CreateClaim/` holds the command, handler, and validator), so a pack's files sit together.
- **Interfaces:** named `I<Name>`, one per file. Interfaces that Infrastructure or Persistence implement (`IStorageService`, `IAuditLogService`, `IUnitOfWork`, repositories) are defined in Application. Domain defines none unless a domain concept needs one. Each implementing project registers its services in its own DI extension (`AddInfrastructure`, `AddPersistence`).
- **Declarations:** `_camelCase` private fields; `Async` suffix on async methods (not on `Handle` or controller actions); `CancellationToken` as the last parameter; `sealed` classes by default; records for commands, queries, and DTOs; primary constructors for dependency injection; `var` only when the type is evident from the right side; no `#region`.
- **Frontend:** Prettier formatting with `printWidth` 120, `.prettierrc` added when the Angular app is scaffolded.
- **Enforcement:** `.editorconfig` carries the mechanical rules (namespaces, braces, `var`, naming, line length). LINQ and parameter layout are checked in code review.

### No explanatory comments

Do not add comments to code. Code must be self-documenting: favor clear names and small methods. Durable "why" knowledge goes in `docs/ARCHITECTURE.md`, not inline.

Exceptions - comments are allowed only when:

- The user explicitly asks for one.
- It is an XML doc comment on public API surface.

Temporary dev logs are not comments: their `DEV-LOG` marker sits inside the log message string (dev-log skill).

### Migrations

EF Core migrations are generated by tooling and never hand-edited.

### Data & naming conventions

- Primary keys: sequential GUIDs. Money: `DECIMAL(19,4)`, never float or `money`. Timestamps: `DATETIMEOFFSET(7)`, stored in UTC.
- Every business table carries soft-delete (`IsDeleted`, `DeletedAt`; append-only logs such as the audit log are the exception), audit columns (`CreatedAt`, `UpdatedAt`, `UserCreated`, `UserModified`), and a tenant column (name not yet decided; see S:DATA-Tenant and A:DATA-Tenant). Aggregate roots carry `RowVer` (`ROWVERSION`).
- EF configuration: Fluent API via `IEntityTypeConfiguration<T>`, no data annotations for schema; global query filters for soft delete and tenant; `HasPrecision(19, 4)` on decimals.
- Seed data through `HasData` or migration scripts, never at application startup. No hard-coded GUIDs in application logic.
- Names: tables and entities PascalCase singular; commands `VerbNounCommand`; queries `GetNounQuery` / `ListNounsQuery`; DTOs `...Dto` or `...Request` / `...Response`; routes kebab-case plural.
- Cross-cutting: FluentValidation runs as a MediatR pipeline behavior; AutoMapper profiles live in Application; storage and audit sit behind interfaces (`IStorageService`, `IAuditLogService`); audit writes never go straight to the `DbContext` from handlers.

-

## Tooling, Skills & Temp

### Shell & git

- **Git writes belong to the user.** Never run `git add`, `git commit`, `git push`, `git reset`, `git checkout`, `git restore`, `git rebase`, `git merge`, `git stash` (other than `list` and `show`), `git tag`, `git rm`, `git mv`, `git pull`, `git cherry-pick`, `git revert`, or `git am`; `.claude/settings.json` blocks them in every mode. When one is needed, say so and give the exact command for the user to run. Also never run `git clean`, `git branch -D`, or `git switch` with a force flag (`-f`, `--force`, `--discard-changes`, `-C`, `--force-create`). Read-only git (`diff`, `log`, `show`, `status`, `blame`, `ls-files`, `branch --list`) and read-only search (`grep`, Grep, Glob) run freely, and so does plain `git switch` to change branch.
- **`gh` writes need explicit instruction** (creating or merging PRs, issues, releases, any non-GET `gh api`). Named read-only `gh` subcommands (`pr view/list/diff/checks/status`, `issue view/list`, `run view/list`, `repo view`, `auth status`) run freely; every other `gh` command, including all `gh api`, is ask-first (see 'Permissions model').
- **Never chain `cd && ...`.** Run git plainly from the repo root (the session's working directory), with the subcommand first. `.claude/settings.json` blocks global options before the subcommand (`git -C`, `-c`, `--no-pager`), shell wrappers (`bash -c`, `sh -c`, `cmd /c`, `powershell`, `pwsh`), full paths to git, and the PowerShell tool. Use explicit path arguments for everything else.
- **PowerShell is unavailable in this project** (the tool is denied). Give the user any PowerShell-only command to run.
- Prefer the dedicated file and search tools over shell equivalents.

### Permissions model

The user runs Claude Code with permissions bypassed. Approvals therefore work in two layers:

- **Hard layer:** the deny list in `.claude/settings.json` blocks commands (git writes, `git clean`, forced branch operations) in every mode. Deny rules match the command text only, so a script or `python -c` that runs git is not caught; the rule layer covers that. Its allow and ask entries only apply when a session is not in bypass mode.
- **Rule layer:** everything else that needs approval is a rule in this file or in a skill, enforced by Claude. Ask in chat first and wait for an explicit yes.

Ask-first actions, every time, even if a similar one was approved earlier:

- `dotnet ef migrations add` and `dotnet ef database update`
- any `az` command, and any deploy or publish step
- any `gh` command other than the named read-only subcommands, including all `gh api`
- deleting or overwriting files outside the current task
- any command with effects outside the repository (network writes, cloud resources)

### Workflow check

Run `python -I .claude/tools/check_workflow.py` automatically and silently at these points, and speak up only when it reports findings:

- after Claude edits this file, a skill, `settings.json`, a temp note, the rule register, or `docs/AI-WORKFLOW.md`;
- after an ai-journal entry, a parking-lot append, and "spec-check build";
- when the user says they are about to commit or asks whether the repo is ready.

Fix findings in files Claude touched before continuing; for findings in other files, report them and ask. A clean run says nothing. If the script reports skipped checks (local-only files missing), mention it once per session.

### Skill triggers

Skills live in `.claude/skills/<name>/SKILL.md`.

- **explore:** "explore X", "find X" (X is code, a location, or behavior), "investigate X", "trace X", "how does X work", "where is X", "look up X", or a claim to verify against the code (a claim naming a rule ID goes to spec-check instead). Invoke the skill before investigating directly. Checking ONE already-named fact (a single symbol's location, one file's content) is a direct lookup, not an investigation.
- **design:** "design X", "how should we build X". Produces a design; never writes code.
- **code-review:** "code review", "review the code", "review the changes", "review the branch", "check my changes" (a code object is required; "review the design" is not a code review). Report-only; fixing a finding is a separate, explicit request.
- **spec-check:** "spec-check", "check rules", "rule coverage", "check <rule ID>", "spec-check build". Also runs passively after every code-review. Read-only except build.
- **ai-journal:** "log prompt", "log correction", "log split", "log assessment", "log session", "journal this", "list journal", "compile journal". Records real events only; proactive offers always ask first.
- **dev-log:** user-invoked only, as `/dev-log on`, `/dev-log check`, or `/dev-log off` (the skill is not model-invoked). If asked for dev logs in plain words, reply with the command instead of adding logs by hand. Never leave an untagged dev log in code.

### Temp notes

`.claude/temp/` holds working notes, committed unless the name ends in `.local.md`: the holding pen between a hypothesis and a verified fact. Notes are created only on explicit request (or by a skill's explicit notes step). They never contain secrets, tokens, `.env` content, or raw command output.

- **Naming:** `<kind>-<topic>.md`, lowercase kebab-case, no date (git history carries dates). Kinds: `design`, `explore`, `review`, `notes`, `plan`. Topic is the noun phrase of the request in kebab-case (for example `claim-document-upload`), reused exactly across files (e.g. `design-claim-submission.md`, `explore-claim-lifecycle.md`).
- **Local-only notes:** a `.local.md` suffix (for example `notes-recruitment-context.local.md`) marks a note that is gitignored (`*.local.md`) and never committed; other clones do not have it.
- **First line:** `status: active` or `status: done`. Set `done` when the topic is finished.
- **Sections:** `OPEN` (unanswered questions), `PROVISIONAL` (decisions or findings not yet approved or verified), `NEXT` (next steps). Prune resolved lines.
- **Extra sections by kind:** design files add `DECISIONS` (settled choices with the reason) and `ANALOGUE` (verified existing code or docs that the design follows, with file:line); plan files add `PHASES`.
- **Own formats:** the journal (`notes-ai-journal.md`) defines its own FORMAT and ENTRIES blocks and is append-only; the parking lot (`notes-parking-lot.md`) is a flat list of tagged one-line items (`TOPIC:`, `NEXT:`, `PROPOSAL:`).
- **Parking lot:** derived topics, next steps, and proposals that arise outside the current focus go to `.claude/temp/notes-parking-lot.md`. When one arises, append a single tagged line (uppercase tag, colon, space: `TOPIC: `, `NEXT: `, or `PROPOSAL: `) with `printf '%s\n' 'TAG: text' >> .claude/temp/notes-parking-lot.md` (single quotes, no read; pre-authorized), and continue the current task without discussing it. Do not read the parking lot unless the user asks ("open parking lot"); then show the non-empty tag groups. When an item is taken up, move it into an existing note (or ask first) and delete its line with an exact-match delete.
- **General plan and context:** `.claude/temp/plan-ai-workflow.md` is the general working plan (phases, flow, which skill is used where); read it at the start of a phase. `.claude/temp/notes-recruitment-context.local.md` (local-only, gitignored) holds the assessment and grading context, kept out of this file on purpose.
- **Promotion:** a verified fact or settled decision graduates to `docs/ARCHITECTURE.md` (or this file, if it is a rule) and is removed from the note.
- **Context hygiene:** prefer `/clear` at step boundaries (investigate, `/clear`, implement, `/clear`, review), paging the relevant note back into context; use `/compact` only within a step that has grown long.
