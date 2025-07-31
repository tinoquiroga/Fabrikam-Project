#Requires -Version 7.0
<#
.SYNOPSIS
    Configure VS Code settings after ARM template deployment

.DESCRIPTION
    This script configures VS Code workspace settings with the deployment outputs
    from the ARM template, including custom domain configuration.

.PARAMETER ResourceGroup
    The Azure resource group name where resources were deployed

.PARAMETER SubscriptionId
    The Azure subscription ID (optional - will detect if not provided)

.PARAMETER SettingsPath
    Path to the VS Code settings file (default: .vscode/settings.json)

.EXAMPLE
    .\Configure-VSCodeSettings.ps1 -ResourceGroup "rg-fabrikam-k3m9"

.EXAMPLE
    .\Configure-VSCodeSettings.ps1 -ResourceGroup "rg-fabrikam-k3m9" -SubscriptionId "1ae622b1-c33c-457f-a2bb-351fed78922f"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory = $false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory = $false)]
    [string]$SettingsPath = ".vscode/settings.json"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "🔧 Configuring VS Code Settings After ARM Deployment" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

try {
    # Verify Azure CLI is available and logged in
    Write-Host "📋 Checking Azure CLI status..." -ForegroundColor Yellow
    $azAccount = az account show --output json 2>$null | ConvertFrom-Json
    
    if (-not $azAccount) {
        Write-Error "❌ Azure CLI not logged in. Please run 'az login' first."
        exit 1
    }
    
    # Set subscription if provided
    if ($SubscriptionId) {
        Write-Host "🔄 Setting subscription to: $SubscriptionId" -ForegroundColor Yellow
        az account set --subscription $SubscriptionId
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Failed to set subscription"
            exit 1
        }
    }
    
    # Get current Azure context
    $azContext = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Using Azure subscription: $($azContext.name)" -ForegroundColor Green
    
    # Get the latest deployment from the resource group
    Write-Host "🔍 Getting deployment outputs from resource group: $ResourceGroup" -ForegroundColor Yellow
    $deployments = az deployment group list --resource-group $ResourceGroup --output json | ConvertFrom-Json
    
    if (-not $deployments -or $deployments.Count -eq 0) {
        Write-Error "❌ No deployments found in resource group: $ResourceGroup"
        exit 1
    }
    
    # Get the most recent successful deployment
    $latestDeployment = $deployments | Where-Object { $_.properties.provisioningState -eq "Succeeded" } | Sort-Object { $_.properties.timestamp } -Descending | Select-Object -First 1
    
    if (-not $latestDeployment) {
        Write-Error "❌ No successful deployments found in resource group: $ResourceGroup"
        exit 1
    }
    
    Write-Host "✅ Found deployment: $($latestDeployment.name)" -ForegroundColor Green
    
    # Get deployment outputs
    $outputs = $latestDeployment.properties.outputs
    
    # Extract values from outputs
    $apiUrl = $outputs.apiUrl.value
    $mcpUrl = $outputs.mcpUrl.value
    $instanceSuffix = $outputs.instanceSuffix.value
    $apiAppName = $outputs.apiAppName.value
    $mcpAppName = $outputs.mcpAppName.value
    $customDomainsEnabled = $outputs.customDomainsEnabled.value
    $apiCustomDomain = $outputs.apiCustomDomain.value
    $mcpCustomDomain = $outputs.mcpCustomDomain.value
    
    Write-Host "📊 Deployment Information:" -ForegroundColor Cyan
    Write-Host "  Instance Suffix: $instanceSuffix" -ForegroundColor White
    Write-Host "  API URL: $apiUrl" -ForegroundColor White
    Write-Host "  MCP URL: $mcpUrl" -ForegroundColor White
    Write-Host "  Custom Domains: $customDomainsEnabled" -ForegroundColor White
    
    if ($customDomainsEnabled -eq $true) {
        Write-Host "  API Custom Domain: $apiCustomDomain" -ForegroundColor White
        Write-Host "  MCP Custom Domain: $mcpCustomDomain" -ForegroundColor White
    }
    
    # Load the template
    $templatePath = "deployment/vscode-settings-template.json"
    if (-not (Test-Path $templatePath)) {
        Write-Error "❌ VS Code settings template not found: $templatePath"
        exit 1
    }
    
    $template = Get-Content $templatePath -Raw
    
    # Replace placeholders
    $settings = $template `
        -replace "{{API_URL}}", $apiUrl `
        -replace "{{MCP_URL}}", $mcpUrl `
        -replace "{{SUBSCRIPTION_ID}}", $azContext.id `
        -replace "{{SUBSCRIPTION_NAME}}", $azContext.name `
        -replace "{{TENANT_ID}}", $azContext.tenantId `
        -replace "{{TENANT_NAME}}", $azContext.tenantDisplayName `
        -replace "{{RESOURCE_GROUP}}", $ResourceGroup `
        -replace "{{INSTANCE_NAME}}", "fabrikam-$($outputs.instanceSuffix.value)" `
        -replace "{{INSTANCE_SUFFIX}}", $instanceSuffix `
        -replace "{{API_APP_NAME}}", $apiAppName `
        -replace "{{MCP_APP_NAME}}", $mcpAppName `
        -replace "{{CUSTOM_DOMAINS_ENABLED}}", $customDomainsEnabled.ToString().ToLower() `
        -replace "{{API_CUSTOM_DOMAIN}}", $apiCustomDomain `
        -replace "{{MCP_CUSTOM_DOMAIN}}", $mcpCustomDomain
    
    # Ensure .vscode directory exists
    $vscodePath = Split-Path $SettingsPath -Parent
    if (-not (Test-Path $vscodePath)) {
        Write-Host "📁 Creating .vscode directory..." -ForegroundColor Yellow
        New-Item -Path $vscodePath -ItemType Directory -Force | Out-Null
    }
    
    # Check if settings.json already exists
    if (Test-Path $SettingsPath) {
        Write-Host "⚠️  Settings file already exists: $SettingsPath" -ForegroundColor Yellow
        $response = Read-Host "Do you want to overwrite it? (y/N)"
        if ($response -ne "y" -and $response -ne "Y") {
            $backupPath = "$SettingsPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Write-Host "📄 Creating backup: $backupPath" -ForegroundColor Yellow
            Copy-Item $SettingsPath $backupPath
        }
    }
    
    # Write the settings file
    Write-Host "✍️  Writing VS Code settings to: $SettingsPath" -ForegroundColor Yellow
    $settings | Out-File -FilePath $SettingsPath -Encoding UTF8
    
    Write-Host "✅ VS Code settings configured successfully!" -ForegroundColor Green
    
    # Show DNS configuration if custom domains are enabled
    if ($customDomainsEnabled -eq $true -and $outputs.dnsRecordsNeeded) {
        Write-Host "`n🌐 DNS Configuration Required:" -ForegroundColor Cyan
        Write-Host $outputs.dnsRecordsNeeded.value -ForegroundColor Yellow
        Write-Host "`nCreate these CNAME records in your DNS:" -ForegroundColor Cyan
        Write-Host "  $apiCustomDomain -> $apiAppName.azurewebsites.net" -ForegroundColor White
        Write-Host "  $mcpCustomDomain -> $mcpAppName.azurewebsites.net" -ForegroundColor White
    }
    
    Write-Host "`n🎉 Configuration complete! Your VS Code workspace is ready to use." -ForegroundColor Green
    
} catch {
    Write-Error "❌ Error configuring VS Code settings: $($_.Exception.Message)"
    exit 1
}
