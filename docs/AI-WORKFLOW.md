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

Rules are split so that only what is always needed is loaded every session:

- `.claude/CLAUDE.md`: the short project rules (workflow, output, permissions, the main coding rules). Skills cite its sections by name.
- `docs/CODING-STANDARDS.md` and one file per layer in `docs/layers/`: the detailed rules for code style and naming, and for each layer its rules and its contracts with the neighbouring layers. Claude reads the file of the layer it changes or reviews.
- `docs/ARCHITECTURE.md`: the reasons and the decisions, updated as they change.
- `.claude/skills/<name>/SKILL.md`: the procedures for design, review and the other skills.
- `.claude/temp/`: working notes, the parking lot and the AI journal.

## Safeguards

- **Permissions:** permissions are bypassed in my sessions, so protection has two layers. A deny list in `.claude/settings.json` hard-blocks commands such as git writes. Rules in `CLAUDE.md` and the skills make Claude ask in chat before anything else with side effects (migrations, Azure, deleting files).
- **Confidential documents:** the task documents are local, gitignored copies. Their text is never pasted into tracked files, with one exception: the specification's user-facing validation messages, which appear in Domain rules and the tests that pin them. The workflow check fails when tracked text overlaps the local documents.
- **Executable rules:** the conventions that matter are enforced by code, not only by instructions. Examples are tests that every command has a validator, that command and query names follow their shape and that the compiled assemblies hold no hard-coded GUID, plus the workflow check described below.

## Models and subagents

Claude works inline by default, because a subagent starts without the session's context. The exceptions are fixed: code review always runs in a fresh Opus subagent, spec-check and the AI journal run on Sonnet, and large read-only searches go to Haiku, which returns a list of hits and never a judgment.

## Generated records

Each of these has one fixed format and procedure, so the result does not depend on a particular session:

- **AI journal** (`.claude/temp/notes-ai-journal.md`, written by the ai-journal skill). Append-only: a header `## TYPE-NNN | phase | date` and fixed `key: value` lines. Types are PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION and SUMMARY, and counters are never reused. At the end the skill compiles the correction section below from these entries.
- **Rule register and spec-check** (`docs/rules.local.md`, `.claude/skills/spec-check/SKILL.md`). A trimmed index of the local task documents: 151 one-line rules (122 from the spec, 29 from the assessment) as `ID | area | paraphrase | source | flags`, with a `conflict:` flag (22 rules) where the documents disagree. IDs are source-qualified and stable, and code carries the same ID (`[Trait("Rule", "S:BR-R-03")]`).
- **Workflow check** (`.claude/tools/check_workflow.py`). A read-only script that verifies these files against each other and against the code: citations, the settings deny rules, the register, the note formats, overlap with the confidential documents, and the code layout the standards describe (line length, comments, namespaces, naming, Domain layering, the test mirror). It prints one line per finding and runs after edits, after each code pack and before commits.

## Claude system

- `.claude/CLAUDE.md`: the project rules.
- `.claude/skills/<name>/SKILL.md`: project skills: design, explore, code-review, dev-log, spec-check, ai-journal.
- `.claude/settings.json`: the deny list (the hard layer in every mode) and allow and ask entries for sessions that are not in bypass mode.
- `.claude/tools/check_workflow.py`: the consistency check; `CLAUDE.md` 'Workflow check' says when it runs.
- `.claude/temp/`: committed working notes named `<kind>-<topic>.md`: designs, the parking lot and the AI journal.
- `*.local.md`: gitignored local-only files: copies of the confidential task documents, the rule register and a note with the assessment context. A clone does not have them, so Claude checks for them at the start of a session.

## AI correction journal

Cases where the AI was wrong or weak, and how each was corrected, are recorded as they happen in `.claude/temp/notes-ai-journal.md`. This section will be generated at the end by the ai-journal skill (compile mode), then reviewed and edited by me.
