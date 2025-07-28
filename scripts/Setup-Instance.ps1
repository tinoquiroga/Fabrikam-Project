#!/usr/bin/env pwsh
# Setup-Instance.ps1 - Configure repository variables for a new instance

param(
    [Parameter(Mandatory = $true)]
    [string]$ApiDomain,
    
    [Parameter(Mandatory = $true)]
    [string]$McpDomain,
    
    [Parameter(Mandatory = $true)]
    [string]$InstanceName,
    
    [string]$AzureApiAppName = "",
    [string]$AzureMcpAppName = "",
    [string]$AzureResourceGroup = "",
    [switch]$UseGitHubCLI
)

Write-Host "🚀 Setting up new Fabrikam Project instance: $InstanceName" -ForegroundColor Green
Write-Host ""

# Validate domain inputs
if (-not $ApiDomain.StartsWith("http")) {
    $ApiDomain = "https://$ApiDomain"
}
if (-not $McpDomain.StartsWith("http")) {
    $McpDomain = "https://$McpDomain"
}

# Extract just the domain part for repository variables
$ApiDomainOnly = $ApiDomain -replace "https://", ""
$McpDomainOnly = $McpDomain -replace "https://", ""

Write-Host "📋 Configuration Summary:" -ForegroundColor Yellow
Write-Host "  API Domain: $ApiDomainOnly" -ForegroundColor White
Write-Host "  MCP Domain: $McpDomainOnly" -ForegroundColor White
Write-Host "  Instance Name: $InstanceName" -ForegroundColor White

if ($AzureApiAppName) {
    Write-Host "  Azure API App: $AzureApiAppName" -ForegroundColor White
}
if ($AzureMcpAppName) {
    Write-Host "  Azure MCP App: $AzureMcpAppName" -ForegroundColor White
}
if ($AzureResourceGroup) {
    Write-Host "  Azure Resource Group: $AzureResourceGroup" -ForegroundColor White
}

Write-Host ""

# Update local configuration files
Write-Host "🔧 Updating local configuration files..." -ForegroundColor Yellow

# Update VS Code settings
$settingsPath = ".vscode/settings.json"
if (Test-Path $settingsPath) {
    $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
    $settings.'rest-client.environmentVariables'.azure.apiUrl = $ApiDomain
    $settings.'rest-client.environmentVariables'.azure.mcpUrl = $McpDomain
    $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath
    Write-Host "  ✅ Updated $settingsPath" -ForegroundColor Green
}

# Update copilot workspace
$workspacePath = ".copilot-workspace.md"
if (Test-Path $workspacePath) {
    $content = Get-Content $workspacePath -Raw
    $content = $content -replace "https://fabrikam-api-dev\.levelupcsp\.com", $ApiDomain
    $content = $content -replace "https://fabrikam-mcp-dev\.levelupcsp\.com", $McpDomain
    $content | Set-Content $workspacePath
    Write-Host "  ✅ Updated $workspacePath" -ForegroundColor Green
}

# Update MCP settings
$mcpSettingsPath = "FabrikamMcp/src/appsettings.json"
if (Test-Path $mcpSettingsPath) {
    $mcpSettings = Get-Content $mcpSettingsPath -Raw | ConvertFrom-Json
    $mcpSettings.FabrikamApi.BaseUrl = $ApiDomain
    $mcpSettings | ConvertTo-Json -Depth 10 | Set-Content $mcpSettingsPath
    Write-Host "  ✅ Updated $mcpSettingsPath" -ForegroundColor Green
}

$mcpProdSettingsPath = "FabrikamMcp/src/appsettings.Production.json"
if (Test-Path $mcpProdSettingsPath) {
    $mcpProdSettings = Get-Content $mcpProdSettingsPath -Raw | ConvertFrom-Json
    $mcpProdSettings.FabrikamApi.BaseUrl = $ApiDomain
    $mcpProdSettings | ConvertTo-Json -Depth 10 | Set-Content $mcpProdSettingsPath
    Write-Host "  ✅ Updated $mcpProdSettingsPath" -ForegroundColor Green
}

Write-Host ""

if ($UseGitHubCLI) {
    Write-Host "🔄 Setting GitHub repository variables..." -ForegroundColor Yellow
    
    try {
        & gh variable set API_DOMAIN --body $ApiDomainOnly
        Write-Host "  ✅ Set API_DOMAIN = $ApiDomainOnly" -ForegroundColor Green
        
        & gh variable set MCP_DOMAIN --body $McpDomainOnly
        Write-Host "  ✅ Set MCP_DOMAIN = $McpDomainOnly" -ForegroundColor Green
        
        & gh variable set PROJECT_INSTANCE_NAME --body $InstanceName
        Write-Host "  ✅ Set PROJECT_INSTANCE_NAME = $InstanceName" -ForegroundColor Green
        
        if ($AzureApiAppName) {
            & gh variable set AZURE_API_APP_NAME --body $AzureApiAppName
            Write-Host "  ✅ Set AZURE_API_APP_NAME = $AzureApiAppName" -ForegroundColor Green
        }
        
        if ($AzureMcpAppName) {
            & gh variable set AZURE_MCP_APP_NAME --body $AzureMcpAppName
            Write-Host "  ✅ Set AZURE_MCP_APP_NAME = $AzureMcpAppName" -ForegroundColor Green
        }
        
        if ($AzureResourceGroup) {
            & gh variable set AZURE_RESOURCE_GROUP --body $AzureResourceGroup
            Write-Host "  ✅ Set AZURE_RESOURCE_GROUP = $AzureResourceGroup" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "✅ GitHub repository variables set successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "  ❌ Error setting GitHub variables: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  💡 Make sure GitHub CLI is installed and authenticated" -ForegroundColor Yellow
    }
}
else {
    Write-Host "📋 Manual GitHub Setup Required:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Go to your repository Settings → Secrets and Variables → Actions → Variables" -ForegroundColor White
    Write-Host "Add these variables:" -ForegroundColor White
    Write-Host ""
    Write-Host "  API_DOMAIN = $ApiDomainOnly" -ForegroundColor Cyan
    Write-Host "  MCP_DOMAIN = $McpDomainOnly" -ForegroundColor Cyan
    Write-Host "  PROJECT_INSTANCE_NAME = $InstanceName" -ForegroundColor Cyan
    
    if ($AzureApiAppName) {
        Write-Host "  AZURE_API_APP_NAME = $AzureApiAppName" -ForegroundColor Cyan
    }
    if ($AzureMcpAppName) {
        Write-Host "  AZURE_MCP_APP_NAME = $AzureMcpAppName" -ForegroundColor Cyan
    }
    if ($AzureResourceGroup) {
        Write-Host "  AZURE_RESOURCE_GROUP = $AzureResourceGroup" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "🎉 Instance setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Commit and push the updated configuration files" -ForegroundColor White
Write-Host "2. If not using GitHub CLI, manually set the repository variables" -ForegroundColor White
Write-Host "3. Run a deployment to test the new configuration" -ForegroundColor White
Write-Host "4. Update any remaining documentation with your specific domains" -ForegroundColor White

Write-Host ""
Write-Host "🔍 Your instance will be accessible at:" -ForegroundColor Cyan
Write-Host "  API: $ApiDomain" -ForegroundColor White
Write-Host "  MCP: $McpDomain" -ForegroundColor White
