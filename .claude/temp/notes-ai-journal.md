status: active

Running record of how AI is used on this project: prompts worth showing, AI mistakes and their fixes, the AI/manual split, and an honest assessment. Raw material for the final AI-WORKFLOW.md. Written by the ai-journal skill (.claude/skills/ai-journal/SKILL.md); to be reviewed and turned into the final document at the end.

FORMAT

Append-only. One entry per block, newest last. The entry headers are the map: `grep "^## " .claude/temp/notes-ai-journal.md` lists every entry.

Header line: `## <TYPE>-<NNN> | <phase> | <YYYY-MM-DD>`
- TYPE is one of PROMPT, CORRECTION, SPLIT, ASSESSMENT, SESSION, SUMMARY. NNN is a 3-digit counter per type, never reused.
- phase is the full label: p0 setup, p1 domain, p2 data, p3 backend, p4 frontend, p5 review (for example `p3 backend`).
- Entries are separated by one blank line. If the phase is unknown, ask.

Then fixed `key: value` lines, one per line, each value a single line of at most 200 characters (600 for SUMMARY), no blank lines inside an entry:
- PROMPT: purpose, prompt, outcome (kept | changed | dropped), note
- CORRECTION: what, caught-by (user | code-review | spec-check | test | build), fix, lesson
- SPLIT: item, who (ai | manual | hybrid), note
- ASSESSMENT: area, helped (high | medium | low), note
- SESSION: name, phase, note
- SUMMARY: topic, story, decisions, ai-errors, human-vs-ai, refs (a consolidated account of one topic or pack, in a few sentences per field; refs lists the raw entry IDs it covers, or `none`)

