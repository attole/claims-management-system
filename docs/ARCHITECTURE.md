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
- Reopened is a normal state (the reopen method, under Claims).
- A policy period is two inclusive calendar dates, compared with the UTC calendar date of the loss date, because all timestamps are stored in UTC.
- The claim's reported date (the first notice received) is the creation time, set by the system. The loss event's report date (the formal report) comes on the create input from intake, may be earlier than the creation time, is required (`LOSS_REPORT_DATE_REQUIRED`) and never changes afterwards. The estimated loss amount is optional and has no bound.
- Roles are nested as the specification describes them (S:ROLE-Supervisor, S:ROLE-Manager): a supervisor holds every handler right and a manager every supervisor right, so a role rule names only the lowest role (`HasMinimumRole`). The roles and that rule live in their own `Roles/` folder, shaped like an entity, because both aggregates use the roles and the engine in `Core` holds no business rule. Each role also states the reserve amounts it may decide (`CanDecideReserve`: a supervisor up to 100,000, a manager any amount), so approve and reject read the same tier. `UserRole` is a closed set whose variants list the roles whose rights they hold, so no rule depends on the order of an enum. The Application turns the role from the token into a variant with `UserRole.From`.
- Which role may call which operation is decided in the Application; the documents state only the explicit rules (reopen, approval authority, the limit override). Waiving a Critical issue is our own role rule (supervisor or above), kept in the Domain beside the others.
- The specification's validation messages are used verbatim in Domain rule classes and in the transition refusal, under a narrow exception in `CLAUDE.md`, and nowhere else.
- The specification's approval and posting status columns are states in the Domain.
- The closure message uses an ASCII hyphen where the specification has a dash.
- Closing and moving to pending payment have no role rule, because the specification's transition table lists none. Investigate and the move back to Open ask only for the handler role, which every role holds, so they have no role rule either; reopen (supervisor) is the only claim move that reads the role, and only its input carries it.

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
| Primary keys | sequential GUIDs generated in the Domain, never by the database | aggregates and events have ids at creation |
| Reserve current amount | not stored; the sum of the reserve's approved changes, computed by a query | the spec calls it computed from history |
| Reserve posting failure reason (added) | the failure reason is stored with the job id in a posting details record on the reserve change | the reserves tab can show why a posting failed; the spec only audits it |
| Reserve change component type and submission time (added) | the component type is copied from the reserve at submit, and the submission time is kept as a business time | approval reads the type from the change itself, and a review can be checked against the submission |
| Reserve review (changed) | who made the latest approval change, when, and the reason, on the reserve change instead of separate approved and rejected columns | a change is approved or rejected, never both, so half of the paired columns would always be empty |

### Decisions on gaps

