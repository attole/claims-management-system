status: active

OPEN

- Valid reason codes: the spec defines no list; the code stays free text and must not be invented.
- Whether a party needs a name or a valid email: the spec gives only nullable columns.
- Reserve Active or Closed status and notes are informational; nothing blocks on them until the spec defines more.
- No domain event for opening a reserve; the audit log maps the three reserve change events.
- The open item shared with the layer notes (which warnings need acknowledgement) is in notes-layer-application.md; the others are decided in ARCHITECTURE 'Decisions on gaps'.

PROVISIONAL

- Application pack: event handlers for the audit entries and the posting job, the outside-fact queries with their definitions, and the command handlers that call one aggregate method each.

DECISIONS

- Settled architecture, interpretations and schema departures live in docs/ARCHITECTURE.md (the backend architecture sections); code shapes and conventions live in .claude/CLAUDE.md. The decisions of the domain packs have graduated there.
- Test fixtures build claims through the public aggregate methods, so a fixture claim has been evaluated and has raised its events like a real one. Two shortcuts bypass them on purpose: `TestClaims.MoveTo` places a claim in a state through a test transition, and `TestClaims.ChangeLoss` sets the loss on the child to skip the after-Draft guard.

ANALOGUE

- The engine is `backend/src/ClaimsModule.Domain/Core`; the claim aggregate is under `Claims/`; the reserve aggregates are `Reserves/` and `ReserveChanges/`.
- `OpenClaimTransition` is the reference transition; `SubmitReserveOperation` is the reference creation operation with outside facts.
- The layer structure and registration pattern follow `backend/src/ClaimsModule.Application/ApplicationServiceExtensions.cs`.

NEXT

- The audit entries and the reference-data lookups belong to the Application pack.
- Then the Application, Persistence, Infrastructure and API packs, each starting from notes-layer-application.md and notes-layer-persistence.md.
