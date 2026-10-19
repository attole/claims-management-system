# Architecture

The final design of the claims module. The Domain is built and tested, and the Application is built except for events, audit and idempotency; Persistence, Infrastructure, API and frontend are next. This file holds the reasons and the decisions, `.claude/CLAUDE.md` holds the rules and `docs/DOMAIN-STANDARDS.md` and `docs/APPLICATION-STANDARDS.md` the code shapes.

## Status

| Layer | State |
|---|---|
| Domain | complete: the claim with its children, the reserve component with its history rows, claim documents, policies, the cause-of-loss reference data, roles and the generic engine, with tests |
| Application | built with tests: results, command and query markers, closed-set `From`, the claim, reserve and document use cases, status changes and all reads, the audit handlers, the event dispatcher with the Unit of Work that runs it, and the idempotency store contract; the obligations the Domain hands over are listed under 'What the next layers must do' |
| Persistence, Infrastructure, API, frontend | not started; working notes are in `.claude/temp/notes-layer-application.md` and `.claude/temp/notes-layer-persistence.md` |

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

- A Draft is incomplete or unvalidated by definition, so it is saved with its problems stored as issues.
- A rule marked Critical means a stored Critical issue (S:VAL-Severity).
- Only a request that cannot be bound fails at the API edge; the sample 422 body fixes the error shape only.
- A reserve amount is the signed change the user typed. The "greater than zero" rule applies to the resulting balance, at submission and again at approval. A zero amount is refused (user decision).
- An approved history row has the approval Approved or AutoApproved.
- The 10 million in the roles table is the claim-wide cap, with a manager override (S:BR-R-02, S:BR-R-05). The claim total is the net sum over all reserves, with no exclusions.
- A claim may hold several reserve components, including several of one type.
- Reopened is a normal state.
- A policy period is two inclusive calendar dates, compared with the UTC calendar date of the loss date, because all timestamps are stored in UTC.
- The claim's reported date (the first notice received) is the creation time, set by the system. The loss event's report date (the formal report) comes on the create input from intake, may be earlier than the creation time, is required (`LOSS_REPORT_DATE_REQUIRED`) and never changes afterwards. The estimated loss amount is optional and has no bound.
- Roles are nested as the specification describes them (S:ROLE-Supervisor, S:ROLE-Manager): a supervisor holds every handler right and a manager every supervisor right, so a role rule names only the lowest role (`HasMinimumRole`). `UserRole` is a closed set whose variants list the roles whose rights they hold, so no rule depends on the order of an enum. Each role also states the reserve amounts it may decide (`CanDecideReserve`: a supervisor up to 100,000, a manager any amount). The roles and their rule live in `Roles/`, shared by the aggregates. The Application turns the role from the token into a variant with `UserRole.From`.
- Which role may call which operation is decided in the Application; the documents state only the explicit rules (reopen, approval authority, the limit override). Waiving a Critical issue is our own role rule (supervisor or above), kept in the Domain beside the others.
- The specification's validation messages are used verbatim in Domain rule classes and in the transition refusal, under a narrow exception in `CLAUDE.md`, and nowhere else. The closure message uses an ASCII hyphen where the specification has a dash.
- The specification's approval and posting status columns are states in the Domain.
- Closing and moving to pending payment have no role rule, because the specification's transition table lists none. Investigate and the move back to Open ask only for the handler role, which every role holds. Reopen (supervisor) is the only claim move that reads the role, and only its input carries it.

### Schema departures from the spec

| Change | Departure | Why |
|---|---|---|
| Loss date, cause code | nullable loss date, cause code as text without a foreign key | a Draft may be incomplete; an unknown code is a stored issue (S:BR-C-05) |
| Closed date, closure reason | dropped; derived from the latest change into Closed | the state-change history already holds them |
| `ClaimValidationIssue` (added) | stored child: the finding (code, message, field, severity, evidence), state (raised, cleared, acknowledged, waived), who resolved it and when | the spec describes recorded issues but lists no entity |
| `ClaimStateChange` (added) | append-only child: from, to, optional reason (code and text), note; user and time are its audit columns | reasons and the closing note have no columns |
| Limit override flag (added) | boolean on the claim, set by a manager-only operation | the claim-wide cap needs a flag (S:BR-R-05) |
| Policy cancellation date (added) | nullable column; the coverage window ends at the earlier of the end and the cancellation date | a status alone cannot place a loss before a cancellation |
| Claim severity | own type with an "unspecified" start; a plain operation sets it | the documents give no setter, default or rule |
| Primary keys | sequential GUIDs generated in the Domain, never by the database; Persistence also declares the `NEWSEQUENTIALID()` default to match the spec | aggregates and events have ids at creation |
| Reserve current amount | not stored; the sum of the component's approved history rows, computed in the Domain | the spec calls it computed from history |
| Reserve component status | an Active or Closed state that follows the claim | the spec gives the column but no rule (see Decisions on gaps) |
| History posting failure reason (added) | the failure reason is stored with the job id in a posting details record on the row | the reserves tab can show why a posting failed; the spec only audits it |
| History row component type and submission time (added) | the component type is copied from the component at submit, and the submission time is kept as a business time | approval reads the type from the row itself, and a review can be checked against the submission |
| Reserve review (changed) | who made the latest approval change, when, and the reason, on the history row instead of separate approved and rejected columns | a row is approved or rejected, never both, so half of the paired columns would always be empty |
| Document soft delete (added) | `IsDeleted` and `DeletedAt` on the document | the spec lists no delete column; removal is a soft delete |

### Decisions on gaps

**Claims and intake**

