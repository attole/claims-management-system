# Frontend layer

Rules and contracts of the Angular client in `frontend/`: three screens over the API in `docs/layers/api.md`, every call through one typed service layer. `.claude/CLAUDE.md` points here; read this file before changing or reviewing the frontend. The stack, naming, component conventions, styles and test shapes are in `docs/FRONTEND-STANDARDS.md`. The mockups in `frontend/design/` are the visual reference and are not part of the build.

## Layout

```text
frontend/
    design/           static mockups
    openapi.json      the API's OpenAPI document, input of api:types
    public/           copied to the build root: favicon, staticwebapp.config.json
    src/
        environments/ apiBaseUrl per build
        styles/       theme colours, tokens, badges, banners, buttons
        app/
            core/     singletons: api/, auth/, dates/, http/, layout/, notify/, theme/
            shared/   stateless building blocks: forms/, pipes/, ui/, util/, validators/
            features/ one folder per screen: claims-list/, claim-intake/, claim-detail/
```

- `core/` holds what exists once (root services, interceptors, the app header); `shared/` holds what two features reuse and keeps no state. A feature never imports another feature.
- Each feature has a route file (`<feature>.routes.ts`, a default export) loaded lazily from `app.routes.ts`; lazy route files are the feature modules (no NgModules). `claims/new` is declared before `claims/:claimId`.

## State

- One store per page (`*.store.ts`), provided on the page. It holds writable signals for its inputs, `computed` values for derived state and one `rxResource` per read keyed by those signals.
- A resource throws on `value()` while in error and resets to its default when its params change. A store therefore copies each resolved value into a last-value signal with an effect and exposes `computed` values over it, with `hasError`, `hasLoaded` and `isFirstLoad`. A new filter, page or a failed reload keeps the shown data.
- Writes go through the store: it calls a service, then reloads the affected resources. `isSaving` and `isUploading` are counters, so parallel writes and a long upload do not clear each other.
- Root state is the signed-in user, the theme and the loading counter.

## Services and HTTP

