status: active

Holding area for derived topics, next steps and proposals that come up outside the current focus. Not loaded into context by default; read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and then the layer or area it belongs to; plain ASCII, no confidential document text. An item that is settled moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: Frontend - add frontend/.prettierrc with printWidth equal to the line length in CLAUDE.md when the Angular app is scaffolded
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
NEXT: API pack order (docs/layers/api.md has the rules): 1 host and identity (built); 2 errors, envelopes and the Idempotency-Key filter (built); 3 claims list and reference data; 4 intake (POST claims); 5 claim detail (detail, status, status-check with CheckClaimStatusQuery and a shared runner map, parties, notes, audit); 6 reserves and documents (approve, reject, retract, retry posting, upload with the 50 MB limit, local download); 7 hardening (Swagger bearer and note, reflection tests, README); the frontend can start after pack 4
NEXT: Frontend - build the three screens against the backend: errors surface to the user and no HTTP call leaves the typed service layer; the multi-step intake form logic is validated by hand
NEXT: Deployment - the CI/CD pipeline and the Azure deployment; every outward action is confirmed first (CLAUDE.md 'Permissions model')
NEXT: Docs - a final pass on README.md, docs/ARCHITECTURE.md and docs/AI-WORKFLOW.md, with the correction section generated from the journal, and the export of the conversation history
NEXT: Deployment - provision the claims_app database role membership for the app user (the second migration creates the role and denies update and delete on ClaimAuditLog); give CI a SQL Server service and CLAIMS_TEST_SQLSERVER, otherwise the SQL Server test set skips silently
NEXT: Persistence - add a SQL Server test for the claim list loss-date filter (untestable on SQLite)
NEXT: Infrastructure - run the Azure blob list and delete and the Hangfire jobs against Azurite or a real account and a real Hangfire database before turning Hangfire:BlobSweep:Enabled on in Azure (docs/layers/infrastructure.md 'Jobs')
NEXT: API pack 6 - the upload action sets its request size and form limits with attributes (authorization-stage filters, so they apply before the Idempotency-Key filter reads the form); a test must prove a body just under 50 MB reaches the action with the key header sent (docs/layers/api.md 'Idempotency-Key')
NEXT: API pack 5 (reserves and documents), then pack 6 (hardening); packs 4 and 5 of the old plan were merged into the claim routes pack
