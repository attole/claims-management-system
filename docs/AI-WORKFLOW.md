# AI Workflow

How AI is used in this project. This is an overview: the rules themselves are in `.claude/CLAUDE.md`, `docs/CODING-STANDARDS.md` and `docs/layers/`, and the decisions behind the design are in `docs/ARCHITECTURE.md`.

## Approach

I use Claude Code in the terminal as an implementation partner under my direction, not as an author working on its own. The behavior is fixed in the repository before any feature work:

- Claude creates or edits files only when I ask. An approved plan is not permission to implement it, and ambiguous wording is treated as a question.
- Git writes are mine: Claude gives me the exact command. `gh` writes (pull requests, issues, releases) need my explicit instruction.
- For any problem with more than one reasonable approach, Claude gives a few argued options and a recommendation, and I choose.
- Business rules come from me and from local copies of the task documents. When a rule is missing, Claude asks instead of inferring one.

## Working loop

Work happens in packs, one cohesive and separately reviewable set of changes at a time:

1. I ask for a design. The design skill writes no code; open choices are settled with me before anything is built.
2. I approve the design and ask for one pack. Claude implements it and stops.
3. Claude runs its own gates before reporting: the build has 0 warnings and 0 errors, the tests pass and the workflow check reports 0 findings.
4. I review the pack file by file and ask Claude to explain anything that differs from what I expected. This is where most of the shaping happens, and it is a loop, not a sign-off (see below).
5. I run the code-review skill. A fresh Opus reader with none of the session's context reads the change and reports findings; I decide which are fixed and which are left alone.
6. The spec-check skill (Sonnet) then maps the changed files to the business rules and reports each problem as missing, untested, deviates, conflict or orphan.
7. I make the commits myself. Settled decisions go into `docs/ARCHITECTURE.md`; side topics go to a tagged parking-lot note instead of being discussed on the spot.

Steps 4 to 6 repeat until the pack is clean. A pack is rarely accepted as first generated:

- **Questions first:** I ask why a class, layer or option exists ("why do we need this?", "what does this do?") before touching it. The answer either justifies it or shows it should go.
- **Alternatives:** when a result feels wrong I ask for the options with their trade-offs, or I propose a different idea myself (a reflection-built list instead of a hand-written table, a dictionary instead of runner classes, one file per use case instead of three) and Claude argues it against the current design.
- **Refactor prompts:** I ask for structural changes on the whole layer, not only the file at hand: regrouping folders by role, merging a command with its validator and handler, renaming a concept. Claude applies them as one mass change and the gates run again.
- **Selective fixes:** I pick findings from a review ("fix all but the first", "only the style ones"), reject suggestions that do not fit, and sometimes decide against a proposal outright.
- **Rule changes:** a repeated correction becomes a rule in `CLAUDE.md` or a layer file, or an executable check, so the next pack does not need the same correction.

The loop ends when the build, tests and checks are clean, the review has no findings I accept, and I have read the result and agree with it.

My prompts describe structure, behavior and guarantees, not code. Style and conventions live in the repository, and I adjust generated code with small follow-up requests. Context between sessions lives in committed notes in `.claude/temp/`, so a new session loads the relevant note instead of replaying a long conversation.

## Instruction layers

Rules are split so that only what is always needed is loaded every session (`CLAUDE.md` holds the gates and the core rules; the detailed style, naming and test shapes load when Claude reads the file it points to):

- `.claude/CLAUDE.md`: the short project rules (workflow, output, permissions, the main coding rules). Skills cite its sections by name.
- `docs/CODING-STANDARDS.md` and one file per layer in `docs/layers/`: the detailed rules for code style and naming, and for each layer its rules and its contracts with the neighbouring layers. Claude reads the file of the layer it changes or reviews.
- `docs/ARCHITECTURE.md`: the reasons and the decisions, updated as they change.
- `.claude/skills/<name>/SKILL.md`: the procedures for design, review and the other skills.
- `.claude/settings.json`: the deny list (the hard layer in every mode) and allow and ask entries for sessions that are not in bypass mode.
- `.claude/tools/check_workflow.py`: the consistency check; `CLAUDE.md` 'Workflow check' says when it runs.
- `.claude/temp/`: committed working notes named `<kind>-<topic>.md`: the parking lot and the AI journal.
- `*.local.md`: gitignored local-only files: copies of the confidential task documents, the rule register and a note with the assessment context. A clone does not have them, so Claude checks for them at the start of a session.

