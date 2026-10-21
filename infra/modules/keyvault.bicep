@description('Name of the key vault (3 to 24 characters).')
param name string

@description('Azure region.')
param location string

@description('Tags applied to the resource.')
param tags object

@description('Storage account whose connection string is stored as a secret.')
param storageAccountName string

@description('Name of the secret that holds the storage connection string.')
param storageSecretName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2026-06-01' existing = {
  name: storageAccountName
}

resource vault 'Microsoft.KeyVault/vaults@2026-05-15' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
  }
}

resource storageSecret 'Microsoft.KeyVault/vaults/secrets@2026-05-15' = {
  parent: vault
  name: storageSecretName
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

output name string = vault.name
output uri string = vault.properties.vaultUri
