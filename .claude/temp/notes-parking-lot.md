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
PROPOSAL: API auth option B (alternative to the dev token option A above): DevTokenIssuer and DevAuthController in API/Configuration/DevAuth next to Jwt, JwtOptions stays in the API, no Application port, no Infrastructure token service; HttpCurrentUser (scoped ICurrentUser from HttpContext.User, claims sub/org/role), users and the seed organisation id under DevAuth config; DevAuthOptions moves out of Infrastructure; the same pack registers IClock and ICorrelationContext (nothing registers them yet). Undecided: option A or B, then the pack is built.
NEXT: API - register ICurrentUser, IClock and ICorrelationContext (nothing registers them yet, so the app cannot resolve ClaimsDbContext); the dev auth organisation id must equal the seed organisation (Persistence Schema/Seed/SeedIds, derived from the name organisation)
NEXT: API - reject an Idempotency-Key longer than 200 characters (the IdempotencyKeys column length)
NEXT: Application - the tenancy rule for job commands (jobs have no organisation; the SLA reader already spans organisations, the other job commands need a rule)
NEXT: Infrastructure - the cleanup job that deletes expired IdempotencyKeys rows (docs/layers/infrastructure.md)
NEXT: Deployment - provision the claims_app database role membership for the app user (the second migration creates the role and denies update and delete on ClaimAuditLog); give CI a SQL Server service and CLAIMS_TEST_SQLSERVER, otherwise the SQL Server test set skips silently
NEXT: Persistence - add a SQL Server test for the claim list loss-date filter (untestable on SQLite)
NEXT: infrastructure packs: 1 clock and storage (Azure + local with signed download token), 2 posting scheduler and GL job, 3 SLA and posting sweep jobs with registration, 4 idempotency cleanup (needs Application purge command + Persistence method + job). Pack 2 to 4 blocked by the job tenancy rule.
NEXT: API pack: local download endpoint validates LocalDownloadToken and streams the file; static files are not used.