- **Liability determined:** the status-change request carries an explicit confirmation, required for the move to PendingPayment and written to the audit entry; nothing is stored on the claim. The frontend needs a matching control.
- **Withdrawal and reopen reasons:** valid only with both a code and a text, stricter than the documents so every reason is reportable. The code is free text until a list exists.
- **Loss date after the report date:** a Critical issue (`LOSS_DATE_AFTER_REPORT`) when the loss falls on a later UTC calendar day than the report, because a loss cannot postdate its own report; a loss later on the report day passes, as intake may send the report date as a date only. The documents only forbid a date in the future (S:BR-C-01); since the report date is never later than the creation time, this rule covers S:BR-C-01 and replaces it, so the loss checks need no clock. The cost: a future loss date reports this code and message, not the spec's own.
- **Report date in the future:** creation is refused (`LOSS_REPORT_DATE_IN_FUTURE`) when the formal report date from intake is later than the creation time. The documents state no such rule; claims systems commonly offer it as an intake sanity check, and it keeps the report date a safe upper bound for the loss date.
- **Negative estimated loss:** refused by the value (an out-of-range exception), and the API validates it. The documents give the column only as a nullable amount.
- **No risk object:** a Warning issue (`CLAIM_NO_RISK_OBJECT`). The documents list it as a warning example and the intake screen shows an advisory, but no check existed.
- **Edits after Draft:** a Draft may be incomplete, but once the claim is past Draft a loss update that would leave a Critical loss issue is refused (`IsLossCompleteAfterDraft`), so a claim never falls back below what Open required. It is a claim rule on the claim's loss update step, next to the editable rule: it takes the proposed details and the loss reference from the input, reads the claim's state and the loss's report date, and runs the same loss checks as the issue evaluation, so the issues and the refusal share codes and messages. A problem whose issue was waived (same code and evidence) stays allowed, so a waiver does not make the loss read-only; a new problem of the same code is refused. Only the loss has such a guard; the last-claimant rule already protects the claimant.
- **Closed and withdrawn claims are frozen** (`CLAIM_NOT_EDITABLE`): parties, risk objects, loss details, severity, the handler, the policy link, the limit override and issue waivers and acknowledgements are refused on a Closed or Withdrawn claim, a reserve cannot be opened and a change cannot be submitted (`RESERVE_CLAIM_NOT_EDITABLE`), and the issue re-evaluation does not run, so no new issue appears. The documents state no such rule, but they make reopening a closed claim a supervisor move with a reason, and that gate would mean nothing if a closed claim could still change. Notes are the exception and stay editable in every state: they are commentary, not the facts a closure was decided on.
- **Policy cancellation ends cover:** the coverage window ends at the earlier of the expiration and the cancellation date. The documents give a policy a Cancelled status but no date, and a status alone cannot place a loss before or after the cancellation; ignoring it would pass a loss after the cancellation, and treating a cancelled policy as covering nothing would warn on a loss before it.
- **Reject follows the approval tier** (`HasRejectionAuthority`, S:BR-R-02): only a role that may approve an amount may reject it. The documents disagree: the role table gives supervisors and managers approve and reject authority up to their tier, while the reject endpoint only asks for the supervisor or manager role. The role table is the specification's own definition of what each role may do, so it wins; the refusal has its own message, because the specification's message speaks of approving.
- **Reserve amount messages:** the amount and balance refusals (`IsReserveAmountNonZero`, `HasNonNegativeBalance`, `IsApprovedBalanceNonNegative`) use their own wording instead of the specification's amount message. That message is written for positive amounts only, and a reserve amount here is a signed change, so it would be wrong for a decrease.
- **Withdraw settles the reserves first:** a withdraw is refused while a reserve change awaits approval (`CLAIM_WITHDRAW_PENDING_RESERVE`) or an approved change is not yet posted (`RESERVE_CHANGE_NOT_POSTED`), and with an open reserve balance it needs a note (`CLAIM_WITHDRAW_NOTE_REQUIRED`) and returns a warning (`CLAIM_WITHDRAWN_WITH_OPEN_BALANCE`). The documents only ask for a reason; without these, a change approved after the withdraw would post onto a withdrawn claim. The checks mirror the closure conditions with their own codes, because the specification's closure messages speak of closing.
- **Close waits for postings:** a close is refused while an approved change is not yet posted, Pending or Failed (`RESERVE_CHANGE_NOT_POSTED`), so the ledger matches the closed balance. This makes the posting re-enqueue sweep a requirement: a job lost between commit and enqueue would otherwise block the close for good.
- **A reserve change belongs to its reserve:** its claim, organisation and component type come from the reserve's own `Reference` at submit and stay on the change, so the approval reads the component type from the change, never from a request field.
- **A waiver lapses with its facts:** a finding carries the checked values as evidence, and an issue matches by code and evidence, so moving the loss date or relinking the policy after a waiver or an acknowledgement clears the old issue and raises a new one. Without it, an acknowledgement of a loss one day outside the policy would also cover a loss three years outside.
- **Decreases and the claim limit:** a change that does not increase the total passes the claim limit, at approval and at submit, so exposure can always be reduced. The manager override cannot be removed while the approved total is above the limit (`CLAIM_OVERRIDE_STILL_NEEDED`). The limit lives on the claim, next to the override flag: `Claim.IsWithinReserveLimit` for a total (the override removal) and `Claim.IsReserveChangeWithinLimit` for a change (submit and approval).
- **Waivers:** a missing loss date, an inactive cause code and a missing claimant can never be waived (`ISSUE_MUST_BE_FIXED`): the first two because the stored schema requires them, the claimant because opening requires one anyway (S:BR-C-03), so its waiver would show resolved while Open still refused. Every other Critical issue may be waived, by a supervisor or manager only. The documents allow waived issues before opening but name neither the waivable issues nor a role.
- **A handler before opening:** a Draft opens only with an assigned handler, and after Draft the handler cannot be removed (`CLAIM_HANDLER_REQUIRED` to open, `CLAIM_HANDLER_NOT_KEPT` to unassign). The specification's state list makes a handler part of Open, its transition table does not; the stricter reading wins so no open claim drops out of the handler filters.
- **What counts as an approved reserve for payment:** the fact behind the move to PendingPayment is a reserve of a non-recoverable type with an approved balance above zero, not any approved change, so a claim whose only approval is a recovery, or whose reserve was reversed to zero, cannot move to payment. The Application query owns this definition.
- **Transaction types are labels:** Add, Adjust and Reverse are recorded as the user picks them; the signed amount alone drives balances, limits and tiers. The documents give no definitions, so no type rule is invented.
- **Tiers use the size of the change:** the auto-approval limit and the approval tiers compare the absolute amount, so a large decrease needs the same approver as a large increase. A literal reading would auto-approve every decrease.
- **Drafts stay:** the specification's transition table has no exit from Draft and forbids unlisted moves, so a Draft remains until it is opened; the SLA job keeps flagging an abandoned one.
- **Party names:** a request validator in the Application requires a first or last name for a person and a company name for a company. No document states it as a business rule, so it is shape validation, not a Domain rule.
- **Claim numbers without gaps:** generated by a counter row per organisation and year, incremented with an atomic update inside the claim's transaction, as the specification prescribes, so a failed creation rolls the number back; a database sequence would leave gaps.
- **One primary risk object:** a risk object can be set primary (on add or edit) only while no other active risk object is primary; the primary is moved by editing the old one first.
- **Moves see current issues:** every claim move re-evaluates the issues with its input's reference before the transition runs, and again after it, so a cause code deactivated or a policy changed since the last edit is caught by the move's own rules (S:BR-ST-02, S:CC-02). A refused move keeps the refreshed issues: they are correct data, not a half-applied move.
- **The loss reference belongs to the claim's policy:** a `PolicyPeriod` names its policy, and the issue evaluation and the after-Draft guard throw when the reference was built for another policy than the claim's (`LossReference.RequireFor`); a reference without active cause codes throws when built. A wrong reference is a loading error, not a user mistake, and a silent re-evaluation with it would clear decided issues.
- **Auto-approval has no reviewer:** its review keeps the time and an empty reviewer; the AutoApproved state says the system decided, so the history never shows the submitter as an approver (S:BR-R-03).
- **Audited changes:** the events cover the specification's fixed audit list, plus `AggregateLimitOverrideChangedEvent` for the limit override, an authority decision. The policy link, the handler, severity, notes, loss and risk-object edits and opening a reserve raise no event; their trace is the audit columns.
- **Business times and actors are guarded:** a review earlier than the change's submission, or a waiver, acknowledgement or risk-object removal earlier than the claim's reported date, throws (`InputTime.NotBefore`), and a decision input without an actor throws (`InputActor.Required`), like a missing time. The Application stamps both, so a failure is a programming error, not a refusal.
- **Change sequence:** a submit with a sequence of zero or less throws; that the sequence is unique per reserve is a database index (`.claude/temp/notes-layer-persistence.md`).
- **A posting move names its job:** posted, failed and retry need a job id (`RESERVE_POSTING_JOB_REQUIRED`), so the stored job id always belongs to the last move; cancel has none.
- **A reversal while payment is pending is allowed:** a reserve on a PendingPayment claim can be reduced to zero, because a reserve stays independent of the claim state, like a decrease under the claim limit; a claim left with nothing to pay is closed.
- **Claim number cap:** the specification's format holds seven sequence digits per organisation and year, so `ClaimNumber.Create` throws past 9,999,999; the counter refuses first with a conflict.
- **No-policy message:** the warning issue and the reserve block both use the validation table's wording, which names reserves; the business-rule text differs only in phrasing.
- **Handler submissions above 10,000:** the role table's handler limit is read as the auto-approval authority, so a handler may submit any amount and the change waits for an approver, as the approval flow describes.
- **Unspecified cases** (authentication failures, not found, concurrent edits, oversized uploads, idempotency-key replays) are decided when the API pack is designed and recorded here.

