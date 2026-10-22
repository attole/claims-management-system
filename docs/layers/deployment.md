# Deployment layer

Rules and contracts of the Azure environment and the GitHub Actions pipelines: `infra/` (Bicep, SQL scripts, the bootstrap runbook) and `.github/workflows/`. `.claude/CLAUDE.md` points here; read this file before changing either. Every Azure command and every deployment run asks first (CLAUDE.md 'Permissions model').

## Layout

```text
infra/
    main.bicep            resource group scope; composes the modules, builds the app settings, outputs names only
    demo.bicepparam       the demo environment: region, SKUs, switches, dev users; no secrets
    modules/              one resource family each: identity, monitoring, storage, keyvault, sql, staticwebapp, appservice, roles
    sql/                  grant-app-user.sql, verify-app-user.sql
    bootstrap.ps1         the one-time PowerShell script (step by step) and the teardown
    seed-demo-data.ps1    demo claims through the API, run by hand
.github/workflows/
    build-and-test.yml    every pull request and push to main; produces the artifacts the deployment ships
    deployment.yml        manual only (workflow_dispatch), environment demo
```

- One environment, `demo`, in one resource group (`rg-claims-demo`, Poland Central; the static web app in Central US, because West Europe refuses new subscriptions and only its management data lives there). Names are `<kind>-claims-demo-<suffix>` (the storage account, which allows no hyphens, is `stclaimsdemo<suffix>`), the suffix taken from the resource group id; every resource carries the `application` and `environment` tags.
- Outputs are names and hostnames only; no secret is ever a parameter or an output. The one exception inside the template is the Application Insights connection string passed from the monitoring module to the web app; it is not a root output.

## Azure resources

- **API:** App Service plan Linux B1 and one web app (`DOTNETCORE|9.0`): Always On (Hangfire runs in the process), HTTPS only, TLS 1.2, FTP off, no ARR affinity, health check on `/health/ready`, platform logs to Log Analytics.
- **Identity:** a user-assigned managed identity for the web app. It is the database user and reads Key Vault; its client id is an ARM property, so the pipeline creates the database user without Microsoft Graph.
- **Database:** Azure SQL logical server with Entra-only authentication (no SQL password exists) and one Basic database; the deploy identity is the Entra administrator. The firewall rule that allows Azure services is what admits the GitHub-hosted runner; there is no per-run rule.
- **Storage:** StorageV2 Standard LRS, TLS 1.2, no public blob access, the private container `claim-documents`. Downloads are SAS links straight from the blob (S:BR-D-02).
- **Key Vault:** RBAC mode, soft-delete retention 7 days. Secrets: `jwt-signing-key` (set by the bootstrap script), `storage-connection-string` (written by the template from the account key). The web app identity holds Key Vault Secrets User.
- **Frontend:** a Free static web app serving the Angular build; `staticwebapp.config.json` rewrites every route to `index.html`.
- **Monitoring:** a Log Analytics workspace (30 days) and an Application Insights component stored in it. The app sends requests, exceptions, dependencies and logs through `Telemetry__ConnectionString` (`Pipeline/Telemetry/` in the API). The console and platform logs also go to the workspace as a diagnostic setting, because they exist when the app does not start and so cannot send telemetry: the console shows the startup error that names a missing setting, the platform log shows a container crash. HTTP logs are not sent, since requests are traced.

## Settings and secrets

The template writes every app setting; the README 'Configuration' table lists what each one means.

| Setting | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `Production`, `true` |
| `WEBSITE_RUN_FROM_PACKAGE` | `1`: the zip deploy is mounted read-only and the app switches to it on restart, so the running files are never overwritten in place; the app writes nothing under its folder (documents go to Blob storage, Hangfire state to SQL) |
| `Jwt__SigningKey`, `AzureConnectionString` | Key Vault references, resolved by the web app identity at start |
| `ConnectionStrings__ClaimsDb` | the server with `Authentication=Active Directory Managed Identity` and the identity's client id; EF Core and Hangfire share it |
| `Cors__AllowedOrigins__0` | the static web app's `https://` origin |
| `Telemetry__ConnectionString` | the Application Insights connection string (an ingestion address, not a credential), from the monitoring module's output |
| `StorageProvider` | `AzureBlob` |
| `Swagger__Enabled`, `DevAuth__Enabled`, `Hangfire__Dashboard__Enabled` | `true` |
| `Hangfire__BlobSweep__Enabled` | `false` |
| `DevAuth__OrganisationId`, `DevAuth__Users__N__Id`, `Name`, `Role` | the seeded organisation and the three users of `demo.bicepparam` |

