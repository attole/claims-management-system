# Architecture

## Configuration approach

- Required settings are bound to options classes and validated at startup (`ValidateOnStart`); a missing one stops the
  app with the key named. The settings table and setup steps are in the root `README.md`.
- Secrets are never committed. Locally they live in the user-secrets store of `ClaimsModule.API`; deployed they come
  from app settings or Key Vault.
- `Swagger:Enabled`, `Hangfire:Dashboard:Enabled` and `DevAuth:Enabled` are booleans that default to false and never
  depend on the environment, so Azure can switch each one on through an app setting. `appsettings.Development.json`
  turns all three on.
- Risk: the dashboard filter allows every request when its switch is on, because a page load carries no Bearer token.
  Enabling it in a shared environment exposes job data to anyone with the URL.
- `DevAuth:Enabled` is bound but nothing reads it until the dev token endpoint exists.

## Task documents: reading and adaptation

### Spec versus assessment conflicts

Where the two task documents disagree, the specification is used and the assessment side is not implemented.

| Topic | Rule IDs | Decision |
|---|---|---|
| Reserve component kinds | S:RES-Components over A:RES-Components | the specification's four kinds and its rule on which kind may go negative |
| Reserve amount rule | S:BR-R-01 over A:BR-R-01 | the amount is a signed change; the balance may not go negative, except for the recoverable kind |
| Adjusting a reserve | S:RES-EventSourced over A:API-PUT-reserve | reserves are never updated; a change appends a history row |
| Over-limit handling | S:BR-R-05 over A:BR-R-07 | warning at submit, manager override before approval |
| Status transitions | S:BR-ST-01 over A:BR-C-06 | the specification's transition table is the state machine |
| SLA job | S:JOB-SLA over A:JOB-SLA | writes an audit entry only; no extra status |
| Tenant column | S:DATA-Tenant over A:DATA-Tenant | the specification's column name |
| Policy-period warning | S:BR-C-02 over A:BR-C-02 | must be cleared or acknowledged before opening |
| Claimant requirement | S:BR-C-03 over A:BR-C-03 | required before opening, not before saving the Draft |
| Entity set | S:ENT-Set over A:ENT-Set | the specification's tables are the model, with the departures below |
| Reserve decision routes | S:API-POST-reserve-approve over A:API-POST-reserve-decision | the specification's routes, which name a history row |

### How requirements were interpreted

- The sample 422 body (S:API-ErrorShape) fixes the shape of an error response only, not which rules reject a request.
- The status table defines Draft as incomplete or unvalidated, so a Draft is saved with its problems recorded as
  issues and a rule marked Critical means a stored Critical issue (S:STATUS-Values, S:VAL-Severity). Only a request
  that cannot be bound fails at the API edge.
- The reserve request amount is the signed change typed by the user; the "greater than zero" rule is applied to the
  resulting balance, at submission and again at approval.
- An approved reserve is a history row with the status Approved or AutoApproved. The documents' "posted/approved"
  wording for the current amount is read as the approval status, not the posting status.
- The manager limit of 10 million in the roles table is read as loose wording for the claim-wide cap with an override;
  a manager approves any amount above the supervisor limit (S:BR-R-02, S:BR-R-05).
- Reopened is a normal state that moves on to Open in the same operation, leaving two status-change rows.
- A policy period is two inclusive calendar dates, compared with the calendar date of the loss date in its own offset.
  The reported date and the loss event's report date are set by the system at creation. The estimated loss amount
  defaults to zero and is never negative.
- Role enforcement (S:ROLE-Enforcement) is modeled as roles in the Domain plus the rules the documents state
  explicitly (reopen, approval authority, the limit override). Which role may call which operation is decided in the
  Application layer; the documents give no table of actions permitted per claim status.
- The exact validation message texts from the specification are used verbatim in Domain rule classes, under a narrow
  exception in CLAUDE.md, and nowhere else.

### Schema departures from the spec

The model departs from the spec tables in the ways below. Each has its reason in the documents themselves.

