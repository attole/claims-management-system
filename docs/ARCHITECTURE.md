# Architecture

## Configuration

Settings, secrets and feature switches follow `.claude/CLAUDE.md` 'Configuration'; the settings table and setup steps are in the root `README.md`. Two points are specific to this design:

- The Hangfire dashboard is open to every request when its switch is on, because a page load carries no Bearer token, so enabling it in a shared environment exposes job data to anyone with the URL.
- `DevAuth:Enabled` is bound but nothing reads it until the dev token endpoint exists.

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

- A Draft is incomplete or unvalidated by definition, so it is saved with its problems stored as issues, and a rule marked Critical means a stored Critical issue (S:VAL-Severity). Only a request that cannot be bound fails at the API edge; the sample 422 body fixes the error shape only.
- A reserve amount is the signed change the user typed; the "greater than zero" rule applies to the resulting balance, at submission and again at approval. An approved reserve is a history row with the status Approved or AutoApproved.
- The 10 million in the roles table is read as the claim-wide cap with a manager override; a manager approves any amount above the supervisor limit (S:BR-R-02, S:BR-R-05).
- Reopened is a normal state that moves on to Open in the same operation, leaving two state-change rows.
- A policy period is two inclusive calendar dates, compared with the calendar date of the loss date in its own offset. The reported dates are set by the system at creation; the estimated loss amount is optional and has no bound.
- Which role may call which operation is decided in the Application layer; the documents state only the explicit rules (reopen, approval authority, the limit override).
- The specification's validation messages are used verbatim in Domain rule and check classes and in the transition finding, under a narrow exception in `CLAUDE.md`, and nowhere else.

### Schema departures from the spec

| Change | Departure | Why |
|---|---|---|
| Loss date, cause code | nullable loss date, cause code as text without a foreign key | a Draft may be incomplete; an unknown code is a stored issue (S:BR-C-05) |
| Closed date, closure reason | dropped; derived from the latest change into Closed | the state-change history already holds them |
| `ClaimValidationIssue` (added) | stored child: rule id, message, severity, state (raised, cleared, acknowledged, waived) | the spec describes recorded issues but lists no entity |
| `ClaimStateChange` (added) | append-only child: from, to, optional reason (code and text), closing note, user, time | reasons and the closing note have no columns |
| Limit override flag (added) | boolean on the claim, set by a manager-only operation | the claim-wide cap needs a flag (S:BR-R-05) |
| Policy cancellation date (added) | nullable column; the coverage window ends at the earlier of the end and the cancellation date | a status alone cannot place a loss before a cancellation |
| Claim severity | own type with an "unspecified" start; a plain operation sets it | the documents give no setter, default or rule |
| Primary keys | sequential GUIDs generated in the Domain, never by the database | aggregates and events have ids at creation |

The reserve history table (one append-only row per change) and the reserve components are used as the specification defines them; a claim can have several components, including several of one kind, and each component's current amount is recomputed from its approved rows.

### Decisions on gaps

- **Liability determined:** the status-change request carries an explicit confirmation, required for the move to PendingPayment and written to the audit entry; nothing is stored on the claim. The frontend needs a matching control.
- **Withdrawal and reopen reasons:** valid only with both a code and a text, stricter than the documents so every reason is reportable. The code is free text until a list exists.
- **Unspecified cases** (authentication failures, not found, concurrent edits, a reserve without a policy, oversized uploads, who may retract a pending reserve, idempotency-key replays) are decided when the API pack is designed and recorded here.

## Backend architecture

### Layers

Clean Architecture with five projects, dependencies pointing inward: `API` -> `Application` -> `Domain`, and `Persistence` and `Infrastructure` implement interfaces that `Application` defines. The Domain has no project references and no packages.

- **Domain:** the business model and every business rule, plus the generic engine that runs them.
- **Application:** turns a request into a Domain call: loads data, builds the operation input (stamping the time once), runs the operation, saves through the Unit of Work, converts the outcome, and decides who may call what.
- **Persistence, Infrastructure, API:** EF Core mapping with a save interceptor for audit, soft-delete and tenant fields; Hangfire jobs and outside adapters; controllers and startup.

The Application and Persistence designs are written when those layers are built; the open points are in `.claude/temp/notes-layer-application.md` and `.claude/temp/notes-layer-persistence.md`.

