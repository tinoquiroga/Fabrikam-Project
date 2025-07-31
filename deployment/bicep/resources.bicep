// Core Azure resources for FabrikamApi with authentication mode support
// Creates App Service, Key Vault, monitoring, and authentication-aware configurations

@description('Primary location for all resources')
param location string

@description('Resource token to make resource names unique')
param resourceToken string

@description('Tags to apply to all resources')
param tags object = {}

@description('Authentication mode for the API')
@allowed([
  'Disabled'
  'BearerToken'
  'EntraExternalId'
])
param authenticationMode string

@description('Enable user tracking across requests')
param enableUserTracking bool

@description('Entra External ID tenant domain')
param entraExternalIdTenant string

@description('Entra External ID application client ID')
param entraExternalIdClientId string

@secure()
@description('Entra External ID client secret')
param entraExternalIdClientSecret string

@description('Object ID of the deployment user for Key Vault access')
param deploymentUserId string

@description('App Service Plan SKU')
param appServiceSku string

// Create unique names for resources
var keyVaultName = 'kv-${resourceToken}'
var appServicePlanName = 'plan-${resourceToken}'
var webAppName = 'app-${resourceToken}'
var logAnalyticsName = 'log-${resourceToken}'
var appInsightsName = 'appi-${resourceToken}'
var managedIdentityName = 'id-${resourceToken}'

// Define built-in role IDs
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultSecretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

// Log Analytics Workspace - Foundation for monitoring and logging
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights - Application monitoring and telemetry
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    DisableIpMasking: false
    DisableLocalAuth: false
  }
}

// User-Assigned Managed Identity - For secure resource access
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: tags
}

// Key Vault for secure configuration storage
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Grant Key Vault Secrets User role to managed identity
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, managedIdentity.id, keyVaultSecretsUserRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Key Vault Secrets Officer role to deployment user (if provided)
resource keyVaultSecretsOfficerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(deploymentUserId)) {
  scope: keyVault
  name: guid(keyVault.id, deploymentUserId, keyVaultSecretsOfficerRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsOfficerRoleId)
    principalId: deploymentUserId
    principalType: 'User'
  }
}

// Generate secrets based on authentication mode
resource jwtSecretKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (authenticationMode == 'BearerToken') {
  parent: keyVault
  name: 'jwt-secret-key'
  properties: {
    value: base64(guid(resourceGroup().id, 'jwt-secret'))
    contentType: 'JWT signing key for BearerToken authentication'
  }
}

resource entraClientSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (authenticationMode == 'EntraExternalId' && !empty(entraExternalIdClientSecret)) {
  parent: keyVault
  name: 'entra-client-secret'
  properties: {
    value: entraExternalIdClientSecret
    contentType: 'Entra External ID client secret'
  }
}

resource entraClientId 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (authenticationMode == 'EntraExternalId' && !empty(entraExternalIdClientId)) {
  parent: keyVault
  name: 'entra-client-id'
  properties: {
    value: entraExternalIdClientId
    contentType: 'Entra External ID client ID'
  }
}

resource entraTenantId 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (authenticationMode == 'EntraExternalId' && !empty(entraExternalIdTenant)) {
  parent: keyVault
  name: 'entra-tenant-id'
  properties: {
    value: entraExternalIdTenant
    contentType: 'Entra External ID tenant domain'
  }
}

resource guidTrackingSalt 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (enableUserTracking) {
  parent: keyVault
  name: 'guid-tracking-salt'
  properties: {
    value: base64(guid(resourceGroup().id, 'tracking-salt'))
    contentType: 'Salt for GUID-based user tracking'
  }
}

// App Service Plan - Compute infrastructure for the API
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: appServiceSku
    tier: appServiceSku == 'F1' ? 'Free' : (startsWith(appServiceSku, 'B') ? 'Basic' : (startsWith(appServiceSku, 'S') ? 'Standard' : 'PremiumV3'))
  }
  properties: {
    reserved: false // Windows
    targetWorkerCount: 1
    targetWorkerSizeId: 0
  }
}

