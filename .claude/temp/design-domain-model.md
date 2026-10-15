status: active

OPEN

- Valid reason codes: the spec defines no list; the code stays free text and must not be invented.
- Whether a party needs a name or a valid email: the spec gives only nullable columns.
- Manager versus supervisor: the spec table gives a manager all supervisor capabilities and a supervisor all handler capabilities. Pack 4 follows the chosen reading (reopen: supervisor only; revert to Open: handler only; investigate: handler or supervisor). Each is one role list on one transition.
- Cause code reference data (S:BR-C-05): an unknown or inactive code becomes a stored issue with the reference-data pack; the active codes travel as a snapshot on the claim input.
- Unknown or already removed party and risk object ids before the first remove command (notes-layer-application.md).
- Reserve Active or Closed status and notes are informational; nothing blocks on them until the spec defines more.
- No domain event for opening a reserve; the audit log maps the two reserve change events.
- Open items shared with the layer notes (who may waive a Critical issue, warning acknowledgement, handler assignment, state-based permissions, pending reserves on withdrawal) are in notes-layer-application.md and notes-layer-persistence.md.

PROVISIONAL

- None; the Domain packs are built.

DECISIONS

- Settled architecture, interpretations and schema departures live in docs/ARCHITECTURE.md; code shapes and conventions live in .claude/CLAUDE.md.
- The Domain never chains steps (user decision); the chains are listed in docs/ARCHITECTURE.md 'Application chains'.
- Flows have no hooks beyond the blocked decision; a transition answers an undeclared source straight from `Execute`.
- A reserve change is created pending and moved by transitions; the starting state is never decided by a helper.
- A reserve's balance is never stored; rules read a snapshot that the Application computes.
- A closed set that carries no fact and is not an engine state is a plain enum (user role, reserve transaction type).

ANALOGUE

- The engine is `backend/src/ClaimsModule.Domain/Core`; the claim aggregate is under `Claims/`; the reserve aggregates are `Reserves/` and `ReserveChanges/`.
- `OpenClaimTransition` is the reference transition; `SubmitReserveOperation` is the reference creation operation with a snapshot.
- The layer structure and registration pattern follow `backend/src/ClaimsModule.Application/ApplicationServiceExtensions.cs`.

NEXT

- Reference data and audit pack: cause codes (S:BR-C-05) and the audit entries.
- Then the Application, Persistence, Infrastructure and API packs, each starting from notes-layer-application.md and notes-layer-persistence.md.
