# 🧪 Test New User Addition
# This script tests adding only new users while gracefully skipping existing ones

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json",
    [switch]$WhatIf
)

Write-Host "🧪 Testing New User Addition" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

# Load configuration
if (Test-Path $ConfigFile) {
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Host "✅ Configuration loaded: $($config.users.Count) users" -ForegroundColor Green
}
else {
    Write-Host "❌ Configuration file not found: $ConfigFile" -ForegroundColor Red
    exit 1
}

# Check connections
Write-Host ""
Write-Host "🔍 Checking authentication..." -ForegroundColor Cyan

# Microsoft Graph
try {
    $graphContext = Get-MgContext -ErrorAction SilentlyContinue
    if ($graphContext -and $graphContext.TenantId -eq $config.tenantId) {
        Write-Host "✅ Microsoft Graph already connected" -ForegroundColor Green
    }
    else {
        Write-Host "🔌 Connecting to Microsoft Graph..." -ForegroundColor Yellow
        Connect-MgGraph -TenantId $config.tenantId | Out-Null
        Write-Host "✅ Microsoft Graph connected" -ForegroundColor Green
    }
}
catch {
    Write-Host "❌ Failed to connect to Microsoft Graph: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Azure
try {
    $azContext = Get-AzContext -ErrorAction SilentlyContinue
    if ($azContext -and $azContext.Subscription.Id -eq $config.azureSubscriptionId) {
        Write-Host "✅ Azure already connected" -ForegroundColor Green
    }
    else {
        Write-Host "🔌 Connecting to Azure..." -ForegroundColor Yellow
        Connect-AzAccount -SubscriptionId $config.azureSubscriptionId | Out-Null
        Write-Host "✅ Azure connected" -ForegroundColor Green
    }
}
catch {
    Write-Host "❌ Failed to connect to Azure: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "👥 Processing users..." -ForegroundColor Cyan

$results = @()
$activeUsers = $config.users | Where-Object { $_.enabled -ne $false }

foreach ($userData in $activeUsers) {
    $userPrincipalName = "$($userData.username)@$($config.tenantDomain)"
    $resourceGroupName = "rg-fabrikam-coe-$($userData.username)"
    
    Write-Host ""
    Write-Host "🔄 Processing: $($userData.displayName)" -ForegroundColor White
    Write-Host "   UPN: $userPrincipalName" -ForegroundColor Gray
    
    # Check if user exists
    try {
        $existingUser = Get-MgUser -UserId $userPrincipalName -ErrorAction SilentlyContinue
        
        if ($existingUser) {
            Write-Host "   ✅ User already exists - skipping" -ForegroundColor Green
            $results += [PSCustomObject]@{
                DisplayName = $userData.displayName
                UserPrincipalName = $userPrincipalName
                Status = "Already Exists"
                Action = "Skipped"
            }
        }
        else {
            Write-Host "   🆕 New user detected - would create" -ForegroundColor Cyan
            
            if (-not $WhatIf) {
                # Create the user
                Write-Host "   📧 Creating user..." -ForegroundColor Yellow
                
                $passwordProfile = @{
                    Password = $config.defaultPassword
                    ForceChangePasswordNextSignIn = $true
                }
                
                $newUser = New-MgUser -DisplayName $userData.displayName `
                    -UserPrincipalName $userPrincipalName `
                    -MailNickname $userData.username `
                    -PasswordProfile $passwordProfile `
                    -AccountEnabled:$true
                
                Write-Host "   ✅ User created successfully" -ForegroundColor Green
                
                # Create resource group
                Write-Host "   ☁️  Creating resource group: $resourceGroupName" -ForegroundColor Yellow
                
                $existingRG = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
                if (-not $existingRG) {
                    $tags = @{
                        "createdFor" = $userData.username
                        "purpose" = "COE-Demo"
                        "created" = (Get-Date).ToString("yyyy-MM-dd")
                        "project" = "Fabrikam-COE-Demo"
                    }
                    
                    $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location $config.azureLocation -Tag $tags
                    Write-Host "   ✅ Resource group created" -ForegroundColor Green
                }
                else {
                    Write-Host "   ✅ Resource group already exists" -ForegroundColor Green
                }
                
                # Assign roles
                Write-Host "   🔐 Assigning permissions..." -ForegroundColor Yellow
                
                # Reader at subscription level
                $existingReader = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" -ErrorAction SilentlyContinue
                if (-not $existingReader) {
                    New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$($config.azureSubscriptionId)" | Out-Null
                    Write-Host "   ✅ Reader role assigned (subscription)" -ForegroundColor Green
                }
                
                # Contributor to resource group
                $existingContributor = Get-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue
                if (-not $existingContributor) {
                    New-AzRoleAssignment -SignInName $userPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $resourceGroupName | Out-Null
                    Write-Host "   ✅ Contributor role assigned (resource group)" -ForegroundColor Green
                }
                
                $results += [PSCustomObject]@{
                    DisplayName = $userData.displayName
                    UserPrincipalName = $userPrincipalName
                    Status = "Created"
                    Action = "New User Added"
                }
            }
            else {
                Write-Host "   [WHAT-IF] Would create new user and assign permissions" -ForegroundColor Magenta
                $results += [PSCustomObject]@{
                    DisplayName = $userData.displayName
                    UserPrincipalName = $userPrincipalName
                    Status = "Would Create"
                    Action = "What-If Mode"
                }
            }
        }
    }
    catch {
        Write-Host "   ❌ Error processing user: $($_.Exception.Message)" -ForegroundColor Red
        $results += [PSCustomObject]@{
            DisplayName = $userData.displayName
            UserPrincipalName = $userPrincipalName
            Status = "Error"
            Action = $_.Exception.Message
        }
    }
}

Write-Host ""
Write-Host "📊 RESULTS SUMMARY" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
$results | Format-Table DisplayName, Status, Action -AutoSize

$newUsers = $results | Where-Object { $_.Status -eq "Created" -or $_.Status -eq "Would Create" }
$existingUsers = $results | Where-Object { $_.Status -eq "Already Exists" }

Write-Host ""
Write-Host "📈 STATISTICS:" -ForegroundColor Yellow
Write-Host "   Existing users: $($existingUsers.Count)" -ForegroundColor White
Write-Host "   New users: $($newUsers.Count)" -ForegroundColor White
Write-Host "   Total processed: $($results.Count)" -ForegroundColor White

if ($newUsers.Count -gt 0) {
    Write-Host ""
    Write-Host "🎯 The script successfully handles both existing and new users!" -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "✅ All users already exist - script handles re-runs gracefully!" -ForegroundColor Green
}
