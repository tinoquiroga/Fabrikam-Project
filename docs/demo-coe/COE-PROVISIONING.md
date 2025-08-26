# 🛠️ COE User Provisioning Guide

This guide explains how to provision COE demo users in the `fabrikam.cspsecurityaccelerate.com` M365 tenant and Azure subscription.

## 📋 Overview

The provisioning process:
- ✅ Creates users in M365 tenant with matching Microsoft usernames
- ✅ Assigns Global Admin role for Copilot & Copilot Studio access
- ✅ Creates individual Azure resource groups for each user
- ✅ Assigns Contributor access to the Azure subscription
- ✅ Handles existing users gracefully (updates roles if needed)
- ✅ Tags resource groups with user ownership information

## 📁 Files

| File | Purpose |
|------|---------|
| `coe-users.csv` | **Template** for user data (safe for source control) |
| `coe-users-actual.csv` | **Actual user data** (gitignored, not in source control) |
| `Provision-COE-Users.ps1` | PowerShell provisioning script |
| `COE-PROVISIONING.md` | This documentation file |

## 🔐 Security Note

**Personal data is protected from source control:**
- ✅ `coe-users.csv` contains only template/sample data
- ✅ `coe-users-actual.csv` contains real user data (gitignored)
- ✅ Real user data never committed to public repository
- ✅ Template provides format for creating actual data file

## 🚀 Quick Start

### Prerequisites

1. **PowerShell Modules** (install if needed):
   ```powershell
   Install-Module Microsoft.Graph -Scope CurrentUser
   Install-Module Az -Scope CurrentUser
   ```

2. **Permissions Required**:
   - Global Administrator in target M365 tenant
   - Owner or User Access Administrator in Azure subscription

3. **Create User Data File**:
   ```powershell
   # Copy template to actual user data file
   Copy-Item "coe-users.csv" "coe-users-actual.csv"
   
   # Edit coe-users-actual.csv with real user information
   # This file is gitignored and won't be committed to source control
   notepad "coe-users-actual.csv"
   ```

### Run the Provisioning Script

1. **Test run first** (recommended):
   ```powershell
   .\Provision-COE-Users.ps1 -UserDataFile "coe-users-actual.csv" `
     -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
     -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878" `
     -WhatIf
   ```

2. **Execute the provisioning**:
   ```powershell
   .\Provision-COE-Users.ps1 -UserDataFile "coe-users-actual.csv" `
     -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
     -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878"
   ```

## 👥 COE Demo Users

| Display Name | Username | Email | Lab Account |
|--------------|----------|-------|-------------|
| Chris DePalma | chridep | chridep@microsoft.com | chridep@fabrikam.cspsecurityaccelerate.com |
| Francois van Hemert | franvanh | franvanh@microsoft.com | franvanh@fabrikam.cspsecurityaccelerate.com |
| Florentino Quiroga | fquiroga | fquiroga@microsoft.com | fquiroga@fabrikam.cspsecurityaccelerate.com |
| Mike Palitto | mikepalitto | mikepalitto@microsoft.com | mikepalitto@fabrikam.cspsecurityaccelerate.com |
| Mariusz Ostrowski | mariuszo | mariuszo@microsoft.com | mariuszo@fabrikam.cspsecurityaccelerate.com |
| Stefan Schulz | stschulz | stschulz@microsoft.com | stschulz@fabrikam.cspsecurityaccelerate.com |
| Cedric Depaepe | cdepaepe | cdepaepe@microsoft.com | cdepaepe@fabrikam.cspsecurityaccelerate.com |
| Jim Banach | jimbanach | jimbanach@microsoft.com | jimbanach@fabrikam.cspsecurityaccelerate.com |
| David Bjurman-Birr | davidb | davidb@microsoft.com | davidb@fabrikam.cspsecurityaccelerate.com |

## 🔧 Script Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `UserDataFile` | Yes | - | Path to CSV file with user data |
| `TenantDomain` | Yes | - | M365 tenant domain |
| `AzureSubscriptionId` | Yes | - | Azure subscription ID |
| `DefaultPassword` | No | `TempPassword123!` | Initial password for new users |
| `AzureLocation` | No | `East US 2` | Azure region for resource groups |
| `WhatIf` | No | - | Test mode - shows what would be done |

## 📊 What the Script Does

### M365 Tenant Actions

1. **Check for existing users** in the target tenant
2. **Create new users** with:
   - Display name from CSV
   - Username@fabrikam.cspsecurityaccelerate.com
   - Default password (must change on first login)
   - Enabled account with US usage location
