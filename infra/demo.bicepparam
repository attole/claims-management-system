using 'main.bicep'

param location = 'polandcentral'
param staticWebAppLocation = 'westeurope'
param environmentName = 'demo'
param appServiceSkuName = 'B1'

param sqlAdminObjectId = readEnvironmentVariable('SQL_ADMIN_OBJECT_ID')
param sqlAdminLogin = readEnvironmentVariable('SQL_ADMIN_LOGIN')

param isSwaggerEnabled = true
param isHangfireDashboardEnabled = false
param isBlobSweepEnabled = false
param isDevAuthEnabled = true

param devAuthOrganisationId = '3a44da3d-69e0-b835-4a7c-2859c519e664'
param devAuthUsers = [
  { id: 'a1000000-0000-4000-8000-000000000001', name: 'Hana Handler', role: 'Handler' }
  { id: 'a1000000-0000-4000-8000-000000000002', name: 'Sam Supervisor', role: 'Supervisor' }
  { id: 'a1000000-0000-4000-8000-000000000003', name: 'Mia Manager', role: 'Manager' }
]