## Backend architecture

### Layers

Clean Architecture with five projects, dependencies pointing inward: `API` -> `Application` -> `Domain`. `Persistence` and `Infrastructure` implement interfaces that `Application` defines. The Domain has no project references and no packages.

- **Domain:** the aggregates, their rules and events, and the small engine that runs a change.
- **Application:** the use cases. It loads data, builds the input with the facts the rules need, calls one aggregate method, reads its outcome, saves through the Unit of Work, dispatches events and maps findings to responses.
- **Persistence:** EF Core mapping, with a save interceptor for the audit columns, soft delete, the tenant and the root touch.
- **Infrastructure:** Hangfire jobs and outside adapters.
- **API:** controllers and startup.

The Application and Persistence designs are written when those layers are built; the open points are in `.claude/temp/notes-layer-application.md` and `.claude/temp/notes-layer-persistence.md`.

### The domain model

The Domain is a domain-driven model: the claim with its children, the reserve and the reserve change are entities that own their state and change it only through their own public methods. The Application calls `claim.AddParty(input)` and the claim decides what that means.

Inside those methods every change runs through one small engine, which exists for error handling and order. A step checks all of its rules before it changes anything and returns the refusals as findings, all at once and never as exceptions, so a refused change leaves the aggregate untouched; it applies its change in one place, so "rules, then the change, then what follows" is the same order for every method. Rules are named classes, and a new one is a class plus the line that requires it. The engine knows nothing about claims or reserves.

