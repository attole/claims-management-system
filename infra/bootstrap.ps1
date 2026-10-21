<#
.SYNOPSIS
    One-time Azure and GitHub setup for the claims demo, and its teardown.

.DESCRIPTION
    Run it yourself in Windows PowerShell after `az login` and `gh auth login`.
    Each step is separate and can be repeated: a step looks up what it needs from
    Azure and GitHub instead of relying on variables from an earlier step, so a
    new window or a retry after an error is safe.

    Order:
      Check         register the resource providers and check the region
      ResourceGroup create the resource group
      Identity      create the deploy identity for GitHub (OIDC, no secret)
      GitHub        create the GitHub environment, its rules and its variables
      Provision     what-if, then deploy infra/main.bicep (asks before deploying)
      Secrets       put the JWT signing key and the dev sign-in access code in Key Vault
      Verify        call the deployed API
      Teardown      delete everything (asks you to type the resource group name)

    The deployment workflow (.github/workflows/deployment.yml) takes over after
    these steps: it provisions again (a no-op), applies the migrations, creates
    the web app's database user, deploys the API and smoke-tests it.

    Accepted trade-offs for the demo:
      - The deploy identity is the SQL administrator, so the pipeline can
        administer the database.
      - The web app's database user is in db_ddladmin because Hangfire creates
        its own tables on first start. The audit table's update and delete deny
        still holds (the workflow proves it), but a compromised app could change
        the schema.
      - The SQL firewall allows Azure services, and the vault and storage accept
        public connections, protected by identity and keys, not by the network.
      - The dev sign-in is on: anyone with the API URL can get a token for any of
        the three roles. The data is fake and the Hangfire dashboard stays off.
      - The API reads the storage account key, which reaches the whole account.
        Identity-based access would need user-delegation links (a code change).

.PARAMETER Step
    The step to run, see the order above.

.PARAMETER Rotate
    Secrets only: replace the secrets even when they already exist. Every issued
    token stops validating, the old access code stops working, and the web app is
    restarted.

.PARAMETER AccessCode
    Secrets only: the access code for reviewers (16 or more characters). A random
    one is generated and shown once when this is left out.

.EXAMPLE
    .\infra\bootstrap.ps1 -Step Check
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Check', 'ResourceGroup', 'Identity', 'GitHub', 'Provision', 'Secrets', 'Verify', 'Teardown')]
    [string]$Step,

    [switch]$Rotate,

    [string]$AccessCode
)

# Native commands report failure through the exit code, which is checked after each call.
$ErrorActionPreference = 'Continue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$Location = 'polandcentral'
$LocationDisplayName = 'Poland Central'
$ResourceGroup = 'rg-claims-demo'
$DeployAppName = 'claims-github-deploy'
$EnvironmentName = 'demo'
$DeploymentName = 'provisioning'
$JwtSecretName = 'jwt-signing-key'
$DevAuthSecretName = 'devauth-secret'
$ParamFile = Join-Path $PSScriptRoot 'demo.bicepparam'

$Providers = @(
    'Microsoft.Web', 'Microsoft.Sql', 'Microsoft.Storage', 'Microsoft.KeyVault',
    'Microsoft.OperationalInsights', 'Microsoft.Insights', 'Microsoft.Authorization',
    'Microsoft.ManagedIdentity'
)

function Assert-Success([string]$What) {
    if ($LASTEXITCODE -ne 0) {
        throw "$What failed (exit code $LASTEXITCODE)."
    }
}

function Get-SubscriptionId {
    $id = az account show --query id -o tsv
    Assert-Success 'Reading the Azure subscription (run az login first)'
    return $id
}

function Get-GithubRepo {
    $repo = gh repo view --json nameWithOwner --jq .nameWithOwner
    Assert-Success 'Reading the GitHub repository (run gh auth login and run this from a clone)'
    return $repo
}

