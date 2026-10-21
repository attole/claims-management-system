# Azure bootstrap and teardown runbook

The one-time steps that create the resource group, the deploy identity and the secrets before the deployment workflow can take over. Run them yourself from the repository root in a Bash shell after `az login`. Nothing here is run by the build or the deployment workflow.

What the deployment workflow does on every run (not part of this runbook): provisions the resources from `infra/main.bicep`, checks the access code is in the vault, applies the migrations, creates the web app's database user with `infra/sql/grant-app-user.sql`, deploys the API and smoke-tests it, then builds the frontend against the API and uploads it to the static web app.

-

## Values used below

```bash
SUBSCRIPTION_ID=<your subscription id>
LOCATION=polandcentral
RESOURCE_GROUP=rg-claims-demo
GITHUB_REPO=attole/claims-management-system
DEPLOY_APP_NAME=claims-github-deploy
```

The ids this runbook produces are not secrets. There are three secrets. The JWT signing key and the storage account key never appear in a command line, a GitHub variable or a log. The dev sign-in access code is read by the workflow (masked in its logs) and given to the reviewers on purpose.

-

## 1. Check the subscription, providers and region

```bash
az login
az account set --subscription "$SUBSCRIPTION_ID"

for namespace in Microsoft.Web Microsoft.Sql Microsoft.Storage Microsoft.KeyVault \
    Microsoft.OperationalInsights Microsoft.Insights Microsoft.Authorization; do
  az provider register --namespace "$namespace"
done

az provider show --namespace Microsoft.Sql --query "resourceTypes[?resourceType=='servers'].locations[]" -o tsv | grep -i poland
az provider show --namespace Microsoft.Storage --query "resourceTypes[?resourceType=='storageAccounts'].locations[]" -o tsv | grep -i poland
az provider show --namespace Microsoft.KeyVault --query "resourceTypes[?resourceType=='vaults'].locations[]" -o tsv | grep -i poland
az appservice list-locations --sku B1 --linux-workers-enabled --query "[?name=='Poland Central']" -o tsv
```

Each query must print Poland Central. If one does not, pick the nearest region that shows in all four and set it in `LOCATION` and in `infra/demo.bicepparam`.

Availability is not quota. A free-trial subscription can have no quota for Basic App Service plans in a region, which only shows when the deployment runs, as an error about additional quota. If that happens, request quota in the portal or try another region.

-

## 2. Resource group

```bash
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
```

-

## 3. Deploy identity for GitHub (OIDC, no secret)

```bash
APP_ID=$(az ad app create --display-name "$DEPLOY_APP_NAME" --query appId -o tsv)
az ad sp create --id "$APP_ID"
SP_OBJECT_ID=$(az ad sp show --id "$APP_ID" --query id -o tsv)

CREDENTIAL_FILE=$(mktemp)
cat > "$CREDENTIAL_FILE" <<EOF
{
  "name": "github-demo-environment",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:${GITHUB_REPO}:environment:demo",
  "audiences": ["api://AzureADTokenExchange"]
}
EOF
az ad app federated-credential create --id "$APP_ID" --parameters "$CREDENTIAL_FILE"

RESOURCE_GROUP_ID=$(az group show --name "$RESOURCE_GROUP" --query id -o tsv)
az role assignment create --assignee-object-id "$SP_OBJECT_ID" --assignee-principal-type ServicePrincipal \
  --role Contributor --scope "$RESOURCE_GROUP_ID"
az role assignment create --assignee-object-id "$SP_OBJECT_ID" --assignee-principal-type ServicePrincipal \
  --role "Role Based Access Control Administrator" --scope "$RESOURCE_GROUP_ID"
```

- The federated credential accepts a GitHub token only for this repository and only when the job runs in the GitHub environment named `demo`. A pull request from a fork cannot obtain it.
- Contributor creates and changes the resources. Contributor cannot create role assignments, so the second role is needed for the template's two assignments (the web app identity and the deploy identity reading Key Vault secrets). Both roles are scoped to the resource group, not the subscription.
- The same identity becomes the SQL server's Entra administrator through the template, so the workflow can run the migrations without a SQL password. This gives the pipeline full control of the database; it is accepted for a demo.

-

## 4. GitHub environment and variables

These are ids, not secrets, so they are variables.

```bash
gh api --method PUT "repos/$GITHUB_REPO/environments/demo"

gh variable set AZURE_CLIENT_ID --env demo --repo "$GITHUB_REPO" --body "$APP_ID"
gh variable set AZURE_TENANT_ID --env demo --repo "$GITHUB_REPO" --body "$(az account show --query tenantId -o tsv)"
gh variable set AZURE_SUBSCRIPTION_ID --env demo --repo "$GITHUB_REPO" --body "$SUBSCRIPTION_ID"
gh variable set AZURE_RESOURCE_GROUP --env demo --repo "$GITHUB_REPO" --body "$RESOURCE_GROUP"
gh variable set SQL_ADMIN_OBJECT_ID --env demo --repo "$GITHUB_REPO" --body "$SP_OBJECT_ID"
gh variable set SQL_ADMIN_LOGIN --env demo --repo "$GITHUB_REPO" --body "$DEPLOY_APP_NAME"
```

Then, in the repository settings under Environments, open `demo`, limit deployment branches to `main` and add yourself as a required reviewer (both are free on a public repository). The repository is public, so the deployment workflow must only ever run on `workflow_dispatch`, never on a pull request.

-

