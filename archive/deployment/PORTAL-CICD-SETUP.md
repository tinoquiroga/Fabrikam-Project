# 🚀 Post-Deployment CI/CD Setup Guide

## 📋 Overview

After deploying your Fabrikam infrastructure with the ARM template, follow this guide to set up **automatic deployments** using Azure Portal's Deployment Center - no command line tools required!

This approach mirrors the successful [CIPP project pattern](https://docs.cipp.app/setup/self-hosting-guide/runfrompackage) and avoids security restrictions often encountered with PowerShell/CLI automation.

## 🎯 Why This Approach?

### **✅ Advantages over Command Line Setup:**
- **🔐 No security restrictions** - Works within Azure Portal permissions
- **👥 User-friendly** - Point-and-click interface, no CLI tools needed
- **🛡️ Secure** - Azure manages secrets automatically
- **📱 Accessible** - Works from any browser, including mobile
- **🔄 Reliable** - Azure's native CI/CD integration

### **✅ Compared to CIPP Pattern:**
- Same proven deployment approach that works for thousands of users
- Leverages Azure Portal's native GitHub integration
- Automatic GitHub Actions workflow generation
- Built-in secrets management

## 🚀 **Step-by-Step Setup Guide**

### **Prerequisites**
Before starting, ensure you have:
- ✅ **Fabrikam infrastructure deployed** (via ARM template)
- ✅ **GitHub repository forked** from `davebirr/Fabrikam-Project`
- ✅ **GitHub Personal Access Token** with `repo` and `workflow` permissions
  - [Create GitHub PAT Guide](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)

---

## 🔧 **Part 1: API Service CI/CD Setup**

### **Step 1: Navigate to API App Service**
1. Open [Azure Portal](https://portal.azure.com)
2. Go to **Resource Groups** → Find your resource group (e.g., `rg-fabrikam-test`)
3. Click on your **API App Service** (e.g., `fabrikam-api-dev-nf66`)

### **Step 2: Configure Deployment Center**
1. In the left sidebar, click **Deployment Center**
2. Under **Source**, select **GitHub**
3. **Building with GitHub Actions** should be selected (default option)
4. **Signed in as**: Your GitHub account (e.g., `davebirr`) - this will be unique per user
5. **Organization**: Select your GitHub username/organization
6. **Repository**: Select `Fabrikam-Project` (should be same for everyone)
7. **Branch**: 
   - For demo users: Select `main`
   - For development: Select your feature branch (e.g., `feature/phase-1-authentication`)

### **Step 3: Workflow Configuration**
8. **Workflow Option**: **"Add a workflow"** (creates new instance-specific workflow)

### **Step 4: Build Configuration**
9. **Runtime Stack**: **.NET** (default)
10. **Runtime Version**: **9.0** (default)  
11. **Authentication Type**: **User-assigned identity** (default - recommended over Basic)
12. **Subscription**: Your subscription name (e.g., `MCPP Subscription`) - varies per user
13. **Identity**: **(new) oidc-msi-xxxx** (default - Azure generates unique name)
    - ✅ **Recommended**: Use Azure's auto-generated identity (easier setup)
    - ⚡ **Advanced**: Pre-create identity with consistent naming if desired

### **Step 5: Save Configuration**
14. Click **Save**
15. Azure will:
    - ✅ Create GitHub Actions workflow file
    - ✅ Configure deployment secrets automatically
    - ✅ Start first deployment immediately

**What happens next:**
- ✅ Azure creates a new GitHub Actions workflow file (e.g., `main_fabrikam-api-dev-nf66.yml`)
- ✅ The workflow is optimized for your specific app service
- ✅ Deployment secrets are automatically configured
- ✅ First deployment starts immediately

### **Step 4: Build Configuration**
1. **Runtime Stack**: `.NET 9`
2. **Version**: `9.0`
3. **Build Command**: Leave empty (uses default)
4. **Startup Command**: Leave empty

### **Step 5: Advanced Settings**
Click **Advanced** and configure:
```yaml
# Build and deployment paths for monorepo
Working Directory: FabrikamApi/src
Publish Profile: FabrikamApi/src/FabrikamApi.csproj
```

### **Step 6: Save Configuration**
1. Click **Save**
2. Azure will create a GitHub Actions workflow file
3. The first deployment will start automatically

---

## 🔧 **Part 2: MCP Service CI/CD Setup**

### **Step 1: Navigate to MCP App Service**
1. In the same resource group, click your **MCP App Service** (e.g., `fabrikam-mcp-dev-nf66`)
2. Go to **Deployment Center**

### **Step 2: Repository Configuration**
1. **Source**: **GitHub** (should remember authorization)
2. **Building with GitHub Actions**: Selected by default
3. **Signed in as**: Your GitHub account (already authenticated)
4. **Organization**: Your GitHub username
5. **Repository**: `Fabrikam-Project` (same repository)
6. **Branch**: Same branch as API (e.g., `main` or your feature branch)
7. **Workflow Option**: **"Add a workflow"** (creates MCP-specific workflow)
8. Click **Save**

**Result**: Second GitHub Actions workflow created for MCP service

---

## 🧪 **Testing Scenario: Using Feature Branch**

If you're testing with a feature branch (like `feature/phase-1-authentication`):

### **✅ Expected Behavior**
- ✅ Workflows trigger on pushes to your feature branch
- ✅ Both API and MCP services deploy independently
- ✅ You can test changes before merging to main

### **🔄 Branch Strategy**
```
feature/phase-1-authentication → Deploy to test instance (nf66)
main → Deploy to production instances (user forks)
```

This allows you to:
- Test the CI/CD process with real changes
- Validate deployment workflows work correctly  
- Refine the process before documenting for demo users

---

## 📊 **Part 3: Verify and Optimize**

### **Check GitHub Actions**
1. Go to your GitHub repository
2. Click the **Actions** tab
3. You should see 2 new workflows:
   - `Build and deploy .NET Core app to Azure Web App - fabrikam-api-dev-nf66`
   - `Build and deploy .NET Core app to Azure Web App - fabrikam-mcp-dev-nf66`

### **Monitor First Deployment**
1. Click on the running workflow
2. Watch the build and deployment process
3. **Note**: Initial deployment may fail due to monorepo structure

### **🎯 Optimize Workflows (Recommended)**
Azure's auto-generated workflows often need refinement for monorepo projects.

**📖 See [Workflow Optimization Guide](WORKFLOW-OPTIMIZATION.md) for CIPP-inspired improvements**

Key optimizations:
- ✅ **Simpler workflow structure** (following CIPP's proven pattern)
- ✅ **Proper monorepo path handling** (`FabrikamApi/src`, `FabrikamMcp/src`)
- ✅ **Publish profiles** instead of managed identity (more reliable)
- ✅ **Targeted deployments** (only deploy when relevant files change)

### **Test Deployed Services**
```bash
# Test API (replace with your suffix)
curl https://fabrikam-api-dev-nf66.azurewebsites.net/health

# Test MCP (replace with your suffix)  
curl https://fabrikam-mcp-dev-nf66.azurewebsites.net/status
```

---

## 🔄 **Part 4: Fork Sync Strategy**

### **Option A: Manual Sync (Recommended for Demo Users)**

#### **Keep Your Fork Updated**
1. **In GitHub Web Interface**:
   - Go to your fork
   - Click **Sync fork** button
   - Select **Update branch** if changes are available

2. **Or via Git Commands**:
   ```bash
   git remote add upstream https://github.com/davebirr/Fabrikam-Project
   git fetch upstream
   git checkout main
   git merge upstream/main
   git push origin main
   ```

3. **Automatic Deployment**:
   - Any push to `main` triggers deployment
   - Both API and MCP services update automatically

### **Option B: Automated Sync (Advanced Users)**

Create a GitHub Action for daily upstream sync:

1. **Create** `.github/workflows/sync-upstream.yml`:
   ```yaml
   name: Sync from Upstream
   on:
     schedule:
       - cron: '0 2 * * *'  # Daily at 2 AM UTC
     workflow_dispatch:        # Manual trigger
   
   jobs:
     sync:
       runs-on: ubuntu-latest
       steps:
       - uses: actions/checkout@v4
         with:
           token: ${{ secrets.GITHUB_TOKEN }}
           fetch-depth: 0
       
       - name: Sync upstream changes
         run: |
           git config user.name "GitHub Actions"
           git config user.email "actions@github.com"
           git remote add upstream https://github.com/davebirr/Fabrikam-Project
           git fetch upstream
           git checkout main
           git merge upstream/main --no-edit
           git push origin main
   ```

---

## 🎉 **Success Indicators**

### **✅ You've Successfully Set Up CI/CD When:**
1. **GitHub Actions** shows 2 successful workflows
2. **App Services** display your application (not Azure welcome page)
3. **Health endpoints** return API responses:
   ```json
   {"status": "Healthy", "timestamp": "2025-07-27T18:00:00Z"}
   ```
4. **Auto-deployment** works: Push to main → GitHub Actions runs → Apps update

### **🔧 Troubleshooting Common Issues**

#### **Deployment Fails**
1. Check **GitHub Actions logs** for build errors
2. Verify **project paths** in Deployment Center settings
3. Ensure **.NET 9** is selected as runtime stack

#### **Apps Show Welcome Page**
1. **Wait 5 minutes** - initial deployment takes time
2. Check **App Service logs** in Azure Portal
3. Verify **startup commands** and **project structure**

#### **GitHub Authorization Issues**
1. **Revoke and re-authorize** Azure's GitHub access
2. Check **PAT permissions** (repo, workflow, admin:repo_hook)
3. Try **Basic Authentication** instead of managed identity

---

## 📚 **Additional Resources**

### **Microsoft Documentation**
- [Azure App Service Deployment Center](https://docs.microsoft.com/en-us/azure/app-service/deploy-continuous-deployment)
- [GitHub Actions for Azure](https://docs.microsoft.com/en-us/azure/developer/github/)
- [App Service Authentication](https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization)

### **Fabrikam Project Guides**
- [Main Deployment README](README.md) - ARM template deployment
- [VS Code Configuration](Configure-VSCodeSettings.ps1) - Development setup
- [Architecture Documentation](../docs/architecture/) - System design

### **Inspired By**
- [CIPP Self-Hosting Guide](https://docs.cipp.app/setup/self-hosting-guide/) - The pattern we follow
- [Azure Static Web Apps CI/CD](https://docs.microsoft.com/en-us/azure/static-web-apps/github-actions-workflow) - Similar approach

---

## 🎯 **Why This Works Better**

### **Compared to Command Line Automation:**
| Aspect | Command Line Setup | Azure Portal Setup |
|--------|-------------------|-------------------|
| **Security** | ❌ Often blocked by enterprise policies | ✅ Works within Azure permissions |
| **User Experience** | ❌ Requires CLI tools and tokens | ✅ Point-and-click interface |
| **Troubleshooting** | ❌ Complex error messages | ✅ Visual feedback and logs |
| **Accessibility** | ❌ Developer tools required | ✅ Any browser, any device |
| **Enterprise Friendly** | ❌ May violate security policies | ✅ Standard Azure functionality |

### **Demo User Benefits:**
- **🚀 Fast Setup** - 10 minutes vs 30+ minutes with CLI
- **🛡️ No Security Issues** - Works in restricted environments  
- **📱 Mobile Friendly** - Can set up from phone/tablet
- **👥 Team Collaboration** - Multiple people can configure
- **🔄 Reliable Updates** - Azure manages the sync process

This approach follows the **proven CIPP pattern** that has enabled thousands of users to successfully deploy and maintain their instances with minimal technical barriers.
