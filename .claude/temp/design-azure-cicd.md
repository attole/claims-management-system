status: active

# Azure deployment and GitHub Actions CI/CD (backend)

## OPEN

Questions that block or shape the build. Nothing here is decided by the user yet.

- Azure subscription: which one, its type (free trial, pay-as-you-go, sponsorship) and your role on it (Owner is needed once, to create the deploy identity and its role assignments).
- Region: not stated anywhere. Pick one close to the reviewers and available for every service (App Service, Azure SQL, Static Web Apps).
- Repo visibility: `origin` is `github.com/attole/claims-management-system`. A private repo works with OIDC; GitHub Environments with required reviewers need GitHub Pro/Team on a private repo. Which is it?
- Deployed sign-in: see the `[?] [Deployed sign-in]` block. The assessment wants test users on the deployed app; the repo's README says `DevAuth` is "not used" when deployed.
- Frontend: this pack set covers the backend and the shared Azure resources only. Which Static Web Apps hostname the frontend gets is an output of Pack 3, and the frontend session must adopt it for `Cors__AllowedOrigins__0` and its API base URL.
- The workflow check (`.claude/tools/check_workflow.py`) stays a local-only tool, committed in the repo but not run in CI (decided).
- Storage key names in `docs/layers/infrastructure.md` fixed to the top-level names the code binds; deployed app settings use the top-level names.
- Region: Bulgaria has no Azure region; Poland Central is the nearest EU candidate. Availability of each service there to be checked with a read-only `az` query (asks first).
- Cost: figures below are approximate and unverified; check the Azure pricing calculator for the chosen region before provisioning.

-

## ANALOGUE

Facts verified in the repo. Nothing in the repo deploys today: no `.github/`, no `infra/`, no Dockerfile, no health endpoint.

- `backend/src/ClaimsModule.API/Program.cs` (line 8): four `Add...` calls and `UseApiPipeline`; no `ForwardedHeaders`, no health checks.
- `backend/src/ClaimsModule.Infrastructure/BackgroundJobs/BackgroundJobsServiceExtensions.cs` (line 20): Hangfire storage is SQL Server on the `ClaimsDb` connection string, and `AddHangfireServer()` runs the job server inside the web process. Consequence: the web app must stay running (Always On) or the sweeps, the SLA job and GL posting stop, and close/withdraw refuse while an approved row is unposted (`docs/ARCHITECTURE.md` 'Decisions on gaps').
- Hangfire creates its own tables on first start (`docs/layers/persistence.md` Run locally step), so the app's database user needs DDL rights, not only read/write.
- `docs/layers/persistence.md` 'Migrations and seed data': migrations never run at startup; `ClaimListViewAndAuditGrants` creates the role `claims_app` and denies update/delete on `ClaimAuditLog` to it; "the deployment adds the app's database user to that role". This is a hard requirement on the deployment (S:AUD-NoUpdateDelete).
- `backend/src/ClaimsModule.Persistence/Schema/DesignTimeDbContextFactory.cs` (line 11): tooling reads `CLAIMS_DB_CONNECTION`. A migration bundle can take `--connection` instead.
- `backend/dotnet-tools.json`: `dotnet-ef` is a local tool, so the pipeline runs `dotnet tool restore`.
- `docs/layers/api.md` Identity: anonymous routes are exactly three (dev token, Swagger, signed local download) and a reflection test enforces it. A health endpoint is a fourth and needs the rule and the test updated together.
- `docs/layers/api.md` Identity: the Hangfire dashboard is open to every request when on, and "must not be enabled in a deployed environment".
- `docs/layers/infrastructure.md` Storage adapter: `AzureBlobStorageService` signs the SAS with the account key, so the connection string must carry a key; the Azure listing, delete and SAS were never run against a real account or Azurite. The blob sweep is off by default and stays off until that is checked.
- README Configuration table: the full list of settings and their deployed form (`__` for `:`). Required settings fail at startup naming the key, which is the first smoke signal after a deploy.
- Packages: `Microsoft.Data.SqlClient 5.1.9` (supports Entra managed-identity authentication in the connection string), `Azure.Storage.Blobs 12.30.1`, Hangfire 1.8.25, EF Core 9.0.20. SDK on this machine is 10.0.300; the target stays net9.0.

