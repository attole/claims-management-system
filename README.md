# Claims Management System

Claims handling for a policy administration system, built end to end: a .NET 9 backend (Clean Architecture, CQRS) and an Angular frontend.

Status: the Domain and the Application are built and tested. Persistence, Infrastructure and the API hold only their skeletons, and the frontend is not started.

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

## Run locally

1. Create the database (Hangfire creates its tables, not the database): `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE ClaimsModule"`
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

Feature switches are booleans that default to false and do not depend on the environment; `appsettings.Development.json` turns all three on.

| Switch | Effect when true |
|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request |
| `DevAuth:Enabled` | reserved for the dev token endpoint (not built yet) |