| Change | Departure | Reason | Rules |
|---|---|---|---|
| Loss date | nullable column; the domain uses an empty value, never null | a Draft may be incomplete, and a missing date is a stored Critical issue | S:V-LossDate-Required, S:BR-C-01 |
| Cause code | text with no foreign key; an issue records a code that is unknown or inactive | same | S:BR-C-05 |
| Closed date, closure reason | dropped from the claim and derived from the status-change history: the time and reason of the latest change into Closed, empty unless the claim is currently Closed | avoids duplicating the history; the claims list and detail header show no closed date | S:ENT-Set |
| `ClaimValidationIssue` (added) | stored child of the claim: rule id, message, severity (Critical, Warning), state (open, cleared, acknowledged, waived) | the spec describes recorded issues but lists no entity | S:CC-02, S:VAL-Severity |
| `ClaimStatusChange` (added) | append-only child: from, to, optional reason (a code and a text), closing justification note, user, time; separate from the audit log | withdrawal, reopen and closing reasons have no columns | S:ST-Open-Withdrawn, S:BR-ST-04, S:CC-04 |
| Limit override flag (added) | boolean on the claim, false by default, set by a manager-only operation; the operation and its Application command exist, no controller | the claim-wide cap needs a flag the claim lacks and no endpoint is documented | S:BR-R-05 |
| Policy cancellation date (added) | nullable column on the simulated policy; the coverage window runs from the start to the earlier of the end date and the cancellation date; the stored status stays informational | a status alone cannot place a loss before a cancellation | S:BR-C-02, S:POL-Seed |
| Claim severity | its own type with an "unspecified" starting value; nothing sets it | the documents give no setter, default or rule | S:ENT-Set |
| Primary keys | generated in the domain as SQL Server-ordered sequential GUIDs, no database default, configured as never generated by the database | aggregates and events have ids at creation, tests need no database, and new child rows are inserted, not updated | S:DATA-Conventions, S:DATA-EfConfig |

Used as the spec defines them: the reserve history table (one append-only row per change, with the approval and posting
statuses on the row; "transaction" is prose, not a table) and the reserve components. A claim can have several
components, including several of one kind, and a component's current amount is recomputed from its approved rows and
stored on every change.

### Decisions on gaps in the documents

- **Liability determined** (S:ST-UnderInvestigation-PendingPayment). The documents define no data for it. The
  status-change request carries an explicit confirmation; the move from UnderInvestigation is blocked unless it is
  true, and the confirmation is written to the audit entry. Nothing is stored on the claim. This extends the request
  body beyond the documented target status and optional reason, and the frontend needs a matching control.
- **Withdrawal and reopen reasons.** A reason is valid only when both its code and its text are provided, for every
  withdrawal source and for reopen. This is stricter than the documents, chosen so every reason is reportable. The
  documents define no list of reason codes, so the code is free text for now.
- **Unspecified cases.** Authentication and authorisation failures, not-found and concurrent-edit conflicts; status
  codes for a reserve on a claim without a policy and for an oversized upload; who may retract a pending reserve; what
  approving or rejecting a non-pending reserve does; what an idempotency-key replay returns. Defaults are decided when
  the API pack is designed and recorded here.

## Backend architecture

### Domain engine

#### Purpose

The domain is a collection of different rules that apply in different states, and that collection grows as the
business defines more of it. Writing the rules as conditionals scattered across handlers and entities would turn every
new rule into a search through the code. Instead, the Domain project contains one general engine for state machines,
and all domain logic is expressed through it as states, transitions, operations and rules. The engine knows nothing
about claims or reserves. It only knows how to run rules against a subject and how to move a subject between states,
so extending the domain means adding a declaration, not changing the engine or the code that calls it.

The engine lives in the Domain project in three folders, each with its own namespace: `Engine/Operations` (the
operation, its context and its outcome), `Engine/Rules` (the rule contract, the rule set and the rule result) and
`Engine/Transitions` (the transition, its builder and the not-permitted error).

#### The operation

The operation is the base of the engine. It represents one business action performed on one subject, such as adding a
party to a claim or submitting a reserve. The subject is the aggregate or entity the action applies to. Rules read it
and the apply step changes it, and it is passed to the operation separately from the data of the action.

That data travels in an input, a plain record that exists for that operation alone. It holds the values the rules and
the apply step need, including any snapshot of outside data a rule depends on, such as the reserve totals of a claim.
The input type is defined in the Domain project, but the Application layer fills it, in the handler or in a dedicated
input builder.

Business conditions are expressed as rules. A rule is a small class behind a single interface: given the subject and the
operation context, it returns a rule result holding a list of errors and a list of warnings, either of which may be
empty. An operation declares its rules once, when it is constructed. A rule is declared either as a ready instance or as
a lambda that builds the rule from the input when the operation runs, so each rule receives only the part of the input
it needs. The lambdas are kept in a rule set that the operation owns.

