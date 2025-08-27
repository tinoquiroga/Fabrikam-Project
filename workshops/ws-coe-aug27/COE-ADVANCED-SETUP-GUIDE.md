# 🏗️ COE Advanced Setup Guide

**This guide is for participants who want to manually create deployment workflows instead of using Azure Portal's automated approach.**

## When to Use This Guide

- You prefer full control over your CI/CD configuration
- You want to understand the complete workflow structure
- You're comfortable editing YAML files
- You need custom deployment configurations

## Prerequisites

- Complete **Steps 1-4** of the [COE Complete Setup Guide](COE-COMPLETE-SETUP-GUIDE.md)
- Skip Step 5 in the main guide and follow this advanced approach instead

---

## 🚀 Advanced Step 5: Manual CI/CD Pipeline Setup

**🎯 Goal**: Manually create optimized deployment workflows that are pre-configured for monorepo compatibility.

### 5.1 Create API Deployment Workflow

1. **In your forked repository**, navigate to `.github/workflows/`

2. **Create new file**: Click **Add file** > **Create new file**

3. **Name the file**: `deploy-api.yml`

4. **Add the workflow content**:
   ```yaml
   name: Deploy API to Azure App Service

   on:
     push:
       branches:
         - main
       paths:
         - 'FabrikamApi/**'
         - '.github/workflows/deploy-api.yml'
     workflow_dispatch:

   env:
     AZURE_WEBAPP_NAME: 'YOUR_API_APP_SERVICE_NAME'    # Replace with your app service name
     AZURE_WEBAPP_PACKAGE_PATH: './publish'
     DOTNET_VERSION: '9.0.x'

   jobs:
     build-and-deploy:
       runs-on: ubuntu-latest
       
       steps:
       - name: 'Checkout GitHub Action'
         uses: actions/checkout@v4

       - name: Setup .NET Core
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: ${{ env.DOTNET_VERSION }}

       - name: 'Restore dependencies'
         run: dotnet restore FabrikamApi/src/FabrikamApi.csproj

       - name: 'Build project'
         run: dotnet build FabrikamApi/src/FabrikamApi.csproj --configuration Release --no-restore

       - name: 'Test project'
         run: dotnet test FabrikamTests/FabrikamTests.csproj --no-build --verbosity normal

       - name: 'Publish project'
         run: dotnet publish FabrikamApi/src/FabrikamApi.csproj --configuration Release --no-build --output ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}

       - name: 'Deploy to Azure WebApp'
         uses: azure/webapps-deploy@v3
         with:
           app-name: ${{ env.AZURE_WEBAPP_NAME }}
           publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_API }}
           package: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
   ```

5. **Commit the file** with message: "Add API deployment workflow"

### 5.2 Create MCP Deployment Workflow

1. **Create another new file**: `deploy-mcp.yml`

2. **Add the workflow content**:
   ```yaml
   name: Deploy MCP to Azure App Service

   on:
     push:
       branches:
         - main
       paths:
         - 'FabrikamMcp/**'
         - '.github/workflows/deploy-mcp.yml'
     workflow_dispatch:

   env:
     AZURE_WEBAPP_NAME: 'YOUR_MCP_APP_SERVICE_NAME'    # Replace with your app service name
     AZURE_WEBAPP_PACKAGE_PATH: './publish'
     DOTNET_VERSION: '9.0.x'

   jobs:
     build-and-deploy:
       runs-on: ubuntu-latest
       
       steps:
       - name: 'Checkout GitHub Action'
         uses: actions/checkout@v4

       - name: Setup .NET Core
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: ${{ env.DOTNET_VERSION }}

       - name: 'Restore dependencies'
         run: dotnet restore FabrikamMcp/src/FabrikamMcp.csproj

       - name: 'Build project'
         run: dotnet build FabrikamMcp/src/FabrikamMcp.csproj --configuration Release --no-restore

       - name: 'Test project'
         run: dotnet test FabrikamTests/FabrikamTests.csproj --no-build --verbosity normal

       - name: 'Publish project'
         run: dotnet publish FabrikamMcp/src/FabrikamMcp.csproj --configuration Release --no-build --output ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}

       - name: 'Deploy to Azure WebApp'
         uses: azure/webapps-deploy@v3
         with:
           app-name: ${{ env.AZURE_WEBAPP_NAME }}
           publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_MCP }}
           package: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
   ```

3. **Commit the file** with message: "Add MCP deployment workflow"

### 5.3 Configure App Service Names

