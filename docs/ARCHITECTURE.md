# Architecture

The design of the claims module and the decisions behind it. `.claude/CLAUDE.md` holds the rules, `docs/CODING-STANDARDS.md` the code style and `docs/layers/` one file per layer with its rules and contracts; this file holds the reasons.

## Overview

Clean Architecture with five projects, dependencies pointing inward: `API` -> `Application` -> `Domain`. `Persistence` and `Infrastructure` implement interfaces that the Application defines, and the Domain has no project references and no packages. State changes are commands and reads are queries (MediatR); a Unit of Work saves a change and dispatches the domain events it raised.

- **Domain:** three aggregates (the claim with its parties, risk objects, loss, issues and state changes; the reserve component with its history rows; the claim document), two reference entities (policy, cause of loss) and the roles. Aggregates refer to each other by id only. A small generic engine runs every change: rules first, then the change, with refusals returned as findings.
- **Application:** the use cases. A handler loads the aggregate and the outside facts, builds the input, calls one aggregate method, commits and maps the result. Validators check shape only; business rules stay in the Domain. Audit and the follow-ups of an event are handlers around the commit.
- **Persistence:** EF Core mapping, the readers that project straight into DTOs, the Unit of Work, and a save interceptor for the audit columns, soft delete, tenant and claim-row touch.
- **Infrastructure:** the Hangfire jobs (posting, SLA, the posting sweep, the idempotency cleanup, the orphan blob sweep) and outside adapters such as Blob storage.
- **API:** controllers, the error mapping, the Idempotency-Key filter and startup.
- **Deployment:** one Azure environment from Bicep and two GitHub Actions workflows, build and test, and a manual deployment of a tested build (`docs/layers/deployment.md`).
- **Frontend:** an Angular client with one page and one store per screen, standalone OnPush components on signals, and every HTTP call in hand-written typed services over types generated from the API's OpenAPI document. It validates shape and shows hints; every business decision is the server's, and a refusal comes back as a 422 mapped onto the form or a banner.

Each layer's rules and contracts are in `docs/layers/`. Key choices:

- **Entities with behavior, steps behind them:** the engine gives every method the same order and error handling, and a refused change can never half-apply. The cost is one more hop and a generic base.
- **Follow-ups chained on the outcome:** the order stays visible in the method, and a follow-up cannot refuse, so a condition that could is a rule on the step.
- **Facts passed in, not read:** plain values on the input keep the Domain free of infrastructure; fact definitions live in the Application.
- **The reserve component owns its history rows,** and there is no stored balance: the balance is the sum of the approved rows, as the specification describes.
- **Cross-aggregate effects by event:** a frozen claim closes its components through its state-changed event, so each aggregate guards itself from its own state.
- **No runtime rule engine:** rules are classes declared in code, visible to review and tests.
- **Client mirrors, server decides:** the frontend repeats a few constants for feedback before a request (field lengths, the approval tiers, the upload limit and file types); a stale copy only changes a hint, because the server still refuses.

## Configuration

Settings, secrets and feature switches follow `.claude/CLAUDE.md` 'Configuration'; the settings table is in the root `README.md`. Two points are specific to this design:

- The Hangfire dashboard is open to every request when its switch is on, because a page load carries no bearer token; it is on only in development and in the fake-data demo.
- `POST /api/dev/token` is the simulated sign-in the specification asks for (S:UI-General). It is open on purpose: the specification asks for a mock authentication with hardcoded users, so anyone who can reach the route can sign in as any configured user. That is acceptable only on fake data, and an extra access code was tried and removed because it burdened testers and reviewers more than it protected.

## Task documents

Where the specification and the assessment disagree, the specification is used and the assessment side is not implemented.

### Conflicts resolved for the specification

| Topic | Rule IDs | Decision |
|---|---|---|
| Reserve components | S:RES-Components | the specification's four kinds and its negative-balance rule |
| Reserve amount | S:BR-R-01 | a signed change; the balance may not go negative, except for the recoverable kind |
| Adjusting a reserve | S:RES-EventSourced | never updated; a change appends a history row |
| Over-limit handling | S:BR-R-05 | warning at submit, manager override before approval |
| Status transitions | S:BR-ST-01 | the specification's table is the state machine |
| SLA job | S:JOB-SLA | writes an audit entry only; no extra status |
| Tenant column | S:DATA-Tenant | the specification's column name |
| Policy-period warning | S:BR-C-02 | cleared or acknowledged before opening |
| Claimant | S:BR-C-03 | required before opening, not before saving the Draft |
| Entity set | S:ENT-Set | the specification's tables, with the departures below |
| Reserve decision routes | S:API-POST-reserve-approve | the specification's routes, which name a history row |