- A Key Vault reference is resolved when the app starts, so a changed secret takes effect after a restart.
- A missing app setting stops the app at startup and names the key, but a reference to a missing secret reaches the app as the reference text itself, which passes a length check. The JWT options therefore reject a signing key that starts with `@Microsoft.KeyVault`, so the app stops at startup, readiness never answers and the smoke test fails the run; without that guard the app would sign tokens with a key anyone can derive from the vault name. Run `bootstrap.ps1 -Step Secrets` before the first deployment.
- The frontend is built with the API's absolute URL written into `environment.ts` by the pipeline; the bundle holds no secret.

## Database access

- Migrations run in the pipeline from an EF migration bundle, never at app start, signed in as the deploy identity. A migration must stay compatible with the running release for the minutes between the migration and the code deployment.
- `grant-app-user.sql` (idempotent) creates the web app's database user by its identity's SID and adds it to `db_datareader`, `db_datawriter`, `db_ddladmin` (Hangfire creates its own tables) and `claims_app`, whose deny on update and delete of `ClaimAuditLog` keeps the audit log append-only (S:AUD-NoUpdateDelete).
- `verify-app-user.sql` fails the run unless that user can read and insert audit rows and can neither update nor delete them.

## Pipelines

- **Build and Test** (every pull request and push to main): a backend job and a frontend job. Backend: restore, Release build, tests against a SQL Server 2022 service container (`CLAIMS_TEST_SQLSERVER` set, so the SQL Server, API and Hangfire tests run instead of skipping), then the published API and the self-contained linux-x64 migration bundle as the artifacts `api` and `migrations`. Frontend (Node 24): `npm ci`, lint, format check, tests, build; the build only proves it compiles, because the API's URL is part of the bundle and the deployment rebuilds it. The deployed API and migration bundle are the tested bytes.
- **Deployment** never builds. A `select` job picks the latest successful Build and Test run of a push to main, or the run in the optional `run_id` input (a rollback to any build whose artifacts are still stored, 7 days). It refuses a run that is not Build and Test, not a push to main, not successful or whose artifacts expired, and warns when the chosen commit is not the latest on main. Then one job in the GitHub environment `demo` (main only, a required reviewer) checks out that run's commit, so the Bicep and SQL scripts match the API, downloads its artifacts and signs in to Azure by OIDC with a federated credential bound to that environment.
- **Deployment steps:**
    - Create the resources from `demo.bicepparam` (no what-if: the run already waits for approval). The step's first command fetches the database token right after the sign-in, because the GitHub token behind the login lives about five minutes; the Azure CLI caches the database token for the migration and SQL steps.
    - Apply the migrations (retried), then the grant and verify scripts.
    - Zip deploy of the API, which restarts the app.
    - Build the frontend against the API's URL and upload it to the static web app.
    - The smoke test, last, so the API's cold start overlaps with the frontend steps.
