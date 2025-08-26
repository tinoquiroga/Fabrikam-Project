# 🚀 COE User Provisioning Script for Fabrikam Demo
# This script provisions users in M365 tenant and creates Azure resource groups
# Author: Dave Birr
# Date: August 26, 2025

param(
    [Parameter(Mandatory = $false)]
    [string]$UserDataFile = "coe-users.csv",
    
    [Parameter(Mandatory = $true)]
    [string]$TenantDomain = "fabrikam.cspsecurityaccelerate.com",
    
    [Parameter(Mandatory = $true)]
    [string]$AzureSubscriptionId = "b7699934-0c99-4899-8799-763fffc90878",
    
    [Parameter(Mandatory = $false)]
    [string]$DefaultPassword = "TempPassword123!",
    
    [Parameter(Mandatory = $false)]
    [string]$AzureLocation = "East US 2",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

# Script configuration
$ErrorActionPreference = "Continue"
$InformationPreference = "Continue"

Write-Host "🚀 COE User Provisioning Script Starting..." -ForegroundColor Cyan
Write-Host "Target Tenant: $TenantDomain" -ForegroundColor Yellow
Write-Host "Azure Subscription: $AzureSubscriptionId" -ForegroundColor Yellow
Write-Host "User Data File: $UserDataFile" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "⚠️  WHAT-IF MODE: No changes will be made" -ForegroundColor Magenta
}

# Function to generate unique suffix for resource groups
function Get-UniqueResourceGroupSuffix {
    return -join ((97..122) | Get-Random -Count 6 | ForEach-Object {[char]$_})
}

# Function to check if user exists in Azure AD
function Test-AzureADUserExists {
    param([string]$UserPrincipalName)
    
    try {
        $user = Get-MgUser -UserId $UserPrincipalName -ErrorAction SilentlyContinue
        return $null -ne $user
    }
    catch {
        return $false
    }
}

