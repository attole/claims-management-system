# Claims Management System

Claims handling for a policy administration system, built end to end: a .NET 9 backend (Clean Architecture, CQRS) and an Angular frontend.

Status: the backend (Domain, Application, Persistence, Infrastructure and API) is built and tested; the frontend is not started.

## Documentation

- `docs/ARCHITECTURE.md`: the design overview, the decisions and the reasons.
- `docs/CODING-STANDARDS.md`: code style, naming and tests.
- `docs/layers/`: the rules and contracts of each backend layer.
- `docs/AI-WORKFLOW.md`: how AI is used in this project.

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (`MSSQLLocalDB`) or another SQL Server 2022 instance

## Build and test

```bash
dotnet build backend
dotnet test backend
python -I .claude/tools/check_workflow.py
```

The tests in `ClaimsModule.Persistence.SqlServerTests` and `ClaimsModule.API.Tests` create a temporary database on LocalDB (or on the server in `CLAIMS_TEST_SQLSERVER`) and are skipped when no server answers. When `CLAIMS_TEST_SQLSERVER` is set and the server does not answer, they fail instead, so CI never skips them silently. The Hangfire tests in `ClaimsModule.Infrastructure.Tests` use the same database and rule.

## Run locally

1. Create and seed the database. The app does not migrate at startup. From the `backend` folder run `dotnet tool restore`, then `dotnet dotnet-ef database update --project src/ClaimsModule.Persistence` (Hangfire creates its own tables later). It targets LocalDB; set `CLAIMS_DB_CONNECTION` to use another server.
2. Set the signing key (32 or more characters, any test value locally): `dotnet user-secrets set "Jwt:SigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
3. Run: `dotnet run --project backend/src/ClaimsModule.API`
4. Get a token for a user: `POST /api/dev/token` with `{ "userId": "<id>" }`, using an id from `DevAuth:Users` in `appsettings.Development.json` (a handler, a supervisor and a manager), and send it as `Authorization: Bearer <token>`. This is a simulation of sign-in, only available while `DevAuth:Enabled` is true.

With `ASPNETCORE_ENVIRONMENT=Development`, Swagger is at `/swagger` and the Hangfire dashboard at `/hangfire`.

## API

All routes are under `/api` and need a bearer token, except `POST /api/dev/token`, Swagger and the signed local document download. The routes, with `{id}` for a claim:

| Route | What it does |
|---|---|
| `GET /claims`, `POST /claims` | paged list with filters; create a draft with its children (201) |
| `GET /claims/{id}`, `PUT /claims/{id}/status`, `GET /claims/{id}/status-check?targetStatus=` | detail; change the status; what blocks a status change, without saving |
| `PUT /claims/{id}/notes`, `GET /claims/{id}/audit` | update the notes; paged audit log |
| `POST /claims/{id}/parties`, `DELETE /claims/{id}/parties/{partyId}` | add and remove a party |
| `GET /claims/{id}/reserves`, `POST /claims/{id}/reserves` | balances and history; open or adjust a reserve (201) |
| `POST /claims/{id}/reserves/{historyId}/approve`, `reject`, `retract`, `retry-posting` | decide on a pending reserve; retry a failed posting |
| `GET /claims/{id}/documents`, `POST /claims/{id}/documents` | list with download links; multipart upload up to 50 MB (201) |
| `GET /policies/search?q=`, `GET /reference/cause-of-loss-codes`, `GET /reference/claim-statuses` | reference data |

- A successful write answers `{ "data": ..., "warnings": [...] }`; a read answers the bare object or page. Lists take `page` and `pageSize` (default 25, at most 100).
- A refusal answers `{ "type", "title", "status", "errors" }` with 401, 403, 404, 409 or 422.
- Enums are sent and read as their names. Every response carries `X-Correlation-Id`.
- Send `Idempotency-Key: <value>` on any write to make a retry safe: the same key with the same body returns the first response, with a different body it answers 422.
- `docs/layers/api.md` has the rules behind all of this.

## Configuration

Required settings are validated at startup; a missing one stops the app and names the key. Deployed, a setting is an app setting with `__` for `:` (for example `Jwt__Issuer`).

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
| `DevAuth:OrganisationId`, `DevAuth:Users` (id, name, role), `DevAuth:TokenLifetimeMinutes` (default 480, at most 1440), required while `DevAuth:Enabled` is true; the organisation must be the seeded one | `appsettings.Development.json` | not used |
| `Hangfire:SlaBreach:IntervalMinutes` (optional, default 15, must divide 60 evenly) | `appsettings.json` | app settings |
| `Hangfire:IdempotencyCleanup:IntervalHours` (optional, default 1; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:MinimumAge` (optional, default `1.00:00:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:IntervalHours` (optional, default 12; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |

Feature switches are booleans that default to false and do not depend on the environment; `appsettings.Development.json` turns on the first three; the blob sweep deletes blobs, so it stays off until set.

| Switch | Effect when true |
|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request |
| `DevAuth:Enabled` | serves `POST /api/dev/token`, a simulated sign-in that issues a token for a configured user |
| `Hangfire:BlobSweep:Enabled` | schedules the job that deletes stored blobs no document refers to |
