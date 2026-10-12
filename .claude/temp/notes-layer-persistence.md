status: active

Persistence-layer notes moved out of design-domain-model.md. The Persistence project holds only the options and registration skeleton today; read this note when its packs begin. Most findings come from a throwaway EF Core 9.0.20 spike (SQL Server provider for the model and script, SQLite in memory for round trips); the spike is outside the repo.

OPEN

- The SLA job's 24-hour dedupe needs the latest breach entry per claim; it reads the audit log by claim and event type, which needs an index there (S:JOB-SLA-Dedupe).
- Claim number sequence per year or global is open.
- Indexes on columns inside a complex type (for example on party role or email) were not checked; verify before relying on one.
- The other nullable columns need the same check as the loss date and the estimated amount before they are mapped.

PROVISIONAL

- Claim number generation is an Application interface implemented in Persistence by an atomic database sequence (S:CLAIMNO-Atomic); the Domain record only validates the format (S:BR-C-04).
- Draft-tolerant schema (deviation from the spec's table, S:ENT-Set): the loss date has no value until set (an empty value in the Domain, a nullable column) and the cause code is stored as text with no database foreign key. Draft to Open requires a real date and an existing active code (S:BR-ST-02).
- Closed sets: Persistence holds the EF converters (marker text out, record in), built with the Application closed-set service that the DbContext receives by constructor. No design-time factory is planned; add a small one only if migrations cannot create the context. Plain enums use EF's built-in text conversion.
- Details values: LossEvent holds LossDetails, ClaimParty holds PartyDetails and ClaimRiskObject holds RiskObjectDetails. Each is mapped as a complex property into the entity table and replaced as a whole (a spike confirmed that only the changed members are marked modified; two entities sharing one details instance saved without error). The default column names carry a Details_ prefix; give every member HasColumnName to match the spec table, and HasPrecision(19, 4) on the amount.
- Every entity has a private parameterless constructor with empty placeholders, because EF cannot bind domain constructors that take values it does not map: Claim (ClaimNumber.Empty, a Draft state, severity Unspecified, an empty LossEvent), LossEvent, ClaimParty and ClaimRiskObject. A Domain test builds each through it. The other private Claim constructor is the factory's and is not usable by EF.
- Nullable columns on non-nullable struct properties are refused by EF, so absent values are sealed record classes with an Empty instance: LossDate and EstimatedLossAmount (and ClaimNumber and LossDetails for placeholders). The converter maps NULL to Empty and Empty to NULL (ConvertsNulls) and the property is declared optional; the spike confirmed a nullable datetimeoffset column and a Draft round trip for the loss date.
- Estimated loss amount: the spec column is nullable, so no estimate is not an estimate of 0 (user decision). Map it as a converted scalar on a nullable decimal(19,4) column, NULL <-> Empty, and check that a stored zero reads back as a set estimate. This part was not spiked.
- Closed sets inside the details (party role and type) are converted scalars (text with a maximum length); the spike used a plain switch in place of the registry.
- Audit, soft-delete and tenant fields (user decision): the capability interfaces IAuditable, ISoftDeletable and ITenantOwned are get-only and the entities have private setters, so outside code cannot write them. The Domain sets what it owns in its own methods (the soft-delete flag in the remove methods, the update time in Touch). The save interceptor stamps the rest through the EF property API, for example Entry(e).Property(nameof(IAuditable.UserModified)).CurrentValue = actor; the spike confirmed that this writes private-setter properties and persists them.
    - Pitfall: the update time may be stamped by both Touch and the interceptor. The interceptor should leave a value the Domain already set at the operation time, or take the time from the same source, so the two cannot disagree.
    - The interceptor needs the current user and the time from Application interfaces (not the Domain). The names are used as strings, so a test against a real context must cover every interface member.
    - Soft delete: the global query filter reads the flag through the interface property; a hard Remove on a tracked entity should be turned into the soft-delete flag by the interceptor only for entities that do not do it themselves.
    - If four copies of the same property block become a burden, a shared base class can hold them later without changing the interceptor.
- Queries: computed members (IsSet, CountsAsClaimant), the marker of a converted closed set and range comparisons on converted values do not translate to SQL. Equality against an instance and OrderBy do translate. List and search queries therefore do not use the Domain entities: they project to DTOs, or use flat read models mapped to the same table with plain nullable columns (for example a nullable date column for loss date ranges), and filter on plain columns. Never filter a Domain entity with AsEnumerable; that loads rows into memory. Domain rules run on a loaded aggregate and are unaffected.
- Concurrency: any change to a child (party, document, reserve, status history) also touches the claim row, so its updated time and RowVer change (see the design note); the RowVer token must be configured on the claim and the reserve component.

NEXT

- Write the entity configurations with the points above, then run a real-database check of the converters, the nullable columns and the interceptor.
- Migrations are generated by tooling and need the user's go-ahead (CLAUDE.md 'Permissions model').