### Interpretations

How the documents are read and carried out where they are ambiguous, silent or in conflict. The kind says what sort of choice it is: `reading` (how an ambiguous statement is understood), `mechanism` (how a stated rule is carried out), `gap` (something the documents leave open, filled without a new refusal), `conflict` (the documents disagree and one side is chosen) and `departure` (a deliberate difference from the specification).

| Topic | Reading | Codes | Kind |
|---|---|---|---|
| Draft | incomplete by definition, so it is saved with its problems stored as issues; a rule marked Critical means a stored Critical issue | S:VAL-Severity | gap |
| API edge | only a request that cannot be bound fails there; the sample 422 body fixes the error shape only | S:API-ErrorShape | reading |
| Reserve amount | the signed change the user typed, a zero amount refused; "greater than zero" applies to the resulting balance at submission and again at approval | S:BR-R-01 | reading |
| Claim limit | the 10 million in the roles table is the claim-wide cap with a manager override; the claim total is the net sum over all reserves; a claim may hold several components, even of one type | S:BR-R-02, S:BR-R-05 | reading |
| Policy period | two inclusive calendar dates, compared with the UTC calendar date of the loss date | S:BR-C-02 | reading |
| Dates | the claim's reported date is the creation time, set by the system; the loss event's report date comes on the create input from intake and never changes | none | reading |
| Roles | nested as described (a manager holds every supervisor right), so a role rule names only the lowest role; which role may call which operation is decided in the Application, and the Domain holds only the explicit rules (reopen, approval authority, limit override, waivers) | S:ROLE-Supervisor, S:ROLE-Manager | reading |
| Role on moves | closing and moving to pending payment have no role rule, because the transition table lists none; reopen is the only claim move that reads the role | S:BR-ST-04 | reading |
| Validation messages | verbatim in Domain rules, the transition refusal and the tests that pin them, nowhere else; the closure message uses an ASCII hyphen | S:V-Messages | mechanism |
| Liability confirmation | the status-change request carries an explicit confirmation, required for PendingPayment and written to the audit entry; nothing is stored on the claim | LIABILITY_CONFIRMATION_REQUIRED | gap |
| Loss date | a Critical issue when later than the time of the change; every claim input carries that time (`Now`); the evidence is the loss date only, so the issue does not churn | S:BR-C-01 | mechanism |
| Waiver lifecycle | an issue matches its finding by code and evidence, so a waiver or acknowledgement lapses when the checked values change | none | mechanism |
| Moves see current issues | every move re-evaluates the issues before and after it; a refused move commits nothing, so its refreshed issues are not saved | S:BR-ST-02, S:CC-02 | mechanism |
| Drafts stay | the transition table has no exit from Draft except Open; the SLA job flags an abandoned one | S:BR-ST-01 | reading |
| Claim numbers | a counter row per organisation and year, incremented atomically in the claim's transaction; the format holds seven digits | S:CLAIMNO-Atomic | mechanism |
| Unknown policy at intake | the claim is created without a policy and returns a warning; linking an unknown policy to an existing claim is a not found | POLICY_NOT_FOUND | gap |
| No risk object, no policy | Warning issues; both use the validation table's wording | CLAIM_NO_RISK_OBJECT, S:INTAKE-NoPolicy | reading |
| History row | a child of its component; the component computes the change sequence and the approved balance; the idempotency key is `Reserve:{component id}:Change:{sequence}`, backed by a unique index | S:BR-R-06 | mechanism |
| Reject authority | follows the approval tier: the role table wins over the reject route's role requirement; the refusal has its own message | S:BR-R-02 | conflict |
| Transaction types | Add, Adjust and Reverse are labels; the signed amount alone drives balances, limits and tiers | none | gap |
| Auto-approval | no reviewer is recorded, so the history never shows the submitter as approver | S:BR-R-03 | mechanism |
| Approved reserve for payment | any transaction of the claim that is Approved or AutoApproved; the Application reader owns the definition | S:ST-Open-PendingPayment, S:ST-UnderInvestigation-PendingPayment | reading |
| Amount messages | the amount and balance refusals use their own wording, because the specification's message assumes positive amounts | none | departure |
| Reversal during payment | allowed; a reserve is independent of the claim state | none | gap |
| Document aggregate | its own root, frozen with the claim; the size limit, the content-type allowlist and the content signature are Domain rules (the Application reads the first bytes and hands them in on the input); the Application builds the path and talks to storage | S:DOC-MaxSize, S:DOC-MimeAllowlist | mechanism |
| Blob name | the specification's path with a random upload id before the file name (`{uploadId}_{name}`): a path is unique by construction, so a failed commit or a removed document never blocks a name, and an orphan blob only costs storage | S:BR-D-01 | departure |
| Audited events | the specification's fixed list, plus five more types (issue acknowledged, issue waived, limit override changed, reserve component state changed, document removed) kept apart in `AuditEventTypes.Additional`, because a waiver or an override is a significant action (S:BR-A-02); other edits leave their trace in the audit columns | S:AUD-Events | gap |
| Guarded times and actors | a missing time or actor, or a business time earlier than the one it follows, throws | none | mechanism |
| Error body | an error without a field goes under the key `General`, set in the API's one error mapper | S:API-ErrorShape | gap |
| Claim list date range | filters the loss date of the claim | none | gap |
| Unspecified cases | authentication failures, concurrent edits, idempotency-key replays and oversized uploads, decided in `docs/layers/api.md` 'Errors', 'Idempotency-Key' and 'Documents' | none | gap |
| Reserve request | one request opens or adjusts a component: `componentType` opens a new one (always an Add), `componentId` adds a row to an existing one, exactly one of the two; the specification names a single component field | S:API-POST-reserves | departure |

