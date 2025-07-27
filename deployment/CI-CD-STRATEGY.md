# 🎯 Fabrikam CI/CD & Fork Strategy Guide

## 📋 Overview

This guide explains the **professional deployment pattern** for Fabrikam instances, designed for **demo users** and **enterprise customers** who want to maintain their own instances while receiving upstream updates.

## 🔄 **Fork-and-Sync Pattern**

### **The Strategy**
```
┌─────────────────┐    Fork     ┌─────────────────┐
│ Upstream Repo   │ ────────────> │ User's Fork     │
│ (davebirr/...)  │              │ (user/...)      │
│                 │              │                 │
│ ✅ New Features │    Sync      │ ✅ Custom Config│
│ ✅ Bug Fixes    │ <────────────│ ✅ Instance Data│
│ ✅ Updates      │              │ ✅ CI/CD Setup  │
└─────────────────┘              └─────────────────┘
                                          │
                                          │ Deploy
                                          ▼
                                 ┌─────────────────┐
                                 │ Azure Instance  │
                                 │ (unique suffix) │
                                 │                 │
                                 │ ✅ API Service  │
                                 │ ✅ MCP Service  │
                                 │ ✅ Monitoring   │
                                 └─────────────────┘
```

### **Why This Pattern?**

✅ **Benefits for Demo Users:**
- Get new features automatically
- Maintain custom configurations
- Independent deployment control
- No conflicts with upstream changes

✅ **Benefits for Enterprise:**
- Security isolation (own GitHub repository)
- Custom branding and configuration
- Audit trail and compliance
- Professional deployment practices

## 🚀 **Complete Setup Guide**

### **Phase 1: Fork & Deploy Infrastructure**

#### **Step 1: Fork the Repository**
```bash
# Via GitHub UI or CLI
gh repo fork davebirr/Fabrikam-Project --clone
cd Fabrikam-Project
```

#### **Step 2: Deploy Infrastructure with ARM Template**
```bash
# Option A: One-click deployment
# Click "Deploy to Azure" button in README

# Option B: Azure CLI deployment
az group create --name "rg-fabrikam-demo" --location "East US 2"
az deployment group create \
  --resource-group "rg-fabrikam-demo" \
  --template-file "deployment/AzureDeploymentTemplate.json" \
  --parameters "deployment/AzureDeploymentTemplate.parameters.json"
```

#### **Step 3: Configure VS Code Settings**
```powershell
# Auto-configure VS Code with deployment outputs
.\deployment\Configure-VSCodeSettings.ps1 -ResourceGroup "rg-fabrikam-demo"
```

### **Phase 2: Set Up CI/CD**

#### **Step 4: Create GitHub Personal Access Token**
1. Go to GitHub → Settings → Developer settings → Personal access tokens
2. Create token with these scopes:
   - `repo` (Full control of private repositories)
   - `workflow` (Update GitHub Action workflows)
   - `admin:repo_hook` (Full control of repository hooks)

#### **Step 5: Configure GitHub Actions**
```powershell
# Set up CI/CD with fork relationship
.\deployment\Setup-GitHubActions.ps1 `
  -ResourceGroup "rg-fabrikam-demo" `
  -GitHubRepository "https://github.com/YOUR-USERNAME/Fabrikam-Project" `
  -GitHubToken "ghp_your_token_here" `
  -SetupFork
```

**What this script does:**
- ✅ Creates Azure service principal for deployment
- ✅ Configures GitHub repository secrets
- ✅ Creates instance-specific GitHub Actions workflows
- ✅ Sets up upstream sync automation
- ✅ Configures fork relationship

#### **Step 6: Commit and Deploy**
```bash
# Add the new workflow files
git add .github/workflows/
git commit -m "feat: Add instance-specific CI/CD workflows"
git push origin main

# This triggers the first deployment!
```

### **Phase 3: Ongoing Operations**

#### **Daily Upstream Sync (Automated)**
The `sync-upstream.yml` workflow runs daily and:
1. ✅ Fetches latest changes from upstream
2. ✅ Merges into your main branch
3. ✅ Creates PR for review if there are conflicts
4. ✅ Automatically deploys non-breaking changes

#### **Manual Sync (When Needed)**
```bash
# Manual sync from upstream
git fetch upstream
git checkout main
git merge upstream/main
git push origin main
```

#### **Custom Configurations**
Your fork can maintain:
- ✅ Custom `appsettings.json` values
- ✅ Environment-specific configurations  
- ✅ Custom domain settings
- ✅ Branding and UI customizations
- ✅ Additional features and integrations

## 🔧 **ARM Template vs Post-Deployment CI/CD**

