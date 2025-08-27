# 🚀 COE Complete Setup Guide - From GitHub Account to Production Deployment

**Welcome to the COE Team! This guide will take you from zero to a fully deployed Fabrikam project with CI/CD in your own Azure environment.**

## 📋 Overview

This guide covers:
- ✅ Setting up a dedicated browser profile for clean authentication
- ✅ Initial Azure/Microsoft 365 login and MFA setup
- ✅ Creating a GitHub account (if needed)
- ✅ Forking the Fabrikam project
- ✅ Deploying to your Azure subscription
- ✅ Setting up automated CI/CD pipelines
- ✅ Configuring authentication and testing

**⏱️ Estimated time: 30-45 minutes**

---

## 🔧 Prerequisites

Before starting, ensure you have:
- [ ] Your COE username (e.g., `imatest`, `chridep`, etc.)
- [ ] Access to the COE tenant: `fabrikam.cspsecurityaccelerate.com`
- [ ] Default password: `TempPassword123!` (you'll change this during setup)
- [ ] Web browser (Chrome, Edge, or Firefox recommended)

---

## 🌐 Step 1: Create Dedicated Browser Profile

**🎯 Goal**: Set up a clean browser environment to avoid authentication conflicts with your personal/work accounts.

### 1.1 Create New Browser Profile

**For Chrome:**
1. **Open Chrome** and click your profile icon (top-right)
2. **Click "Add"** → **"Add"** again
3. **Name**: Use your COE username (e.g., `imatest`, `chridep`)
4. **Choose an avatar** and click "Done"
5. **Chrome will open a new window** with your dedicated profile

**For Edge:**
1. **Open Edge** and click your profile icon (top-right)
2. **Click "Add profile"**
3. **Select "Add"** → **"Add without data"**
4. **Name**: Use your COE username (e.g., `imatest`, `chridep`)
5. **Choose a theme/avatar** and click "Confirm"
6. **Edge will open a new window** with your dedicated profile

**For Firefox:**
1. **Type in address bar**: `about:profiles`
2. **Click "Create a New Profile"**
3. **Click "Next"** → **Enter your COE username** → **"Finish"**
4. **Click "Launch profile in new browser"**

### 1.2 Initial Microsoft 365 / Azure Login

**🔑 This step establishes your identity in the COE tenant and sets up MFA**

1. **In your new browser profile**, go to: [portal.azure.com](https://portal.azure.com)

2. **Sign in with your COE credentials**:
   ```
   Email: [your-username]@fabrikam.cspsecurityaccelerate.com
   Password: TempPassword123!
   ```
   *(Replace `[your-username]` with your actual username like `imatest`)*

3. **Change your password** when prompted:
   - Choose a strong, unique password
   - **Write it down** - you'll need it for other tools

4. **Set up Multi-Factor Authentication (MFA)**:
   - Follow the prompts to set up MFA (phone app recommended)
   - Complete the verification process

5. **Accept any terms and conditions** that appear

6. **Verify access**: You should see the Azure Portal dashboard

### 1.3 Test Additional Services

**Test Copilot Studio access:**
1. **Open new tab**: [copilotstudio.microsoft.com](https://copilotstudio.microsoft.com)
2. **Verify** you can access without additional login prompts

**Test Microsoft 365 access:**
1. **Open new tab**: [portal.office.com](https://portal.office.com)
2. **Verify** you can access the M365 portal

### 1.4 Bookmark Key URLs

**Add these bookmarks** to your COE browser profile:
- Azure Portal: `https://portal.azure.com`
- Copilot Studio: `https://copilotstudio.microsoft.com`
- Microsoft 365: `https://portal.office.com`
- GitHub: `https://github.com`

**✅ Success Criteria**: You can access Azure, Copilot Studio, and M365 without authentication prompts between them.

---

## 📝 Step 2: Create GitHub Account (Skip if you already have one)

**🎯 Goal**: Set up a GitHub account for code collaboration and CI/CD.

### 2.1 Sign Up for GitHub

1. **In your COE browser profile**, navigate to [github.com](https://github.com)

2. **Click "Sign up"** in the top-right corner

3. **Enter your details**:
   ```
   Email: [use your work email or personal email]
   Password: [create a strong password - can be different from COE password]
   Username: [choose a unique username - consider using your name/initials]
   ```

4. **Verify your email**: GitHub will send a verification email - click the link to confirm

5. **Choose your plan**: Select "Free" (sufficient for this project)

### 2.2 Configure Your Profile

1. **Upload a profile photo** (optional but recommended)
2. **Add your name** in the profile settings
3. **Consider enabling two-factor authentication** for security

---

## 🍴 Step 3: Fork the Fabrikam Project

**🎯 Goal**: Create your own copy of the Fabrikam project for development and deployment.

### 3.1 Navigate to the Source Repository

1. **Go to**: [https://github.com/davebirr/Fabrikam-Project](https://github.com/davebirr/Fabrikam-Project)

2. **Click the "Fork" button** in the top-right corner

3. **Configure your fork**:
   - Owner: [Your GitHub username]
   - Repository name: `Fabrikam-Project` (keep default)
   - Description: Keep the original description
   - ✅ Copy the main branch only

4. **Click "Create fork"**

### 3.2 Your Fork is Ready!

You now have your own copy at: `https://github.com/[your-username]/Fabrikam-Project`

### 3.3 🔄 Important: Fork Management & Updates

**Understanding Forks:**
- Your fork is a **complete copy** of the original repository
- You can make changes, deploy, and customize without affecting the original
- **This same process** can be used to update your fork when new features are added to the original

**📋 Future Updates Process:**
1. Visit your fork on GitHub
2. Click **"Sync fork"** button (appears when updates are available)
3. Choose **"Update branch"** to pull latest changes

**⚠️ CRITICAL WARNING - Never Discard Commits:**

> **🚨 NEVER click "Discard commits" when syncing your fork!**
> 
> **Why?** When you deploy to Azure, the system creates custom workflow files (`.github/workflows/`) specific to your deployment. Discarding commits will **delete these files** and break your CI/CD pipeline.
> 
> **Always choose "Update branch"** instead, which safely merges new changes while preserving your custom workflows.

**✅ Safe Update Process:**
- ✅ Click "Sync fork" → "Update branch" (preserves your workflow files)
- ❌ Never click "Discard commits" (deletes your deployment configuration)

---

## ☁️ Step 4: Deploy to Azure

**🎯 Goal**: Deploy the Fabrikam application to your Azure subscription with proper resource naming.

### 4.1 Verify Your Pre-Created Resources

**✅ Good news!** Your Azure resources have already been created during the COE provisioning process.

1. **Open Azure Portal**: Go to [portal.azure.com](https://portal.azure.com)

2. **Verify your resource group exists**: 
   - Search for `rg-fabrikam-coe-[your-username]`
   - For example: `rg-fabrikam-coe-imatest`
   - You should see it in your resource groups list

3. **Get your User Object ID** (required for Key Vault permissions setup):
   - Open **Cloud Shell**: Click the terminal icon (`>_`) in the top toolbar
   - Choose **PowerShell** when prompted (recommended for consistency, if you want Bash it's fine)
   - Choose **No storage account required** (You can create one if you want, this is faster)
   - Select **FabrikamAI Subscription** and press **Apply** 
   - **Run this command** once the shell starts:
     ```powershell
     az ad signed-in-user show --query id -o tsv
     ```
   - **Copy the result** - you'll need this User Object ID for deployment

### 4.2 Deploy Using ARM Template

**🎯 Goal**: Deploy the Fabrikam application with one click.

**Click the button below to deploy directly from the main repository:**

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.modular.json)

**Fill in the deployment form**:
- **Subscription**: Your Azure subscription (should be pre-selected)
- **Resource Group**: `rg-fabrikam-coe-[your-username]` (your existing resource group)
- **Resource Prefix**: Leave default `fabrikam` (optional - only affects storage/key vault names)
- **User Object ID**: The value from Step 4.1
- **Authentication Mode**: Choose `Disabled` (recommended for COE workshops)

**💡 Note**: App Services will be named `fabrikam-api-development-[suffix]` and `fabrikam-mcp-development-[suffix]` regardless of the Resource Prefix setting.

**Click "Review + Create"** then **"Create"**

**Wait for deployment** (typically 10-15 minutes)

**✅ Azure permissions for successful deployment:**
- 📖 **Reader** - Subscription level (can see all resources)
- 🔐 **Contributor** - Resource Group level (can create/modify resources)
- 🔑 **User Access Administrator** - Resource Group level (can assign roles to Key Vault)

**💡 Pro Tip**: Bookmark your deployment URL for future use!

---

## 🔄 Step 5: Set Up CI/CD Pipeline

### 📚 Understanding CI/CD Pipelines

**What is CI/CD?**
- **CI (Continuous Integration)**: Automatically builds and tests your code whenever changes are pushed to GitHub
- **CD (Continuous Deployment)**: Automatically deploys your tested code to Azure when builds pass

**Why use CI/CD?**
- 🔍 **Quality Assurance**: Catch bugs before they reach production
- 🚀 **Faster Deployments**: No manual deployment steps
- 📈 **Reliability**: Consistent, repeatable deployment process
- 👥 **Team Collaboration**: Everyone's changes are automatically tested and deployed

### 🗂️ GitHub Workflow Files in Your Repository

Your Fabrikam project will have **4 GitHub workflow files** in `.github/workflows/`:

| Workflow File | Purpose | When It Runs |
|---------------|---------|--------------|
| **`testing.yml`** | 🧪 **Quality Gate** - Runs unit tests and validates code quality | Every push and pull request |
| **`authentication-validation.yml`** | 🔐 **Security Check** - Validates authentication configurations | Pull requests only (or manual trigger) |
| **`main_fabrikam-api-development-[suffix].yml`** | 🌐 **API Deployment** - Deploys API to Azure App Service | When code changes and tests pass |
| **`main_fabrikam-mcp-development-[suffix].yml`** | 🤖 **MCP Deployment** - Deploys MCP server to Azure App Service | When code changes and tests pass |

**How they work together:**
1. 📤 You push code to GitHub
2. 🧪 Testing workflow runs first (quality gate)
3. 🔐 Authentication validation ensures security (runs on pull requests to validate code before merging)
4. ✅ If tests pass, deployment workflows run automatically
5. 🚀 Your apps are updated in Azure

**💡 Note**: Since COE workshops typically use direct pushes to main (not pull requests), you may not see the authentication-validation workflow run automatically. This is by design - it's meant to validate security before code reaches production. You can trigger it manually from the GitHub Actions tab if needed.

---

**🎯 Goal**: Configure automatic deployment from your GitHub repository to Azure App Services using Azure Portal's Deployment Center, then fix the generated workflows for monorepo compatibility.

> **💡 Alternative Approach**: For advanced users who prefer full control over their CI/CD pipeline, see the [COE Advanced Setup Guide](COE-ADVANCED-SETUP-GUIDE.md) which covers manual workflow creation with optimized monorepo support and path-based triggering.

### 5.1 Configure API App Service Deployment

1. **In Azure Portal**, navigate to your resource group: `rg-fabrikam-coe-[your-username]`

2. **Click on your API App Service**: Look for the app service with "api" in the name (e.g., `fabrikam-api-development-bb7fsc`)

3. **Open Deployment Center**:
   - In the left navigation, click **Deployment** > **Deployment Center**

4. **Configure GitHub Integration**:
   - **Source**: Select **GitHub**
   - **Authorize**: Click **Authorize** and sign in to **AzureAppService** when prompted
   - **Organization**: Select your GitHub username
   - **Repository**: Select your fork of **Fabrikam-Project**
   - **Branch**: Select **main**

5. **Workflow Configuration**:
   - **Workflow option**: Leave default **"Add a workflow"** selected
   - **Authentication type**: Leave default
   - **Runtime stack**: Leave default (.NET)
   - **Version**: Leave default

6. **Save**: Click **Save** at the top of the Settings pane

7. **Wait for initial deployment**: This will automatically trigger a deployment from your main branch

### 5.2 Configure MCP App Service Deployment

**Repeat the same process for the MCP App Service:**

1. **Go back to your resource group**: `rg-fabrikam-coe-[your-username]`

2. **Click on your MCP App Service**: Look for the app service with "mcp" in the name (e.g., `fabrikam-mcp-development-bb7fsc`)

3. **Repeat steps 3-7** from the API configuration above:
   - **Source**: GitHub
   - **Authorize**: AzureAppService (if not already authorized)
   - **Organization**: Your GitHub username
   - **Repository**: Your Fabrikam-Project fork
   - **Branch**: main
   - **Save** the configuration

### 5.3 Fix Generated Workflows for Monorepo Compatibility

**⚠️ IMPORTANT**: Azure Portal generates workflows that need modification to work with our monorepo structure. I'm still working on automating / eliminating this step for these workshops so we have super simple CI/CD setup with no customization required.

1. **Go to your GitHub repository** and navigate to **Actions** tab

2. **Wait for the initial deployments to complete** (they will likely fail - this is expected)

3. **Edit the API workflow**:
   - Go to `.github/workflows/` in your repository
   - Find the file that starts with your API app service name (e.g., `main_fabrikam-api-development-bb7fsc.yml`)
   - Click **Edit** (pencil icon)

4. **Update the API workflow** by modifying the `dotnet publish` line under   `jobs:`:
   ```yaml
   # Change this line:
   - name: dotnet publish
     run: dotnet publish -c Release -o ${{env.DOTNET_ROOT}}/myapp

   # To this:
   - name: dotnet publish
     run: dotnet publish FabrikamApi/src/FabrikamApi.csproj -c Release -o ${{env.DOTNET_ROOT}}/myapp
   ```

6. **Commit the API workflow fix**: 
   - Click **"Commit changes..."** button
   - Commit message: "Fix API workflow for monorepo"
   - Click **"Commit changes"**

#### Fix MCP Deployment Workflow

7. **Edit the MCP workflow**:
   - **Go back to `.github/workflows/`** (you'll be redirected to file list after commit)
   - Find the file that starts with your MCP app service name (e.g., `main_fabrikam-mcp-development-bb7fsc.yml`)
   - Click the filename → Click **Edit** (pencil icon)

8. **Update the MCP workflow** by modifying the `dotnet publish` line:
   ```yaml
   # Change this line:
   - name: dotnet publish
     run: dotnet publish -c Release -o ${{env.DOTNET_ROOT}}/myapp

   # To this:
   - name: dotnet publish
     run: dotnet publish FabrikamMcp/src/FabrikamMcp.csproj -c Release -o ${{env.DOTNET_ROOT}}/myapp
   ```

9. **Commit the MCP workflow fix**: 
   - Click **"Commit changes..."** button  
   - Commit message: "Fix MCP workflow for monorepo"
   - Click **"Commit changes"**

**💡 Note**: You must commit each workflow file separately - GitHub's web interface doesn't allow editing multiple files in one commit.

### 5.4 Verify CI/CD Pipeline Setup

1. **Check GitHub Actions**:
   - Go to your forked repository on GitHub
   - Click the **Actions** tab
   - You should see workflow runs for both API and MCP deployments
   - **Note**: Your fork starts with 2 core workflows (testing + security), and Azure Portal adds 2 deployment workflows

2. **Test Automatic Deployment (Optional)**:
   - After fixing the workflows, make a small change to trigger the pipeline:
     - Edit `README.md` in your repository
     - Add a line: `<!-- Updated by [Your Name] for COE demo -->`
     - Commit and push the change
   - Watch the Actions tab for new workflow runs

3. **Monitor Deployment Progress**:
   - In Azure Portal, go to each App Service
   - Check **Deployment** > **Deployment Center** 
   - You should see deployment status and history

**✅ Success Criteria**: 
- Both App Services show "Success" in Deployment Center
- GitHub Actions show 4 workflows total (2 core + 2 deployment)
- All workflow runs show green checkmarks
- Making code changes triggers automatic deployments
- All workflow runs show green checkmarks
- Making code changes triggers automatic deployments

---

## 🧪 Step 6: Test Your Deployment

### 5.1 Verify Azure Resources

1. **In Azure Portal**, navigate to your resource group
2. **Confirm these resources exist**:
   - App Service (API)
   - Container App (MCP Server)
   - Key Vault
   - Application Insights
   - Container Registry

### 5.2 Test the API ✅

1. **Find your API URL**:
   - Go to your App Service in Azure Portal
   - Copy the "Default domain" URL
   - It should look like: `https://fabrikam-api-development-[suffix].azurewebsites.net`

2. **Test the API endpoints**:
   - **Swagger UI** (✅ Works): Open `https://[your-api-url]/swagger`
   - **API Info endpoint** (✅ Works): `https://[your-api-url]/api/info`
     - Should return JSON with application information
     - Example: `{"applicationName":"Fabrikam Modular Homes API","version":"1.1.0"...}`

> **💡 Note**: If browser testing fails, try using a different browser profile or incognito/private mode. The endpoints work correctly when tested with tools like curl or Postman.

### 5.3 Test Authentication (Optional - BearerToken mode only)

> **⚠️ Note**: Authentication testing may not work reliably in all browsers due to CORS and cookie policies. This is expected for workshop/demo environments.

If your deployment uses BearerToken authentication mode:

1. **Get demo credentials**:
   - Use the `/api/auth/demo-credentials` endpoint in Swagger
   - Copy the provided test credentials

2. **Login and test** (Optional):
   - Use `/api/auth/login` endpoint with demo credentials
   - Copy the JWT token from the response
   - Use "Authorize" button in Swagger to set Bearer token
   - Try authenticated endpoints like `/api/customers`

**Expected Results:**
- ✅ Swagger UI loads and displays endpoints
- ✅ `/api/info` returns application data
- ⚠️ Authentication may fail in browser (expected for workshop setup)

---

## 🤖 Step 7: Set Up Copilot Studio Integration

If you want to test the AI capabilities:

1. **Follow the Copilot Studio guide**: [Copilot-Studio-Disabled-Setup-Guide.md](./Copilot-Studio-Disabled-Setup-Guide.md)

2. **Use your deployed MCP endpoint**:
   - Find your Container App URL in Azure Portal
   - Use it in the Copilot Studio configuration

---

## ✅ Step 8: Verification Checklist

Confirm everything is working:

- [ ] ✅ GitHub account created and configured
- [ ] ✅ Fabrikam project forked successfully
- [ ] ✅ Azure resources deployed without errors
- [ ] ✅ CI/CD pipeline configured and working
- [ ] ✅ API accessible via Swagger interface
- [ ] ✅ `/api/info` endpoint returns application data
- [ ] ⚠️ Authentication working (optional - may fail in browser)
- [ ] ✅ MCP server accessible (if testing Copilot Studio)

**🎉 Success Criteria**: Swagger UI loads and `/api/info` returns valid JSON data. Authentication issues are expected in workshop environments and don't prevent success.

---

## 🆘 Troubleshooting

### Common Issues and Solutions

**GitHub Fork Issues:**
- **Problem**: Can't fork the repository
- **Solution**: Ensure you're logged into GitHub and have verified your email

**Azure Deployment Fails:**
- **Problem**: ARM template deployment errors
- **Solution**: Check that you have Contributor permissions and the resource group exists

**CI/CD Pipeline Fails:**
- **Problem**: GitHub Actions workflow errors
- **Solution**: Verify Azure credentials are correctly configured in GitHub secrets

**API Not Accessible:**
- **Problem**: Can't reach the Swagger interface
- **Solution**: Wait 5-10 minutes after deployment for services to fully start

**Authentication Issues:**
- **Problem**: JWT tokens not working
- **Solution**: Ensure you selected "BearerToken" mode during deployment

### Getting Help

**During the COE Session:**
- Ask Dave Birr for assistance
- Use the chat for questions
- Reference this guide for step-by-step instructions

**After the Session:**
- Check the [GitHub Issues](https://github.com/davebirr/Fabrikam-Project/issues) for known problems
- Review the [documentation index](../DOCUMENTATION-INDEX.md) for additional guides

---

## 🎯 Next Steps After Setup

Once your environment is ready:

1. **Explore the API**: Test different endpoints and authentication modes
2. **Customize the project**: Modify code and see CI/CD deploy changes
3. **Try Copilot Studio**: Set up AI integration with your deployed services
4. **Scale and optimize**: Experiment with Azure scaling and monitoring features

---

## 📚 Additional Resources

- **Project Documentation**: [Full documentation index](../DOCUMENTATION-INDEX.md)
- **Azure Architecture**: [Architecture guides](../architecture/)
- **Authentication Deep Dive**: [Authentication implementation guide](../development/AUTHENTICATION-LESSONS-LEARNED.md)
- **Deployment Options**: [Deployment documentation](../deployment/)

---

**🎉 Congratulations! You now have a complete Fabrikam deployment with automated CI/CD in your own Azure environment.**

*This project demonstrates modern .NET development, Azure deployment, authentication patterns, and AI integration - perfect for learning and experimentation in your COE environment.*
