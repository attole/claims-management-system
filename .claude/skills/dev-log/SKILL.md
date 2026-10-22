---
name: dev-log
description: Add, check, or remove temporary dev logs that trace a flow. User-invoked only - /dev-log on, /dev-log check, /dev-log off.
disable-model-invocation: true
---

# Dev Log

Adds temporary log lines through a flow so real execution can be traced, then removes them in one pass. Every dev log carries the literal marker `DEV-LOG`, so one grep finds them all. The marker in the code is the only record; nothing else is tracked.

-

## Triggers

User-invoked only (the skill is not model-invoked):

- ON: `/dev-log on`
- CHECK: `/dev-log check`
- OFF: `/dev-log off`

-

## Procedure

### Marker convention

One marker, inside the log message string, never varied:

```text
DEV-LOG:<topic> <Name>: field1=val1 field2=val2 id=...
```

- `<topic>` is the kebab-case feature slug (e.g. `claim-submission`). Resolve in order: the user's message, the current branch name (`git branch --show-current`), else ask.
- The `DEV-LOG:<topic>` token uses letters, digits, hyphen and colon only: no brackets, no variants (`DBG`, `[DEV-LOG]`).
- Always include the primary record ID so entries correlate.
- `grep DEV-LOG` finds every log; `DEV-LOG:claim-submission` scopes to one topic. A multi-line call is found from the line holding the marker, so no trailing comment marker is needed.
- No comments are added alongside; the marker string is the machinery (CLAUDE.md 'No explanatory comments').

### Logger

Match what the surrounding code already uses; never introduce a different logger.

- **Backend C#:** the `ILogger<T>` already injected in the class, under its existing name (`_logger` or a primary-constructor `logger`); add it to the constructor only if the class has none. Use `LogInformation` with a structured template: `_logger.LogInformation("DEV-LOG:claim-submission SubmitClaim entry: claimId={ClaimId}", claimId);`. Never `Console.WriteLine`, never a file sink.
- **Angular:** `console.log("DEV-LOG:claim-submission ...", value)`.

### Search

The authoritative list of what exists is one marker search, excluding build output:

```bash
grep -rn "DEV-LOG" backend frontend/src --include=*.cs --include=*.ts --include=*.html 2>/dev/null
```

A hit is the line holding the marker; for a multi-line call the statement starts on an earlier line.

### Add logs

1. Resolve the topic (ask if none).
2. Identify the flow to trace; if unclear, ask which feature or files.
3. Read each file, reuse its logging pattern, and add one line per significant transition, same shape everywhere:
    - entry point: trigger and key inputs (controller action, MediatR handler start)
    - validation outcome, and each branch decision with the value that decided it
    - domain event raised
    - persistence: what was saved, with IDs (after the Unit of Work commit)
    - external calls: inputs before, response fields after (Blob, email)
    - Hangfire job: enqueued, started, finished, with the job input

### Check logs

1. Backend: if Claude started the app as a background process, read its output filtered on `DEV-LOG:<topic>`, newest first. Otherwise ask the user to paste the console output.
2. Frontend: Claude cannot read browser devtools; ask the user to paste the console lines.
3. Correlate rows by the record ID. No rows means the path did not run or did not reach that code.

### Remove logs

1. Resolve the topic. With none given and several present, list them from the search and ask.
2. Run the search. No topic given: clean all matches (or list files and counts and ask which).
3. In each matched file, delete every `DEV-LOG` statement. For a multi-line call delete the whole statement, from the call start to its terminating `;`, plus any blank line added only to set off a dev-log block. Remove an `ILogger` constructor addition made only for dev logs.
4. **Branch sweep** (a safety net for a forgotten marker): read the branch's added lines with `git diff origin/<base>...HEAD --unified=0` and `git diff --unified=0` (`<base>` is the default branch; drop `origin/` when there is no remote; skip the sweep outside a git repository). Delete only `+` lines carrying `DEV-LOG` that step 3 missed. Untagged `LogInformation` or `console.log(` calls added on the branch are candidates only: list them and ask before removing any.
5. Re-run the search and confirm zero hits.

-

## Output

Per CLAUDE.md 'Output discipline': the status line is the entire turn; the procedure runs inline (CLAUDE.md 'Subagent policy'). Emit only:

- ON: `N dev logs added in M file(s)` plus one line saying where they surface (backend console output, or browser console).
- CHECK: the matching rows, or `no rows - path didn't run`.
- OFF: `N dev logs removed from M file(s)`, or `no dev logs`. A second line only for an anomaly (untagged candidates to confirm, or a non-zero re-grep).

-

## Guardrails

- Git writes stay with the user (CLAUDE.md 'Shell & git'); run OFF before dev logs can be committed.
- Dev logs go to the console only, never to a file or data store. They are temporary and exempt from the line length in CLAUDE.md 'Code style'.
- Never log secrets, tokens or personal data; log IDs and non-sensitive state.
- Approval-first actions follow CLAUDE.md 'Permissions model'.
