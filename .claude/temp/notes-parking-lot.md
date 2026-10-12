status: active

Holding area for derived topics, next steps and proposals that come up outside the current focus. Not loaded into context by default; read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

- One line per item, appended at the end of the file with `printf '%s\n' 'TAG: text' >> .claude/temp/notes-parking-lot.md` (single quotes), without reading the file first. The file must end with a newline.
- Each line starts with an uppercase tag, a colon and a space: `TOPIC: ` (side topic worth discussing), `NEXT: ` (follow-up step), or `PROPOSAL: ` (idea or alternative raised outside the current focus).
- Plain ASCII, a single line, no confidential document text.
- When an item is taken up, move it into an existing design or notes note (or ask first) and delete its line here with an exact-match delete. Deleting taken-up lines is the only allowed edit.

ITEMS

NEXT: Add frontend/.prettierrc (printWidth 120) when the Angular app is scaffolded.
NEXT: dev token endpoint (option A): DevAuth:Enabled flag, server-side mock users, Infrastructure token service behind Application interface, DevAuthController POST /api/dev/token; flag on in Azure, documented as simulation
NEXT: decide unknown or already removed party and risk object ids before the first remove command (notes-layer-application.md)
NEXT: add the spec-message allow-list to .claude/tools/check_workflow.py before the first verbatim message string is written
PROPOSAL: shared base class for the audit, delete and tenant property blocks if the four copies become a burden (notes-layer-persistence.md)
TOPIC: check indexes on columns inside a complex type before relying on one (notes-layer-persistence.md)
