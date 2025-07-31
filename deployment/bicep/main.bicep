// Main deployment template for Fabrikam API with three authentication modes
// Supports: Disabled, BearerToken, and EntraExternalId authentication modes

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (e.g. dev, prod, staging)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Name of the resource group')
param resourceGroupName string = 'rg-fabrikam-${environmentName}'

@description('Authentication mode for the Fabrikam API')
@allowed([
  'Disabled'
  'BearerToken' 
  'EntraExternalId'
])
param authenticationMode string = 'Disabled'

@description('Enable user tracking across requests')
param enableUserTracking bool = true

@description('Entra External ID tenant domain (required for EntraExternalId mode)')
param entraExternalIdTenant string = ''

@description('Entra External ID application client ID (required for EntraExternalId mode)')
param entraExternalIdClientId string = ''

@secure()
@description('Entra External ID client secret (required for EntraExternalId mode)')
param entraExternalIdClientSecret string = ''

@description('Object ID of the user deploying this template for Key Vault access')
param deploymentUserId string = ''

@description('Pricing tier for App Service Plan')
@allowed([
  'F1'
  'B1'
  'B2'
  'S1'
  'S2'
  'P1v3'
  'P2v3'
])
param appServiceSku string = 'B1'

// Generate a unique suffix for resource names to avoid conflicts
var resourceToken = uniqueString(subscription().id, location, environmentName)

// Tags to be applied to all resources
var tags = {
  'environment': environmentName
  'project': 'FabrikamApi'
  'authenticationMode': authenticationMode
  'location': location
  'version': '2.0'
}

// Create the resource group
resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy the main resources
module resources 'resources.bicep' = {
  name: 'fabrikam-api-resources'
  scope: rg
  params: {
    location: location
    resourceToken: resourceToken
    tags: tags
    authenticationMode: authenticationMode
    enableUserTracking: enableUserTracking
    entraExternalIdTenant: entraExternalIdTenant
    entraExternalIdClientId: entraExternalIdClientId
    entraExternalIdClientSecret: entraExternalIdClientSecret
    deploymentUserId: deploymentUserId
    appServiceSku: appServiceSku
  }
}

// Deploy EntraExternalId specific resources if needed
module entraResources 'entra-resources.bicep' = if (authenticationMode == 'EntraExternalId') {
  name: 'fabrikam-entra-resources'
  scope: rg
  params: {
    location: location
    resourceToken: resourceToken
    tags: tags
    entraExternalIdTenant: entraExternalIdTenant
    entraExternalIdClientId: entraExternalIdClientId
    keyVaultName: resources.outputs.KEY_VAULT_NAME
  }
}

// Outputs for reference by other templates or applications
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId

// Resource group details
output AZURE_RESOURCE_GROUP string = rg.name
output RESOURCE_GROUP_ID string = rg.id

// Authentication mode
output AUTHENTICATION_MODE string = authenticationMode

// App Service details
output API_URI string = resources.outputs.API_URI
output API_NAME string = resources.outputs.API_NAME
output APP_SERVICE_PLAN_NAME string = resources.outputs.APP_SERVICE_PLAN_NAME

// Monitoring and logging
output APPLICATION_INSIGHTS_NAME string = resources.outputs.APPLICATION_INSIGHTS_NAME
output LOG_ANALYTICS_WORKSPACE_NAME string = resources.outputs.LOG_ANALYTICS_WORKSPACE_NAME

// Security
output MANAGED_IDENTITY_CLIENT_ID string = resources.outputs.MANAGED_IDENTITY_CLIENT_ID
output MANAGED_IDENTITY_NAME string = resources.outputs.MANAGED_IDENTITY_NAME
output KEY_VAULT_NAME string = resources.outputs.KEY_VAULT_NAME

// EntraExternalId specific outputs (if deployed)
output ENTRA_AUTHORITY_URL string = authenticationMode == 'EntraExternalId' ? entraResources.outputs.ENTRA_AUTHORITY_URL : ''
output ENTRA_WELL_KNOWN_ENDPOINT string = authenticationMode == 'EntraExternalId' ? entraResources.outputs.ENTRA_WELL_KNOWN_ENDPOINT : ''
