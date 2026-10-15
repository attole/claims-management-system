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
| Adjusting a reserve | S:RES-EventSourced | never updated; a change appends a reserve change |
| Over-limit handling | S:BR-R-05 | warning at submit, manager override before approval |
| Status transitions | S:BR-ST-01 | the specification's table is the state machine |
| SLA job | S:JOB-SLA | writes an audit entry only; no extra status |
| Tenant column | S:DATA-Tenant | the specification's column name |
| Policy-period warning | S:BR-C-02 | cleared or acknowledged before opening |
| Claimant | S:BR-C-03 | required before opening, not before saving the Draft |
| Entity set | S:ENT-Set | the specification's tables, with the departures below |
| Reserve decision routes | S:API-POST-reserve-approve | the specification's routes, which name a reserve change |

### Interpretations

- A Draft is incomplete or unvalidated by definition, so it is saved with its problems stored as issues.
- A rule marked Critical means a stored Critical issue (S:VAL-Severity).
- Only a request that cannot be bound fails at the API edge; the sample 422 body fixes the error shape only.
- A reserve amount is the signed change the user typed.
- The "greater than zero" rule applies to the resulting balance, at submission and again at approval.
- A zero amount is refused (user decision).
- An approved reserve change has the approval Approved or AutoApproved.
- The 10 million in the roles table is the claim-wide cap, with a manager override (S:BR-R-02, S:BR-R-05).
- The claim total is the net sum over all reserves, with no exclusions.
- Reopened is a normal state; the Application runs the Open move right after the reopen.
- A policy period is two inclusive calendar dates, compared with the calendar date of the loss date in its own offset.
- The reported dates are set by the system at creation; the estimated loss amount is optional and has no bound.
- Which role may call which operation is decided in the Application; the documents state only the explicit rules (reopen, approval authority, the limit override).
- The specification's validation messages are used verbatim in Domain rule and check classes and in the transition finding, under a narrow exception in `CLAUDE.md`, and nowhere else.
- The specification's approval and posting status columns are states in the Domain.

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
| Reserve current amount | not stored; the sum of the reserve's approved changes, computed by a query | the spec calls it computed from history |
| Reserve posting failure reason (added) | the failure reason is stored with the job id in a posting details record on the reserve change | the reserves tab can show why a posting failed; the spec only audits it |
| Reserve review (changed) | who made the latest approval change, when, and the reason, on the reserve change instead of separate approved and rejected columns | a change is approved or rejected, never both, so half of the paired columns would always be empty |

### Decisions on gaps

- **Liability determined:** the status-change request carries an explicit confirmation, required for the move to PendingPayment and written to the audit entry; nothing is stored on the claim. The frontend needs a matching control.
- **Withdrawal and reopen reasons:** valid only with both a code and a text, stricter than the documents so every reason is reportable. The code is free text until a list exists.
- **Unspecified cases** (authentication failures, not found, concurrent edits, oversized uploads, idempotency-key replays) are decided when the API pack is designed and recorded here.

## Backend architecture

### Layers

Clean Architecture with five projects, dependencies pointing inward: `API` -> `Application` -> `Domain`. `Persistence` and `Infrastructure` implement interfaces that `Application` defines. The Domain has no project references and no packages.

- **Domain:** state, rules, single steps and events, plus the generic engine that runs them.
- **Application:** the use cases. It loads data, builds the input, computes the snapshot of values the rules need, runs the steps in order, chains follow-ups, saves through the Unit of Work and maps findings to responses.
- **Persistence:** EF Core mapping, with a save interceptor for audit, soft-delete and tenant fields.
- **Infrastructure:** Hangfire jobs and outside adapters.
- **API:** controllers and startup.

The Application and Persistence designs are written when those layers are built; the open points are in `.claude/temp/notes-layer-application.md` and `.claude/temp/notes-layer-persistence.md`.

### Domain and Application

The Domain says what is true and what may change. The Application says what happens, in which order and in which transaction.

- The Domain holds state, rules, single steps and events.
- The Domain never loads or saves, reads no clock, reads no other aggregate and never chains steps.
- The Application loads, builds the input, computes the snapshot, runs the steps in order, reads each outcome and saves.
- A true-or-false statement about state and request is a rule.
- A change to one aggregate's state is a step.
- A "then do X", a second aggregate, a load, a save, a clock, a user identity or a job belongs to the Application.
- A value that needs data from outside the aggregate is computed by the Application into the snapshot on the input.

### Domain model

