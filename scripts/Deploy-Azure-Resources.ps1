# 🚀 Azure Resources Deployment Script for Fabrikam Project
# 
# This script creates all necessary Azure resources for the Fabrikam project
# in a new Azure subscription/tenant with full deployment automation.
#
# Usage Examples:
#   .\Deploy-Azure-Resources.ps1 -Environment "dev" 
#   .\Deploy-Azure-Resources.ps1 -Environment "dev" -CustomDomain "roanoketechhub.com"
#   .\Deploy-Azure-Resources.ps1 -Environment "prod" -Location "East US"

param(
    [Parameter(Mandatory = $false)]
    [string]$SubscriptionId = "",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "East US 2",
    
    [Parameter(Mandatory = $false)]
    [string]$CustomDomain = "",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$CreateServicePrincipal = $false
)

# Color output functions
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Step { param($Message) Write-Host "`n🔄 $Message" -ForegroundColor Blue }

Write-Host @"
🏗️  FABRIKAM AZURE DEPLOYMENT
============================
Environment: $Environment
Location: $Location
Custom Domain: $(if($CustomDomain) { $CustomDomain } else { "None" })
What-If Mode: $WhatIf
"@ -ForegroundColor Magenta

# Check prerequisites
Write-Step "Checking Prerequisites"

# Check if Azure CLI is installed
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Success "Azure CLI version $($azVersion.'azure-cli') found"
} catch {
    Write-Error "Azure CLI not found. Please install Azure CLI first."
    exit 1
}

# Check current Azure context
try {
    $currentAccount = az account show --output json | ConvertFrom-Json
    Write-Success "Logged in as: $($currentAccount.user.name)"
    Write-Info "Current subscription: $($currentAccount.name) ($($currentAccount.id))"
    Write-Info "Tenant: $($currentAccount.tenantDisplayName) ($($currentAccount.tenantId))"
    
    # Use current subscription if not specified
    if (-not $SubscriptionId) {
        $SubscriptionId = $currentAccount.id
        Write-Info "Using current subscription: $SubscriptionId"
    }
} catch {
    Write-Error "Not logged into Azure. Please run 'az login' first."
    exit 1
}

# Set subscription if different
if ($SubscriptionId -ne $currentAccount.id) {
    Write-Step "Setting Azure subscription to $SubscriptionId"
    if (-not $WhatIf) {
        az account set --subscription $SubscriptionId
        Write-Success "Switched to subscription: $SubscriptionId"
    }
}

# Configure Azure CLI extensions
Write-Step "Configuring Azure CLI Extensions"
if (-not $WhatIf) {
    az config set extension.use_dynamic_install=yes_without_prompt
    Write-Success "Enabled automatic extension installation"
}

