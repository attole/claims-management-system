status: active

Holding area for derived topics, next steps and proposals that come up outside the current focus. Not loaded into context by default; read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes' (Parking lot). Items are plain ASCII, one line, with no confidential document text.

ITEMS

NEXT: Add frontend/.prettierrc (printWidth = the line length in CLAUDE.md) when the Angular app is scaffolded.
NEXT: dev token endpoint (option A): DevAuth:Enabled flag, server-side mock users, Infrastructure token service behind Application interface, DevAuthController POST /api/dev/token; flag on in Azure, documented as simulation
PROPOSAL: shared base for the soft-delete block (IsDeleted, DeletedAt) if its six copies become a burden (notes-layer-persistence.md)
TOPIC: check indexes on columns inside a complex type before relying on one (notes-layer-persistence.md)
NEXT: docs update - state in ARCHITECTURE that business rules stay in the Domain only and FluentValidation checks request shape only (assessment says rules also as validators); explain for the live review
NEXT: docs update - state that primary keys are sequential GUIDs generated in the Domain, and that Persistence also declares the NEWSEQUENTIALID() default expression to match spec 15.2
NEXT: Infrastructure sweep that removes blobs no document row refers to (an uploaded blob is stored before the commit, so a failed commit leaves an orphan)
