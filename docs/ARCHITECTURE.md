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
- Reopened is a normal state; the claim's reopen method runs the Open move right after the reopen.
- A policy period is two inclusive calendar dates, compared with the UTC calendar date of the loss date, because all timestamps are stored in UTC.
- The reported dates are set by the system at creation; the estimated loss amount is optional and has no bound.
- Which role may call which operation is decided in the Application; the documents state only the explicit rules (reopen, approval authority, the limit override).
- The specification's validation messages are used verbatim in Domain rule and check classes and in the transition finding, under a narrow exception in `CLAUDE.md`, and nowhere else.
- The specification's approval and posting status columns are states in the Domain.
- The closure message uses an ASCII hyphen where the specification has a dash.
- Closing and moving to pending payment have no role rule, because the specification's transition table lists none.

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

- **Domain:** the aggregates, their rules and events, and the small engine that runs a change.
- **Application:** the use cases. It loads data, builds the input with the facts the rules need, calls one aggregate method, reads its outcome, saves through the Unit of Work, dispatches events and maps findings to responses.
- **Persistence:** EF Core mapping, with a save interceptor for audit, soft-delete and tenant fields.
- **Infrastructure:** Hangfire jobs and outside adapters.
- **API:** controllers and startup.

The Application and Persistence designs are written when those layers are built; the open points are in `.claude/temp/notes-layer-application.md` and `.claude/temp/notes-layer-persistence.md`.

### The domain model

The Domain is a domain-driven model: the claim with its children, the reserve and the reserve change are entities that own their state and change it only through their own public methods. The Application calls `claim.AddParty(input)` and the claim decides what that means.

Inside those methods every change runs through one small engine, which exists for error handling and order. A flow evaluates all of its rules before it changes anything and returns the refusals as findings, all at once and never as exceptions, so a refused change leaves the aggregate untouched; it applies its change in one place, so "rules, then the change, then what follows" is the same order for every method. Rules are named classes, and a new one is a class plus the line that requires it. The engine knows nothing about claims or reserves.

| Concern | Where it lives | How |
|---|---|---|
| State and its changes | the aggregate, an entity | public methods; `internal` setters and child mutators; lifecycles as closed sets |
| The conditions of a change | rules, internal classes with a `Code` | declared once per step; a failed rule is a finding |
| Running a change | flows: operations, transitions, whole-entity flows | rules first, then `Apply`; the result is a `FlowOutcome` |
| What follows a successful change | the aggregate method's chain | `OnSuccess` hooks: raise the event, record the move, run an inner step, settle |
| Facts from outside the aggregate | a snapshot on the input | computed by the Application before the call |
| Effects beyond the aggregate | domain events | handled by the Application around the commit |

### How a change runs

```csharp
public FlowOutcome<ClaimParty, DomainFinding> AddParty(AddPartyInput input)
{
    return AddPartyOperation.Execute(this, input)
        .OnSuccess(party => Raise(new PartyAddedEvent(input.Stamp(), Id, party.Id, party.Details.Role.Name)))
        .OnSuccess(() => Settle(input));
}
```