## Safeguards

- **Permissions:** permissions are bypassed in my sessions, so protection has two layers. A deny list in `.claude/settings.json` hard-blocks commands such as git writes. Rules in `CLAUDE.md` and the skills make Claude ask in chat before anything else with side effects (migrations, Azure, deleting files).
- **Confidential documents:** the task documents are local, gitignored copies. Their text is never pasted into tracked files, with one exception: the specification's user-facing validation messages, which appear in Domain rules and the tests that pin them. The workflow check fails when tracked text overlaps the local documents.
- **Executable rules:** the conventions that matter are enforced by code, not only by instructions. Examples are tests that every command has a validator, that command and query names follow their shape and that the compiled assemblies hold no hard-coded GUID, plus the workflow check described below.

## Models and subagents

Claude works inline by default, because a subagent starts without the session's context. The exceptions are fixed: code review always runs in a fresh Opus subagent, spec-check and the AI journal run on Sonnet, and large read-only searches go to Haiku, which returns a list of hits and never a judgment.

## Generated records

Each of these has one fixed format and procedure, so the result does not depend on a particular session:

- **AI journal** (`.claude/temp/notes-ai-journal.md`, written by the ai-journal skill). Append-only: a header `## TYPE-NNN | phase | date` and fixed `key: value` lines. Types are PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION and SUMMARY, and counters are never reused. The skill compiles the correction section below from these entries.
- **Rule register and spec-check** (`docs/rules.local.md`, `.claude/skills/spec-check/SKILL.md`). A trimmed index of the local task documents: 151 one-line rules (122 from the spec, 29 from the assessment) as `ID | area | paraphrase | source | flags`, with a `conflict:` flag (22 rules) where the documents disagree. IDs are source-qualified and stable, and code carries the same ID (`[Trait("Rule", "S:BR-R-03")]`).
- **Workflow check** (`.claude/tools/check_workflow.py`). A read-only script that verifies these files against each other and against the code: citations, the settings deny rules, the register, the note formats, overlap with the confidential documents, and the code layout the standards describe (line length, comments, namespaces, naming, Domain layering, the test mirror). It prints one line per finding and runs after edits, after each code pack and before commits.

## AI correction journal

Cases where the AI was wrong or weak, and how each was corrected, are recorded as they happen in `.claude/temp/notes-ai-journal.md` (entries `CORRECTION-NNN`). This section is compiled from them and grouped by kind of mistake; trivial cases are left out.

Of 52 recorded corrections, I caught 33 myself, mostly with a "why does this exist?" or "where does the spec say that?" question. Code review caught 11, tests 4, the build 2 and spec-check 2. The split changed with the layer: on the backend my questions caught most mistakes, because they were design mistakes; on the frontend the fresh-context review caught most, because they were state and timing bugs that only show in a running component.

### Reading the documents

The AI filled gaps with rules no source states, or read an example as a rule.

| Case | Caught by | Fix |
|---|---|---|
| Read a sample error body and the not-null columns as proof that create must reject bad input, overriding the stated severity (005) | me | Drafts save incomplete with stored issues; the schema departure is recorded |
| Showed a reserve flow where the frontend computes a delta, which no source describes (008) | me | the amount is the signed change the user types |
| Named the ledger row after spec prose instead of its table, and assumed one component per kind (011, 012, 025) | me | entities follow the spec tables; no uniqueness the tables do not state |
| Proposed a warning on a stored cancelled status, which would fire on valid claims (010) | me | a cancellation date column and a coverage window derived from dates |
| Recorded its own conditions as decisions (a loss-after-report rule, a report-date refusal, a narrowed reading) (033) | spec-check | the spec's literal reading; own conditions listed separately for confirmation |
| Recommended a URI error type and an upload exemption from Idempotency-Key without checking the spec or the framework (041, 042) | me | the spec's short error names; uploads covered like every write |

### Over-built design

The AI's first drafts often added a layer that carried nothing. Each was removed after I asked what it was for.

