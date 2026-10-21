@description('Name of the static web app.')
param name string

@description('Region of the static web app; only a few regions are offered.')
@allowed([
  'westus2'
  'centralus'
  'eastus2'
  'westeurope'
  'eastasia'
])
param location string

@description('Tags applied to the resource.')
param tags object

resource staticSite 'Microsoft.Web/staticSites@2025-03-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

output name string = staticSite.name
output defaultHostname string = staticSite.properties.defaultHostname
