# Infrastructure layer

Rules and contracts of Infrastructure: the background jobs and the adapters for outside services. It implements `IStorageService` and `IPostingScheduler` and sends the job commands (`docs/layers/application.md` 'Provides and expects'). `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the Infrastructure project or reviewing its code. Status: not built; this file holds what the Domain and the Application hand over, and grows into standards as the layer is written.

## Layout

- The project holds the Hangfire job definitions and their registration, and the adapters that implement ports the Application defines (`IStorageService`, `IPostingScheduler`, and email if it is added). `InfrastructureServiceExtensions` registers them; settings are options classes bound with `BindConfiguration`.
- A job or adapter holds no business logic. It sends a command through MediatR or calls a port, and nothing else.

## Jobs

- **Posting job (GL).** The Application enqueues it after the commit through `IPostingScheduler` (an approved or auto-approved row, or a failed posting returned to pending). The job marks the row through `MarkReservePostedCommand` or `MarkReservePostingFailedCommand`.
  - An approval passes an empty job id; the job assigns its own and reports it when it marks the row. A retry stores a fresh token as the job id (the Domain requires one on the move) and the after-commit handler passes it to the scheduler, so the job runs under the recorded id.
  - The job is idempotent (S:JOB-GL-Idempotent): marking an already posted row succeeds without a write. It retries automatically and, when the retries are exhausted, marks the row failed (S:JOB-GL-Failure).
  - The audit entries for posted and failed come from the posting events, not from the job.
- **SLA job.** Runs every 15 minutes (S:JOB-SLA) and sends `DetectSlaBreachesCommand`. The command writes the breach entries as the system actor and respects the 24-hour dedupe; the job writes nothing itself.
- **Posting sweep.** A periodic job sends `ResendStalePostingsCommand` with a minimum age taken from its options. It is required: a crash between the commit and the enqueue loses the posting job, and close and withdraw refuse while an approved row is unposted.
- **Idempotency cleanup.** Removes the stored Idempotency-Key rows after about 24 hours.
- **Registration.** All recurring jobs are registered at application startup (S:JOB-Registration). The Hangfire dashboard is behind its feature switch, which defaults to off.
- Every job is safe to run twice.

## Storage adapter

- `IStorageService` stores a blob, removes it and returns a download URL. The URL is made on read with a one-hour lifetime (S:BR-D-02).
- The blob path is built by the Application (`claim-documents/{organisationId}/{claimId}/{uploadId}_{sanitised file name}`); the adapter only stores under it. The local filesystem fallback uses the same path. Removing a document is a soft delete and keeps its blob.
- A failed commit after the blob is stored leaves an unreferenced blob; a sweep that removes blobs no row refers to is parked in `.claude/temp/notes-parking-lot.md`.
