status: active

Holding area for next steps, topics and proposals that come up outside the current focus. Read it only when asked ("open parking lot"), then show the non-empty tag groups.

FORMAT

The append command, the tags and the removal rule are in CLAUDE.md 'Temp notes'. One item per line, starting with its tag (`TOPIC:`, `NEXT:` or `PROPOSAL:`) and the layer or area; plain ASCII, no confidential document text. A settled item moves to `docs/ARCHITECTURE.md` or the layer file in `docs/layers/` and is removed here.

ITEMS

NEXT: API - check by hand once: the Hangfire dashboard and an Azure download link with the original file name (Swagger runs on the deployed demo)
NEXT: Infrastructure - check the Azure blob list by age and delete against the real account before turning Hangfire:BlobSweep:Enabled on (upload and SAS download already run through the demo seed; no Azurite tests by choice)
NEXT: Deployment - the frontend still has no screen to assign a handler to an existing claim; decide on Application Insights
NEXT: Docs - review and edit the compiled AI-WORKFLOW correction section; export the conversation history
TOPIC: Live review - be ready to explain why business rules stay in the Domain and validators check request shape only; the assessment suggests rules as validators too (docs/ARCHITECTURE.md 'Overview' states the choice)
TOPIC: claim intake raises transient VALIDATION_ISSUE_ADDED audit rows (about six for a complete claim) because each assembly step re-evaluates the issues; evaluate once at the end of intake.
TOPIC: Cold-start 500 - once, on the first GET /api/claims after a deployment (2026-10-22 01:06:50 UTC), EF threw "Static field requires null instance, non-static field requires non-null instance" while translating the reserve totals query in ClaimReader.LoadReserveTotalsAsync; not reproduced in 25 cold test processes (75 fresh hosts, concurrent first requests); lead: ClosedSetConverter.FromColumn builds its expression around a captured delegate, which EF inlines into the shapers; the smoke test now retries the read three times and prints the body