function Get-DeployIdentity {
    $appId = az ad app list --display-name $DeployAppName --query '[0].appId' -o tsv
    Assert-Success 'Looking up the deploy app registration'
    if (-not $appId) {
        throw "The app registration $DeployAppName does not exist: run -Step Identity first."
    }

    $objectId = az ad sp list --filter "appId eq '$appId'" --query '[0].id' -o tsv
    Assert-Success 'Looking up the deploy service principal'
    if (-not $objectId) {
        throw "The service principal of $DeployAppName does not exist: run -Step Identity first."
    }

    return @{ AppId = $appId; ObjectId = $objectId }
}

function Get-DeploymentOutputs {
    $json = az deployment group show --resource-group $ResourceGroup --name $DeploymentName --query properties.outputs -o json
    Assert-Success 'Reading the deployment outputs (run -Step Provision first)'
    return ($json | ConvertFrom-Json)
}

function Test-RegionOffered([string]$Namespace, [string]$ResourceType) {
    $locations = az provider show --namespace $Namespace --query "resourceTypes[?resourceType=='$ResourceType'].locations[]" -o tsv
    return [bool]($locations | Select-String -SimpleMatch $LocationDisplayName)
}

function Invoke-Check {
    # Registering a provider only enables its resource types on the subscription. It is free.
    foreach ($namespace in $Providers) {
        az provider register --namespace $namespace | Out-Null
        Assert-Success "Registering $namespace"
    }

    # Each of these must be True, otherwise pick another region here and in demo.bicepparam.
    "Sql servers offered in ${LocationDisplayName}: " + (Test-RegionOffered 'Microsoft.Sql' 'servers')
    "Storage accounts offered in ${LocationDisplayName}: " + (Test-RegionOffered 'Microsoft.Storage' 'storageAccounts')
    "Key vaults offered in ${LocationDisplayName}: " + (Test-RegionOffered 'Microsoft.KeyVault' 'vaults')
    $b1 = az appservice list-locations --sku B1 --linux-workers-enabled --query "[?name=='$LocationDisplayName'].name" -o tsv
    "B1 Linux plans offered in ${LocationDisplayName}: " + [bool]$b1

    # Registration runs in the background. Every provider must say Registered before -Step Provision.
    foreach ($namespace in $Providers) {
        "$namespace : " + (az provider show --namespace $namespace --query registrationState -o tsv)
    }

    # Availability is not quota. A free-trial subscription can have no quota for Basic plans in a region;
    # that only shows when the deployment runs, as an error about additional quota.
}

function Invoke-ResourceGroup {
    az group create --name $ResourceGroup --location $Location | Out-Null
    Assert-Success 'Creating the resource group'
    "Resource group $ResourceGroup is ready in $Location."
}