### Schema departures from the spec

Table names are the spec's (section 9, listed in `docs/layers/persistence.md`); the entity classes keep their Domain names and map to them with `ToTable`.

| Change | Departure | Why |
|---|---|---|
| Loss date, cause code | nullable loss date, cause code as text without a foreign key | a Draft may be incomplete; an unknown code is a stored issue (S:BR-C-05) |
| Closed date, closure reason | dropped; derived from the latest change into Closed | the state-change history already holds them |
| `ClaimValidationIssue` (added) | stored child: the finding, its state (raised, cleared, acknowledged, waived), who resolved it and when | the spec describes recorded issues but lists no entity |
| `ClaimStateChange` (added) | append-only child: from, to, optional reason (code and text), note | reasons and the closing note have no columns |
| Limit override flag (added) | boolean on the claim, set by a manager-only operation | the claim-wide cap needs a flag (S:BR-R-05) |
| Policy cancellation date (added) | nullable column; the coverage window ends at the earlier of the end and the cancellation date | a status alone cannot place a loss before a cancellation |
| Claim severity | own type with an "unspecified" start | the documents give no setter, default or rule |
| Primary keys | sequential GUIDs generated in the Domain; Persistence also declares the `NEWSEQUENTIALID()` default | aggregates and events have ids at creation |
| Reserve current amount | not stored; the sum of the component's approved history rows | the spec calls it computed from history |
| Reserve component status | an Active or Closed state that follows the claim | the spec gives the column but no rule |
| History row (added columns) | the component type, the submission time and the posting failure reason | approval reads the type from the row; a review is checked against the submission; the tab shows why a posting failed |
| Reserve review (changed) | who made the latest approval change, when and why, on the row instead of paired approved and rejected columns | a row is approved or rejected, never both |
| Policy status | no column; the policy reader fills it from `PolicyPeriod.StatusOn(today)` | the specification seeds dates only, and a stored value would go stale at the end date with nothing to refresh it |
| History idempotency key | not stored and not unique-indexed; the row computes it from the component and the change sequence | EF 9 cannot index a member of a complex property, and the component's `RowVer`, moved by the root touch, already stops two writers using the same sequence |
| Soft delete on history | the state-change and reserve-history tables have no `IsDeleted` or `DeletedAt` and refuse a delete | history rows are never removed, so the history stays complete |
| Deletion time | `DeletedAt` is a Persistence column stamped by the save interceptor; the Domain keeps only `IsDeleted` | no rule reads it |
| Tables outside section 9 (added) | `ClaimIssues`, `ClaimStateChanges`, `PolicyCoverageTypes`, `ClaimNumberCounters`, `IdempotencyKeys` | stored issues, state-change history, a policy's coverage types, the atomic claim number and the Idempotency-Key store have no table in the specification |
| Cause code sort order | a shadow column; the Domain holds no order | display order is a read concern |
| Document soft delete (added) | `IsDeleted` and `DeletedAt` on the document | the spec lists no delete column |
| Claim list view (added) | the database view `ClaimListView` joining claims, loss events and policies, created by the second migration | the list filters, sorts and pages on columns of three tables in one query |
| Claim policy | `Claims.PolicyId` is a non-null id with the empty id for "no policy", and has no foreign key to `Policies` | the Domain keeps no null and EF cannot map a non-nullable property as an optional foreign key; the Application checks that a policy exists when it is linked, and policies are never deleted |
| History delete behaviour | the foreign keys of the audit log, the state changes and the reserve history restrict a delete (the third migration); the other children still cascade | a hard delete of a claim must never take its audit trail or its history with it |
| Audit table permission (added) | update and delete on `ClaimAuditLog` are denied to the database role `claims_app`, created by the second migration | the spec forbids updating or deleting audit rows; the deployment adds the app's database user to the role (S:AUD-NoUpdateDelete) |

