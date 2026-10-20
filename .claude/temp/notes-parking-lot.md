status: active

Holding area for derived topics, next steps and proposals that come up outside the current focus. Not loaded into context by default; read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and then the layer or area it belongs to; plain ASCII, no confidential document text. An item that is settled moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: Frontend - add frontend/.prettierrc with printWidth equal to the line length in CLAUDE.md when the Angular app is scaffolded
NEXT: API - dev token endpoint (option A): DevAuth:Enabled flag, server-side mock users, an Infrastructure token service behind an Application interface, DevAuthController POST /api/dev/token; flag on in Azure, documented as a simulation (docs/layers/api.md 'Identity and correlation')
NEXT: Infrastructure - sweep that removes blobs no document row refers to; a blob is stored before the commit, so a failed commit leaves an orphan (docs/layers/infrastructure.md 'Storage adapter')
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
NEXT: Frontend - build the three screens against the backend: errors surface to the user and no HTTP call leaves the typed service layer; the multi-step intake form logic is validated by hand
NEXT: Deployment - the CI/CD pipeline and the Azure deployment; every outward action is confirmed first (CLAUDE.md 'Permissions model')
NEXT: Docs - a final pass on README.md, docs/ARCHITECTURE.md and docs/AI-WORKFLOW.md, with the correction section generated from the journal, and the export of the conversation history
NEXT: Persistence - add the SQL Server integration set (counter, concurrency, idempotency key, migration) after the Persistence packs; SQLite carries the tests until then (docs/layers/persistence.md)
NEXT: replace the raw SQL of ClaimListRowConfiguration (keyless ToSqlQuery over Claims, LossEvents, Policies) with a mapped SQL view created in the migration pack; the SQL names tables and columns as strings and is untested on SQL Server
