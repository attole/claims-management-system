status: active

Running record of how AI is used on this project: prompts worth showing, AI mistakes and their fixes, the AI/manual split, and an honest assessment. Raw material for the final AI-WORKFLOW.md. Written by the ai-journal skill (.claude/skills/ai-journal/SKILL.md); to be reviewed and turned into the final document at the end.

FORMAT

Append-only. One entry per block, newest last. The entry headers are the map: `grep "^## " .claude/temp/notes-ai-journal.md` lists every entry.

Header line: `## <TYPE>-<NNN> | <phase> | <YYYY-MM-DD>`
- TYPE is one of PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION, SUMMARY. NNN is a 3-digit counter per type, never reused.
- phase is the full label: p0 setup, p1 domain, p2 data, p3 backend, p4 frontend, p5 review (for example `p3 backend`).
- Entries are separated by one blank line. If the phase is unknown, ask.

Then fixed `key: value` lines, one per line, each value a single line of at most 200 characters (600 for SUMMARY), no blank lines inside an entry:
- PROMPT: purpose, prompt, outcome (kept | changed | dropped), note
- CORRECTION: what, caught-by (user | code-review | spec-check | test | build), fix, lesson
- SPLIT: item, who (ai | manual | hybrid), note
- ASSESSMENT: area, helped (high | medium | low), note
- SESSION: name, phase, note
- SUMMARY: topic, story, decisions, ai-errors, human-vs-ai, refs (a consolidated account of one topic or pack, in a few sentences per field; refs lists the raw entry IDs it covers, or `none`)

