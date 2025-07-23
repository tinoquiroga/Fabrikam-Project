// Main deployment template for FabrikamApi
// This template creates all necessary Azure resources for hosting the FabrikamApi .NET 9.0 Web API

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (e.g. dev, prod, staging)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Name of the resource group')
param resourceGroupName string = 'rg-${environmentName}'

// Generate a unique suffix for resource names to avoid conflicts
var resourceToken = uniqueString(subscription().id, location, environmentName)

// Tags to be applied to all resources
var tags = {
  'azd-env-name': environmentName
  'azd-service-name': 'fabrikamapi'
  project: 'FabrikamApi'
  environment: environmentName
  location: location
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
  }
}

// Outputs for reference by other templates or applications
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId

// Resource group details
output AZURE_RESOURCE_GROUP string = rg.name
output RESOURCE_GROUP_ID string = rg.id

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
