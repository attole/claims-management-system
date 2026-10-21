@description('Name of the App Service plan.')
param planName string

@description('Name of the web app.')
param appName string

@description('Azure region.')
param location string

@description('Tags applied to the resources.')
param tags object

@description('Plan size; Basic or higher is needed for Always On.')
param skuName string = 'B1'

@description('Runtime stack of the Linux web app.')
param linuxFxVersion string = 'DOTNETCORE|9.0'

@description('Path the platform probes to judge the app healthy.')
param healthCheckPath string = '/health/ready'

@description('Resource id of the user-assigned managed identity the web app runs as.')
param identityId string

@description('Log Analytics workspace that receives the web app logs.')
param workspaceId string

@description('App settings as name and value objects.')
param appSettings array

resource plan 'Microsoft.Web/serverfarms@2025-03-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: skuName
  }
  properties: {
    reserved: true
  }
}

resource site 'Microsoft.Web/sites@2025-03-01' = {
  name: appName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    serverFarmId: plan.id
    keyVaultReferenceIdentity: identityId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: healthCheckPath
      appSettings: appSettings
    }
  }
}

resource diagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: site
  name: 'send-to-log-analytics'
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServicePlatformLogs'
        enabled: true
      }
    ]
  }
}

output name string = site.name
output defaultHostName string = site.properties.defaultHostName