## 5. First provisioning (manual, once)

Run the same template the workflow will run, so the vault exists before the first workflow run and the first smoke test can pass.

```bash
export SQL_ADMIN_OBJECT_ID="$SP_OBJECT_ID"
export SQL_ADMIN_LOGIN="$DEPLOY_APP_NAME"

az deployment group what-if --resource-group "$RESOURCE_GROUP" --parameters infra/demo.bicepparam
az deployment group create --resource-group "$RESOURCE_GROUP" --name first-provisioning \
  --parameters infra/demo.bicepparam
az deployment group show --resource-group "$RESOURCE_GROUP" --name first-provisioning \
  --query properties.outputs
```

Read the what-if output before the create. The outputs give the web app, the SQL server, the vault and the static web app names and hostnames; the frontend session needs `staticWebAppHostName` and `webAppHostName`.

-

## 6. JWT signing key and dev sign-in access code

The API needs `Jwt:SigningKey` (32 or more characters) and, with the dev sign-in on, `DevAuth:Secret` (16 or more characters). Both live only in Key Vault; the web app reads them through Key Vault references. A reference to a secret that does not exist reaches the app as the reference text, so the app would start with a wrong value; the deployment workflow reads the access code first and stops before releasing when it is missing.

```bash
VAULT=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name first-provisioning \
  --query properties.outputs.keyVaultName.value -o tsv)
VAULT_ID=$(az keyvault show --name "$VAULT" --query id -o tsv)
MY_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)

az role assignment create --assignee-object-id "$MY_OBJECT_ID" --assignee-principal-type User \
  --role "Key Vault Secrets Officer" --scope "$VAULT_ID"

KEY_FILE=$(mktemp)
openssl rand -base64 48 | tr -d '\n' > "$KEY_FILE"
az keyvault secret set --vault-name "$VAULT" --name jwt-signing-key --file "$KEY_FILE" --encoding utf-8
rm "$KEY_FILE"

CODE_FILE=$(mktemp)
openssl rand -base64 24 | tr -d '\n/+=' > "$CODE_FILE"
az keyvault secret set --vault-name "$VAULT" --name devauth-secret --file "$CODE_FILE" --encoding utf-8
rm "$CODE_FILE"
```

The role assignment can take a few minutes to take effect; repeat the `secret set` if it answers forbidden. To rotate either value, repeat its commands and restart the web app (`az webapp restart`); a new JWT key invalidates every issued token, a new access code is asked again by the frontend.

Give the reviewers the access code (`az keyvault secret show --vault-name "$VAULT" --name devauth-secret --query value -o tsv`); the frontend asks for it once per browser session.

The storage connection string is already in the vault: the template wrote it with `listKeys()`, so no one handles the account key.

-

## 7. First deployment

Run the deployment workflow from the Actions tab or with `gh workflow run deployment.yml`. Its order is provisioning (a no-op after step 5), migrations, the grant of the web app's database user, the API deployment with a restart, the smoke test, and the frontend build and upload to the static web app. The restart is what makes the app pick up the secrets and the role assignment, because Key Vault references are resolved when the app starts.

The web app runs as a user-assigned managed identity created by the template, not as a system-assigned one. Its client id is readable from Azure Resource Manager, which the workflow needs to create the database user without any Microsoft Graph permission, and it survives a recreated web app, so the database user stays valid.

The database steps need the SQL administrator (the deploy identity), so they cannot be run by your own account unless you add yourself as an administrator.

-

## Verify

- `GET https://<webAppHostName>/health/ready` answers 200.
- `GET https://<webAppHostName>/openapi/v1.json` and `/swagger` answer 200.
- `POST /api/dev/token` with a user id from `infra/demo.bicepparam` and the header `X-DevAuth-Secret: <access code>` returns a token, and `GET /api/claims` with it answers 200.
- `https://<staticWebAppHostName>` opens the claims list after the access code is entered.
- `infra/sql/verify-app-user.sql` (run by the workflow with the managed identity's name) proves the web app's database user can read and append audit rows but cannot update or delete them.

-

## Teardown

Everything lives in one resource group. Two resources outlive it unless removed on purpose: a deleted Key Vault stays soft-deleted for 7 days and a deleted Log Analytics workspace for 14, and both block the same name, which a recreated environment would reuse.

```bash
VAULT=<key vault name>
WORKSPACE=$(az monitor log-analytics workspace list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)

az monitor log-analytics workspace delete --resource-group "$RESOURCE_GROUP" --workspace-name "$WORKSPACE" \
  --force true --yes
az group delete --name "$RESOURCE_GROUP" --yes
az keyvault purge --name "$VAULT" --location "$LOCATION"
az ad app delete --id "$APP_ID"
```

-

## Accepted trade-offs for the demo

- The deploy identity is the SQL administrator, so the pipeline can administer the database.
- The web app's database user is in `db_ddladmin` because Hangfire creates its own tables on first start; the audit table's update and delete deny still holds, and the verify script proves it, but a compromised app could change the schema.
- The SQL firewall allows Azure services and the vault and storage accept public connections, protected by identity and keys, not by network isolation.
- The dev sign-in is on behind the access code, so anyone who holds the code can obtain a token for any of the three roles. The data is fake, the Hangfire dashboard stays off and the repository documents the exception.
- The deploy identity can read the vault's secrets, which the smoke test needs for the access code.
- The API reads the storage account key, which reaches the whole account. Identity-based access would need user-delegation links, a code change.
