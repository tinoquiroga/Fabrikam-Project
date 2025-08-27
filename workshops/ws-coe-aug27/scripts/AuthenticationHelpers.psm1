# 🔧 Authentication Helper Functions
# Functions to handle authentication more reliably

function Clear-AllAuthenticationSessions {
    Write-Host "🧹 Clearing all authentication sessions..." -ForegroundColor Cyan
    
    try {
        # Disconnect Graph
        if (Get-Command "Disconnect-MgGraph" -ErrorAction SilentlyContinue) {
            Disconnect-MgGraph -ErrorAction SilentlyContinue | Out-Null
        }
        
        # Disconnect Azure
        if (Get-Command "Disconnect-AzAccount" -ErrorAction SilentlyContinue) {
            Disconnect-AzAccount -ErrorAction SilentlyContinue | Out-Null
        }
        
        # Clear Azure contexts
        if (Get-Command "Clear-AzContext" -ErrorAction SilentlyContinue) {
            Clear-AzContext -Force -ErrorAction SilentlyContinue | Out-Null
        }
        
        # Clear Graph contexts if available
        if (Get-Command "Clear-MgContext" -ErrorAction SilentlyContinue) {
            Clear-MgContext -ErrorAction SilentlyContinue | Out-Null
        }
        
        Write-Host "   ✓ Authentication cache cleared" -ForegroundColor Green
    }
    catch {
        Write-Host "   ⚠️  Some clearing operations failed (this is normal)" -ForegroundColor Yellow
    }
}

function Connect-TenantSpecificGraph {
    param(
        [string]$TenantId,
        [string]$TenantDomain
    )
    
    Write-Host "🔌 Connecting to Microsoft Graph..." -ForegroundColor Cyan
    Write-Host "   Target Tenant: $TenantId" -ForegroundColor Gray
    Write-Host "   Domain: $TenantDomain" -ForegroundColor Gray
    Write-Host "   When prompted, use an account from: $TenantDomain" -ForegroundColor Yellow
    
    try {
        Connect-MgGraph -TenantId $TenantId `
            -Scopes "User.ReadWrite.All", "Directory.ReadWrite.All", "RoleManagement.ReadWrite.Directory" `
            -UseDeviceAuthentication:$false `
            -NoWelcome
        
        $context = Get-MgContext
        if ($context) {
            Write-Host "   ✅ Microsoft Graph connected!" -ForegroundColor Green
            Write-Host "   Account: $($context.Account)" -ForegroundColor White
            Write-Host "   Tenant: $($context.TenantId)" -ForegroundColor White
            
            if ($context.TenantId -eq $TenantId) {
                Write-Host "   ✅ Correct tenant confirmed" -ForegroundColor Green
                return $context
            }
            else {
                throw "Connected to wrong tenant. Expected: $TenantId, Got: $($context.TenantId)"
            }
        }
        else {
            throw "No Microsoft Graph context returned"
        }
    }
    catch {
        throw "Microsoft Graph authentication failed: $($_.Exception.Message)"
    }
}

function Connect-TenantSpecificAzure {
    param(
        [string]$TenantId,
        [string]$SubscriptionId,
        [object]$GraphContext = $null
    )
    
    Write-Host "🔌 Connecting to Azure..." -ForegroundColor Cyan
    Write-Host "   Target Tenant: $TenantId" -ForegroundColor Gray
    Write-Host "   Target Subscription: $SubscriptionId" -ForegroundColor Gray
    
    try {
        # First, try standard authentication
        try {
            if ($GraphContext -and $GraphContext.Account) {
                Write-Host "   Trying with Graph account: $($GraphContext.Account)" -ForegroundColor Gray
                Connect-AzAccount -TenantId $TenantId `
                    -SubscriptionId $SubscriptionId `
                    -AccountId $GraphContext.Account `
                    -Force | Out-Null
            }
            else {
                Connect-AzAccount -TenantId $TenantId `
                    -SubscriptionId $SubscriptionId `
                    -Force | Out-Null
            }
        }
        catch {
            Write-Host "   Standard authentication failed, trying device authentication..." -ForegroundColor Yellow
            Connect-AzAccount -TenantId $TenantId `
                -SubscriptionId $SubscriptionId `
                -UseDeviceAuthentication | Out-Null
        }
        
        $context = Get-AzContext
        if ($context) {
            Write-Host "   ✅ Azure connected!" -ForegroundColor Green
            Write-Host "   Account: $($context.Account.Id)" -ForegroundColor White
            Write-Host "   Tenant: $($context.Tenant.Id)" -ForegroundColor White
            Write-Host "   Subscription: $($context.Subscription.Id)" -ForegroundColor White
            
            if ($context.Subscription.Id -eq $SubscriptionId) {
                Write-Host "   ✅ Correct subscription confirmed" -ForegroundColor Green
                return $context
            }
            else {
                throw "Connected to wrong subscription. Expected: $SubscriptionId, Got: $($context.Subscription.Id)"
            }
        }
        else {
            throw "No Azure context returned"
        }
    }
    catch {
        throw "Azure authentication failed: $($_.Exception.Message)"
    }
}

# Export functions for use in other scripts
Export-ModuleMember -Function Clear-AllAuthenticationSessions, Connect-TenantSpecificGraph, Connect-TenantSpecificAzure
