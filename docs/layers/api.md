# API layer

Rules and contracts of the API: controllers, the error body, identity and the Idempotency-Key filter. It supplies the clock, the current user and the correlation id the Application expects (`docs/layers/application.md` 'Provides and expects'). `.claude/CLAUDE.md` holds the short rules and points here; read this file before changing the API project or reviewing its code. Status: not built; this file holds what the Application hands over, and grows into standards as the layer is written.

## Controllers and routes

- A controller action maps the request to one command or query, sends it through MediatR and returns the mapped `Result`. It holds no business logic and builds no error JSON by hand.
- Routes use plural nouns in kebab-case, under `/api`. The reserve decision routes are the specification's, which name a history row. Two assessment routes are deliberately not built (a reserve update and a combined decision route).
- Reads reuse the composed queries; every list is paged and the page size is capped by the Application.

## Errors

- One error mapper turns a `Result` into the response: validation 422, not found 404, forbidden 403, conflict 409.
- A `ConcurrencyConflictException` from the Unit of Work (a concurrent edit or a duplicate key) is caught by middleware and returned as a 409 with the same body.
- The body follows the specification's sample: `type`, `title`, `status` and `errors` grouped by field. An error without a field goes under the key `General` (S:API-ErrorShape). The error code is not on the wire.
- Only a request that cannot be bound at all is rejected before a command runs, and that rejection uses the same 422 body, not an own 400 body (a missing non-nullable value included).
- The warnings of a successful `Result` are returned with the success body (the shape is decided when the first endpoint is written); the Application never returns HTTP types.

## Identity and correlation

- The actor and the role come from the authenticated user, never from the request. The role from the token becomes a `UserRole` with `UserRole.From`. A dev token endpoint behind `DevAuth:Enabled` is parked in `.claude/temp/notes-parking-lot.md`.
- Persistence builds `ClaimsDbContext` and its save interceptors from the scoped `ICurrentUser` and `IClock`, so the API must register both (and `ICorrelationContext`) before any request reaches a handler. The organisation of the user is the tenant filter value; the dev users carry the seeded organisation id, or the seeded policies and cause-of-loss codes are invisible to them (`docs/layers/persistence.md` 'Migrations and seed data').
- Middleware sets the correlation id that `ICorrelationContext` exposes, so every audit entry of one request carries it (S:AUD-Correlation).

## Idempotency-Key

A write operation with an `Idempotency-Key` header is idempotent (S:API-IdempotencyKey). It is an action filter, not a MediatR behavior, because only the API knows the header, the route and the final status and body.

1. Before the action the filter hashes the body and calls `IIdempotencyStore.BeginAsync` with the key, method, route and hash.
2. `Started`: run the action. `Completed`: return the stored response without running it. `InProgress`: return 409. `BodyMismatch` (the same key with another body): return 422.
3. After the action the filter calls `CompleteAsync` with the status code and body.

The store saves the key with the command's own commit, so a key with no stored response means the request is still running or crashed after its commit: it answers 409 and the action is never run again until the key expires. Completion (the response) is its own save after the action.

The header is optional and the filter applies to every write action; a request without it runs normally and is not stored. A replay returns the first response even if the claim has changed since; fresh state comes from a read. A stored key expires after about 24 hours and then counts as new.

## Tests

- A route test checks that every route is kebab-case plural (S:DATA-Naming).
- The error mapper has a test per error kind and for the `General` key.
