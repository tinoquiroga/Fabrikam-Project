# 🔧 Fix COE User Permissions
# Remove subscription-level Contributor and set proper Reader + Resource Group Contributor

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json"
)

Write-Host "🔧 COE Permission Correction Tool" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

# Load configuration
if (Test-Path $ConfigFile) {
    try {
        $config = Get-Content $ConfigFile | ConvertFrom-Json
        Write-Host "✅ Loaded configuration from: $ConfigFile" -ForegroundColor Green
        Write-Host "   Tenant: $($config.tenantDomain)" -ForegroundColor White
        Write-Host "   Subscription: $($config.azureSubscriptionId)" -ForegroundColor White
        Write-Host "   Users: $($config.users.Count)" -ForegroundColor White
    }
    catch {
        Write-Host "❌ Failed to load configuration: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "❌ Configuration file not found: $ConfigFile" -ForegroundColor Red
    exit 1
}

# Check Azure connection
Write-Host ""
Write-Host "🔍 Checking Azure connection..." -ForegroundColor Cyan
$azContext = Get-AzContext -ErrorAction SilentlyContinue
if (-not $azContext -or $azContext.Subscription.Id -ne $config.azureSubscriptionId) {
    Write-Host "⚠️  Not connected to correct Azure subscription. Connecting..." -ForegroundColor Yellow
    try {
        Connect-AzAccount -SubscriptionId $config.azureSubscriptionId | Out-Null
        Write-Host "✅ Connected to Azure" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to connect to Azure: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "✅ Already connected to correct subscription" -ForegroundColor Green
}

# Process each user
Write-Host ""
Write-Host "👥 Fixing user permissions..." -ForegroundColor Cyan

$results = @()
$activeUsers = $config.users | Where-Object { $_.enabled -eq $true }

foreach ($userData in $activeUsers) {
    $userPrincipalName = "$($userData.username)@$($config.tenantDomain)"
    $resourceGroupName = "rg-fabrikam-coe-$($userData.username)"
    
    Write-Host ""
    Write-Host "🔄 Processing: $($userData.displayName)" -ForegroundColor White
    Write-Host "   UPN: $userPrincipalName" -ForegroundColor Gray
    Write-Host "   Resource Group: $resourceGroupName" -ForegroundColor Gray
    
    $userResult = [PSCustomObject]@{
        DisplayName = $userData.displayName
        UserPrincipalName = $userPrincipalName
        ResourceGroup = $resourceGroupName
        SubscriptionContributor = "Not Processed"
        SubscriptionReader = "Not Processed"
        ResourceGroupContributor = "Not Processed"
    }
    
    try {
        # 1. Remove subscription-level Contributor if it exists
        Write-Host "   🔍 Checking for subscription-level Contributor..." -ForegroundColor Gray
        $subContributor = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction SilentlyContinue
        
        if ($subContributor) {
            Write-Host "   🗑️  Removing subscription-level Contributor..." -ForegroundColor Yellow
            try {
                Remove-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" -Force -ErrorAction Stop
                Write-Host "   ✅ Removed subscription-level Contributor" -ForegroundColor Green
                $userResult.SubscriptionContributor = "Removed"
            }
            catch {
                Write-Host "   ❌ Failed to remove subscription Contributor: $($_.Exception.Message)" -ForegroundColor Red
                $userResult.SubscriptionContributor = "Failed to Remove"
            }
        }
        else {
            Write-Host "   ✅ No subscription-level Contributor found" -ForegroundColor Green
            $userResult.SubscriptionContributor = "Not Found"
        }
        
        # 2. Add subscription-level Reader if not exists
        Write-Host "   🔍 Checking for subscription-level Reader..." -ForegroundColor Gray
        $subReader = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction SilentlyContinue
        
        if (-not $subReader) {
            Write-Host "   📖 Adding subscription-level Reader..." -ForegroundColor Yellow
            try {
                New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction Stop | Out-Null
                Write-Host "   ✅ Added subscription-level Reader" -ForegroundColor Green
                $userResult.SubscriptionReader = "Added"
            }
            catch {
                Write-Host "   ❌ Failed to add subscription Reader: $($_.Exception.Message)" -ForegroundColor Red
                $userResult.SubscriptionReader = "Failed to Add"
            }
        }
        else {
            Write-Host "   ✅ Already has subscription-level Reader" -ForegroundColor Green
            $userResult.SubscriptionReader = "Already Exists"
        }
        
        # 3. Add resource group-level Contributor if not exists
        Write-Host "   🔍 Checking for resource group-level Contributor..." -ForegroundColor Gray
        $rgExists = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
        
        if ($rgExists) {
            $rgContributor = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue
            
            if (-not $rgContributor) {
                Write-Host "   🔐 Adding resource group-level Contributor..." -ForegroundColor Yellow
                try {
                    New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction Stop | Out-Null
                    Write-Host "   ✅ Added resource group-level Contributor" -ForegroundColor Green
                    $userResult.ResourceGroupContributor = "Added"
                }
                catch {
                    Write-Host "   ❌ Failed to add resource group Contributor: $($_.Exception.Message)" -ForegroundColor Red
                    $userResult.ResourceGroupContributor = "Failed to Add"
                }
            }
            else {
                Write-Host "   ✅ Already has resource group-level Contributor" -ForegroundColor Green
                $userResult.ResourceGroupContributor = "Already Exists"
            }
        }
        else {
            Write-Host "   ❌ Resource group not found: $resourceGroupName" -ForegroundColor Red
            $userResult.ResourceGroupContributor = "RG Not Found"
        }
    }
    catch {
        Write-Host "   ❌ Error processing user: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    $results += $userResult
}

# Display summary
Write-Host ""
Write-Host "📊 PERMISSION CORRECTION SUMMARY" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
$results | Format-Table DisplayName, SubscriptionContributor, SubscriptionReader, ResourceGroupContributor -AutoSize

# Final verification
Write-Host ""
Write-Host "🔍 Final verification..." -ForegroundColor Cyan

Write-Host ""
Write-Host "📖 Subscription-level Reader assignments:" -ForegroundColor Yellow
$subReaders = Get-AzRoleAssignment -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" | 
              Where-Object { $_.SignInName -like "*@$($config.tenantDomain)" } |
              Select-Object DisplayName, SignInName
if ($subReaders) {
    $subReaders | Format-Table -AutoSize
}
else {
    Write-Host "   None found" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔐 Resource group-level Contributor assignments:" -ForegroundColor Yellow
foreach ($userData in $activeUsers) {
    $resourceGroupName = "rg-fabrikam-coe-$($userData.username)"
    $rgContributors = Get-AzRoleAssignment -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue |
                      Where-Object { $_.SignInName -like "*@$($config.tenantDomain)" }
    if ($rgContributors) {
        Write-Host "   ${resourceGroupName}:" -ForegroundColor White
        $rgContributors | Select-Object DisplayName, SignInName | Format-Table -AutoSize
    }
}

Write-Host ""
Write-Host "✅ Permission correction completed!" -ForegroundColor Green
Write-Host "💡 Users now have:" -ForegroundColor Yellow
Write-Host "   📖 Reader access to entire subscription (can see all resources)" -ForegroundColor White
Write-Host "   🔐 Contributor access only to their own resource group" -ForegroundColor White
