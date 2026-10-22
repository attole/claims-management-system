# AI Workflow

How AI is used in this project. This is an overview: the rules are in [`.claude/CLAUDE.md`](../.claude/CLAUDE.md), the standards files (backend, frontend, deployment) and [`docs/layers/`](layers), and the decisions behind the design are in [`docs/ARCHITECTURE.md`](ARCHITECTURE.md). Excerpts of the Claude Code sessions are in the [conversation history](#conversation-history).

## Approach and working loop

I use Claude Code in the terminal as an implementation partner under my direction, not as an author working on its own. My effort goes in before the code. I keep proper docs for the AI in the repository (rules, conventions, one file per layer), so every design and approach stays aligned, and for each task I give the context and the implementation details up front, as structure, behavior and guarantees rather than code. The AI then builds what was described instead of guessing, accuracy is better and there is little to refactor; in my experience that means less rework of large implemented chunks and fewer tokens, though I have not measured it. Context between sessions lives in committed notes in [`.claude/temp/`](../.claude/temp), so a new session loads the relevant note instead of replaying a long conversation. Work is cut into packs, each a cohesive set of changes small enough for me to read and audit, and small enough not to overload the session context, which in my experience keeps the AI faster and more accurate. The behavior is fixed in the repository: Claude creates or edits files only when I ask, git writes are mine, `gh` writes need my explicit instruction, a choice between approaches gets argued options and a recommendation, and business rules come from me and the local task documents, so a missing rule is asked about, never inferred.

The loop for each pack:

1. **Understand:** I first understand the area, its domain and functionality. I use Claude for this too: it has access to the docs, and sometimes I tell it to search the web.
2. **Design:** if the codebase already has a similar implementation, I tell Claude to base the work on it. If not, I think through the design, load the details and my reasoning into the session, and use the design skill. It writes no code, and open choices are settled with me first.
3. **Build:** I approve the design and ask for one pack. Claude implements it following the repo docs and stops.
4. **Gates:** Claude runs the build (0 warnings, 0 errors), the tests and the workflow check (0 findings) before reporting.
5. **My review:** I read the pack file by file and ask about anything unexpected.
6. **Refactor:** where the shape is not right, I ask for structural changes, across a whole layer when needed; the gates run again.
7. **Code review:** the code-review skill runs a fresh Opus reader with none of the session's context; I decide which findings are fixed.
8. **Rule check:** the spec-check skill (Sonnet) maps the changed files to the business rules and reports what is missing, untested, deviating, conflicting or invented.
9. **Close:** I make the commits. Settled decisions go to [`docs/ARCHITECTURE.md`](ARCHITECTURE.md), side topics to a tagged parking-lot note.

Steps 5 to 8 repeat until everything is clean and I agree with the result; a pack is rarely accepted as first generated. In that loop I review for parts that look wrong, over-engineered or hardcoded, and bring Claude a detailed question for each: what looks wrong to me, sometimes why it worries me, and the angle to check it from. It re-analyzes the code in that scope and explains or changes it. I also propose alternatives and have Claude argue them, pick which review findings to fix, and turn a repeated correction into a rule.

## Instruction layers

Only what is always needed loads every session; the detailed rules load when Claude reads the file they are in:

- [`.claude/CLAUDE.md`](../.claude/CLAUDE.md): the short project rules (workflow, output, permissions, the core coding rules). Skills cite its sections by name.
- [`docs/BACKEND-STANDARDS.md`](BACKEND-STANDARDS.md), [`docs/FRONTEND-STANDARDS.md`](FRONTEND-STANDARDS.md) and [`docs/DEPLOYMENT-STANDARDS.md`](DEPLOYMENT-STANDARDS.md): how code is written in each area (style, naming, test shapes), one file per area.
- [`docs/layers/`](layers): for each layer its rules and contracts with its neighbours. Claude reads the standards file and the layer file of the area it changes.
- [`docs/ARCHITECTURE.md`](ARCHITECTURE.md): the reasons and the decisions, updated as they change.
- [`.claude/skills/<name>/SKILL.md`](../.claude/skills): the procedures for design, review and the other skills.
- [`.claude/settings.json`](../.claude/settings.json): the deny list, the hard layer in every mode.
- [`.claude/tools/check_workflow.py`](../.claude/tools/check_workflow.py): the consistency check (see Generated records).
- [`.claude/temp/`](../.claude/temp): committed working notes, the parking lot and the AI journal.
- `*.local.md`: gitignored files (the confidential task documents, the rule register, an assessment note). A clone does not have them, so Claude checks for them at the start of a session.

## Safeguards

- **Permissions:** permissions are bypassed in my sessions, so protection has two layers: a deny list in [`.claude/settings.json`](../.claude/settings.json) that hard-blocks git writes, and rules in `CLAUDE.md` and the skills that make Claude ask in chat before anything else with side effects (migrations, Azure, deleting files).
- **Confidential documents:** their text is never pasted into tracked files, except the specification's user-facing validation messages in Domain rules and the tests that pin them. The workflow check fails when tracked text overlaps the local documents.
- **Executable rules:** conventions are enforced by code where it matters, for example tests that every command has a validator, that command and query names follow their shape, and that the assemblies hold no hard-coded GUID.

## Tools, models and subagents

The only AI tool used is Claude: Claude Code in the terminal, with its web search when I asked for it. No other AI tool (Cursor, Copilot, ChatGPT) was used. Claude ran the ordinary developer tools on my behalf inside the permission rules above: the .NET SDK, npm, Git (reads only), the GitHub CLI, the Azure CLI, PowerShell, a local SQL Server (LocalDB) and the app itself for local runs. Git writes, deployments and every approval stayed mine.

Claude works inline by default, because a subagent starts without the session's context. The exceptions are fixed: code review runs in a fresh Opus subagent, spec-check and the AI journal run on Sonnet, and large read-only searches go to Haiku, which returns hits and never a judgment.

## Generated records

Each has one fixed format and procedure, so the result does not depend on a particular session:

- **AI journal** ([`.claude/temp/notes-ai-journal.md`](../.claude/temp/notes-ai-journal.md), ai-journal skill): append-only entries `## TYPE-NNN | phase | date` with fixed `key: value` lines. Types are PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION and SUMMARY. The sections below are compiled from it.
- **Rule register** (`docs/rules.local.md`, spec-check skill): 151 one-line rules (122 from the spec, 29 from the assessment) as `ID | area | paraphrase | source | flags`, 22 of them flagged `conflict:` where the documents disagree. Code carries the same ID (`[Trait("Rule", "S:BR-R-03")]`).
- **Workflow check** (`check_workflow.py`): a read-only script that verifies citations, the settings deny rules, the register, the note formats, overlap with the confidential documents, and the code layout (line length, comments, namespaces, naming, Domain layering, the test mirror). It runs after edits, after each code pack and before commits.

## Representative prompts

My prompts ask for understanding, evidence or a decision, and the answer comes back as options for me to pick from. Examples (paraphrased from the journal; longer verbatim stretches are in the [conversation history](#conversation-history)):

- **Scaffold from the documents, not from model memory** ([001][p001]): "read the stack and the frontend connection from the task documents, then create every layer as its own project". Claude returned argued options first and I picked solution format, central packages, MediatR 12 and the test shells; on the other side, its first layering needed my pushback.
- **Understand before designing** ([003][p003]): "explain the domain in real-life terms: what goes in and out, the data flow, what must be processed". It led to a read of every section, a conflict list and the split into a claim side and a reserve side.
- **Steer the design** ([004][p004]): "statuses and rules should be objects with static rules and states, not booleans and if/else". It drove the state and transition design away from branching; on the other side, the first proposals were over-built with abstractions and extra layers, and needed many rounds of simplification.
- **Audit the plan against the sources** ([009][p009]): "read only the original documents on the tech stack and align each package and pattern with our plan; say what is missed". It produced the gap list that set the restructure order.
- **Make the AI verify** ([011][p011]): "when unsure, ask what a term means or check the docs, the frontend section and the existing rules before deciding". It worked: two recommendations were reversed after those checks; on the other side, the AI had made them without checking until it was told to.
- **Question each abstraction from my review** ([012][p012]): why AutoMapper for requests, why this envelope name, why this folder, asked with my concern and the scope to re-check. Each question forced an argued comparison and a decision.

## What AI built and what I designed

On the backend, AI wrote most of the code, tests and docs, but the architecture is mine: the layering, the engine and all the conventions, the Domain, its split from Persistence (shadow properties, views, audit) and the events. It was not accepted as first proposed. It came out of many design iterations in which I asked why each part exists, proposed alternatives and had the AI argue them, and I reviewed and refactored it until I could explain every decision. The reasons are in [`docs/ARCHITECTURE.md`](ARCHITECTURE.md), and the journal records the split per pack (`SPLIT-NNN`).

The frontend was coded far more by AI. I set up the general structure once (layout, pipes, interceptors, service layer) and then ran pack after pack with almost no manual review of the code, relying on the build, the tests, the lint and the AI code-review skill. That is why the review, not I, found most frontend corrections. Deployment and the demo scope (any user may assign a handler, the Hangfire dashboard open on fake data) are my decisions, built by Claude.

## AI correction journal

Cases where the AI was wrong or weak are recorded as they happen in the [AI journal](../.claude/temp/notes-ai-journal.md) (`CORRECTION-NNN`; the numbers below link to the entries). This section is compiled from them: a few cases in full, then the rest in short.

Of 55 recorded corrections, I caught 35 myself, mostly with a detailed question from my review ("why does this exist?", "where does the spec say that?") that carried my concern and the angle to check. Code review caught 12, tests 4, the build 2 and spec-check 2. On the backend my questions caught most mistakes, because they were design mistakes; on the frontend the fresh-context review caught most, because they were state and timing bugs that only show in a running component.

### Featured cases

**The AI invented rules the sources do not state** ([005][c005], [033][c033]). It read a sample error body and the not-null columns as proof that create must reject bad input, overriding the stated severity, and later recorded its own conditions as decisions. I caught the first; spec-check caught the second. Drafts now save incomplete with their issues stored, and the AI's own conditions are listed separately for me to confirm. An example is not a rule, and a missing rule is asked about.

**Over-built design** ([006][c006], [013][c013], [015][c015], [019][c019], [022][c022], [023][c023]). The engine and the transitions went through several rounds, not one correction. The drafts carried a Unit type, gate hooks, before and after hooks, a hook-state type, an edge list, capability interfaces, a shared input base, a context object for the time and Domain bases that chained steps. Each round I reviewed what came back, picked what looked over-engineered and asked what the part was for and why it was needed, with my concern; the AI defended it and then agreed, and the next draft removed one layer and exposed the next. It ended with a step that returns an outcome, follow-ups that chain on it, two engine types and the time on the input. These targeted questions removed more structure than any review round.

**A review claim that was false** ([004][c004]). A code review said editorconfig naming rules would not work, while they had never been enforced at all. I tested with a scratch file of deliberate violations, and naming rules now fail the build. A review finding is a claim to test, not a verdict.

**A green pack with five defects** ([039][c039]). Build, tests and workflow check passed, and the fresh-context review still found five defects. Four API rounds had 4, 7, 9 and 4 findings. Fresh-context review is now part of every pack.

**Stale state in OnPush components** ([048][c048]). Child components read the shared intake form through methods and showed stale values after an edit in another step or a 422. Only the review found it; the tests passed. The form value now reaches each step as a signal input.

**A review fix that could not work** ([045][c045]). Test data without cause codes produced 500s, and the review asked for a fix that could not work as written. The test failure showed it. I questioned the finding, added an organisation helper with its own code, and now check each finding against the schema first.

### Other cases

**The AI filled gaps the sources leave open:**

- A reserve flow where the frontend computes a delta, which no source describes: the amount is the signed change the user types ([008][c008]).
- The ledger row named after spec prose and one component per kind assumed: entities follow the spec tables ([011][c011], [012][c012], [025][c025]).
- A warning on a stored cancelled status that would fire on valid claims: a cancellation date column and a coverage window derived from dates ([010][c010]).
- A URI error type and an upload exemption from Idempotency-Key, recommended without checking: the spec's short error names, uploads covered like every write ([041][c041], [042][c042]).

**The AI added structure that carried nothing** (found by my review questions):

- Per-input builder classes, a context loader and four files per use case: explicit readers, one file per use case ([027][c027], [029][c029]).
- Two Application ports that only reached EF, an opt-out switch for soft delete and needless helpers: Unit of Work in Persistence, soft delete from a Domain interface ([031][c031], [036][c036], [038][c038]).

**The AI put data in the wrong owner** (found by me):

- Hangfire packages on the API project: each layer registers itself through its own extension methods ([001][c001]).
- A closed-set registry in the Domain: markers in the Domain, the factory in the Application, converters in Persistence ([009][c009]).
- Withdrawal and reopen reasons as fields on the claim: an append-only state-change history ([007][c007]).
- Policy period bundled with the cause codes: each fact under its owner ([024][c024], [028][c028]).
- A download controller using local storage classes, with a token forgery risk under Azure: an Application port ([046][c046]).

**The AI stated things it had not verified:**

- A version pin recommended before checking its advisory: risk accepted and recorded; caught by the build ([002][c002]).
- Layer docs promised an EF mapping that EF refuses: shadow columns; caught by a test ([034][c034], [035][c035]).
- An Azure download said to keep the original file name, while the blob name has an upload prefix: the link carries the original name, with a test ([047][c047]).
- A review fix put the reject reason in the wrong audit field: moved back; spec-check now runs inside each pack ([032][c032]).
- Advice on logging given without checking what was captured: it said there was no Application Insights and that a one-off 500 would have left no trace, and advised dropping console logs; Log Analytics already held them, and the startup error lives there. Console logs restored; caught by me, then by code review ([057][c057]).

**The AI broke the repo's own rules** (found by code review):

- Overridable transitions and missing events and guards: sealed flows, events and existence rules with tests ([017][c017], [018][c018]).
- Controllers with no request record, a hand-built 422 and enums as numbers: the layer note is reread before writing a controller ([043][c043]).
- Tests off the test shape in every API pack: the workflow check now catches length and using order ([044][c044]).
- A copied name pair: `Was`/`Has` from a bool event used for ids, and event parameters on one line against the event shape: renamed to `From` and `To` ([058][c058]).
- A validation behavior that threw, guarded by a scan test (found by me): marker interfaces the compiler enforces ([030][c030]).

**Frontend state and tests** (found by code review, tests and manual testing):

- Hidden state leaked: party names kept after switching to a company, an old policy search kept after Change ([049][c049]).
- Idempotency keys kept after a 422, which would refuse a corrected body: a key is kept only after a failure without an answer ([050][c050]).
- An upload that locked the whole page and dropped warnings: separate upload state and one success handler ([051][c051]).
- A failed reload that wiped the screen and unsaved notes: stores keep the last good value ([054][c054], [055][c055]).
- A missing time format that crashed the intake page, shown only by rendering the control in a test ([053][c053]).
- A Cancel button that closed with an empty string, so cancelling the status, policy and reject dialogs sent an empty write (a 415 on the API): the helpers accept only a real answer, with cancel tests; found through the monitoring logs and a manual repro ([056][c056]).
- Specs that could not fail, such as a 422 test that never left step 1: each test now fails when the behavior it names breaks ([052][c052]).

### What changed in the process

- **Repeated corrections became rules and checks:** one word per concept ([016][c016], [021][c021]), explicit safe defaults in the type ([003][c003]), exceptions in `CLAUDE.md` instead of noisy per-case ones ([014][c014]), and workflow-check rules for naming, GUIDs, test shape and confidential text overlap.
- **Review findings are claims to test:** two were wrong or unworkable as written ([004][c004], [045][c045]).
- **The instruction files evolved with the project, and I simplified them.** `CLAUDE.md` went from about 23 KB to 17 KB, with style, naming and test shapes moved to one standards file per area and the layer files. Each skill was trimmed and given the same five sections, and the workflow check verifies that their citations match the headings in `CLAUDE.md`. Fixes followed what the work showed: code review gained scripts and deployment, explore a deployment area.
- **My most effective tool was the targeted question:** after reviewing, I bring what looks wrong, over-engineered or hardcoded to Claude with my concern and the angle to check, so it re-analyzes that scope instead of defending it.

## Honest assessment

On the codebase as it is now, developed and documented, the AI has been accurate at adding new functionality and fixes. I did not time or cost these, so I do not claim speed or price. I attribute the accuracy to the repository docs, the conventions and the small packs, not to the model alone: the first packs needed many corrections, and these needed few. Two examples from the same day:

- **New feature, in one shot** ([bf52eec](https://github.com/attole/claims-management-system/commit/bf52eec12133484d7a9ffe651ad36de17def761c)): handler assignment, with a domain event, a new audit type and handler, the store, the page and a new frontend panel, with tests and docs, across backend and frontend. The automatic fresh-context review found five items, all of naming, layout or a duplicated helper, and no defect (event parameter layout, a `Has` name on a Guid, a missing test trait, a misnamed local, one duplicated helper).
- **Bug fix, found and fixed in one session** ([78eef98](https://github.com/attole/claims-management-system/commit/78eef984b17c448526cd662569ec7f37129dffc2)): a status change failed with a 415. The monitoring logs showed the request, I reproduced it by pressing Cancel in the dialog, and Claude traced it to a Cancel button that closes with an empty string, fixed the three dialog helpers with cancel tests and audited every other dialog and write ([056][c056]). It combined monitoring, the AI, a code check and manual testing.

Helped most:

- **Mechanical refactors:** 27 files moved in minutes, and about 150 call sites changed for one new argument, with the build and tests as the check ([004][a004], [011][a011], [012][a012]).
- **Reading the documents:** strong at listing error cases and conflicts, and at mapping the spec's routes and screens onto the use cases, though it had to be told to check rather than infer ([002][a002], [014][a014]).
- **A pack built in one pass after an argued outline:** source, tests and docs together; only unused usings and long lines needed fixing ([008][a008]).
- **Real-database tests:** SQLite round trips plus a SQL Server model check exposed three EF mapping limits within minutes ([013][a013]).
- **Fresh-context review:** every round found defects the writing session missed ([005][a005], [015][a015]).

Helped some or little:

- **Web research:** useful leads on EF Core, but mostly secondary sources; on domain practice thin and vendor-specific. It informed options and decided nothing ([003][a003], [010][a010]).
- **Resuming a pack cut by a session limit:** left a using-order finding and four stale doc lines, caught by the workflow check and a reread ([009][a009]).

Where it was weak: the first abstractions were over-built, it defended its own structure before agreeing with a question, it raised style items that needed a rule decision first, and it did not verify claims about tools and the framework unless asked.

## Conversation history

Four excerpts of the Claude Code sessions are in [`docs/ai-transcripts/`](ai-transcripts): 170 of my messages with Claude's replies, including the context compactions.

1. [Setting up the AI working system](ai-transcripts/01-workflow-setup.md) (10 Oct)
2. [The domain engine, designed through questions](ai-transcripts/02-backend-engine-design.md) (13 Oct)
3. [An Application pack, from build to review to refactor](ai-transcripts/03-backend-application-pack.md) (19 Oct)
4. [Cloud infra, from failing deploys to a simpler design](ai-transcripts/04-cloud-infra-incident.md) (21 to 22 Oct)

They are verbatim: my messages and Claude's replies are unchanged, with tool calls as one-line notes. Local paths, environment names, ids, tokens and e-mail addresses are replaced by placeholders, and a paragraph quoting the confidential task documents is replaced by an `[omitted]` line. The full sessions are not committed, because they contain that text and environment ids.

[a002]: ../.claude/temp/notes-ai-journal.md#assessment-002--p1-domain--2026-10-11
[a003]: ../.claude/temp/notes-ai-journal.md#assessment-003--p1-domain--2026-10-11
[a004]: ../.claude/temp/notes-ai-journal.md#assessment-004--p1-domain--2026-10-12
[a005]: ../.claude/temp/notes-ai-journal.md#assessment-005--p1-domain--2026-10-14
[a008]: ../.claude/temp/notes-ai-journal.md#assessment-008--p1-domain--2026-10-15
[a009]: ../.claude/temp/notes-ai-journal.md#assessment-009--p1-domain--2026-10-15
[a010]: ../.claude/temp/notes-ai-journal.md#assessment-010--p1-domain--2026-10-18
[a011]: ../.claude/temp/notes-ai-journal.md#assessment-011--p1-domain--2026-10-19
[a012]: ../.claude/temp/notes-ai-journal.md#assessment-012--p3-backend--2026-10-20
[a013]: ../.claude/temp/notes-ai-journal.md#assessment-013--p3-backend--2026-10-20
[a014]: ../.claude/temp/notes-ai-journal.md#assessment-014--p3-backend--2026-10-21
[a015]: ../.claude/temp/notes-ai-journal.md#assessment-015--p3-backend--2026-10-21
[c001]: ../.claude/temp/notes-ai-journal.md#correction-001--p3-backend--2026-10-10
[c002]: ../.claude/temp/notes-ai-journal.md#correction-002--p3-backend--2026-10-10
[c003]: ../.claude/temp/notes-ai-journal.md#correction-003--p3-backend--2026-10-10
[c004]: ../.claude/temp/notes-ai-journal.md#correction-004--p3-backend--2026-10-10
[c005]: ../.claude/temp/notes-ai-journal.md#correction-005--p1-domain--2026-10-11
[c006]: ../.claude/temp/notes-ai-journal.md#correction-006--p1-domain--2026-10-11
[c007]: ../.claude/temp/notes-ai-journal.md#correction-007--p1-domain--2026-10-11
[c008]: ../.claude/temp/notes-ai-journal.md#correction-008--p1-domain--2026-10-11
[c009]: ../.claude/temp/notes-ai-journal.md#correction-009--p1-domain--2026-10-11
[c010]: ../.claude/temp/notes-ai-journal.md#correction-010--p1-domain--2026-10-12
[c011]: ../.claude/temp/notes-ai-journal.md#correction-011--p1-domain--2026-10-12
[c012]: ../.claude/temp/notes-ai-journal.md#correction-012--p1-domain--2026-10-12
[c013]: ../.claude/temp/notes-ai-journal.md#correction-013--p1-domain--2026-10-12
[c014]: ../.claude/temp/notes-ai-journal.md#correction-014--p1-domain--2026-10-12
[c015]: ../.claude/temp/notes-ai-journal.md#correction-015--p1-domain--2026-10-12
[c016]: ../.claude/temp/notes-ai-journal.md#correction-016--p1-domain--2026-10-14
[c017]: ../.claude/temp/notes-ai-journal.md#correction-017--p1-domain--2026-10-14
[c018]: ../.claude/temp/notes-ai-journal.md#correction-018--p1-domain--2026-10-14
[c019]: ../.claude/temp/notes-ai-journal.md#correction-019--p1-domain--2026-10-14
[c021]: ../.claude/temp/notes-ai-journal.md#correction-021--p1-domain--2026-10-14
[c022]: ../.claude/temp/notes-ai-journal.md#correction-022--p1-domain--2026-10-15
[c023]: ../.claude/temp/notes-ai-journal.md#correction-023--p1-domain--2026-10-15
[c024]: ../.claude/temp/notes-ai-journal.md#correction-024--p1-domain--2026-10-18
[c025]: ../.claude/temp/notes-ai-journal.md#correction-025--p1-domain--2026-10-18
[c027]: ../.claude/temp/notes-ai-journal.md#correction-027--p3-backend--2026-10-19
[c028]: ../.claude/temp/notes-ai-journal.md#correction-028--p1-domain--2026-10-19
[c029]: ../.claude/temp/notes-ai-journal.md#correction-029--p3-backend--2026-10-19
[c030]: ../.claude/temp/notes-ai-journal.md#correction-030--p3-backend--2026-10-19
[c031]: ../.claude/temp/notes-ai-journal.md#correction-031--p3-backend--2026-10-20
[c032]: ../.claude/temp/notes-ai-journal.md#correction-032--p3-backend--2026-10-20
[c033]: ../.claude/temp/notes-ai-journal.md#correction-033--p3-backend--2026-10-20
[c034]: ../.claude/temp/notes-ai-journal.md#correction-034--p3-backend--2026-10-20
[c035]: ../.claude/temp/notes-ai-journal.md#correction-035--p3-backend--2026-10-20
[c036]: ../.claude/temp/notes-ai-journal.md#correction-036--p3-backend--2026-10-20
[c038]: ../.claude/temp/notes-ai-journal.md#correction-038--p3-backend--2026-10-21
[c039]: ../.claude/temp/notes-ai-journal.md#correction-039--p3-backend--2026-10-21
[c041]: ../.claude/temp/notes-ai-journal.md#correction-041--p3-backend--2026-10-21
[c042]: ../.claude/temp/notes-ai-journal.md#correction-042--p3-backend--2026-10-21
[c043]: ../.claude/temp/notes-ai-journal.md#correction-043--p3-backend--2026-10-21
[c044]: ../.claude/temp/notes-ai-journal.md#correction-044--p3-backend--2026-10-21
[c045]: ../.claude/temp/notes-ai-journal.md#correction-045--p3-backend--2026-10-21
[c046]: ../.claude/temp/notes-ai-journal.md#correction-046--p3-backend--2026-10-21
[c047]: ../.claude/temp/notes-ai-journal.md#correction-047--p3-backend--2026-10-21
[c048]: ../.claude/temp/notes-ai-journal.md#correction-048--p4-frontend--2026-10-21
[c049]: ../.claude/temp/notes-ai-journal.md#correction-049--p4-frontend--2026-10-21
[c050]: ../.claude/temp/notes-ai-journal.md#correction-050--p4-frontend--2026-10-21
[c051]: ../.claude/temp/notes-ai-journal.md#correction-051--p4-frontend--2026-10-21
[c052]: ../.claude/temp/notes-ai-journal.md#correction-052--p4-frontend--2026-10-21
[c053]: ../.claude/temp/notes-ai-journal.md#correction-053--p4-frontend--2026-10-21
[c054]: ../.claude/temp/notes-ai-journal.md#correction-054--p4-frontend--2026-10-21
[c055]: ../.claude/temp/notes-ai-journal.md#correction-055--p4-frontend--2026-10-21
[c056]: ../.claude/temp/notes-ai-journal.md#correction-056--p4-frontend--2026-10-22
[c057]: ../.claude/temp/notes-ai-journal.md#correction-057--p5-review--2026-10-22
[c058]: ../.claude/temp/notes-ai-journal.md#correction-058--p5-review--2026-10-22
[p001]: ../.claude/temp/notes-ai-journal.md#prompt-001--p3-backend--2026-10-10
[p003]: ../.claude/temp/notes-ai-journal.md#prompt-003--p1-domain--2026-10-11
[p004]: ../.claude/temp/notes-ai-journal.md#prompt-004--p1-domain--2026-10-11
[p009]: ../.claude/temp/notes-ai-journal.md#prompt-009--p1-domain--2026-10-18
[p011]: ../.claude/temp/notes-ai-journal.md#prompt-011--p3-backend--2026-10-21
[p012]: ../.claude/temp/notes-ai-journal.md#prompt-012--p3-backend--2026-10-21