- **Aggregates:** the claim is a root with children (parties, risk objects, loss, issues, state changes). A reserve and a reserve change are two roots. Aggregates refer to each other by id only.
- **One door in:** children have private setters and `internal` mutators; every change goes through a step that calls a root method, which keeps the invariants and raises a domain event.
- **Entity data in one record,** replaced whole, so an edit cannot half-apply and EF maps it as a complex property.
- **No null:** a value that may be missing is a record with an `Empty` instance and an `IsSet` flag, because EF cannot map a struct to a nullable column.
- **Ids and time:** ids are sequential GUIDs made in the Domain; the time of a run is on the input, stamped once by the Application.
- **A Draft may be incomplete;** what is missing is stored as validation issues instead of rejecting the save.
- **Closed sets:** a set that carries facts, or that a transition uses as a state, is a marker enum plus a record per value. A set with neither is a plain enum.
- **Claim numbers:** `ClaimNumber.Create(year, sequence)` formats and validates; `ClaimNumber.From` parses a stored number. The Application takes the sequence from the database counter.

### Domain engine

All domain logic goes through one small engine, because rules written as conditionals spread over handlers and entities turn every new rule into a search. The engine knows nothing about claims or reserves. Extending the domain means adding a declaration, not changing the engine or its callers.

The engine lives in `Domain/Core`. Each folder is a layer that uses only the layers above it, which the workflow check enforces; `Core` never references the aggregates.

| Layer | Folder | Idea |
|---|---|---|
| 1 | `Core/Base/Checks` | a check reports findings about a subject |
| 2 | `Core/Base/Flows` | the template that selects checks, evaluates them and applies |
| 3 | `Core/Rules` | a rule is a check with domain findings |
| 4 | `Core/Operations` | a business action on a subject; a creation operation has no subject yet |
| 5 | `Core/Transitions` | an operation that moves a subject between states |

**Checks and rules.**
- A check returns findings about a subject; none means it passed.
- A rule is a check whose findings are domain findings: a code, a message, blocking by default or advisory.
- A domain finding is `RuleFinding` (code, message, field) or `TransitionFinding` (source, target, valid targets).
- Stored validation issues are checks with their own finding type, because an issue is recorded and can be waived; it is not a refusal.

**Flows and operations.**
- A flow selects its checks, evaluates them, applies the decision on a blocked run (stop, skip or continue) and then applies.
- An operation is a flow for one business action. It declares its rules once and takes its data and the time of the run in an input record.
- The operation changes the subject only in its apply step, through the subject's `internal` methods.
- Creation is an operation without a subject, whose rules check the input.

**Transitions.**
- A transition is an operation that moves its subject to another state and returns the new state.
- It is configured by a target, the source states it is allowed from with the rules that apply from them, and common rules.
- A move from an undeclared state returns one finding without running any rule.
- A subject exposes its state through the `IStateful` capability.
- The aggregate's transition base does the state change: it sets the state and writes the history and the event.
- Who may make a move is a rule on the transition.
- The Application lists the valid next states from the declared sources and targets.

**Validation issues.**
- `EvaluateIssuesFlow` reconciles the stored issues with their checks: a new finding raises an issue, a resolved one is cleared, a waived one stays resolved while the problem remains.
- A Critical issue blocks opening; a waived one does not.
- The evaluation is idempotent, and the Application runs it after every claim change.

**Failures and boundaries.**
- Rule failures are findings, all returned at once.
- A broken invariant throws.
- Not found, forbidden and conflict belong to the Application.
- The engine loads nothing, saves nothing and decides nobody's permission.

**Extending.**
- A new state is a record.
- A new move is a transition class.
- A new rule is a class plus a line that requires it.
- A new action is an operation and its input.
- A new aggregate adds its own base input.
- The recipes are in `.claude/CLAUDE.md` 'Extending the Domain'.

### Claims

**Lifecycle.**

| Transition | Target | From | Rules |
|---|---|---|---|
| Open | Open | Draft | no blocking issues, an active claimant |
| Open | Open | UnderInvestigation | handler role |
| Open | Open | Reopened | none |
| Investigate | UnderInvestigation | Open | handler or supervisor role |
| Withdraw | Withdrawn | Open, UnderInvestigation | a reason with code and text |
| Reopen | Reopened | Closed | supervisor role, a reason |

- The PendingPayment and Closed moves, with the closure conditions, come in a later pack.
- Each move appends a `ClaimStateChange` row and raises `ClaimStateChangedEvent`.
- A reopen leaves two rows because the Application runs the Open move right after it.

**Edits.**
- Parties, risk objects, loss details, severity and handler are refused on Closed and Withdrawn claims.
- Notes are refused only on Withdrawn claims.

### Reserves