- **Smoke test:** `/health/ready` answers 200 within five minutes (the wait is the app's cold start), the dev token call returns a token, and `GET /api/claims` answers 200 with it, tried up to three times because the first query after a cold start failed once. A failure stops the run; there is no automatic rollback (one instance, no slot).
- Actions are pinned to commit SHAs, workflow permissions are `contents: read` and `actions: read` (plus `id-token: write` on the deploy job), a deployment never cancels another, and a superseded build is cancelled.

## Bootstrap and teardown

`infra/bootstrap.ps1` holds the one-time steps the pipeline cannot do for itself. Run it in Windows PowerShell after `az login` and `gh auth login`, one step per call (`.\infra\bootstrap.ps1 -Step <name>`); every step looks up what it needs, so it can be repeated:

1. `Check`: register the providers and check the region's services.
2. `ResourceGroup`: create the resource group.
3. `Identity`: create the deploy identity: an app registration with a federated credential for the environment `demo`, Contributor and Role Based Access Control Administrator on the resource group. GitHub puts the numeric owner and repository ids into the token subject (`repo:owner@id/name@id:environment:demo`), and Azure compares it exactly, so the script builds the subject from those ids.
4. `GitHub`: create the environment `demo` (main only, you as required reviewer) and its variables (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`, `SQL_ADMIN_OBJECT_ID`, `SQL_ADMIN_LOGIN`); ids only, no secrets.
5. `Provision`: show the what-if, then provision once from the same template after your yes.
6. `Secrets`: set `jwt-signing-key` in the vault (it gives you the Key Vault Secrets Officer role first). The step also sets the Key Vault reference again, because App Service keeps a reference that failed to resolve (the secret did not exist yet) and a restart does not retry it.
7. Run the deployment workflow, then `Verify`.

`Teardown` deletes the Log Analytics workspace with force, the resource group, purges the soft-deleted vault and deletes the app registration, so a rebuild can reuse the names.

## Demo data

`infra/seed-demo-data.ps1` fills the deployed demo with claims for testing lists, filters, paging and the reserve workflow. It is run by hand and is not a migration: it signs in as the three demo users and calls the API, so every claim passes the Domain rules and carries its audit entries, issues and balances.

- The data is random but repeatable (`-Seed`; `-Count` claims, default 100). Claims are created in loss-date order, so claim numbers rise with the loss dates; every status appears in a normal split leaning by age; handler, policy, cause, parties, risk objects and amounts are chosen independently.
- Each claim gets zero to three reserve components with additions, adjustments and reversals in mixed approval states, and some get PDF documents (which exercise the Blob adapter) and notes.
- Some claims carry validation issues: a policy that does not cover the loss date (acknowledged before opening, or left raised on a draft), a too short description or a loss date in the future (critical, waived by a supervisor, or left raised on a draft), an unknown cause of loss or no claimant (critical, cannot be waived, so the claim stays a draft) and no risk object (a warning that stays raised).
- Reserves are tested at their limits: three claims reach the 10 million claim limit (an approval that would pass it is refused and the row stays waiting) and five sit at the approval tiers (10,000 approves itself, 100,000 needs a supervisor, 100,050 a manager). Normal claims leave rows approved, rejected, retracted or waiting. The run ends with a summary of issues, row states and totals read back from the API, and lists the refusals it expected.
- Status moves run in a second phase after a pause, because closing is refused while an approved row is still being posted. A refused step is counted and shown without stopping the run.
- It stops when claims already exist unless `-Force` is given. To start from nothing, delete the Azure SQL database (soft delete only hides rows) and run the Deployment workflow, which recreates it, migrates it and grants the web app its user.

## Accepted trade-offs

- The deploy identity administers the database.
- The web app's database user can change the schema (`db_ddladmin`), still without update or delete on the audit log.
- SQL, storage and Key Vault accept public connections, protected by identity, keys and the firewall rather than network isolation.
- The Hangfire dashboard is on at `/hangfire`, open to every request and able to trigger, requeue and delete jobs, because a browser page cannot send the bearer token. The jobs are idempotent and the posting sweep re-enqueues a deleted posting, so the cost is noise, not data loss; it must not stay on if the environment ever holds real data.
- The dev sign-in is on and open, as the specification's mock authentication: anyone who can reach the API can sign in as any configured user, on fake data.
- The API reads the storage account key, which reaches the whole account.

## Provides and expects

- **Provides:** the public demo (the static web app and the API with Swagger), the managed identity as the database user and secret reader, the Key Vault references, the database role membership, and a tested artifact per run.
- **Expects from the API:** `/health/ready` reachable without a token, forwarded headers honoured, settings validated at startup (a missing setting stops the app and names the key), the dev token route (`docs/layers/api.md`).
- **Expects from Persistence:** migrations runnable as a bundle, the `claims_app` role from the migrations and the seeded organisation of `DevAuth__OrganisationId` (`docs/layers/persistence.md`).
- **Expects from the frontend:** the deployment contract in `docs/layers/frontend.md` 'Provides and expects': the build output folder, the `apiBaseUrl` line the pipeline replaces and `public/staticwebapp.config.json`.