3. **Assign Global Admin role** for full M365/Copilot access

### Azure Subscription Actions

1. **Create resource groups** with pattern: `rg-fabrikam-coe-{username}-{random}`
2. **Apply tags** to resource groups:
   - `createdFor`: Username of intended user
   - `purpose`: COE-Demo
   - `created`: Current date
   - `project`: Fabrikam-COE-Demo
3. **Assign Contributor role** at subscription level

## 🎯 Resource Group Naming

Each user gets a unique resource group:
```
rg-fabrikam-coe-{username}-{6-char-random}
```

Examples:
- `rg-fabrikam-coe-chridep-xbm4kp`
- `rg-fabrikam-coe-franvanh-qs8cny`
- `rg-fabrikam-coe-davidb-lm9pqr`

## 🔐 Security & Access

### M365 Permissions
- ✅ **Global Admin**: Full access to M365 admin center
- ✅ **Copilot Studio**: Can create and manage Copilot Studio bots
- ✅ **Microsoft Copilot**: Full AI assistant capabilities

### Azure Permissions
- ✅ **Subscription Contributor**: Can create/manage all resources
- ✅ **Resource Group Owner**: Full control of their assigned resource group
- ✅ **Billing Access**: Can view costs and usage

## 🚨 Important Notes

### Security Considerations
- All users get **Global Admin** rights (required for Copilot Studio)
- Users can access **entire Azure subscription** (Contributor role)
- Default password is **temporary** - users must change on first login
- Consider time-limited access for demo purposes

### Resource Management
- Each user has a **dedicated resource group**
- Resource groups are **tagged with ownership**
- Multiple runs create **new resource groups** (doesn't reuse existing ones)
- Clean up resource groups after demo if needed

### Script Behavior
- **Gracefully handles existing users** - updates roles if needed
- **Creates new resource groups each run** - doesn't reuse existing ones
- **Continues processing** if individual operations fail
- **Provides detailed logging** and summary report

## 🧪 Testing the Script

### Dry Run (Recommended First)
```powershell
# Test what the script would do without making changes
.\Provision-COE-Users.ps1 -UserDataFile "coe-users.csv" `
  -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
  -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878" `
  -WhatIf
```

### Single User Test
Create a test CSV with one user to verify the process:
```csv
Test User,testuser,testuser@microsoft.com
```

## 🆘 Troubleshooting

### Common Issues

**"Access Denied" errors**:
- Verify you have Global Admin rights in M365 tenant
- Verify you have Owner/User Access Administrator in Azure subscription
- Check if MFA is required and properly authenticated

**User creation fails**:
- Check if username already exists in tenant
- Verify domain name is correct
- Check license availability in tenant

**Azure resource group creation fails**:
- Verify subscription ID is correct
- Check if you're connected to the right Azure tenant
- Verify region availability

**Role assignment fails**:
- Wait a few minutes for user creation to propagate
- Check if roles are already assigned
- Verify required permissions

### Getting Help

1. **Run with -WhatIf first** to identify issues
2. **Check error messages** in the console output
3. **Verify prerequisites** (modules, permissions)
4. **Review Azure Portal** for created resources
5. **Check M365 Admin Center** for user status

## 📈 Post-Provisioning Steps

### For COE Demo Participants

1. **Log in to Azure Portal**: https://portal.azure.com
   - Use: `{username}@fabrikam.cspsecurityaccelerate.com`
   - Password: `TempPassword123!` (will be prompted to change)

2. **Verify access**:
   - Can see the Azure subscription
   - Can see their dedicated resource group
   - Can create resources in their resource group

3. **Follow COE Demo Guide**:
   - Use the [COE-COMPLETE-SETUP-GUIDE.md](./COE-COMPLETE-SETUP-GUIDE.md)
   - Deploy Fabrikam project to their resource group
   - Set up CI/CD pipeline

### For Demo Administrators

1. **Verify all users created successfully**
2. **Check resource group tagging** for proper attribution
3. **Monitor Azure costs** during demo period
4. **Plan cleanup** after demo completion
5. **Document any issues** for future improvements

## 🧹 Cleanup After Demo

### Remove Users (Optional)
```powershell
# Script to remove demo users (create as needed)
# Remove users from M365 tenant
# Remove Azure role assignments
# Delete resource groups
```

### Cost Management
- Monitor Azure spending during demo
- Set up alerts for unexpected usage
- Delete resource groups after demo completion
- Review and remove unused resources

---

**🎉 This provisioning setup ensures every COE participant has a complete, isolated environment for the Fabrikam demo with full M365 and Azure access!**
