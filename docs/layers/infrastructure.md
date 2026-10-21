# Infrastructure layer

Infrastructure's background jobs and adapters for outside services: it implements `IStorageService`, `IPostingScheduler`, `IClock` and `ICurrentUser` and sends the job commands (`docs/layers/application.md` 'Provides and expects'). `.claude/CLAUDE.md` points here; read this file before changing or reviewing Infrastructure code.

## Layout

- The project holds the Hangfire job definitions and their registration, and the adapters for ports the Application defines. `InfrastructureServiceExtensions` registers them; settings are options classes bound with `BindConfiguration`.
- `BackgroundJobs/` holds the Hangfire host (storage, server, dashboard and its options), the on-demand posting job with its scheduler, and the shared `JobResult`. `BackgroundJobs/Recurring/` holds the scheduled jobs, their options and `RecurringJobsServiceExtensions` (`AddRecurringJobs` registers options and job classes, `UseRecurringJobs` adds the recurring definitions at startup); every scheduled job goes there.
- A job or adapter holds no business logic: it sends a command through MediatR or calls a port.

## Jobs

Every job is safe to run twice.

- **Posting job (GL).** The Application enqueues it after the commit through `IPostingScheduler` (an approved or auto-approved row, or a failed posting returned to pending); `HangfirePostingScheduler` enqueues `ReservePostingJob`, which marks the row through `MarkReservePostedCommand` or `MarkReservePostingFailedCommand`.
  - An approval passes an empty job id; the job assigns its own and reports it when it marks the row. A retry stores a fresh token as the job id (the Domain requires one on the move) and the after-commit handler passes it to the scheduler, so the job runs under the recorded id.
  - Marking an already posted row succeeds without a write (S:JOB-GL-Idempotent). A refused command counts as a failure and retries like an exception; `ReservePostingJob.MaxRetries` is 3 (four runs), and the run at the maximum marks the row failed instead of throwing (S:JOB-GL-Failure).
  - **Tenancy.** The scheduler carries the row's organisation as the first argument of `IPostingScheduler.EnqueueAsync` (the after-commit handlers read it from `ICurrentUser`, the sweep from `UnpostedReserve.OrganisationId`). The job sets it on the `ScopedCurrentUser` (system actor `Guid.Empty`, lowest role) of a fresh DI scope and sends the command there, so the repositories and the tenant filter apply to the write. Each command gets its own scope, because after a failed commit the first scope's context still tracks the unsaved change and the failed-mark would load that stale row.
  - The audit entries for posted and failed come from the posting events, not from the job.
