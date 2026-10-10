status: active

Running record of how AI is used on this project: prompts worth showing, AI mistakes and their fixes, the AI/manual split, and an honest assessment. Raw material for the final AI-WORKFLOW.md. Written by the ai-journal skill (.claude/skills/ai-journal/SKILL.md); to be reviewed and turned into the final document at the end.

FORMAT

Append-only. One entry per block, newest last. The entry headers are the map: `grep "^## " .claude/temp/notes-ai-journal.md` lists every entry.

Header line: `## <TYPE>-<NNN> | <phase> | <YYYY-MM-DD>`
- TYPE is one of PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION. NNN is a 3-digit counter per type, never reused.
- phase is the full label: p0 setup, p1 domain, p2 data, p3 backend, p4 frontend, p5 review (for example `p3 backend`).
- Entries are separated by one blank line. If the phase is unknown, ask.

Then fixed `key: value` lines, one per line, each value a single line of at most 200 characters, no blank lines inside an entry:
- PROMPT: purpose, prompt, outcome (kept | changed | dropped), note
- CORRECTION: what, caught-by (user | code-review | spec-check | test | build), fix, lesson
- SPLIT: item, who (ai | manual | hybrid), note
- ASSESSMENT: area, helped (high | medium | low), note
- SESSION: name, phase, note

Rules:
- Capture broadly; when in doubt, log. The final document is compiled and filtered later.
- Only real events. Claude drafts, the user confirms; nothing is invented or embellished.
- Paraphrase. Never copy text from the confidential local documents (docs/*.local.md), never include secrets, tokens, or connection strings. Rules are referenced by source-qualified ID only.
- Prompt values are the user's prompt trimmed or paraphrased, never pasted document text.

ENTRIES

