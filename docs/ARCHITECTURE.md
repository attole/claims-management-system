# Architecture

## Configuration

Required settings fail startup when missing (options validation with `ValidateOnStart`). Full steps are in the root
`README.md`.

| Setting | Local | Deployed |
|---|---|---|
| `Jwt:SigningKey` (32+ characters) | user-secrets | app setting `Jwt__SigningKey`, Key Vault reference |
| `ConnectionStrings:ClaimsDb` | `appsettings.Development.json` (LocalDB) | app setting `ConnectionStrings__ClaimsDb` |
| `Cors:AllowedOrigins` | `appsettings.Development.json` | app setting `Cors__AllowedOrigins__0` |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | same, override per environment if needed |

Secrets are never committed. Locally they live in the user-secrets store of `ClaimsModule.API`.

## Feature switches

- `Swagger:Enabled`, `Hangfire:Dashboard:Enabled` and `DevAuth:Enabled` are booleans bound to options classes and
  default to false. Environment is never used as the gate, so Azure can switch each one on through app settings
  (`Swagger__Enabled`, `Hangfire__Dashboard__Enabled`, `DevAuth__Enabled`).
- `appsettings.Development.json` turns all three on for local runs.
- The Hangfire dashboard filter allows every request when the switch is on, because a browser page load carries no
  Bearer token. Switching it on in a shared environment exposes job data to anyone with the URL.
- `DevAuth:Enabled` is bound but nothing reads it until the dev token endpoint is built.

## Known risks

### AutoMapper 14.0.0 advisory

- AutoMapper is pinned to 14.0.0 in `backend/Directory.Packages.props`. It is the last release before the commercial
  licence change (15 and later need a licence key).
- GitHub advisory GHSA-rvv3-g6hj-g44x (denial of service through uncontrolled recursion) affects versions below 15.1.1.
  Patched versions are only available under the commercial licence.
- Accepted risk. Exploitation needs a deeply recursive or cyclic object graph reaching a map. Mapping here is limited
  to flat entity-to-DTO and command-to-entity projections with no recursive types, and input is bounded by
  FluentValidation before mapping.
- Mitigation: keep DTOs non-recursive and add no self-referencing maps. Revisit if the licence position changes or a
  recursive type is introduced.