- The step evaluates its rules. If any blocks, the outcome carries every blocking finding, nothing has changed, and the chain falls through.
- On success the follow-ups run in the written order: the event the step owns, a move record, an inner step of the same aggregate, and `Settle` last (touch and recompute derived state, the claim's issues).
- The method returns the step's outcome; a follow-up never changes it, so a condition that could refuse is a rule on the step. The Application reads `IsSuccess`, `Value`, `Blocking` and `Advisory` and never looks inside the aggregate.

### Context from the Application: inputs and snapshots

The Domain never loads, reads no clock and knows no identity. Everything a change needs that the aggregate does not hold arrives on the input: the time and the actor, stamped once by the Application, the request data, and a snapshot of facts from outside the aggregate.

| Snapshot | Holds | Used by |
|---|---|---|
| `ReserveSnapshot` | policy linked, claim approved total, override flag, reserve type, approved balance, next change sequence | the reserve change steps |
| `ClaimReserveSnapshot` | approved reserve count, a change pending, a balance open | send to payment, close |
| `ClaimReferenceSnapshot` | active cause-of-loss codes, policy period | issue evaluation, on every claim input |

- The definition of each fact (for example "approved" means Approved or AutoApproved) lives in one Application query per snapshot, covered by Application tests.
- A stale snapshot is covered by the claim-row touch on save, which serializes concurrent changes on one claim.

### Model conventions

- **Aggregates:** the claim is a root with children (parties, risk objects, loss, issues, state changes). A reserve and a reserve change are two roots. Aggregates refer to each other by id only.
- **Entity data in one record,** replaced whole, so an edit cannot half-apply and EF maps it as a complex property.
- **No null:** a value that may be missing is a record with an `Empty` instance and an `IsSet` flag, because EF cannot map a struct to a nullable column.
- **Ids, time and actor:** ids are sequential GUIDs made in the Domain; an event takes its id, time and actor from the input as one `EventStamp`.
- **A Draft may be incomplete;** what is missing is stored as validation issues instead of rejecting the save.
- **Closed sets:** a set that carries facts, or that a transition uses as a state, is a marker enum plus a record per value. A set with neither is a plain enum.
- **Claim numbers:** `ClaimNumber.Create(year, sequence)` formats and validates; `ClaimNumber.From` parses a stored number. The Application takes the sequence from the database counter.

### Domain engine

The engine lives in `Domain/Core`, layered so that each folder uses only the layers above it (enforced by the workflow check); `Core` never references the aggregates. Its machinery is internal; only the outcome, the findings, the inputs and `StateChange` are public.

| Layer | Folder | Idea |
|---|---|---|
| 1 | `Core/Base/Checks` | a check reports findings about a subject |
| 2 | `Core/Base/Flows` | the template that evaluates checks and applies; the outcome, with `OnSuccess` |
| 3 | `Core/Rules` | a rule is a check with domain findings |
| 4 | `Core/Operations` | a business action on a subject; a creation operation has no subject yet |
| 5 | `Core/Transitions` | an operation that moves a subject between states and returns the `StateChange` |

- A rule is a check whose findings are domain findings: a code, a message, blocking by default or advisory. Stored validation issues are checks with their own finding type, because an issue is recorded and can be waived; it is not a refusal.
- A step is one shared instance, built on first use and run through a static `Execute`; it holds nothing beyond its rules and changes the subject only in `Apply`, through `internal` members, without touching the root or raising events.
- A transition is configured by a target and the source states it is allowed from, each with its rules; a move from an undeclared state returns one finding without running any rule. The aggregate's transition base sets the state, writes the history row and returns the change. Who may make a move is a rule on the transition.
- `EvaluateIssuesFlow` reconciles the stored issues with their checks: a new finding raises an issue, a resolved one is cleared, a waived one stays resolved while the problem remains. It is idempotent and runs in every claim method's `Settle`.
- Rule failures are findings; a broken invariant throws. Not found, forbidden and conflict belong to the Application. The engine loads nothing, saves nothing and decides nobody's permission.
- Transitions are internal, so the claim will expose its valid next states for the Application (Application pack).

Extending: a new state is a record; a new move is a transition class; a new rule is a class plus a line that requires it; a new action is an operation and its input. The recipes are in `.claude/CLAUDE.md` 'Extending the Domain'.

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
| SendToPayment | PendingPayment | Open | at least one approved reserve |
| SendToPayment | PendingPayment | UnderInvestigation | at least one approved reserve, liability confirmed |
| Close | Closed | Open, UnderInvestigation, PendingPayment | the four closure conditions |

- The closure conditions are four rules: no reserve change awaiting approval (S:CC-01), no unresolved Critical issue (S:CC-02), a claimant (S:CC-03), and a closing note when a reserve balance is above zero (S:CC-04).
- A failed close returns every failed condition at once.
- A close with an open reserve balance also returns the S:CC-04 warning as an advisory finding, whether or not a note was given.
- The reserve facts come in a snapshot on the input: the approved reserve count, whether a reserve change is pending, and whether a balance is open.
- Liability confirmation is a flag on the move from UnderInvestigation and is not stored on the claim.
- Any move may carry a note; it is stored on the state-change row, and a close with an open balance requires it.
- The manager-only limit override is `SetLimitOverrideOperation`, which sets the flag on the claim.
- Each move appends a `ClaimStateChange` row and raises `ClaimStateChangedEvent`.
- A reopen leaves two rows because the reopen method runs the Open move right after it; Open from Reopened has no rules, so that inner step cannot be refused.

**Validation checks.**
- Critical: loss date present, loss date not in the future, description long enough, a claimant, and an active cause of loss code (S:BR-C-05).
- Warning: no policy linked, and loss date outside the policy period (S:BR-C-02).
- The reference snapshot on every claim input carries the active cause codes and the policy period (start, end and cancellation date).
- The policy period ends at the earlier of the expiration and the cancellation date; both ends are inclusive.
- A claim holds its policy id; `LinkPolicyOperation` sets it, and the Application looks the policy up.
- `AcknowledgeIssueOperation` resolves an unresolved Warning issue; Critical issues are waived, not acknowledged.
- Opening from Draft needs the policy-period warning cleared, waived or acknowledged (S:BR-C-02).

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
- The submit method runs AutoApprove right after the submit; a refusal there only leaves the change pending.
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

**Concurrency.** Every reserve change also touches the claim row on save in Persistence, which serializes concurrent approvals on one claim and keeps the claim's last-activity time current.

**Posting.**
- The posting moves are transitions on the change, with its posting state as the subject.
- Posted and Failed need an approved change; a cancel needs a rejected or retracted one.
- A repeated post on a Posted change returns the not-permitted finding, which the job reads as already done.
- A Failed change can be posted again.
- Each move stores the job id and the failure reason in the posting details and raises `ReservePostingStateChangedEvent`.
- The job itself, its retries and the audit entries belong to Infrastructure and the Application.

### Design trade-offs

- **Entities with behaviour, flows behind them.** The aggregate's public methods are the model's API; the engine behind them gives every method the same error handling and order. The cost is one more hop and a generic base with a self type; the gain is that no method hand-writes its checks, every rule has a name and a code, and a refusal can never half-apply.
- **Follow-ups chained on the outcome, not hooks in the steps.** Hooks hid where a change happened and let a follow-up fail after it. The chain keeps the order visible in the method and the outcome the step's own; the deliberate limit is that a follow-up cannot refuse.
- **Facts passed in, not read.** Snapshots keep the Domain free of infrastructure and testable with plain records; the alternative, a Domain interface that Persistence implements, was rejected. The cost is that fact definitions live in the Application.
- **Two reserve aggregates.** A reserve change is its own root so that transitions can move it and several changes can be pending on one reserve. The invariants that span them (non-negative balance, claim limit) are checked against a snapshot at submit and again at approval.
- **No stored balance.** A reserve's balance is the sum of its approved changes, as the specification describes; a stored figure would need a second step to keep it in sync.
- **No runtime rule engine.** Rules are classes declared in code; a settings-driven engine would hide them from review and tests.
- **Events.** Raised in the success follow-ups; the Unit of Work publishes them before the commit for reactions in the same transaction and after it for effects outside.

### Aggregate follow-ups and events

Inside the aggregate method, as `OnSuccess` hooks:

- Any claim change, then the issue evaluation, using the reference snapshot on every claim input.
- A reopen, then the Open move from Reopened.
- A reserve submit, then the auto-approve attempt; a refusal only leaves the change pending.
- A reject or retract, then the posting cancel move.

Across the boundary, in the Application, driven by the events:

- An approval or auto-approval, then the posting job after the commit.
- Every domain event, then its audit entry.

Each aggregate method has Domain tests; each event reaction has an Application test.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat entity-to-DTO and command-to-entity, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a recursive type is introduced.
