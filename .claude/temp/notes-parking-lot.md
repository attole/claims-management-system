status: active

Holding area for next steps, topics and proposals that come up outside the current focus. Read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and the layer or area; plain ASCII, no confidential document text. A settled item moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: Frontend - scaffold the Angular app against the finished API (docs/layers/api.md, README 'API'); add frontend/.prettierrc with printWidth equal to the line length in CLAUDE.md; build the three screens with every HTTP call in the typed service layer, errors shown to the user, and the multi-step intake form logic validated by hand
NEXT: API - check by hand once: Swagger UI and its Authorize button, the Hangfire dashboard, an Azure download link with the original file name (the 51 MB limit is now tested on Kestrel)
NEXT: API - add a test that posts a full create body (loss, parties, risk objects and a reserve); the reserve body takes componentId (adjust) or componentType (open), while the specification names one `component` field: confirm the shape with the frontend
NEXT: Persistence - add a SQL Server test for the claim list loss-date filter (untestable on SQLite)
NEXT: Infrastructure - check the Azure blob upload, SAS download with the original name, list by age and delete against a real Azure account before turning Hangfire:BlobSweep:Enabled on (no Azurite tests by choice); the Hangfire tests already run on SQL Server (docs/layers/infrastructure.md 'Tests')
NEXT: Deployment - the CI/CD pipeline and the Azure deployment, every outward action confirmed first (CLAUDE.md 'Permissions model'); until it exists prod is deployed by hand, with the secrets and other Azure-side settings copied manually and nothing relying on CI/CD
NEXT: Deployment - provision the claims_app database role membership for the app user (the second migration creates the role and denies update and delete on ClaimAuditLog); give CI a SQL Server service and set CLAIMS_TEST_SQLSERVER, so an unreachable server fails the run instead of skipping the SQL Server tests
NEXT: Docs - a final pass on README.md, docs/ARCHITECTURE.md and docs/AI-WORKFLOW.md, with the correction section generated from the journal, and the export of the conversation history
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