### Domain model

The claim is the aggregate root; what must stay consistent with it is a child that only the root changes, and other aggregates refer to it by id.

- **One door in.** Children have private setters and `internal` mutators, and the root exposes read-only views. Every change goes through an operation that calls a root method, which keeps the invariants and raises a domain event for a significant change.
- **Entity data in one record,** replaced whole, so an edit cannot half-apply and EF maps it as a complex property.
- **No null.** A value that may be missing is a record with an `Empty` instance and an `IsSet` flag, because EF cannot map a struct to a nullable column.
- **Ids and time.** Ids are sequential GUIDs made in the Domain. The Domain reads no clock: the time of a run is on the input, stamped once by the Application.
- **A Draft may be incomplete;** what is missing is stored as validation issues instead of rejecting the save.
- **Lifecycle as a closed set:** a state is a record per value, changed only by a transition.

### Domain engine

All domain logic goes through one small engine, because rules written as conditionals spread over handlers and entities turn every new rule into a search. The engine knows nothing about claims; it runs rules against a subject and moves a subject between states. Extending the domain means adding a declaration, not changing the engine or its callers.

The engine lives in `Domain/Core`. Each folder is a layer that uses only the layers above it, which the workflow check enforces; `Core` never references `Claims`.

| Layer | Folder | Idea |
|---|---|---|
| 1 | `Core/Base/Checks` | a check reports findings about a subject |
| 2 | `Core/Base/Flows` | the template that runs checks around a step |
| 3 | `Core/Rules` | a rule is a check with domain findings |
| 4 | `Core/Operations` | a business action on a subject; a creation operation has no subject yet |
| 5 | `Core/Transitions` | an operation that moves a subject between states |

Each aggregate adds its own bases on top, in its `Root/Bases` folder, for what every change of it shares (the claim has a base input, base operation, base transition and the transition input).

**Checks and rules.** A check returns findings about a subject; none means it passed. A rule is a check whose findings are domain findings: a code, a message, blocking by default or advisory. Operations and transitions require rules. Stored validation issues are checks with their own finding type, because an issue is not a refusal: it is recorded, shown and can be waived. Folders say which is which (`Rules/`, `Checks/`).

**Flows and operations.** A flow runs preconditions, rules, and an apply step. Where a blocked run ends is a decision declared on the flow (stop, skip or continue). An operation is a flow for one business action: it declares its rules once, takes its data and the time of the run in an input record the Application fills, and changes the subject only in its apply step, through the subject's internal methods. An aggregate's base operation adds what every change needs, such as re-evaluating its issues. Creation is an operation without a subject whose rules check the input; it creates and evaluates in its apply step.

**Transitions.** A transition is an operation that moves its subject to another state, which makes the engine a state machine. It is configured, not programmed: a target, the source states it is allowed from with the rules that apply from them, and optional common rules. A move from an undeclared state always stops with a finding, and a transition cannot change how it runs, so it cannot skip a rule, the history row or the event. A subject exposes its state through the `IStateful` capability, which is how the engine reads it. The aggregate's transition base supplies the state-change history row, the state-changed event and the follow-up; a state can be a pass-through state that moves on at once (Reopened moves on to Open in the same run, with a row and an event for each move). Who may make a move is a rule on the transition, so each move configures its own roles. The finding names the source and the target; the Application lists the valid next states from the declared sources and targets of all transitions.

**Validation issues.** Checks run after every claim change and reconcile the stored issues with their findings: a new finding raises an issue, a resolved one is cleared, a waived one stays resolved while the problem remains. A Critical issue blocks opening; a waived one does not. The evaluation is idempotent.

**Failures and boundaries.** Rule failures are findings, all returned at once. Errors that are not about a domain rule (not found, forbidden, conflict) belong to the Application, and a broken invariant throws. The engine loads nothing, saves nothing and decides nobody's permission: input builders load snapshots, the Unit of Work saves and dispatches events, the Application checks roles, the API maps findings to responses.

**Extending.** A new state is a record, a new move is a declaration on the target's transition, a new rule is a class plus a line that requires it, a new action is an operation and its input, a new aggregate adds its own bases. The recipes are in `.claude/CLAUDE.md` 'Extending the Domain'.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat entity-to-DTO and command-to-entity, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a recursive type is introduced. 