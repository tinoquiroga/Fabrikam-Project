# 🔧 Fix COE User Role Assignments
# This script assigns Contributor roles to users who might be missing them

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json"
)

Write-Host "🔧 COE Role Assignment Fix Tool" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

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
Write-Host "👥 Processing user role assignments..." -ForegroundColor Cyan

$results = @()
$activeUsers = $config.users | Where-Object { $_.enabled -eq $true }

foreach ($userData in $activeUsers) {
    $userPrincipalName = "$($userData.username)@$($config.tenantDomain)"
    Write-Host ""
    Write-Host "🔄 Processing: $($userData.displayName)" -ForegroundColor White
    Write-Host "   UPN: $userPrincipalName" -ForegroundColor Gray
    
    try {
        # Check current role assignments
        Write-Host "   🔍 Checking existing role assignments..." -ForegroundColor Gray
        $existingAssignment = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction SilentlyContinue
        
        if ($existingAssignment) {
            Write-Host "   ✅ User already has Contributor role" -ForegroundColor Green
            $results += [PSCustomObject]@{
                DisplayName = $userData.displayName
                UserPrincipalName = $userPrincipalName
                Status = "Already Assigned"
            }
        }
        else {
            Write-Host "   🔐 Assigning Contributor role..." -ForegroundColor Yellow
            try {
                New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction Stop | Out-Null
                Write-Host "   ✅ Contributor role assigned successfully" -ForegroundColor Green
                $results += [PSCustomObject]@{
                    DisplayName = $userData.displayName
                    UserPrincipalName = $userPrincipalName
                    Status = "Newly Assigned"
                }
            }
            catch {
                Write-Host "   ❌ Failed to assign Contributor role: $($_.Exception.Message)" -ForegroundColor Red
                $results += [PSCustomObject]@{
                    DisplayName = $userData.displayName
                    UserPrincipalName = $userPrincipalName
                    Status = "Failed: $($_.Exception.Message)"
                }
            }
        }
    }
    catch {
        Write-Host "   ❌ Error checking user: $($_.Exception.Message)" -ForegroundColor Red
        $results += [PSCustomObject]@{
            DisplayName = $userData.displayName
            UserPrincipalName = $userPrincipalName
            Status = "Error: $($_.Exception.Message)"
        }
    }
}

# Display summary
Write-Host ""
Write-Host "📊 ROLE ASSIGNMENT SUMMARY" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
$results | Format-Table DisplayName, Status -AutoSize

# Verification
Write-Host ""
Write-Host "🔍 Final verification - listing all Contributor assignments..." -ForegroundColor Cyan
try {
    $allContributors = Get-AzRoleAssignment -RoleDefinitionName "Contributor" -Scope "/subscriptions/$($config.azureSubscriptionId)" | 
                      Where-Object { $_.SignInName -like "*@$($config.tenantDomain)" } |
                      Select-Object DisplayName, SignInName, RoleDefinitionName
    
    if ($allContributors) {
        Write-Host "✅ Found $($allContributors.Count) Contributor assignments for your domain:" -ForegroundColor Green
        $allContributors | Format-Table -AutoSize
    }
    else {
        Write-Host "⚠️  No Contributor assignments found for your domain" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "❌ Failed to retrieve role assignments: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "✅ Role assignment fix completed!" -ForegroundColor Green
Write-Host "💡 Users should now have Contributor access to the entire subscription" -ForegroundColor Yellow
