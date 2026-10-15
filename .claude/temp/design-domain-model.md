status: active

OPEN

- Valid reason codes: the spec defines no list; the code stays free text and must not be invented.
- Whether a party needs a name or a valid email: the spec gives only nullable columns.
- Manager versus supervisor: the spec table gives a manager all supervisor capabilities and a supervisor all handler capabilities. Pack 4 follows the chosen reading (reopen: supervisor only; revert to Open: handler only; investigate: handler or supervisor). Each is one role list on one transition.
- Reserve changes carry no claim-state rule: the Domain checks sums and limits whatever the claim state (Draft, Closed or Withdrawn); the spec states no restriction, so none is added until a source exists.
- Unknown or already removed party and risk object ids before the first remove command (notes-layer-application.md).
- Reserve Active or Closed status and notes are informational; nothing blocks on them until the spec defines more.
- No domain event for opening a reserve; the audit log maps the two reserve change events.
- Open items shared with the layer notes (who may waive a Critical issue, warning acknowledgement, handler assignment, state-based permissions, pending reserves on withdrawal) are in notes-layer-application.md and notes-layer-persistence.md.

PROVISIONAL

- Application pack: event handlers for the audit entries and the posting job, the snapshot queries with their definitions, and the command handlers that call one aggregate method each.

DECISIONS

- Settled architecture, interpretations and schema departures live in docs/ARCHITECTURE.md (the backend architecture sections); code shapes and conventions live in .claude/CLAUDE.md. The decisions of the domain packs have graduated there.
- A closed set that carries no fact and is not an engine state is a plain enum (user role, reserve transaction type).
- Test fixtures build claims through the public aggregate methods, so a fixture claim has been touched, evaluated and has raised its events like a real one.

ANALOGUE

- The engine is `backend/src/ClaimsModule.Domain/Core`; the claim aggregate is under `Claims/`; the reserve aggregates are `Reserves/` and `ReserveChanges/`.
- `OpenClaimTransition` is the reference transition; `SubmitReserveOperation` is the reference creation operation with a snapshot.
- The layer structure and registration pattern follow `backend/src/ClaimsModule.Application/ApplicationServiceExtensions.cs`.

NEXT

- The audit entries and the reference-data lookups belong to the Application pack.
- Then the Application, Persistence, Infrastructure and API packs, each starting from notes-layer-application.md and notes-layer-persistence.md.
