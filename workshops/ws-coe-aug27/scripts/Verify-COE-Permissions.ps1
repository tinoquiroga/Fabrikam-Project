# 🔍 Verify COE Permissions Summary
# Shows what users should see in Azure Portal

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json"
)

Write-Host "🔍 COE Permissions Verification" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Load configuration
if (Test-Path $ConfigFile) {
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Host "✅ Configuration loaded: $($config.users.Count) users" -ForegroundColor Green
}
else {
    Write-Host "❌ Configuration file not found: $ConfigFile" -ForegroundColor Red
    exit 1
}

# Check Azure connection
$azContext = Get-AzContext -ErrorAction SilentlyContinue
if (-not $azContext -or $azContext.Subscription.Id -ne $config.azureSubscriptionId) {
    Write-Host "⚠️  Connecting to Azure..." -ForegroundColor Yellow
    Connect-AzAccount -SubscriptionId $config.azureSubscriptionId | Out-Null
}

Write-Host ""
Write-Host "📊 SUBSCRIPTION-LEVEL PERMISSIONS" -ForegroundColor Yellow
Write-Host "===================================" -ForegroundColor Yellow

Write-Host ""
Write-Host "📖 Reader Access (can see all resources):" -ForegroundColor Green
$readerAssignments = Get-AzRoleAssignment -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" | 
                     Where-Object { $_.SignInName -like "*@$($config.tenantDomain)" }

if ($readerAssignments) {
    $readerAssignments | Select-Object DisplayName, SignInName | Format-Table -AutoSize
}
else {
    Write-Host "❌ No Reader assignments found" -ForegroundColor Red
}

Write-Host ""
Write-Host "🚫 Contributor Access (should be empty at subscription level):" -ForegroundColor Red
$contributorAssignments = Get-AzRoleAssignment -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" | 
                          Where-Object { $_.SignInName -like "*@$($config.tenantDomain)" }

if ($contributorAssignments) {
    Write-Host "⚠️  Found unexpected subscription-level Contributor assignments:" -ForegroundColor Yellow
    $contributorAssignments | Select-Object DisplayName, SignInName | Format-Table -AutoSize
}
else {
    Write-Host "✅ No subscription-level Contributor assignments (correct)" -ForegroundColor Green
}

Write-Host ""
Write-Host "🔐 RESOURCE GROUP-LEVEL PERMISSIONS" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow

$activeUsers = $config.users | Where-Object { $_.enabled -eq $true }

foreach ($userData in $activeUsers) {
    $resourceGroupName = "rg-fabrikam-coe-$($userData.username)"
    Write-Host ""
    Write-Host "📁 $resourceGroupName (for $($userData.displayName)):" -ForegroundColor Cyan
    
    $rgExists = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
    if ($rgExists) {
        $rgContributor = Get-AzRoleAssignment -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue |
                        Where-Object { $_.SignInName -eq "$($userData.username)@$($config.tenantDomain)" }
        
        if ($rgContributor) {
            Write-Host "   ✅ $($userData.displayName) has Contributor access" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ $($userData.displayName) missing Contributor access" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   ❌ Resource group not found" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎯 SUMMARY - What users can do:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "📖 View all resources in subscription (Reader access)" -ForegroundColor White
Write-Host "🔐 Create/modify resources only in their own resource group (Contributor access)" -ForegroundColor White
Write-Host "👥 See each other's work but cannot modify it" -ForegroundColor White
Write-Host "🚫 Cannot create resources outside their assigned resource group" -ForegroundColor White

Write-Host ""
Write-Host "💡 In Azure Portal, users will see:" -ForegroundColor Yellow
Write-Host "   • Subscription: Reader (inherited permissions)" -ForegroundColor White
Write-Host "   • Their RG: Contributor (direct assignment)" -ForegroundColor White
Write-Host "   • Other RGs: Reader (inherited from subscription)" -ForegroundColor White