### Decisions on gaps

Conditions and constants that no document states and that this repo added. Each can be confirmed or removed.

| Area | Topic | Rule | Codes |
|---|---|---|---|
| Claim | Reasons | withdrawal and reopen need both a code and a text; the code is free text until a list exists | STATE_CHANGE_REASON_REQUIRED |
| Claim | Loss report date | required on create | LOSS_REPORT_DATE_REQUIRED |
| Claim | Edits after Draft | a loss update that would leave a Critical loss issue is refused; a waived problem stays allowed | LOSS_INCOMPLETE_AFTER_DRAFT |
| Claim | Handler | a Draft opens only with an assigned handler, which cannot be removed after Draft; a new claim is created with its creator as the handler | CLAIM_HANDLER_REQUIRED, CLAIM_HANDLER_NOT_KEPT |
| Claim | Back to Open | a move from UnderInvestigation to Open meets the conditions of opening a Draft (no unresolved critical issue, no unacknowledged policy-period warning, a claimant, a handler); a move from Reopened stays unconditional | S:BR-ST-02 |
| Claim | Frozen claims | a closed or withdrawn claim refuses edits to its data, reserves and documents, and its components close and reopen with it; notes stay editable | CLAIM_NOT_EDITABLE, RESERVE_CLAIM_NOT_EDITABLE, DOCUMENT_CLAIM_NOT_EDITABLE |
| Claim | Waivers | supervisor or manager only; a missing loss date, an inactive cause code and a missing claimant can never be waived | ISSUE_MUST_BE_FIXED |
| Claim | Primary risk object | only one active risk object is primary | RISK_OBJECT_PRIMARY_EXISTS |
| Claim | Party names | shape validation in the Application: a first or last name for a person, a company name for a company | none |
| Claim | Party email | an email, when given, must be a well-formed address; the Application validator and the party form both check it | none |
| Claim | Policy cancellation | the coverage window ends at the cancellation date when earlier; a period names its policy and throws when used for another | none |
| Reserve | Components follow the claim | closing or withdrawing closes every component, reopening reopens them, in the same Unit of Work; a closed component refuses a new row | RESERVE_COMPONENT_NOT_ACTIVE |
| Reserve | Withdraw | refused while a row awaits approval or an approved row is unposted; with an open balance it needs a note and warns | CLAIM_WITHDRAW_PENDING_RESERVE, CLAIM_WITHDRAW_NOTE_REQUIRED, CLAIM_WITHDRAWN_WITH_OPEN_BALANCE |
| Reserve | Close | refused while an approved row is unposted (Pending or Failed); this makes the posting re-enqueue sweep a requirement | RESERVE_HISTORY_NOT_POSTED |
| Reserve | Claim limit | a change that does not increase the total passes the limit; one that would pass it is never auto-approved; the override cannot be removed while the total is over | CLAIM_OVERRIDE_STILL_NEEDED |
| Reserve | Limit override out of scope | the manager-only override is modelled, enforced and audited in the Domain but has no route or screen in this slice, so an approval that would pass the claim limit is refused | S:BR-R-05 |
| Reserve | Tiers | the auto-approval limit and the approval tiers compare the absolute amount; a handler may submit any amount and the row waits for an approver | S:BR-R-02 |
| Reserve | Posting moves | posted, failed and retry need a job id; cancel has none | RESERVE_POSTING_JOB_REQUIRED |
| Reserve | Intake without a policy | an intake with an initial reserve and no linked policy is refused as a whole (the create is one transaction); the intake form blocks it before sending and links back to the policy step | S:BR-C-06 |
| Document | Content signature | one table in `DocumentContentTypes` lists every allowed type with its signature, so the allowlist and the signature check cannot drift apart; the first 512 bytes must match the declared type: `%PDF` for PDF, the PNG and JPEG headers, a ZIP header for docx and xlsx, no NUL byte for text and CSV; a ZIP header alone does not tell a docx from any other ZIP | DOC_CONTENT_MISMATCH |
| Policy | Status | shown only: Active up to the effective end date (the earlier of expiry and cancellation), then Cancelled when a cancellation ended the cover, otherwise Expired; the date is an argument, the Domain reads no clock | none |
| Read | Recent audit entries | the claim detail shows 10 | none |
| Read | Page size cap | a page holds at most 100 rows | none |
| UI | Handler filter | the claims list filters the handler with a select of the known users instead of a text search, because the list API filters by handler id and the summary rows carry no handler name | S:UI-List |
| Job | Batch caps | one SLA run flags at most 500 claims and one posting sweep re-enqueues at most 500 rows; the rest wait for the next run | none |

