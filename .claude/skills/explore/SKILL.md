---
name: explore
description: Trace or map how a feature works from the real code, with file:line evidence. Trace (default) answers a pointed question; discovery maps an unknown-scope feature end to end. Triggers - "explore X", "find X", "investigate X", "trace X", "how does X work", "where is X", "look up X", or a claim to verify against the code.
---

# Explore

Investigates how a feature or area works by reading the actual code: entry points, call chains, data transformations, storage, cross-cutting concerns. The result is a factual map of what the code does, grounded in file:line evidence. Never edits files.

-

## Triggers

- trace (default): "find X", "trace X", "investigate X", "where is X", "how does X work", "look up X", a pointed question, or a claim to verify.
- discovery: the literal word "explore", an explicit call, or an unknown or whole-map scope ("map X end to end", "walk the entire flow", "I don't know where this lives"). A bare "how does X work" without a whole-scope signal is trace.
- When unsure between the two, use trace.

-

## Procedure

### Context first

Use what is already in hand before searching:

- conversation context: paths and decisions stated this session, earlier explore or design output;
- `docs/ARCHITECTURE.md`, the file in `docs/layers/` of the layer in question, the local domain documents for a business rule (CLAUDE.md 'Domain sources'; cite by source-qualified ID) and any relevant `.claude/temp/` note.

A named symbol or a documented location is the address; do not re-search it. If the code for the topic does not exist yet, say so and answer from the docs, naming the doc. In discovery keep the labels, with `flow: not implemented (docs: <name>, <rule IDs>)`.

### Claims to verify

Quotes, descriptions and explanations from the user or the team are hypotheses (CLAUDE.md 'Source of truth hierarchy'). Verify each against the code and surface any divergence. When a claim conflicts with the code, say whether it looks like an intended future state, a misremembering or an actual bug, and lead the answer with that divergence.

### Investigate

Run inline with Grep, Glob and Read. Subagents are opt-in: only when the user asks, or when a discovery spans backend and frontend across many files. Then split per CLAUDE.md 'Subagent policy': Haiku search agents (Grep and Glob only, returning file:line:match hit lists, one per area), then one Opus agent that reads the hits and traces the chain.

Search by area:

- API: controllers, middleware, routes
- Application: MediatR command or query, handler, validator, DTO, mapping profile
- Domain: aggregate, child entity, value, closed set, rule, step (operation, transition), input, domain event
- Persistence: DbContext configuration, repository, Unit of Work, migration
- Infrastructure: Blob and email services, Hangfire job definitions
- Frontend: component, service, route, form
- Deployment: `infra/` (Bicep, SQL, scripts), `.github/workflows/`

Flow tracing: open with the entry point (confirm what search found; read the signature and about 20 lines for the trigger and inputs), then follow the call chain hop by hop, reading enough of each next file to confirm what happens; do not guess from names. Name the entities each hop reads or writes and when. Record external calls (Blob, email, HTTP) as boundary hops and do not follow into them.

Zero hits: record a dimension with no results, do not skip it. Try broader patterns first (partial name, alternate casing); if still nothing, say so. If every dimension is empty, check history before concluding the feature does not exist:

```bash
git log --all --oneline --grep="<keyword>" -i
```

Empty output means it was never committed. Outside a git repository, skip the check and add `history not checked` to search gaps.

When a problem is surfaced, frame it per CLAUDE.md 'Argued options': one minimal targeted fix first, then broader alternatives argued if they exist.

-

## Output

Per CLAUDE.md 'Output discipline': only the final report reaches the user. Both modes: no markdown (no bold, headers, dividers), file:line for anything specific, parentheses for brief clarifications, no padding.

**trace:** fluid, no fixed sections. Answer exactly what was asked, then the few file:line that prove it, following only the thread traced. Put chains inline (`A.cs:42 -> B.cs:88 -> Entity`). If the thread forked, name the fork and ask which to follow. No summary or search-gaps sections. If a claim was verified, start with the verdict line (accurate / inaccurate / partially accurate).

**discovery:** structured, each label lowercase plus colon:

- verdict: only when a claim was asked to be verified. One line (accurate / inaccurate / partially accurate), then 2-4 lines on what is true, what is wrong and the key file:line.
- flow: the entry point first (file:line, what triggers it), then one compact line per hop with the data it reads or writes:

    ```text
    entry: ClaimsController.Submit:42 (POST /claims, on form submit)
    SubmitClaimCommandHandler.Handle:30 -> IUnitOfWork.CommitAsync -> Claim
    writes: Claim (insert), ClaimDocument (insert); raises ClaimSubmittedEvent
    ```

- summary: a synthesizing paragraph, not a re-list of the hops: what the feature does overall and the non-obvious things that govern it (validation, auth, domain events, background jobs, config, environment-dependent behavior, parallel paths).
- search gaps: any zero-hit dimension, e.g. `Infrastructure: no hits (searched: BlobService, IBlobStorage)`.

No flat file list and no schema dump. Persist findings to `.claude/temp/explore-<topic>.md` only on request, per CLAUDE.md 'Temp notes'.

-

## Handoff

Explore ends when the code is mapped; the report is the output. Its context can be carried into the design skill; explore never triggers it.

-

## Guardrails

- Read-only: never edit files.
- Investigation ends at system boundaries: record the external API call, the file or blob path and format, the email template and trigger.
- If the feature is too large to trace in one pass, say so and ask which path or area to prioritize.
- Confidential document text stays out of the report (CLAUDE.md 'Domain sources').
- Approval-first actions follow CLAUDE.md 'Permissions model'.
