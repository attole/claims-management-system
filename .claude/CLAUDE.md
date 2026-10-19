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

- `ClaimsModule.Domain` - aggregates, entities, closed sets, value objects, domain events, and the generic engine (`Core`) that runs rules, operations and transitions
- `ClaimsModule.Application` - commands and queries with their handlers, validators, DTOs, AutoMapper profiles, interfaces
- `ClaimsModule.Infrastructure` - Hangfire job definitions and adapters for outside services (Azure Blob, email)
- `ClaimsModule.Persistence` - EF Core DbContext, repositories, Unit of Work, migrations
- `ClaimsModule.API` - controllers, middleware, startup and configuration

CQRS through MediatR: state changes are Commands, reads are Queries. Significant state transitions raise domain events. A Unit of Work coordinates persistence. `docs/ARCHITECTURE.md` is the final artifact: it starts as the original architecture plan and is updated as decisions change; it holds the reasons, this file holds the rules.

### Domain sources

Business rules come from the user and from two local, gitignored markdown documents in `docs/`: `spec.local.md` (functional specification) and `assessment.local.md` (technical assessment brief). Read them for domain detail.

- They are confidential. Never paste or closely reproduce their text in tracked files (code, docs, temp notes, commit messages); paraphrase. One narrow exception: the spec's user-facing validation messages (`S:V-Messages`) are used verbatim as the message strings in Domain rule classes and in `Transition` (the not-permitted message), because the spec requires those exact messages. They appear nowhere else (not in docs, temp notes, test descriptions, or commit messages).
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

### Configuration

The backend needs a .NET 9 runtime and a local SQL Server (LocalDB). Required settings are validated at startup through the options pattern (`ValidateOnStart`) and a missing one stops the app with the key named.

- Secrets live in user-secrets locally (`Jwt:SigningKey`, 32+ characters) and in app settings or Key Vault when deployed. Never put a secret in a committed file.
- Feature switches (`Swagger:Enabled`, `Hangfire:Dashboard:Enabled`, `DevAuth:Enabled`) are booleans that default to false and never depend on the environment.
- New settings follow the same pattern: an options class with an explicit default, bound with `BindConfiguration` in the owning layer's per-feature extension.
- Full setup steps and the settings table are in the root `README.md`; the short version is in `docs/ARCHITECTURE.md`.

-

## Workflow & Permissions

### File edits & action gating

- Do NOT create or edit files unless explicitly asked. For analysis, planning, or explanation, provide text output only. Skill-triggered writes (e.g. dev-log invoked by its trigger phrase) are authorized by that phrase; parking-lot appends are pre-authorized (see 'Temp notes').
- **Ambiguous or implicit action triggers: ask, don't execute.** Phrases like "let's start implementing", "let's go", "execute phase 1", or any phrasing that could read as a broad action signal are NOT coding instructions. Phase labels are the user's internal tracking, not commands. When a message could mean "do this now" but doesn't say so explicitly, default to chat and ask what specifically to do before writing anything.
- Before changing multiple files, outline the plan first: which files, what changes in each, the order of operations. Wait for approval before executing. An explicit instruction to implement an approved pack counts as that approval; no second outline.
- **An approved plan is NOT permission to write code.** Approving a plan or design (including via plan mode) authorizes the plan, not implementation. Implementation needs a separate, explicit instruction, and even then proceed one pack at a time: make the pack's changes, stop, report, wait for "continue" ("continue" after a pack report means the next pack; otherwise ask). Never one-shot multiple packs across a plan in a single turn.
- **Pack:** one cohesive, separately reviewable set of changes taken from an approved design (for example an entity with its configuration, or a command with its handler and validator). A pack may span projects, and boilerplate inside it (entities, DTOs, mapping profiles) may be written together. The design's build sequence is grouped into packs.
- **A pack is done when:** `dotnet build backend` reports 0 warnings and 0 errors, `dotnet test backend` passes, the workflow check reports 0 findings, no code line is over the limit in 'Code style', no file has an unused using, and the new tests mirror the folders of the code they cover. Audit the code against these standards before reporting; the report names the files changed, the test count and any open point, then stops.
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
- Markdown (docs, notes, skills, this file) has no line limit: one paragraph or list item per line, no hard wraps. Code blocks and tables are exempt.

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
- **[?] undecided blocks:** `[?]` as prefix, area name in its own brackets, no status text in the name, no file paths inside. The header sits alone on its own line, followed by a blank line. Each option gets a labeled paragraph per 'Argued options', with a blank line between options. The label (`option A (short name):`) is a line of its own and the option text starts on the next line. The recommendation is its own paragraph, one blank line after the last option.
- **Flow order:** list elements top-to-bottom in execution order, without arrows (see 'Output formatting'). Position implies "called by the element above".
- **Blank lines:** in [?] blocks, after the header, between options, and before the recommendation. Never between consecutive elements in a flow block or sub-items in a file block.

