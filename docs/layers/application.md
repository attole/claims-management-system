# Application layer

The Application's layout, file shapes, handler flow and tests, and its contracts with the other layers. `.claude/CLAUDE.md` points here; read this file before changing or reviewing Application code. Reasons: `docs/ARCHITECTURE.md` 'Overview'.

## Layout

One folder per aggregate root (`Claims`, `ReserveComponents`, `ClaimDocuments`), read-only feature (`Audit`, `Policies`, `CauseOfLoss`) or background-job feature (`Sla`, flat); a use case never gets a folder of its own.

- A root past about six loose files uses role folders: `Commands/`, `Queries/`, `Dtos/` (DTOs and their mapping profile), `Contracts/` (repository and reader interfaces and the records only they use, such as a list filter or facts record), `Requests/` (shared request records), `Handlers/` (event handlers).
- A feature's errors and field lengths share one file named after the errors class (`ClaimErrors.cs` holds `ClaimErrors` and `ClaimFieldLengths`).
- A child entity gets a folder mirroring the Domain (`Claims/Parties/AddPartyCommand.cs`), flat until about six files (`ReserveComponents/History`). A small feature stays flat beside a `Queries/` folder.
- Cross-cutting ports (clock, current user, unit of work, correlation, storage, job scheduler) are in `Abstractions/`. Shared blocks: `Results/`, `Messaging/` (`ICommand<T>`, `IQuery<T>`), `Paging/`, `Behaviors/`, `Events/` (`Contracts/` for the context, handler and dispatcher interfaces, `Services/` for the dispatcher) and `Idempotency/` (the store contract, the purge command and its DTO). `Audit/` has `Contracts/`, `Dtos/`, `Handlers/` and `Queries/`.
- Test folders mirror the source folders; `Support/` holds the fixtures.

## Command file

One file named after the command (`AssignHandlerCommand.cs`) holds, in order:

- **The command record** implementing `ICommand<Result<TDto>>`, with a `ToInput(...)` method that builds the Domain input from the request data and the facts the handler passes in (policy period, cause codes, actor, role, time).
- **The validator** (`<Command>Validator : AbstractValidator<Command>`, empty for a command with no input): shape only. Ids not empty, enums known, required text not null, maximum lengths from `<Feature>FieldLengths`, numbers the Domain would reject. No business rule, no existence check.
- **The handler** (`<Command>Handler`, primary-constructor injection) implementing `IRequestHandler`.

## Handler flow

1. Load the aggregate through the repository; a failure (not found) is returned with `ToFailure<TDto>()`.
2. Check that a child id exists on the aggregate (the Domain would throw on an unknown id) and return the feature's not-found error.
3. Read the outside facts through explicit readers (policy period, active cause codes, reserve facts), never a hidden loader. An upload reads the file's first 512 bytes with `DocumentContent.ReadAsync` and passes them as `Header` on `UploadClaimDocumentInput`, so the Domain checks the content signature (`docs/layers/domain.md` 'Domain modeling').
4. Build the input with `command.ToInput(..., clock.UtcNow)`, reading the clock once; take actor and role from `ICurrentUser`, never from the request.
5. Call one aggregate method and convert its outcome with `outcome.ToResult()`.
6. Commit through `IUnitOfWork` only on success.
7. Map the DTO after the commit with `result.Map(...)` (AutoMapper for one flat entity, by hand for a `Move` or several things).

Complexity stays within the limit by moving a load or store step to a private method, never by dropping a step.

## Results and errors

- `Result` / `Result<T>`: a success holds the value and the warnings, a failure holds errors (never empty) and no value; `Success(value, warnings)`, `Failure([errors])`, `Map`, `ToFailure<T>()`.
- `outcome.ToResult()` is the only conversion from a Domain outcome: Critical findings become validation errors, Warning findings travel as warnings.
- `Error` kinds: Validation (422), NotFound (404), Forbidden (403), Conflict (409). Only the Application creates a 404, for an unknown id. The feature's errors class holds the codes and factories (`ClaimErrors.ClaimNotFound(id)`).
- `ValidationBehavior` returns a failed `Result` for a failing validator and never throws. `ICommand<T>` and `IQuery<T>` require a `Result` response, so anything else does not compile.

## Query file

One file named after the query in the feature's `Queries/`: the query record (`IQuery<Result<T>>`), its validator and its handler. A query loads no entity: a reader interface in `Contracts/`, implemented by Persistence, projects into the DTO. A read scoped to a claim first calls `IClaimReader.ExistsAsync` and returns the claim not-found error.

- A paged query implements `IPagedQuery` (page and size from the request, size capped by `PageLimits`), its validator derives from `PagedQueryValidator<T>`, the handler passes `PageWindow.From(query)` to the reader, and the result is `PagedResult<T>`.
- A filter handed to a reader has no nullable member: a list for an optional choice (empty means any), open bounds for dates. The query record keeps the nullable request values and converts them in `ToFilter()`.
- A composed read (the claim detail) is assembled in its handler from the separate readers, each reused by its own endpoint.

## Request file

