# 🔧 Test Authentication Script
# Use this to debug authentication issues step by step

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json"
)

Write-Host "🔧 Testing Authentication for COE Provisioning" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# Load configuration
if (-not (Test-Path $ConfigFile)) {
    Write-Host "❌ Configuration file not found: $ConfigFile" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
    Write-Host "✅ Configuration loaded" -ForegroundColor Green
    Write-Host "   Tenant Domain: $($config.tenantDomain)" -ForegroundColor White
    Write-Host "   Tenant ID: $($config.tenantId)" -ForegroundColor White
    Write-Host "   Subscription: $($config.azureSubscriptionId)" -ForegroundColor White
}
catch {
    Write-Host "❌ Failed to load configuration: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🧹 Step 1: Clearing all authentication caches..." -ForegroundColor Yellow

try {
    Disconnect-MgGraph -ErrorAction SilentlyContinue | Out-Null
    Write-Host "   ✓ Disconnected Microsoft Graph" -ForegroundColor Green
}
catch {
    Write-Host "   - Microsoft Graph not connected" -ForegroundColor Gray
}

try {
    Disconnect-AzAccount -ErrorAction SilentlyContinue | Out-Null
    Write-Host "   ✓ Disconnected Azure Account" -ForegroundColor Green
}
catch {
    Write-Host "   - Azure Account not connected" -ForegroundColor Gray
}

try {
    Clear-AzContext -Force -ErrorAction SilentlyContinue | Out-Null
    Write-Host "   ✓ Cleared Azure Context" -ForegroundColor Green
}
catch {
    Write-Host "   - Azure Context already clear" -ForegroundColor Gray
}

try {
    if (Get-Command "Clear-MgContext" -ErrorAction SilentlyContinue) {
        Clear-MgContext -ErrorAction SilentlyContinue | Out-Null
        Write-Host "   ✓ Cleared Microsoft Graph Context" -ForegroundColor Green
    }
}
catch {
    Write-Host "   - Microsoft Graph Context not available" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔌 Step 2: Testing Microsoft Graph Authentication..." -ForegroundColor Yellow
Write-Host "   Target Tenant: $($config.tenantId)" -ForegroundColor White
Write-Host "   When browser opens, authenticate with an account from: $($config.tenantDomain)" -ForegroundColor Cyan
Write-Host ""

try {
    Connect-MgGraph -TenantId $config.tenantId `
        -Scopes "User.ReadWrite.All", "Directory.ReadWrite.All", "RoleManagement.ReadWrite.Directory" `
        -UseDeviceAuthentication:$false `
        -NoWelcome
    
    $mgContext = Get-MgContext
    if ($mgContext) {
        Write-Host "   ✅ Microsoft Graph connected successfully!" -ForegroundColor Green
        Write-Host "   Connected Tenant: $($mgContext.TenantId)" -ForegroundColor White
        Write-Host "   Account: $($mgContext.Account)" -ForegroundColor White
        
        if ($mgContext.TenantId -eq $config.tenantId) {
            Write-Host "   ✅ Correct tenant confirmed!" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ Wrong tenant! Expected: $($config.tenantId)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   ❌ No context returned" -ForegroundColor Red
    }
}
catch {
    Write-Host "   ❌ Microsoft Graph authentication failed: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "1. Make sure you authenticate with an account from $($config.tenantDomain)" -ForegroundColor White
    Write-Host "2. Check that the account has Global Admin rights" -ForegroundColor White
    Write-Host "3. Try running .\Clear-AuthCache.ps1 first" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "🔌 Step 3: Testing Azure Authentication..." -ForegroundColor Yellow
Write-Host "   Using device authentication to avoid account selection..." -ForegroundColor White

try {
    # Use device authentication which is more reliable
    Connect-AzAccount -TenantId $config.tenantId `
        -SubscriptionId $config.azureSubscriptionId `
        -UseDeviceAuthentication | Out-Null
    
    $azContext = Get-AzContext
    if ($azContext) {
        Write-Host "   ✅ Azure connected successfully!" -ForegroundColor Green
        Write-Host "   Connected Tenant: $($azContext.Tenant.Id)" -ForegroundColor White
        Write-Host "   Account: $($azContext.Account.Id)" -ForegroundColor White
        Write-Host "   Subscription: $($azContext.Subscription.Id)" -ForegroundColor White
        
        if ($azContext.Subscription.Id -eq $config.azureSubscriptionId) {
            Write-Host "   ✅ Correct subscription confirmed!" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ Wrong subscription! Expected: $($config.azureSubscriptionId)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   ❌ No Azure context returned" -ForegroundColor Red
    }
}
catch {
    Write-Host "   ❌ Azure authentication failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 All authentication tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "✅ You can now run the provisioning script:" -ForegroundColor Cyan
Write-Host "   .\Provision-COE-Users.ps1 -ConfigFile '$ConfigFile' -WhatIf" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
