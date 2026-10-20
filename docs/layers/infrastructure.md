# Infrastructure layer

Rules and contracts of Infrastructure: the background jobs and the adapters for outside services. It implements `IStorageService` and `IPostingScheduler` and sends the job commands (`docs/layers/application.md` 'Provides and expects'). `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the Infrastructure project or reviewing its code. Status: the clock and the storage adapters are built; the jobs are not (this file holds what the Domain and the Application hand over for them).

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

## Clock

- `SystemClock` implements `IClock` (singleton). The Persistence interceptors, the stores and the handlers read time only through it.

## Storage adapter

- `IStorageService` stores a blob and returns a download URL. The URL is made on read with a one-hour lifetime (S:BR-D-02). It has no delete: removing a document is a soft delete and keeps its blob.
- The blob path is built by the Application (`claim-documents/{organisationId}/{claimId}/{uploadId}_{sanitised file name}`); the adapter only stores under it. The first segment is the container; the rest is the blob name.
- The provider is `StorageProvider`, `LocalFileSystem` (default) or `AzureBlob` (S:BR-D-03). `StorageOptionsValidator` stops the app at startup naming each missing key: `AzureConnectionString` for Azure, `LocalBaseUrl` and `LocalSigningKey` for the local provider.
- **Azure.** `AzureBlobStorageService` creates the container when missing, uploads with the content type and returns a read-only SAS URL. The SAS is signed with the account key, so the connection string must carry one. Not run against a real account or Azurite; the tests cover the URL shape and its expiry only.
- **Local.** `LocalFileSystemStorageService` writes under `LocalRoot` (default `uploads`, the container segment dropped, so `uploads/{organisationId}/{claimId}/...`). A path that leaves the root is refused. The URL is `Storage:LocalBaseUrl` + `LocalDownloadToken.DownloadRoute` + a token. The token is the expiry and the path signed with HMAC-SHA256 under `Storage:LocalSigningKey` (a secret, user-secrets); `LocalDownloadToken.ReadPath` returns the path, or an empty string when the token is malformed, tampered with or expired.
- The API serves the local download route: it reads the path from the token and streams `LocalFileSystemStorageService.OpenRead`. This is the one place bytes pass through the API, a local-only exception to S:BR-D-02 (the Azure URL goes straight to the blob).
- A failed commit after the blob is stored leaves an unreferenced blob; a sweep that removes blobs no row refers to is parked in `.claude/temp/notes-parking-lot.md`.
