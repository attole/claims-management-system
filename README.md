# Claims Management System

Claims handling for a policy administration system, built end to end: a .NET 9 backend (Clean Architecture, CQRS), an Angular frontend and an Azure deployment from Bicep and GitHub Actions.

It covers claim intake, the claim lifecycle, parties, reserves with tiered approval and a simulated GL posting, documents in Blob storage, the audit log and background jobs, behind three screens: the claims list, the intake form and the claim detail.

## Documentation

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md): the design overview, the decisions and the reasons.
- [`docs/BACKEND-STANDARDS.md`](docs/BACKEND-STANDARDS.md), [`docs/FRONTEND-STANDARDS.md`](docs/FRONTEND-STANDARDS.md) and [`docs/DEPLOYMENT-STANDARDS.md`](docs/DEPLOYMENT-STANDARDS.md): code style, naming and tests of each area.
- [`docs/layers/`](docs/layers): the rules and contracts of each backend layer, of the Angular client ([`frontend.md`](docs/layers/frontend.md)) and of Azure and the pipelines ([`deployment.md`](docs/layers/deployment.md)).
- [`infra/`](infra): the Bicep templates, the SQL scripts, the one-time `bootstrap.ps1` and the demo data script. [`.github/workflows/`](.github/workflows): build and test, and the manual deployment.
- [`docs/AI-WORKFLOW.md`](docs/AI-WORKFLOW.md): how AI is used in this project.
- [`frontend/design/`](frontend/design): static HTML mockups of the three screens, the visual reference (not part of the build).

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (`MSSQLLocalDB`) or another SQL Server 2022 instance
- Node.js 24 with npm 11 (frontend)

## Build and test

```bash
dotnet build backend
dotnet test backend
npm --prefix frontend run build
npm --prefix frontend test
```

The tests in `ClaimsModule.Persistence.SqlServerTests` and `ClaimsModule.API.Tests` create a temporary database on LocalDB (or on the server in `CLAIMS_TEST_SQLSERVER`) and are skipped when no server answers. When `CLAIMS_TEST_SQLSERVER` is set and the server does not answer, they fail instead, so CI never skips them silently. The Hangfire tests in `ClaimsModule.Infrastructure.Tests` use the same database and rule.

## Run locally

### Backend

1. Create the database. The app does not migrate at startup. From the `backend` folder run `dotnet tool restore`, then `dotnet dotnet-ef database update --project src/ClaimsModule.Persistence` (Hangfire creates its own tables on first start). It targets LocalDB; set `CLAIMS_DB_CONNECTION` to use another server.
2. Set the two signing keys (32 or more characters, any test values locally), one for the bearer token and one for the signed local download links:
    - `dotnet user-secrets set "Jwt:SigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
    - `dotnet user-secrets set "LocalSigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
3. Run `dotnet run --project backend/src/ClaimsModule.API`. The `https` profile serves `https://localhost:54694`, the address `LocalBaseUrl` in `appsettings.Development.json` points at. With `ASPNETCORE_ENVIRONMENT=Development`, Swagger is at `/swagger` and the Hangfire dashboard at `/hangfire`.
4. Get a token for a user: `POST /api/dev/token` with `{ "userId": "<id>" }`, using an id from `DevAuth:Users` in `appsettings.Development.json` (a handler, a supervisor and a manager), and send it as `Authorization: Bearer <token>`. This simulates a sign-in and works only while `DevAuth:Enabled` is true.

### Frontend

1. Trust the .NET development certificate once, so the browser accepts the API: `dotnet dev-certs https --trust`
2. Install the packages: `npm --prefix frontend install`
3. With the API running, start the dev server: `npm --prefix frontend start`, then open `http://localhost:4200`. The app signs in as the handler; the role switcher in the header changes the test user.
4. Checks: `npm --prefix frontend run lint` and `npm --prefix frontend run format:check`.
5. After an API contract change, save the API's `/openapi/v1.json` over `frontend/openapi.json` and run `npm --prefix frontend run api:types`.

The three screens are the claims list (`/claims`), the intake form (`/claims/new`) and the claim detail (`/claims/{id}`). [`docs/layers/frontend.md`](docs/layers/frontend.md) describes each one.

## API

All routes are under `/api` and need a bearer token, except `POST /api/dev/token`, Swagger and the signed local document download. Writes answer `{ "data": ..., "warnings": [...] }` and reads the bare object or page. A refusal answers `{ "type", "title", "status", "errors" }` with 401, 403, 404, 409 or 422. Send `Idempotency-Key: <value>` on a write to make a retry safe. Swagger lists every route, and [`docs/layers/api.md`](docs/layers/api.md) has the rules and the route list.

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
| `Telemetry:ConnectionString` (optional; empty sends nothing to Application Insights) | not used | app settings |
| `DevAuth:OrganisationId`, `DevAuth:Users` (id, name, role), `DevAuth:TokenLifetimeMinutes` (default 480, at most 1440), required while `DevAuth:Enabled` is true; the organisation must be the seeded one | `appsettings.Development.json` | app settings, for a live demo |
| `Hangfire:PostingSweep:MinimumAge` (optional, default `00:10:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:PostingSweep:IntervalMinutes` (optional, default 5, must divide 60 evenly) | `appsettings.json` | app settings |
| `Hangfire:SlaBreach:IntervalMinutes` (optional, default 15, must divide 60 evenly) | `appsettings.json` | app settings |
| `Hangfire:IdempotencyCleanup:IntervalHours` (optional, default 1; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:MinimumAge` (optional, default `1.00:00:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:IntervalHours` (optional, default 12; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |

Feature switches are booleans that default to false and do not depend on the environment. `appsettings.Development.json` turns on the first three; the blob sweep deletes blobs and is turned on only by an explicit setting.

| Switch | Effect when true |
|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request |
| `DevAuth:Enabled` | serves `POST /api/dev/token`, a simulated sign-in that issues a token for a configured user |
| `Hangfire:BlobSweep:Enabled` | schedules the job that deletes stored blobs no document refers to |

## Deployment

The demo runs on Azure, all of it created from [`infra/main.bicep`](infra/main.bicep): the API on App Service, the frontend on a static web app, Azure SQL, Blob storage, Key Vault, and Log Analytics with Application Insights. The three feature switches are on in the demo and the blob sweep is off, because the data is fake.

1. **One-time setup:** [`infra/bootstrap.ps1`](infra/bootstrap.ps1) (Windows PowerShell, one step per call) creates the deploy identity, the GitHub environment `demo` and the JWT key in the vault.
2. **Build and Test** ([`build-and-test.yml`](.github/workflows/build-and-test.yml)) runs on every push and pull request and stores the tested artifacts.
3. **Deployment** ([`deployment.yml`](.github/workflows/deployment.yml)) is run by hand: it takes the latest green build of `main`, creates the resources, migrates the database, deploys the API and the frontend and smoke-tests the API.
4. **Demo data:** [`infra/seed-demo-data.ps1`](infra/seed-demo-data.ps1) fills the deployed demo with claims through the API.

Reviewers open the frontend, which signs in as a demo user, and switch roles in the header. [`docs/layers/deployment.md`](docs/layers/deployment.md) has the details and [`docs/DEPLOYMENT-STANDARDS.md`](docs/DEPLOYMENT-STANDARDS.md) the conventions.