## Backend details

### Engine

The engine's shapes and vocabulary are in `docs/layers/domain.md` 'Domain engine'; this is why it is built that way.

- A step declares its rules once, checks all of them before it changes anything and returns an `Outcome`, so a refused change never half-applies and every method has the same order and error handling.
- An aggregate method is one step plus follow-ups chained on the outcome (raise the event, `Settle`); a follow-up cannot change the outcome, so a condition that could refuse is a rule on the step.
- A rule failure is a finding; a broken invariant throws. Not found, forbidden and conflict belong to the Application.
- A closed set (a state, a role, a type) is a marker enum plus a record per value; a test loops over every value, so a variant without its enum value fails the build.

### Inputs

The Domain loads nothing and knows no identity. Everything a change needs that the aggregate does not hold arrives on the input, computed by the Application before the call.

| Input | Outside facts |
|---|---|
| every claim input | `PolicyPeriod`, `CauseCodes` and `Now`, because every claim change re-evaluates the issues |
| `CreateClaimInput` | the organisation, the claim number from the counter, the formal report date |
| `FreezeClaimInput`, `SendToPaymentClaimInput` | the plain reserve facts (a row pending, a row unposted, a balance open, an approved reserve) |
| `ReopenClaimInput`, `RejectHistoryInput`, `WaiveIssueInput` | the actor's role |
| `SetLimitOverrideInput`, `ApproveHistoryInput` | the claim approved total, the role and the override flag |
| `OpenReserveComponentInput`, `UploadClaimDocumentInput` | `ClaimReference`: the claim's id, organisation and state |
| `SubmitHistoryInput` | the claim's state, whether a policy is linked and the claim approved total |

- The two claim facts stay separate parameters because they come from different readers. A reference record exists only where another aggregate needs several facts from one aggregate together (`ClaimReference`); later steps take only the state.
- The history inputs come in two layers: the public input carries the row id and the request data, and the component's step builds the row's internal input, adding the balance from its own state.
- The definition of each fact (for example "approved" means Approved or AutoApproved) lives in one Application query with its own tests.

### Rules

Rules are classes with a source-qualified or UPPER_SNAKE `Code` (shape in `docs/layers/domain.md` 'Code shapes'). The finding states its severity and the step decides the consequence: a step stops on a Critical finding, while the issue evaluation stores each finding as an issue. A warning that must block is its own rule with its own Critical finding.