-

## PROVISIONAL

### Target architecture

One environment (`demo`) in one resource group, sized for the assessment: public, working, cheap, torn down with one command.

- Resource group `rg-claims-demo`.
- App Service plan (Linux, B1) and one Web App (runtime .NET 9) for the API. Always On, HTTPS only, TLS 1.2 minimum, FTP disabled, ARR affinity off, health check path set.
- Azure SQL logical server and one database. Entra-only authentication (no SQL password exists). The web app's system-assigned managed identity is the database user.
- Storage account (StorageV2, LRS, TLS 1.2, public blob access off) with the private container `claim-documents`. Documents are served by SAS URL straight from the blob (S:BR-D-02), never through the API.
- Key Vault (RBAC mode) holding the JWT signing key and the storage connection string. The web app reads them through Key Vault references in app settings, resolved by its managed identity (role Key Vault Secrets User). No code change, no extra package.
- Static Web App (Free) for the frontend, created now so its hostname exists for CORS. Its deployment belongs to the frontend session.
- Log Analytics workspace and Application Insights (workspace-based), connected through two app settings once Pack 1 adds the SDK.
- The deploy identity: an Entra app registration with a federated credential for the GitHub repo and environment (OIDC). No client secret is stored anywhere.

Request and data flow:

- Browser -> Static Web App (Angular) and -> Web App (API, bearer token) -> Azure SQL (EF Core and Hangfire tables) and -> Blob (upload through the API; download by SAS directly from the blob).
- Hangfire server runs inside the Web App and uses the same database.
- Web App -> Key Vault (at start and on reference refresh) through its managed identity.

### Shape of the repo (names only)

[+] infra/main.bicep
    - subscription-independent, resource group scope; composes the modules below
[+] infra/modules/appservice.bicep, sql.bicep, storage.bicep, keyvault.bicep, monitoring.bicep, staticwebapp.bicep, roles.bicep
    - one resource family each; outputs are names and hostnames only, never secrets
[+] infra/demo.bicepparam
    - the environment's parameters (region, SKUs, name prefix, CORS origin, DevAuth users); no secrets
[+] infra/sql/grant-app-user.sql
    - idempotent: create the managed-identity user by SID, add it to `claims_app`, `db_datareader`, `db_datawriter`, `db_ddladmin`
[+] infra/bootstrap.md
    - the one-time manual runbook: resource group, app registration, federated credentials, role assignments, GitHub environment and variables
[+] .github/workflows/build-and-test.yml
[+] .github/workflows/deployment.yml
[~] backend/src/ClaimsModule.API (Pack 1: health endpoints, forwarded headers, Application Insights)
[~] README.md, docs/ARCHITECTURE.md, docs/layers/api.md, docs/layers/infrastructure.md (Pack 7)

### Build and test workflow (`build-and-test.yml`)

Triggers: pull request and push to `main`.