Rules:
- Capture broadly; when in doubt, log. The final document is compiled and filtered later.
- Only real events. Claude drafts, the user confirms; nothing is invented or embellished.
- Paraphrase. Never copy text from the confidential local documents (docs/*.local.md), never include secrets, tokens, or connection strings. Rules are referenced by source-qualified ID only.
- Prompt values are the user's prompt trimmed or paraphrased, never pasted document text.

ENTRIES

## CORRECTION-001 | p3 backend | 2026-10-10
what: AI proposed direct Hangfire packages (AspNetCore, SqlServer) on the API project instead of keeping all background-job wiring behind the Infrastructure layer
caught-by: user
fix: Hangfire registration and dashboard moved into AddInfrastructure and UseInfrastructure extension methods; API references only layer projects
lesson: each layer registers its own services via extension methods; the host takes no direct dependency on a layer's technology packages

## CORRECTION-002 | p3 backend | 2026-10-10
what: AI suggested pinning AutoMapper below 15 after checking advisories; the check showed 14.0.0 is affected by an advisory whose fixes are only in the commercial releases
caught-by: build
fix: user chose to accept the risk on 14.0.0; that single advisory is suppressed in the audit settings and the risk is recorded in docs/ARCHITECTURE.md
lesson: verify advisories before recommending a version pin; warnings-as-errors surfaces accepted risks, so suppress by advisory ID and document it

## CORRECTION-003 | p3 backend | 2026-10-10
what: AI left the Enabled feature switches with no inline default and argued the implicit false was enough; user wanted the fail-closed default written in the class
caught-by: user
fix: added an explicit false default to the Enabled property of the Swagger, Hangfire dashboard and dev auth options
lesson: when a safe default carries meaning, state it in the type instead of relying on the language default and an explanation

## SPLIT-001 | p3 backend | 2026-10-10
item: backend backbone: slnx, five layer projects, three test shells, central packages, options with startup validation, feature switches, per-feature registration
who: hybrid
note: AI wrote the files; user decided the shape: Hangfire behind Infrastructure, central packages, test split, per-feature extensions, AutoMapper 14 risk accepted

## ASSESSMENT-001 | p3 backend | 2026-10-10
area: backend scaffolding and dependency setup
helped: high
note: fast on layout, package versions and advisory checks; weaker on machine-specific NuGet fallback and runtime problems, and the first draft needed user pushback on layering

## PROMPT-001 | p3 backend | 2026-10-10
purpose: scaffold the backend from the task documents instead of from model memory
prompt: asked Claude to read the stack and frontend connection from the specs, then create every layer as its own project with NuGet dependencies and versions taken from them
outcome: changed
note: Claude returned an outline with argued options first; user picked slnx, central packages, MediatR 12, STJ, test shells, Swagger option 2, and the pack was built after

## PROMPT-002 | p3 backend | 2026-10-10
purpose: gate Swagger, the job dashboard and mock auth in deployed environments without an environment check
prompt: asked for boolean settings read through the options pattern that switch these on in Azure config, with code setup driven by the options
outcome: kept
note: user supplied the design (booleans plus options) and Claude implemented it; fail-closed defaults and the open dashboard risk were documented afterwards

## SESSION-001 | p3 backend | 2026-10-10
name: backend dev work
phase: p3 backend
note: backend backbone: layers, packages, options validation, feature switches, validation behavior, two code reviews, journal entries CORRECTION-001 to 003

## CORRECTION-004 | p3 backend | 2026-10-10
what: AI code review wrongly claimed editorconfig severity suffixes and a const naming rule would not work in builds; and the earlier enforce-style fix left naming rules (IDE1006) unenforced
caught-by: build
fix: scratch file with deliberate violations showed the review claims were false and naming rules passed silently; added IDE1006 as a warning so bad names now fail the build
lesson: verify AI review claims and enforcement settings with a deliberate failing case instead of trusting either; a rule that never fired is not proof it works

## SUMMARY-002 | p3 backend | 2026-10-10
topic: layer structure and package choices
story: Claude read the required stack from the task documents and proposed five layer projects with argued options for solution format, package management, Swagger and tests. The user chose slnx, central package management, MediatR 12 pinned, three test shells and the first-party OpenAPI generator with the classic UI. The package pass found AutoMapper 14.0.0 flagged by an advisory whose fixes sit behind the commercial licence.
decisions: User kept Hangfire wholly inside Infrastructure, so the API takes no direct package and each layer registers itself by extension methods. AutoMapper stays on 14.0.0 with a suppression for that one advisory and the risk written into the architecture doc.
ai-errors: Claude first placed Hangfire packages on the API project and suggested pinning AutoMapper below 15 without checking the advisory first. The user challenged the layering; the advisory check showed the pin was unsafe.
human-vs-ai: Claude researched versions, drafted options and wrote the projects; the user decided every structural boundary and which risk to accept.
refs: PROMPT-001, SPLIT-001, ASSESSMENT-001, CORRECTION-001, CORRECTION-002

## SUMMARY-003 | p3 backend | 2026-10-10
topic: configuration and feature switches
story: Settings moved to options classes validated at startup, so a missing connection string, signing key or CORS origin stops the app with the key named. The user then asked for boolean switches for Swagger, the job dashboard and the mock auth, read through the options pattern and independent of the environment, so Azure can open them by app setting. Secrets stay in user-secrets locally and app settings or Key Vault when deployed, with setup written in the README.
decisions: Switches default to false, state the default inline, and live in per-feature folders. The dashboard filter allows every request when on, which is documented as a risk. The dev token endpoint is planned as a separate pack behind its own switch.
ai-errors: Claude left the switch default implicit and argued it was enough; the user wanted it written in the type. An earlier fallback to an empty connection string hid missing config until code review flagged it.
human-vs-ai: The user supplied the switch design and the folder layout; Claude implemented, ran the API against LocalDB and checked each endpoint.
refs: PROMPT-002, CORRECTION-003

## SUMMARY-004 | p3 backend | 2026-10-10
topic: build enforcement and verifying AI review
story: Style rules were made to fail the build, then tested with deliberate violations. That showed two review claims were false and that naming rules had never been enforced. The validation pipeline behavior was added with tests tied to the validation rule, after a build error exposed a dropped cancellation token.
decisions: Naming and unused-using rules are warnings treated as errors, confirmed by scratch files. The validation behavior throws for now and will return a failed result once the result type exists.
ai-errors: A second AI code review reported editor-config problems that did not exist, while the real gap was missed; scratch builds disproved the claims and exposed it. The behavior draft also dropped a cancellation token and used the wrong delegate shape.
human-vs-ai: The user asked for the checks and set the bar; Claude ran them. AI review output was treated as a claim to test, not a verdict.
refs: CORRECTION-004
