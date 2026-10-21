# Claims Management System

Claims handling for a policy administration system, built end to end: a .NET 9 backend (Clean Architecture, CQRS) and an Angular frontend.

It covers claim intake, the claim lifecycle, parties, reserves with tiered approval and a simulated GL posting, documents in Blob storage, the audit log and background jobs, behind three screens: the claims list, the intake form and the claim detail.

## Documentation

- `docs/ARCHITECTURE.md`: the design overview, the decisions and the reasons.
- `docs/CODING-STANDARDS.md`: code style, naming and tests.
- `docs/layers/`: the rules and contracts of each backend layer, `frontend.md` for the Angular client and `deployment.md` for Azure and the pipelines.
- `docs/AI-WORKFLOW.md`: how AI is used in this project.
- `frontend/design/`: static HTML mockups of the three screens, the visual reference (not part of the build).

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (`MSSQLLocalDB`) or another SQL Server 2022 instance
- Node.js 24 with npm 11 (frontend)

## Build and test

```bash
dotnet build backend
dotnet test backend
python -I .claude/tools/check_workflow.py
```

The tests in `ClaimsModule.Persistence.SqlServerTests` and `ClaimsModule.API.Tests` create a temporary database on LocalDB (or on the server in `CLAIMS_TEST_SQLSERVER`) and are skipped when no server answers. When `CLAIMS_TEST_SQLSERVER` is set and the server does not answer, they fail instead, so CI never skips them silently. The Hangfire tests in `ClaimsModule.Infrastructure.Tests` use the same database and rule.

## Run locally

1. Create and seed the database. The app does not migrate at startup. From the `backend` folder run `dotnet tool restore`, then `dotnet dotnet-ef database update --project src/ClaimsModule.Persistence` (Hangfire creates its own tables on first start). It targets LocalDB; set `CLAIMS_DB_CONNECTION` to use another server.
2. Set the two signing keys (32 or more characters, any test values locally), one for the bearer token and one for the signed local download links:
    - `dotnet user-secrets set "Jwt:SigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
    - `dotnet user-secrets set "LocalSigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
3. Run: `dotnet run --project backend/src/ClaimsModule.API` (the `https` profile serves `https://localhost:54694`, the address `LocalBaseUrl` in `appsettings.Development.json` points at)
4. Get a token for a user: `POST /api/dev/token` with `{ "userId": "<id>" }`, using an id from `DevAuth:Users` in `appsettings.Development.json` (a handler, a supervisor and a manager), and send it as `Authorization: Bearer <token>`. This is a simulation of sign-in, only available while `DevAuth:Enabled` is true; when `DevAuth:Secret` is set, add the header `X-DevAuth-Secret: <secret>`.

With `ASPNETCORE_ENVIRONMENT=Development`, Swagger is at `/swagger` and the Hangfire dashboard at `/hangfire`.

### Frontend

1. Trust the .NET development certificate once, so the browser accepts the API at `https://localhost:54694`: `dotnet dev-certs https --trust`
2. Install the packages: `npm --prefix frontend install`
3. With the API running, start the dev server: `npm --prefix frontend start`, then open `http://localhost:4200`. The app signs in as the handler; the header's role switcher changes the test user. Against an API with `DevAuth:Secret` set, the app asks once for the access code.
4. Checks: `npm --prefix frontend run build`, `npm --prefix frontend test`, `npm --prefix frontend run lint`, `npm --prefix frontend run format:check`
5. After an API contract change, save the API's `/openapi/v1.json` over `frontend/openapi.json` and run `npm --prefix frontend run api:types`.

The screens:

- **Claims list** (`/claims`): filters (status, loss date range, cause, handler, search) kept in the URL, paging, status badges.
- **Log new claim** (`/claims/new`): a three-step form (policy and loss, parties and risk objects, optional initial reserve with the approval tier and a review), with warnings confirmed before creating.
- **Claim detail** (`/claims/{id}`): the header with the status change and its checklist, and the Overview, Parties, Reserves, Documents and Audit Log tabs. Approve and Reject show to a supervisor or manager only.

## API

All routes are under `/api` and need a bearer token, except `POST /api/dev/token`, Swagger and the signed local document download. The routes, with `{id}` for a claim:

