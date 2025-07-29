targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Optional. The pricing tier for the App Service Plan. Defaults to B1.')
param appServicePlanSku string = 'B1'

@description('Optional. The name of the resource group to deploy the resources to.')
param resourceGroupName string = 'rg-${environmentName}'

@description('Optional. The base URL of the FabrikamApi service that this MCP server should connect to.')
param fabrikamApiBaseUrl string = ''

// Generate a unique token for resource naming
var resourceToken = toLower(uniqueString(subscription().id, location, environmentName))
var tags = { 'azd-env-name': environmentName }

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  name: 'resources'
  scope: resourceGroup
  params: {
    location: location
    resourceToken: resourceToken
    tags: tags
    sku: appServicePlanSku
    fabrikamApiBaseUrl: fabrikamApiBaseUrl
  }
}

output AZURE_LOCATION string = location
output RESOURCE_GROUP_ID string = resourceGroup.id
output WEB_URI string = resources.outputs.WEB_URI
