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

| File | Purpose | In Source Control |
|------|---------|------------------|
| `coe-config-template.json` | Configuration template with examples and user array structure | ✅ Yes |
| `coe-config.json` | Actual tenant/subscription config with real user data | ❌ No (gitignored) |
| `Provision-COE-Users.ps1` | PowerShell provisioning script | ✅ Yes |
| `Run-COE-Provisioning.ps1` | Quick execution helper script | ✅ Yes |
| `SECURITY-AND-PRIVACY.md` | Security guidelines and best practices | ✅ Yes |

## 🔐 Security & Privacy

**Personal and configuration data is fully protected from source control:**
- ✅ `coe-config-template.json` - Safe template with example structure in public repository
- ✅ `coe-config.json` - Your actual tenant/subscription details and user data (gitignored, stays local)
- ✅ Real participant and tenant information never exposed publicly
- ✅ Support for multiple workshops with different configurations
- ✅ Workshop organizers can safely keep actual data for events
- ✅ **Simplified single-file configuration** - no separate CSV files needed

**See [SECURITY-AND-PRIVACY.md](./SECURITY-AND-PRIVACY.md) for complete guidelines.**

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
   # Copy template to create your actual user data file
   Copy-Item "coe-users-template.csv" "coe-users.csv"
   
   # Edit with actual participant information
   notepad coe-users.csv
   ```
### Run the Provisioning Script

### Setup Instructions

1. **Create configuration from template**:
   ```powershell
   Copy-Item "coe-config-template.json" "coe-config.json"
   ```

2. **Edit your configuration**:
   ```powershell
   notepad coe-config.json
   ```
   - Add your actual tenant ID and domain
   - Add your Azure subscription ID
   - Add your user data in the users array

3. **Quick start** (uses helper script):
   ```powershell
   .\Run-COE-Provisioning.ps1
   ```

4. **Test run first** (recommended):
   ```powershell
   .\Provision-COE-Users.ps1 -ConfigFile "coe-config.json" -WhatIf
   ```

5. **Execute the provisioning**:
   ```powershell
   .\Provision-COE-Users.ps1 -ConfigFile "coe-config.json"
   ```

## 👥 COE Demo Users

**Personal data is protected in the configuration file (`coe-config.json`).**

The configuration file includes a users array with the format:
- **displayName**: Full name for Azure AD
- **username**: Short username (matches Microsoft username)  
- **originalEmail**: Reference email from Microsoft

Example format (from template):
```json
{
  "users": [
    {
      "displayName": "John Doe",
      "username": "johndoe",
      "originalEmail": "johndoe@microsoft.com"
    },
    {
      "displayName": "Jane Smith", 
      "username": "janesmith",
      "originalEmail": "janesmith@microsoft.com"
    }
  ]
}
```

**Actual participant details are in your local `coe-config.json` file.**

## 🔧 Script Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `ConfigFile` | No | `coe-config.json` | Path to JSON configuration file |
| `TenantDomain` | No | From config | M365 tenant domain (overrides config) |
| `TenantId` | No | From config | Entra ID tenant ID (overrides config) |
| `AzureSubscriptionId` | No | From config | Azure subscription ID (overrides config) |
| `DefaultPassword` | No | From config or `TempPassword123!` | Initial password for new users |
| `AzureLocation` | No | From config or `East US 2` | Azure region for resource groups |
| `WhatIf` | No | - | Test mode - shows what would be done |

**Note:** All configuration is now in the JSON file, with command line parameters serving as optional overrides.
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
- Each user gets a **dedicated resource group**: `rg-fabrikam-coe-{username}`
- Resource group names directly include the username for easy identification
- Resource groups are **tagged with ownership** using `createdFor` tag
- Multiple runs create **new resource groups** (doesn't reuse existing ones)
- Clean up resource groups after demo if needed

### Tags Applied to Resource Groups
- **createdFor**: Username of the COE participant
- **purpose**: COE-Demo
- **created**: Date of creation (yyyy-MM-dd format)
- **project**: Fabrikam-COE-Demo

### Script Behavior
- **Gracefully handles existing users** - updates roles if needed
- **Creates new resource groups each run** - doesn't reuse existing ones
- **Continues processing** if individual operations fail
- **Provides detailed logging** and summary report

## 🚀 Complete Workflow

### Step-by-Step Process

1. **Prepare user data**:
   ```powershell
   # Copy template and edit with actual participants
   Copy-Item "coe-users-template.csv" "coe-users.csv"
   notepad coe-users.csv
   ```

2. **Test the provisioning**:
   ```powershell
   # Run helper script for guided setup
   .\Run-COE-Provisioning.ps1
   # Choose option 1 for test mode
   ```

3. **Execute provisioning**:
   ```powershell
   # Run helper script again
   .\Run-COE-Provisioning.ps1  
   # Choose option 2 for actual provisioning
   ```

4. **Verify results**:
   - Check Azure Portal for resource groups
   - Verify users in M365 Admin Center
   - Test participant login credentials

### Multiple Workshop Support

For different workshops, create separate user data files:
```powershell
# Workshop 1 
Copy-Item "coe-users-template.csv" "coe-users-workshop1.csv"
# Edit with Workshop 1 participants

