status: active

Holding area for derived topics, next steps and proposals that come up outside the current focus. Not loaded into context by default; read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and then the layer or area it belongs to; plain ASCII, no confidential document text. An item that is settled moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: Frontend - add frontend/.prettierrc with printWidth equal to the line length in CLAUDE.md when the Angular app is scaffolded
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
NEXT: Frontend - build the three screens against the backend: errors surface to the user and no HTTP call leaves the typed service layer; the multi-step intake form logic is validated by hand
NEXT: Deployment - the CI/CD pipeline and the Azure deployment; every outward action is confirmed first (CLAUDE.md 'Permissions model')
NEXT: Docs - a final pass on README.md, docs/ARCHITECTURE.md and docs/AI-WORKFLOW.md, with the correction section generated from the journal, and the export of the conversation history
NEXT: Deployment - provision the claims_app database role membership for the app user (the second migration creates the role and denies update and delete on ClaimAuditLog); give CI a SQL Server service and CLAIMS_TEST_SQLSERVER, otherwise the SQL Server test set skips silently
NEXT: Persistence - add a SQL Server test for the claim list loss-date filter (untestable on SQLite)
NEXT: Infrastructure - run the Azure blob list and delete and the Hangfire jobs against Azurite or a real account and a real Hangfire database before turning Hangfire:BlobSweep:Enabled on in Azure (docs/layers/infrastructure.md 'Jobs')
NEXT: API pack 6 (hardening): reflection tests (no [Authorize(Roles)], every action authenticated except the three anonymous routes, every write covered by the idempotency filter, one 2xx per ApiControllerBase action with CommandResponse for writes, route naming allow-list), Swagger bearer and note, README; the frontend can start now
NEXT: API pack 6 - add a test helper that builds an open claim with a linked policy and a pending reserve, then API tests for reserve submit (201 with approval status) and approve, reject, retract success and role refusals (S:API-POST-reserves, S:API-POST-reserve-approve, S:API-POST-reserve-reject, S:API-POST-reserve-retract)