The apply step is the only place where the subject changes. It works through the subject's own internal methods, which
keep the aggregate's invariants, raise domain events and mark the owning aggregate as modified. It returns the value of
the operation, such as the record that was created, or the `Success` value when there is nothing to return.

#### Executing an operation

Every operation runs in the same order. The caller passes the subject, the input and a time provider. The engine reads
the current time once into the operation context and shares that single value with the rules and the apply step, so a
run never sees two different times and the Domain reads no clock anywhere else.

The engine then checks the preconditions of the run. A plain operation has none, and a transition uses this step to
verify the subject's state. If a precondition fails, the run ends with a failed outcome and no rule is evaluated. The
engine then builds the rules from the input and evaluates all of them, aggregating every error and every warning. If
any error was reported, the run ends with a failed outcome and the subject is untouched. Otherwise the apply step runs,
a final step runs for subclasses that need one, and the outcome reports success together with any warnings and the
value the apply step returned. The outcome is a domain type, which the Application layer converts to its own result.

#### The transition

A transition is an operation that also moves its subject from one state to another, and it is what makes the engine a
state machine. It receives its target state in the constructor and declares one or more groups of allowed source states.
Each group carries its own rules, because the same target can be reached from different states under different
conditions. A rule required outside any group applies to every source.

The precondition of a transition is that the subject's current state is one of the declared sources. If it is not, the
run returns a transition-not-permitted error naming the source and the target; the Application layer adds the valid
targets from the full set of transitions, since one transition does not know the others. Otherwise the transition
evaluates the shared rules and the rules of the group for the current state, applies the change, and moves the subject
to the target. The state setter is internal to the Domain project, so a transition is the only way a state can change.
Declaring the same source in two groups is a mistake that is detected when the transition is built.

A transition author writes the constructor with the target and the source groups, and the apply step. Two members are
implemented once per subject type, usually in a shared base: how to read the subject's current state and how to set it.

Because sources and targets are declared on the transitions, the list of valid next states and the message for an
invalid move are derived from them: each transition exposes its target and its sources, and no separate table has to
be kept in step. A state can also name a follow-up state, in which case the subject moves on to it within the same run
and the history records both moves.

#### States

A state is a record from a closed set: an abstract record for the concept, one sealed record for each value, and an enum
marker that is the flag stored in the database. Transitions refer to states by record type, so a mistaken reference
fails to compile. States hold no rules themselves. The rules live on operations and transitions, and a restriction
such as "this action is not allowed in that state" is simply a rule that reads the subject's state.

#### Errors

Failures reported by rules are domain errors. The base error carries a code, which is the rule identifier from the
task documents, and a message. A rule violation can also name the field it concerns, and the transition-not-permitted
error carries the source state, the target state and the valid targets. The first two live in `Common` because other
domain types use them, such as a stored validation issue. The third lives with the transitions because only a
transition raises it. Errors that are not about a domain rule, such as not found, forbidden or a conflict, do not exist
in the Domain project; the Application layer creates them. A broken invariant is a programming error and is reported
with a standard exception, such as an argument or invalid-operation exception, instead of an error value.

#### What the engine leaves to other layers

The engine does not load data; input builders in the Application layer load any snapshot an operation needs. It does
not save changes or dispatch events, which is the work of the Unit of Work. It does not decide who may call an
operation, which the Application layer does, and it does not map errors to responses, which is the API's job.
Operations and transitions hold no state of their own, so a single instance serves every call, and they are found by
dependency injection scanning.

#### Extending it

Each kind of change to the domain has one place. A new state is a marker value and a record. A new allowed move is a
declaration on the target's transition. A new rule is a rule class plus a line that requires it. A new action is an
operation and its input record. The claim lifecycle, the claim edits, the party changes, the reserve submission and
approval, and the posting steps are all built this way.

The closed-set types and factory, validation layers, domain events, and EF Core notes are written with the packs that
build them.

## Known risks

### AutoMapper 14.0.0 advisory

- Pinned to 14.0.0 in `backend/Directory.Packages.props`, the last release before the commercial licence change
  (15 and later need a licence key). Advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion)
  affects versions below 15.1.1, and patched versions are commercial.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is flat
  entity-to-DTO and command-to-entity, with input bounded by FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a
  recursive type is introduced.