function Invoke-Identity {
    # The app registration is the identity's definition (like an IAM role). The service principal is its login.
    $appId = az ad app list --display-name $DeployAppName --query '[0].appId' -o tsv
    Assert-Success 'Looking up the app registration'
    if (-not $appId) {
        $appId = az ad app create --display-name $DeployAppName --query appId -o tsv
        Assert-Success 'Creating the app registration'
    }

    $objectId = az ad sp list --filter "appId eq '$appId'" --query '[0].id' -o tsv
    Assert-Success 'Looking up the service principal'
    if (-not $objectId) {
        az ad sp create --id $appId | Out-Null
        Assert-Success 'Creating the service principal'
        $objectId = az ad sp list --filter "appId eq '$appId'" --query '[0].id' -o tsv
        Assert-Success 'Looking up the service principal'
    }

    # The federated credential trusts GitHub tokens only for this repository, and only for jobs that run in
    # the GitHub environment named demo. A pull request from a fork cannot obtain such a token.
    # GitHub puts the numeric owner and repository ids into the subject (repo:owner@id/name@id:environment:demo),
    # so a renamed or re-created repository can never match an old credential. Azure compares it exactly.
    $repo = Get-GithubRepo
    $ownerName = gh api "repos/$repo" --jq .owner.login
    Assert-Success 'Reading the repository owner'
    $ownerId = gh api "repos/$repo" --jq .owner.id
    Assert-Success 'Reading the repository owner id'
    $repoName = gh api "repos/$repo" --jq .name
    Assert-Success 'Reading the repository name'
    $repoId = gh api "repos/$repo" --jq .id
    Assert-Success 'Reading the repository id'
    $subject = "repo:$($ownerName)@$($ownerId)/$($repoName)@$($repoId):environment:$EnvironmentName"

    $credentialName = 'github-demo-environment'
    $credential = @{
        name      = $credentialName
        issuer    = 'https://token.actions.githubusercontent.com'
        subject   = $subject
        audiences = @('api://AzureADTokenExchange')
    } | ConvertTo-Json -Compress
    $credentialFile = Join-Path $env:TEMP 'federated-credential.json'
    [System.IO.File]::WriteAllText($credentialFile, $credential)

    $currentSubject = az ad app federated-credential list --id $appId --query "[?name=='$credentialName'].subject | [0]" -o tsv
    Assert-Success 'Listing the federated credentials'
    if (-not $currentSubject) {
        az ad app federated-credential create --id $appId --parameters $credentialFile | Out-Null
    }
    elseif ($currentSubject -ne $subject) {
        az ad app federated-credential update --id $appId --federated-credential-id $credentialName --parameters $credentialFile | Out-Null
    }
    $saved = $LASTEXITCODE
    Remove-Item $credentialFile
    if ($saved -ne 0) {
        throw 'Saving the federated credential failed.'
    }

    # Contributor creates and changes the resources. Contributor cannot create role assignments, and the
    # template has one (the web app identity reading Key Vault secrets), so the second role is needed.
    # Both are scoped to the resource group, not the subscription.
    $scope = az group show --name $ResourceGroup --query id -o tsv
    Assert-Success 'Reading the resource group (run -Step ResourceGroup first)'
    foreach ($role in 'Contributor', 'Role Based Access Control Administrator') {
        az role assignment create --assignee-object-id $objectId --assignee-principal-type ServicePrincipal --role $role --scope $scope | Out-Null
        Assert-Success "Assigning the role $role"
    }

    "Deploy identity $DeployAppName is ready (client id $appId)."
}

function Invoke-GitHub {
    $repo = Get-GithubRepo
    $identity = Get-DeployIdentity
    $subscriptionId = Get-SubscriptionId
    $tenantId = az account show --query tenantId -o tsv
    Assert-Success 'Reading the tenant'

    # The environment holds the Azure ids as variables (they are ids, not secrets). Its rules: only main may
    # deploy to it, and you must approve each deployment. The repository is public, so the deployment
    # workflow runs on workflow_dispatch only, never on a pull request.
    $userId = gh api user --jq .id
    Assert-Success 'Reading your GitHub user id'
    $environment = @{
        reviewers                = @(@{ type = 'User'; id = [int]$userId })
        deployment_branch_policy = @{ protected_branches = $false; custom_branch_policies = $true }
    } | ConvertTo-Json -Depth 5 -Compress
    $environment | gh api --method PUT "repos/$repo/environments/$EnvironmentName" --input - | Out-Null
    Assert-Success 'Creating the GitHub environment'

    $policies = gh api "repos/$repo/environments/$EnvironmentName/deployment-branch-policies" --jq '.branch_policies[].name'
    Assert-Success 'Listing the deployment branch policies'
    if ($policies -notcontains 'main') {
        gh api --method POST "repos/$repo/environments/$EnvironmentName/deployment-branch-policies" -f name=main | Out-Null
        Assert-Success 'Restricting deployments to main'
    }

    $variables = [ordered]@{
        AZURE_CLIENT_ID       = $identity.AppId
        AZURE_TENANT_ID       = $tenantId
        AZURE_SUBSCRIPTION_ID = $subscriptionId
        AZURE_RESOURCE_GROUP  = $ResourceGroup
        SQL_ADMIN_OBJECT_ID   = $identity.ObjectId
        SQL_ADMIN_LOGIN       = $DeployAppName
    }
    foreach ($name in $variables.Keys) {
        gh variable set $name --env $EnvironmentName --repo $repo --body $variables[$name]
        Assert-Success "Setting the variable $name"
    }

    "GitHub environment $EnvironmentName is ready in $repo."
}