| Concern | Where it lives | How |
|---|---|---|
| State and its changes | the aggregate, an entity | public methods; `internal` setters and child mutators; lifecycles as closed sets |
| The conditions of a change | rules, internal classes with a `Code` | declared once per step; a failed rule is a finding |
| Running a change | steps: operations, creations and transitions | rules first, then `Apply`; the result is an `Outcome` |
| What follows a successful change | the aggregate method's chain | `OnSuccess` hooks: raise the event, record the move, run an inner step, settle |
| Facts from outside the aggregate | plain fields on the input | computed by the Application before the call |
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

### Context from the Application: inputs

The Domain never loads, reads no clock for business times and knows no identity. Everything a change needs that the aggregate does not hold arrives on the input: the request data, the time and the actor where the step records them, the actor's role where a rule reads it, and the facts from outside the aggregate as plain fields, only on the inputs whose rules read them.

| Input | Outside facts | Used by |
|---|---|---|
| every claim input | `LossReference`: active cause-of-loss codes, the period of the claim's current policy | the issue evaluation around every change, the loss update |
| `CreateClaimInput` | the organisation, the claim number from the counter, the formal report date | create |
| `FreezeClaimInput` | a reserve change pending, an approved change not yet posted, a reserve balance open | close, withdraw |
| `ReopenClaimInput`, `RejectReserveInput`, `WaiveIssueInput` | the actor's role | reopen, reject, waive |
| `SendToPaymentClaimInput` | an approved reserve | send to payment |
| `SubmitReserveInput` | the reserve's own reference (`reserve.Reference`: reserve, claim, organisation, component type), claim editable, policy linked, claim approved total, reserve approved balance, change sequence | submit |
| `OpenReserveInput` | claim editable | open a reserve |
| `AutoApproveReserveInput` | claim approved total, reserve approved balance; no reviewer | auto-approve |
| `ApproveReserveInput` | the same, plus the override flag and the actor's role | approve |
| `SetLimitOverrideInput` | the claim approved total, the actor's role | set or remove the limit override |

