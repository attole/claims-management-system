# Deployment standards

How the Bicep templates, the scripts and the workflows are written, in `infra/` and `.github/workflows/`: the counterpart of `docs/BACKEND-STANDARDS.md` and `docs/FRONTEND-STANDARDS.md`. It is shorter, because the environment itself (resources, settings, pipelines, trade-offs) is described in `docs/layers/deployment.md`. `.claude/CLAUDE.md` points here; read this file before writing or reviewing a template, script or workflow. Every Azure command and every deployment run asks first (CLAUDE.md 'Permissions model').

## Bicep

- `main.bicep` is at resource group scope: it composes the modules, builds the app settings and outputs names and hostnames only. One module per resource family in `modules/`. Environment values (region, SKUs, switches, dev users) live in `demo.bicepparam`, never a secret.
- Every parameter has a `@description`, and every resource declares its API version.
- Names are `<kind>-claims-demo-<suffix>`, the suffix taken from the resource group id; the storage account, which allows no hyphens, is `stclaimsdemo<suffix>`. Every resource carries the `application` and `environment` tags.
- No secret is a parameter or an output; a secret reaches the app as a Key Vault reference. The one exception inside the template is the Application Insights connection string passed from the monitoring module to the web app; it is not a root output.
- The template writes every app setting, with `__` for `:`. A feature switch is a boolean parameter `is<Feature>Enabled`, written as `true` or `false`.

## Scripts

- PowerShell for Windows PowerShell 5.1, ASCII only, no secrets. A step looks up what it needs, so it can be repeated: `bootstrap.ps1` runs one step per call (`-Step <name>`), and the teardown is a step of its own.
- SQL scripts are idempotent (`grant-app-user.sql`), and a verify script fails the run when the result is wrong.
- `seed-demo-data.ps1` goes through the API as the demo users, never to the database. A refused step is counted and listed and the run goes on; it stops when claims exist unless `-Force`.
- A function that returns one item can hand back the bare item, so a result that must be a list is wrapped in `@()`.

## Workflows

- Actions are pinned to commit SHAs. Workflow permissions are `contents: read` and `actions: read`, plus `id-token: write` on the deploy job only. Sign-in is GitHub OIDC with a federated credential; the repository variables hold ids, never secrets.
- Build and Test runs on every pull request and push to main, and a superseded run is cancelled. Deployment is manual (`workflow_dispatch`) in the environment `demo`, never builds, and never cancels another deployment.
- A deployment ends with a smoke test, and a failure stops the run.
