# 🔧 Workshop Scripts

PowerShell scripts for workshop setup, user management, and troubleshooting.

## 📋 Script Categories

### **User Provisioning**
- `Provision-COE-Users.ps1` - Main user provisioning script
- `Run-COE-Provisioning.ps1` - Wrapper script with dependency management
- `Test-UserAddition.ps1` - Test script for safe re-runs

### **Permission Management**
- `Fix-COE-Permissions.ps1` - Fix workshop permission issues
- `Fix-RoleAssignments.ps1` - Repair role assignment problems
- `Fix-UserAccessAdmin-Permissions.ps1` - Fix user access admin roles
- `Verify-COE-Permissions.ps1` - Verify permission setup

### **Authentication & Testing**
- `Test-Authentication.ps1` - Test authentication workflows
- `Simple-Auth-Test.ps1` - Basic authentication verification
- `Clear-AuthCache.ps1` - Clear authentication cache for troubleshooting

### **Utilities**
- `AuthenticationHelpers.psm1` - PowerShell module with helper functions

## 🚀 Usage

Most scripts require admin privileges and should be run by workshop organizers only.

### Example: Provision Users
```powershell
# Run the main provisioning script
.\Run-COE-Provisioning.ps1

# Test a specific user addition
.\Test-UserAddition.ps1 -UserEmail "participant@example.com"
```

### Example: Fix Permissions
```powershell
# Verify current permissions
.\Verify-COE-Permissions.ps1

# Fix any issues found
.\Fix-COE-Permissions.ps1
```

## ⚠️ Prerequisites

- PowerShell 5.1 or later
- Appropriate Azure AD permissions
- Access to workshop Azure subscription
- Configuration files in `../config/` folder

---

**Note**: These scripts are for workshop organizers. Participants should use the main workshop guides.