- `LossReference` is the one facts record every claim input carries, because every claim change re-evaluates the issues. `ReserveReference` is the other record: the reserve change's claim, organisation and component type must all come from its own reserve, so the Application passes `reserve.Reference` at submit and the change keeps the component type.
- The definition of each fact (for example "approved" means Approved or AutoApproved) lives in one Application query, covered by Application tests.
- A stale fact is covered by the claim-row touch on save (see Concurrency under Reserves).

### Model conventions

- **Aggregates:** the claim is a root with children (parties, risk objects, loss, issues, state changes). A reserve and a reserve change are two roots. Aggregates refer to each other by id only.
- **Entity data in one record,** replaced whole, so an edit cannot half-apply and EF maps it as a complex property.
- **No null:** a value that may be missing is a record with an `Empty` instance and an `IsSet` flag, because EF cannot map a struct to a nullable column.
- **Ids:** sequential GUIDs made in the Domain by `Ids.Next()`, which writes the current UTC time into the bytes SQL Server sorts by, so inserts land at the end of the index (a COMB-style generator; a GUID v7 puts its time in the bytes SQL Server compares last, so it would not be sequential there). It is the only clock read in the Domain: an id is a technical value, not business data.
- **Business times and decisions:** `ResolvedAt` and `ResolvedBy` on an issue, `DeletedAt` on a risk object, the change's `SubmittedAt` and the reserve review come from the input, and only the inputs whose step records one carry a time. They are Domain state, not audit columns: a later clear modifies the issue row, so `UpdatedAt` and `UserModified` would overwrite who decided. An automatic clear records neither, because nobody decided it.
- **Events and audit:** events carry business facts only; the Application wraps each in an envelope (event id, time, actor) at dispatch, and Persistence stamps the audit columns.
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
- A transition is configured by a target and the source states it is allowed from, each with its rules; a move from an undeclared state returns one finding without running any rule. The engine returns the `Move`; the aggregate's transition base sets the state (and on the claim writes the history row). Who may make a move is a rule on the transition.
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

- The closure conditions are S:CC-01 to S:CC-04 and `RESERVE_CHANGE_NOT_POSTED` (Decisions on gaps).
- A failed close returns every failed condition at once.
- A close with an open reserve balance also returns the S:CC-04 warning as a Warning finding, whether or not a note was given.
- Any move may carry a note; it is stored on the state-change row, and a close with an open balance requires it.
- Each move appends a `ClaimStateChange` row and raises `ClaimStateChangedEvent`.
- A reopen leaves two rows because the reopen method runs the Open move right after it; Open from Reopened has no rules, so that inner step cannot be refused.

**Issue rules.**
- Critical: loss date present, loss date not after the report date (covers S:BR-C-01), description long enough, a claimant, and an active cause of loss code (S:BR-C-05).
- Warning: no policy linked, no risk object (`CLAIM_NO_RISK_OBJECT`), and loss date outside the policy period (S:BR-C-02).
- The policy period ends at the earlier of the expiration and the cancellation date; both ends are inclusive.
- A claim holds its policy id; `LinkPolicyOperation` sets it, and the Application looks the policy up.
- `AcknowledgeIssueOperation` resolves an unresolved Warning issue; Critical issues are waived, not acknowledged.
- A removed party is deactivated (`IsActive`) and stays on the claim; a removed risk object is soft-deleted with its `DeletedAt`.

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
| HasNoPendingReserveChange, HasNoCriticalIssuesForClose, HasClaimantForClosure, HasClosingNoteForOpenReserves, HasNoOpenReserveBalance (warning) | S:CC-01 to S:CC-04 | close |
| HasNoUnpostedReserveChange | RESERVE_CHANGE_NOT_POSTED | close, withdraw |
| HasNoPendingReserveChangeForWithdrawal, HasWithdrawalNoteForOpenReserves, HasNoOpenReserveBalanceOnWithdrawal (warning) | CLAIM_WITHDRAW_PENDING_RESERVE, CLAIM_WITHDRAW_NOTE_REQUIRED, CLAIM_WITHDRAWN_WITH_OPEN_BALANCE | withdraw |