# Workshop 2
Copy-Item "coe-users-template.csv" "coe-users-workshop2.csv"  
# Edit with Workshop 2 participants

# Run specific workshop
.\Provision-COE-Users.ps1 -UserDataFile "coe-users-workshop1.csv" `
  -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
  -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878"
```

## 🧪 Testing the Script

### Test Mode Execution

Run the script with `-WhatIf` to see what would happen without making changes:

```powershell
.\Provision-COE-Users.ps1 -UserDataFile "coe-users.csv" `
  -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
  -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878" `
  -WhatIf
```

### Validate Prerequisites

Check required modules and authentication:

```powershell
# Test module availability
Get-Module -ListAvailable Microsoft.Graph
Get-Module -ListAvailable Az

# Test authentication status
Get-MgContext
Get-AzContext

# Test tenant access
Get-MgDomain | Where-Object {$_.Id -eq "fabrikam.cspsecurityaccelerate.com"}
```

### Manual Testing Steps

1. **Verify CSV data format**:
   ```powershell
   Import-Csv "coe-users.csv" | Format-Table
   ```

2. **Test single user creation**:
   ```powershell
   # Create test CSV with one user
   @"
   DisplayName,UserPrincipalName,Email
   Test User,testuser,testuser@microsoft.com
   "@ | Out-File "test-user.csv" -Encoding UTF8
   
   # Run script with test user
   .\Provision-COE-Users.ps1 -UserDataFile "test-user.csv" `
     -TenantDomain "fabrikam.cspsecurityaccelerate.com" `
     -AzureSubscriptionId "b7699934-0c99-4899-8799-763fffc90878"
   ```

3. **Cleanup test resources**:
   ```powershell
   # Remove test users and resources
   Get-MgUser -Filter "startswith(displayName,'Test')" | Remove-MgUser
   Get-AzResourceGroup -Name "*test*" | Remove-AzResourceGroup -Force
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

**Cannot find my resource group**:
- Use Azure Portal: Go to **Resource groups** and look for `rg-fabrikam-coe-[your-username]`
- Use PowerShell: `Get-AzResourceGroup -Name "rg-fabrikam-coe-[your-username]"`
- Example: If your username is `chridep`, look for `rg-fabrikam-coe-chridep`

### Getting Help

1. **Run with -WhatIf first** to identify issues
2. **Check error messages** in the console output
3. **Verify prerequisites** (modules, permissions)
4. **Review Azure Portal** for created resources
5. **Check M365 Admin Center** for user status

## 📈 Post-Provisioning Steps

### For COE Demo Participants

1. **Access Azure Portal**: https://portal.azure.com
   - Use your assigned credentials: `username@fabrikam.cspsecurityaccelerate.com`
   - Change your password on first login
   - Locate your dedicated resource group: `rg-fabrikam-coe-[your-username]`

2. **Find your resource group**:
   - Go to **Resource groups** in Azure Portal
   - Look for `rg-fabrikam-coe-[your-username]` (e.g., `rg-fabrikam-coe-chridep`)
   - This is your dedicated workspace for the Fabrikam demo

2. **Access Copilot Studio**: https://copilotstudio.microsoft.com
   - Sign in with your COE credentials
   - Create your demo copilot
   - Test integration with Fabrikam platform

### For Workshop Administrators

1. **Verify provisioning results**:
   ```powershell
   # Check user creation
   Get-MgUser -Filter "endsWith(userPrincipalName,'fabrikam.cspsecurityaccelerate.com')"
   
   # Check resource groups and their tags
   Get-AzResourceGroup -Name "rg-fabrikam-coe-*" | 
     Select-Object ResourceGroupName, Tags
   
   # Find resource group for specific user (now easier!)
   Get-AzResourceGroup -Name "rg-fabrikam-coe-chridep"
   ```

2. **Share credentials with participants**:
   - Provide login instructions
   - Include password change requirements
   - Share Azure Portal and Copilot Studio links

3. **Monitor usage during workshop**:
   - Check Azure costs in subscription
   - Monitor resource group activity
   - Assist with authentication issues

