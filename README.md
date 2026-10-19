# Claims Management System

Vertical slice of a claims management system for a policy administration system: .NET 9 backend (Clean Architecture, CQRS) and Angular frontend. Design notes are in `docs/ARCHITECTURE.md`.

## Solution overview

The backend has five projects: `Domain`, `Application`, `Infrastructure`, `Persistence` and `API`. The Domain is built and tested: the claim with its parties, risk objects, loss, issues and state changes; the reserve component with its history rows (approval and posting states); claim documents; policies; the cause-of-loss codes; and the roles. The other layers and the frontend are not built yet. The decisions behind the model, the rules and the next steps for the other layers are in `docs/ARCHITECTURE.md`.

## Local setup (backend)

### Prerequisites

- .NET 9 SDK (the runtime must be 9.x to run and test the net9.0 projects)
- SQL Server LocalDB (`MSSQLLocalDB`) or another SQL Server 2022 instance

### First run

1. Create the local database (Hangfire creates its tables, but not the database):

    ```bash
    sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE ClaimsModule"
    ```

2. Set the JWT signing key in user-secrets (32 or more characters, any test value locally):

    ```bash
    dotnet user-secrets set "Jwt:SigningKey" "<32+ character test value>" --project backend/src/ClaimsModule.API
    ```

3. Build, test and run:

    ```bash
    dotnet build backend
    dotnet test backend
    dotnet run --project backend/src/ClaimsModule.API
    ```

With `ASPNETCORE_ENVIRONMENT=Development`, Swagger is at `/swagger` and the Hangfire dashboard at `/hangfire`.

## Configuration

Required settings are validated at startup. A missing one stops the app with a message naming the key.

| Setting | Local | Deployed |
|---|---|---|
| `Jwt:SigningKey` (32+ characters, secret) | user-secrets | `Jwt__SigningKey`, from Key Vault |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | `Jwt__Issuer`, `Jwt__Audience` |
| `ConnectionStrings:ClaimsDb` | `appsettings.Development.json` | `ConnectionStrings__ClaimsDb` |
| `Cors:AllowedOrigins` (at least one) | `appsettings.Development.json` | `Cors__AllowedOrigins__0`, `__1`, ... |

User-secrets are stored outside the repository, in the user profile. List them with `dotnet user-secrets list --project backend/src/ClaimsModule.API`.

### Feature switches

Each switch is a boolean that defaults to false, is read through the options pattern, and does not depend on the environment. Turn one on in a deployed environment through an app setting.

| Switch | Effect when true | App setting |
|---|---|---|
| `Swagger:Enabled` | serves the OpenAPI document and Swagger UI | `Swagger__Enabled` |
| `Hangfire:Dashboard:Enabled` | mounts the Hangfire dashboard, open to every request | `Hangfire__Dashboard__Enabled` |
| `DevAuth:Enabled` | reserved for the dev token endpoint (not built yet) | `DevAuth__Enabled` |

`appsettings.Development.json` turns all three on.