- **SLA job.** `SlaBreachJob` sends `DetectSlaBreachesCommand` every 15 minutes by default (S:JOB-SLA; `Hangfire:SlaBreach:IntervalMinutes`, a divisor of 60, validated at startup). The command writes the breach entries as the system actor with the 24-hour dedupe and flags at most 500 claims per run (the reader's batch size); the next run takes the rest. It runs without a user: the SLA reader spans organisations and the audit service takes the organisation from the claim. It does not retry (`Attempts = 0`): the next run is the retry.
- **Posting sweep.** `PostingSweepJob` sends `ResendStalePostingsCommand` with the minimum age from `PostingSweepOptions` and re-enqueues at most the 500 oldest stale rows per run. Settings, validated at startup: `Hangfire:PostingSweep:MinimumAge` (default 10 minutes, positive) and `Hangfire:PostingSweep:IntervalMinutes` (default 5, a divisor of 60, because the cron minute step restarts every hour). It does not retry. It is required: a crash between the commit and the enqueue loses the posting job, and close and withdraw refuse while an approved row is unposted.
- **Idempotency cleanup.** `IdempotencyCleanupJob` sends `PurgeExpiredIdempotencyKeysCommand`, which deletes the Idempotency-Key rows older than 24 hours, the window in which the store treats a key as used. `Hangfire:IdempotencyCleanup:IntervalHours` defaults to 1 (1, 2, 3, 4, 6, 8 or 12). It does not retry.
- **Blob sweep.** `BlobSweepJob` sends `SweepOrphanBlobsCommand` with the minimum age from `BlobSweepOptions` (`Hangfire:BlobSweep:MinimumAge`, default 1 day, which protects a blob stored before its commit). It lists the document blobs last written before that age, asks Persistence 500 paths at a time which a document row refers to (soft-deleted rows and every organisation count) and deletes the unreferenced ones. It is destructive, so it is behind `Hangfire:BlobSweep:Enabled` (default off). `IntervalHours` defaults to 12. It does not retry. Run it once against a real Azure account before turning it on (see Tests, below).
- **Registration.** `UseInfrastructure` calls `UseRecurringJobs` at startup (S:JOB-Registration), which adds or updates each job by a fixed id: `sla-breach-detection`, `posting-sweep`, `idempotency-cleanup`, and `blob-sweep` while its switch is on (otherwise its definition is removed). A job whose command is refused throws (`JobResult.ThrowIfFailed`), so the failure shows in the dashboard. The Hangfire dashboard is behind its feature switch (default off).

## Clock

- `SystemClock` implements `IClock` (singleton). The Persistence interceptors, the stores and the handlers read time only through it.

## Current user

- `ScopedCurrentUser` (`IdentityServiceExtensions`) is the one registered `ICurrentUser`: a settable scoped holder (`Set(id, organisationId, role)`) that the API fills from the token and a job from its arguments. Its empty state (empty ids, lowest role) is what the tenant filter sees when nothing set it.

## Storage adapter

- `IStorageService` stores a blob and returns a download URL, made on read with a one-hour lifetime (S:BR-D-02). `ListAsync(container, createdBefore)` returns the full paths (`<container>/<name>`) of the blobs last written before the time; `DeleteAsync(path)` removes one and ignores a missing blob. Removing a document is a soft delete and keeps its blob; only the blob sweep deletes, and only unreferenced blobs, including one left by a commit that failed after the store.
- The Application builds the blob path (`claim-documents/{organisationId}/{claimId}/{uploadId}_{sanitised file name}`); the first segment is the container, the rest the blob name.
- The provider is `StorageProvider`: `LocalFileSystem` (default) or `AzureBlob` (S:BR-D-03). `StorageOptionsValidator` stops the app at startup naming each missing key: `AzureConnectionString` for Azure, `LocalBaseUrl` and `LocalSigningKey` for the local provider.
- **Azure.** `AzureBlobStorageService` creates the container when missing, uploads with the content type and returns a read-only SAS URL with the original file name as content disposition, so the browser saves the file without the upload id prefix. The SAS is signed with the account key, so the connection string must carry one.
- **Local.** `LocalFileSystemStorageService` writes under `LocalRoot` (default `uploads`, container segment dropped: `uploads/{organisationId}/{claimId}/...`) and refuses a path that leaves the root. The URL is `LocalBaseUrl` + `LocalDownloadToken.DownloadRoute` + a token: the expiry and path signed with HMAC-SHA256 under `LocalSigningKey` (a secret, in user-secrets; the storage keys are top-level, the options section name is empty). `LocalDownloadToken.ReadPath` returns the path, or an empty string for a malformed, tampered or expired token.
- `LocalDocumentDownloads` implements the Application port `ILocalDocumentDownloads`: it checks the provider and the token, finds the file and names it, and the API streams it. This is the one place bytes pass through the API, a local-only exception to S:BR-D-02.

## Tests

- The jobs and storage adapters have unit tests on fakes (the job harness, a temporary folder for the local provider, an offline SAS link for Azure).
- Four tests run Hangfire on SQL Server (LocalDB, or `CLAIMS_TEST_SQLSERVER`, failing instead of skipping when that variable is set and the server does not answer): a job enqueued through `IPostingScheduler` runs on a real server and sends its command, and `UseRecurringJobs` stores the recurring jobs with their cadence, adding the blob sweep when its switch is on and removing it when off. They share one xUnit collection because Hangfire keeps its storage in a static.
- `AzureBlobStorageService` has no test against a real or emulated Azure Storage, by choice: the SAS link and its expiry, the list by age and the delete are checked offline only.