A request record shared by several commands (`PartyRequest`, `RiskObjectRequest`, `LossRequest`, `ReserveRequest`) lives in one file with its validator. It maps itself to the Domain (`ToDetails`, `ToOpenInput`, `ToSubmitInput`), never through an extension class, and converts a null optional text to an empty string there.

## DTOs and mapping

- A DTO is a sealed record with explicit empty defaults, in `Dtos/` with the mapping profile for a root folder, beside the use cases for a child folder or small feature.
- One AutoMapper profile per entity maps a flat Domain entity to its DTO; a DTO built from a `Move` or several things is built by hand. Values stamped on save (audit columns, `RowVer`) are mapped after the commit. A profile test asserts the configuration is valid.

## Contracts

Repository and reader interfaces are defined here and implemented by Persistence; storage and the job scheduler by Infrastructure. A repository `GetAsync` returns `Result<T>` with the not-found error, scoped by claim id so a wrong claim is a not found. One interface per file, with no implementation detail.

## Event handlers

- `IBeforeCommitEventHandler<TEvent>` runs in the same transaction (audit, components following the claim); `IAfterCommitEventHandler<TEvent>` runs after the commit (enqueue a job).
- A handler implements the generic interface for its one event and its typed `HandleAsync(TEvent, ExtendedEvent, CancellationToken)`. Each generic interface extends a non-generic one in the same file (`IBeforeCommitEventHandler`) carrying `EventType` and `HandleAsync(ExtendedEvent, CancellationToken)`, with default implementations that return `typeof(TEvent)` and cast into the typed method. The dispatcher calls only the handlers whose `EventType` equals `extendedEvent.Event.GetType()`: no reflection at dispatch, no event check in a handler.
- An event type with no handler is ignored, so a new event that must be audited needs its audit handler and test.
- Handlers live in the feature's `Handlers/` folder and are registered by assembly scan (`AddEvents`, which also registers the dispatcher). A handler reads the `ExtendedEvent` for the id, time, actor and correlation id. Audit handlers write through `IAuditLogService`, which takes an `AuditContext` (time, actor, correlation id); an `ExtendedEvent` is one.

## Job commands

A background job sends an ordinary command (`DetectSlaBreachesCommand`, `ResendStalePostingsCommand`, the mark posted and mark failed commands). It reads the time from `IClock`, writes audit as the system actor (`Guid.Empty`), uses the same reader contracts as the API, and is safe to run twice (a posted row answers success without a write).

## Naming checks (S:DATA-Naming)

- A command is `<Verb><Noun>Command` with the verb from the list in `ICommandTests`; a query is `Get<Noun>Query`, `List<Nouns>Query` or `Check<Noun>Query` (`IQueryTests`). Both are reflection tests, so a new verb is added to the list on purpose.
- The workflow check requires a file in `Dtos/` to end with `Dto` or `MappingProfile`, in `Requests/` with `Request`, in `Commands/` with `Command` and in `Queries/` with the query name shape.
- Table, column, entity and route names are checked by tests over the EF model (Persistence) and the route table (API).

## Tests

- One class per use-case file, `<UseCase>CommandTests` or `<UseCase>QueryTests`, with its `Handle_...` and `Validate_...` tests; a request file has `<Request>Tests`, a mapping profile `<Profile>Tests`.
- Handlers are tested against fakes: `ClaimHandlerContext` (writes) and `QueryContext` (reads) in `Support/Claims` build the Moq readers and repositories, with `With...` methods for arranged data and `Verify...` methods for effects.
- A handler test covers the success path with what it stores or commits, the not-found path and one refusal asserted by its rule code with `[Trait("Rule", ...)]`; a validator test covers one failing case per rule. Domain rules are not retested.
- Real Domain outcomes build the arranged state (`ApplicationClaims`, `ApplicationReserves`, `ApplicationDocuments`), never private state. `Support/Events` holds `ExtendedEvents` and `RecordingAuditLog` for event and audit handler tests.

## Provides and expects

- **Provides:** the commands, queries and DTOs, the `Result` type, the event dispatcher (`AddEvents`), the job commands (`DetectSlaBreachesCommand`, `ResendStalePostingsCommand`, `PurgeExpiredIdempotencyKeysCommand`, `SweepOrphanBlobsCommand` and the posting commands) and the ports below.
- **Expects from Persistence:** `IClaimRepository`, `IReserveComponentRepository`, `IClaimDocumentRepository`, the readers (`IClaimReader`, `IClaimReserveReader`, `IPolicyReader`, `ICauseCodeReader`, `ISlaClaimReader`, `IAuditLogReader`), `IClaimNumberGenerator`, `IAuditLogService`, `IIdempotencyStore` and `IUnitOfWork`, which translates EF save errors into `ConcurrencyConflictException`. See `docs/layers/persistence.md`.
- **Expects from Infrastructure:** `IStorageService`, `IPostingScheduler`, `IClock`, `ICurrentUser` (as `ScopedCurrentUser`) and the jobs that send the job commands. See `docs/layers/infrastructure.md`.
- **Expects from the API:** the correlation id (`ICorrelationContext`), the current user filled from the token, the error mapping and the Idempotency-Key filter. See `docs/layers/api.md`.
