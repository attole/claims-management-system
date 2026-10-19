status: active

Application-layer working note. Packs 1 to 4 are built; the settled design is in `docs/ARCHITECTURE.md` 'The Application layer' and 'What the next layers must do', so this note holds only open work. Infrastructure items are kept here until that layer has its own note.

OPEN

- Pack 5 (contracts, audit and idempotency) is next and ends the Application layer.
- An error without a field (for example a not-permitted status move) needs a key in the spec's error body (S:API-ErrorShape); decide when the API maps errors.
- API pack: make model binding reject a missing non-nullable value in the same 422 body as the validators, not in its own 400 body.
- Which date the claim list's date range filters (the spec names only the two parameters); the reader picks one, probably the reported date.
- Constants chosen without a source: 10 recent audit entries on the claim detail, page size cap 100.
- A document blob is stored before the commit, so a failed commit leaves an unreferenced blob: an Infrastructure sweep removes blobs no row refers to.

PACK 5

- Audit handler for every Domain event of the spec's fixed list (S:AUD-Events), written before the commit through `IAuditLogService`, with the correlation id from the event context (S:AUD-Correlation); the log is append-only (S:BR-A-01).
- Event dispatcher and Unit of Work contract: pop events from every tracked entity (roots and children), wrap each in an `EventContext` (new id, clock time, current user, correlation id), run the before-commit handlers, commit, run the after-commit handlers, and repeat until no new events appear (the component-closing handler raises events). A crash between commit and enqueue can lose a posting job, so a sweep that re-enqueues old Pending postings is required: close and withdraw refuse while an approved row is unposted.
- Idempotency-Key contract: a filter in the API backed by a Persistence table with a unique index on key, tenant, user, method and route, plus a request body hash, an in-progress marker, the response status and body and the created time. The same key with another body is a 422, a key in progress a 409, a completed key replays the stored response; rows expire after about 24 hours through a cleanup job; the completion write is in the same transaction as the command.

FOR THE NEXT LAYERS

- Infrastructure: the two Hangfire jobs, their startup registration and their audit writes (S:JOB-Registration). The posting job calls the mark posted and mark failed commands and the retry button the retry command. A retry stores a fresh token as the job id (the Domain requires one on the move) and the after-commit handler passes it to `IPostingScheduler`, so the job runs under the recorded id; an approval passes an empty id and the job assigns its own and reports it when it marks the row.
- Persistence implements the readers by projecting straight into the DTOs (queries load no entity), the claim number generator (see `notes-layer-persistence.md`), the Unit of Work and the repositories. Reader facts: a pending row is PendingApproval; an unposted row is approved (Approved or AutoApproved) with posting Pending or Failed; an open balance is a component with approved balance above zero; an approved reserve for payment is a component of a non-recoverable type with approved balance above zero; the claim approved total is the net sum over all components.
- API: only a request that cannot be bound at all is rejected before a command runs; the actor and role come from the authenticated user; the Idempotency-Key filter and the error mapping (validation 422, not found 404, forbidden 403, conflict 409; the body groups errors by field and the code is not on the wire).
