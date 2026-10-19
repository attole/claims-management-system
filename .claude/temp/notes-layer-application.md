status: active

Application-layer working note. Packs 1 to 5 are built and the layer is complete; the settled design is in `docs/ARCHITECTURE.md` 'The Application layer' and 'What the next layers must do', so this note holds only open work. Infrastructure items are kept here until that layer has its own note.

OPEN

- An error without a field (for example a not-permitted status move) needs a key in the spec's error body (S:API-ErrorShape); decide when the API maps errors.
- API pack: make model binding reject a missing non-nullable value in the same 422 body as the validators, not in its own 400 body.
- Which date the claim list's date range filters (the spec names only the two parameters); the reader picks one, probably the reported date.
- Constants chosen without a source: 10 recent audit entries on the claim detail, page size cap 100.
- A document blob is stored before the commit, so a failed commit leaves an unreferenced blob: an Infrastructure sweep removes blobs no row refers to.

PACK 5 (built)

- Audit: one before-commit handler per event of the spec's fixed list (S:AUD-Events), in `Audit/Handlers`; they write through `IAuditLogService`. Closing and reopening write a second entry beside STATUS_CHANGED. Events outside the list (document removed, issue acknowledged, waived or cleared, limit override, component state) write nothing; SLA_BREACH_DETECTED comes from the SLA job.
- Commit: the Unit of Work is built in the Persistence pack, not here. One `UnitOfWork` class (`IUnitOfWork`) takes the `DbContext`, `IEventDispatcher`, the clock, `ICurrentUser` and `ICorrelationContext`: pop events from every tracked entity (`ChangeTracker.Entries<Entity>()`), wrap each in an `EventContext` (new `Ids.Next()`, clock time, user id, correlation id), run the before-commit handlers and repeat until no new event appears, save once, then run the after-commit handlers. Tests against EF: order (before, save, after), repeat until empty, context fields, no save when a before-commit handler throws. `AddEvents` registers the handlers and the dispatcher.
- Idempotency: `IIdempotencyStore.BeginAsync` (Started, InProgress, Completed with the stored response, BodyMismatch) and `CompleteAsync`. Completion is its own save, not part of the command transaction: a crash between leaves the key in progress (409) until it expires.
- Still needed: the sweep that re-enqueues old Pending postings (a crash between commit and enqueue loses the job), in Infrastructure.

FOR THE NEXT LAYERS

- Infrastructure: the two Hangfire jobs, their startup registration and their registration (S:JOB-Registration; the GL audit entries come from the posting events, the SLA job writes its own through `IAuditLogService`). The posting job calls the mark posted and mark failed commands and the retry button the retry command. A retry stores a fresh token as the job id (the Domain requires one on the move) and the after-commit handler passes it to `IPostingScheduler`, so the job runs under the recorded id; an approval passes an empty id and the job assigns its own and reports it when it marks the row.
- Persistence implements the readers by projecting straight into the DTOs (queries load no entity), the claim number generator (see `notes-layer-persistence.md`), the Unit of Work and the repositories. Reader facts: a pending row is PendingApproval; an unposted row is approved (Approved or AutoApproved) with posting Pending or Failed; an open balance is a component with approved balance above zero; an approved reserve for payment is a component of a non-recoverable type with approved balance above zero; the claim approved total is the net sum over all components.
- API: only a request that cannot be bound at all is rejected before a command runs; the actor and role come from the authenticated user; the Idempotency-Key filter (backed by `IIdempotencyStore`, a Persistence table with a unique index on key, tenant, user, method and route, a body hash, the response and a 24 hour expiry cleanup job) and the error mapping (validation 422, not found 404, forbidden 403, conflict 409; the body groups errors by field and the code is not on the wire).