| Case | Caught by | Fix |
|---|---|---|
| Transition design with an edge list, info and capability interfaces and a shared input base (006) | me | one input record per transition, rules as plain classes |
| Engine with a Unit type, gate hooks, before and after hooks, and a third type to carry hook state (013, 019, 023) | me | a step returns an outcome; follow-ups chain on it; two engine types |
| A one-field context object for the time, then Domain bases that chained steps (015, 022) | me | the time travels on the input; the Application calls one aggregate method |
| Per-input builder classes, a context loader, four files per use case (027, 029) | me | explicit readers, one file per use case with its validator and handler |
| Two Application ports that only reached EF, an opt-out switch for soft delete, needless helpers and stubs in Infrastructure (031, 036, 038) | me | Unit of Work in Persistence; soft delete from a Domain interface; helpers inlined |

### Data placed in the wrong spot

| Case | Caught by | Fix |
|---|---|---|
| Hangfire packages on the API project (001) | me | each layer registers itself through its own extension methods |
| A closed-set registry in the Domain, described as both a check and a lookup (009) | me | markers in the Domain, the factory in the Application, converters in Persistence |
| Withdrawal and reopen reasons as fields on the claim (007) | me | an append-only state-change history; closed date and reason derived from it |
| Policy period bundled with the cause codes, cause codes kept under Loss (024, 028) | me | each fact under its owner, listed explicitly on every claim input |
| A download controller using local storage classes, with a token forgery risk under Azure (046) | me | an Application port; the check sits next to what it protects |

### Claims the AI did not verify

| Case | Caught by | Fix |
|---|---|---|
| A code review said editorconfig settings would not work, while naming rules were really never enforced (004) | build | a scratch file with deliberate violations; naming rules now fail the build |
| A version pin recommended before checking its advisory (002) | build | risk accepted for that one advisory and recorded in this repo |
| Layer docs promised an EF mapping that EF refuses (034, 035) | test, me | shadow columns; mapping shapes changed instead of forced |
| Said an Azure download keeps the original file name; the blob name has an upload prefix (047) | me | the link carries the original name, with a test |
| A review fix put the reject reason in the wrong audit field (032) | spec-check | moved back; spec-check now runs inside each pack |
| Test data without cause codes produced 500s; a review asked for a fix that could not work as written (045) | test | an organisation helper with its own code; findings checked against the schema first |

### Rules the AI broke in its own code

| Case | Caught by | Fix |
|---|---|---|
| Overridable transitions, missing events and guards (017, 018) | code review | sealed flows, events and existence rules with tests |
| A validation behavior that threw, with a scan test as the guard (030) | me | marker interfaces the compiler enforces; the behavior returns a result |
| Controllers broke the layer note: no request record, a plural property named in the singular, a hand-built 422, enums as numbers (043) | code review | fixed; the layer note is reread before writing a controller |
| Tests broke the test shape in every API pack (044) | code review | fixed each round; the workflow check catches length and using order |
| A green pack still had five defects (039) | code review | fresh-context review is part of every pack, not optional |

### Frontend state and tests

Every frontend pack passed build, tests and lint before the review, and every review still found real defects.

| Case | Caught by | Fix |
|---|---|---|
| OnPush children read the shared intake form through methods and showed stale values after edits in another step or a 422 (048) | code review | the form value reaches each step as a signal input |
| Hidden state leaked: party names kept after switching to a company, an old policy search kept after Change (049) | code review | the shared party editor clears the other type's fields; the store query is reset |
| Idempotency keys kept after a 422, which would refuse a corrected body (050) | code review | keep a key only after a failure without an answer; one shared refusal check |
| An upload locked the whole page, dropped warnings and showed 0% with no size; history sorted by a per-component number (051) | code review | separate upload state, one success handler, an indeterminate bar, sort by submit time |
| Resource reads in the error state froze a list; a bad id spun forever; a failed reload wiped the screen and unsaved notes (054, 055) | test, code review | stores keep the last good value and name every state; notes count as saved only when the server confirms |
| A missing time format crashed the intake page; only rendering the control in a test showed it (053) | test | the date adapter gets the time formats |
| Specs that could not fail: a 422 test that never left step 1, a removal test without the declined path, a spec restating its constants (052) | code review | each test now fails when the behavior it names breaks |

### What changed in the process

- A repeated correction became a rule or a check: one word per concept (016, 021), explicit safe defaults in the type (003), CLAUDE.md exceptions instead of noisy per-case ones (014), and checks for naming, GUIDs, test shape and confidential text overlap in the workflow check.
- Review findings are claims to test, not verdicts: two of them were wrong or unworkable as written (004, 045).
- My most effective tool was the short why-question; it removed more structure than any review round.