**Model.**
- A `Reserve` is a thin root: claim id, component type, status (Active or Closed, informational) and notes.
- A claim can have several reserves, including several of one component type.
- The component types are Indemnity, Expense, Alae and SubrogationRecoverable; only the recoverable type may go negative.
- A `ReserveChange` is its own root: reserve id, claim id, transaction type (Add, Adjust or Reverse), signed amount, reason, change sequence, submitter, approval state, posting state, review and balances.
- A reserve change refers to its reserve and claim by id; the reserve holds no collection of changes and no balance.
- A reserve's balance is the sum of its approved changes, computed by a query.
- The idempotency key of a change is `Reserve:{reserve id}:Change:{change sequence}`.

**Lifecycle of a change.**

| Step | Kind | Result |
|---|---|---|
| Submit | creation operation | a pending change, balances empty |
| AutoApprove | transition | PendingApproval -> AutoApproved |
| Approve | transition | PendingApproval -> Approved |
| Reject | transition | PendingApproval -> Rejected |
| Retract | transition | PendingApproval -> Cancelled |
| MarkPosted | transition | Pending or Failed -> Posted (posting state) |
| MarkPostingFailed | transition | Pending -> Failed (posting state) |
| CancelPosting | transition | Pending -> Cancelled (posting state) |

- A change is always created pending, like a claim is created as a Draft.
- The Application runs AutoApprove right after the submit; a refusal there only leaves the change pending.
- Approved, AutoApproved, Rejected and Cancelled changes never move again.
- A correction is a new Adjust or Reverse change.
- Every approval change stores the actor, time and reason in the review and raises `ReserveApprovalStateChangedEvent`.
- The change fills its balances (previous and new) when it becomes approved.
- The posting state starts Pending and moves independently of the approval state.

**Authority.**
- An amount up to 10,000 may be auto-approved, whatever the submitter's role.
- Approving up to 100,000 needs a supervisor or a manager; above that, a manager.
- Rejecting needs a supervisor or a manager and a reason.
- Retracting is the submitter's.
- Nobody approves their own change.
- A handler may submit above their own limit; the change waits as pending.
- A change that would take the claim total over 10 million is never auto-approved, even with the override flag set.
- At approval, such a change needs the manager override flag on the claim.
- At submit, the same case only returns a warning, which is not stored as an issue.

**Rules.** Every rule holds its own limit and reads the snapshot.

| Rule | Code | Used by |
|---|---|---|
| IsReserveAmountNonZero | RESERVE_AMOUNT_ZERO | submit |
| IsPolicyLinked | S:BR-C-06 | submit |
| HasNonNegativeBalance | S:BR-R-01 | submit |
| IsClaimTotalWithinLimit (advisory) | S:BR-R-05 | submit |
| IsWithinAutoApprovalLimit | S:BR-R-02 | auto-approve |
| IsClaimLimitCleared | S:BR-R-05 | auto-approve, approve |
| HasApprovalAuthority | S:BR-R-02 | approve |
| IsNotSelfApproval | S:BR-R-03 | approve |
| IsApprovedBalanceNonNegative | S:BR-R-01 | approve |
| HasApprovalRole | RESERVE_APPROVER_ROLE_REQUIRED | reject |
| HasRejectionReason | RESERVE_REJECTION_REASON_REQUIRED | reject |
| IsSubmitter | S:API-POST-reserve-retract | retract |
| IsReserveChangeApproved | RESERVE_CHANGE_NOT_APPROVED | mark posted, mark posting failed |
| IsReserveChangeRefused | RESERVE_CHANGE_NOT_REFUSED | cancel posting |

**Snapshot.** The Application computes these values by query and passes them on the input:
- Whether a policy is linked.
- The claim's approved total.
- The claim's override flag.
- The reserve's component type.
- The reserve's approved balance.
- The next change sequence.

**Concurrency.** Every reserve change also touches the claim row on save in Persistence, which serializes concurrent approvals on one claim and keeps the claim's last-activity time current.

**Posting.**
- The posting moves are transitions on the change, with its posting state as the subject.
- Posted and Failed need an approved change; a cancel needs a rejected or retracted one.
- A repeated post on a Posted change returns the not-permitted finding, which the job reads as already done.
- A Failed change can be posted again.
- Each move stores the job id and the failure reason in the posting details and raises `ReservePostingStateChangedEvent`.
- The job itself, its retries and the audit entries belong to Infrastructure and the Application.

### Application chains

The Domain runs one step at a time. The Application chains steps in one Unit of Work and reads each outcome before the next. The chains are:

- Any claim change, then `EvaluateIssuesFlow`.
- A reopen, then the Open move from Reopened.
- A reserve submit, then the auto-approve move. A refusal only leaves the change pending.
- An approval or auto-approval, then the posting job after the commit.
- A reject or retract, then the posting cancel move.

A handler that skips a step fails silently, so every chain has an Application test.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat entity-to-DTO and command-to-entity, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a recursive type is introduced.
