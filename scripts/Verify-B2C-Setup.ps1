# 🔐 Azure AD B2C Setup Verification Script
# Run this after completing the B2C tenant setup to verify configuration

param(
    [switch]$ShowConfig,
    [switch]$TestConnection,
    [switch]$ListUsers
)

Write-Host "🔐 Azure AD B2C Setup Verification" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Check if we're in the correct Azure context
Write-Host "`n📋 Checking Azure Context..." -ForegroundColor Yellow

try {
    $context = az account show --output json | ConvertFrom-Json
    $currentSubscription = $context.name
    $currentTenant = $context.tenantId
    
    Write-Host "✅ Current Subscription: $currentSubscription" -ForegroundColor Green
    Write-Host "✅ Current Tenant: $currentTenant" -ForegroundColor Green
    
    # Verify we're in the correct subscription
    $expectedSubscription = "MCAPS-Hybrid-REQ-59531-2023-davidb"
    $expectedTenant = "16b3c013-d300-468d-ac64-7eda0820b6d3"
    
    if ($currentSubscription -eq $expectedSubscription) {
        Write-Host "✅ Correct subscription confirmed" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Expected subscription: $expectedSubscription" -ForegroundColor Yellow
        Write-Host "   Run: az account set --subscription '$expectedSubscription'" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "❌ Azure CLI not authenticated or available" -ForegroundColor Red
    Write-Host "   Run: az login" -ForegroundColor Cyan
    exit 1
}

# Check for authentication resources in resource group
Write-Host "`n🏗️ Checking Authentication Resources..." -ForegroundColor Yellow

try {
    # Check for SQL Server resources
    $sqlServers = az sql server list --resource-group "rg-fabrikam-dev" --output json | ConvertFrom-Json
    
    if ($sqlServers.Count -gt 0) {
        Write-Host "✅ SQL Server resources found in rg-fabrikam-dev:" -ForegroundColor Green
        foreach ($server in $sqlServers) {
            Write-Host "   - Server: $($server.name)" -ForegroundColor White
            Write-Host "   - Location: $($server.location)" -ForegroundColor White
            
            # Check for databases
            $databases = az sql db list --server $server.name --resource-group "rg-fabrikam-dev" --output json | ConvertFrom-Json
            foreach ($db in $databases) {
                if ($db.name -ne "master") {
                    Write-Host "   - Database: $($db.name)" -ForegroundColor White
                }
            }
        }
    }
    else {
        Write-Host "❌ No SQL Server found in rg-fabrikam-dev" -ForegroundColor Red
        Write-Host "   Run: .\scripts\Setup-Authentication-Strategy.ps1" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "⚠️  Could not check SQL Server resources" -ForegroundColor Yellow
}

if ($ShowConfig) {
    Write-Host "`n📝 Configuration Template:" -ForegroundColor Yellow
    Write-Host "=========================" -ForegroundColor Yellow
    
    $configPath = "docs/deployment/b2c-configuration-template.jsonc"
    $authStrategyPath = "docs/architecture/DUAL-AUTHENTICATION-STRATEGY.md"
    
    if (Test-Path $authStrategyPath) {
        Write-Host "✅ Dual authentication strategy guide: $authStrategyPath" -ForegroundColor Green
        Write-Host "   Review strategy selection based on permissions" -ForegroundColor Cyan
    }
    
    if (Test-Path $configPath) {
        Write-Host "✅ Configuration template available at: $configPath" -ForegroundColor Green
        Write-Host "   Update this file with actual values after setup" -ForegroundColor Cyan
    }
    else {
        Write-Host "❌ Configuration template not found" -ForegroundColor Red
    }
}

if ($TestConnection) {
    Write-Host "`n🔗 Testing B2C Connection..." -ForegroundColor Yellow
    Write-Host "This feature will be implemented after B2C tenant creation" -ForegroundColor Cyan
    Write-Host "Manual verification steps:" -ForegroundColor White
    Write-Host "1. Switch to B2C tenant in Azure Portal" -ForegroundColor White
    Write-Host "2. Verify user flows are accessible" -ForegroundColor White
    Write-Host "3. Test application registration" -ForegroundColor White
}

if ($ListUsers) {
    Write-Host "`n👥 B2C Users Check..." -ForegroundColor Yellow
    Write-Host "User management will be available after B2C tenant setup" -ForegroundColor Cyan
}

Write-Host "`n📚 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Review strategy guide: docs/architecture/DUAL-AUTHENTICATION-STRATEGY.md" -ForegroundColor White
Write-Host "2. Run authentication setup: .\scripts\Setup-Authentication-Strategy.ps1" -ForegroundColor White
Write-Host "3. Implement ASP.NET Core Identity authentication" -ForegroundColor White
Write-Host "4. Update FabrikamApi project with auth middleware" -ForegroundColor White
Write-Host "5. Test authentication endpoints and user flows" -ForegroundColor White

Write-Host "`n🎯 Issue Tracking:" -ForegroundColor Yellow
Write-Host "GitHub Issue #3: Authentication Setup (ASP.NET Identity Strategy)" -ForegroundColor White
Write-Host "Branch: feature/phase-1-authentication" -ForegroundColor White