```text
[?] [file storage abstraction]

option A (short name):
prose including pros, cons, pitfalls, fit...

option B (short name):
prose including pros, cons, pitfalls, fit...

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

- Spell the empty value out even when it equals the CLR default: `= Guid.Empty`, `= false`, `= EstimatedLossAmount.Empty`, `= string.Empty`. Properties assigned in every constructor need no initializer.
- Domain and Application: required.
- Everywhere else: preferred as a safeguard. Any null that must exist at a boundary (EF nullable columns, external input) is converted to an empty value at that boundary.

### Extract logic into `private static` methods

Write a method as one readable flow: a few named variables (one per query or step), then the steps in order. Do not split it into one helper per query or per condition. Extract a helper only when the logic is used in more than one place, when it is a separate concept that deserves its own name, or when the method would pass the complexity limit and cannot be simplified first. A method that reads no instance fields and has no side effects must be `static`.

### Cyclomatic complexity <= 4 per method

No method may exceed a cyclomatic complexity of 4. Simplify before extracting: chain filters in separate `Where` calls instead of `&&` or `||` in one lambda, use set or dictionary lookups, a switch expression or a pattern. Extract a sub-method only when that is not enough.

### API results go through one result abstraction

Every API operation returns through a single result abstraction (`Result<T>` or equivalent) that the API layer maps to its wire format. `ProblemDetails` or a custom envelope with a status and an error message are both valid implementations of that mapping. Controllers never hand-build error JSON, and the Application layer never returns HTTP types.

### Syntax hygiene

- Always use braces `{}` for `if`, `else`, `for`, `foreach`, `while`, even single-line bodies.
- Use `string.Equals` / `string.Compare` with `StringComparison` instead of `==` for string comparisons where culture or case sensitivity matters.
- Use `.Equals()` instead of `==` when comparing non-primitive reference types.
- Methods have block bodies; only read-only properties may be expression-bodied.
- Prefer explicit, readable expressions over clever one-liners. Put the result of a LINQ chain or any non-trivial expression into a named variable first, then use that variable in the `?:`, the `if` or the `return` (`bool isLastClaimant = ...;` then `return isLastClaimant ? ... : ...;`). A named result reads better and can be inspected in the debugger. Keep the count small: name a query or a step, not every sub-expression.
- In the Domain a broken invariant throws a standard exception (`ArgumentException`, `InvalidOperationException`); a business refusal is a finding, never an exception.

### Code style

Layout

- **Line length:** 120 characters maximum, backend and frontend code. Markdown has its own rule ('Output formatting').
- **LINQ:** method syntax only. A chain of two or more LINQ calls, or any line that would pass the limit, puts each call on its own line starting with `.`, indented one level; a single LINQ call after a member or method call stays on its line (`claim.Parties.Select(party => party.Id)`).
- **Parameters and arguments:** keep them on one line when the line fits the line length. When the line would pass the limit, put one item per line, indented one level. This is a length rule only: how many arguments a call has, or whether they are calls themselves, never forces a wrap. The same applies to constructor injection and object initializers. Never nest a wrapped call inside another call: put the inner creation or call into a named variable first (`var input = new AddPartyInput(...);` then `Run(claim, input)`), and keep long literal text out of the call as a constant.
- **Records and constructors:** a positional record keeps its base list on the declaration line when it fits (`public sealed record UpdateNotesInput(...) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);`) and moves it to the next line, indented one level, when it does not. A constructor initializer (`: base(id)`, `: this(...)`) stays on the signature line when the line fits the line length (`public OpenClaimTransition() : base(new ClaimOpenState())`); when it would pass the limit, or the parameters are wrapped, it moves to its own line below the signature, indented one level.
- **Blank lines:** one blank line between members; none after an opening brace or before a closing brace; one before the final `return` unless it is the first statement of its block. Inside a body, a blank line separates the steps of a test (arrange, act, assert) and the computed statement from its `return`.
- **Wrapped lists:** a wrapped list (arguments, initializer, collection expression, enum members) puts one item per line and keeps a trailing comma after the last item where the language allows it.
- **Ternaries:** a ternary that does not fit on one line puts `?` and `:` on their own lines, indented one level; the true branch comes first, and in a check the passing branch is the true branch.
- **Usings:** `System` namespaces first, then the others alphabetically, in one block above the namespace; no unused usings (the build reports them as errors). A file-scoped namespace follows after one blank line.
- **Frontend:** Prettier formatting with `printWidth` set to the line length, `.prettierrc` added when the Angular app is scaffolded.

Declarations

- `var` wherever the right side shows the type: object creation, casts, LINQ results, `foreach` variables, property and indexer reads (`var issue = claim.Issues[0];`), and calls named after what they return (`Claim.Create`, a test helper). Explicit types only for the built-in keyword types (`string`, `int`, `bool`, `decimal`), lambdas and delegates, collection expressions, and a variable declared as a base or interface type on purpose (`PartyType person = new PartyPersonType();`).
- Never `Type name = new(...)`; write `var name = new Type(...)`.
- Collections are `[]` collection expressions; an empty value is spelled out (`string.Empty`, `Guid.Empty`, `DateTimeOffset.MinValue`, `[]`), not `null` ('No null').
- Classes are `sealed` unless built for inheritance; records for commands, queries, DTOs, inputs, events, findings and details; primary constructors for dependency injection and for classes whose only constructor parameters are used as fields.
- Constants over `static readonly` for compile-time values; a private constant is still PascalCase.
- Members are ordered: constants, fields, constructors (a private parameterless one for EF first), properties (identity and data, then collections, then computed properties, then the capability and audit properties last), public methods, internal methods, protected methods, private methods. Within a group a static factory comes first. In a hook-based class the properties that supply decisions come before the methods.
- No `#region`, no commented-out code, no TODO markers.

Enforcement

