# 🚀 COE User Provisioning Script for Fabrikam Demo
# This script provisions users in M365 tenant and creates Azure resource groups
# Author: Dave Birr
# Date: August 26, 2025

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigFile = "coe-config.json",
    
    [Parameter(Mandatory = $false)]
    [string]$TenantDomain,
    
    [Parameter(Mandatory = $false)]
    [string]$TenantId,
    
    [Parameter(Mandatory = $false)]
    [string]$AzureSubscriptionId,
    
    [Parameter(Mandatory = $false)]
    [string]$DefaultPassword,
    
    [Parameter(Mandatory = $false)]
    [string]$AzureLocation,
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

# Script configuration
$ErrorActionPreference = "Continue"
$InformationPreference = "Continue"

Write-Host "🚀 COE User Provisioning Script Starting..." -ForegroundColor Cyan

# Load configuration from file
if (Test-Path $ConfigFile) {
    Write-Host "📋 Loading configuration from: $ConfigFile" -ForegroundColor Cyan
    try {
        $config = Get-Content $ConfigFile | ConvertFrom-Json
        
        # Use config values as defaults, allow parameter overrides
        if (-not $TenantDomain) { $TenantDomain = $config.tenantDomain }
        if (-not $TenantId) { $TenantId = $config.tenantId }
        if (-not $AzureSubscriptionId) { $AzureSubscriptionId = $config.azureSubscriptionId }
        if (-not $DefaultPassword) { $DefaultPassword = $config.defaultPassword }
        if (-not $AzureLocation) { $AzureLocation = $config.azureLocation }
        
        # Get users from config
        $users = $config.users
        
        if (-not $users -or $users.Count -eq 0) {
            throw "No users found in configuration file"
        }
        
        Write-Host "✅ Configuration loaded successfully" -ForegroundColor Green
        Write-Host "📊 Found $($users.Count) users in configuration" -ForegroundColor Green
    }
    catch {
        throw "Failed to load configuration from $ConfigFile`: $($_.Exception.Message)"
    }
}
else {
    Write-Host "⚠️  Configuration file not found: $ConfigFile" -ForegroundColor Yellow
    Write-Host "📝 Please create config file from template:" -ForegroundColor Yellow
    Write-Host "   Copy-Item 'coe-config-template.json' '$ConfigFile'" -ForegroundColor White
    Write-Host "   Then edit $ConfigFile with your tenant details and user data" -ForegroundColor White
    
    # Fallback to old parameter validation for backward compatibility
    if (-not $TenantDomain) {
        throw "TenantDomain is required when no config file is present"
    }
    if (-not $AzureSubscriptionId) {
        throw "AzureSubscriptionId is required when no config file is present"
    }
    
    # Set defaults for missing optional parameters
    if (-not $DefaultPassword) { $DefaultPassword = "TempPassword123!" }
    if (-not $AzureLocation) { $AzureLocation = "East US 2" }
    
    # No users available without config file
    throw "Configuration file is required to provide user data"
}

