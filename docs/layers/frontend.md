# Frontend layer

Rules and contracts of the Angular client in `frontend/`. It talks only to the API described in `docs/layers/api.md`, through one typed service layer. `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the frontend or reviewing its code. Status: bootstrapped (workspace, theme, app shell, sign-in, HTTP pipeline); the three screens are built next, one pack each. The static mockups in `frontend/design/` are the visual reference and are not part of the build.

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

## Services and HTTP

- Only services in `core/api/` call `HttpClient`, one service per resource (`claims.service.ts`, `reserves.service.ts`, ...). A service returns typed Observables, builds every URL with `apiUrl(path)` and unwraps the `WriteResult` envelope where the caller only needs the data.
- Interceptors, in this order: `loadingInterceptor` (the global progress bar), `errorInterceptor` (a snackbar for every failure except 422; a body that is not the API's error body is reported by its status code) and `authInterceptor` (adds the bearer token to API calls, never to the dev token call; on a 401 it waits for one shared re-sign-in, `AuthService.refresh()`, however many requests failed, and repeats the request).
- Idempotency: a write that the user may repeat (create a claim, submit a reserve, upload) gets a key from `newIdempotencyKey()` when the user starts the action and sends it with `idempotencyHeaders(key)`. A retry of the same action reuses the key; the API stores only successful responses, so retrying a refused request with the same key is safe. Keys are never generated inside an interceptor.

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

- Dates and money go through pipes only: `claimDate` (one date and one date-time format), `money` (signed, two decimals, USD), `enumLabel` (`PendingPayment` becomes "Pending Payment"). Times are shown in the browser's time zone; the API receives ISO strings with the offset.
- Policy dates arrive as `yyyy-MM-dd` and are compared as calendar dates, the way the backend compares the UTC date of the loss.

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