function Invoke-Provision {
    $identity = Get-DeployIdentity

    # demo.bicepparam reads the two SQL administrator values from the environment.
    $env:SQL_ADMIN_OBJECT_ID = $identity.ObjectId
    $env:SQL_ADMIN_LOGIN = $DeployAppName

    az deployment group what-if --resource-group $ResourceGroup --parameters $ParamFile
    Assert-Success 'The what-if'

    $answer = Read-Host 'Deploy these changes? Type yes'
    if ($answer -ne 'yes') {
        'Stopped before deploying.'
        return
    }

    az deployment group create --resource-group $ResourceGroup --name $DeploymentName --parameters $ParamFile | Out-Null
    Assert-Success 'The deployment'

    # The frontend needs staticWebAppHostName and webAppHostName from these outputs.
    Get-DeploymentOutputs | Format-List
}

function New-RandomText([int]$Length) {
    # Letters and digits without look-alike characters, so the access code can be read out and typed.
    $alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789'
    $bytes = New-Object byte[] $Length
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $generator.GetBytes($bytes)
    $text = New-Object System.Text.StringBuilder
    foreach ($byte in $bytes) {
        [void]$text.Append($alphabet[$byte % $alphabet.Length])
    }

    return $text.ToString()
}

function New-JwtSigningKey {
    $bytes = New-Object byte[] 48
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $generator.GetBytes($bytes)

    return [Convert]::ToBase64String($bytes)
}

function Invoke-Secrets {
    # Two secrets live only in Key Vault; the web app reads them through Key Vault references, resolved when the
    # app starts:
    #   jwt-signing-key  the key that signs the bearer tokens (the API needs 32 or more characters)
    #   devauth-secret   the access code reviewers type to sign in with the dev token (16 or more characters)
    $vault = (Get-DeploymentOutputs).keyVaultName.value
    $vaultId = az keyvault show --name $vault --query id -o tsv
    Assert-Success 'Reading the key vault'

    # Reading or writing secrets is data-plane access, so you need the role before the first list. The role can
    # take a few minutes to take effect, so a forbidden answer is retried.
    $myObjectId = az ad signed-in-user show --query id -o tsv
    Assert-Success 'Reading your Entra user'
    az role assignment create --assignee-object-id $myObjectId --assignee-principal-type User --role 'Key Vault Secrets Officer' --scope $vaultId | Out-Null
    Assert-Success 'Assigning yourself Key Vault Secrets Officer'

    $existing = $null
    $listed = $false
    for ($attempt = 1; $attempt -le 10 -and -not $listed; $attempt++) {
        $existing = az keyvault secret list --vault-name $vault --query '[].name' -o tsv
        if ($LASTEXITCODE -eq 0) {
            $listed = $true
        }
        else {
            "Attempt $attempt failed, waiting 30 seconds for the role to apply."
            Start-Sleep -Seconds 30
        }
    }
    if (-not $listed) {
        throw 'Listing the key vault secrets failed: the role did not apply in time. Run the step again.'
    }

    if (-not $AccessCode) {
        $AccessCode = New-RandomText 24
    }
    $secrets = @(
        @{ Name = $JwtSecretName; Value = (New-JwtSigningKey) },
        @{ Name = $DevAuthSecretName; Value = $AccessCode }
    )

    $written = @()
    foreach ($secret in $secrets) {
        if (($existing -contains $secret.Name) -and -not $Rotate) {
            "$($secret.Name) already exists. Use -Rotate to replace it."
            continue
        }

        $file = Join-Path $env:TEMP "$($secret.Name).txt"
        [System.IO.File]::WriteAllText($file, $secret.Value)
        $saved = $false
        for ($attempt = 1; $attempt -le 10 -and -not $saved; $attempt++) {
            az keyvault secret set --vault-name $vault --name $secret.Name --file $file --encoding utf-8 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                $saved = $true
            }
            else {
                "Attempt $attempt failed, waiting 30 seconds."
                Start-Sleep -Seconds 30
            }
        }
        Remove-Item $file
        if (-not $saved) {
            throw "Saving the secret $($secret.Name) failed."
        }
        $written += $secret.Name
    }

    if ($Rotate -and $written.Count -gt 0) {
        $app = (Get-DeploymentOutputs).webAppName.value
        az webapp restart --resource-group $ResourceGroup --name $app
        Assert-Success 'Restarting the web app'
    }

    if ($written -contains $DevAuthSecretName) {
        "The access code for reviewers is: $AccessCode"
        'Keep it: it is the only time it is shown. Read it later with: az keyvault secret show --vault-name <vault> --name devauth-secret --query value -o tsv'
    }
    'The storage connection string is already in the vault: the template wrote it, so no one handles the account key.'
}