| Route | What it does |
|---|---|
| `GET /claims`, `POST /claims` | paged list with filters; create a draft with its children (201) |
| `GET /claims/{id}`, `PUT /claims/{id}/status`, `GET /claims/{id}/status-check?targetStatus=` | detail; change the status; what blocks a status change, without saving |
| `PUT /claims/{id}/notes`, `GET /claims/{id}/audit` | update the notes; paged audit log |
| `PUT /claims/{id}/handler`, `PUT /claims/{id}/policy`, `PUT /claims/{id}/loss` | assign the handler (the creator by default); link a policy; update the loss details |
| `POST /claims/{id}/issues/{issueId}/acknowledge`, `waive` | acknowledge a warning; waive a critical issue (supervisor or manager) |
| `POST /claims/{id}/parties`, `DELETE /claims/{id}/parties/{partyId}` | add and remove a party |
| `POST /claims/{id}/risk-objects`, `DELETE /claims/{id}/risk-objects/{riskObjectId}` | add and remove a risk object |
| `GET /claims/{id}/reserves`, `POST /claims/{id}/reserves` | balances and history; open or adjust a reserve (201) |
| `POST /claims/{id}/reserves/{historyId}/approve`, `reject`, `retract`, `retry-posting` | decide on a pending reserve; retry a failed posting |
| `GET /claims/{id}/documents`, `POST /claims/{id}/documents` | list with download links; multipart upload up to 50 MB (201) |
| `GET /policies/search?q=`, `GET /reference/cause-of-loss-codes`, `GET /reference/claim-statuses` | reference data |

- A successful write answers `{ "data": ..., "warnings": [...] }`; a read answers the bare object or page. Lists take `page` and `pageSize` (default 25, at most 100).
- A refusal answers `{ "type", "title", "status", "errors" }` with 401, 403, 404, 409 or 422.
- Enums are sent and read as their names. Every response carries `X-Correlation-Id`.
- Send `Idempotency-Key: <value>` on any write to make a retry safe: the same key with the same body returns the first successful response, with a different body it answers 422. A refused or failed write is not stored, so the same key can be retried once the cause is fixed.
- `docs/layers/api.md` has the rules behind all of this.

## Configuration

Required settings are validated at startup; a missing one stops the app and names the key. Deployed, a setting is an app setting with `__` for `:` (for example `Jwt__Issuer`). A secret marked Key Vault is an app setting whose value is a Key Vault reference (`@Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/<name>/)`), resolved by App Service through the app's managed identity, so the code reads it like any other setting.

| Setting | Local | Deployed |
|---|---|---|
| `Jwt:SigningKey` (32+ characters, secret) | user-secrets | Key Vault |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | app settings |
| `ConnectionStrings:ClaimsDb` | `appsettings.Development.json` | app settings |
| `Cors:AllowedOrigins` (at least one) | `appsettings.Development.json` | `Cors__AllowedOrigins__0`, `__1`, ... |
| `StorageProvider` (`LocalFileSystem` default, or `AzureBlob`) | `appsettings.json` | app settings |
| `LocalBaseUrl`, `LocalSigningKey` (secret), for `LocalFileSystem`; `LocalRoot` is optional | URL in `appsettings.Development.json`, key in user-secrets | not used |
| `AzureConnectionString` (secret, with an account key), for `AzureBlob` | not used | Key Vault |
| `Hangfire:PostingSweep:MinimumAge` (optional, default `00:10:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:PostingSweep:IntervalMinutes` (optional, default 5, must divide 60 evenly) | `appsettings.json` | app settings |
| `DevAuth:OrganisationId`, `DevAuth:Users` (id, name, role), `DevAuth:TokenLifetimeMinutes` (default 480, at most 1440), required while `DevAuth:Enabled` is true; the organisation must be the seeded one | `appsettings.Development.json` | app settings, for a live demo |
| `DevAuth:Secret` (16+ characters, secret), the access code for `POST /api/dev/token`; required outside Development while `DevAuth:Enabled` is true | optional, user-secrets | Key Vault |
| `Hangfire:SlaBreach:IntervalMinutes` (optional, default 15, must divide 60 evenly) | `appsettings.json` | app settings |
| `Hangfire:IdempotencyCleanup:IntervalHours` (optional, default 1; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:MinimumAge` (optional, default `1.00:00:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:IntervalHours` (optional, default 12; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |

Feature switches are booleans that default to false and do not depend on the environment; `appsettings.Development.json` turns on the first three; the blob sweep deletes blobs and is turned on only by an explicit setting.

| Switch | Effect when true |
|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request |
| `DevAuth:Enabled` | serves `POST /api/dev/token`, a simulated sign-in that issues a token for a configured user; with `DevAuth:Secret` set, the call must send it as `X-DevAuth-Secret` |
| `Hangfire:BlobSweep:Enabled` | schedules the job that deletes stored blobs no document refers to |

## Deployment

The demo runs on Azure: the API on App Service, the frontend on a static web app, Azure SQL, Blob storage and Key Vault, all from `infra/main.bicep`. `infra/bootstrap.md` holds the one-time setup (deploy identity, GitHub environment, the JWT key and the access code in the vault); after it, the `Deployment` workflow (manual) builds, tests, provisions, migrates, deploys the API and the frontend, and smoke-tests the API. Reviewers sign in with the access code from the vault. `docs/layers/deployment.md` has the details.