- Checkout, `actions/setup-dotnet` 9.x, NuGet cache keyed on `Directory.Packages.props`.
- A `mcr.microsoft.com/mssql/server:2022-latest` service container with `CLAIMS_TEST_SQLSERVER` set, so the SQL Server, API and Hangfire tests run and fail loudly instead of skipping (the repo's own rule for CI).
- `dotnet build backend -warnaserror`-equivalent (the repo builds with 0 warnings; follow `Directory.Build.props`), `dotnet test backend`. The workflow check is not part of CI.
- Pin third-party actions to a commit SHA, `permissions: contents: read`, and a `concurrency` group that cancels superseded runs.
- Frontend build and test jobs are added by the frontend session as a separate job in the same file.

### Deployment workflow (`deployment.yml`)

Trigger: `workflow_dispatch` (the assessment accepts manual), with an optional later `workflow_run` after a green CI on `main`. Runs in the GitHub Environment `demo`. `concurrency: deploy-demo`, never cancel in progress.

Jobs, in order:

1. `build`: restore, `dotnet publish` the API (Release, framework-dependent, linux-x64 not required), `dotnet ef migrations bundle` as a self-contained executable, upload both as one artifact. The build runs once; later jobs deploy exactly that artifact.
2. `infra`: `azure/login` with OIDC, `az deployment group what-if`, then `az deployment group create` with `infra/demo.bicepparam`; the JWT signing key comes in as a secure parameter from an environment secret and is written to Key Vault by the template. Outputs (web app name, SQL server FQDN, SWA hostname) feed the next jobs.
3. `database`: open a temporary firewall rule for the runner IP (removed in an `always()` step), run the migration bundle with an Entra-token connection string, then run `infra/sql/grant-app-user.sql`. The deploy identity is the SQL server's Entra admin in this design (see the SQL access block).
4. `deploy`: `azure/webapps-deploy` with the published artifact (zip deploy), then restart.
5. `smoke`: poll `/health/ready` until 200 (timeout 5 min), fetch `/swagger/v1/swagger.json`, call `POST /api/dev/token` for the handler user and `GET /api/claims` with it, assert 200 and the `X-Correlation-Id` header. A failed smoke fails the run; no automatic rollback (one instance, no slot).

Migrations run before the new code is live, so every migration must be backward compatible with the previous release for the minutes between jobs 3 and 4 (expand, then contract). State this as a rule in `docs/layers/persistence.md` in Pack 7.

### Application settings (deployed)

Set by Bicep on the Web App. `__` stands for `:`. A name ending in `(KV)` is a Key Vault reference.

| Setting | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Jwt__SigningKey` (KV) | the vault secret |
| `Jwt__Issuer`, `Jwt__Audience` | the appsettings defaults; override only if the frontend needs it |
| `ConnectionStrings__ClaimsDb` | `Server=tcp:<fqdn>,1433;Database=<db>;Authentication=Active Directory Managed Identity;Encrypt=True;` |
| `Cors__AllowedOrigins__0` | `https://<swa hostname>` |
| `StorageProvider` | `AzureBlob` |
| `AzureConnectionString` (KV) | the vault secret, built from the account key by the template |
| `Swagger__Enabled` | `true` (the assessment requires Swagger on the public URL) |
| `Hangfire__Dashboard__Enabled` | `false`, never on |
| `Hangfire__BlobSweep__Enabled` | `false` until the Azure adapter is run once against the real account |
| `DevAuth__Enabled`, `DevAuth__OrganisationId`, `DevAuth__Users__N__*` | see the sign-in block |
| `WEBSITE_HEALTHCHECK_MAXPINGFAILURES` and the health check path | `/health/ready` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | from the monitoring module, after Pack 1 |

Two traps: the managed-identity connection string works for EF Core and Hangfire alike because both read `ClaimsDb`, but only if the identity's database user exists before the first request, otherwise the app starts and the first query fails; and `DevAuth:OrganisationId` must equal the seeded organisation id (`docs/layers/persistence.md` Migrations and seed data), which the template takes as a parameter and a test or smoke step should confirm.

### Flows other than the happy path

- First deploy: the Web App exists before the database user does, so `infra` succeeds, `database` creates the user, and the app is only healthy after `deploy`. Order in the workflow handles it; the Web App's first start before `database` completes is allowed to fail its health check.
- Re-deploy twice: `what-if` shows no change, the migration bundle applies nothing, the grant script is idempotent, zip deploy replaces the same artifact. This is the fire-twice check.
- Rotating the JWT key: change the environment secret and re-run; Key Vault gets a new secret version, the reference resolves on restart, and issued tokens stop validating. Acceptable for a demo.
- Teardown and rebuild: `az group delete` removes everything; soft-deleted Key Vault names stay reserved for the retention period, so the vault name carries a short unique suffix and purge protection is off for this environment.

### [?] decisions

[?] Hosting

option A (App Service, Linux, B1, zip deploy):
what it does: a managed web host that runs the published .NET 9 output. Pros: the assessment names it; no image build or registry; Always On and health check are plain settings; managed identity and Key Vault references work out of the box; about 13 USD a month. Cons: no deployment slots on B1 (a bad deploy is live at once); one instance by default; the runtime image is Microsoft's, not yours. Pitfalls: the free F1 tier has no Always On and a daily CPU quota, so Hangfire stops and the posting sweep never runs; do not use it. Fit: the Infrastructure layer already runs Hangfire in the web process, which is exactly what a long-lived App Service instance expects.

option B (Container Apps with a Dockerfile and a registry):
what it does: runs a container image on a consumption plan. Pros: a Dockerfile also serves local docker-compose (the assessment values it); scale rules and revisions give a rollback path. Cons: needs Azure Container Registry (about 5 USD a month on Basic) or a public registry, a Dockerfile to write and maintain, an environment, ingress and identity plumbing; scale to zero would stop Hangfire unless the minimum replicas is 1. Pitfalls: the same minimum-replica trap as F1; image tags must be immutable or a redeploy does nothing. Fit: no Docker artifact exists in the repo, so this adds a whole new surface.

recommendation: option A - it matches the stated stack, needs the least new machinery, and the Hangfire-in-process design is safe on it; revisit B only if a Dockerfile is wanted for local compose.

[?] Infrastructure as code

option A (Bicep):
what it does: declarative Azure templates compiled to ARM, deployed at resource group scope. Pros: Azure-native, no state file to host or lock, `what-if` preview, first-class support for new Azure features, readable modules; the repo's CI can lint with `az bicep build`. Cons: Azure only; the language is another thing to learn; no import of existing resources. Pitfalls: secrets passed as outputs leak into deployment history, so outputs carry names only; role assignments need the deploy identity to hold a role-assignment permission. Fit: one cloud, one environment.

option B (Terraform):
what it does: declarative providers with a state file. Pros: widely known, plan/apply, multi-cloud. Cons: the state needs its own storage account and locking before anything else exists (a chicken-and-egg bootstrap), the state file contains secrets, and a provider lag behind new Azure features. Pitfalls: state drift when someone edits in the portal. Fit: nothing else in the repo uses it.

option C (az CLI scripts):
what it does: imperative commands in a shell script. Pros: no new language; trivial to read. Cons: not idempotent by default, no preview, no drift detection, ordering and dependency by hand. Pitfalls: a half-failed script leaves a half-built environment. Fit: fine for the one-time bootstrap only, which is where it is used.

recommendation: option A - declarative, stateless and previewable; keep CLI for the bootstrap runbook only.

[?] Database authentication and access

option A (Entra-only server, managed identity for the app, deploy identity as SQL Entra admin):
what it does: no SQL passwords exist. The Web App's identity is a database user created by SID; the deploy identity runs migrations and the grant script. Pros: nothing to leak or rotate; the connection string holds no secret; matches Key Vault maturity. Cons: more moving parts; creating the user needs `CREATE USER ... WITH SID = <client id as binary>, TYPE = E` to avoid a Graph lookup that the server identity may not be allowed; making the deploy service principal the server admin gives CI full control of the database. Pitfalls: Hangfire needs DDL on first start, so the app user gets `db_ddladmin`; `DENY` on `ClaimAuditLog` still wins over the `db_datawriter` grant, and a smoke test must prove it by attempting an update as that user.

option B (SQL authentication, password in Key Vault):
what it does: SQL admin login plus a contained app user with a password. Pros: one connection-string pattern for local and cloud; simplest first deploy. Cons: a password to generate, store, rotate and leak into logs; the admin password would also sit in a deploy parameter. Pitfalls: a login that owns the database is not bound by the audit-table DENY (`docs/layers/persistence.md`), so the app must use the restricted user, not the admin.

recommendation: option A - no stored database secret, and it forces the least-privilege split that S:AUD-NoUpdateDelete expects. Accept the CI admin trade-off for a demo and note it.

[?] Migration execution in the pipeline

option A (EF migration bundle executed in the `database` job):
what it does: `dotnet ef migrations bundle` builds one executable; the job runs it with the connection string. Pros: no SDK or source needed at deploy time; same artifact is reproducible; the run is a single visible step. Cons: the bundle is built per run (a minute); needs the firewall rule for the runner. Pitfalls: bundle and published app must come from the same commit, so build them in one job.

option B (`dotnet ef database update` from source):
what it does: runs the tool from the checkout. Pros: nothing to build. Cons: needs the SDK and the whole tree on the migrating job; the design-time factory reads `CLAIMS_DB_CONNECTION` and does not take a token connection string without code care. Pitfalls: easy to run against the wrong database through a leaked env var.

option C (idempotent SQL script via `sqlcmd`):
what it does: `dotnet ef migrations script --idempotent` produces SQL a DBA can read. Pros: reviewable; one tool to run it, the same one as the grant script. Cons: the hand-added view and grant SQL in the second migration is included as is, fine, but the script is one more artifact. Pitfalls: none serious.

option D (migrate at app start): rejected; the repo rule is that the app does not migrate at startup.

recommendation: option A - the assessment asks for migrations in the pipeline and a bundle is the cleanest way; option C is the fallback if the bundle fights the Entra token connection string.

[?] Deployed sign-in

option A (DevAuth on in the demo, three configured users):
what it does: `POST /api/dev/token` stays on, with the handler, supervisor and manager from app settings. Pros: matches the assessment's "mock auth is acceptable" and the required test credentials; zero code. Cons: anyone who finds the URL can mint a manager token and use or alter the data. Pitfalls: this reverses the README line "not used" when deployed, so the README and `docs/layers/api.md` need the exception written down; the data must be fake; the Hangfire dashboard still stays off.

option B (DevAuth off, real sign-in via Entra ID):
what it does: the API validates Entra tokens. Pros: honest security. Cons: an app registration, role mapping to `Handler`/`Supervisor`/`Manager`, claim mapping (`sub`, `org`, `role`, `name`) and a frontend sign-in flow; much larger, and across both sessions. Pitfalls: contradicts "authentication is simulated" in the assessment's own wording.

option C (DevAuth on, but behind a shared secret header or IP allow-list at App Service):
what it does: access restrictions limit who reaches the API. Pros: cheap protection for the demo. Cons: reviewers must be given the secret or their IPs, and Swagger must also be reachable by them. Pitfalls: breaks the "publicly accessible" requirement.

recommendation: option A for the review window, with seed-only data and a README note; it is the only choice that meets the stated deliverable without a large new feature. Needs your explicit yes because it is a deliberate exposure.

[?] JWT signing key and storage secret delivery

option A (GitHub environment secret as a secure Bicep parameter, template writes Key Vault):
Pros: fully reproducible from the repo plus one secret; no human step after bootstrap; the storage connection string is built inside the template with `listKeys` and never shown. Cons: the key passes through GitHub (encrypted) and the deployment history marks it secure but not absent. Pitfalls: never output it; rotating needs a re-run.

option B (generate once by the bootstrap script, store in Key Vault, pipeline never sees it):
Pros: the pipeline holds no secret at all. Cons: an extra manual step before the first healthy deploy, an ordering trap (a Key Vault reference to a missing secret shows as unresolved), and a less reproducible setup story for the README requirement.

recommendation: option A - reproducibility is a stated deliverable, and the exposure is limited to a demo signing key.

### Build sequence

Dependency order, as a suggestion. Each pack is a stop point. Packs 1, 3, 4 and 5 write files; Pack 6 runs Azure commands and needs your explicit go-ahead per command group.

### Pack 1 - Cloud readiness in the API

[~] backend/src/ClaimsModule.API
    - a `Pipeline/Health/` feature: liveness (`/health/live`, no checks) and readiness (`/health/ready`, database reachable through EF), anonymous, with its `Add...` and `Use...` lines in `ApiPipelineServiceExtensions`
    - forwarded headers for `X-Forwarded-For` and `X-Forwarded-Proto`, first in the pipeline order (the Linux App Service image enables it by default; explicit configuration makes the behavior testable)
    - Application Insights through the OpenTelemetry distro, behind a setting that defaults to off, so local runs are unchanged
[~] backend/Directory.Packages.props
    - the one monitoring package, version checked on NuGet before pinning
[~] backend/tests/ClaimsModule.API.Tests
    - the anonymous-route reflection test changes from three to four allowed routes and names the health routes; health tests for 200 ready, 503 when the database is unreachable, and no auth required
[~] docs/layers/api.md
    - the Identity rule 'anonymous routes' and the pipeline order, in the same pack
[=] the check: `python -I .claude/tools/check_workflow.py` must stay at 0 findings

### Pack 2 - CI workflow

[+] .github/workflows/build-and-test.yml
    - build, test with the SQL service container, pinned actions, least permissions, concurrency
[?] verify on the first real run what differs on a Linux runner: path separators and LocalDB-only assumptions; any fix is a separate decision

### Pack 3 - Bicep infrastructure

[+] infra/main.bicep, infra/demo.bicepparam
[+] infra/modules/appservice.bicep, sql.bicep, storage.bicep, keyvault.bicep, monitoring.bicep, staticwebapp.bicep, roles.bicep
    - names built from a prefix and `uniqueString(resourceGroup().id)`, tags on everything, outputs names only
    - the role assignments: web app identity -> Key Vault Secrets User on the vault; nothing else needs one
[=] validated offline with `az bicep build` and `what-if` (the second needs a subscription, so it moves to Pack 6)

### Pack 4 - Bootstrap and database access

[+] infra/bootstrap.md
    - resource group, app registration with federated credentials for the GitHub environment `demo` (subject `repo:attole/claims-management-system:environment:demo`), Contributor plus a constrained role-assignment role on the resource group, the SQL Entra admin, the GitHub environment variables
[+] infra/sql/grant-app-user.sql
    - user by SID, role memberships, idempotent; a second statement set that proves the audit DENY for the smoke step

### Pack 5 - Deploy workflow

[+] .github/workflows/deployment.yml
    - the five jobs above, OIDC login, firewall open and close in `always()`, artifact reuse, smoke test with polling and a hard timeout

### Pack 6 - First provisioning and live verification (Azure commands, asks first)

- Run the bootstrap commands you approve one group at a time, then trigger the workflow and read its run through read-only `gh run view`.
- Run the Azure Blob adapter once against the real account (upload, SAS download, list by age, delete), because it was never run; only then consider turning the blob sweep on.
- Record the resulting URLs and the test users for the README.

### Pack 7 - Docs

[~] README.md: Azure resources, how to reproduce the deployment, the deployed settings, test users, URLs
[~] docs/ARCHITECTURE.md: an 'Azure architecture' section (the assessment requires one): services, how they interact, the decisions above and their reasons, the DevAuth exception
[~] docs/layers/persistence.md: backward-compatible migrations rule, the deployment grants
[~] docs/layers/infrastructure.md: the storage key names fix, the Azure adapter verified-on date
[~] .claude/CLAUDE.md: only if a rule changes; the 'Configuration' paragraph's "deployed" wording stays as is

-

## DECISIONS

- IaC: Bicep.
- Deployed sign-in: DevAuth stays on in the demo so reviewers can test every role; fake data only.
- Subscription: Azure free account with the 200 USD credit (30-day window); repo is public.
- Hosting: App Service Linux B1. Region: Poland Central (check service availability first). Database: Entra-only with managed identity. Migrations: EF bundle in the pipeline. JWT key: set straight in Key Vault by the bootstrap runbook; Bicep writes the storage connection string itself.
- Pack 1 done: health endpoints (`/health/live`, `/health/ready`). Forwarded headers are not code: Pack 3 sets the app setting `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`. Application Insights is deferred to a later pack (package version to be verified first).

-

## NEXT

- You pick the six `[?]` blocks and answer the open items (subscription, region, repo visibility, the DevAuth exposure).
- Then Pack 1 on your word ("do pack 1"), one pack at a time.
- Graduate settled decisions to `docs/ARCHITECTURE.md` in Pack 7 and delete this note when the packs are done.