- Only the services in `core/api/` call `HttpClient`, one per resource: `ClaimsService`, `PartiesService`, `RiskObjectsService`, `IssuesService`, `ReservesService`, `DocumentsService`, `PoliciesService`, `ReferenceService`. They return typed Observables and build URLs with `apiUrl(path)`.
- A write returns the whole `WriteResult`, because the screens show its warnings; a read returns the data or the `PagedResult` as sent. An upload returns `UploadProgress` events (a percent, then the result). `ReferenceService` caches the cause codes and the status table for the session and drops a failed load.
- The list filter is a `ClaimsFilter`: empty values stay out of the query string and each status repeats as `status`.
- Interceptors, in this order: `loadingInterceptor` (the progress bar), `errorInterceptor` (a snackbar for every failure except 422; a body that is not the API's error body is named by its status code), `authInterceptor` (the bearer token on API calls except the dev token call; on a 401 one shared `AuthService.refresh()` and a retry).
- A 422 is a refusal (`isRefusal` in `core/http/error-body.ts`). `applyServerErrors(form, body)` sets a `server` error on each control its key names (dotted and indexed paths, any casing) and returns the other messages, `General` included, for a banner.
- Idempotency (S:API-IdempotencyKey): every write method takes an optional key and sends it with `idempotencyHeaders(key)`. A user action creates its key when it starts and reuses it for every retry; the key is kept after a failure without an answer and replaced after a refusal or a success. The claim detail store keeps one key per action (per row for row actions, with setting the primary apart from removing the same risk object); an upload's key belongs to the file (name, size, last change). Keys are never made in an interceptor.

## Sign-in

- `AuthService.signInAs(user)` posts `{ userId }` to `/api/dev/token` and keeps the token in memory and the user id in sessionStorage. The user (id, name, role) comes from the token response; a sign-in that finishes after a newer one is ignored. At startup the app signs in as the stored user, or the handler; the header's role switcher signs in as another user.
- The users in `core/auth/dev-users.ts` equal `DevAuth:Users` of the development settings; `userName(id)` names them on screen, with "System" for the empty id.

## API types

- `core/api/api-schema.ts` is generated from `frontend/openapi.json` by `npm --prefix frontend run api:types` (openapi-typescript through `npx`) and never edited; lint, format and the workflow check skip it.
- `core/api/api.model.ts` is the only importer of the generated types. It names them for the client, narrows every status and kind string to a union (claim status and severity, issue state and severity, party role and type, reserve approval, posting, component state and type, transaction type, policy status) and drops computed request members. Nested types are taken from the property that holds them (`ClaimDetail['claim']`), because the generator emits suffixed duplicates for nullable references.
- A contract change: save the API's `/openapi/v1.json` over `frontend/openapi.json`, run `api:types` and build; a compile error marks what to adapt.

## Forms and validation

- The client checks shape only: required fields, the backend field lengths (`FIELD_LENGTHS` in `core/api/api-limits.ts`), email, a loss date not in the future, a description of at least 20 characters, a non-zero amount, at least one claimant, a name for a party of its type. Validators are in `shared/validators/`; every message comes from the `errorMessage` pipe.
- Business rules are the server's, and the server checks every write. The UI also gates an action when the data it already holds decides it: an action that is never the user's (a role, someone else's row) is hidden; an action blocked by the current state (a closed or withdrawn claim, the last claimant, no linked policy) is disabled with a short reason. A rule that needs data the client does not hold (the claim limit, balances, the closure checks) stays a server refusal shown in the banner.
- The gates live in `shared/util/`: `action-gates.ts` (a frozen claim, Acknowledge on raised warnings, Waive on waivable critical issues for a supervisor or manager, Reopened for a supervisor or manager, the last active claimant) and `reserve-authority.ts` (the approval tier and `canDecideReserve`; Approve is also hidden on the user's own reserve, which they may still reject or retract). Each gate mirrors a backend rule, carries its rule ID in its spec, and changes with it.
- Hints: the in-force badge (the loss day against the policy start and its effective end, the cancellation when earlier), the policy's coverage types, the approval tier, the closure checklist from `status-check`, and the upload type and size check (`ALLOWED_UPLOAD_TYPES`, `MAX_UPLOAD_BYTES`). These constants mirror the backend and change with it.
- A form clears what it hides: the party editor keeps only the name fields of the chosen type and trims every value before it emits.
- Shared form pieces: `shared/forms/party-form.ts` (the party group, roles, types, `partyName`, `trimmedParty`), `shared/forms/loss-form.ts` (the loss group with its validators, `toLossRequest`, `lossValueOf` for a saved loss), `shared/forms/risk-object-form.ts` (the risk object group with `isPrimary`, asset types, `trimmedRiskObject`, `withPrimary`) and `shared/forms/reserve-form.ts` (component and transaction lists). `shared/ui/` holds the inline `party-editor` and `risk-object-editor`, the `cause-field` autocomplete and the `policy-picker` search, each used by the intake and the claim detail; `shared/util/in-force.ts` gives the in-force badge.
- `PUT /loss` replaces the whole loss, so the loss editor fills every field from the saved loss (the police report number and an unset estimate included) and sends them all back.

## Formats

- `core/api/api-dates.ts` is the only place that turns a `Date` into an API value: a timestamp is an `IsoDateTime` in UTC (`toIsoDateTime`), a calendar date is a `CalendarDate` `yyyy-MM-dd` (`toCalendarDate`); an empty filter is `NO_DATE`.
- Patterns are constants there (`CALENDAR_DATE_FORMAT`, `TIME_FORMAT`, `DATE_TIME_DISPLAY_FORMAT`); `provideAppDateAdapter()` in `core/dates/` gives the Material date and time pickers the same formats.
- The in-force badge compares `calendarDateOf(lossDate)`, the UTC day of the loss, with the policy dates, as the backend does.
- Display goes through pipes only: `claimDate` (`'day'` the UTC day, `'dateTime'` local date and time), `money` (USD, two decimals), `enumLabel` (`PendingPayment` reads "Pending Payment"), `fileSize`.

## Screens

- **Claims list** (`features/claims-list/`, S:UI-List, S:UI-Badges): `ClaimsListStore` keeps the `ClaimsFilter` in a signal and in the query string (`replaceUrl`), so a reload or a link shows the same list; values read from the URL are checked (statuses, `yyyy-MM-dd` dates, GUIDs, page sizes) and a page past the end moves to the last page. `ClaimsFilterBar` emits a valid, changed filter after 300 ms. The handler filter is a select of the known users, because the API filters by handler id. A row opens the claim detail.
- **Intake** (`features/claim-intake/`, S:UI-Intake-Steps, S:UI-Intake-Policy, S:UI-Intake-Parties, S:UI-Intake-Reserve, S:UI-Intake-Submit): one typed form (`intake-form.ts`) drives a linear three-step `mat-stepper`; Next advances only when the step is valid.
    - Policy and loss: a policy search (two characters, 300 ms), the picked policy with its in-force badge or the Unknown policy switch; loss date and time on one control; the cause of loss matched against the reference list.
    - Parties and risk objects: inline forms and cards; a missing claimant blocks creation, a missing risk object is advised; one risk object may be marked primary.
    - Reserve and review: an optional initial reserve with the approval tier, which needs a linked policy; the review of every value.
    - Create: warnings are confirmed in a dialog first; a success opens the claim with its number in a snackbar; a 422 marks the fields, lists the rest and returns to the first invalid step.
- **Claim detail** (`features/claim-detail/`, S:UI-Detail-Header, S:UI-Detail-Tabs, S:UI-Reserves, S:UI-Documents, S:UI-Audit, A:UI-Reserves-Roles): `ClaimDetailStore` loads the claim, then its audit page; an id that is not a GUID is the error state.
    - Header: number with a copy button, status badge, policy (Not linked with a Link policy button while none is linked; the dialog searches, shows the in-force badge for the loss date and warns before linking a policy outside it), client, loss day, cause, handler, and a Change status menu with the allowed next statuses (Reopened only for a supervisor or manager). A closed or withdrawn claim shows a notice that only its notes can change, and its edit actions are disabled. A refusal shows in a banner under the header until the next write or a tab change. Changing status asks `status-check` and shows its findings in a dialog: a finding on another field blocks, a finding on `Reason`, `Note` or `IsLiabilityConfirmed` makes that input required, warnings are notes; the server decides again on submit.
    - Overview: loss event with Edit (the loss editor in place), severity, notes (saved with a button and kept through reloads while edited), open validation issues with Acknowledge and Waive, the handler panel (the assigned handler, a select of the known users and Assign; any user may assign, as the backend has no role rule; the button is off until another handler is picked, and the action is keyed per target handler), risk objects with Add, Make or Unset primary (Make is off while another is primary) and Remove after confirmation.
    - Parties: cards with their state, Remove after a confirmation (disabled for the last active claimant), the shared party editor.
    - Reserves: a balance card per component with its pending sum, the approved total, the history newest first by submit time, and an inline editor that adjusts an open component or opens a new type with an Add. Approve and Reject show on pending rows to a manager, and to a supervisor up to the supervisor limit, with Approve hidden on the user's own row (Reject asks for a reason), Retract to the submitter, Retry posting on a failed posting. Add reserve is disabled until a policy is linked.
    - Documents: the list with a Download link opening in a new tab, and an upload with progress (indeterminate until the size is known).
    - Audit Log: paged, read-only, with user names.
    - A refusal lists its messages in a banner under the header; other failures use the snackbar.

## Provides and expects

- **Provides:** the three screens of the specification (S:UI-List, S:UI-Intake-Steps, S:UI-Detail-Tabs) and the mock sign-in with the role switcher (S:UI-General).
- **Provides to the deployment** (`docs/layers/deployment.md`): a production build in `dist/claims-frontend/browser`; `src/environments/environment.ts` holding the literal line `apiBaseUrl: '/api'`, which the pipeline replaces with the API's URL; `public/staticwebapp.config.json`, which rewrites every route to `index.html`.
- **Expects from the API:** the routes, bodies and error body of `docs/layers/api.md`; enums as names; the `{ data, warnings }` envelope on writes; the Idempotency-Key filter; `status-check` findings with their field; the dev token route; CORS for `http://localhost:4200` and the deployed origin.
