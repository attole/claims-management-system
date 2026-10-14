# AI Workflow

## How I use AI

I use Claude Code in the terminal as an implementation partner under my direction, not as an author working on its own. The behavior is fixed in the repository before any feature work, in `.claude/CLAUDE.md` and `.claude/settings.json`:

- Claude creates or edits files only when I ask, an approved plan is not permission to implement it, git writes are mine (Claude gives me the command), `gh` writes need my explicit instruction, and ambiguous phrasing is treated as a question.
- Permissions are bypassed, so protection has two layers: a deny list in `settings.json` that hard-blocks commands such as git writes, and rules in `CLAUDE.md` and the skills that make Claude ask in chat before anything else with side effects (migrations, Azure, deletions).
- For any problem with more than one reasonable approach Claude gives a few argued options and a recommendation, and I choose. Business rules come from me and from local copies of the task documents; when a rule is missing, Claude asks instead of inferring one.

Work happens in packs. I ask for a design (the design skill writes no code), settle the open choices, approve it, then ask for one pack at a time; Claude implements it and stops. Side topics go to a tagged parking-lot note instead of being discussed on the spot. My prompts describe structure, behavior and guarantees, not code; style and conventions live in `CLAUDE.md`, and I adjust generated code with small follow-up queries. Context between topics lives in committed notes in `.claude/temp/`, so I clear the session and load the relevant note instead of carrying long conversations.

When a pack is done I review it file by file myself and ask Claude to explain anything that differs from what I expected. Then I run the code-review skill (a fresh Opus reader with none of the session's context) followed by the passive spec-check against the business rules. I run the build and tests and make the commits myself. Decisions go into `docs/ARCHITECTURE.md`.

To make this faster and more consistent from one session to the next I rely on a few generated files, each with one fixed format and procedure so the result does not depend on a particular session:

- **AI journal** (`.claude/temp/notes-ai-journal.md`, written by the ai-journal skill). Append-only: a header `## TYPE-NNN | phase | date` and fixed `key: value` lines. Types are PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION and SUMMARY; counters are never reused. At the end the skill compiles the correction section from these entries.
- **Rule register and spec-check** (`docs/rules.local.md`, `.claude/skills/spec-check/SKILL.md`). A trimmed index of the local task documents: 151 one-line rules (122 from the spec, 29 from the assessment) as `ID | area | paraphrase | source | flags`, with a `conflict:` flag (22 rules) where the documents disagree. IDs are source-qualified and stable, and code carries the same ID (`[Trait("Rule", "S:BR-R-03")]`). After every code review the spec-check skill maps the changed files to rules and reports each problem as missing, untested, deviates, conflict or orphan.
- **Workflow check** (`.claude/tools/check_workflow.py`). A read-only script that verifies the files above against each other and the code: citations, the settings deny rules, the register, the note formats, overlap with the confidential documents, and the code layout the standards describe (line length, comments, namespaces, naming, the Domain layering, the test mirror). It prints one line per finding and runs silently after edits, after each code pack and before commits.

## Claude system

- `.claude/CLAUDE.md` - the project rules, the single source of truth; skills cite its sections by name.
- `.claude/skills/<name>/SKILL.md` - project skills: design, explore, code-review, dev-log, spec-check, ai-journal.
- `.claude/settings.json` - the deny list (the hard layer in every mode) and allow and ask entries for sessions that are not in bypass mode.
- `.claude/tools/check_workflow.py` - the consistency check described above; `CLAUDE.md` 'Workflow check' says when it runs.
- `.claude/temp/` - committed working notes named `<kind>-<topic>.md`: designs, the general plan, the parking lot and the AI journal.
- `*.local.md` - gitignored local-only files: copies of the confidential task documents, the rule register and a note with the assessment context. A clone does not have them, so Claude checks for them at the start of a session.

## AI correction journal

Cases where the AI was wrong or weak, and how each was corrected, are recorded as they happen in `.claude/temp/notes-ai-journal.md`. This section will be generated at the end by the ai-journal skill (compile mode), then reviewed and edited by me.