# Function to create or update Azure AD user
function New-OrUpdate-AzureADUser {
    param(
        [string]$DisplayName,
        [string]$Username,
        [string]$TenantDomain,
        [string]$Password,
        [bool]$WhatIfMode
    )
    
    $userPrincipalName = "$Username@$TenantDomain"
    $mailNickname = $Username
    
    Write-Host "  📧 Processing user: $DisplayName ($userPrincipalName)" -ForegroundColor White
    
    if (Test-AzureADUserExists -UserPrincipalName $userPrincipalName) {
        Write-Host "    ✅ User already exists: $userPrincipalName" -ForegroundColor Green
        
        if (-not $WhatIfMode) {
            # Get existing user for role assignment
            $user = Get-MgUser -UserId $userPrincipalName
            return $user
        }
        else {
            Write-Host "    [WHAT-IF] Would update existing user: $userPrincipalName" -ForegroundColor Magenta
            return @{ Id = "whatif-user-id"; UserPrincipalName = $userPrincipalName }
        }
    }
    else {
        Write-Host "    🆕 Creating new user: $userPrincipalName" -ForegroundColor Yellow
        
        if (-not $WhatIfMode) {
            try {
                $passwordProfile = @{
                    Password = $Password
                    ForceChangePasswordNextSignIn = $true
                }
                
                $newUser = New-MgUser -DisplayName $DisplayName `
                    -UserPrincipalName $userPrincipalName `
                    -MailNickname $mailNickname `
                    -PasswordProfile $passwordProfile `
                    -AccountEnabled:$true `
                    -UsageLocation "US"
                
                Write-Host "    ✅ Created user: $userPrincipalName" -ForegroundColor Green
                return $newUser
            }
            catch {
                Write-Host "    ❌ Failed to create user: $($_.Exception.Message)" -ForegroundColor Red
                return $null
            }
        }
        else {
            Write-Host "    [WHAT-IF] Would create new user: $userPrincipalName" -ForegroundColor Magenta
            return @{ Id = "whatif-user-id"; UserPrincipalName = $userPrincipalName }
        }
    }
}

# Function to assign Global Admin role
function Set-GlobalAdminRole {
    param(
        [object]$User,
        [bool]$WhatIfMode
    )
    
    Write-Host "  🔑 Assigning Global Admin role to: $($User.UserPrincipalName)" -ForegroundColor White
    
    if (-not $WhatIfMode) {
        try {
            # Get Global Administrator role template
            $globalAdminRole = Get-MgDirectoryRoleTemplate | Where-Object { $_.DisplayName -eq "Global Administrator" }
            
            # Check if role is activated
            $activeRole = Get-MgDirectoryRole | Where-Object { $_.RoleTemplateId -eq $globalAdminRole.Id }
            if (-not $activeRole) {
                # Activate the role
                $activeRole = New-MgDirectoryRole -RoleTemplateId $globalAdminRole.Id
            }
            
            # Check if user already has the role
            $existingAssignment = Get-MgDirectoryRoleMember -DirectoryRoleId $activeRole.Id | Where-Object { $_.Id -eq $User.Id }
            
            if (-not $existingAssignment) {
                # Assign the role
                New-MgDirectoryRoleMemberByRef -DirectoryRoleId $activeRole.Id -BodyParameter @{ "@odata.id" = "https://graph.microsoft.com/v1.0/users/$($User.Id)" }
                Write-Host "    ✅ Global Admin role assigned" -ForegroundColor Green
            }
            else {
                Write-Host "    ✅ User already has Global Admin role" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "    ❌ Failed to assign Global Admin role: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "    [WHAT-IF] Would assign Global Admin role" -ForegroundColor Magenta
    }
}

# Function to create Azure resource group
function New-AzureResourceGroup {
    param(
        [string]$Username,
        [string]$Location,
        [string]$SubscriptionId,
        [bool]$WhatIfMode
    )
    
    $suffix = Get-UniqueResourceGroupSuffix
    $resourceGroupName = "rg-fabrikam-coe-$Username-$suffix"
    
    Write-Host "  ☁️  Creating Azure resource group: $resourceGroupName" -ForegroundColor White
    
    if (-not $WhatIfMode) {
        try {
            # Set Azure context to the correct subscription
            Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
            
            # Create resource group with tags
            $tags = @{
                "createdFor" = $Username
                "purpose" = "COE-Demo"
                "created" = (Get-Date).ToString("yyyy-MM-dd")
                "project" = "Fabrikam-COE-Demo"
            }
            
            $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location $Location -Tag $tags
            Write-Host "    ✅ Resource group created: $resourceGroupName" -ForegroundColor Green
            return $resourceGroup
        }
        catch {
            Write-Host "    ❌ Failed to create resource group: $($_.Exception.Message)" -ForegroundColor Red
            return $null
        }
    }
    else {
        Write-Host "    [WHAT-IF] Would create resource group: $resourceGroupName" -ForegroundColor Magenta
        return @{ ResourceGroupName = $resourceGroupName }
    }
}

# Function to assign Azure subscription contributor role
function Set-AzureContributorRole {
    param(
        [string]$UserPrincipalName,
        [string]$SubscriptionId,
        [bool]$WhatIfMode
    )
    
    Write-Host "  🔐 Assigning Contributor role for subscription: $SubscriptionId" -ForegroundColor White
    
    if (-not $WhatIfMode) {
        try {
            # Set Azure context
            Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
            
            # Check if user already has Contributor role at subscription level
            $existingAssignment = Get-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$SubscriptionId" -ErrorAction SilentlyContinue
            
            if (-not $existingAssignment) {
                # Assign Contributor role at subscription level
                New-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$SubscriptionId"
                Write-Host "    ✅ Contributor role assigned for subscription" -ForegroundColor Green
            }
            else {
                Write-Host "    ✅ User already has Contributor role for subscription" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "    ❌ Failed to assign Contributor role: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "    [WHAT-IF] Would assign Contributor role for subscription" -ForegroundColor Magenta
    }
}

# Main execution
try {
    # Check if user data file exists
    if (-not (Test-Path $UserDataFile)) {
        Write-Host "❌ User data file not found: $UserDataFile" -ForegroundColor Red
        Write-Host ""
        
        if (Test-Path "coe-users-template.csv") {
            Write-Host "📋 Template file found. To get started:" -ForegroundColor Yellow
            Write-Host "1. Copy 'coe-users-template.csv' to '$UserDataFile'" -ForegroundColor White
            Write-Host "2. Edit '$UserDataFile' with your actual user data" -ForegroundColor White
            Write-Host "3. Run this script again" -ForegroundColor White
            Write-Host ""
            Write-Host "Quick command to copy template:" -ForegroundColor Cyan
            Write-Host "Copy-Item 'coe-users-template.csv' '$UserDataFile'" -ForegroundColor Gray
        }
        else {
            Write-Host "📋 Create a CSV file named '$UserDataFile' with the format:" -ForegroundColor Yellow
            Write-Host "DisplayName,Username,OriginalEmail" -ForegroundColor White
            Write-Host "John Doe,johndoe,johndoe@microsoft.com" -ForegroundColor Gray
        }
        
        throw "User data file not found: $UserDataFile"
    }
    
    # Read user data from CSV
    Write-Host "📂 Reading user data from: $UserDataFile" -ForegroundColor Cyan
    $users = Import-Csv $UserDataFile -Header "DisplayName", "Username", "OriginalEmail" | Where-Object { $_.DisplayName -notlike "#*" }
    
    Write-Host "👥 Found $($users.Count) users to process" -ForegroundColor Cyan
    
    # Connect to Microsoft Graph (if not in WhatIf mode)
    if (-not $WhatIf) {
        Write-Host "🔌 Connecting to Microsoft Graph..." -ForegroundColor Cyan
        try {
            Connect-MgGraph -Scopes "User.ReadWrite.All", "Directory.ReadWrite.All", "RoleManagement.ReadWrite.Directory" -NoWelcome
            Write-Host "✅ Connected to Microsoft Graph" -ForegroundColor Green
        }
        catch {
            throw "Failed to connect to Microsoft Graph: $($_.Exception.Message)"
        }
        
        # Connect to Azure
        Write-Host "🔌 Connecting to Azure..." -ForegroundColor Cyan
        try {
            Connect-AzAccount -SubscriptionId $AzureSubscriptionId | Out-Null
            Write-Host "✅ Connected to Azure subscription: $AzureSubscriptionId" -ForegroundColor Green
        }
        catch {
            throw "Failed to connect to Azure: $($_.Exception.Message)"
        }
    }
    
    # Process each user
    $results = @()
    foreach ($userData in $users) {
        Write-Host "🔄 Processing: $($userData.DisplayName)" -ForegroundColor Cyan
        
        # Create or update Azure AD user
        $user = New-OrUpdate-AzureADUser -DisplayName $userData.DisplayName `
            -Username $userData.Username `
            -TenantDomain $TenantDomain `
            -Password $DefaultPassword `
            -WhatIfMode $WhatIf
        
        if ($user) {
            # Assign Global Admin role
            Set-GlobalAdminRole -User $user -WhatIfMode $WhatIf
            
            # Create Azure resource group
            $resourceGroup = New-AzureResourceGroup -Username $userData.Username `
                -Location $AzureLocation `
                -SubscriptionId $AzureSubscriptionId `
                -WhatIfMode $WhatIf
            
            # Assign Azure Contributor role
            Set-AzureContributorRole -UserPrincipalName $user.UserPrincipalName `
                -SubscriptionId $AzureSubscriptionId `
                -WhatIfMode $WhatIf
            
            # Store result
            $results += [PSCustomObject]@{
                DisplayName = $userData.DisplayName
                Username = $userData.Username
                UserPrincipalName = $user.UserPrincipalName
                ResourceGroupName = if ($resourceGroup) { $resourceGroup.ResourceGroupName } else { "Failed" }
                Status = "Processed"
            }
        }
        else {
            $results += [PSCustomObject]@{
                DisplayName = $userData.DisplayName
                Username = $userData.Username
                UserPrincipalName = "Failed"
                ResourceGroupName = "Failed"
                Status = "Failed"
            }
        }
        
        Write-Host "✅ Completed processing: $($userData.DisplayName)" -ForegroundColor Green
        Write-Host "" # Blank line for readability
    }
    
    # Summary
    Write-Host "📊 PROVISIONING SUMMARY" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    $results | Format-Table -AutoSize
    
    $successCount = ($results | Where-Object { $_.Status -eq "Processed" }).Count
    $failCount = ($results | Where-Object { $_.Status -eq "Failed" }).Count
    
    Write-Host "✅ Successfully processed: $successCount users" -ForegroundColor Green
    if ($failCount -gt 0) {
        Write-Host "❌ Failed: $failCount users" -ForegroundColor Red
    }
    
    # Additional instructions
    Write-Host ""
    Write-Host "📝 NEXT STEPS FOR COE DEMO:" -ForegroundColor Yellow
    Write-Host "1. Users can log in to https://portal.azure.com with their new credentials" -ForegroundColor White
    Write-Host "2. Default password: $DefaultPassword (users will be prompted to change)" -ForegroundColor White
    Write-Host "3. Each user has their own resource group for the Fabrikam demo" -ForegroundColor White
    Write-Host "4. Users have Contributor access to the entire subscription" -ForegroundColor White
    Write-Host "5. All users have Global Admin rights for M365/Copilot Studio access" -ForegroundColor White
    
    if ($WhatIf) {
        Write-Host ""
        Write-Host "⚠️  This was a WHAT-IF run. To execute changes, run without -WhatIf parameter." -ForegroundColor Magenta
    }
}
catch {
    Write-Host "❌ Script execution failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    # Disconnect from services (if connected)
    if (-not $WhatIf) {
        try {
            Disconnect-MgGraph -ErrorAction SilentlyContinue
            Disconnect-AzAccount -ErrorAction SilentlyContinue
        }
        catch {
            # Ignore disconnection errors
        }
    }
}

Write-Host "🎉 COE User Provisioning Script Completed!" -ForegroundColor Cyan
