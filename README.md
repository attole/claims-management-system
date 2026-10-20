# Claims Management System

Claims handling for a policy administration system, built end to end: a .NET 9 backend (Clean Architecture, CQRS) and an Angular frontend.

Status: the Domain, the Application, the Persistence (with its migrations and seed) and the Infrastructure (jobs, storage adapters, clock) are built and tested. The API holds only its skeleton, and the frontend is not started.

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

The tests in `ClaimsModule.Persistence.SqlServerTests` create a temporary database on LocalDB (or on the server in `CLAIMS_TEST_SQLSERVER`) and are skipped when no server answers.

## Run locally

1. Create and seed the database. The app does not migrate at startup. From the `backend` folder run `dotnet tool restore`, then `dotnet dotnet-ef database update --project src/ClaimsModule.Persistence` (Hangfire creates its own tables later). It targets LocalDB; set `CLAIMS_DB_CONNECTION` to use another server.
2. Set the signing key (32 or more characters, any test value locally): `dotnet user-secrets set "Jwt:SigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API`
3. Run: `dotnet run --project backend/src/ClaimsModule.API`

With `ASPNETCORE_ENVIRONMENT=Development`, Swagger is at `/swagger` and the Hangfire dashboard at `/hangfire`.

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
| `Hangfire:SlaBreach:IntervalMinutes` (optional, default 15, must divide 60 evenly) | `appsettings.json` | app settings |
| `Hangfire:IdempotencyCleanup:IntervalHours` (optional, default 1; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:MinimumAge` (optional, default `1.00:00:00`, must be positive) | `appsettings.json` | app settings |
| `Hangfire:BlobSweep:IntervalHours` (optional, default 12; 1, 2, 3, 4, 6, 8 or 12) | `appsettings.json` | app settings |

Feature switches are booleans that default to false and do not depend on the environment; `appsettings.Development.json` turns on the first three; the blob sweep deletes blobs, so it stays off until set.

| Switch | Effect when true |
|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request |
| `DevAuth:Enabled` | reserved for the dev token endpoint (not built yet) |
| `Hangfire:BlobSweep:Enabled` | schedules the job that deletes stored blobs no document refers to |
