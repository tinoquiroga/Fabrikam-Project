# Fix User Access Administrator Permissions for COE Users
# This script adds the missing User Access Administrator role to existing COE users
# so they can deploy ARM templates that create role assignments

param(
    [string]$ConfigFile = "coe-config.json",
    [switch]$WhatIf
)

Write-Host "🔧 Fix User Access Administrator Permissions for COE Users" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# Load configuration
try {
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Host "✅ Configuration loaded from: $ConfigFile" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to load configuration from $ConfigFile" -ForegroundColor Red
    exit 1
}

# Connect to Azure (with session reuse)
try {
    $context = Get-AzContext
    if (-not $context -or $context.Subscription.Id -ne $config.azureSubscriptionId) {
        Write-Host "🔐 Connecting to Azure..." -ForegroundColor Yellow
        Connect-AzAccount -TenantId $config.tenantId -SubscriptionId $config.azureSubscriptionId | Out-Null
    }
    else {
        Write-Host "✅ Using existing Azure session" -ForegroundColor Green
    }
    
    Set-AzContext -SubscriptionId $config.azureSubscriptionId | Out-Null
    Write-Host "✅ Azure context set to subscription: $($config.azureSubscriptionId)" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to connect to Azure: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Process each user
foreach ($user in $config.users) {
    $username = $user.username
    $userPrincipalName = "$username@$($config.tenantDomain)"
    $resourceGroupName = "rg-fabrikam-coe-$username"
    
    Write-Host ""
    Write-Host "👤 Processing user: $username" -ForegroundColor White
    Write-Host "📧 UPN: $userPrincipalName" -ForegroundColor Gray
    Write-Host "📁 Resource Group: $resourceGroupName" -ForegroundColor Gray
    
    if (-not $WhatIf) {
        try {
            # Check if resource group exists
            $rg = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
            if (-not $rg) {
                Write-Host "  ⚠️  Resource group $resourceGroupName does not exist - skipping" -ForegroundColor Yellow
                continue
            }
            
            # Check current User Access Administrator role
            $existingUserAccess = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "User Access Administrator" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue
            
            if ($existingUserAccess) {
                Write-Host "  ✅ User already has User Access Administrator role" -ForegroundColor Green
            }
            else {
                Write-Host "  🔑 Adding User Access Administrator role..." -ForegroundColor Yellow
                New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "User Access Administrator" -ResourceGroupName $resourceGroupName -ErrorAction Stop | Out-Null
                Write-Host "  ✅ User Access Administrator role assigned successfully" -ForegroundColor Green
            }
            
            # Verify all required roles
            Write-Host "  📋 Current role assignments:" -ForegroundColor Cyan
            
            $readerRole = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction SilentlyContinue
            $contributorRole = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue
            $userAccessRole = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "User Access Administrator" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue
            
            Write-Host "    📖 Reader (Subscription): $(if ($readerRole) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($readerRole) { 'Green' } else { 'Red' })
            Write-Host "    🔐 Contributor (RG): $(if ($contributorRole) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($contributorRole) { 'Green' } else { 'Red' })
            Write-Host "    🔑 User Access Admin (RG): $(if ($userAccessRole) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($userAccessRole) { 'Green' } else { 'Red' })
        }
        catch {
            Write-Host "  ❌ Failed to update user permissions: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "  [WHAT-IF] Would check and assign User Access Administrator role to RG: $resourceGroupName" -ForegroundColor Magenta
    }
}

Write-Host ""
Write-Host "🎯 Summary" -ForegroundColor Cyan
Write-Host "=" * 30 -ForegroundColor Gray
if (-not $WhatIf) {
    Write-Host "✅ Permission fix complete!" -ForegroundColor Green
    Write-Host "💡 Users can now deploy ARM templates that create role assignments" -ForegroundColor White
}
else {
    Write-Host "🔍 What-If mode completed - no changes made" -ForegroundColor Magenta
    Write-Host "💡 Run without -WhatIf to apply changes" -ForegroundColor White
}

Write-Host ""
Write-Host "📋 Required Permissions for ARM Template Deployment:" -ForegroundColor Cyan
Write-Host "  📖 Reader - Subscription level (view all resources)" -ForegroundColor White
Write-Host "  🔐 Contributor - Resource Group level (create/modify resources)" -ForegroundColor White  
Write-Host "  🔑 User Access Administrator - Resource Group level (assign roles to Key Vault)" -ForegroundColor White