A combined rule has a `Code` that names it but no finding of its own: it returns the findings of the rules it runs.

### Reserves

**Model.**
- A `Reserve` is a thin root: claim id, component type, status (Active or Closed, informational) and notes.
- A claim can have several reserves, including several of one component type.
- The component types are Indemnity, Expense, Alae and SubrogationRecoverable; only the recoverable type may go negative.
- A `ReserveChange` is its own root: reserve id, claim id, component type, transaction type (Add, Adjust or Reverse), signed amount, reason, change sequence, submitter and submission time, approval state, posting state, review and balances.
- A reserve change refers to its reserve and claim by id; the reserve holds no collection of changes and no balance.
- The idempotency key of a change is `Reserve:{reserve id}:Change:{change sequence}`.

**Lifecycle of a change.**

| Step | Kind | Result |
|---|---|---|
| Submit | creation operation | a pending change, balances empty |
| AutoApprove | transition | PendingApproval -> AutoApproved |
| Approve | transition | PendingApproval -> Approved |
| Reject | transition | PendingApproval -> Rejected |
| Retract | transition | PendingApproval -> Cancelled |
| MarkPosted | transition | Pending -> Posted (posting state) |
| RetryPosting | transition | Failed -> Pending (posting state), with the new job id |
| MarkPostingFailed | transition | Pending -> Failed (posting state) |
| CancelPosting | transition | Pending -> Cancelled (posting state) |

- A change is always created pending, like a claim is created as a Draft.
- The submit method runs AutoApprove right after the submit; a refusal there only leaves the change pending.
- Approved, AutoApproved, Rejected and Cancelled changes never move again.
- A correction is a new Adjust or Reverse change.
- Every approval change stores the reviewer (none for an auto-approval), the time and the reason in the review and raises `ReserveApprovalStateChangedEvent`.
- The change fills its balances (previous and new) when it becomes approved.
- The posting state starts Pending and moves independently of the approval state.

**Authority.**
- An amount up to 10,000 may be auto-approved, whatever the submitter's role.
- Approving up to 100,000 needs a supervisor or a manager; above that, a manager.
- Rejecting follows the same tiers and needs a reason.
- Retracting is the submitter's.
- Nobody approves their own change.
- A handler may submit above their own limit; the change waits as pending.
- A change that would take the claim total over 10 million is never auto-approved, even with the override flag set.
- At approval, such a change needs the manager override flag on the claim.
- At submit, the same case only returns a warning, which is not stored as an issue.

**Rules.** Each rule takes the facts it reads as plain values; the auto-approval limit and the balance after a change are on `ReserveChangeDetails`, the approval tiers on `UserRole.CanDecideReserve`, the claim limit on `Claim`.

| Rule | Code | Used by |
|---|---|---|
| IsClaimAcceptingReserves | RESERVE_CLAIM_NOT_EDITABLE | submit, open a reserve |
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
| IsReserveChangeApproved | RESERVE_CHANGE_NOT_APPROVED | mark posted, mark posting failed |
| IsReserveChangeRefused | RESERVE_CHANGE_NOT_REFUSED | cancel posting |
| HasPostingJob | RESERVE_POSTING_JOB_REQUIRED | mark posted, mark posting failed, retry posting |