- `.editorconfig` carries the mechanical rules (namespaces, braces, `var`, naming, line length).
- `python -I .claude/tools/check_workflow.py` (which reads the line length from here) checks, for every backend project, line length, comments, namespaces, using order, the blank line before `return`, `Is`/`Has`/`Can`/`Should` on bool members, `I<Name>` interfaces, the `Is`/`Has` name and `Code` of rules, the `...Operation` file names, the entity subfolders, the Domain layering, the test mirror and the underscore form of test names.
- Code review checks the rest: LINQ layout, parameter layout and count, named intermediate variables, explicit empty defaults, member order and the code shapes.

### Code shapes

Each kind of class has one shape, so the next one is written the same way.

- **Rule:** an `internal` class with constants `Code` (public), `Message` and `Field` (private); one `Check` method that names the statement in a `bool` that is true when the rule passes (`bool hasClaimant = ...;`, `bool isNotLastClaimant = !...;`) and returns `statement ? Pass : Found(finding)`. A rule built from the input takes its value through a primary-constructor parameter, as plain values (`HasNoPendingReserveHistory(bool hasPendingReserveHistory)`), never a whole input or a facts record. The statement reads a fact that lives on its owner ('Domain modeling'), so the rule only maps the fact to its code and message. A rule that does not read its subject checks `object` (`IRule<object>`), so it serves any step, a creation's `NoSubject` included; a rule about a concept the aggregates share lives with that concept, never in `Core` (`Roles/Rules/HasMinimumRole`). A combined rule (`HasValidLossDetails`, `IsLossCompleteAfterDraft`) runs other rules over one part of an aggregate and returns their findings: it keeps a `Code` that names it, but has no `Message`, `Field` or finding of its own.
- **Operation:** an `internal` class in `Flows/` naming itself as the first type argument of its base (`Operation<AddPartyOperation, Claim, AddPartyInput, ClaimParty>`); its input record is a type of its own in `Inputs/` (public for an aggregate method, internal for a child's method). The constructor only calls `Require`; the class has no fields, because the engine keeps one shared instance per step and runs it through the static `Run`. `Apply` changes the subject through its `internal` members and returns `Done(...)` with the child it added or the root itself when it changed one of its own fields, or, for a change to an existing child, the outcome of that child's own method; it holds no condition, does not touch the root and raises no event.
- **Evaluation over a whole entity:** an `internal` operation in `Flows/` whose only required rule is that the claim is editable (a frozen claim keeps the issues its closure was judged on), its input in `Inputs/`; `Apply` checks the evaluated rules in one private static method (`CheckResult.Combine`), builds a few named queries over the findings and then loops over them (`EvaluateIssuesOperation`). The issue rules are not required, because a required Critical rule would stop the step.
- **Transition:** an `internal` class in `Flows/` deriving from the aggregate's transition base with itself as the first type argument (`ClaimBaseTransition<OpenClaimTransition, ClaimTransitionInput>`); its input is the aggregate's transition input, or a record in `Inputs/` deriving from it when the move needs more (`FreezeClaimInput`). The constructor names the target and declares each `AllowedFrom(...)` with its rules. The engine seals `Check` and `Apply` and returns the `Move` (from, to); the aggregate's base seals `ApplyMove`, which sets the state (and on the claim appends the history row). Nothing is overridden.
- **Entity (child):** constructors (private for EF, then `internal` with `id`, parent id, organisation id and its data, a details record where the entity has editable data), properties, then `internal` methods, one per change, each running the child's own step on itself (`party.Remove()`, `riskObject.Remove(input)`) and raising the child's events in its follow-ups; the fields a step sets have `internal` setters. Audit columns are not Domain state: Persistence stamps them.
- **Aggregate root:** fields with `internal` setters and child lists reachable through `internal` accessors, so a step changes them; the root keeps the follow-up helpers as its `private` methods. The issue evaluation returns the issues it raised and cleared (`IssueChanges`): the root raises the raised events, and each cleared issue raises its own event through `issue.Clear()`, like an acknowledged or waived one.
- **Aggregate method:** a `public` method that takes the step's input record only (outside facts the follow-ups need ride on that input, like `PolicyPeriod` and `CauseCodes` on every claim input) and returns one chain: `Step.Run(this, input)`, then one `.OnSuccess(...)` per follow-up of its own aggregate in order (raise the event, record the move, run an inner step), then, on the claim, `.OnSuccess(() => Settle(input))` last (the issue re-evaluation). A claim move also calls `Settle(input);` before the chain, so the transition's rules read issues evaluated with the move's own facts. A follow-up is an inline lambda that constructs its event in one line (`party => Raise(new PartyAddedEvent(Id, party.Id, party.Details.Role.Name))`), and a private method only when it is reused, runs an inner step, or builds an event that does not fit on one line (`Settle`, `RecordMove`, `OpenAfterReopen`, `RecordRaisedIssues`; on the reserve component `RecordSubmission` and `AutoApprove`, on its history row `CancelPosting`, `RecordApproval` and `RecordPosting`). The chain is the return value; it holds no rule and never changes the outcome. A creation method is static, named by the domain verb (`Claim.Create`, `ReserveComponent.Open`), and its hooks take the created aggregate (`claim => claim.Settle(input)`).
- **Details record:** the identifying values positional, the optional ones `{ get; init; }` with an explicit empty default.
- **Closed set:** one file named after the base type holding the marker enum, the abstract record, and the sealed variants with `() : Base(Kind.Value)`; a variant with behavior overrides the base member in a body.
- **Event:** a positional record named `<Thing><PastVerb>Event`, `(<ids and facts>) : DomainEvent`, one parameter per line. An event carries the business facts only; its id, time and actor are an event context the Application adds at dispatch.
- **Options:** a sealed class with a `SectionName` constant and `{ get; init; }` properties with explicit defaults; bound in the owning layer's `<Feature>ServiceExtensions` with `BindConfiguration`, and validated with `ValidateDataAnnotations().ValidateOnStart()` when a missing value must stop the app.
- **Extension class:** `<Feature>ServiceExtensions` holds `Add<Feature>` (and `Use<Feature>`), where the class name and the `Add` method share the feature word (`FrontendCorsServiceExtensions`, `AddFrontendCors`).

### Files and folders

- **Files:** one type per file, and the file name equals the type name. Use file-scoped namespaces; a namespace equals its folder path. Exception: types of one registry, such as a closed set of marker records with their marker enum, share one file named after the base type (`ClaimState.cs` holds the enum, the abstract record and every sealed record). This applies while each variant is empty or only overrides a member of the base (`PartyRole.cs`). A type with real logic or its own state gets its own file. The other exception is an Application use case: one file named after the command (`UpdateNotesCommand.cs`) holds the command record with its `ToInput` method, its validator and its handler, in that order, so one file explains the use case. An input record is the aggregate's public contract, not the step's dependent, so it has its own file in `Inputs/` (`AddPartyInput.cs`) next to the internal step in `Flows/` (`AddPartyOperation.cs`).
- **Interfaces:** one per file. Interfaces that Infrastructure or Persistence implement (`IStorageService`, `IAuditLogService`, `IUnitOfWork`, repositories) are defined in Application. Domain defines interfaces only for its own concepts (`IRule`, markers, capabilities). Each implementing project registers its services in its own DI extension (`AddInfrastructure`, `AddPersistence`).
- **Folders:** PascalCase and plural for a collection of one role (`Flows`, `Inputs`, `Events`, `Rules`, `Parties`, `RiskObjects`). Application is organized by feature (for example `Claims/Commands/CreateClaimCommand.cs` holds the command, its mapping, validator and handler), so a pack's files sit together. A child entity of an aggregate gets its own folder inside the aggregate's, mirroring the Domain (`Claims/Parties/AddPartyCommand.cs`), and that folder holds the entity's DTO, request, validator, mapping profile and its use-case files. The aggregate's own use cases go in a `Commands/` folder (`Claims/Commands/LinkPolicyCommand.cs`) beside its shared types (DTO, mapping profile, errors, field lengths, repository interfaces). A use case never gets a folder of its own. A request or command maps itself to the Domain with a method on its own record (`ToInput`, `ToDetails`), never with a separate extension class. Domain folders are by concept ('Domain folders'); test folders mirror the source folders ('Tests').

### Naming

One standard for every name. The name says the role, and the suffix of a type is its role.

**General**

- English, PascalCase for types, methods, properties, constants and enum members; camelCase for parameters and locals; `_camelCase` for private fields; `I` prefix for interfaces.
- Whole words. The only abbreviations are the common ones (`Id`, `Dto`, `Url`, `Json`) and the names the spec gives (`RowVer`). Write an acronym as a word (`Dto`, not `DTO`). No type in a name (`ClaimList`, `strName`).
- A collection is plural (`Parties`), an item is singular (`party`). A lambda parameter is the singular word of its element (`issue`, `finding`, `party`), or `_` when unused; never a single letter.
- An id ends in `Id` (`ClaimId`), a timestamp in `At` (`UpdatedAt`), a calendar date in `Date` (`ReportedDate`), a user reference as the spec names it (`UserCreated`).
- A name that the spec or the stored schema fixes (audit columns, `IsDeleted`, `DeletedAt`, `RowVer`, `OrganisationId`, `ReportDate`) is kept as written, even where it breaks a rule here.

**Booleans**

- Every `bool` member, method or property, starts with `Is`, `Has`, `Can` or `Should` (`HasCriticalFindings`, `IsSuccess`, `IsClaimant`, `HasPersonName`).
- Exceptions: members that a framework or interface fixes (`Equals`, `Authorize`) and options-class properties bound to a configuration key (`Enabled`).

**Methods**

- A verb or verb phrase for what it does: `AddParty`, `Run`, `EvaluateIssues`. `Async` suffix on async methods (not on `Handle` or controller actions); `CancellationToken` last.
- `Create` makes a new object, `From` parses or converts a value into a validated one (`ClaimNumber.From`), `As<Thing>` returns a converted copy (`AsWarning`), `Get<Thing>` is a query.
- A child entity's mutator is a bare verb (`Remove`); the root's method for it is the verb plus the thing (`RemoveRiskObject`).
- The names the engine fixes are not changed: `Apply`, `Check`, `Require`, `AllowedFrom`.

**Types by role**

| Role | Name | Example |
|---|---|---|
| Rule (an `IRule`) | `Is<Statement>` or `Has<Thing>` | `IsNotLastClaimant`, `HasClaimant` |
| Entity data replaced whole | `<Thing>Details` | `PartyDetails` |
| Input, operation, transition | `<Verb><Thing>` + `Input`, `Operation`, `Transition` | `AddPartyInput`, `AddPartyOperation`, `OpenClaimTransition` |
| Base shared by one scope | `<Scope>Base<Role>` (the claim's transition input is `ClaimTransitionInput`; the reserve history's are `RowDecisionInput` and `RowPostingInput`) | `ClaimBaseOperationInput`, `ClaimBaseTransition`, `ReserveHistoryBaseApprovalTransition` |
| Domain event | `<Thing><PastVerb>Event` | `PartyAddedEvent` |
| Value object | the plain noun | `LossDate` |
| Absent value | static `Empty` and an `IsSet` flag | `LossDate.Empty` |
| Closed set | enum `<Concept>Kind` (the claim lifecycle keeps `ClaimStateKind`), abstract record `<Concept>`, variants `<Owner><Value><Concept>` (the reserve history's sets use `Reserve<Value>State` and `ReservePosting<Value>State`) | `IssueStateKind`, `IssueState`, `IssueRaisedState` |
| Options | `<Name>Options` | `JwtOptions` |
| Command, query, DTO | `VerbNounCommand`, `GetNounQuery` / `ListNounsQuery`, `...Dto` / `...Request` / `...Response` | `CreateClaimCommand` |

- A closed set without behavior is a plain enum (`AssetType`).
- A finding's code says what went wrong, so its name says who reports it, not the problem.
- A constant is PascalCase, private ones too; a rule keeps `Code` public and `Message` and `Field` private.

**Tests**

- A class is `<Type>Tests`; a method is `<Subject>_<Condition>_<Result>`. See 'Tests'.

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
- Every business table carries soft-delete (`IsDeleted`, `DeletedAt`; append-only rows such as the audit log, the state history and reserve history rows are the exception), audit columns (`CreatedAt`, `UpdatedAt`, `UserCreated`, `UserModified`; EF shadow properties stamped by the save interceptor, not Domain properties), and a tenant column (name not yet decided; see S:DATA-Tenant and A:DATA-Tenant). Aggregate roots carry `RowVer` (`ROWVERSION`).
- EF configuration: Fluent API via `IEntityTypeConfiguration<T>`, no data annotations for schema; global query filters for soft delete and tenant; `HasPrecision(19, 4)` on decimals.
- Draft claims may be incomplete: the loss date column is nullable and the cause code has no database foreign key, so Critical validation issues can be stored on a Draft (deviation from the spec table; see `docs/ARCHITECTURE.md` 'Schema departures from the spec').
- Seed data through `HasData` or migration scripts, never at application startup. No hard-coded GUIDs in application logic.
- Names: tables and entities PascalCase singular; routes kebab-case plural; type names follow 'Naming'.
- Cross-cutting: FluentValidation runs as a MediatR pipeline behavior; AutoMapper profiles live in Application; storage and audit sit behind interfaces (`IStorageService`, `IAuditLogService`); audit writes never go straight to the `DbContext` from handlers.

### Tests

- **Mirror:** a use-case file has one test class, `<UseCase>CommandTests`, holding the handler and validator tests (`Handle_...`, `Validate_...`). Otherwise one test class per type under test, named `<Type>Tests` (one class per operation, rule or closed set), in the folder that mirrors the type's folder and with the matching namespace. The only class without a source twin is `EntityConstructionTests`.
- **Fixtures:** they live under `Support/`, mirroring the source folders (`Support/Claims`, `Support/Core`, `Support/Common`, `Support/ReserveComponents`, `Support/ClaimDocuments`), and are used only by tests. Claims are filled through the aggregate methods (`TestClaims`), never through private state; `MoveTo` (a test transition) and `ChangeLoss` (the child's own update) are the two named shortcuts. Sample types are named `Sample<Role>`, or `<Variant>Sample<Role>` for a variant (`SampleOperation`, `FailingSampleOperation`, `CriticalSampleClaimRule`); a fixture constant is `const` or `static readonly` and PascalCase when public, `_camelCase` when private. Setup that two tests share is a private static helper in the class or a fixture in `Support/`, never a field that tests mutate.
- **Name:** `<Subject>_<Condition>_<Result>`, PascalCase parts joined by underscores. The subject is the member under test (`Run`, `Check`, `Combine`) or a noun phrase for the thing (`Roles`, `AFailedPrecondition`); the condition is dropped when there is none (`Run_ReturnsTheValue`); the result is a verb phrase for the expected outcome. Examples: `Run_FromAnUndeclaredSource_ReturnsNotPermittedAndChangesNothing`, `Handle_WhenAValidatorFails_ThrowsAndSkipsTheHandler`.
- **Form:** arrange, act, assert, each block separated from the next by one blank line, no comments.
    - Arrange holds everything the act needs, including the input record.
    - Act is exactly one statement, the call under test. Its result, when asserted, is `var outcome` for an operation or transition and `var result` for anything else; a call that must throw is `Action act` or `Func<T> act` and the assert is `act.Should().Throw<...>()`. A call whose value is not asserted stands alone, and the assert reads the effect (state, events, issues).
    - Assert reads only the result and the arranged objects.
    - A test with nothing to arrange starts at the act. A test about an arranged value (a closed-set table, an equality, a constant, a property) has an arrange block and an assert block and needs no act variable.
- **Each case tested once, where it lives:** a fact's class (the entity, value or closed set that owns it) tests its edge cases (a `[Theory]` when cases differ only in data); a rule's class tests a pass and a fail; a step's class tests each declared source, one undeclared source, one refusal per required rule (proof the step requires it) and what the step changes, never a rule's edge cases again; an aggregate method's class tests only its follow-ups (events, the recorded move, `Settle`, inner steps) and one refusal that runs no follow-up.
- **One behavior per test:** one act and one reason to fail. The same act with different data is a `[Theory]`; two acts that are compared become two tests that share a helper.
- **Attributes:** `[Fact]` or `[Theory]` first, then `[Trait("Rule", ...)]` for each rule the test covers (a document rule ID or a contract code constant), then the `[InlineData]` rows.
- **Asserts:** FluentAssertions chains. A finding is asserted by its code, never by its message text. The class is `public sealed`.
- **Style:** test code follows the same standards as production code.

-

## Domain

### Domain and Application

An aggregate is an entity that owns its state and changes it through its own public methods; a step is how a method runs one change, rules first and refusals returned as findings; the Application supplies, on the input, every fact the aggregate does not hold.

- The Domain holds state (aggregates, children, value records, closed sets), rules (a statement about one subject and the request, a class with a `Code`) and single steps (an operation, a creation or a transition that changes one aggregate in `Apply`).
- The public face of an aggregate is its methods: each is a chain on one step's outcome, with the follow-ups that stay inside the same aggregate and `Settle` last.
- Steps and rules are `internal`; inputs, events, outcomes and findings are the public contract.
- The Domain never loads or saves, reads no clock (except `Ids`, below), knows no identity, reads no other aggregate (a static fact on its type, such as `Claim.IsWithinReserveLimit`, is not a read), returns no HTTP type and never chains across aggregates.
- The Application holds the use case: it loads, builds the input with the outside facts, calls one aggregate method, reads its outcome, saves, dispatches events and maps findings. It holds no statement about state.
- A true-or-false statement about state and request is a rule; a change to one aggregate's state is a step; a second aggregate, a load, a save, a clock, a user identity or a job is Application; a value from outside the aggregate is computed by the Application into a field on the input; an effect that crosses a boundary (audit, a job) is a domain event with an Application handler.

### Domain modeling

- **No invented rules:** a rule that no source states (a bound on an amount, a length) is not added; ask for the source first. Every rule exposes its code as a `Code` constant: the source-qualified rule ID when a document states the rule (`S:BR-C-05`), or a plain upper-snake code for a rule that guards the system's own contract (`CLAIM_NUMBER_REQUIRED`); the constants are the only list, and rules that enforce one source rule share its code. The class keeps `Message` and `Field` as private constants, so the failing line reads `CheckResult.Found(new Finding(Code, Message, Field))`.
- **Facts on their lowest owner:** a fact about one entity, value or closed set lives on it (`change.IsSubmittedBy`, `details.CanHoldBalanceAfter`, `state.IsEditable`, `lossDate.IsAfterDay`, `causeCodes.IsActive`, `role.HasRightsOf`); a fact about a single child lives on the child; a fact about a list of children lives on the root that holds the list (`claim.HasActiveClaimant`, `claim.IsOnlyActiveClaimant`, `claim.HasOtherPrimaryRiskObject`, `claim.HasUnresolvedWarning`). Two rules with different codes share one fact, so no logic is written twice.
- **Time and actor:** the Domain reads no clock and no identity, except `Ids.Next()`, which reads the UTC clock to order new ids (a technical value, not business data). Only an input whose step records a business time or a person's decision carries `Now` (and `ActorId` for a decision), stamped once by the Application: create claim (the received date), remove risk object (`DeletedAt`), waive and acknowledge an issue, the reserve history's submit and decision inputs. A missing time or actor is refused by one shared guard (`InputTime.Required`, `InputActor.Required`), and a business time earlier than the time it follows (a review before the submission, a resolve or delete before the claim's reported date) by `InputTime.NotBefore`; all three throw, because the Application stamps them. The claim's base input carries only the policy period and the cause codes: an automatic clear records no time and no actor, because nobody decided it. Time and actor feed business rules and records only, never audit columns or event metadata. A calendar day derived from a timestamp is its UTC day.
- **Entities:** a child keeps its editable data in one immutable details record, replaced whole with `with`. Capability interfaces are get-only; the fields a step sets have `internal` setters, the others private, and mutators are `internal`; nothing outside the Domain changes any of it.
- **EF friendliness:** every entity has a private parameterless constructor with empty placeholders, built by a test. A value that may be absent is a sealed record with a static `Empty` and an `IsSet` flag. List and search queries use projections or read models, never filtered Domain entities.
- **Creation:** a claim is created by `CreateClaimOperation` as a minimal valid Draft; parties, risk objects and loss details are added by the same operations as later edits.

### Domain folders

The Domain has top-level folders `Core/` (the engine), `Common/` (entity and event base types, capability interfaces, markers, `Ids`, `InputTime`), `Roles/` (the user roles and their rule, shared by the aggregates), and one per aggregate: `Claims/`, `ReserveComponents/`, `ClaimDocuments/` and `Policies/` (the policy entity and its period: reference data that the claim points to by id, with no operations), plus `CauseOfLoss/` (the cause-of-loss code entity and the `CauseCodes` value the claim inputs carry: reference data of the same shape as `Policies/`). An aggregate folder holds the root itself (`Claim.cs` and its values: state, number, severity) and the root's role folders directly; the reserve component folder also holds `History/`, the child entity folder of its history rows.

- `Claims/` also holds one folder per child entity (`Parties/`, `RiskObjects/`, `Loss/`, `Issues/`, `StateChanges/`), each shaped the same way.
- A folder holds the entity or root, its closed sets and values, and these subfolders, created when the first file exists: `Inputs/` (the input records: the public ones the aggregate's methods take, its base inputs, and the internal ones a parent builds for a child's method), `Events/`, `Flows/` (the internal steps: operations and transitions), `Rules/` (every rule about the entity; whether a finding refuses a step or becomes a stored issue is decided by the step or the issue evaluation that requires the rule). A base lives with what it is a base of: a base input in `Inputs/`, the aggregate's base transition in `Flows/`. The entity's root, `Inputs/` and `Events/` are the public surface; the rest is internal.
- An evaluation over a whole entity is one operation in `Flows/` (`EvaluateIssuesOperation.cs`), its input in `Inputs/`.
- Steps and rules that span entities live in the aggregate's own `Flows/` and `Rules/`; the aggregate's step for a change to a child (`<Verb><Thing>OnClaimOperation`) and the aggregate's rules about a child collection (`IsNotLastClaimant`) live in the child's folder.
- **Inputs per method:** every method, public or private, takes one input record with exactly the data it needs and does not already have. An aggregate method's input is request data plus outside facts (actor, time, the policy period and cause codes, plain reserve facts), never the aggregate's own data. Steps that need the same data share one record (`ClaimTransitionInput` for open and investigate, `RowPostingInput` for the posting moves), and a step that needs more derives from it, so a step that reads no role gets none: `RowDecisionInput` (retract) is extended by `RejectRowInput` (the role) and the abstract `RowApprovalInput` (the balance facts), which `AutoApproveRowInput` (no reviewer) and `ApproveRowInput` (the role and the override) extend; `FreezeClaimInput` (close and withdraw) and `SendToPaymentClaimInput` add their reserve facts, and `ReopenClaimInput` adds the role. The reserve component's public inputs (`ApproveHistoryInput`, `RejectHistoryInput`, `RetractHistoryInput`, `HistoryPostingInput`) carry the history row id, and its step builds the row's internal input, adding the balance from the component's own `ApprovedBalance`. A child's method takes its own internal record from the child's `Inputs/` (`RiskObjectDetailsInput`, `IssueResolutionInput`), which the aggregate's step builds from its input and, when the child needs an aggregate fact, from the aggregate's own fields (`this.State`), never the whole aggregate; a method that needs nothing takes `NoInput`.

### Domain engine

`Core` is generic and layered: `Core/Rules` and `Core/Flows`. A layer uses only the layers before it, `Core` and `Common` reference neither `Roles` nor an aggregate, `Common` does not reference `Core`, `Roles` references no aggregate, and `Core` holds no business rule. The reasons are in `docs/ARCHITECTURE.md` 'Domain engine'.

- A step declares its rules once in the constructor with `Require`: a ready instance, or a rule built from the input (`input => new IsNotLastClaimant(input.PartyId)`).
- A step is one shared instance: it names itself as the first type argument of its base, the engine builds it on the first call and reuses it, and `Run(subject, input)` is static (`AddPartyOperation.Run(this, input)`). A step therefore keeps no state beyond its rules (a transition also keeps its target and its rules per source).
- The engine's machinery (`Operation`, `CreationOperation`, `Transition`, `RuleSet`, `IRule`, `CheckResult`) is `internal`; the result side (`Outcome`, `Finding`, `FindingSeverity`, `NoSubject`, `NoInput`, `Move`) is public, because that is all the Application reads. On `Outcome` only `IsSuccess`, `Value`, `CriticalFindings` and `WarningFindings` are public; the follow-up hook (`OnSuccess`) and the factories are `internal`, so only the Domain builds an outcome and its follow-ups.
- `Outcome.OnSuccess(hook)` runs the hook when the step succeeded and returns the same outcome; it never adds a finding or changes the value. A follow-up cannot refuse: a condition that could is a rule on the step that runs first.
- Vocabulary: a rule is checked (`rule.Check`, `rules.Check`) and its `CheckResult` either passes (`Pass`) or holds what it found (`Found`); a finding is Critical or Warning (`CriticalFindings`, `WarningFindings`); a step succeeds or fails (`IsSuccess`) and a move from a state the transition does not allow is refused with a finding too (`Transition.NotPermittedCode`); evaluate means check the issue rules and record the issues (`EvaluateIssues`); an issue is a stored finding on a claim with its own lifecycle (raise, clear, acknowledge, waive). "Check" is only a verb.
- There is one finding type, `Finding` (code, message, field, severity, evidence). A rule that checks a value puts it in the evidence; an issue matches its finding by code and evidence, so a waiver or acknowledgement lapses when the checked values change. Every step stops on a Critical finding. The issue evaluation requires only an editable claim and stores each finding of the issue rules as an issue, unchanged, because an issue is recorded, not a refusal. A part of an aggregate with several rules of its own (the claim's loss) has one combined rule over that part (`HasValidLossDetails`), with the facts from outside it in the constructor; the issue evaluation checks it over the stored loss, and the edit guard after Draft over the proposed values.
- "Not allowed in that state" is a rule that reads the state, never a conditional in an operation.
- A change to an existing child is split by subject. The aggregate's step (`<Verb><Thing>OnClaimOperation`, in the child's `Flows/`) holds only the aggregate's rules (editable, collection rules such as not the last claimant); its `Apply` finds the child by id among all children (an unknown id is the Application's not found) and returns the outcome of the child's own method. The child's method (`party.Remove()`) runs the child's step on the child, with only the child's rules (still active, not removed, unresolved), and raises the child's events in its follow-ups. Every method, on the aggregate or on a child, is one step followed by `OnSuccess` effects; nothing after the step can refuse.
- A child collection on the root returns every row, removed ones included (`Parties`, `RiskObjects`); a filtered question is a bool fact on the root (`HasActiveRiskObject`, `HasUnresolvedCriticalIssues`).
- `Apply` is the only place the subject changes; it uses the subject's `internal` members and returns what it changed: the new or changed child, the root for a change to the root's own fields, or the `Move` for a move.
- A transition is configured by its target, allowed sources and rules only; its hooks are sealed. It returns the `Move` (from, to) it made.
- Every step derives from `Operation`: a creation from `CreationOperation`, a move from `Transition`.
- A `Finding` is Critical by default; `AsWarning()` makes it a Warning. The finding states its severity and the step decides the consequence: an operation or a transition stops on any Critical finding. A warning that must block is its own rule with its own Critical finding (`HasNoUnacknowledgedPolicyWarning`).

### Extending the Domain

Names follow 'Naming' and folders 'Domain folders'.

- **A new entity in an aggregate:** a private parameterless constructor, a `Details` record, private setters and `internal` mutators; an `internal` operation in `Flows/` with its input in `Inputs/`, and a public method on the root that chains the operation's outcome: the event on success, then `Settle`; every precondition of the action as a rule; a stored completeness problem as a rule in its entity's `Rules/` that the issue evaluation checks; tests in the mirrored folders and an entry in `EntityConstructionTests`.
- **A new aggregate:** a top-level folder shaped like `Claims/`, a base input in its `Inputs/` (and a base transition in its `Flows/` once it has a lifecycle), a creation operation behind a public static method named by the domain verb that chains its event, the capability interfaces it needs, and other aggregates referenced by id only.
- **A fact from outside the aggregate:** a plain field on the step's input, computed by one Application query; facts travel as one record only when several fields must come together from one aggregate (`ClaimReference`: the claim's id, organisation and state, which the Application takes from `claim.Reference` to open a reserve component); never a load inside the Domain.
- **State:** a lifecycle is a closed set held in a state property (`State`, `ApprovalState`) with an `internal` setter. A move is a transition ('Code shapes') with the aggregate's transition input or a record deriving from it; a change beyond the move is a separate operation that the Application runs in the same Unit of Work. The aggregate method raises the state-changed event in a follow-up. Who may perform a move is a rule on that transition (`HasMinimumRole` with the lowest role the move needs); `UserRole` is a closed set whose variants list the roles whose rights they hold, so the nesting is stated once, on the roles. `OpenClaimTransition` is the reference.

-

## Tooling, Skills & Temp

### Shell & git

- **Git writes belong to the user.** Never run `git add`, `git commit`, `git push`, `git reset`, `git checkout`, `git restore`, `git rebase`, `git merge`, `git stash` (other than `list` and `show`), `git tag`, `git rm`, `git mv`, `git pull`, `git cherry-pick`, `git revert`, or `git am`; `.claude/settings.json` blocks them in every mode. When one is needed, say so and give the exact command for the user to run. Also never run `git clean`, `git branch -D`, or `git switch` with a force flag (`-f`, `--force`, `--discard-changes`, `-C`, `--force-create`). Read-only git (`diff`, `log`, `show`, `status`, `blame`, `ls-files`, `branch --list`) and read-only search (`grep`, Grep, Glob) run freely, and so does plain `git switch` to change branch.
- **Commit messages:** `type: summary` with the verb in the past tense (`added`, `fixed`, `renamed`), lowercase after the colon, 72 characters or fewer, no trailing period. Types: `feat` (new behavior), `fix` (bug fix), `docs`, `refactor`, `test`, `style` (formatting only), `perf`, `build`, `ci`, `chore`. An optional scope goes in parentheses (`feat(reserves): ...`). Add a body only when the why is not obvious from the summary. One logical change per commit, matching a reviewed pack where possible. Examples: `feat: added claim submission command and handler`, `fix: rejected self-approval of reserves`, `docs: described the AI workflow`. When asked for a message, output only the message (CLAUDE.md 'Output discipline').
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

- after Claude edits this file, a skill, `settings.json`, a temp note, the rule register, `docs/AI-WORKFLOW.md` or `docs/ARCHITECTURE.md`;
- after a code pack, because it also checks the code layout ('Code style');
- after an ai-journal entry, a parking-lot append, and "spec-check build";
- when the user says they are about to commit or asks whether the repo is ready.

Fix findings in files Claude touched before continuing; for findings in other files, report them and ask. A clean run says nothing. If the script reports skipped checks (local-only files missing), mention it once per session.

### Skill triggers

Skills live in `.claude/skills/<name>/SKILL.md`.

The phrases that trigger each skill are in its `SKILL.md` 'Triggers'. What only this file says:

- **explore:** invoke it before investigating directly. Checking ONE already-named fact (a single symbol's location, one file's content) is a direct lookup, not an investigation. A claim naming a rule ID goes to spec-check instead.
- **design:** produces a design; never writes code.
- **code-review:** a code object is required ("review the design" is not a code review). Report-only; fixing a finding is a separate, explicit request.
- **spec-check:** also runs passively after every code-review. Read-only except "spec-check build".
- **ai-journal:** records real events only and merges related events into topic-level entries and SUMMARY narratives; proactive offers always ask first.
- **dev-log:** user-invoked only, as `/dev-log on`, `/dev-log check` or `/dev-log off` (not model-invoked). If asked for dev logs in plain words, reply with the command instead of adding logs by hand. Never leave an untagged dev log in code.

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
