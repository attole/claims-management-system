status: active

Application-layer working note. Packs 1 to 5 are built and the layer is complete; the settled design is in `docs/ARCHITECTURE.md` 'The Application layer' and 'What the next layers must do', so this note holds only open work. Infrastructure items are kept here until that layer has its own note.

OPEN

- API pack: the error body (S:API-ErrorShape) groups errors by field, and an error without a field goes under the key `General`, set in the one mapper.
- API pack: make model binding reject a missing non-nullable value in the same 422 body as the validators, not in its own 400 body.
- Intake with an unknown policy id creates the claim without a policy and returns a POLICY_NOT_FOUND warning; linking an unknown policy to an existing claim is still NotFound.

PACK 5 (built)

- Audit: one before-commit handler per event of the spec's fixed list (S:AUD-Events), in `Audit/Handlers`; they write through `IAuditLogService`. Closing and reopening write a second entry beside STATUS_CHANGED. Events outside the list (document removed, issue acknowledged, waived or cleared, limit override, component state) write nothing; SLA_BREACH_DETECTED comes from the SLA job.
- Commit: the Unit of Work is built in the Persistence pack, not here. One `UnitOfWork` class (`IUnitOfWork`) takes the `DbContext`, `IEventDispatcher`, the clock, `ICurrentUser` and `ICorrelationContext`: pop events from every tracked entity (`ChangeTracker.Entries<Entity>()`), wrap each in an `EventContext` (new `Ids.Next()`, clock time, user id, correlation id), run the before-commit handlers and repeat until no new event appears, save once, then run the after-commit handlers. Tests against EF: order (before, save, after), repeat until empty, context fields, no save when a before-commit handler throws. `AddEvents` registers the handlers and the dispatcher.
- Jobs (folder `Sla/`): `DetectSlaBreachesCommand` (the SLA job; `ISlaClaimReader` lists Draft or Open claims not updated for 48 hours with no breach entry in the last 24 hours, the handler writes one SLA_BREACH_DETECTED entry each as the system actor) and `ResendStalePostingsCommand` (the sweep; `IClaimReserveReader.ListStalePendingPostingsAsync` lists Pending approved rows older than the given age, the handler enqueues each). Marking an already posted row returns success without a write (S:JOB-GL-Idempotent). The Hangfire jobs send these commands.
- Idempotency: `IIdempotencyStore.BeginAsync` (Started, InProgress, Completed with the stored response, BodyMismatch) and `CompleteAsync`. Completion is its own save, not part of the command transaction: a crash between leaves the key in progress (409) until it expires.
- Still needed: the sweep that re-enqueues old Pending postings (a crash between commit and enqueue loses the job), in Infrastructure.

FOR THE NEXT LAYERS

- Infrastructure: the two Hangfire jobs, their startup registration and their registration (S:JOB-Registration; the GL audit entries come from the posting events, the SLA job writes its own through `IAuditLogService`). The posting job calls the mark posted and mark failed commands and the retry button the retry command. A retry stores a fresh token as the job id (the Domain requires one on the move) and the after-commit handler passes it to `IPostingScheduler`, so the job runs under the recorded id; an approval passes an empty id and the job assigns its own and reports it when it marks the row.
- Persistence implements the readers by projecting straight into the DTOs (queries load no entity), the claim number generator (see `notes-layer-persistence.md`), the Unit of Work and the repositories. Reader facts: a pending row is PendingApproval; an unposted row is approved (Approved or AutoApproved) with posting Pending or Failed; an open balance is a component with approved balance above zero; an approved reserve for payment is any transaction of the claim that is Approved or AutoApproved; the claim approved total is the net sum over all components.
- Naming (S:DATA-Naming) for the later layers: a Persistence test over the EF model (table names equal to the spec's section 9 list, columns PascalCase), an API test over the route table (kebab-case plural).
- API: only a request that cannot be bound at all is rejected before a command runs; the actor and role come from the authenticated user; the Idempotency-Key filter (backed by `IIdempotencyStore`, a Persistence table with a unique index on key, tenant, user, method and route, a body hash, the response and a 24 hour expiry cleanup job) and the error mapping (validation 422, not found 404, forbidden 403, conflict 409; the body groups errors by field and the code is not on the wire).
