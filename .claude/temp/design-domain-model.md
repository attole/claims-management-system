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

- Pack 9 (aggregate methods in front of the engine), outline agreed, choices pending: the aggregates get public behaviour methods; operations, transitions, rules and checks become internal with InternalsVisibleTo for the tests; the intra-aggregate follow-ups (reopen then Open, a claim change then issue evaluation, submit then auto-approve, refuse then cancel posting) move into those methods; audit and the posting job stay events with handlers.
- Pack 9 choices to settle: the method signature (recommended: take the existing input record, keep the `Input` names); issue evaluation inside every claim method (every claim input then carries the reference snapshot) or one `Evaluate` method called by an Application pipeline behavior; a composite returns the primary step's outcome; one pack per aggregate, the claim first.
- Events plan for the Application: pre-commit handlers for the reopen follow-up, the auto-approve attempt, the posting cancel and the audit entries; a post-commit handler for the posting job.

DECISIONS

- Settled architecture, interpretations and schema departures live in docs/ARCHITECTURE.md; code shapes and conventions live in .claude/CLAUDE.md.
- The Domain never chains steps today (user decision); the chains are listed in docs/ARCHITECTURE.md 'Application chains'. The question of moving the intra-aggregate ones into aggregate methods is open (pack 9).
- Snapshots are the way facts from outside an aggregate reach a rule; the trade-offs and the alternative are in docs/ARCHITECTURE.md 'Design trade-offs'.
- Flows have no hooks beyond the blocked decision; a transition answers an undeclared source straight from `Execute`.
- A reserve change is created pending and moved by transitions; the starting state is never decided by a helper.
- A reserve's balance is never stored; rules read a snapshot that the Application computes.
- A closed set that carries no fact and is not an engine state is a plain enum (user role, reserve transaction type).

ANALOGUE

- The engine is `backend/src/ClaimsModule.Domain/Core`; the claim aggregate is under `Claims/`; the reserve aggregates are `Reserves/` and `ReserveChanges/`.
- `OpenClaimTransition` is the reference transition; `SubmitReserveOperation` is the reference creation operation with a snapshot.
- The layer structure and registration pattern follow `backend/src/ClaimsModule.Application/ApplicationServiceExtensions.cs`.

NEXT

- The audit entries and the reference-data lookups belong to the Application pack.
- Then the Application, Persistence, Infrastructure and API packs, each starting from notes-layer-application.md and notes-layer-persistence.md.
