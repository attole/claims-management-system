# Frontend layer

Rules and contracts of the Angular client in `frontend/`. It talks only to the API described in `docs/layers/api.md`, through one typed service layer. `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the frontend or reviewing its code. Status: bootstrapped (workspace, theme, app shell, sign-in, HTTP pipeline) with the typed API services and the claims list screen; the intake form and the claim detail screen follow, one pack each. The static mockups in `frontend/design/` are the visual reference and are not part of the build.

## Stack

- Angular 22 with standalone components, signals, zoneless change detection and the built-in control flow (`@if`, `@for` with `track`). Angular Material 22 (M3) for every control. Reactive Forms, typed and non-nullable, for every form (the specification asks for Reactive Forms; signal forms are not used).
- TypeScript strict with strict templates. ESLint (`angular-eslint`, `typescript-eslint`) and Prettier at 120 columns. Unit tests on Vitest through the Angular test builder.
- Commands: `npm --prefix frontend run build`, `npm --prefix frontend test`, `npm --prefix frontend run lint`, `npm --prefix frontend run format:check`, `npm --prefix frontend start` (dev server on 4200).

## Layout

```text
frontend/
    design/           static mockups, reference only
    openapi.json      snapshot of the API's OpenAPI document, input of the type generation
    src/
        environments/ apiBaseUrl per build
        styles/       Material theme, design tokens, badges
        app/
            core/     singletons: api/, auth/, http/, notify/, theme/, layout/
            shared/   stateless building blocks: ui/, pipes/, validators/, util/
            features/ one folder per screen: claims-list/, claim-intake/, claim-detail/
