---
name: spec-check
model: sonnet
description: Check code against the business rules in the local domain documents, passively after a code review or on demand for coverage. Read-only except build. Triggers - "spec-check", "check rules", "rule coverage", "check <rule ID>", "spec-check build"; also invoked by the code-review skill.
---

# Spec Check

Verifies that business rules from the local domain documents are enforced and tested in code, and that code does not invent rules the documents do not contain. A diff review cannot see what is missing; this skill can. Coverage is not correctness: a rule marked covered still needs the code to be right, which is why deviations are judged against the rule text.

-

## Triggers

- Passive: runs after a code-review, which passes its changed-file list (the default mode).
- Coverage: "spec-check", "check rules", "rule coverage".
- Check: "check S:BR-R-03" (any rule ID).
- Build: "spec-check build".

-

## Procedure

### Rule sources

1. **Register (preferred):** `docs/rules.local.md`, a compact index of every rule. Gitignored (`*.local.md`) because it holds paraphrased rule content. One rule per line:

    ```text
    <ID> | <area> | <paraphrase, 15 words or fewer> | <spec|assessment> <section> | <flags>
    S:BR-R-03 | reserve | a user cannot approve their own reserve | spec 7.2 |
    S:ST-Open-Closed | status | closing needs every closure condition met | spec 4.2 | derived
    A:BR-R-01 | reserve | reserve amount must be positive, no exception stated | assessment 3.4.2 | conflict:S:BR-R-01
    ```

    - ID: source-qualified per CLAUDE.md 'Domain sources' (`S:` spec, `A:` assessment). Rules with no ID in the document (in either source) get a stable derived ID `<S|A>:<KIND>-<Name>` and the flag `derived`, for example transitions `S:ST-<From>-<To>`, validation rows `S:V-<Field>-<Aspect>`, endpoints `S:API-<VERB>-<route>`. Closure conditions and other numbered rules keep their own IDs.
    - area: claim, party, status, reserve, job, document, audit, validation, api, data, ui.
    - flags: `derived`; `conflict:<other ID>` where the documents disagree (listed on both rules).
2. **Originals:** `docs/spec.local.md` and `docs/assessment.local.md`. Open ONLY the section a register line points to, to judge a deviation. Never read a whole document for a passive run.

If the register is missing, a passive run does nothing. An explicit run may fall back to grepping the originals for the area at hand.

### Tying code to rules

Per CLAUDE.md 'No explanatory comments', rule IDs ride on code, not comments:

- tests: `[Trait("Rule", "S:BR-R-03")]`
- validators: `.WithErrorCode("S:BR-R-03")`
- Domain rules: the ID as the rule's `Code` constant

Enforcement search: grep for the ID first, then search semantically (rule, step, entity, command and validator names in the area) when no ID is present. A missing ID is a finding only for rules that are otherwise enforced.

### Passive mode

1. Scope: the changed-file list passed by the caller (code-review passes its Collect list).
2. Map changed files and symbols to register areas by path and name (Reserve, Claim, Status, Party, Document, Audit, Sla/Job, validators, controllers, Angular screens). Select the register rules for those areas, narrowed to rules a changed symbol plausibly implements. No matching area: emit nothing.
3. For each rule in scope, locate enforcement and tests. Open the rule's source section only when the code and the paraphrase appear to disagree.
4. Judge and report only what is not clean. When more than 8 rules are in scope, hand the judgment to one fresh subagent with `model: sonnet`; otherwise run inline (also when no subagent can be spawned; the output is unchanged). This skill never uses Opus (CLAUDE.md 'Subagent policy').

### Coverage mode

Whole-system matrix: every register rule with where it is enforced, where it is tested, and a verdict. Group by area.

### Check mode

Deep verification of one rule: read the source section, find the enforcement, and test the implementation against the rule text including edge cases (boundaries, repeated calls, role, state).

### Build mode

Create or refresh `docs/rules.local.md` from the two local documents; the phrase authorizes this write. If the register exists, refresh in place: keep existing derived IDs stable, then report counts added, changed, and removed. Mark documented disagreements with `conflict:` on both rules. Then run the workflow check (CLAUDE.md 'Workflow check') and fix register findings.

### Verdicts

- `missing` - no enforcement found for a rule that applies to the changed code.
- `untested` - enforced, but no test carries the rule (a test without the rule's `Rule` trait does not count) or covers it. In passive mode, reported only when the enforcing code is in the change.
- `deviates` - enforcement exists but differs from the rule (threshold, role, state, boundary, severity, error shape).
- `conflict` - the documents disagree; report both IDs and the difference, do not pick a side.
- `orphan` - changed code implements a business threshold, condition, status, or role check that no register rule supports. This is an invented rule: ask the user for the source.

-

## Output

Per CLAUDE.md 'Output discipline'. Findings are paraphrased, never quoted; the register's paraphrase is the most that may appear.

Printed as plain text, never inside a code block. Passive, appended after the review findings:

rules - <areas checked>
<rule ID> - <verdict>: <paraphrased problem>. Fix: <one-line fix>.

- An invented rule has no rule ID: `orphan - <file>:<line>: <condition>. Fix: ask the user for the source.`
- A conflict names both IDs: `<ID> / <ID> - conflict: <difference>. Fix: ask the user.`
- Rules in scope and all covered: one line, `rules: <N> in scope, covered`.
- Nothing in scope: no output.
- Coverage and Check: one line per rule, `<ID> | enforced: <file:Symbol> | tested: <file:Symbol> | <verdict>`, then orphans and conflicts. Saved to `.claude/temp/review-rule-coverage.md` only on request, IDs and verdicts only, no rule text.

-

## Guardrails

- Read-only, except Build writing `docs/rules.local.md`.
- Confidential text stays inside `*.local.md` files (CLAUDE.md 'Domain sources').
- Never resolve a document conflict and never invent a rule: a missing or doubtful rule goes to the user (CLAUDE.md 'Conflict, uncertainty & sources of truth').
- Grading and recruitment content is not a rule source; only business rules are.
- Approval-first actions follow CLAUDE.md 'Permissions model'.