// App Service - The web application hosting the API
resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  tags: tags
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      alwaysOn: appServiceSku != 'F1' // AlwaysOn not supported on Free tier
      webSocketsEnabled: false
      http20Enabled: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      healthCheckPath: '/health'
      // Configure app settings for the .NET 9 API with authentication mode support
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ASPNETCORE_HTTPS_PORT'
          value: '443'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsights__InstrumentationKey'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
        // Authentication configuration
        {
          name: 'Authentication__Mode'
          value: authenticationMode
        }
        {
          name: 'Authentication__EnableUserTracking'
          value: string(enableUserTracking)
        }
        // Key Vault configuration for secrets
        {
          name: 'KeyVault__VaultUri'
          value: keyVault.properties.vaultUri
        }
        // JWT configuration (BearerToken mode)
        {
          name: 'Authentication__Jwt__Issuer'
          value: 'https://${webAppName}.azurewebsites.net'
        }
        {
          name: 'Authentication__Jwt__Audience'
          value: 'fabrikam-api'
        }
        {
          name: 'Authentication__Jwt__SecretKeyFromKeyVault'
          value: authenticationMode == 'BearerToken' ? 'jwt-secret-key' : ''
        }
        // EntraExternalId configuration
        {
          name: 'Authentication__EntraExternalId__TenantId'
          value: authenticationMode == 'EntraExternalId' ? entraExternalIdTenant : ''
        }
        {
          name: 'Authentication__EntraExternalId__ClientId'
          value: authenticationMode == 'EntraExternalId' ? entraExternalIdClientId : ''
        }
        {
          name: 'Authentication__EntraExternalId__ClientSecretFromKeyVault'
          value: authenticationMode == 'EntraExternalId' ? 'entra-client-secret' : ''
        }
        {
          name: 'Authentication__EntraExternalId__Authority'
          value: authenticationMode == 'EntraExternalId' ? 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0' : ''
        }
        // GUID tracking configuration
        {
          name: 'UserTracking__SaltFromKeyVault'
          value: enableUserTracking ? 'guid-tracking-salt' : ''
        }
        // Logging configuration
        {
          name: 'Logging__LogLevel__Default'
          value: 'Information'
        }
        {
          name: 'Logging__LogLevel__Microsoft.AspNetCore'
          value: 'Warning'
        }
        {
          name: 'Logging__ApplicationInsights__LogLevel__Default'
          value: 'Information'
        }
      ]
      // Enable CORS for API access
      cors: {
        allowedOrigins: [
          'https://${webAppName}.azurewebsites.net'
          // Add specific origins in production
        ]
        supportCredentials: authenticationMode != 'Disabled' // Enable credentials for authenticated modes
      }
      // Default documents for API documentation
      defaultDocuments: [
        'index.html'
        'swagger/index.html'
      ]
      // Authentication configuration based on mode
      authSettings: authenticationMode == 'EntraExternalId' ? {
        enabled: true
        unauthenticatedClientAction: 'RedirectToLoginPage'
        tokenStoreEnabled: true
        defaultProvider: 'AzureActiveDirectory'
        clientId: entraExternalIdClientId
        issuer: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0'
        allowedAudiences: [
          entraExternalIdClientId
        ]
        additionalLoginParams: []
        isAadAutoProvisioned: false
      } : {
        enabled: false
      }
    }
  }
}

// Diagnostic settings for App Service monitoring
resource webAppDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: webApp
  name: 'diagnostics'
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
      }
      {
        category: 'AppServiceAuditLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Output important resource information
output API_URI string = 'https://${webApp.properties.defaultHostName}'
output API_NAME string = webApp.name
output APP_SERVICE_PLAN_NAME string = appServicePlan.name
output APPLICATION_INSIGHTS_NAME string = applicationInsights.name
output LOG_ANALYTICS_WORKSPACE_NAME string = logAnalytics.name
output MANAGED_IDENTITY_CLIENT_ID string = managedIdentity.properties.clientId
output MANAGED_IDENTITY_NAME string = managedIdentity.name
output MANAGED_IDENTITY_PRINCIPAL_ID string = managedIdentity.properties.principalId
output KEY_VAULT_NAME string = keyVault.name
output KEY_VAULT_URI string = keyVault.properties.vaultUri
