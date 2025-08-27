# 🚀 Simple Authentication Solution
# This script demonstrates the most reliable authentication approach

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json"
)

# Import our authentication helpers
Import-Module ".\AuthenticationHelpers.psm1" -Force

Write-Host "🚀 COE Authentication Test - Simplified Approach" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Load configuration
try {
    $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
    Write-Host "✅ Configuration loaded" -ForegroundColor Green
    Write-Host "   Tenant: $($config.tenantDomain) ($($config.tenantId))" -ForegroundColor White
    Write-Host "   Subscription: $($config.azureSubscriptionId)" -ForegroundColor White
}
catch {
    Write-Host "❌ Failed to load configuration: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Clear authentication cache
Clear-AllAuthenticationSessions

Write-Host ""

# Connect to Microsoft Graph
try {
    $graphContext = Connect-TenantSpecificGraph -TenantId $config.tenantId -TenantDomain $config.tenantDomain
}
catch {
    Write-Host "❌ Microsoft Graph authentication failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Connect to Azure
try {
    $azureContext = Connect-TenantSpecificAzure -TenantId $config.tenantId -SubscriptionId $config.azureSubscriptionId -GraphContext $graphContext
}
catch {
    Write-Host "❌ Azure authentication failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 Authentication successful!" -ForegroundColor Green
Write-Host ""
Write-Host "✅ Ready to run provisioning:" -ForegroundColor Cyan
Write-Host "   .\Provision-COE-Users.ps1 -ConfigFile '$ConfigFile' -WhatIf" -ForegroundColor White
Write-Host ""
