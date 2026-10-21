status: active

Holding area for next steps, topics and proposals that come up outside the current focus. Read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and the layer or area; plain ASCII, no confidential document text. A settled item moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: API - check by hand once: Swagger UI and its Authorize button, the Hangfire dashboard, an Azure download link with the original file name (the 51 MB limit is now tested on Kestrel)
NEXT: Infrastructure - check the Azure blob upload, SAS download with the original name, list by age and delete against a real Azure account before turning Hangfire:BlobSweep:Enabled on (no Azurite tests by choice); the Hangfire tests already run on SQL Server (docs/layers/infrastructure.md 'Tests')
NEXT: Deployment - run infra/bootstrap.md (every az and gh write asks first), then the Deployment workflow; the Bicep, the workflows and the frontend upload have not run against Azure yet, so check the first run's what-if, the static web app upload and the smoke test closely
NEXT: Docs - review and edit the compiled AI-WORKFLOW correction section; export the conversation history
NEXT: Tooling - the .NET 9 runtime is no longer installed on this machine (only 5, 6 and 10); reinstall it, or the backend tests need DOTNET_ROLL_FORWARD=Major to run
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
TOPIC: claim intake raises transient VALIDATION_ISSUE_ADDED audit rows (about six for a complete claim) because each assembly step re-evaluates the issues; evaluate once at the end of intake.