**Concurrency.** Every reserve change also touches the claim row on save in Persistence, which serializes concurrent approvals on one claim and keeps the claim's last-activity time current.

**Posting.**
- The posting moves are transitions on the change that move its posting state.
- Posted and Failed need an approved change, and they and a retry need a job id; a cancel needs a rejected or retracted one.
- A repeated post on a Posted change returns the not-permitted finding, which the job reads as already done.
- A Failed change is retried by moving it back to Pending with the new job's id, then posted, so the posting state always reflects the job in flight and a second failure can be recorded.
- Each move stores the job id and the failure reason in the posting details and raises `ReservePostingStateChangedEvent`.
- The job itself, its retries and the audit entries belong to Infrastructure and the Application.

### Design trade-offs

- **Entities with behaviour, steps behind them.** The aggregate's public methods are the model's API; the engine behind them gives every method the same error handling and order. The cost is one more hop and a generic base with a self type; the gain is that no method hand-writes its checks, every rule has a name and a code, and a refusal can never half-apply.
- **Follow-ups chained on the outcome, not hooks in the steps.** Hooks hid where a change happened and let a follow-up fail after it. The chain keeps the order visible in the method and the outcome the step's own; the deliberate limit is that a follow-up cannot refuse.
- **Facts passed in, not read.** Plain fields on the inputs keep the Domain free of infrastructure and testable with plain values; the alternative, a Domain interface that Persistence implements, was rejected. The cost is that fact definitions live in the Application.
- **Two reserve aggregates.** A reserve change is its own root so that transitions can move it and several changes can be pending on one reserve. The invariants that span them (non-negative balance, claim limit) are checked against the facts on the input at submit and again at approval.
- **No stored balance.** A reserve's balance is the sum of its approved changes, as the specification describes; a stored figure would need a second step to keep it in sync.
- **No runtime rule engine.** Rules are classes declared in code; a settings-driven engine would hide them from review and tests.
- **Events.** Raised in the success follow-ups; the Unit of Work publishes them before the commit for reactions in the same transaction and after it for effects outside.

### Aggregate follow-ups and events

Inside the aggregate method:

- Any claim move, first the issue evaluation with the input's reference (the only step before the chain).
- Any claim change, then the issue evaluation, using the loss reference on every claim input.
- A reopen, then the Open move from Reopened.
- A reserve submit, then the auto-approve attempt; a refusal only leaves the change pending.
- A reject or retract, then the posting cancel move.

Across the boundary, in the Application, driven by the events:

- An approval or auto-approval, then the posting job after the commit.
- Every domain event, then its audit entry.

Each aggregate method has Domain tests; each event reaction has an Application test.

| Method | Event | Raised by |
|---|---|---|
| `Claim.Create` | `ClaimCreatedEvent` | claim |
| any claim change | `ValidationIssueRaisedEvent` per new issue, `ValidationIssueClearedEvent` per cleared one | claim, issue |
| `AddParty`, `RemoveParty` | `PartyAddedEvent`, `PartyRemovedEvent` | claim, party |
| every claim move | `ClaimStateChangedEvent` | claim |
| `SetLimitOverride` | `AggregateLimitOverrideChangedEvent` | claim |
| `WaiveIssue`, `AcknowledgeIssue` | `ValidationIssueWaivedEvent`, `ValidationIssueAcknowledgedEvent` | issue |
| `ReserveChange.Submit` | `ReserveSubmittedEvent`, then `ReserveApprovalStateChangedEvent` when auto-approved | change |
| `Approve`, `Reject`, `Retract` | `ReserveApprovalStateChangedEvent`, and `ReservePostingStateChangedEvent` for the cancel after a reject or retract | change |
| `MarkPosted`, `MarkPostingFailed`, `RetryPosting` | `ReservePostingStateChangedEvent` | change |

No event: risk-object, loss, handler, severity, notes and policy-link edits, and `Reserve.Open`.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat entity-to-DTO and command-to-entity, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a recursive type is introduced.
