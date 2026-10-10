---
name: ai-journal
model: sonnet
description: Record real AI-workflow events (prompts, AI mistakes and fixes, AI/manual split, assessments, sessions) into the structured journal, merging related events into topic-level entries and summaries. Triggers - "log prompt", "log correction", "log split", "log assessment", "log session", "log summary", "summarize journal", "journal this", "list journal", "compile journal".
---

# AI Journal

Appends structured entries to `.claude/temp/notes-ai-journal.md`, the raw material for the final `docs/AI-WORKFLOW.md`. Related events are merged into topic-level entries and SUMMARY narratives, not logged as a run of small items. The journal file defines the entry format in its FORMAT block; this skill follows it and never restates or varies it. Capture broadly: compile filters later, so when in doubt, log.

-

## Triggers

- Log: "log prompt", "log correction", "log split", "log assessment", "log session", "log summary", "summarize journal", "journal this". The phrase authorizes the write.
- List: "list journal".
- Compile: "compile journal" (end of project).
- Proactive: when a signal below appears, offer ONE draft entry as a single line and wait for yes/no before writing anything. At most one offer per turn: when several signals apply, offer the highest-value type (CORRECTION first). A declined offer is not repeated for the same event.
    - a review or rule check reported a defect in code Claude wrote;
    - the user says Claude's output was wrong, off-target, or not what was asked, and a fix follows;
    - a build or test failure traces back to Claude-generated code and gets fixed;
    - a prompt, design, or option set produced an unusually good or bad result;
    - the user rejected a recommended option, or reversed an earlier decision;
    - the user asked Claude to explain generated code during review, and the explanation changed the decision or the code;
    - a rule, skill, or setting was changed because of an AI problem;
    - a decision about how AI is used on this project was made.

-

## Procedure

### Log

1. Read the FORMAT block at the top of the journal. Determine the type from the trigger or the signal.
2. Think in topics, not events. Merging is the normal behavior, not an exception:
    - Group the session's events by topic or pack (a design decision, a layer, an enforcement setting) before drafting anything. Several small events on one topic become one entry that tells the arc: what was decided, what the AI got wrong, how it was caught, what changed.
    - A trigger that names a type ("log correction") covers every related event of that type in the session, not only the last one. Draft one entry per topic, and say which events it merged.
    - Do not write a run of tiny entries for one story. A bug, its cause, and the rule it produced are one CORRECTION; a prompt and the design it led to are one PROMPT; a whole topic with several types of event is a SUMMARY.
    - Keep apart what is genuinely separate: unrelated lessons, different phases, different people deciding.
    - Raw entries stay short evidence; the SUMMARY is the narrative that cites them (`refs`). Prefer a SUMMARY at the end of a pack or topic, and offer one whenever three or more related raw entries exist without a summary.
3. Draft the entry from the session: only what actually happened. Never invent, soften, or embellish a mistake or a success. If a required value is unknown, ask one short question; do not guess.
4. Number it: scan existing headers for the highest NNN of that type and add one.
5. Confidentiality check before writing:
    - No text copied from `docs/*.local.md`. Strip punctuation from each value, take its 6-word windows (a value shorter than 6 words is checked whole), and run `grep -F -i -c -e "<window>" docs/spec.local.md docs/assessment.local.md`; any hit means rephrase and recheck. Cite rules by source-qualified ID only (CLAUDE.md 'Domain sources').
    - No secrets, tokens, connection strings, or personal data.
    - Prompt values are trimmed or paraphrased; if the prompt contained pasted document text, record a paraphrase of the intent.
6. Append the block at the end of the file. The file is append-only: never reorder, renumber, or rewrite earlier entries. An edit to an existing entry happens only when the user asks, one entry at a time, keeping its ID. Merging means drafting a new consolidated entry that cites the raw ones; it never rewrites them.
7. Run the workflow check (CLAUDE.md 'Workflow check') and fix any journal finding before emitting the Output line.

What makes a good entry, by type:

- PROMPT: prompts that show context loading, sequencing, or a clear instruction style. `purpose` says what the prompt was for; `outcome` records kept, changed, or dropped.
- CORRECTION: the most valuable type. `what` names the wrong or weak output concretely; `caught-by` names who or what found it; `fix` says what changed; `lesson` is the transferable point (and names the rule added to CLAUDE.md, if any).
- SPLIT: meaningful units (a pack, a document, a form), not individual files.
- ASSESSMENT: an area where AI helped notably more or less than expected.
- SESSION: one entry per named Claude Code session, so the conversation history can be located and exported.
- SUMMARY: the most useful type for the final report. `topic` names the subject; `story` is a short narrative of what was built or decided and why; `decisions` lists the choices and who made them; `ai-errors` says what the AI got wrong and how it was caught; `human-vs-ai` says who drove what; `refs` lists the raw entry IDs it covers (`none` if there are none). Write for a reviewer who has not seen the session: a few full sentences per field, concrete names and outcomes, no event-by-event listing.

### List

Run `grep -n "^## " .claude/temp/notes-ai-journal.md` and print each header with its first value line, in the List format under Output.

### Compile

Read the whole journal and draft, in the user's first person and plain voice, only what the entries support:

- the narrative backbone of `docs/AI-WORKFLOW.md`, built from the SUMMARY entries (one section per topic), with the raw entries they cite as evidence;
- the "AI correction journal" section of `docs/AI-WORKFLOW.md`, built from the CORRECTION entries (each with how it was caught and fixed), grouped by topic and dropping trivial ones;
- optional lists of representative prompts (PROMPT), the AI/manual split (SPLIT), the honest assessment (ASSESSMENT), and the sessions to export (SESSION).

Do not draft or overwrite the user's hand-written sections. Flag any list the journal cannot support yet instead of filling it. The user edits and owns the result.

-

## Output

Per CLAUDE.md 'Output discipline'; the procedure runs inline per CLAUDE.md 'Subagent policy'. Emit only:

- Log: `logged: <TYPE>-<NNN> (<short title>)`, one line per entry written; a merged entry adds ` merges <ids>` when it covers several events
- List: the entry headers, one per line, each followed by ` - <first value of the entry>`
- Compile: the drafted sections as markdown, in chat (written to a file only when asked)
- Proactive offer: one line, `journal? <TYPE>: <short title>`, and nothing else. It is always the last line of the turn, after any other skill output.

-

## Guardrails

- Append-only, and only real events.
- Confidential document text and secrets stay out of entries (CLAUDE.md 'Domain sources').
- Do not commit; the user commits the journal like any other temp note.
- Approval-first actions follow CLAUDE.md 'Permissions model'.