- **Liability determined:** the status-change request carries an explicit confirmation, required for the move to PendingPayment and written to the audit entry; nothing is stored on the claim. The frontend needs a matching control.
- **Withdrawal and reopen reasons:** valid only with both a code and a text, stricter than the documents so every reason is reportable. The code is free text until a list exists.
- **Loss date after the report date:** a Critical issue (`LOSS_DATE_AFTER_REPORT`) when the loss falls on a later UTC calendar day than the report; a loss later on the report day passes, as intake may send the report date as a date only. The documents only forbid a date in the future (S:BR-C-01); since the report date is never later than the creation time, this rule covers S:BR-C-01 and replaces it, so the loss checks need no clock. The cost: a future loss date reports this code and message, not the spec's own.
- **Report date in the future:** creation is refused (`LOSS_REPORT_DATE_IN_FUTURE`) when the formal report date is later than the creation time. The documents state no such rule; it keeps the report date a safe upper bound for the loss date.
- **Negative estimated loss:** refused by the value (an out-of-range exception), and the API validates it.
- **No risk object:** a Warning issue (`CLAIM_NO_RISK_OBJECT`).
- **Edits after Draft:** once the claim is past Draft, a loss update that would leave a Critical loss issue is refused (`IsLossCompleteAfterDraft`), so a claim never falls back below what Open required. The rule takes the proposed details, the policy period and the cause codes from the input and runs the same loss checks as the issue evaluation, so the issues and the refusal share codes and messages. A problem whose issue was waived (same code and evidence) stays allowed; a new problem of the same code is refused. Only the loss has such a guard; the last-claimant rule already protects the claimant.
- **Closed and withdrawn claims are frozen** (`CLAIM_NOT_EDITABLE`): parties, risk objects, loss details, severity, the handler, the policy link, the limit override and issue waivers and acknowledgements are refused, a reserve cannot be opened (`RESERVE_CLAIM_NOT_EDITABLE`), a history row cannot be submitted, a document cannot be uploaded or removed (`DOCUMENT_CLAIM_NOT_EDITABLE`), and the issue re-evaluation does not run, so no new issue appears. The documents state no such rule, but they make reopening a closed claim a supervisor move with a reason, and that gate would mean nothing if a closed claim could still change. Notes are the exception and stay editable in every state: they are commentary, not the facts a closure was decided on.
- **Policy cancellation ends cover:** the coverage window ends at the earlier of the expiration and the cancellation date. A status alone cannot place a loss before or after the cancellation.
- **The policy period belongs to the claim's policy:** a `PolicyPeriod` names its policy, and the issue evaluation and the after-Draft guard throw when the period was built for another policy than the claim's (`PolicyPeriod.RequireFor`); `CauseCodes` without active codes throws when built. A wrong period is a loading error, not a user mistake, and a silent re-evaluation with it would clear decided issues.
- **A waiver lapses with its facts:** a finding carries the checked values as evidence, and an issue matches by code and evidence, so moving the loss date or relinking the policy after a waiver or an acknowledgement clears the old issue and raises a new one.
- **Waivers:** a missing loss date, an inactive cause code and a missing claimant can never be waived (`ISSUE_MUST_BE_FIXED`): the first two because the stored schema requires them, the claimant because opening requires one anyway (S:BR-C-03). Every other Critical issue may be waived, by a supervisor or manager only.
- **A handler before opening:** a Draft opens only with an assigned handler, and after Draft the handler cannot be removed (`CLAIM_HANDLER_REQUIRED` to open, `CLAIM_HANDLER_NOT_KEPT` to unassign). The stricter reading wins so no open claim drops out of the handler filters.
- **Moves see current issues:** every claim move re-evaluates the issues with its input's facts before the transition runs, and again after it, so a cause code deactivated or a policy changed since the last edit is caught by the move's own rules (S:BR-ST-02, S:CC-02). A refused move keeps the refreshed issues: they are correct data, not a half-applied move.
- **Drafts stay:** the specification's transition table has no exit from Draft and forbids unlisted moves, so a Draft remains until it is opened; the SLA job keeps flagging an abandoned one.
- **One primary risk object:** a risk object can be set primary (on add or edit) only while no other active risk object is primary; the primary is moved by editing the old one first.
- **Party names:** a request validator in the Application requires a first or last name for a person and a company name for a company. No document states it as a business rule, so it is shape validation, not a Domain rule.
- **Claim numbers without gaps:** generated by a counter row per organisation and year, incremented with an atomic update inside the claim's transaction, as the specification prescribes, so a failed creation rolls the number back. The format holds seven sequence digits, so `ClaimNumber.Create` throws past 9,999,999; the counter refuses first with a conflict.
- **No-policy message:** the warning issue and the reserve block both use the validation table's wording, which names reserves.

**Reserves**