Rules:
- Capture broadly; when in doubt, log. The final document is compiled and filtered later.
- Only real events. Claude drafts, the user confirms; nothing is invented or embellished.
- Paraphrase. Never copy text from the confidential local documents (docs/*.local.md), never include secrets, tokens, or connection strings. Rules are referenced by source-qualified ID only.
- Prompt values are the user's prompt trimmed or paraphrased, never pasted document text.

ENTRIES

## CORRECTION-001 | p3 backend | 2026-10-10
what: AI proposed direct Hangfire packages (AspNetCore, SqlServer) on the API project instead of keeping all background-job wiring behind the Infrastructure layer
caught-by: user
fix: Hangfire registration and dashboard moved into AddInfrastructure and UseInfrastructure extension methods; API references only layer projects
lesson: each layer registers its own services via extension methods; the host takes no direct dependency on a layer's technology packages

## CORRECTION-002 | p3 backend | 2026-10-10
what: AI suggested pinning AutoMapper below 15 after checking advisories; the check showed 14.0.0 is affected by an advisory whose fixes are only in the commercial releases
caught-by: build
fix: user chose to accept the risk on 14.0.0; that single advisory is suppressed in the audit settings and the risk is recorded in docs/ARCHITECTURE.md
lesson: verify advisories before recommending a version pin; warnings-as-errors surfaces accepted risks, so suppress by advisory ID and document it

## CORRECTION-003 | p3 backend | 2026-10-10
what: AI left the Enabled feature switches with no inline default and argued the implicit false was enough; user wanted the fail-closed default written in the class
caught-by: user
fix: added an explicit false default to the Enabled property of the Swagger, Hangfire dashboard and dev auth options
lesson: when a safe default carries meaning, state it in the type instead of relying on the language default and an explanation

## SPLIT-001 | p3 backend | 2026-10-10
item: backend backbone: slnx, five layer projects, three test shells, central packages, options with startup validation, feature switches, per-feature registration
who: hybrid
note: AI wrote the files; user decided the shape: Hangfire behind Infrastructure, central packages, test split, per-feature extensions, AutoMapper 14 risk accepted

## ASSESSMENT-001 | p3 backend | 2026-10-10
area: backend scaffolding and dependency setup
helped: high
note: fast on layout, package versions and advisory checks; weaker on machine-specific NuGet fallback and runtime problems, and the first draft needed user pushback on layering

## PROMPT-001 | p3 backend | 2026-10-10
purpose: scaffold the backend from the task documents instead of from model memory
prompt: asked Claude to read the stack and frontend connection from the specs, then create every layer as its own project with NuGet dependencies and versions taken from them
outcome: changed
note: Claude returned an outline with argued options first; user picked slnx, central packages, MediatR 12, STJ, test shells, Swagger option 2, and the pack was built after

## PROMPT-002 | p3 backend | 2026-10-10
purpose: gate Swagger, the job dashboard and mock auth in deployed environments without an environment check
prompt: asked for boolean settings read through the options pattern that switch these on in Azure config, with code setup driven by the options
outcome: kept
note: user supplied the design (booleans plus options) and Claude implemented it; fail-closed defaults and the open dashboard risk were documented afterwards

## SESSION-001 | p3 backend | 2026-10-10
name: backend dev work
phase: p3 backend
note: backend backbone: layers, packages, options validation, feature switches, validation behavior, two code reviews, journal entries CORRECTION-001 to 003

## CORRECTION-004 | p3 backend | 2026-10-10
what: AI code review wrongly claimed editorconfig severity suffixes and a const naming rule would not work in builds; and the earlier enforce-style fix left naming rules (IDE1006) unenforced
caught-by: build
fix: scratch file with deliberate violations showed the review claims were false and naming rules passed silently; added IDE1006 as a warning so bad names now fail the build
lesson: verify AI review claims and enforcement settings with a deliberate failing case instead of trusting either; a rule that never fired is not proof it works

## SUMMARY-001 | p3 backend | 2026-10-10
topic: layer structure and package choices
story: Claude read the required stack from the task documents and proposed five layer projects with argued options for solution format, package management, Swagger and tests. The user chose slnx, central package management, MediatR 12 pinned, three test shells and the first-party OpenAPI generator with the classic UI. The package pass found AutoMapper 14.0.0 flagged by an advisory whose fixes sit behind the commercial licence.
decisions: User kept Hangfire wholly inside Infrastructure, so the API takes no direct package and each layer registers itself by extension methods. AutoMapper stays on 14.0.0 with a suppression for that one advisory and the risk written into the architecture doc.
ai-errors: Claude first placed Hangfire packages on the API project and suggested pinning AutoMapper below 15 without checking the advisory first. The user challenged the layering; the advisory check showed the pin was unsafe.
human-vs-ai: Claude researched versions, drafted options and wrote the projects; the user decided every structural boundary and which risk to accept.
refs: PROMPT-001, SPLIT-001, ASSESSMENT-001, CORRECTION-001, CORRECTION-002

## SUMMARY-002 | p3 backend | 2026-10-10
topic: configuration and feature switches
story: Settings moved to options classes validated at startup, so a missing connection string, signing key or CORS origin stops the app with the key named. The user then asked for boolean switches for Swagger, the job dashboard and the mock auth, read through the options pattern and independent of the environment, so Azure can open them by app setting. Secrets stay in user-secrets locally and app settings or Key Vault when deployed, with setup written in the README.
decisions: Switches default to false, state the default inline, and live in per-feature folders. The dashboard filter allows every request when on, which is documented as a risk. The dev token endpoint is planned as a separate pack behind its own switch.
ai-errors: Claude left the switch default implicit and argued it was enough; the user wanted it written in the type. An earlier fallback to an empty connection string hid missing config until code review flagged it.
human-vs-ai: The user supplied the switch design and the folder layout; Claude implemented, ran the API against LocalDB and checked each endpoint.
refs: PROMPT-002, CORRECTION-003

## SUMMARY-003 | p3 backend | 2026-10-10
topic: build enforcement and verifying AI review
story: Style rules were made to fail the build, then tested with deliberate violations. That showed two review claims were false and that naming rules had never been enforced. The validation pipeline behavior was added with tests tied to the validation rule, after a build error exposed a dropped cancellation token.
decisions: Naming and unused-using rules are warnings treated as errors, confirmed by scratch files. The validation behavior throws for now and will return a failed result once the result type exists.
ai-errors: A second AI code review reported editor-config problems that did not exist, while the real gap was missed; scratch builds disproved the claims and exposed it. The behavior draft also dropped a cancellation token and used the wrong delegate shape.
human-vs-ai: The user asked for the checks and set the bar; Claude ran them. AI review output was treated as a claim to test, not a verdict.
refs: CORRECTION-004

## CORRECTION-005 | p1 domain | 2026-10-11
what: AI read the generic 422 sample and the NOT NULL and foreign key columns as proof that create rejects a bad date or unknown cause code, overriding the rules' Critical severity
caught-by: user
fix: User pointed to the status table (Draft may be incomplete) and said Critical means stored; schema relaxed for Drafts: empty loss date, cause code as text, issues stored as a child entity
lesson: An illustrative sample is not a rule; when schema and rule severity disagree, surface it as a conflict instead of choosing, and record the schema deviation in the architecture doc

## PROMPT-003 | p1 domain | 2026-10-11
purpose: understand how the domain works before designing anything
prompt: asked Claude to explain the domain in real-life terms: what goes in and out, the data flow, what must be processed and how it works, grounded in the spec
outcome: kept
note: led to a read of every section, the conflict list, and the decision to split the domain into a claim side and a reserve side

## PROMPT-004 | p1 domain | 2026-10-11
purpose: steer the lifecycle design away from enums and if/else checks
prompt: said statuses and rules should be records or objects with static rules and states, not booleans and if/else in code, and asked for abstractions wherever possible
outcome: changed
note: drove the state and transition object design; the first proposals were refined through many follow-up objections

## ASSESSMENT-002 | p1 domain | 2026-10-11
area: domain reading and design
helped: high
note: strong at reading the documents, finding conflicts and listing error cases; first abstraction proposals were over-built and needed repeated simplification by the user

## SPLIT-002 | p1 domain | 2026-10-11
item: domain design for the claim side and the notes that record it
who: hybrid
note: user made every business and structural decision; Claude searched, explained, argued options and wrote the design note and the architecture entries

## CORRECTION-006 | p1 domain | 2026-10-11
what: Claude kept adding layers to the transition design (edge list, info interface, capability interfaces, facts generic, shared input base), scattering the context of one rule over several types
caught-by: user
fix: one input record per transition, rules as plain classes created by lambdas, one dictionary of rules by source, no capability interfaces
lesson: keep the data of a rule next to the rule and prefer one mechanism over parallel abstractions; ask what every extra type is for

## CORRECTION-007 | p1 domain | 2026-10-11
what: Claude recommended storing withdrawal and reopen reasons as fields on the claim, which brought set, clear and stale-value rules that kept growing
caught-by: user
fix: reasons moved to an append-only status-change history owned by the claim; the closed date and closure reason are derived from it
lesson: data about an event belongs in a record of the event, not in fields of the current state

## SUMMARY-004 | p1 domain | 2026-10-11
topic: understanding the domain and reading the documents
story: The work started with a shared picture of the domain: a loss is reported and becomes a claim, a handler moves it through a status lifecycle while investigating, and the claim carries reserves, which are cost estimates needing role-based approval and posted to a simulated ledger by background jobs. Claude read the rule register and the spec sections on lifecycle, intake, reserves, jobs and entities, explained the data flow end to end, and listed where the spec and assessment disagree. A web search on established claims platforms showed drafts are saved incomplete and only promotion is gated.
decisions: The user decided the spec wins every conflict between the two documents and asked that each be marked in the design note and its decisions; eleven conflicts are recorded by rule ID. The user split the domain into a claim side, a reserve side, and reference data with the audit log, started with the claim side, and deferred the reserve questions until the claim side is designed.
ai-errors: No wrong output in the reading itself. The reserve explanation needed a second pass before it was clear that a component is a bucket per cost type with its own transaction history, that the balance is derived from approved transactions, and that approved means an accepted estimate change, not money paid.
human-vs-ai: Claude read, searched and explained; the user steered the order of work, chose the spec over the assessment, and set the scope. The user insisted on understanding the real business flow before any code, which shaped every later design choice.
refs: PROMPT-003, ASSESSMENT-002

## SUMMARY-005 | p1 domain | 2026-10-11
topic: draft claims, validation and a deliberate schema departure
story: The question was whether create rejects bad input or saves a Draft with the problems recorded. The documents pull both ways: the validation section and a sample 422 body suggest rejection, while the status table, the Draft to Open gate and the closure conditions assume issues stored on a saved claim. Claude searched both documents for every error, warning and blocking case and compiled one list of 38. The user settled it: the status table defines Draft as incomplete or unvalidated, so Critical means a stored Critical issue, not a rejection.
decisions: A Draft stores Critical and Warning issues as a child entity of the claim, which the spec does not list. To allow that, the loss date column is nullable and the cause code is plain text with no foreign key, both deviations from the spec table and recorded in the architecture doc. Draft to Open needs a real date and an existing active code. Only a request that cannot be bound fails at the API edge. The sample 422 body is treated as illustrative of the shape only.
ai-errors: Claude first read the sample 422 body and the NOT NULL and foreign key columns as proof of rejection and recommended rejecting a bad date and an unknown cause code. That overrode the stated Critical severity of the rule and hid a conflict inside the spec. The user caught it by pointing to the status table; the correction is logged as CORRECTION-005.
human-vs-ai: The user supplied the interpretation rule (follow the stated severity and the Draft definition) and chose the schema option. Claude searched, listed the cases, argued three options, and wrote the decisions and the deviation into the design note and the architecture doc.
refs: CORRECTION-005

## SUMMARY-006 | p1 domain | 2026-10-11
topic: the claim lifecycle modeled as objects
story: The user did not want statuses as enums with if chains in Application. The design went through rounds: a transition table was rejected because transitions need different inputs, a shared context with optional fields was rejected, and capability interfaces with facts generics were rejected for scattering a rule across types. The final shape: a status enum as a marker, state records, one transition class per target with a fluent To, From and Require declaration, rules as plain classes built from a typed input record by a lambda, reflection discovery, and input builders in Application.
decisions: Targets reachable from sources with different checks use one From group per source (PendingPayment). Rules return all blockers at once. Outside data such as reserve counts arrives as a snapshot on the input, built through a read repository. The state setter is internal so only transitions change status. Reopened is a normal state with an automatic follow-up to Open. Time is passed in, and ids are generated in the domain as SQL Server-ordered GUIDs, where the spec says database default.
ai-errors: Claude kept adding layers: an edge list with a common-rules list, an info interface, capability interfaces, a facts generic and a shared input base. Each time the user asked what a piece was for and the design got simpler. One claim, that rules are less discoverable in the lambda form, was overstated and withdrawn. Logged as CORRECTION-006.
human-vs-ai: The user drove the design by objection and by naming the shape wanted: typed input per transition, self-contained rules, a chained builder, reflection discovery. Claude produced the options, scratch sketches and file layouts, and recorded the result in the design note.
refs: PROMPT-004, CORRECTION-006

## SUMMARY-007 | p1 domain | 2026-10-11
topic: reasons, status history and derived fields
story: Withdrawal and reopen need reasons that the spec gives no columns for. Storage went through three shapes: reuse the closure column, new columns on the claim, then an append-only status-change history owned by the claim. The user chose the history once the problems of state-specific fields were clear: clearing rules, stale values, and a reopen reason that belongs to a phase, not one state. The closed date and closure reason are now derived from the history, and the closing justification rides on the closing row. Liability determined became an explicit confirmation on the request.
decisions: A reason is a record with a code and a text, and both are required for every withdrawal and reopen, stricter than the spec and chosen for reporting. The history holds from, to, reason, who and when, and a reopen leaves two rows. The closed date and closure reason columns of the spec are dropped. Liability determined is a request field, audit-logged, enforced only from UnderInvestigation. Severity stays unset. Errors split into a domain error and an application error.
ai-errors: Claude recommended keeping reasons as fields on the claim, which brought set-and-clear questions that kept growing. The user redirected by asking why the fields lived on the aggregate at all, and the history option resolved them. Logged as CORRECTION-007.
human-vs-ai: The user made each business call: both code and text required, history over fields, derive the closed fields, liability as a confirmation. Claude searched the documents, explained the gaps and recorded every deviation.
refs: CORRECTION-007

## PROMPT-005 | p1 domain | 2026-10-11
purpose: get a full-scope domain design with EF Core and validation pitfalls checked
prompt: asked the design skill to read the scratch model and all documents, check EF Core and Application pitfalls, and explain validation on states and entities, domain errors and events
outcome: kept
note: produced the layered validation model, the error family, two-phase events and the EF pitfall list; many follow-up questions then refined it

## ASSESSMENT-003 | p1 domain | 2026-10-11
area: web research for EF Core and domain behavior
helped: medium
note: useful leads but mostly secondary sources; two key EF behaviors could not be confirmed and were flagged for tests in the first packs

## CORRECTION-008 | p1 domain | 2026-10-11
what: Claude illustrated a reserve flow where the frontend subtracts the balance from a target and sends the delta, which the API table and UI section never describe
caught-by: user
fix: user asked whether it matched the API table; the flow was withdrawn and the amount defined as the signed delta typed by the user, as the Add Reserve panel shows
lesson: do not present a convenient flow as intended behavior; check each step against the API table and the UI section first

## CORRECTION-009 | p1 domain | 2026-10-11
what: Claude put the marker-to-record registry in Domain, described it as both a check and a lookup, and said migrations needed a design-time factory
caught-by: user
fix: Domain holds only markers and records; the factory and the startup check live in Application, converters in Persistence; the design-time factory was dropped
lesson: ask which layer needs each lookup before placing a helper, and describe what a helper does once, not two ways

## SUMMARY-008 | p1 domain | 2026-10-11
topic: the reserve model
story: Reserves were reread table by table. A claim has many reserve components, duplicates of a kind allowed, each with an append-only history of rows; the spec table is the reserve history and transaction is only prose. Each row has its own approval status, which decides whether it counts toward the balance, queues a posting job and blocks closing. A component balance is the sum of its approved deltas, cached by recomputation, and the claim total is a query. The request amount is the signed delta typed by the user, and the balance may not go below zero except for the recoverable kind.
decisions: The ledger row is named after its table. Pending rows get their balances when they enter the ledger and the non-negative rule is rechecked at approval. No per-transaction cap: 10 million is the claim total with a manager override flag. Component status is informational. The transaction type is its own extendable type with no behavior. Reserve operations and approval and posting transitions run on the shared engine, with builders only for submit and approve.
ai-errors: Claude first described approval as acting on the component and then illustrated a flow where the frontend computes the difference to a target amount, which the documents never describe. The user caught both by asking what the approval status belongs to and whether the flow matched the API table. Logged as CORRECTION-008.
human-vs-ai: The user reread the tables and re-derived the model (instances, rows, status ownership, amount rules); Claude searched the documents, explained gaps with worked examples, argued options, and recorded each decision in the design note.
refs: CORRECTION-008

## SUMMARY-009 | p1 domain | 2026-10-11
topic: closed sets, records versus enums, and the factory
story: The user wanted domain pieces as records with enums only as flags. Claude first proposed a registry inside Domain; the user ruled out reflection and conversion there, asked how a check-only registry could serve lookups (it could not), whether ASP.NET and EF map enums natively (they do), and whether the factory could be a service. Result: a marker enum plus records only where a value carries behavior, plain enums for labels, one generic closed-set class and a singleton service built by one extension, converters in Persistence, the API untouched.
decisions: Records for claim status, approval and posting status, issue severity and state, user role, party role and type, reserve kind and transaction type; plain enums for claim severity, asset type, peril category, policy status and component status. A shared Marked base record carries the marker. Startup fails on a missing or duplicate marker, and one Application test calls the same extension. No design-time factory unless migrations need it.
ai-errors: Claude placed the helper in Domain, described it as a check and a lookup in the same breath, and overstated a design-time factory for migrations. The user questions exposed each one. Logged as CORRECTION-009.
human-vs-ai: The user set the layering rules and challenged every claim about where lookups happen; Claude explained the options, compared them with FluentValidation, and updated the notes.
refs: CORRECTION-009

## SUMMARY-010 | p1 domain | 2026-10-11
topic: full-scope domain design and the small entities
story: The design skill produced a full-scope Domain design: a shared engine (rule, operation, transition), layered validation (invariants throw, rules return domain errors, completeness stored as issues, request shape in Application), a domain error family, two-phase event dispatch, and the EF Core 9 pitfalls (children loaded for rules, client-generated keys, no optional complex types, unmapped state records, converted types in queries, Hangfire final failure). The small entities were then checked: policy period, report dates, estimate, primary flag, documents.
decisions: The policy period is an inclusive date-only record with a new cancellation date column, and the stored policy status stays informational. Report dates are set by the system. The estimate defaults to 0 and is never negative, rejected by request validation and refused by the entity. The primary flag is a plain flag. Roles are modeled in Domain while per-operation permissions are left to Application. Messages use the spec texts under a narrow exception in CLAUDE.md.
ai-errors: No wrong output caught in this part. Two EF behaviors from web results could not be confirmed and were queued for tests in the first packs.
human-vs-ai: The user decided the policy, estimate and role rules and pushed for domain-model clarity; Claude researched, listed gaps and tradeoffs, and recorded every decision and deviation.
refs: PROMPT-005, ASSESSMENT-003

## PROMPT-006 | p1 domain | 2026-10-12
purpose: make policy dates and status a proper domain concept
prompt: asked for a domain type for the policy dates instead of plain times, argued that status follows from the dates and a loss inside the period stays covered after expiry, and asked how cancellation fits
outcome: changed
note: led to the period record, a web check of how periods and occurrence coverage work, and the cancellation date column

## CORRECTION-010 | p1 domain | 2026-10-12
what: Claude proposed that only a Cancelled policy status raises a warning, which would warn on valid claims because no cancellation date is stored
caught-by: user
fix: user reasoned that statuses follow from dates; a cancellation date column, derived statuses and a coverage window checked against the loss date replaced the status rule
lesson: check a stored status against the date it implies; a flag without its date cannot be evaluated against a loss date

## CORRECTION-011 | p1 domain | 2026-10-12
what: Claude modeled the ledger row as a ReserveTransaction entity although the spec has only the reserve history table
caught-by: user
fix: user pointed out there is no transactions table; the entity is named after its table, ReserveHistory, and transaction stays prose
lesson: name entities after the spec tables first; prose words in the spec are not entity names

## CORRECTION-012 | p1 domain | 2026-10-12
what: Claude described a reserve component as one bucket per cost kind and treated a second component of the same kind as unsupported
caught-by: user
fix: user reread the tables: no uniqueness exists and the posting key uses the component id with a per-component sequence; duplicates of a kind are allowed
lesson: do not add uniqueness the tables do not state; check keys and constraints before describing cardinality

## SUMMARY-011 | p1 domain | 2026-10-12
topic: entities and schema departures from the spec
story: Reading the tables against the rules showed the spec entity list is incomplete or inconsistent in places, so the model departs from it deliberately. Additions: a validation issue child and a status-change history child on the claim, a limit-override flag on the claim, a cancellation date on the policy. Changes: loss date and cause code tolerate a Draft, the ledger row is named after its table, ids are generated in the domain, the reserve current amount is a recomputed cache. Removed: the claim closed date and closure reason, now derived from the history.
decisions: Every departure has its reason in the spec itself: Draft means incomplete, closing and reopening need recorded reasons the table cannot hold, the cap needs a flag the claim lacks, a cancelled policy needs a date. Everything else follows the spec tables: decimals with precision 19,4, UTC offset dates, soft delete, audit, tenant and row version columns, closed sets stored as text.
ai-errors: Claude assumed one component per kind and named the ledger row a transaction entity; the user corrected both from the tables. Logged as CORRECTION-011 and CORRECTION-012.
human-vs-ai: The user re-derived the model from the tables and decided each departure; Claude listed the departures, weighed options, and wrote them into the design note and the architecture doc.
refs: CORRECTION-011, CORRECTION-012, SUMMARY-005, SUMMARY-007, SUMMARY-008

## SUMMARY-012 | p1 domain | 2026-10-12
topic: policy period, expiry and cancellation
story: The policy table stores a status (Active, Expired, Cancelled) and two dates but no cancellation date. Claude first proposed that status drive a warning, then narrowed it to Cancelled only. The user reasoned that status follows from the dates and that what matters is whether the loss date falls in the period, so a policy expired at report time can still cover an earlier loss. Web searches confirmed that occurrence policies cover losses inside the period even if reported later, and that real periods run to 12:01 a.m. at the insured address, which the spec seed dates do not follow.
decisions: The period is a date-only record with inclusive ends, compared on the calendar date of the loss date in its own offset. A nullable cancellation date column is added, the coverage window ends at the earlier of expiry and cancellation, and a loss outside it raises the existing period warning. The stored status stays informational and no rule reads it.
ai-errors: Claude proposed a Cancelled warning that would fire on valid claims because the cancellation date was unknown; the user reasoning about derived statuses exposed it. Logged as CORRECTION-010.
human-vs-ai: The user supplied the model of dates and status and asked for a domain type; Claude searched the web, argued three options for recording a cancellation, and recorded the decision in the note and the architecture doc.
refs: PROMPT-006, CORRECTION-010

## CORRECTION-013 | p1 domain | 2026-10-12
what: Claude's first engine had weak or duplicate pieces: a Unit type, DomainOutcome, a Gate hook, Change plus Apply on transitions, a CommonRules class and an Errors folder
caught-by: user
fix: user questioned each piece; renamed to Success, OperationOutcome and CheckPreconditions, merged Change and Apply, removed CommonRules, moved errors to Common, split Common and the test support by role
lesson: ask what each abstraction buys and keep one path per concern; names and folders should follow the role, not the first draft

## CORRECTION-014 | p1 domain | 2026-10-12
what: Claude put ClaimParty in the Claims root but its role and type in a Parties folder, and the one-type-per-file rule left each closed set as 8 files of 3 lines
caught-by: user
fix: closed sets share one file named after the base type, with a general exception in CLAUDE.md; Claims reorganised into Parties, RiskObjects and Loss folders
lesson: check a new folder against its siblings before creating it; when a rule produces noise, change the rule once instead of adding exceptions per case

## CORRECTION-015 | p1 domain | 2026-10-12
what: Claude recommended keeping a one-field OperationContext for the time, although most rules ignored it and Claim.Create already took the time as an argument
caught-by: user
fix: user proposed putting the time in the input, stamped by the Application input builders; Claude added the OperationInput base with a default-time guard and removed the context and the time providers
lesson: a recommended option can lose to a simpler one; check a "uniform signature" argument against how many rules actually use the parameter

## SPLIT-003 | p1 domain | 2026-10-12
item: claim core pack: states, parties, risk objects, loss, eight operations and tests
who: hybrid
note: user fixed scope (no policy or reserves, static factory, Kind suffix) and later the layout; Claude wrote the types, operations and tests and kept build and tests green

## ASSESSMENT-004 | p1 domain | 2026-10-12
area: structural refactors (moving files, namespaces, engine signature change)
helped: high
note: mechanical moves and the engine signature change ran fast with the build and tests as the check; the shape of each change came from the user's questions

## SUMMARY-013 | p1 domain | 2026-10-12
topic: engine refinement and claim core layout
story: The claim core pack was built on the pack 1 engine. Reviewing it led to three reshapes: closed sets in one file per registry, claim folders per owned concept, and operation time moved from a context into the input. Each change was applied across source, tests and the architecture doc.
decisions: The user chose the registry-file exception, the per-concept folders and the time in the input, stamped by builders. Claude argued the options and recommended differently on the layout and on the time.
ai-errors: Claude kept weak engine names, an inconsistent folder split and a context the rules did not use; the user caught all three. Logged as CORRECTION-013 to CORRECTION-015.
human-vs-ai: The user drove the structure through questions; Claude implemented, ran the build and tests, and updated CLAUDE.md and the docs.
refs: CORRECTION-013, CORRECTION-014, CORRECTION-015, SPLIT-003, ASSESSMENT-004

## CORRECTION-016 | p1 domain | 2026-10-14
what: a first rename merged check and rule into one word (ICheck to IRule, new IDomainRule); it was too wide and blurred the engine layering
caught-by: user
fix: reverted; issue checks moved to Checks/ folders, rules stay in Rules/, so each word has one meaning
lesson: confirm the scope of a rename at concept boundaries before running it; a mechanism and a business term are not the same word

## CORRECTION-017 | p1 domain | 2026-10-14
what: transition left overridable (stop decision, rule selection, move hooks, refusal code); an undeclared source could throw or skip issues and event
caught-by: code-review
fix: transition sealed into a locked flow, equatable states, claim transition base raises the status event, unused refusal-code hook removed
lesson: a configured construct should seal everything it does not configure; re-review after every design change

## CORRECTION-018 | p1 domain | 2026-10-14
what: a waived issue later fixed kept the waiver's actor; waive and clear raised no events; removing an unknown party or risk object threw
caught-by: code-review
fix: clear records the actor, events added, existence rules IsPartyActive and IsRiskObjectPresent, tests for each
lesson: guards belong in rules and significant changes raise events; ask what the second call or an unknown id does

## ASSESSMENT-005 | p1 domain | 2026-10-14
area: fresh-context code review as a design driver
helped: high
note: each round found real defects the writing session missed, but it also raised style items that needed a rule decision first

## SUMMARY-014 | p1 domain | 2026-10-14
topic: domain engine hardening and the standards pass
story: the engine was layered into checks, flows, rules, operations and transitions; transitions became locked flows with a claim base for state and events; issues gained events and existence rules; the style standard and test form were rewritten and enforced by the workflow check
decisions: the user chose check versus rule folders, the transition base, the event on the base, optional Apply, removing the refusal code, var on member reads, the test form and shorter docs
ai-errors: over-wide rename, an overridable transition, missing events and guards, helper scripts that corrupted files, docs that grew into inventories; caught by the user, review or the build
human-vs-ai: the user set every structure and name choice; the AI implemented, verified with build and tests, and ran the review loop
refs: CORRECTION-016, CORRECTION-017, CORRECTION-018, ASSESSMENT-005
