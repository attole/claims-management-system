# Application layer

Rules and contracts of the Application: layout, the shape of each kind of file, the handler flow and the test shapes, and what the Application provides and expects. `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the Application or reviewing Application code. Status: complete, with tests. The reasons are in `docs/ARCHITECTURE.md` 'Overview'.

## Layout

One folder per aggregate root (`Claims`, `ReserveComponents`, `ClaimDocuments`), read-only feature (`Audit`, `Policies`, `CauseOfLoss`) or background-job feature (`Sla`, with its command, reader contract and DTO flat inside); a use case never gets a folder of its own.

- A root past about six loose files uses role folders, each created with its first file: `Commands/` (writes), `Queries/` (reads), `Dtos/` (DTOs and the mapping profile that maps to them), `Contracts/` (repository and reader interfaces and the records only they use, such as a list filter or a facts record), `Requests/` (shared request records), `Handlers/` (event handlers).
- The errors and the field lengths of a feature share one file named after the errors class (`ClaimErrors.cs` holds `ClaimErrors` and `ClaimFieldLengths`).
- A child entity gets its own folder mirroring the Domain (`Claims/Parties/AddPartyCommand.cs`) with its files flat, and uses role folders past about six files (`ReserveComponents/History`). A small feature stays flat beside a `Queries/` folder.
- Cross-cutting ports (clock, current user, unit of work, correlation, storage, job scheduler) are in `Abstractions/`. Shared building blocks have their own folders: `Results/`, `Messaging/` (`ICommand<T>`, `IQuery<T>`), `Paging/`, `Behaviors/`, `Events/` (`Contracts/` for the context, handler and dispatcher interfaces, `Services/` for the dispatcher) and `Idempotency/` (the store contract, plus the purge job command and its DTO). `Audit/` is a feature folder with `Contracts/`, `Dtos/`, `Handlers/` (the audit event handlers) and `Queries/`.
- Test folders mirror the source folders; `Support/` holds the fixtures.

## Command file

One file named after the command (`AssignHandlerCommand.cs`) holds, in this order:

- **The command record** implementing `ICommand<Result<TDto>>`, with a `ToInput(...)` method that builds the Domain input from the request data and the facts the handler passes in (policy period, cause codes, actor, role, time). A request maps itself with a method on its own record (`ToInput`, `ToDetails`), never an extension class.
- **The validator** (`<Command>Validator : AbstractValidator<Command>`; a command with no input has an empty one): shape only. Ids not empty, enums known, required text not null, maximum lengths from the column sizes (`<Feature>FieldLengths`), numbers the Domain would reject. No business rule, no existence check.
- **The handler** (`<Command>Handler`, primary-constructor injection) implementing `IRequestHandler`.

## Handler flow

1. Load the aggregate through the repository. A `Result` failure (not found) is returned with `ToFailure<TDto>()`.
2. Check that a child id exists on the aggregate before the Domain call (the Domain would throw on an unknown id) and return the feature's not-found error.
3. Read the outside facts through explicit readers (policy period, active cause codes, reserve facts); there is no loader that hides the reads.
4. Build the input with `command.ToInput(..., clock.UtcNow)` (every claim input carries the time); read the clock once; take actor and role from `ICurrentUser`, never from the request.
5. Call one aggregate method and convert its outcome with `outcome.ToResult()`.
6. Commit through `IUnitOfWork` only on success.
7. Map the DTO after the commit with `result.Map(...)` (AutoMapper for one flat entity, by hand for a `Move` or several things).

Complexity stays within the limit by moving a load or a store step to a private method, not by dropping a step.

## Results and errors

- `Result` / `Result<T>`: a success holds the value and the warnings, a failure holds errors and no value; `Success(value, warnings)`, `Failure([errors])` (never empty), `Map`, `ToFailure<T>()`.
- `outcome.ToResult()` is the only conversion from a Domain outcome. Critical findings become validation errors, Warning findings travel as warnings.
- `Error` kinds: Validation (422), NotFound (404), Forbidden (403), Conflict (409). Only the Application creates a 404, for an unknown id. The feature's errors class holds the codes and factories (`ClaimErrors.ClaimNotFound(id)`).
- `ValidationBehavior` returns a failed `Result` for a failing validator; it never throws. `ICommand<T>` and `IQuery<T>` require a `Result` response, so a request that returns anything else does not compile.

## Query file

One file named after the query in the feature's `Queries/` folder: the query record (`IQuery<Result<T>>`), its validator and its handler. A query loads no entity: a reader interface (in `Contracts/`) that Persistence implements projects straight into the DTO. A read scoped to a claim first calls `IClaimReader.ExistsAsync` and returns the claim not-found error.

- A paged query implements `IPagedQuery` (page and page size from the request, size capped by `PageLimits`), its validator derives from `PagedQueryValidator<T>`, and the handler passes `PageWindow.From(query)` to the reader. The result is `PagedResult<T>`.
- A filter handed to a reader is a record with no nullable member: a list for an optional choice (empty means any), open bounds for dates. The query record keeps the nullable request values and converts them in a `ToFilter()` method.
- A composed read (the claim detail) is assembled in its handler from the separate readers, so each part is reused by its own endpoint.

## Request file

A request record shared by several commands (`PartyRequest`, `RiskObjectRequest`, `LossRequest`, `ReserveRequest`) lives in one file with its validator, named after the record. It maps itself to the Domain (`ToDetails`, `ToOpenInput`, `ToSubmitInput`) and converts a null optional text to an empty string there.

## DTOs and mapping

- A DTO is a sealed record with explicit empty defaults. A root folder keeps its DTOs in `Dtos/` together with the mapping profile; a child entity folder or a small feature keeps them beside the use cases.
- One AutoMapper profile per entity maps a flat Domain entity to its DTO. A DTO built from a `Move` or from several things is built by hand. Values stamped on save (audit columns, `RowVer`) are mapped after the commit. A profile test asserts that the configuration is valid.

## Contracts

Repository and reader interfaces are defined in the Application and implemented by Persistence; storage and the job scheduler by Infrastructure. A repository `GetAsync` returns `Result<T>` with the not-found error, scoped by claim id so a wrong claim is a not found. An interface is one per file and carries no implementation detail.

## Event handlers

`IBeforeCommitEventHandler<TEvent>` runs in the same transaction (audit, components following the claim); `IAfterCommitEventHandler<TEvent>` runs after the commit (enqueue a job). A handler implements the generic interface for its one event and its typed `HandleAsync(TEvent, ExtendedEvent, CancellationToken)`. Each generic interface extends a non-generic one (`IBeforeCommitEventHandler`, in the same file) that carries `EventType` and `HandleAsync(ExtendedEvent, CancellationToken)`, with default implementations that return `typeof(TEvent)` and cast the event into the typed method. The dispatcher holds the non-generic handlers and calls only those whose `EventType` equals `extendedEvent.Event.GetType()`, so there is no reflection at dispatch and no event check in a handler. An event type with no handler is ignored, so a new event that must be audited needs its audit handler and test. Handlers are registered by assembly scan (`AddEvents`, which also registers the dispatcher) and live in the feature's `Handlers/` folder. A handler reads the extended event (`ExtendedEvent`), not the Domain event alone, for the id, time, actor and correlation id. Audit handlers write through `IAuditLogService`, which takes an `AuditContext` (time, actor, correlation id); an `ExtendedEvent` is one.

## Job commands

A background job sends an ordinary command (`DetectSlaBreachesCommand`, `ResendStalePostingsCommand`, the posting job's mark posted and mark failed commands). It reads the time from `IClock`, acts as the system actor (`Guid.Empty`) when it writes audit, takes its readers from the same contracts as the API, and is safe to run twice (a posted row answers success without a write).

## Naming checks (S:DATA-Naming)

- A command is `<Verb><Noun>Command` with the verb from the list in `ICommandTests`; a query is `Get<Noun>Query` or `List<Nouns>Query` with a plural noun (`IQueryTests`). Both are reflection tests, so a new verb is added to the list on purpose.
- The workflow check requires a file in `Dtos/` to end with `Dto` or `MappingProfile`, in `Requests/` with `Request`, in `Commands/` with `Command` and in `Queries/` with the query name shape.
- Table, column, entity and route names are checked in the Persistence and API packs (tests over the EF model and the route table).

## Tests

- One test class per use-case file, `<UseCase>CommandTests` or `<UseCase>QueryTests`, holding its `Handle_...` and `Validate_...` tests; a request file has `<Request>Tests`; a mapping profile `<Profile>Tests`.
- Handlers are tested against fakes: `ClaimHandlerContext` (writes) and `QueryContext` (reads) in `Support/Claims` build the Moq readers and repositories, with `With...` methods for the arranged data and `Verify...` methods for the effects.
- A handler test covers the success path with what it stores or commits, the not-found path and one refusal asserted by its rule code with `[Trait("Rule", ...)]`; a validator test covers one failing case per rule. Domain rules are not retested.
- Real Domain outcomes build the arranged state (`ApplicationClaims`, `ApplicationReserves`, `ApplicationDocuments`), never private state. `Support/Events` holds `ExtendedEvents` and `RecordingAuditLog` for the event and audit handler tests.

## Provides and expects

- **Provides:** the commands, queries and DTOs, the `Result` type, the event dispatcher (`AddEvents`), the job commands (`DetectSlaBreachesCommand`, `ResendStalePostingsCommand`, `PurgeExpiredIdempotencyKeysCommand`, `SweepOrphanBlobsCommand` and the posting commands) and the ports below.
- **Expects from Persistence:** `IClaimRepository`, `IReserveComponentRepository`, `IClaimDocumentRepository`, the readers (`IClaimReader`, `IClaimReserveReader`, `IPolicyReader`, `ICauseCodeReader`, `ISlaClaimReader`, `IAuditLogReader`), `IClaimNumberGenerator`, `IAuditLogService`, `IIdempotencyStore` and `IUnitOfWork`. It translates EF save errors into `ConcurrencyConflictException`. See `docs/layers/persistence.md`.
- **Expects from Infrastructure:** `IStorageService` and `IPostingScheduler`, and the jobs that send the job commands. See `docs/layers/infrastructure.md`.
- **Expects from the API:** the correlation id (`ICorrelationContext`), the current user set from the token (Infrastructure registers `IClock` and `ICurrentUser` as `ScopedCurrentUser`; the API fills it), the error mapping and the Idempotency-Key filter. See `docs/layers/api.md`.