4. **Map users to resource groups**:
   ```powershell
   # Get all COE resource groups (now with predictable names!)
   Get-AzResourceGroup -Name "rg-fabrikam-coe-*" | 
     Select-Object ResourceGroupName, 
       @{Name="AssignedUser"; Expression={$_.Tags.createdFor}},
       @{Name="Created"; Expression={$_.Tags.created}} |
     Format-Table -AutoSize
   
   # Resource group names directly show the username
   # rg-fabrikam-coe-chridep     -> chridep user
   # rg-fabrikam-coe-franvanh    -> franvanh user
   # rg-fabrikam-coe-davidb      -> davidb user
   ```

5. **Post-workshop cleanup** (if needed):
   ```powershell
   # Remove user accounts (optional)
   Get-MgUser -Filter "endsWith(userPrincipalName,'fabrikam.cspsecurityaccelerate.com')" | Remove-MgUser
   
   # Remove resource groups
   Get-AzResourceGroup -Name "rg-fabrikam-coe-*" | Remove-AzResourceGroup -Force
   ```

## 🔗 Related Documentation

- **[SECURITY-AND-PRIVACY.md](SECURITY-AND-PRIVACY.md)**: Complete security guidelines
- **[RUN-COE-PROVISIONING.md](RUN-COE-PROVISIONING.md)**: Helper script documentation
- **[COE-DEMO-OVERVIEW.md](COE-DEMO-OVERVIEW.md)**: Workshop structure and content
- **[COPILOT-STUDIO-JWT-SETUP.md](COPILOT-STUDIO-JWT-SETUP.md)**: Authentication configuration

## 💡 Tips for Success

- **Test everything first** with `-WhatIf` parameter
- **Start small** with 1-2 users before provisioning entire group
- **Have backup plan** for authentication issues during demo
- **Monitor costs** as each user has full Azure access
- **Document any customizations** for future workshops
- **Keep security top-of-mind** - these are privileged accounts

### 🎯 **Resource Group Benefits**

The simplified naming pattern `rg-fabrikam-coe-[username]` provides:
- **Easy identification**: Users can immediately find their resource group
- **Simple deployment**: No need to search through tags or random suffixes
- **Clear ownership**: Resource group name directly shows the assigned user
- **Predictable pattern**: Follows standard Azure naming conventions

---

*This documentation supports the Fabrikam COE Demo Workshop. For technical issues, refer to the troubleshooting section above or consult the project documentation.*
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

## 🔧 Troubleshooting

### Authentication Issues

**Problem**: Account selection prompt with no options after browser authentication

**Symptoms**:
- Browser authentication completes successfully
- Script shows "Please select the account you want to login with" 
- No accounts appear in the selection prompt
- Script hangs or fails

**Solutions**:

1. **Use the cache clearing option in helper script**:
   ```powershell
   .\Run-COE-Provisioning.ps1
   # Choose option 3: "Clear authentication cache"
   ```

2. **Run standalone cache clearing script**:
   ```powershell
   .\Clear-AuthCache.ps1
   ```

3. **Manual cache clearing**:
   ```powershell
   Disconnect-MgGraph -ErrorAction SilentlyContinue
   Disconnect-AzAccount -ErrorAction SilentlyContinue  
   Clear-AzContext -Force -ErrorAction SilentlyContinue
   ```

4. **Verify tenant-specific authentication**:
   - Ensure `tenantId` is set in your `coe-config.json`
   - The script should show "Using tenant-specific authentication: {tenantId}"

**Prevention**:
- Always clear authentication cache between different tenants
- Use tenant-specific authentication (tenantId in config)
- Authenticate with the target tenant account from the start

### Configuration Issues

**Problem**: "No users found in configuration file"

**Solution**: 
- Verify `coe-config.json` has a `users` array with at least one user
- Check JSON syntax is valid
- Ensure user objects have `displayName`, `username`, and `originalEmail` properties

**Problem**: Script can't find configuration file

**Solution**:
- Ensure `coe-config.json` exists in the same directory as the script
- Copy from template: `Copy-Item 'coe-config-template.json' 'coe-config.json'`
- Edit the copied file with your actual tenant and user data

### Permission Issues

**Problem**: Script fails with permission errors

**Solution**:
- Verify you have Global Admin rights in the target tenant
- Check Azure subscription access (Contributor or Owner role)
- Ensure the authenticated account matches the required permissions

**Problem**: Resource group creation fails

**Solution**:
- Verify Azure subscription ID is correct
- Check Azure region is valid and available
- Ensure no naming conflicts with existing resource groups

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