### **ARM Template Limitations for CI/CD**
```json
// ❌ ARM template can do this, but it's limited:
{
  "type": "Microsoft.Web/sites/sourcecontrols",
  "properties": {
    "repoUrl": "[parameters('githubRepository')]",
    "branch": "main",
    "isManualIntegration": false
  }
}
```

**❌ Problems:**
- Requires GitHub PAT in ARM parameters (security risk)
- No support for monorepo structure
- No custom build steps
- No environment-specific logic
- No secrets management
- No upstream sync capability

### **Post-Deployment Setup Advantages**
```yaml
# ✅ Full GitHub Actions workflow with proper security:
name: Deploy to Azure (nf66)
on:
  push:
    branches: [ main, develop ]
jobs:
  deploy-api:
    runs-on: ubuntu-latest
    steps:
    - name: Build API
      run: dotnet publish FabrikamApi/src/FabrikamApi.csproj -c Release
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.API_APP_NAME }}
```

**✅ Advantages:**
- Secure secrets management
- Instance-specific workflows
- Monorepo support
- Custom build logic
- Environment promotion
- Fork and sync automation
- Professional DevOps practices

## 📊 **Workflow Files Created**

### **1. `deploy-instance-{suffix}.yml`**
- ✅ Instance-specific deployment workflow
- ✅ Build, test, and deploy both API and MCP services
- ✅ Triggered on push to main branch
- ✅ Uses instance-specific app names and resource groups

### **2. `sync-upstream.yml`** (if SetupFork enabled)
- ✅ Daily automated sync from upstream
- ✅ Handles merge conflicts gracefully
- ✅ Creates PR for manual review when needed
- ✅ Maintains fork relationship

## 🔐 **Security & Secrets Management**

### **GitHub Secrets Created**
```
AZURE_CREDENTIALS       # Service principal for Azure deployment
AZURE_SUBSCRIPTION_ID   # Target Azure subscription
AZURE_RESOURCE_GROUP    # Instance resource group
API_APP_NAME            # API service app name
MCP_APP_NAME            # MCP service app name  
INSTANCE_SUFFIX         # Unique instance identifier
```

### **Service Principal Permissions**
- ✅ **Scope**: Limited to instance resource group only
- ✅ **Role**: Contributor (can deploy apps, not create resources)
- ✅ **Security**: Instance-specific, no cross-contamination

## 🎯 **Demo User Experience**

### **Simple Setup (5 minutes)**
1. **Fork repository** → Click fork button
2. **Deploy infrastructure** → Click "Deploy to Azure" button  
3. **Configure CI/CD** → Run one PowerShell script
4. **Start developing** → Push code, automatic deployment

### **Ongoing Experience**
- ✅ **Get updates automatically** → Upstream sync runs daily
- ✅ **Deploy changes instantly** → Push to main triggers deployment
- ✅ **Monitor deployments** → GitHub Actions provides full visibility
- ✅ **Maintain customizations** → Fork preserves your changes

## 🏆 **Enterprise Benefits**

### **Compliance & Governance**
- ✅ **Audit trail** → All deployments tracked in GitHub Actions
- ✅ **Security isolation** → Each instance has own service principal
- ✅ **Change management** → PR reviews for upstream changes
- ✅ **Environment promotion** → Support for dev/staging/prod

### **Operational Excellence**
- ✅ **Monitoring** → Application Insights and Log Analytics included
- ✅ **Scaling** → Azure App Service auto-scaling capabilities
- ✅ **Backup & DR** → Azure built-in backup and disaster recovery
- ✅ **Custom domains** → Professional DNS configuration support

## 🔄 **Comparison: ARM vs Post-Deployment**

| Feature | ARM Template Only | Post-Deployment Setup |
|---------|-------------------|----------------------|
| **Security** | ❌ PAT in template | ✅ Secure secrets mgmt |
| **Monorepo** | ❌ Limited support | ✅ Full support |
| **Custom Logic** | ❌ Basic only | ✅ Full customization |
| **Fork Sync** | ❌ Not possible | ✅ Automated |
| **Environment Promotion** | ❌ Manual only | ✅ Automated workflows |
| **Professional DevOps** | ❌ Basic | ✅ Enterprise-grade |

## 🎉 **Recommendation**

**Use the Post-Deployment Setup approach** for the Fabrikam project because:

1. **✅ Security** → Proper secrets management without exposing tokens
2. **✅ Flexibility** → Full GitHub Actions capabilities
3. **✅ Monorepo Support** → Handles complex project structure
4. **✅ Fork Pattern** → Enables the demo user workflow you want
5. **✅ Enterprise Ready** → Professional DevOps practices
6. **✅ Maintainable** → Easy to update and customize

The ARM template focuses on **infrastructure deployment**, while the PowerShell script handles **application lifecycle management**—the perfect separation of concerns for a scalable demo platform.
