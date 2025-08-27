# 🎓 COE Workshop - August 27, 2025

**Center of Excellence Workshop: Building and Deploying AI-Powered Applications with Fabrikam**

## 📁 Workshop Structure

This workshop folder (`ws-coe-aug27`) contains everything needed for the COE session:

### 📋 **Setup & Guides**
- **`COE-COMPLETE-SETUP-GUIDE.md`** - Complete step-by-step setup from browser profile to deployment
- **`Browser-Profile-Setup-Checklist.md`** - Quick checklist for browser profile setup
- **`COE-QUICK-REFERENCE.md`** - Quick reference for common tasks

### 🚀 **Deployment**
- **One-Click Deploy Button** - Direct deployment from main repository template

### 🔧 **Provisioning Scripts** (Admin Use Only)
- **`Provision-COE-Users.ps1`** - Main user provisioning script
- **`Run-COE-Provisioning.ps1`** - Wrapper script with dependency management
- **`Test-UserAddition.ps1`** - Test script for safe re-runs
- **`coe-config.json`** - User configuration file

### 📊 **User Management**
- **`Fix-COE-Permissions.ps1`** - Fix permissions to proper Reader + RG Contributor model
- **`Verify-COE-Permissions.ps1`** - Verify permission setup
- **`coe-users-template.csv`** - Template for bulk user import

### 🔐 **Authentication & Security**
- **`SECURITY-NOTICE.md`** - Important security considerations
- **`Simple-Auth-Test.ps1`** - Test authentication setup
- **`Clear-AuthCache.ps1`** - Clear authentication cache

## 🎯 **Workshop Objectives**

By the end of this workshop, participants will have:

1. ✅ **Clean Browser Profile**: Dedicated profile for COE work
2. ✅ **Azure Deployment**: Fabrikam app running in their resource group
3. ✅ **GitHub Integration**: Forked repository with CI/CD pipeline
4. ✅ **Proper Permissions**: Reader at subscription + Contributor to own RG
5. ✅ **Working Application**: API and MCP servers deployed and tested

## 🌐 **Pre-Created Resources**

Each participant has been provisioned with:

- **User Account**: `[username]@fabrikam.cspsecurityaccelerate.com`
- **Resource Group**: `rg-fabrikam-coe-[username]`
- **Default Password**: `TempPassword123!` (change during setup)
- **Permissions**: 
  - Reader access to entire subscription (can see everyone's work)
  - Contributor access to own resource group (can create/modify resources)

## 🔄 **Workshop Flow**

1. **Browser Profile Setup** (5 min)
2. **Initial Login & MFA** (10 min)
3. **GitHub Fork & Setup** (10 min)
4. **Azure Deployment** (15 min)
5. **CI/CD Pipeline** (15 min)
6. **Testing & Verification** (10 min)
7. **Copilot Studio Integration** (Optional)

**Total Time**: ~60 minutes

## 💡 **Key Features**

- **Safe Re-runs**: All scripts handle existing resources gracefully
- **Clean Authentication**: Browser profiles prevent account conflicts
- **Proper Isolation**: Each user has their own sandbox
- **Collaboration Ready**: Users can see each other's work but not modify it
- **CI/CD Ready**: GitHub Actions configured for automated deployment

## 🚨 **Important Notes**

- **Use your dedicated browser profile** for all COE activities
- **Your resource group** is pre-created: `rg-fabrikam-coe-[username]`
- **Default password** must be changed on first login
- **MFA setup** is required during initial login
- **Reader permissions** let you see other participants' work
- **Contributor permissions** only apply to your own resource group

## 📞 **Support**

If you encounter issues during the workshop:
1. Check the troubleshooting sections in the guides
2. Ask the workshop facilitator
3. Reference the `COE-QUICK-REFERENCE.md` for common solutions

---

**Happy Learning! 🚀**
