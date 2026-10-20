# Persistence layer

Rules and contracts of Persistence: schema conventions, how the Domain is mapped, the save interceptor, the Unit of Work, readers and repositories, and migrations. It implements the ports the Application defines (`docs/layers/application.md` 'Provides and expects'). `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the Persistence project or reviewing its code. The schema departures from the specification and their reasons are in `docs/ARCHITECTURE.md` 'Schema departures from the spec'.

Status: pack 1 is built and tested (the context, the model-wide conventions, the filters, the converters, the save interceptor without the root touch, the Policy and cause-of-loss mappings). The claim, reserve and document mappings with the root touch, the Unit of Work, the readers and repositories and the migration are not built. Points marked (spiked) were checked in a throwaway EF Core 9 spike outside the repo (SQL Server provider for the model and script, SQLite in memory for round trips); points marked (not verified) need a real-database check when they are written. Open questions are in `.claude/temp/notes-parking-lot.md`.

## Layout

- One `IEntityTypeConfiguration<T>` per entity, with the Fluent API only and no data annotations. Converters, repositories, readers, interceptors and migrations have their own folders; `PersistenceServiceExtensions` registers them.
- Interfaces that Persistence implements are defined in the Application (repositories, readers, `IUnitOfWork`, the claim number generator, the audit service and reader, the idempotency store). The Domain is never referenced for a Persistence concern.
- Test folders mirror source folders; real-database tests use SQLite in memory for round trips.

## Schema conventions

- Primary keys are sequential GUIDs generated in the Domain (`Ids.Next()`); Persistence also declares the `NEWSEQUENTIALID()` default so the schema matches the specification.
- Money is `DECIMAL(19,4)` (`HasPrecision(19, 4)`), timestamps are `DATETIMEOFFSET(7)` in UTC, short codes `NVARCHAR(50)`.
- Table names are the specification's section 9: `Claims`, `LossEvents`, `ClaimParties`, `ClaimRiskObjects`, `ClaimReserveComponents`, `ReserveHistory`, `ClaimDocuments`, `ClaimAuditLog`, `CauseOfLossCodes`, `Policies`. Entity classes keep their Domain names and map with `ToTable` (`ReserveComponent` to `ClaimReserveComponents` with its state column named `Status`, and so on). Columns are PascalCase. Tables the specification does not list are named here: `ClaimIssues`, `ClaimStateChanges`, `PolicyCoverageTypes`, `ClaimNumberCounters` and `IdempotencyKeys`.
- Every business table has soft delete (`IsDeleted`, `DeletedAt`), audit columns and a tenant column, with global query filters for soft delete and tenant. A table soft-deletes exactly when its entity implements `ISoftDeletable`; the others (state-change history, reserve history) keep their rows, and a `Remove` on one of them throws. The audit log is append-only; a removed party is deactivated (`IsActive`) instead.
- Aggregate roots (the claim, the reserve component, the document) carry a `RowVer` concurrency token.
- A Draft claim may be incomplete: the loss date column is nullable and the cause code has no foreign key (see `docs/ARCHITECTURE.md` 'Schema departures from the spec').
- The audit log has no update or delete permission in the application, and an index by claim and event type for the SLA dedupe lookup.
- One `ClaimsDbContext` serves every repository, reader and service, so one save covers a command. A model-wide convention (not a per-entity line) adds the tenant and soft-delete filters, the audit shadow columns, the soft-delete columns (for `ISoftDeletable` entities only) and the key default to every `Entity`.
- The tenant filter reads the organisation of `ICurrentUser`. A job has no user, so the filter must see an empty organisation and not throw, and job readers bypass it on purpose (`IgnoreQueryFilters` with the soft-delete test written out).
- The coverage types of a policy are a child table (`PolicyCoverageTypes`, one row per type), not a JSON column.
- `CauseOfLossCodes` carries a `SortOrder` shadow column for the display order; the Domain has no order.

## Mapping the Domain

- **Details values** (`LossDetails`, `PartyDetails`, `RiskObjectDetails`, `ReserveHistoryDetails`, `ClaimDocumentDetails`, `Policy.Period`) are complex properties in the entity's table, replaced as a whole (spiked: only the changed members are marked modified). Give every member `HasColumnName` to match the specification, because the default carries a `Details_` prefix. EF binds a record's constructor to mapped members (checked), so a value with a member that has no column of its own (`PolicyPeriod.PolicyId`) is not mapped as a complex property.
- **Policy** maps its dates as scalar shadow columns (`EffectiveDate`, `ExpirationDate` and a nullable `CancelledDate`) and ignores `Period` and `CoverageTypes`; no repository loads a Policy. The policy reader builds `PolicyPeriod` from the row and its id (a NULL cancellation date becomes `DateOnly.MinValue`) and fills the status from `PolicyPeriod.StatusOn(today)` with the date of `IClock` (the rule lives only in the Domain); the specification's status column is not stored (`docs/ARCHITECTURE.md` 'Schema departures from the spec').
- **Entities** have a private parameterless constructor with empty placeholders, because EF cannot bind domain constructors that take values it does not map; `EntityConstructionTests` builds each through it.
- **Absent values** are sealed records with an `Empty` instance (`LossDate`, `EstimatedLossAmount`), since EF refuses a nullable column on a non-nullable struct. The converter maps NULL to `Empty` and `Empty` to NULL (`ConvertsNulls`) and the property is declared optional (spiked for the loss date). A value-type member cannot be optional (checked), so a column that must be NULL for a Domain value type is a Persistence shadow column, and the reader maps NULL to the empty value. A stored zero estimate must read back as a set estimate, because no estimate is not an estimate of 0 (user decision; not verified).
- **Closed sets** use one generic converter (marker text out, record in) that takes the set's public `From`; plain enums use EF's text conversion. The stored text is the enum member name, so renaming a member breaks the stored rows; an unknown text throws on read, which is the intended loud failure, and a test pins the stored names of each set when it is mapped. A converter maps one value and never an entity or a DTO (AutoMapper stays in the Application for DTOs). A converter is a subclass of `ValueConverter` (the only way to set `ConvertsNulls`) whose two conversions are named `private static readonly Expression<Func<...>>` fields, `_toColumn` and `_fromColumn`, passed to the base constructor (EF needs expressions, not delegates); a converter that needs an argument builds the second expression in a private static method. A closed set inside details (party role and type) is a converted scalar with a maximum length. No design-time factory unless migrations cannot create the context.
- **Issues:** `ClaimValidationIssue` is a child collection of the claim with the finding as a complex property (code, field, message, evidence and the severity as a scalar; the evidence column is as large as the trimmed loss description), the state as a converted scalar, and a resolved time and resolver with empty defaults. Cleared issues stay as history.
- **History rows:** a child table of the component with a foreign key (`ReserveComponentId`), the denormalised claim id, and the component type and `SubmittedAt` as mapped columns. A unique index on (`ReserveComponentId`, `ChangeSequence`) backs the idempotency key. Rows are append-only.
- Every other nullable column gets the same check as the loss date before it is mapped, and an index on a column inside a complex type is verified before it is relied on (not verified).

## Save interceptor

- `CreatedAt`, `UpdatedAt`, `UserCreated`, `UserModified` and `DeletedAt` are EF shadow properties, not Domain properties (user decision). `DeletedAt` is stamped when a row's `IsDeleted` turns true, whether the Domain flipped it or a remove was turned into a soft delete. The interceptor stamps them: an added row sets `CreatedAt` and `UserCreated`, a modified row sets `UpdatedAt` and `UserModified`, with the time and user from the Application's clock and current-user interfaces, the same source the input builders stamp from.
- The Domain sets only business fields: `OrganisationId` (on the base `Entity`, `ITenantOwned`) and the soft-delete flag (`ISoftDeletable.IsDeleted`) for a removed risk object or document.
- **Root touch:** when a child is added, modified or soft-deleted, the interceptor also marks its aggregate root modified (children hold their root id), so the root's `UpdatedAt` and `RowVer` move. A history write also touches the claim row of its component, which serializes concurrent approvals across a claim's components; the SLA job depends on the claim touch.
- The global soft-delete filter reads the flag through the interface property; a hard `Remove` on a tracked `ISoftDeletable` entity becomes the soft-delete flag in the interceptor.
- The interceptor reaches the current user and the time through Application interfaces and uses member names as strings, so a test against a real context must cover every interface member.

## Unit of Work and events

One `UnitOfWork` class implements `IUnitOfWork` over the `DbContext`:

1. Pop the events of every tracked entity, roots and children (`ChangeTracker.Entries<Entity>()`).
2. Wrap each in an `EventContext` (new `Ids.Next()`, clock time, current user id, correlation id).
3. Run the before-commit handlers; they can raise more events, so repeat until no new event appears.
4. Save once.
5. Run the after-commit handlers.

Tests against EF cover the order (before, save, after), the repeat until empty, the context fields, and no save when a before-commit handler throws. Idempotency completion is its own save, not part of the command transaction.

**Save errors:** the Unit of Work catches `DbUpdateConcurrencyException` and a unique-index `DbUpdateException` and throws the Application's `ConcurrencyConflictException`, so the Application never sees an EF type. The API middleware maps it to 409 (`docs/layers/api.md` 'Errors'). Nothing else is caught.

## Readers and repositories

- A repository `GetAsync` returns `Result<T>` with the not-found error, scoped by claim id so a wrong claim is a not found.
- Readers project straight into DTOs and load no entity. Computed members (`IsSet`, `IsClaimant`), the marker of a converted closed set and range comparisons on converted values do not translate to SQL; equality against an instance and `OrderBy` do. List and search queries therefore project to DTOs or use flat read models mapped to the same table with plain nullable columns, and never filter a Domain entity with `AsEnumerable`.
- Reader facts: a pending row is `PendingApproval`; an unposted row is approved (Approved or AutoApproved) with posting Pending or Failed; an open balance is a component with an approved balance above zero; an approved reserve for payment is any transaction of the claim that is Approved or AutoApproved; the claim approved total is the net sum over all components.
- The job readers: `ISlaClaimReader` lists Draft or Open claims not updated for 48 hours with no breach entry in the last 24 hours; `IClaimReserveReader.ListStalePendingPostingsAsync` lists approved rows whose posting is still Pending and older than the given age.
- The claim number generator is a counter row per organisation and year, incremented with `UPDATE ... OUTPUT` inside the claim's transaction (S:CLAIMNO-Atomic), so there are no gaps. The format holds seven sequence digits, so the counter refuses past 9,999,999 with a conflict before `ClaimNumber.Create` would throw.
- The idempotency store is a table with a unique index on key, tenant, user, method and route, a request body hash, an in-progress marker, the response status and body, and the creation time; rows expire after about 24 hours through a cleanup job. An in-progress row older than a few minutes counts as abandoned, so a crash between the commit and the completion does not block retries for a day.

## Migrations and seed data

- Migrations are generated by tooling and never hand-edited; `dotnet ef migrations add` and `database update` need the user's go-ahead (`.claude/CLAUDE.md` 'Permissions model').
- Seed data (policies, cause-of-loss codes) uses `HasData` or migration scripts, never application startup. A GUID in seed data is allowed; a hard-coded GUID in logic is not (S:DATA-NoHardGuids).
- A naming test over the EF model checks that table names equal the specification's list and columns are PascalCase (S:DATA-Naming).
