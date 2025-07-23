# 🚀 Quick Deployment Script with Custom Domains
# 
# This script creates Azure resources with globally unique names using random suffixes
# and optionally configures custom domains for your Fabrikam applications.
#
# Prerequisites:
# 1. Azure CLI installed and logged in
# 2. DNS access to configure CNAME records
# 3. SSL certificate for HTTPS (optional but recommended)

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "East US 2",
    
    [Parameter(Mandatory=$false)]
    [string]$CustomDomain = "",  # Set to your domain like "levelupcsp.com"
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipCustomDomain
)

# Set default resource group if not provided
if ([string]::IsNullOrEmpty($ResourceGroup)) {
    $ResourceGroup = "rg-fabrikam-$Environment"
}

Write-Host "🚀 Starting Fabrikam Deployment" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan

if ($CustomDomain) {
    Write-Host "Custom Domain: $CustomDomain" -ForegroundColor Cyan
}

# Generate random suffix for globally unique resource names
$randomSuffix = -join ((65..90) + (97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
Write-Host "Random Suffix: $randomSuffix" -ForegroundColor Yellow

# Configure Azure CLI
Write-Host "📋 Configuring Azure CLI..." -ForegroundColor Blue
az config set extension.use_dynamic_install=yes_without_prompt
az account set --subscription $SubscriptionId

# Create resource group
Write-Host "📁 Creating resource group..." -ForegroundColor Blue
az group create --name $ResourceGroup --location $Location

# Create Log Analytics Workspace
Write-Host "📊 Creating Log Analytics workspace..." -ForegroundColor Blue
az monitor log-analytics workspace create `
  --resource-group $ResourceGroup `
  --workspace-name "log-fabrikam-$Environment" `
  --location $Location

# Get workspace ID
$workspaceId = az monitor log-analytics workspace show `
  --resource-group $ResourceGroup `
  --workspace-name "log-fabrikam-$Environment" `
  --query id --output tsv

# Create Application Insights
Write-Host "📈 Creating Application Insights..." -ForegroundColor Blue
az monitor app-insights component create `
  --app "appi-fabrikam-api-$Environment" `
  --location $Location `
  --resource-group $ResourceGroup `
  --workspace $workspaceId `
  --kind web `
  --application-type web

az monitor app-insights component create `
  --app "appi-fabrikam-mcp-$Environment" `
  --location $Location `
  --resource-group $ResourceGroup `
  --workspace $workspaceId `
  --kind web `
  --application-type web

# Get Application Insights connection strings
$apiInsightsKey = az monitor app-insights component show `
  --app "appi-fabrikam-api-$Environment" `
  --resource-group $ResourceGroup `
  --query connectionString --output tsv

$mcpInsightsKey = az monitor app-insights component show `
  --app "appi-fabrikam-mcp-$Environment" `
  --resource-group $ResourceGroup `
  --query connectionString --output tsv

# Create App Service Plans
Write-Host "🏗️  Creating App Service Plans..." -ForegroundColor Blue
az appservice plan create `
  --name "plan-fabrikam-api-$Environment" `
  --resource-group $ResourceGroup `
  --sku B1 --is-linux

az appservice plan create `
  --name "plan-fabrikam-mcp-$Environment" `
  --resource-group $ResourceGroup `
  --sku B1 --is-linux

# Create Web Apps with unique names
$apiAppName = "fabrikam-api-$Environment-$randomSuffix"
$mcpAppName = "fabrikam-mcp-$Environment-$randomSuffix"

Write-Host "🌐 Creating Web Apps..." -ForegroundColor Blue
Write-Host "  API: $apiAppName" -ForegroundColor Yellow
Write-Host "  MCP: $mcpAppName" -ForegroundColor Yellow

az webapp create `
  --name $apiAppName `
  --resource-group $ResourceGroup `
  --plan "plan-fabrikam-api-$Environment" `
  --runtime "DOTNETCORE:9.0"

az webapp create `
  --name $mcpAppName `
  --resource-group $ResourceGroup `
  --plan "plan-fabrikam-mcp-$Environment" `
  --runtime "DOTNETCORE:9.0"

# Configure Application Insights
Write-Host "⚙️  Configuring Application Insights..." -ForegroundColor Blue
az webapp config appsettings set `
  --name $apiAppName `
  --resource-group $ResourceGroup `
  --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=$apiInsightsKey" `
             "ApplicationInsightsAgent_EXTENSION_VERSION=~3" `
             "ASPNETCORE_ENVIRONMENT=$Environment"

az webapp config appsettings set `
  --name $mcpAppName `
  --resource-group $ResourceGroup `
  --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=$mcpInsightsKey" `
             "ApplicationInsightsAgent_EXTENSION_VERSION=~3" `
             "ASPNETCORE_ENVIRONMENT=$Environment" `
             "FabrikamApi__BaseUrl=https://$apiAppName.azurewebsites.net"

# Custom Domain Configuration
if ($CustomDomain -and -not $SkipCustomDomain) {
    Write-Host "🌍 Configuring custom domains..." -ForegroundColor Blue
    
    $apiCustomDomain = "api-$Environment.$CustomDomain"
    $mcpCustomDomain = "mcp-$Environment.$CustomDomain"
    
    Write-Host "Custom domains will be:" -ForegroundColor Green
    Write-Host "  API: https://$apiCustomDomain" -ForegroundColor Green  
    Write-Host "  MCP: https://$mcpCustomDomain" -ForegroundColor Green
    
    Write-Host "`n⚠️  IMPORTANT: Configure these DNS records before continuing:" -ForegroundColor Red
    Write-Host "  $apiCustomDomain    CNAME   $apiAppName.azurewebsites.net" -ForegroundColor Yellow
    Write-Host "  $mcpCustomDomain    CNAME   $mcpAppName.azurewebsites.net" -ForegroundColor Yellow
    
    $continue = Read-Host "`nHave you configured the DNS records? (y/N)"
    if ($continue -eq 'y' -or $continue -eq 'Y') {
        try {
            Write-Host "Adding custom domain for API..." -ForegroundColor Blue
            az webapp config hostname add `
              --webapp-name $apiAppName `
              --resource-group $ResourceGroup `
              --hostname $apiCustomDomain
            
            Write-Host "Adding custom domain for MCP..." -ForegroundColor Blue
            az webapp config hostname add `
              --webapp-name $mcpAppName `
              --resource-group $ResourceGroup `
              --hostname $mcpCustomDomain
              
            # Update MCP to use custom API domain
            az webapp config appsettings set `
              --name $mcpAppName `
              --resource-group $ResourceGroup `
              --settings "FabrikamApi__BaseUrl=https://$apiCustomDomain"
              
            Write-Host "✅ Custom domains configured successfully!" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Failed to configure custom domains. Check DNS configuration." -ForegroundColor Red
            Write-Host "You can add custom domains later using the Azure Portal." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Skipping custom domain configuration. You can set this up later." -ForegroundColor Yellow
    }
}

# Display results
Write-Host "`n🎉 Deployment Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

Write-Host "`nResource Details:" -ForegroundColor White
Write-Host "  Subscription: $SubscriptionId" -ForegroundColor Cyan
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor Cyan
Write-Host "  Random Suffix: $randomSuffix" -ForegroundColor Cyan

Write-Host "`nApp Service Names:" -ForegroundColor White
Write-Host "  API: $apiAppName" -ForegroundColor Yellow
Write-Host "  MCP: $mcpAppName" -ForegroundColor Yellow

Write-Host "`nDefault URLs:" -ForegroundColor White
Write-Host "  API: https://$apiAppName.azurewebsites.net" -ForegroundColor Green
Write-Host "  MCP: https://$mcpAppName.azurewebsites.net" -ForegroundColor Green

if ($CustomDomain -and -not $SkipCustomDomain) {
    Write-Host "`nCustom URLs:" -ForegroundColor White
    Write-Host "  API: https://api-$Environment.$CustomDomain" -ForegroundColor Green
    Write-Host "  MCP: https://mcp-$Environment.$CustomDomain" -ForegroundColor Green
}

Write-Host "`nNext Steps:" -ForegroundColor White
Write-Host "1. Deploy your applications using 'azd up' in each project folder" -ForegroundColor Cyan
Write-Host "2. Test the health endpoints:" -ForegroundColor Cyan
Write-Host "   curl https://$apiAppName.azurewebsites.net/health" -ForegroundColor Yellow
Write-Host "   curl https://$mcpAppName.azurewebsites.net/status" -ForegroundColor Yellow
if ($CustomDomain) {
    Write-Host "3. Configure SSL certificates for custom domains" -ForegroundColor Cyan
}

Write-Host "`n📋 Save these details for your CI/CD setup!" -ForegroundColor Blue