# Generate random suffix for globally unique resource names
$randomSuffix = -join ((97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
Write-Info "Generated random suffix for unique naming: $randomSuffix"

# Set up resource names
$resourceGroup = "rg-fabrikam-$Environment"
$apiAppName = "fabrikam-api-$Environment-$randomSuffix"
$mcpAppName = "fabrikam-mcp-$Environment-$randomSuffix"
$logWorkspaceName = "log-fabrikam-$Environment"
$apiInsightsName = "appi-fabrikam-api-$Environment"
$mcpInsightsName = "appi-fabrikam-mcp-$Environment"
$apiPlanName = "plan-fabrikam-api-$Environment"
$mcpPlanName = "plan-fabrikam-mcp-$Environment"

Write-Step "Resource Naming Convention"
Write-Host @"
📋 Resource Names:
   Resource Group: $resourceGroup
   API App Service: $apiAppName
   MCP App Service: $mcpAppName
   Log Analytics: $logWorkspaceName
   API App Insights: $apiInsightsName
   MCP App Insights: $mcpInsightsName
   API Service Plan: $apiPlanName
   MCP Service Plan: $mcpPlanName
"@ -ForegroundColor White

if ($WhatIf) {
    Write-Warning "WHAT-IF MODE: No resources will be created"
    Write-Host @"
🔍 What would be created:
   - Resource Group: $resourceGroup in $Location
   - Log Analytics Workspace: $logWorkspaceName
   - Application Insights: $apiInsightsName, $mcpInsightsName
   - App Service Plans: $apiPlanName (B1), $mcpPlanName (B1)
   - Web Apps: $apiAppName, $mcpAppName (.NET 9.0)
   - Configuration: Environment variables and app settings
"@ -ForegroundColor Yellow
    exit 0
}

# Create Resource Group
Write-Step "Creating Resource Group"
$rgExists = az group exists --name $resourceGroup --output tsv
if ($rgExists -eq "false") {
    az group create --name $resourceGroup --location $Location --output table
    Write-Success "Created resource group: $resourceGroup"
} else {
    Write-Info "Resource group already exists: $resourceGroup"
}

# Create Log Analytics Workspace
Write-Step "Creating Log Analytics Workspace"
try {
    $existingWorkspace = az monitor log-analytics workspace show --resource-group $resourceGroup --workspace-name $logWorkspaceName --output json 2>$null
    if ($existingWorkspace) {
        Write-Info "Log Analytics workspace already exists: $logWorkspaceName"
    } else {
        throw "Not found"
    }
} catch {
    az monitor log-analytics workspace create `
        --resource-group $resourceGroup `
        --workspace-name $logWorkspaceName `
        --location $Location `
        --output table
    Write-Success "Created Log Analytics workspace: $logWorkspaceName"
}

# Get Log Analytics Workspace ID
$workspaceId = az monitor log-analytics workspace show `
    --resource-group $resourceGroup `
    --workspace-name $logWorkspaceName `
    --query id --output tsv

Write-Info "Log Analytics Workspace ID: $workspaceId"

# Create Application Insights
Write-Step "Creating Application Insights"

# API Application Insights
try {
    $existingApiInsights = az monitor app-insights component show --app $apiInsightsName --resource-group $resourceGroup --output json 2>$null
    if ($existingApiInsights) {
        Write-Info "API Application Insights already exists: $apiInsightsName"
    } else {
        throw "Not found"
    }
} catch {
    az monitor app-insights component create `
        --app $apiInsightsName `
        --location $Location `
        --resource-group $resourceGroup `
        --workspace $workspaceId `
        --kind web `
        --application-type web `
        --output table
    Write-Success "Created API Application Insights: $apiInsightsName"
}

# MCP Application Insights
try {
    $existingMcpInsights = az monitor app-insights component show --app $mcpInsightsName --resource-group $resourceGroup --output json 2>$null
    if ($existingMcpInsights) {
        Write-Info "MCP Application Insights already exists: $mcpInsightsName"
    } else {
        throw "Not found"
    }
} catch {
    az monitor app-insights component create `
        --app $mcpInsightsName `
        --location $Location `
        --resource-group $resourceGroup `
        --workspace $workspaceId `
        --kind web `
        --application-type web `
        --output table
    Write-Success "Created MCP Application Insights: $mcpInsightsName"
}

# Get Application Insights connection strings
$apiInsightsKey = az monitor app-insights component show `
    --app $apiInsightsName `
    --resource-group $resourceGroup `
    --query connectionString --output tsv

$mcpInsightsKey = az monitor app-insights component show `
    --app $mcpInsightsName `
    --resource-group $resourceGroup `
    --query connectionString --output tsv

Write-Info "Retrieved Application Insights connection strings"

# Create App Service Plans
Write-Step "Creating App Service Plans"

# API App Service Plan
try {
    $existingApiPlan = az appservice plan show --name $apiPlanName --resource-group $resourceGroup --output json 2>$null
    if ($existingApiPlan) {
        Write-Info "API App Service Plan already exists: $apiPlanName"
    } else {
        throw "Not found"
    }
} catch {
    az appservice plan create `
        --name $apiPlanName `
        --resource-group $resourceGroup `
        --sku B1 `
        --is-linux `
        --output table
    Write-Success "Created API App Service Plan: $apiPlanName"
}

# MCP App Service Plan
try {
    $existingMcpPlan = az appservice plan show --name $mcpPlanName --resource-group $resourceGroup --output json 2>$null
    if ($existingMcpPlan) {
        Write-Info "MCP App Service Plan already exists: $mcpPlanName"
    } else {
        throw "Not found"
    }
} catch {
    az appservice plan create `
        --name $mcpPlanName `
        --resource-group $resourceGroup `
        --sku B1 `
        --is-linux `
        --output table
    Write-Success "Created MCP App Service Plan: $mcpPlanName"
}

# Create Web Apps
Write-Step "Creating Web Applications"

# API Web App
try {
    $existingApiApp = az webapp show --name $apiAppName --resource-group $resourceGroup --output json 2>$null
    if ($existingApiApp) {
        Write-Info "API Web App already exists: $apiAppName"
    } else {
        throw "Not found"
    }
} catch {
    az webapp create `
        --name $apiAppName `
        --resource-group $resourceGroup `
        --plan $apiPlanName `
        --runtime "DOTNETCORE:9.0" `
        --output table
    Write-Success "Created API Web App: $apiAppName"
}

# MCP Web App
try {
    $existingMcpApp = az webapp show --name $mcpAppName --resource-group $resourceGroup --output json 2>$null
    if ($existingMcpApp) {
        Write-Info "MCP Web App already exists: $mcpAppName"
    } else {
        throw "Not found"
    }
} catch {
    az webapp create `
        --name $mcpAppName `
        --resource-group $resourceGroup `
        --plan $mcpPlanName `
        --runtime "DOTNETCORE:9.0" `
        --output table
    Write-Success "Created MCP Web App: $mcpAppName"
}

# Configure Application Settings
Write-Step "Configuring Application Settings"

# API App Settings
$apiBaseUrl = "https://$apiAppName.azurewebsites.net"
az webapp config appsettings set `
    --name $apiAppName `
    --resource-group $resourceGroup `
    --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=$apiInsightsKey" `
               "ApplicationInsightsAgent_EXTENSION_VERSION=~3" `
               "ASPNETCORE_ENVIRONMENT=$Environment" `
               "WEBSITE_RUN_FROM_PACKAGE=1" `
    --output table

Write-Success "Configured API application settings"

# MCP App Settings
az webapp config appsettings set `
    --name $mcpAppName `
    --resource-group $resourceGroup `
    --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=$mcpInsightsKey" `
               "ApplicationInsightsAgent_EXTENSION_VERSION=~3" `
               "ASPNETCORE_ENVIRONMENT=$Environment" `
               "FabrikamApi__BaseUrl=$apiBaseUrl" `
               "WEBSITE_RUN_FROM_PACKAGE=1" `
    --output table

Write-Success "Configured MCP application settings"

# Custom Domain Configuration (if specified)
if ($CustomDomain) {
    Write-Step "Setting up Custom Domain Configuration"
    
    $apiCustomDomain = "fabrikam-api-$Environment.$CustomDomain"
    $mcpCustomDomain = "fabrikam-mcp-$Environment.$CustomDomain"
    
    Write-Info @"
🌐 Custom Domain Setup:
   API Domain: $apiCustomDomain
   MCP Domain: $mcpCustomDomain
   
   ⚠️  DNS Configuration Required:
   Add these CNAME records to your DNS:
   $apiCustomDomain → $apiAppName.azurewebsites.net
   $mcpCustomDomain → $mcpAppName.azurewebsites.net
"@
    
    Write-Warning "Custom domain configuration requires manual DNS setup."
    Write-Warning "Run the following commands after DNS configuration:"
    Write-Host @"
    
# Add custom domains (run after DNS setup)
az webapp config hostname add --webapp-name $apiAppName --resource-group $resourceGroup --hostname $apiCustomDomain
az webapp config hostname add --webapp-name $mcpAppName --resource-group $resourceGroup --hostname $mcpCustomDomain

# Update MCP to use custom API domain
az webapp config appsettings set --name $mcpAppName --resource-group $resourceGroup --settings "FabrikamApi__BaseUrl=https://$apiCustomDomain"
"@ -ForegroundColor Gray
}

# Create Service Principal (if requested)
if ($CreateServicePrincipal) {
    Write-Step "Creating Service Principal for GitHub Actions"
    
    try {
        # Create service principal with specific web app access
        $apiAppScope = "/subscriptions/$SubscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$apiAppName"
        $mcpAppScope = "/subscriptions/$SubscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$mcpAppName"
        
        Write-Info "Creating service principal with Website Contributor access..."
        $sp = az ad sp create-for-rbac `
            --name "sp-fabrikam-deploy-$Environment" `
            --role "Website Contributor" `
            --scopes $apiAppScope $mcpAppScope `
            --output json | ConvertFrom-Json
        
        # Format for GitHub secret
        $githubSecret = @{
            clientId = $sp.appId
            clientSecret = $sp.password
            subscriptionId = $SubscriptionId
            tenantId = $sp.tenant
            activeDirectoryEndpointUrl = "https://login.microsoftonline.com"
            resourceManagerEndpointUrl = "https://management.azure.com/"
            activeDirectoryGraphResourceId = "https://graph.windows.net/"
            sqlManagementEndpointUrl = "https://management.core.windows.net:8443/"
            galleryEndpointUrl = "https://gallery.azure.com/"
            managementEndpointUrl = "https://management.core.windows.net/"
        }
        
        Write-Success "Service Principal created successfully!"
        Write-Host @"
        
🔐 GitHub Secret Configuration:
Add this as AZURE_CREDENTIALS in your GitHub repository secrets:

$($githubSecret | ConvertTo-Json -Depth 10)
"@ -ForegroundColor Green
        
    } catch {
        Write-Warning "Service Principal creation failed. You may need to use Azure Portal CI/CD setup instead."
        Write-Info "This is common with restricted tenant policies. See deployment guide for portal-based setup."
    }
}

# Generate deployment summary
Write-Step "Generating Deployment Summary"

$deploymentSummary = @{
    Environment = $Environment
    Location = $Location
    SubscriptionId = $SubscriptionId
    ResourceGroup = $resourceGroup
    ApiAppName = $apiAppName
    McpAppName = $mcpAppName
    ApiUrl = "https://$apiAppName.azurewebsites.net"
    McpUrl = "https://$mcpAppName.azurewebsites.net"
    ApiCustomUrl = if($CustomDomain) { "https://fabrikam-api-$Environment.$CustomDomain" } else { $null }
    McpCustomUrl = if($CustomDomain) { "https://fabrikam-mcp-$Environment.$CustomDomain" } else { $null }
    CreatedAt = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    RandomSuffix = $randomSuffix
}

# Save deployment summary
$summaryPath = "docs/deployment/deployment-summary-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$deploymentSummary | ConvertTo-Json -Depth 10 | Set-Content $summaryPath
Write-Success "Deployment summary saved to: $summaryPath"

# Final output
Write-Host @"

🎉 DEPLOYMENT COMPLETE!
======================

✅ Environment: $Environment
✅ Resource Group: $resourceGroup
✅ API App: $apiAppName
✅ MCP App: $mcpAppName

🌐 Service URLs:
   API: https://$apiAppName.azurewebsites.net
   MCP: https://$mcpAppName.azurewebsites.net

📋 Next Steps:
1. Set up CI/CD in GitHub (see deployment guide)
2. Deploy your code using GitHub Actions
3. Test the deployed services
4. Configure custom domains (if desired)

📖 Health Check Endpoints:
   API Health: https://$apiAppName.azurewebsites.net/health
   MCP Status: https://$mcpAppName.azurewebsites.net/status

"@ -ForegroundColor Green

if ($CustomDomain) {
    Write-Host @"
🌐 Custom Domain Next Steps:
1. Configure DNS CNAME records
2. Add custom domains to App Services
3. Configure SSL certificates
4. Update MCP configuration

"@ -ForegroundColor Cyan
}

Write-Success "Deployment script completed successfully!"
