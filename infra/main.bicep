targetScope = 'resourceGroup'

@description('Azure region for every resource except the static web app.')
param location string = resourceGroup().location

@description('Region of the static web app; only a few regions are offered.')
param staticWebAppLocation string = 'centralus'

@description('Short name that starts every resource name.')
param namePrefix string = 'claims'

@description('Environment name, part of every resource name.')
param environmentName string = 'demo'

@description('App Service plan size; Basic or higher is needed for Always On.')
param appServiceSkuName string = 'B1'

@description('Object id of the Entra service principal that administers the SQL server and runs the migrations.')
param sqlAdminObjectId string

@description('Display name of that service principal.')
param sqlAdminLogin string

@description('Seeded organisation id the dev sign-in users belong to.')
param devAuthOrganisationId string

@description('Dev sign-in users as objects with id, name and role.')
param devAuthUsers array

@description('Serves the OpenAPI document and Swagger UI.')
param isSwaggerEnabled bool = true

@description('Mounts the Hangfire dashboard, which is open to every request; keep it off when deployed.')
param isHangfireDashboardEnabled bool = false

@description('Schedules the job that deletes stored blobs no document refers to.')
param isBlobSweepEnabled bool = false

@description('Serves the simulated sign-in endpoint that issues a token for a configured user.')
param isDevAuthEnabled bool = true

@description('Name of the Key Vault secret that holds the JWT signing key; set by the bootstrap runbook.')
param jwtSecretName string = 'jwt-signing-key'

@description('Name of the Key Vault secret that holds the access code of the dev sign-in; set by the bootstrap runbook.')
param devAuthSecretName string = 'devauth-secret'

var baseName = '${namePrefix}-${environmentName}'
var suffix = take(uniqueString(resourceGroup().id), 6)
var tags = {
  application: 'claims-management-system'
  environment: environmentName
}
var databaseName = 'claims'
var storageSecretName = 'storage-connection-string'

module identity 'modules/identity.bicep' = {
  name: 'identity'
  params: {
    name: 'id-${baseName}-${suffix}'
    location: location
    tags: tags
  }
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    name: 'log-${baseName}-${suffix}'
    location: location
    tags: tags
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    name: toLower('st${namePrefix}${environmentName}${suffix}')
    location: location
    tags: tags
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    name: 'kv-${baseName}-${suffix}'
    location: location
    tags: tags
    storageAccountName: storage.outputs.name
    storageSecretName: storageSecretName
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    serverName: 'sql-${baseName}-${suffix}'
    databaseName: databaseName
    location: location
    tags: tags
    adminObjectId: sqlAdminObjectId
    adminLogin: sqlAdminLogin
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticwebapp'
  params: {
    name: 'swa-${baseName}-${suffix}'
    location: staticWebAppLocation
    tags: tags
  }
}

var platformSettings = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
]

var wiringSettings = [
  { name: 'Jwt__SigningKey', value: '@Microsoft.KeyVault(SecretUri=${keyVault.outputs.uri}secrets/${jwtSecretName}/)' }
  {
    name: 'ConnectionStrings__ClaimsDb'
    value: 'Server=tcp:${sql.outputs.fullyQualifiedDomainName},1433;Database=${sql.outputs.databaseName};Authentication=Active Directory Managed Identity;User Id=${identity.outputs.clientId};Encrypt=True;'
  }
  { name: 'Cors__AllowedOrigins__0', value: 'https://${staticWebApp.outputs.defaultHostname}' }
  { name: 'StorageProvider', value: 'AzureBlob' }
  { name: 'AzureConnectionString', value: '@Microsoft.KeyVault(SecretUri=${keyVault.outputs.uri}secrets/${storageSecretName}/)' }
]

var featureSettings = [
  { name: 'Swagger__Enabled', value: toLower(string(isSwaggerEnabled)) }
  { name: 'Hangfire__Dashboard__Enabled', value: toLower(string(isHangfireDashboardEnabled)) }
  { name: 'Hangfire__BlobSweep__Enabled', value: toLower(string(isBlobSweepEnabled)) }
  { name: 'DevAuth__Enabled', value: toLower(string(isDevAuthEnabled)) }
]

var devAuthSettingsPerUser = [
  for (user, index) in devAuthUsers: [
    { name: 'DevAuth__Users__${index}__Id', value: user.id }
    { name: 'DevAuth__Users__${index}__Name', value: user.name }
    { name: 'DevAuth__Users__${index}__Role', value: user.role }
  ]
]

var devAuthSettings = concat([
  { name: 'DevAuth__OrganisationId', value: devAuthOrganisationId }
  { name: 'DevAuth__Secret', value: '@Microsoft.KeyVault(SecretUri=${keyVault.outputs.uri}secrets/${devAuthSecretName}/)' }
], flatten(devAuthSettingsPerUser))

var appSettings = concat(platformSettings, wiringSettings, featureSettings, devAuthSettings)

module appService 'modules/appservice.bicep' = {
  name: 'appservice'
  params: {
    planName: 'asp-${baseName}-${suffix}'
    appName: 'app-${baseName}-${suffix}'
    location: location
    tags: tags
    skuName: appServiceSkuName
    identityId: identity.outputs.id
    workspaceId: monitoring.outputs.workspaceId
    appSettings: appSettings
  }
}

module roles 'modules/roles.bicep' = {
  name: 'roles'
  params: {
    vaultName: keyVault.outputs.name
    principalId: identity.outputs.principalId
  }
}

module deployRoles 'modules/roles.bicep' = {
  name: 'roles-deploy'
  params: {
    vaultName: keyVault.outputs.name
    principalId: sqlAdminObjectId
  }
}

output resourceGroupName string = resourceGroup().name
output webAppName string = appService.outputs.name
output webAppHostName string = appService.outputs.defaultHostName
output identityName string = identity.outputs.name
output identityClientId string = identity.outputs.clientId
output sqlServerName string = sql.outputs.serverName
output sqlServerFqdn string = sql.outputs.fullyQualifiedDomainName
output databaseName string = sql.outputs.databaseName
output keyVaultName string = keyVault.outputs.name
output staticWebAppName string = staticWebApp.outputs.name
output staticWebAppHostName string = staticWebApp.outputs.defaultHostname
output devAuthSecretName string = devAuthSecretName