```

- `core/` holds what exists once (services provided in root, interceptors, the app header). `shared/` holds what is reused and keeps no state. A feature never imports another feature; what two features share moves to `shared/`.
- Each feature has a route file (`<feature>.routes.ts`, a default export) loaded lazily from `app.routes.ts`. Lazy route files stand in for the specification's lazy-loaded feature modules: there are no NgModules.

## Naming

- File names are kebab-case words; a single dot separates a role word: `claims-list.page.ts`, `claims-list.store.ts`, `claims.service.ts`, `enum-label.pipe.ts`, `auth.interceptor.ts`, `api.model.ts`, `claims-list.routes.ts`. A plain component has no role word (`app-header.ts`, class `AppHeader`), following the current Angular style guide.
- Class names say the role the same way: `ClaimsListPage`, `ClaimsListStore`, `ClaimsService`, `EnumLabelPipe`. Functional interceptors and guards are camelCase constants (`authInterceptor`).
- Selectors use the `app-` prefix. A boolean is `isX`, `hasX` or `canX`, as in the backend.
- Frontend type names describe the payload from the client's side: `WriteResult<T>` (the `{ data, warnings }` envelope of a write), `Warning`, `ErrorBody`, `PagedResult<T>`. Backend type names that only make sense on the server (`CommandResponse`) are not used in the client.

## Components

- Standalone, `ChangeDetectionStrategy.OnPush`, `templateUrl` and `styleUrl` in sibling files. Inputs and outputs are `input()`, `input.required()`, `output()` and `model()`; queries are `viewChild()`. Dependencies come from `inject()`.
- Zoneless means a template only updates from signals: component state is a signal or a `computed`, never a plain field changed in a callback. Templates do not subscribe and do not use the `async` pipe.
- A page component (`*.page.ts`) is routed, provides its store and composes presentational components. A presentational component gets data through inputs and reports through outputs; it injects no service.
- Route parameters bind to inputs (`withComponentInputBinding`), for example `claimId = input.required<string>()` on the detail page.

## State

- One store per page (`*.store.ts`), provided on the page component, not in root. It holds writable signals for its inputs (filters, page, claim id), `computed` values for derived state and one resource per read keyed by those signals; a mutation calls a service and reloads the affected resource.
- Root state is limited to the signed-in user, the theme and the loading counter.

- A resource read through `value()` throws while the resource is in error, and goes back to its default value whenever its params change. So a store never exposes `value()` directly: an effect copies each resolved value into a signal of the last resolved page, and the store exposes `computed` values over that signal plus `hasError`, `hasLoaded` and `isFirstLoad` for the template. A new filter or page keeps the shown rows (dimmed) until the answer arrives.

## Screens

- **Claims list** (`features/claims-list/`, S:UI-List, S:UI-Badges). `ClaimsListStore` holds the `ClaimsFilter` in a signal, reads the page with `rxResource` keyed by it and writes the filter to the query string (`replaceUrl`), so a reload or a shared link shows the same list; it reads the query string once, at start. `ClaimsFilterBar` is a presentational form (status chips, loss date range with an order check, cause of loss, handler, search) that emits one `changed` after a 300 ms debounce and only while valid. The page shows the count, a spinner on the first load, an error state with a retry, an empty state with a clear button, the table (cause names from the reference list, status badges, money) and the paginator; a row (click or Enter) opens the claim detail.
- The handler filter is a select of the known users, not a text search: the API filters by handler id and has no handler name to search (a decision in `docs/ARCHITECTURE.md` 'Decisions on gaps'). The list has no handler column, because the summary rows carry no handler. Search matches the claim number and the client name, as the API does.
- Values read from the URL are checked before they reach the API: statuses against the known set, dates against `yyyy-MM-dd`, handler and policy ids against the GUID pattern (`core/api/api-ids.ts`), paging against the allowed sizes; anything else is dropped. A page past the end (an old link) moves to the last page.
- The filter bar emits a change only when it differs from the current filter, and writes the filter back into the form only for fields whose value differs, so text being typed (a trailing space, a half-typed date) is never overwritten.

## Services and HTTP

- Only services in `core/api/` call `HttpClient`, one service per resource: `ClaimsService` (list, detail, create, status change and check, notes, handler, policy, loss, audit), `PartiesService`, `RiskObjectsService`, `IssuesService`, `ReservesService`, `DocumentsService`, `PoliciesService` and `ReferenceService`. A service returns typed Observables and builds every URL with `apiUrl(path)`. A write returns the whole `WriteResult` (data and warnings), because the screens show the warnings; a read returns the data or the `PagedResult` as sent.
- The list filter is a `ClaimsFilter`; empty values are left out of the query string and every selected status repeats as `status`. An upload returns `UploadProgress` events (a percent, then the result). `ReferenceService` caches the cause codes and the status table for the session, and drops a failed load so the next call tries again.
- A 422 is mapped onto a form by `applyServerErrors(form, body)` in `shared/validators/server-errors.ts`: it sets a `server` error on each control its key names (dotted and indexed paths, any casing) and returns the messages that matched no control, `General` included, for the banner.
- Interceptors, in this order: `loadingInterceptor` (the global progress bar), `errorInterceptor` (a snackbar for every failure except 422; a body that is not the API's error body is reported by its status code) and `authInterceptor` (adds the bearer token to API calls, never to the dev token call; on a 401 it waits for one shared re-sign-in, `AuthService.refresh()`, however many requests failed, and repeats the request).
- Idempotency: every write method takes an optional key and sends it with `idempotencyHeaders(key)`; without one it creates a fresh key for that call. A write that the user may repeat (create a claim, submit a reserve, upload) creates its key with `newIdempotencyKey()` when the user starts the action and passes it on every retry of that action; the API stores only successful responses, so retrying a refused request with the same key is safe. Keys are never generated inside an interceptor.

## Sign-in

- The mock sign-in uses the API's dev token route: `AuthService.signInAs(user)` posts `{ userId }` to `/api/dev/token` and keeps the token in memory and the user id in sessionStorage. The signed-in user (id, name, role) is taken from the token response, never from the local list, and a sign-in that finishes after a newer one is ignored, so a late 401 retry cannot switch the user back. At startup the app signs in as the stored user, or the handler. The header's role switcher changes the user.
- The three users in `core/auth/dev-users.ts` must equal `DevAuth:Users` in the API's development settings, or sign-in answers 404.

## API types

- `src/app/core/api/api-schema.ts` is generated from `frontend/openapi.json` with `npm --prefix frontend run api:types` (openapi-typescript, run through `npx` because it pins TypeScript 5 while the workspace uses 6). It is never edited by hand and is excluded from lint, format and the workflow check.
- `core/api/api.model.ts` is the only file that imports the generated types. It gives them client names, narrows status strings to unions (`ClaimSummary.status` becomes `ClaimStatus`) and drops computed request members (`isNewComponent`). Nested types are taken from the property that holds them (`ClaimDetail['claim']`), because the .NET 9 generator emits duplicate schemas with a `2` suffix for nullable references.
- Refreshing the snapshot: run the API with Swagger enabled, save `/openapi/v1.json` over `frontend/openapi.json`, run `api:types`, then build; a compile error is a contract change to handle.

## Forms and validation

- The client validates shape: required fields, maximum lengths equal to the backend field lengths, email format, a loss date not in the future, a description of at least 20 characters, a non-zero amount and at least one claimant before a claim is created. Messages for these live in one map rendered by `shared/ui/form-errors`.
- Business rules stay on the server. The UI shows hints only (the in-force badge, the authority indicator from the mirrored thresholds in `shared/util/reserve-authority.ts`, the closure checklist from `status-check`) and maps a 422 back onto the form: the error keys match the control paths case-insensitively (`Loss.Description`), and keys that match nothing or `General` go to a banner at the top of the step.

## Formats

- One date convention on the wire, kept in `core/api/api-dates.ts`, the only place that turns a `Date` into an API value:
  - a timestamp is an `IsoDateTime`, always sent in UTC (`toIsoDateTime`, for example `2026-10-21T08:30:00.000Z`): loss date, report date;
  - a calendar date is a `CalendarDate`, `yyyy-MM-dd` (`toCalendarDate`), for values the API binds as dates only: the claim list date filters and the policy period. An empty filter is `NO_DATE` (`''`) and is left out of the query.
  - The in-force badge compares `calendarDateOf(lossDate)` (the UTC day of the loss) with the policy dates, as the backend does.
- Date work uses date-fns (with `@date-fns/utc` for UTC days), never hand-built strings; the format patterns live as constants in `api-dates.ts` (`CALENDAR_DATE_FORMAT`, `DATE_TIME_DISPLAY_FORMAT`). Material date pickers use the date-fns adapter with the same calendar format (`provideAppDateAdapter()` in `core/dates/`), so a picker shows and reads `yyyy-MM-dd` like every other date on screen.
- Dates and money are displayed through pipes only: `claimDate` (`'day'` shows the UTC day of a timestamp, the same day the filters and the backend use; `'dateTime'` shows the date and time in the browser's time zone), `money` (two decimals, USD, a minus sign only below half a cent), `enumLabel` (`PendingPayment` becomes "Pending Payment").
- Every status that crosses the wire is a union in `api.model.ts` (claim, reserve approval and posting, reserve component, policy), never a plain string.

## Styles

- `styles.scss` applies the Material theme with `theme-type: color-scheme`, so one theme serves dark (the default) and light; `ThemeService` switches `data-theme` on `<html>` and remembers the choice. The custom palette in `styles/_theme-colors.scss` is generated by the Material `theme-color` schematic.
- `styles/_tokens.scss` holds the design tokens as CSS variables with `light-dark()` values (surfaces, text, borders, the seven status hues of the specification, gaps). Component styles use these variables and Material system variables (`--mat-sys-*`); a hex colour in a component style is a review finding.
- Badges are the tinted pills of the design: `.badge` with a modifier per status or tone (`badge--pending-payment`, `badge--warn`).
- Material overrides use the official mixins (`mat.button-overrides`, `mat.icon-button-overrides`, `mat.snack-bar-overrides`), never `::ng-deep`.

## Tests

- One spec per source file, next to it (`auth.service.spec.ts` beside `auth.service.ts`); the workflow check enforces the mirror.
- A test name reads as condition and result (`'on a 401 signs in again once and repeats the request with the new token'`), arranged, acted and asserted in that order.
- The rules a spec covers ride on its `describe` name in parentheses, as the backend's `Rule` traits do; the auth interceptor spec, for example, is described as `authInterceptor (S:UI-General, A:UI-General)`. spec-check reads them there.
- Services and interceptors are tested with `HttpTestingController` (URL, method, body, headers, the unwrapped result); validators, pipes and stores as plain classes; components through `TestBed` with their inputs and the rendered DOM. HTTP is never called for real.