Write-Host "Target Tenant: $TenantDomain" -ForegroundColor Yellow
Write-Host "Tenant ID: $TenantId" -ForegroundColor Yellow
Write-Host "Azure Subscription: $AzureSubscriptionId" -ForegroundColor Yellow
Write-Host "User Count: $($users.Count)" -ForegroundColor Yellow

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
    
    $resourceGroupName = "rg-fabrikam-coe-$Username"
    
    Write-Host "  ☁️  Creating Azure resource group: $resourceGroupName" -ForegroundColor White
    
    if (-not $WhatIfMode) {
        try {
            # Set Azure context to the correct subscription
            Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
            
            # Check if resource group already exists
            $existingRG = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
            
            if ($existingRG) {
                Write-Host "    ✅ Resource group already exists: $resourceGroupName" -ForegroundColor Green
                return @{ ResourceGroupName = $resourceGroupName }
            }
            else {
                # Create resource group with tags
                $tags = @{
                    "createdFor" = $Username
                    "purpose" = "COE-Demo"
                    "created" = (Get-Date).ToString("yyyy-MM-dd")
                    "project" = "Fabrikam-COE-Demo"
                }
                
                $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location $Location -Tag $tags
                Write-Host "    ✅ Resource group created: $resourceGroupName" -ForegroundColor Green
                return @{ ResourceGroupName = $resourceGroupName }
            }
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

# Function to assign appropriate Azure roles for COE users
function Set-AzureRoles {
    param(
        [string]$UserPrincipalName,
        [string]$SubscriptionId,
        [string]$ResourceGroupName,
        [bool]$WhatIfMode
    )
    
    Write-Host "  🔐 Setting up Azure roles for COE environment" -ForegroundColor White
    
    if (-not $WhatIfMode) {
        try {
            # Set Azure context
            Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
            
            # 1. Assign Reader role at subscription level (can see all resources but not modify)
            Write-Host "    📖 Setting up subscription-level Reader access..." -ForegroundColor Gray
            $existingReader = Get-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$SubscriptionId" -ErrorAction SilentlyContinue
            
            if (-not $existingReader) {
                New-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Reader" -Scope "/subscriptions/$SubscriptionId" -ErrorAction Stop | Out-Null
                Write-Host "    ✅ Reader role assigned at subscription level" -ForegroundColor Green
            }
            else {
                Write-Host "    ✅ User already has Reader role at subscription level" -ForegroundColor Green
            }
            
            # 2. Remove any existing subscription-level Contributor role (if it exists)
            $existingContributor = Get-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$SubscriptionId" -ErrorAction SilentlyContinue
            if ($existingContributor) {
                Write-Host "    🗑️  Removing subscription-level Contributor role..." -ForegroundColor Yellow
                Remove-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -Scope "/subscriptions/$SubscriptionId" -ErrorAction SilentlyContinue
                Write-Host "    ✅ Removed subscription-level Contributor role" -ForegroundColor Green
            }
            
            # 3. Assign Contributor role only to their specific resource group
            Write-Host "    🔐 Setting up resource group-level Contributor access..." -ForegroundColor Gray
            $existingRGContributor = Get-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
            
            if (-not $existingRGContributor) {
                New-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "Contributor" -ResourceGroupName $ResourceGroupName -ErrorAction Stop | Out-Null
                Write-Host "    ✅ Contributor role assigned to resource group: $ResourceGroupName" -ForegroundColor Green
            }
            else {
                Write-Host "    ✅ User already has Contributor role for resource group: $ResourceGroupName" -ForegroundColor Green
            }
            
            # 4. Assign User Access Administrator role to their specific resource group (needed for Key Vault role assignments)
            Write-Host "    🔑 Setting up resource group-level User Access Administrator access..." -ForegroundColor Gray
            $existingRGUserAccess = Get-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "User Access Administrator" -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
            
            if (-not $existingRGUserAccess) {
                New-AzRoleAssignment -SignInName $UserPrincipalName -RoleDefinitionName "User Access Administrator" -ResourceGroupName $ResourceGroupName -ErrorAction Stop | Out-Null
                Write-Host "    ✅ User Access Administrator role assigned to resource group: $ResourceGroupName" -ForegroundColor Green
            }
            else {
                Write-Host "    ✅ User already has User Access Administrator role for resource group: $ResourceGroupName" -ForegroundColor Green
            }
            
            Write-Host "    💡 User can now:" -ForegroundColor Cyan
            Write-Host "      📖 View all subscription resources (Reader)" -ForegroundColor White
            Write-Host "      🔐 Create/modify resources only in: $ResourceGroupName (Contributor)" -ForegroundColor White
            Write-Host "      🔑 Assign roles to resources in: $ResourceGroupName (User Access Administrator)" -ForegroundColor White
        }
        catch {
            Write-Host "    ❌ Failed to assign roles: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "    [WHAT-IF] Would assign Reader role at subscription level" -ForegroundColor Magenta
        Write-Host "    [WHAT-IF] Would assign Contributor role to resource group: $ResourceGroupName" -ForegroundColor Magenta
        Write-Host "    [WHAT-IF] Would assign User Access Administrator role to resource group: $ResourceGroupName" -ForegroundColor Magenta
    }
}

# Main execution
try {
    # Users are already loaded from JSON config at the top of the script
    # Filter out any commented users (those starting with #)
    $activeUsers = $users | Where-Object { $_.displayName -notlike "#*" }
    
    Write-Host "👥 Processing $($activeUsers.Count) active users from configuration" -ForegroundColor Cyan
    
    # Smart authentication - only clear if we need to change tenant/subscription
    Write-Host "🔍 Checking existing authentication sessions..." -ForegroundColor Cyan
    
    $mgContext = Get-MgContext -ErrorAction SilentlyContinue
    $azContext = Get-AzContext -ErrorAction SilentlyContinue
    
    $needsMgAuth = $false
    $needsAzAuth = $false
    
    # Check Microsoft Graph connection
    if ($mgContext) {
        if ($TenantId -and $mgContext.TenantId -ne $TenantId) {
            Write-Host "   🔄 Microsoft Graph connected to wrong tenant, will re-authenticate" -ForegroundColor Yellow
            $needsMgAuth = $true
            try {
                Disconnect-MgGraph -ErrorAction SilentlyContinue | Out-Null
            } catch { }
        }
        else {
            Write-Host "   ✅ Microsoft Graph already connected to correct tenant" -ForegroundColor Green
        }
    }
    else {
        Write-Host "   🔄 No Microsoft Graph session found" -ForegroundColor Yellow
        $needsMgAuth = $true
    }
    
    # Check Azure connection
    if ($azContext) {
        if ($AzureSubscriptionId -and $azContext.Subscription.Id -ne $AzureSubscriptionId) {
            Write-Host "   🔄 Azure connected to wrong subscription, will re-authenticate" -ForegroundColor Yellow
            $needsAzAuth = $true
            try {
                Disconnect-AzAccount -ErrorAction SilentlyContinue | Out-Null
                Clear-AzContext -Force -ErrorAction SilentlyContinue | Out-Null
            } catch { }
        }
        else {
            Write-Host "   ✅ Azure already connected to correct subscription" -ForegroundColor Green
        }
    }
    else {
        Write-Host "   🔄 No Azure session found" -ForegroundColor Yellow
        $needsAzAuth = $true
    }
    
    # Connect to Microsoft Graph (only if needed)
    if ($needsMgAuth) {
        Write-Host "🔌 Connecting to Microsoft Graph..." -ForegroundColor Cyan
        try {
            if ($TenantId) {
                Write-Host "   Using tenant-specific authentication: $TenantId" -ForegroundColor Gray
                Write-Host "   When browser opens, authenticate with an account from: $($config.tenantDomain)" -ForegroundColor Yellow
                
                # Force interactive authentication with specific tenant
                Connect-MgGraph -TenantId $TenantId `
                    -Scopes "User.ReadWrite.All", "Directory.ReadWrite.All", "RoleManagement.ReadWrite.Directory" `
                    -UseDeviceAuthentication:$false `
                    -NoWelcome
            }
            else {
                Write-Host "   Warning: No tenant ID specified, may prompt for account selection" -ForegroundColor Yellow
                Connect-MgGraph -Scopes "User.ReadWrite.All", "Directory.ReadWrite.All", "RoleManagement.ReadWrite.Directory" -NoWelcome
            }
            
            # Verify we're connected to the correct tenant
            $context = Get-MgContext
            if ($context -and $TenantId -and $context.TenantId -ne $TenantId) {
                throw "Connected to wrong tenant. Expected: $TenantId, Got: $($context.TenantId)"
            }
            
            Write-Host "   ✅ Microsoft Graph connected successfully!" -ForegroundColor Green
            Write-Host "   Connected Tenant: $($context.TenantId)" -ForegroundColor White
            Write-Host "   Account: $($context.Account)" -ForegroundColor White
            if ($TenantId -and $context.TenantId -eq $TenantId) {
                Write-Host "   ✅ Correct tenant confirmed!" -ForegroundColor Green
            }
        }
        catch {
            throw "Failed to connect to Microsoft Graph: $($_.Exception.Message)"
        }
    }
    else {
        Write-Host "🔌 Microsoft Graph - using existing session" -ForegroundColor Green
        $context = Get-MgContext
        Write-Host "   Connected Tenant: $($context.TenantId)" -ForegroundColor White
        Write-Host "   Account: $($context.Account)" -ForegroundColor White
    }
    
    # Connect to Azure (only if needed)
    if ($needsAzAuth) {
        Write-Host "🔌 Connecting to Azure..." -ForegroundColor Cyan
    try {
        if ($TenantId) {
            Write-Host "   Using tenant-specific authentication: $($TenantId.Substring(0,30))..." -ForegroundColor Gray
            Write-Host "   Target domain: $($config.tenantDomain)" -ForegroundColor Yellow
            Write-Host "   ⚠️  IMPORTANT: Use account ending with @$($config.tenantDomain)" -ForegroundColor Magenta
            
            # Clear any existing Azure context to force fresh authentication
            if (Get-AzContext) {
                Write-Host "   Clearing existing Azure sessions..." -ForegroundColor Gray
                Clear-AzContext -Force
            }
            
            # Use device authentication to avoid account selection issues
            Connect-AzAccount -TenantId $TenantId `
                -SubscriptionId $AzureSubscriptionId `
                -UseDeviceAuthentication | Out-Null
        }
        else {
            Connect-AzAccount -SubscriptionId $AzureSubscriptionId | Out-Null
        }
        
        # Verify correct subscription
        $azContext = Get-AzContext
        if ($azContext.Subscription.Id -ne $AzureSubscriptionId) {
            $actualAccount = $azContext.Account.Id
            $expectedDomain = $config.tenantDomain
            throw "Connected to wrong subscription. Expected: $AzureSubscriptionId, Got: $($azContext.Subscription.Id). Account used: $actualAccount (should end with @$expectedDomain)"
        }
        
        Write-Host "   ✅ Azure connected successfully!" -ForegroundColor Green
        Write-Host "   Connected Tenant: $($azContext.Tenant.Id)" -ForegroundColor White
        Write-Host "   Account: $($azContext.Account.Id)" -ForegroundColor White
        Write-Host "   Subscription: $($azContext.Subscription.Id)" -ForegroundColor White
        if ($azContext.Subscription.Id -eq $AzureSubscriptionId) {
            Write-Host "   ✅ Correct subscription confirmed!" -ForegroundColor Green
        }
        
        # Authentication summary
        Write-Host ""
        Write-Host "🎯 Authentication Summary:" -ForegroundColor Cyan
        Write-Host "   📱 Microsoft Graph: Connected to $($config.tenantDomain)" -ForegroundColor Green
        Write-Host "   ☁️  Azure: Connected to subscription $AzureSubscriptionId" -ForegroundColor Green
        Write-Host "   👤 Account: $($azContext.Account.Id)" -ForegroundColor Green
        Write-Host ""
        
        # Verify permissions in WhatIf mode
        if ($WhatIf) {
            Write-Host "🔍 Verifying permissions..." -ForegroundColor Cyan
            
            # Test Microsoft Graph permissions
            try {
                Write-Host "   Testing Microsoft Graph permissions..." -ForegroundColor Gray
                # Try to read directory info (should work with Directory.ReadWrite.All)
                $testOrg = Get-MgOrganization -ErrorAction Stop | Select-Object -First 1
                Write-Host "   ✅ Microsoft Graph permissions verified" -ForegroundColor Green
                Write-Host "     Organization: $($testOrg.DisplayName)" -ForegroundColor White
            }
            catch {
                Write-Host "   ❌ Microsoft Graph permission test failed: $($_.Exception.Message)" -ForegroundColor Red
            }
            
            # Test Azure permissions
            try {
                Write-Host "   Testing Azure permissions..." -ForegroundColor Gray
                # Try to list resource groups (should work with Contributor)
                $testRgs = Get-AzResourceGroup -ErrorAction Stop | Select-Object -First 3
                Write-Host "   ✅ Azure permissions verified" -ForegroundColor Green
                Write-Host "     Can access $($testRgs.Count) resource groups (showing first 3)" -ForegroundColor White
            }
            catch {
                Write-Host "   ❌ Azure permission test failed: $($_.Exception.Message)" -ForegroundColor Red
            }
            
            Write-Host ""
        }
        
        # End of WhatIf permissions check
    }
    catch {
        throw "Failed to connect to Azure: $($_.Exception.Message)"
    }
    else {
        Write-Host "🔌 Azure - using existing session" -ForegroundColor Green
        $azContext = Get-AzContext
        Write-Host "   Connected Tenant: $($azContext.Tenant.Id)" -ForegroundColor White
        Write-Host "   Account: $($azContext.Account.Id)" -ForegroundColor White
        Write-Host "   Subscription: $($azContext.Subscription.Id)" -ForegroundColor White
    }
        throw "Failed to connect to Azure: $($_.Exception.Message)"
    }
    
    # Process each user
    $results = @()
    foreach ($userData in $activeUsers) {
        Write-Host "🔄 Processing: $($userData.displayName)" -ForegroundColor Cyan
        
        # Create or update Azure AD user
        $user = New-OrUpdate-AzureADUser -DisplayName $userData.displayName `
            -Username $userData.username `
            -TenantDomain $TenantDomain `
            -Password $DefaultPassword `
            -WhatIfMode $WhatIf
        
        if ($user) {
            # Assign Global Admin role
            Set-GlobalAdminRole -User $user -WhatIfMode $WhatIf
            
            # Create Azure resource group
            $resourceGroup = New-AzureResourceGroup -Username $userData.username `
                -Location $AzureLocation `
                -SubscriptionId $AzureSubscriptionId `
                -WhatIfMode $WhatIf
            
            # Assign appropriate Azure roles for COE environment
            if ($resourceGroup -and $resourceGroup.ResourceGroupName) {
                Set-AzureRoles -UserPrincipalName $user.UserPrincipalName `
                    -SubscriptionId $AzureSubscriptionId `
                    -ResourceGroupName $resourceGroup.ResourceGroupName `
                    -WhatIfMode $WhatIf
            }
            else {
                Write-Host "  ❌ Skipping role assignment due to resource group creation failure" -ForegroundColor Red
            }
            
            # Store result
            $results += [PSCustomObject]@{
                DisplayName = $userData.displayName
                Username = $userData.username
                UserPrincipalName = $user.UserPrincipalName
                ResourceGroupName = if ($resourceGroup) { $resourceGroup.ResourceGroupName } else { "Failed" }
                Status = "Processed"
            }
        }
        else {
            $results += [PSCustomObject]@{
                DisplayName = $userData.displayName
                Username = $userData.username
                UserPrincipalName = "Failed"
                ResourceGroupName = "Failed"
                Status = "Failed"
            }
        }
        
        Write-Host "✅ Completed processing: $($userData.displayName)" -ForegroundColor Green
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