function Invoke-Verify {
    $outputs = Get-DeploymentOutputs
    $hostName = $outputs.webAppHostName.value
    $app = $outputs.webAppName.value

    (Invoke-WebRequest -UseBasicParsing "https://$hostName/health/ready").StatusCode
    (Invoke-WebRequest -UseBasicParsing "https://$hostName/openapi/v1.json").StatusCode

    $userId = az webapp config appsettings list --resource-group $ResourceGroup --name $app --query "[?name=='DevAuth__Users__0__Id'].value | [0]" -o tsv
    Assert-Success 'Reading the dev user id'
    $vault = $outputs.keyVaultName.value
    $accessCode = az keyvault secret show --vault-name $vault --name $DevAuthSecretName --query value -o tsv
    Assert-Success 'Reading the access code'
    $body = @{ userId = $userId } | ConvertTo-Json -Compress
    $signInHeaders = @{ 'X-DevAuth-Secret' = $accessCode }
    $token = (Invoke-RestMethod -Method Post -Uri "https://$hostName/api/dev/token" -Headers $signInHeaders -ContentType 'application/json' -Body $body).token
    $headers = @{ Authorization = "Bearer $token" }
    (Invoke-WebRequest -UseBasicParsing -Headers $headers "https://$hostName/api/claims").StatusCode
}

function Invoke-Teardown {
    # A deleted key vault stays soft-deleted for 7 days and a deleted Log Analytics workspace for 14, and both
    # block the same name, which a recreated environment would reuse. So the vault is purged and the workspace
    # is force-deleted. Names are read before the resource group goes.
    $answer = Read-Host "This deletes everything in $ResourceGroup. Type the resource group name to continue"
    if ($answer -ne $ResourceGroup) {
        'Stopped.'
        return
    }

    $vault = (Get-DeploymentOutputs).keyVaultName.value
    $workspace = az monitor log-analytics workspace list --resource-group $ResourceGroup --query '[0].name' -o tsv
    Assert-Success 'Listing the workspaces'

    az monitor log-analytics workspace delete --resource-group $ResourceGroup --workspace-name $workspace --force true --yes
    Assert-Success 'Deleting the workspace'
    az group delete --name $ResourceGroup --yes
    Assert-Success 'Deleting the resource group'
    az keyvault purge --name $vault --location $Location
    Assert-Success 'Purging the key vault'

    $appId = az ad app list --display-name $DeployAppName --query '[0].appId' -o tsv
    if ($appId) {
        az ad app delete --id $appId
        Assert-Success 'Deleting the app registration'
    }

    'Everything is deleted. The GitHub environment and its variables remain; delete them in the repository settings if you do not need them.'
}

switch ($Step) {
    'Check' { Invoke-Check }
    'ResourceGroup' { Invoke-ResourceGroup }
    'Identity' { Invoke-Identity }
    'GitHub' { Invoke-GitHub }
    'Provision' { Invoke-Provision }
    'Secrets' { Invoke-Secrets }
    'Verify' { Invoke-Verify }
    'Teardown' { Invoke-Teardown }
}