Reserve authority: up to 10,000 may be auto-approved, approving up to 100,000 needs a supervisor, anything above needs a manager, nobody approves their own row, and a change that would take the claim over 10 million needs the manager override.

### Request flow

One write request, top to bottom (a read skips the filter, the validator and the Unit of Work):

```text
HTTP request
    middleware: correlation id, exception handler, HTTPS, CORS, authentication, current user, authorization
    Idempotency-Key filter (a write with the header)
    controller action: maps its request record to a command
    MediatR pipeline: the FluentValidation behavior checks the shape
    handler: loads the aggregate and the outside facts, builds the input, calls one aggregate method
    aggregate: runs the rules, changes its state, raises its events
    Unit of Work: before-commit handlers (audit), one save, after-commit handlers (enqueue a job)
    controller: maps the Result to { data, warnings } or to the error body
```

### Domain events and their handlers

The Unit of Work dispatches the events an aggregate raised: before the save for the handlers that must commit with the change (audit, the components following a claim), after it for the handlers that start outside work.

| Event | Before commit | After commit |
|---|---|---|
| `ClaimCreatedEvent` | audit CLAIM_CREATED | none |
| `ClaimStateChangedEvent` | audit STATUS_CHANGED (and CLAIM_CLOSED or CLAIM_REOPENED); the reserve components close or reopen with the claim | none |
| `PartyAddedEvent`, `PartyRemovedEvent` | audit PARTY_ADDED, PARTY_REMOVED | none |
| `ValidationIssueRaisedEvent` | audit VALIDATION_ISSUE_ADDED | none |
| `ValidationIssueAcknowledgedEvent`, `ValidationIssueWaivedEvent` | audit of the same names (additional types) | none |
| `ValidationIssueClearedEvent` | none | none |
| `AggregateLimitOverrideChangedEvent` | audit LIMIT_OVERRIDE_CHANGED | none |
| `ReserveHistorySubmittedEvent` | audit RESERVE_CREATED | none |
| `ReserveHistoryApprovalStateChangedEvent` | audit RESERVE_AUTO_APPROVED, RESERVE_APPROVED, RESERVE_REJECTED or RESERVE_RETRACTED | enqueue the posting job when the row is approved |
| `ReserveHistoryPostingStateChangedEvent` | audit GL_POSTING_SIMULATED or GL_POSTING_FAILED, with the simulated journal | enqueue the posting job again when a failed row returns to pending |
| `ReserveComponentStateChangedEvent` | audit RESERVE_COMPONENT_STATE_CHANGED (additional type) | none |
| `ClaimDocumentUploadedEvent`, `ClaimDocumentRemovedEvent` | audit DOCUMENT_UPLOADED, DOCUMENT_REMOVED (additional type) | none |

### Background jobs

Posting (the GL simulation), SLA detection, the posting sweep, the Idempotency-Key cleanup and the orphan blob sweep are described, with cadence, retries and idempotency, in `docs/layers/infrastructure.md` 'Jobs'. A job holds no business logic: it sends a command through MediatR with its own scope and tenant.

### Data model

Every business table carries the tenant column, the audit columns and a soft delete flag (the history tables refuse a delete). Relationships, one line per parent:

```text
Policies 1 - * PolicyCoverageTypes
Claims 1 - 1 LossEvents
Claims 1 - * ClaimParties, ClaimRiskObjects, ClaimIssues, ClaimStateChanges, ClaimDocuments, ClaimAuditLog
Claims 1 - * ClaimReserveComponents 1 - * ReserveHistory
Claims * - 1 Policies (a claim without a policy keeps an empty id)
Claims.CauseOfLossCode refers to CauseOfLossCodes by code, without a foreign key
ClaimNumberCounters: one row per organisation and year
IdempotencyKeys: one row per user, key, method and route
```

## Known risks

### AutoMapper 14.0.0 advisory

- AutoMapper is pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change. Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and the patched versions are commercial.
- Accepted risk: exploitation needs a deeply recursive or cyclic object graph reaching a map, and the mapping here is flat entity-to-DTO with input bounded by FluentValidation. DTOs stay non-recursive, with no self-referencing maps.

### Signature check depth

- A docx or xlsx file is recognised by its ZIP header only, so any ZIP file declared as one passes the content check.
