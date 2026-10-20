---
name: design
description: Work through a feature or integration design - understand what to build, then produce a code-level design. Never writes code. Triggers - "design X", "how should we build X".
---

# Design

Understand WHAT to build, then HOW to code it. Produces a code-level design and never writes code (CLAUDE.md 'File edits & action gating': an approved plan is not permission to write code).

-

## Triggers

- "design X", "how should we build X".
- Pinning down a harder spec before any implementation.

-

## Procedure

### Working style

- Fluid, not a fixed pipeline. Follow the user's lead and iterate between Understand and Design. Respond to the current input directly; undecided areas become `[?]` blocks instead of blocking the design.
- No summary sections in chat ("confirmed so far", "open items"). State settled things in their section and ask about open things inline. Running state is held in working memory and persisted only on request (see Notes).
- Several topics at once: do not sweep them shallowly. (1) Quick trace, 2-3 sentences, naming every topic that needs settling. (2) Take the most decision-critical topic and go deep (3-5 lines of analysis, one specific inline question). (3) Signal "next up: [topic]". Each later message continues one more topic the same way.

### Sources

- Business rules and product decisions come from the user, never from inference or code patterns (CLAUDE.md 'Source of truth hierarchy'). The code shows what exists; the user says what should happen.
- "I'm not sure" is recorded as open, not resolved with a pattern. New input that contradicts earlier input is surfaced: state both versions, ask which is correct.
- Read the local domain documents (CLAUDE.md 'Domain sources'), `docs/ARCHITECTURE.md` and the file in `docs/layers/` of the layer being designed first. Extract settled decisions; do not re-ask what they answer. Cite rules by source-qualified ID and paraphrase confidential text. Where the documents conflict, surface a `[?]` block.

### Order of work

The user decides the order and normally starts with the domain. Do not impose a sequence. Only when a request skips a dependency (application code before the domain model it needs, an API before its handlers, migrations before the entity configuration, a frontend service before the endpoint contract), say so in one line, name what is missing, and let the user decide. If the user already chose to skip it, proceed and mark the assumed shape as `[?]`.

### Understand

- **Business rules:** extract confirmed decisions verbatim, then ask only what is genuinely unresolved: one specific question per topic, embedded where the topic appears. When a topic looks settled on the happy path, probe one level deeper: fail mid-flow (recoverable?), fire twice (duplicate handling?), scope edges (thresholds, dates, status transitions), interactions with notifications, documents, and background jobs.
- **Analogue:** in a young repo it is `docs/ARCHITECTURE.md` plus the previous pack. For gaps those do not cover, use the explore skill to find the closest live code and its save path. Report factual pointers ("X is similar to Y, which uses Z, reusable at path:line"). Flag a file untouched for a long time with few callers as possibly stale and ask.
- **External service** (Azure Blob, email, Hangfire storage): start from what the user provided; research fills gaps, it does not confirm. Cover auth model, operations needed, failure behavior, retry and idempotency, SDK version and last publish date, limits. Cap WebFetch at about 2 attempts per URL; note unreachable sources as unknown. Whether and how to use a capability is the user's decision.

### Design

Assess clarity first:

- fully clear -> full design;
- partially unclear -> design the decided parts in full, `[?]` blocks for undecided areas, blockers in Open;
- core logic undecided -> output only what is unclear: a `[?]` flow with options argued comparatively, and Open. Stop.

**Conflicts and undecided architecture.** When a decision conflicts with a convention or has two valid approaches, surface a `[?]` block per CLAUDE.md 'Status notation', each option argued per CLAUDE.md 'Argued options', then a recommendation. On a true convention conflict, stop and wait. Check these spots: where a rule lives (Domain vs Application), command vs query boundary, validator placement, result and error mapping, DTO shape, event handling (in-process vs via a job), any deviation from the analogue.

**Design lenses.** Check silently; surface only what is non-obvious or needs a decision: Unit of Work scope (one transaction? state on failure?); idempotency (called twice? Hangfire retry?); error recovery (partial write? does the caller know?); auth enforcement; domain events (raised at the right transition, handled where); downstream side effects (documents, emails, jobs, correctly gated); EF query shape (tracking, N+1, indexes); null policy in the model (CLAUDE.md 'No null').

-

## Output

Commit to ONE architecture for the decided parts; undecided areas appear as `[?]` blocks inside the section they affect. Markdown per CLAUDE.md 'Output discipline' and 'Output formatting': feature name as `#`, the sections below as `##`, `###` for packs and flow sub-labels, backticked identifiers, file:line for every analogue cited. Model, flow, and build sequence use CLAUDE.md 'Status notation'.

- **Context:** 2-4 sentences: what is built, the core flow, the constraints that shape it, what is out of scope.
- **Reference:** the analogue files as file:line, each with its one-line role. If there is no analogue, write `none (<reason>)`.
- **Model:** entities, value objects, DTOs, events: field names, types, nullability (new vs reused).
- **Flow + components:** entry, handler, domain, persistence, top to bottom in execution order, one file per line with a one-line role; include error paths and what each result means; parallel flows (events, jobs, admin) get separate labeled blocks.
- **Build sequence:** the work grouped into packs (CLAUDE.md 'File edits & action gating'), in dependency order as a suggestion the user may reorder. Names of planned changes only, no inline code; group by file. Each pack is a stop point for implementation. Pack heading: `### Pack N - <name>`.
- **Critical details:** what breaks silently if missed (enum values, config keys, DI registration, migration).
- **Open:** specific questions and specific actions.
- **Verification:** happy-path smoke test; fire-twice idempotency check; one meaningful negative path.

### Notes

Persist only on request, to `.claude/temp/design-<topic>.md` per CLAUDE.md 'Temp notes':

```text
status: active
ANALOGUE:     existing files and how they work now (file:line) - verified
PROVISIONAL:  model / flow / build sequence to implement
DECISIONS:    settled architectural and product choices (option + reason)
OPEN:         unresolved questions, including known pitfalls not yet covered by a decision
NEXT:         next steps
```

Verified ANALOGUE facts and settled DECISIONS graduate to `docs/ARCHITECTURE.md` or the layer's file in `docs/layers/`. PROVISIONAL stays provisional until built. Prune resolved lines.

-

## Guardrails

- Never write code; the design names changes, it does not contain them.
- Never infer a business rule; ask.
- Confidential document text stays out of the design and the notes (CLAUDE.md 'Domain sources').
- Do not impose an order of work; only flag a skipped dependency.
- Approval-first actions follow CLAUDE.md 'Permissions model'.