1. **Edit both workflow files** to replace the placeholder app service names:

   **For `deploy-api.yml`**:
   - Replace `YOUR_API_APP_SERVICE_NAME` with your actual API app service name
   - Example: `fabrikam-api-development-bb7fsc`

   **For `deploy-mcp.yml`**:
   - Replace `YOUR_MCP_APP_SERVICE_NAME` with your actual MCP app service name  
   - Example: `fabrikam-mcp-development-bb7fsc`

2. **Find your app service names** in Azure Portal:
   - Go to your resource group: `rg-fabrikam-coe-[your-username]`
   - Copy the exact names of your App Services

### 5.4 Configure Publish Profiles

1. **Get API App Service Publish Profile**:
   - In Azure Portal, go to your **API App Service**
   - Click **Get publish profile** (download button in top menu)
   - Open the downloaded `.publishsettings` file in notepad
   - Copy the entire XML content

2. **Add API Publish Profile Secret**:
   - In your GitHub repository, go to **Settings** > **Secrets and variables** > **Actions**
   - Click **New repository secret**
   - **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE_API`
   - **Value**: Paste the entire publish profile XML
   - Click **Add secret**

3. **Get MCP App Service Publish Profile**:
   - In Azure Portal, go to your **MCP App Service**
   - Click **Get publish profile**
   - Copy the XML content

4. **Add MCP Publish Profile Secret**:
   - In GitHub, add another secret
   - **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE_MCP`
   - **Value**: Paste the MCP publish profile XML
   - Click **Add secret**

### 5.5 Test Manual Deployment

1. **Trigger workflows manually**:
   - In your GitHub repository, go to **Actions** tab
   - Click on **Deploy API to Azure App Service**
   - Click **Run workflow** > **Run workflow**
   - Repeat for **Deploy MCP to Azure App Service**

2. **Monitor deployment progress**:
   - Watch the workflow runs in the Actions tab
   - Check Azure Portal for deployment status

3. **Test automatic deployment**:
   - Make a change to code in `FabrikamApi/` or `FabrikamMcp/`
   - Commit and push the changes
   - Only the relevant workflow should trigger (path-based filtering)

### 5.6 Verify Advanced CI/CD Setup

**✅ Success Criteria**:
- GitHub Actions shows 4 workflows total (2 core + 2 manual deployment)
- Manual workflow runs show green checkmarks
- Path-based triggering works (API changes only trigger API deployment)
- Both App Services show successful deployments
- Secrets are properly configured and working

---

## 🔄 Workflow Structure Overview

After completing this advanced setup, your repository will have:

### Core Workflows (from template)
- `testing.yml` - Runs tests on every push/PR
- `authentication-validation.yml` - Validates authentication setup

### Deployment Workflows (manually created)
- `deploy-api.yml` - Deploys API app on API code changes
- `deploy-mcp.yml` - Deploys MCP app on MCP code changes

### Key Advantages of Manual Approach
- **Path-based triggering**: Only deploys what changed
- **Optimized builds**: No unnecessary rebuilds
- **Full control**: Customize deployment steps as needed
- **Better performance**: Faster deployments with targeted builds
- **Monorepo ready**: Pre-configured for monorepo structure

---

## 🆚 Comparison: Portal vs Manual Approach

| Feature | Portal Approach | Manual Approach |
|---------|----------------|-----------------|
| **Setup Speed** | ⚡ Very Fast | 🐌 Moderate |
| **Customization** | 🔒 Limited | 🎛️ Full Control |
| **Monorepo Support** | ❌ Requires fixes | ✅ Built-in |
| **Path Filtering** | ❌ Deploys on every change | ✅ Smart triggering |
| **Learning Value** | 📖 Basic | 🎓 Advanced |
| **Maintenance** | 🔧 Azure managed | 👨‍💻 Self-managed |

Choose the approach that best fits your workshop goals and participant skill levels!

---

## 🛠️ Troubleshooting

### Common Issues

**Workflow fails with build errors:**
- Check that app service names are correct in workflow files
- Verify publish profile secrets are properly formatted (complete XML)

**Deployment succeeds but app doesn't work:**
- Check App Service logs in Azure Portal
- Verify environment variables are set correctly
- Check Application Insights for runtime errors

**Workflows don't trigger automatically:**
- Verify file paths in the `paths:` section match your changes
- Check that branch name is correct (`main` vs `master`)

### Getting Help

- Review the main [COE Complete Setup Guide](COE-COMPLETE-SETUP-GUIDE.md) for troubleshooting steps
- Check Azure Portal > App Service > Deployment Center for deployment logs
- Use GitHub Actions logs for detailed error information

---

**🏁 Next Step**: Continue with **Step 6** in the [main setup guide](COE-COMPLETE-SETUP-GUIDE.md#-step-6-test-your-deployment) to test your deployment.
