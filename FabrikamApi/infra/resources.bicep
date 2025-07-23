// Resources template for FabrikamApi
// Creates the core Azure resources needed for hosting the .NET 9.0 Web API

@description('Primary location for all resources')
param location string

@description('Resource token to make resource names unique')
param resourceToken string

@description('Tags to apply to all resources')
param tags object = {}

// Log Analytics Workspace - Foundation for monitoring and logging
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${resourceToken}'
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
  name: 'appi-${resourceToken}'
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
  name: 'id-${resourceToken}'
  location: location
  tags: tags
}

// App Service Plan - Compute infrastructure for the API
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'plan-${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
    reserved: false // Windows
    isSpot: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

// App Service - The web application hosting the API
resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: 'app-${resourceToken}'
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
    redundancyMode: 'None'
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      alwaysOn: true
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
      // Configure app settings for the .NET 9 API
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
          name: 'APPLICATIONINSIGHTS_ROLE_NAME'
          value: 'FabrikamApi'
        }
        {
          name: 'ApplicationInsights__InstrumentationKey'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
        {
          name: 'WEBSITE_HEALTHCHECK_MAXPINGFAILURES'
          value: '3'
        }
        // Enable detailed logging for troubleshooting
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
          '*'  // In production, replace with specific origins
        ]
        supportCredentials: false
      }
      // Configure default documents for Swagger UI
      defaultDocuments: [
        'index.html'
        'Default.htm'
        'Default.html'
        'Default.asp'
        'index.htm'
        'iisstart.htm'
        'default.aspx'
      ]
      // IP security restrictions (allow all for development)
      ipSecurityRestrictions: [
        {
          ipAddress: 'Any'
          action: 'Allow'
          priority: 2147483647
          name: 'Allow all'
          description: 'Allow all access'
        }
      ]
      scmIpSecurityRestrictions: [
        {
          ipAddress: 'Any'
          action: 'Allow'
          priority: 2147483647
          name: 'Allow all'
          description: 'Allow all access'
        }
      ]
      scmIpSecurityRestrictionsUseMain: false
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
        category: 'AppServiceFileAuditLogs'
        enabled: true
      }
      {
        category: 'AppServiceAuditLogs'
        enabled: true
      }
      {
        category: 'AppServiceIPSecAuditLogs'
        enabled: true
      }
      {
        category: 'AppServicePlatformLogs'
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