- **Reserve components follow the claim:** closing or withdrawing a claim closes every component of the claim, and reopening the claim reopens them, in the same Unit of Work (the Application handles the claim's state-changed event and calls `Close` or `Reopen` on each component). A closed component refuses a new history row (`IsReserveComponentActive`), and the submit also takes the claim's state as a second guard, so a row is refused on a frozen claim even before the components are closed. The documents give the component a status but no rule for when it changes; this is the design decision. Opening a component has no component to read, so it takes the claim's `ClaimReference` (id, organisation, state) and refuses a claim that is not editable.
- **A history row belongs to its component:** it is a child, which supplies its component id, claim and component type at submit; they stay on the row, so the approval reads the component type from the row, never from a request field. The component computes the change sequence (the highest plus one) and its approved balance, so no request supplies either. The idempotency key is `Reserve:{component id}:Change:{sequence}`, and a unique index on component and sequence in Persistence backs it.
- **Reject follows the approval tier** (`HasRejectionAuthority`, S:BR-R-02): only a role that may approve an amount may reject it. The documents disagree: the role table gives supervisors and managers approve and reject authority up to their tier, while the reject endpoint only asks for the supervisor or manager role. The role table is the specification's own definition of what each role may do, so it wins; the refusal has its own message, because the specification's message speaks of approving.
- **Reserve amount messages:** the amount and balance refusals (`IsReserveAmountNonZero`, `HasNonNegativeBalance`, `IsApprovedBalanceNonNegative`) use their own wording instead of the specification's amount message, which is written for positive amounts only.
- **Withdraw settles the reserves first:** a withdraw is refused while a history row awaits approval (`CLAIM_WITHDRAW_PENDING_RESERVE`) or an approved row is not yet posted (`RESERVE_HISTORY_NOT_POSTED`), and with an open reserve balance it needs a note (`CLAIM_WITHDRAW_NOTE_REQUIRED`) and returns a warning (`CLAIM_WITHDRAWN_WITH_OPEN_BALANCE`). Without these, a row approved after the withdraw would post onto a withdrawn claim.
- **Close waits for postings:** a close is refused while an approved row is not yet posted, Pending or Failed (`RESERVE_HISTORY_NOT_POSTED`), so the ledger matches the closed balance. This makes the posting re-enqueue sweep a requirement: a job lost between commit and enqueue would otherwise block the close for good.
- **Decreases and the claim limit:** a change that does not increase the total passes the claim limit, at approval and at submit, so exposure can always be reduced. The manager override cannot be removed while the approved total is above the limit (`CLAIM_OVERRIDE_STILL_NEEDED`). The limit lives on the claim, next to the override flag: `Claim.IsWithinReserveLimit` for a total and `Claim.IsReserveHistoryWithinLimit` for a change.
- **What counts as an approved reserve for payment:** a reserve of a non-recoverable type with an approved balance above zero, not any approved row, so a claim whose only approval is a recovery, or whose reserve was reversed to zero, cannot move to payment. The Application query owns this definition.
- **Transaction types are labels:** Add, Adjust and Reverse are recorded as the user picks them; the signed amount alone drives balances, limits and tiers.
- **Tiers use the size of the change:** the auto-approval limit and the approval tiers compare the absolute amount, so a large decrease needs the same approver as a large increase.
- **Auto-approval has no reviewer:** its review keeps the time and an empty reviewer; the AutoApproved state says the system decided, so the history never shows the submitter as an approver (S:BR-R-03).
- **A posting move names its job:** posted, failed and retry need a job id (`RESERVE_POSTING_JOB_REQUIRED`), so the stored job id always belongs to the last move; cancel has none.
- **A reversal while payment is pending is allowed:** a reserve stays independent of the claim state; a claim left with nothing to pay is closed.
- **Handler submissions above 10,000:** the role table's handler limit is read as the auto-approval authority, so a handler may submit any amount and the row waits for an approver.

**Documents**

- **The blob name carries a unique prefix:** the specification fixes the storage path as the container, the organisation, the claim and the sanitised file name. We keep every one of those parts and put a random upload id in front of the file name (`{uploadId}_{name}`). The plain name gave three failures: a commit that fails after the blob is stored leaves an orphan that would block the same name for ever (a retry is refused as already stored), a removed document keeps its blob (a soft delete) so the name could never be uploaded again, and two names that sanitise to the same text, or two concurrent uploads, would overwrite each other's blob while two rows point to it. With the prefix a path is unique by construction, so no existence check is needed and an orphan only costs storage (an Infrastructure sweep can remove blobs no row refers to). The row keeps the original file name as the document name, so a download still shows it. The path stays well inside the 500-character column (the longest is 378). The same prefix applies to the local filesystem fallback.
- **A document is its own root, frozen with the claim:** it is not a child of the claim (own lifecycle, no shared invariant, a blob side effect). The 50 MB limit and the content-type allowlist are Domain rules (`S:DOC-MaxSize`, `S:DOC-MimeAllowlist`). The Application builds the blob path, sanitises the filename and talks to storage; the Domain receives the finished path.

**Across the model**

- **Audited changes:** the events cover the specification's fixed audit list, plus `AggregateLimitOverrideChangedEvent` for the limit override, `ReserveComponentStateChangedEvent` and the two document events. The policy link, the handler, severity, notes, loss and risk-object edits and opening a reserve raise no event; their trace is the audit columns.
- **Business times and actors are guarded:** a review earlier than the row's submission, a document removal earlier than its upload, or a waiver, acknowledgement or risk-object removal earlier than the claim's reported date, throws (`InputTime.NotBefore`), and a decision input without an actor or a time throws (`InputActor.Required`, `InputTime.Required`). The Application stamps both, so a failure is a programming error, not a refusal.
- **Unspecified cases** (authentication failures, not found, concurrent edits, oversized uploads, idempotency-key replays) are decided when the API pack is designed and recorded here.

## Backend architecture

### Layers

Clean Architecture with five projects, dependencies pointing inward: `API` -> `Application` -> `Domain`. `Persistence` and `Infrastructure` implement interfaces that `Application` defines. The Domain has no project references and no packages.

- **Domain:** the aggregates, their rules and events, and the small engine that runs a change.
- **Application:** the use cases. It loads data, builds the input with the facts the rules need, calls one aggregate method, reads its outcome, saves through the Unit of Work, dispatches events and maps findings to responses; 'The Application layer' below describes how it is built.
- **Persistence:** EF Core mapping, with a save interceptor for the audit columns, soft delete, the tenant and the root touch.
- **Infrastructure:** Hangfire jobs and outside adapters.
- **API:** controllers and startup.

### The domain model

The Domain is a domain-driven model of four aggregates, the cause-of-loss reference data and the roles they share:

| Aggregate | Root and children | Notes |
|---|---|---|
| Claim | `Claim` with parties, risk objects, the loss, issues and state changes | the lifecycle, the issue evaluation and the reserve limit and override |
| Reserve component | `ReserveComponent` with `ReserveHistory` rows | one cost bucket of a claim; the rows carry the approval and posting states |
| Claim document | `ClaimDocument` | metadata of an uploaded file; soft delete |
| Policy | `Policy` | reference data from outside: number, one client name, period, coverage types; no operations |
| Cause of loss | `CauseOfLossCode` and the `CauseCodes` value | reference data from outside: code, name, peril category, active flag; no operations |
| Roles | `UserRole` and `HasMinimumRole` | shared by the aggregates |

Aggregates refer to each other by id only. The Domain never holds another aggregate: it receives plain values on the input (see the table below), plus a few static facts on `Claim` (the reserve limit). The dependency direction is checked by the workflow check: `Claims` uses `Policies`, `CauseOfLoss` and `Roles` only; `ReserveComponents` and `ClaimDocuments` use `Claims` (for its state, its reference and the limit facts) and never each other; `Policies` and `CauseOfLoss` use nothing.

Inside the aggregate methods every change runs through one small engine, which exists for error handling and order. A step checks all of its rules before it changes anything and returns the refusals as findings, all at once and never as exceptions, so a refused change leaves the aggregate untouched; it applies its change in one place, so "rules, then the change, then what follows" is the same order for every method. Rules are named classes, and a new one is a class plus the line that requires it. The engine knows nothing about claims or reserves.

| Concern | Where it lives | How |
|---|---|---|
| State and its changes | the aggregate, an entity | public methods; `internal` setters and child mutators; lifecycles as closed sets |
| The conditions of a change | rules, internal classes with a `Code` | declared once per step; a failed rule is a finding |
| Running a change | steps: operations, creations and transitions | rules first, then `Apply`; the result is an `Outcome` |
| What follows a successful change | the aggregate method's chain | `OnSuccess` hooks: raise the event, record the move, run an inner step, settle |
| Facts from outside the aggregate | plain fields or a reference record on the input | computed by the Application before the call |
| Effects beyond the aggregate | domain events | handled by the Application around the commit |

### How a change runs

```csharp
public Outcome<ClaimParty> AddParty(AddPartyInput input)
{
    return AddPartyOperation.Run(this, input)
        .OnSuccess(party => Raise(new PartyAddedEvent(Id, party.Id, party.Details.Role.Name)))
        .OnSuccess(() => Settle(input));
}
```

- The step evaluates its rules. If any finding is Critical, the outcome carries every finding, nothing has changed, and the chain falls through.
- On success the follow-ups run in the written order: the event the step owns, a move record, an inner step of the same aggregate, and `Settle` last on the claim (re-evaluate the claim's issues).
- The method returns the step's outcome; a follow-up never changes it, so a condition that could refuse is a rule on the step. The Application reads `IsSuccess`, `Value`, `CriticalFindings` and `WarningFindings` and never looks inside the aggregate.
- A change to an existing child is split: the aggregate's step finds the child by id (an unknown id is the Application's not found) and runs only the aggregate's rules; the child's own method runs the child's step with the child's rules and raises the child's events.

### Context from the Application: inputs

The Domain never loads, reads no clock for business times and knows no identity. Everything a change needs that the aggregate does not hold arrives on the input: the request data, the time and the actor where the step records them, the actor's role where a rule reads it, and the facts from outside the aggregate.

| Input | Outside facts | Used by |
|---|---|---|
| every claim input | `PolicyPeriod` (the period of the claim's current policy) and `CauseCodes` (the active cause-of-loss codes), listed as two parameters | the issue evaluation around every change, the loss update |
| `CreateClaimInput` | the organisation, the claim number from the counter, the formal report date | create |
| `FreezeClaimInput` | a history row pending, an approved row not yet posted, a reserve balance open | close, withdraw |
| `ReopenClaimInput`, `RejectHistoryInput`, `WaiveIssueInput` | the actor's role | reopen, reject, waive |
| `SendToPaymentClaimInput` | an approved reserve | send to payment |
| `SetLimitOverrideInput` | the claim approved total, the actor's role | set or remove the limit override |
| `OpenReserveComponentInput` | `ClaimReference`: the claim's id, organisation and state (`claim.Reference`) | open a reserve component |
| `SubmitHistoryInput` | the claim's state, policy linked, claim approved total; the component supplies its own state, balance and next sequence | submit a history row |
| `ApproveHistoryInput` | the history row id, claim approved total, the override flag and the actor's role; the component supplies its approved balance | approve |
| `RetractHistoryInput`, `HistoryPostingInput` | the history row id (and the job facts for posting) | retract, the posting moves |
| `UploadClaimDocumentInput` | `ClaimReference`; the finished blob path and the file facts | upload a document |
| `RemoveClaimDocumentInput` | the claim's state | remove a document |

- Every claim input carries `PolicyPeriod` and `CauseCodes` as two separate parameters, because every claim change re-evaluates the issues. They come from different readers (the policy and the cause-code list), so they are not bundled. The `Policy` entity itself never reaches a claim input.
- A reference record exists only where another aggregate needs several facts from one aggregate together; today that is `ClaimReference`, used when a new component or document is created. Later steps take only `ClaimState`.
- The inputs of a component's history rows come in two layers: the public inputs above carry the row id and the request data, and the component's step builds the row's own internal input (named `...RowInput`), adding the balance from the component's own state.
- The definition of each fact (for example "approved" means Approved or AutoApproved) lives in one Application query, covered by Application tests.
- A stale fact is covered by the claim-row touch on save (see Concurrency under Reserves).

### Model conventions

- **Entity data in one record,** replaced whole, so an edit cannot half-apply and EF maps it as a complex property.
- **No null:** a value that may be missing is a record with an `Empty` instance and an `IsSet` flag, because EF cannot map a struct to a nullable column.
- **Ids:** sequential GUIDs made in the Domain by `Ids.Next()`, which writes the current UTC time into the bytes SQL Server sorts by, so inserts land at the end of the index (a COMB-style generator; a GUID v7 puts its time in the bytes SQL Server compares last). It is the only clock read in the Domain: an id is a technical value, not business data.
- **Business times and decisions:** `ResolvedAt` and `ResolvedBy` on an issue, `DeletedAt` on a risk object or a document, the row's `SubmittedAt` and the reserve review come from the input, and only the inputs whose step records one carry a time. They are Domain state, not audit columns: a later change modifies the row, so `UpdatedAt` and `UserModified` would overwrite who decided. An automatic clear records neither, because nobody decided it.
- **Soft removal:** a removed party is deactivated (`IsActive`, the specification's soft remove) and stays on the claim; a removed risk object or document is soft-deleted with its `DeletedAt`. The `IsDeleted` and `DeletedAt` pair on rows with no removal step (the claim, the loss, issues, parties, the reserve component) is kept for Persistence to stamp and for the global query filter; no Domain step sets it. History rows are append-only and never deleted.
- **Events and audit:** events carry business facts only; the Application wraps each in an event context (event id, time, actor) at dispatch, and Persistence stamps the audit columns.
- **Closed sets:** a set that carries facts, or that a transition uses as a state, is a marker enum plus a record per value. A set with neither is a plain enum.
- **Claim numbers:** `ClaimNumber.Create(year, sequence)` formats and validates; `ClaimNumber.From` parses a stored number. The Application takes the sequence from the database counter.

### Domain engine

The engine lives in `Domain/Core`, layered so that each folder uses only the layers above it (enforced by the workflow check); `Core` never references the aggregates. Its machinery is internal; only the outcome, the findings, `NoSubject`, `NoInput`, the inputs and `Move` are public.

| Layer | Folder | Idea |
|---|---|---|
| 1 | `Core/Rules` | a rule reports findings about a subject; the rule set, the finding, its severity and the result |
| 2 | `Core/Flows` | the steps: an operation checks its rules, stops on a Critical finding, otherwise applies; a creation has no subject yet; a transition moves a subject between states and returns the `Move`; every step returns an `Outcome`, with `OnSuccess` |

- There is one finding: a code, a message, a field, a severity (Critical by default, or Warning) and the evidence (the checked values). The finding states the severity and the step decides what it means: a step stops on a Critical finding, while the issue evaluation stores each finding as an issue, because an issue is recorded, not a refusal.
- A step is one shared instance that keeps no state, so it cannot carry data from one call to the next; the mechanics are in `.claude/CLAUDE.md` 'Domain engine'.
- A transition is configured by a target and the source states it is allowed from, each with its rules; a move from an undeclared state returns one finding without running any rule. The engine returns the `Move`; the aggregate's transition base sets the state (and on the claim writes the history row). Who may make a move is a rule on the transition. A transition runs on any subject with a state, so a child row can have its own state machines (a history row has two).
- `EvaluateIssuesOperation` reconciles the stored issues with their rules by code and evidence: a new finding raises an issue, a fixed problem clears its issue, a waived or acknowledged one stays resolved while the same finding remains. It is idempotent, runs in every claim method's `Settle` (before and after a move), and requires an editable claim.
- Rule failures are findings; a broken invariant throws. Not found, forbidden and conflict belong to the Application. The engine loads nothing, saves nothing and decides nobody's permission.
- Transitions are internal; how the Application lists the valid next states for a refused move is open (`.claude/temp/notes-layer-application.md`).

Extending: a new state is a record; a new move is a transition class; a new rule is a class plus a line that requires it; a new action is an operation and its input. The recipes are in `.claude/CLAUDE.md` 'Extending the Domain'.

### Claims

**Lifecycle.**

| Transition | Target | From | Rules |
|---|---|---|---|
| Open | Open | Draft | no unresolved Critical issues, the policy-period warning resolved (S:BR-C-02), an active claimant, an assigned handler |
| Open | Open | UnderInvestigation | none |
| Open | Open | Reopened | none |
| Investigate | UnderInvestigation | Open | none |
| Withdraw | Withdrawn | Open, UnderInvestigation | a reason with code and text, the reserve conditions under Decisions on gaps |
| Reopen | Reopened | Closed | supervisor role or above, a reason |
| SendToPayment | PendingPayment | Open | at least one approved reserve |
| SendToPayment | PendingPayment | UnderInvestigation | at least one approved reserve, liability confirmed |
| Close | Closed | Open, UnderInvestigation, PendingPayment | the closure conditions below |

- The closure conditions are S:CC-01 to S:CC-04 and `RESERVE_HISTORY_NOT_POSTED` (Decisions on gaps).
- A failed close returns every failed condition at once. A close with an open reserve balance also returns the S:CC-04 warning as a Warning finding, whether or not a note was given.
- Any move may carry a note; it is stored on the state-change row, and a close with an open balance requires it.
- Each move appends a `ClaimStateChange` row and raises `ClaimStateChangedEvent`.
- A reopen leaves two rows because the reopen method runs the Open move right after it; Open from Reopened has no rules, so that inner step cannot be refused.

**Issue rules.**

- Critical: loss date present, loss date not after the report date (covers S:BR-C-01), description long enough, a claimant, and an active cause of loss code (S:BR-C-05).
- Warning: no policy linked, no risk object (`CLAIM_NO_RISK_OBJECT`), and loss date outside the policy period (S:BR-C-02).
- The policy period ends at the earlier of the expiration and the cancellation date; both ends are inclusive.
- A claim holds its policy id; `LinkPolicyOperation` sets it, and the Application looks the policy up.
- `AcknowledgeIssueOperation` resolves an unresolved Warning issue; Critical issues are waived, not acknowledged.

**Rules.**

| Rule | Code | Used by |
|---|---|---|
| HasOrganisation, HasClaimNumber | CLAIM_ORGANISATION_REQUIRED, CLAIM_NUMBER_REQUIRED | create |
| HasReportDate, IsReportDateNotInFuture | LOSS_REPORT_DATE_REQUIRED, LOSS_REPORT_DATE_IN_FUTURE | create |
| IsClaimEditable | CLAIM_NOT_EDITABLE | every edit, waive, acknowledge, the issue evaluation |
| HasValidLossDetails (combined) | LOSS_DETAILS_VALID | the issue evaluation |
| IsLossCompleteAfterDraft (combined) | LOSS_INCOMPLETE_AFTER_DRAFT | the loss update |
| HasClaimant, HasLinkedPolicy, HasRiskObject | S:BR-C-03, S:INTAKE-NoPolicy, CLAIM_NO_RISK_OBJECT | the issue evaluation (and HasClaimant on open) |
| IsNotLastClaimant | S:PARTY-LastClaimant | remove party |
| IsPartyActive | PARTY_NOT_ACTIVE | remove party |
| HasNoOtherPrimaryRiskObject | RISK_OBJECT_PRIMARY_EXISTS | add or update a risk object |
| IsRiskObjectNotRemoved | RISK_OBJECT_REMOVED | update or remove a risk object |
| HasPolicyId | POLICY_ID_REQUIRED | link policy |
| IsHandlerKeptAfterDraft | CLAIM_HANDLER_NOT_KEPT | assign handler |
| HasMinimumRole | ACTOR_ROLE_NOT_PERMITTED | reopen, waive, limit override |
| IsOverrideRevocable | CLAIM_OVERRIDE_STILL_NEEDED | limit override |
| IsIssueWaivable, HasWaivableFinding | ISSUE_NOT_WAIVABLE, ISSUE_MUST_BE_FIXED | waive |
| IsIssueAcknowledgeable | ISSUE_NOT_ACKNOWLEDGEABLE | acknowledge |
| HasNoCriticalIssuesForOpen, HasNoUnacknowledgedPolicyWarning, HasAssignedHandler | S:BR-ST-02, S:BR-C-02, CLAIM_HANDLER_REQUIRED | open from Draft |
| HasStateChangeReason | STATE_CHANGE_REASON_REQUIRED | withdraw, reopen |
| HasApprovedReserveForOpenClaim, HasApprovedReserveForClaimUnderInvestigation, HasLiabilityConfirmation | S:ST-Open-PendingPayment, S:ST-UnderInvestigation-PendingPayment, LIABILITY_CONFIRMATION_REQUIRED | send to payment |
| HasNoPendingReserveHistory, HasNoCriticalIssuesForClose, HasClaimantForClosure, HasClosingNoteForOpenReserves, HasNoOpenReserveBalance (warning) | S:CC-01 to S:CC-04 | close |
| HasNoUnpostedReserveHistory | RESERVE_HISTORY_NOT_POSTED | close, withdraw |
| HasNoPendingReserveHistoryForWithdrawal, HasWithdrawalNoteForOpenReserves, HasNoOpenReserveBalanceOnWithdrawal (warning) | CLAIM_WITHDRAW_PENDING_RESERVE, CLAIM_WITHDRAW_NOTE_REQUIRED, CLAIM_WITHDRAWN_WITH_OPEN_BALANCE | withdraw |

A combined rule has a `Code` that names it but no finding of its own: it returns the findings of the rules it runs.

### Reserves

**Model.**

- A `ReserveComponent` is a root: claim id, component type, state (Active or Closed, following the claim) and notes. A claim can have several, including several of one type. The types are Indemnity, Expense, Alae and SubrogationRecoverable; only the recoverable type may go negative.
- A `ReserveHistory` is a child of the component: component id, claim id, component type, transaction type (Add, Adjust or Reverse), signed amount, reason, change sequence, submitter and submission time, approval state, posting state, review and balances.
- The component holds its rows; its approved balance (the sum of the approved rows' amounts) and the next change sequence are computed from them and never stored.
- The idempotency key of a row is `Reserve:{component id}:Change:{change sequence}`.

**Lifecycle of a history row.**

| Step | Kind | Result |
|---|---|---|
| Submit | operation on the component | a pending row, balances empty |
| AutoApprove | transition | PendingApproval -> AutoApproved |
| Approve | transition | PendingApproval -> Approved |
| Reject | transition | PendingApproval -> Rejected |
| Retract | transition | PendingApproval -> Cancelled |
| MarkPosted | transition | Pending -> Posted (posting state) |
| RetryPosting | transition | Failed -> Pending (posting state), with the new job id |
| MarkPostingFailed | transition | Pending -> Failed (posting state) |
| CancelPosting | transition | Pending -> Cancelled (posting state) |

- A row is always created pending, like a claim is created as a Draft. The submit method runs AutoApprove right after the submit, on that new row only; a refusal there only leaves the row pending.
- Approved, AutoApproved, Rejected and Cancelled rows never move again. A correction is a new Adjust or Reverse row.
- Every approval change stores the reviewer (none for an auto-approval), the time and the reason in the review and raises `ReserveHistoryApprovalStateChangedEvent`. The row fills its balances (previous and new) when it becomes approved.
- The posting state starts Pending and moves independently of the approval state, but only an approved row can be posted. A reject or retract cancels the posting in the same call.
- The component's own state (Active, Closed) is separate: `Close` and `Reopen` are transitions on the component.

**Authority.**

- An amount up to 10,000 may be auto-approved, whatever the submitter's role.
- Approving up to 100,000 needs a supervisor or a manager; above that, a manager.
- Rejecting follows the same tiers and needs a reason. Retracting is the submitter's. Nobody approves their own row.
- A handler may submit above their own limit; the row waits as pending.
- A change that would take the claim total over 10 million is never auto-approved, even with the override flag set. At approval, such a change needs the manager override flag on the claim. At submit, the same case only returns a warning, which is not stored as an issue.

**Rules.** Each rule takes the facts it reads as plain values; the auto-approval limit and the balance after a change are on `ReserveHistoryDetails`, the approval tiers on `UserRole.CanDecideReserve`, the claim limit on `Claim`, the component balance on `ReserveComponent`.

| Rule | Code | Used by |
|---|---|---|
| IsClaimAcceptingReserves | RESERVE_CLAIM_NOT_EDITABLE | open a reserve, submit a history row |
| IsReserveComponentActive | RESERVE_COMPONENT_NOT_ACTIVE | submit a history row |
| IsReserveAmountNonZero | RESERVE_AMOUNT_ZERO | submit |
| IsPolicyLinked | S:BR-C-06 | submit |
| HasNonNegativeBalance | S:BR-R-01 | submit |
| IsClaimTotalWithinLimit (warning) | S:BR-R-05 | submit |
| IsWithinAutoApprovalLimit | S:BR-R-02 | auto-approve |
| IsClaimLimitCleared | S:BR-R-05 | auto-approve, approve |
| HasApprovalAuthority | S:BR-R-02 | approve |
| IsNotSelfApproval | S:BR-R-03 | approve |
| IsApprovedBalanceNonNegative | S:BR-R-01 | approve |
| HasRejectionAuthority | S:BR-R-02 | reject |
| HasRejectionReason | RESERVE_REJECTION_REASON_REQUIRED | reject |
| IsSubmitter | S:API-POST-reserve-retract | retract |
| IsReserveHistoryApproved | RESERVE_HISTORY_NOT_APPROVED | mark posted, mark posting failed |
| IsReserveHistoryRefused | RESERVE_HISTORY_NOT_REFUSED | cancel posting |
| HasPostingJob | RESERVE_POSTING_JOB_REQUIRED | mark posted, mark posting failed, retry posting |

**Concurrency.** Every history write also touches the claim row on save in Persistence. The row version of the component stops two writers on one component; the claim touch serializes approvals across all of a claim's components (the claim total is read across them) and keeps the claim's last-activity time current.

**Posting.**

- The posting moves are transitions on the row that move its posting state.
- Posted and Failed need an approved row, and they and a retry need a job id; a cancel needs a rejected or retracted one.
- A repeated post on a Posted row returns the not-permitted finding, which the job reads as already done.
- A Failed row is retried by moving it back to Pending with the new job's id, then posted, so the posting state always reflects the job in flight and a second failure can be recorded.
- Each move stores the job id and the failure reason in the posting details and raises `ReserveHistoryPostingStateChangedEvent`.
- The job itself, its retries and the audit entries belong to Infrastructure and the Application.

### Claim documents

- A `ClaimDocument` is its own root, not a child of the claim. It refers to its claim by id and holds the metadata of spec table 9.7 in a `ClaimDocumentDetails` record: document type, name, blob path, content type, size in bytes and notes. The bytes live in blob storage behind `IStorageService`.
- `ClaimDocument.Upload` takes the claim's `ClaimReference`, the details, the actor and the time. It refuses a claim that is not editable (`DOCUMENT_CLAIM_NOT_EDITABLE`), a file over 50 MB (`S:DOC-MaxSize`) and a content type outside the allowlist of PDF, JPEG, PNG, DOCX, XLSX, TXT and CSV (`S:DOC-MimeAllowlist`).
- `Remove` is a soft delete (`IsDeleted`, `DeletedAt`) and takes the claim's state and the time. It refuses a frozen claim, a document already removed (`DOCUMENT_REMOVED`) and throws for a time before the upload.
- The blob path (`claim-documents/{organisationId}/{claimId}/{uploadId}_{sanitised filename}`, the one departure from the specification is described under the documents gap decisions), the filename sanitising, the one-hour download URL and the local-filesystem fallback belong to the Application and Infrastructure. The Domain receives the finished path and never touches storage.

| Rule | Code | Used by |
|---|---|---|
| IsClaimAcceptingDocuments | DOCUMENT_CLAIM_NOT_EDITABLE | upload, remove |
| IsFileSizeAllowed | S:DOC-MaxSize | upload |
| IsContentTypeAllowed | S:DOC-MimeAllowlist | upload |
| IsClaimDocumentNotRemoved | DOCUMENT_REMOVED | remove |

### Policies

- A `Policy` is a plain entity: number, one client name, period, coverage types. Policies come from outside and are seeded; there is nothing to create, edit or validate here, so it has no operations, rules, inputs or events.
- The claim holds a `PolicyId`. The Application reads the policy and passes only its `PolicyPeriod` on the claim inputs; the policy search and coverage endpoints read the seeded table directly.
- `PolicyPeriod` knows its own dates (inclusive end, the cancellation date, `CanCover`) and `RequireFor(policyId)`. `LossDate.IsCoveredBy(period)` is the fact behind the policy-period warning.

### Cause of loss

- A `CauseOfLossCode` is a plain entity like `Policy`: code, name, peril category (a plain enum) and an active flag. The codes come from outside and are seeded, so there are no operations, rules, inputs or events.
- The claim's loss holds one code as text. The Application reads the active codes and passes them on every claim input as `CauseCodes`, a value that knows whether a code is active (`IsActive`) and refuses to be built empty. The cause-of-loss list endpoint reads the seeded table directly.
- The value lives with the reference data, not under the claim's loss, because the list is shared and owned by no claim.

### Events

Inside the aggregate method, in order:

- Any claim move: first the issue evaluation with the input's facts (the only step before the chain), then the move.
- Any claim change: then the issue evaluation.
- A reopen: then the Open move from Reopened.
- A history submit: then the auto-approve attempt; a refusal only leaves the row pending.
- A reject or retract: then the posting cancel move.

Across the boundary, in the Application, driven by the events:

- An approval or auto-approval: then the posting job after the commit.
- A claim closed, withdrawn or reopened: the components close or reopen in the same Unit of Work.
- Every domain event: then its audit entry.

Each aggregate method has Domain tests; each event reaction has an Application test.

| Method | Event | Raised by |
|---|---|---|
| `Claim.Create` | `ClaimCreatedEvent` | claim |
| any claim change | `ValidationIssueRaisedEvent` per new issue, `ValidationIssueClearedEvent` per cleared one | claim, issue |
| `AddParty`, `RemoveParty` | `PartyAddedEvent`, `PartyRemovedEvent` | claim, party |
| every claim move | `ClaimStateChangedEvent` | claim |
| `SetLimitOverride` | `AggregateLimitOverrideChangedEvent` | claim |
| `WaiveIssue`, `AcknowledgeIssue` | `ValidationIssueWaivedEvent`, `ValidationIssueAcknowledgedEvent` | issue |
| `ReserveComponent.SubmitHistory` | `ReserveHistorySubmittedEvent` (component), then `ReserveHistoryApprovalStateChangedEvent` (row) when auto-approved | component, row |
| `ReserveComponent.Close`, `Reopen` | `ReserveComponentStateChangedEvent` | component |
| `ApproveHistory`, `RejectHistory`, `RetractHistory` | `ReserveHistoryApprovalStateChangedEvent`, and `ReserveHistoryPostingStateChangedEvent` for the cancel after a reject or retract | row |
| `MarkHistoryPosted`, `MarkHistoryPostingFailed`, `RetryHistoryPosting` | `ReserveHistoryPostingStateChangedEvent` | row |
| `ClaimDocument.Upload`, `Remove` | `ClaimDocumentUploadedEvent`, `ClaimDocumentRemovedEvent` | document |

No event: risk-object, loss, handler, severity, notes and policy-link edits, and `ReserveComponent.Open`.

### The Application layer

- **One result type.** Every command and query returns `Result` or `Result<T>`: a success holds the value and the warnings, a failure holds errors and no value. `ICommand<TResponse>` and `IQuery<TResponse>` constrain the response to `IFailableResult<TResponse>`, so a request that returns anything else does not compile, and `ValidationBehavior` can return a failed result instead of throwing. `ToResult` is the only conversion from a Domain outcome: Critical findings become validation errors, Warning findings travel as warnings on a success. The API maps the error kind (validation 422, not found 404, forbidden 403, conflict 409); the specification fixes only the wire body.
- **A use case is one file.** `UpdateNotesCommand.cs` holds the command with its `ToInput` method, its validator and its handler, so one file explains the use case. The folders mirror the Domain: the claim's own use cases are in `Claims/`, and each child entity has its folder (`Parties/`, `RiskObjects/`, `Loss/`, `Issues/`) with its DTO, request, mapping profile and use-case files.
- **A handler's flow.** It loads the claim through `IClaimRepository` (an unknown id is a not-found result), checks that a child id exists on the claim before the Domain call (the Domain would throw on an unknown id), reads the policy period and the active cause codes through their readers, calls one aggregate method with the input built by `ToInput`, commits only on success and maps the DTO after the commit. Readers are explicit and injected; there is no loader that hides the loads.
- **A status change** is one command whose handler looks the target status up in a dictionary of Domain calls, so there is no switch. The handler reads the policy period, the cause codes and the reserve facts, and a context record builds the Domain input from them for the chosen method. A claim that is closed or withdrawn closes its reserve components, and a reopened claim reopens them, through a handler that runs before the commit, in the same transaction.
- **Reserves** use one request with exactly one of a component type (a new component, opened and stored with its first row in one unit of work) or a component id. Approve, reject and retract return the row's approval move; the posting moves are commands the posting job and the retry button call. A handler that runs after the commit enqueues the posting job when a row becomes approved, or a failed posting returns to pending, so a job never runs for a change that was not stored.
- **Documents** are their own aggregate and their own Application folder. An upload builds the blob path with a unique upload id and a sanitised file name, runs the Domain rules before it touches storage, then stores the blob, the row and the commit in that order (a failed commit leaves an unreferenced blob, never a blocked name); removal is a soft delete that keeps the blob.
- **Reads** are queries in each aggregate's `Queries/` folder. They load no entity: a reader interface that Persistence implements projects straight into the DTO, and the claim detail is composed by its handler from the separate readers, so the reserves, documents and audit endpoints reuse the same parts. Every list is paged by a page and a page size from the request, with the size capped at 100, and a read scoped to a claim answers not found for an unknown claim. The list of statuses with their valid next statuses is not a table: the Domain's `ClaimTransitions` finds the claim transition classes by reflection, as the closed sets are found, and asks each one which states it allows from a given state, so a change to a transition changes the list. Download links are made on read through the storage interface with a one-hour lifetime.
- **Create claim** builds the claim and all its parties, risk objects and loss details in one sequence that stops at the first refusal and stores nothing unless every step succeeded (S:INTAKE-SingleCall). The optional initial reserve joins it with the reserve use cases.
- **Validation is shape only.** Ids not empty, enums known, required text not null, numbers the Domain would reject, maximum lengths from the specification's column sizes, and a name for each person or company party. Business rules stay in the Domain, unknown ids are the handlers' not found, and a null in optional text from outside becomes an empty string in the mapping to the Domain details.
- **Events and audit.** Each Domain event is wrapped in an `EventContext` (event id, time, actor, correlation id) when the Unit of Work dispatches it. Handlers run either before the commit, in the same transaction (audit entries), or after it (the posting job); the Unit of Work and the dispatcher are built with Persistence.
- **Closed sets** are created by one typed `From(kind)` per set, which a single reflection helper in the Domain finds once and caches; a test loops over every value of every set, so a new variant without its enum value fails the build.

### Design trade-offs

- **Entities with behaviour, steps behind them.** The aggregate's public methods are the model's API; the engine behind them gives every method the same error handling and order. The cost is one more hop and a generic base with a self type; the gain is that no method hand-writes its checks, every rule has a name and a code, and a refusal can never half-apply.
- **Follow-ups chained on the outcome, not hooks in the steps.** Hooks hid where a change happened and let a follow-up fail after it. The chain keeps the order visible in the method and the outcome the step's own; the deliberate limit is that a follow-up cannot refuse.
- **Facts passed in, not read.** Plain fields on the inputs keep the Domain free of infrastructure and testable with plain values; the alternative, a Domain interface that Persistence implements, was rejected. The cost is that fact definitions live in the Application.
- **The component owns its history.** A row is a child, so the balance check and the append run on one root, and `RowVer` makes the second of two concurrent writers retry. The invariants that span components (the claim limit) are checked against the facts on the input at submit and again at approval.
- **Cross-aggregate rules by state, not by call.** A frozen claim closes its components through an event, so each aggregate guards itself from its own state; the one place with no state to read, creation, takes the claim's reference.
- **No stored balance.** A reserve's balance is the sum of its approved rows, as the specification describes; a stored figure would need a second step to keep it in sync.
- **No runtime rule engine.** Rules are classes declared in code; a settings-driven engine would hide them from review and tests.
- **Events.** Raised in the success follow-ups; the Unit of Work publishes them before the commit for reactions in the same transaction and after it for effects outside.

## What the next layers must do

The Domain's contract with the Application and Persistence, so nothing is rediscovered when they are built:

- **Handlers** load the aggregate and the outside facts, build the input (time stamped once, actor and role from the authenticated user), call one aggregate method, read the outcome, save in one Unit of Work, then dispatch events and enqueue jobs. An unknown child id is a not found before the call.
- **Fact readers** return the Domain's own values: the claim's `PolicyPeriod` (from the policy) and `CauseCodes`, the reserve facts for close, withdraw and send to payment, the claim approved total, and the claim state or reference for reserve and document inputs.
- **The Unit of Work** collects events from every tracked entity, roots and children (the history rows raise their own events), wraps each in an event context and publishes before and after the commit.
- **A claim state-changed handler** closes or reopens the claim's reserve components in the same Unit of Work.
- **Persistence** maps the details records as complex properties, the history rows as a child table of the component with a unique index on component and sequence, `Policy` to a seeded table, and keeps the table names of the specification. Soft delete, the tenant, the audit columns and the claim-row touch are stamped by the save interceptor.
- **Jobs** call the posting steps with the job id; a sweep re-enqueues old Pending postings.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat entity-to-DTO, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a recursive type is introduced.
