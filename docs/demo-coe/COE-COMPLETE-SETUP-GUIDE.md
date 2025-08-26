# 🚀 COE Complete Setup Guide - From GitHub Account to Production Deployment

**Welcome to the COE Team! This guide will take you from zero to a fully deployed Fabrikam project with CI/CD in your own Azure environment.**

## 📋 Overview

This guide covers:
- ✅ Creating a GitHub account (if needed)
- ✅ Forking the Fabrikam project
- ✅ Deploying to your Azure subscription
- ✅ Setting up automated CI/CD pipelines
- ✅ Configuring authentication and testing

**⏱️ Estimated time: 30-45 minutes**

---

## 🔧 Prerequisites

Before starting, ensure you have:
- [ ] Access to an Azure subscription (with Contributor or Owner permissions)
- [ ] Basic familiarity with Azure Portal
- [ ] Web browser (Chrome, Edge, or Firefox recommended)

---

## 📝 Step 1: Create GitHub Account (Skip if you already have one)

### 1.1 Sign Up for GitHub

1. **Go to GitHub**: Navigate to [github.com](https://github.com)

2. **Click "Sign up"** in the top-right corner

3. **Enter your details**:
   ```
   Email: [use your work or personal email]
   Password: [create a strong password]
   Username: [choose a unique username - consider using your name/initials]
   ```

4. **Verify your email**: GitHub will send a verification email - click the link to confirm

5. **Choose your plan**: Select "Free" (sufficient for this project)

### 1.2 Configure Your Profile

1. **Upload a profile photo** (optional but recommended)
2. **Add your name** in the profile settings
3. **Consider enabling two-factor authentication** for security

---

## 🍴 Step 2: Fork the Fabrikam Project

### 2.1 Navigate to the Source Repository

1. **Go to**: [https://github.com/davebirr/Fabrikam-Project](https://github.com/davebirr/Fabrikam-Project)

2. **Click the "Fork" button** in the top-right corner

3. **Configure your fork**:
   - Owner: [Your GitHub username]
   - Repository name: `Fabrikam-Project` (keep default)
   - Description: Keep the original description
   - ✅ Copy the main branch only

4. **Click "Create fork"**

### 2.2 Your Fork is Ready!

You now have your own copy at: `https://github.com/[your-username]/Fabrikam-Project`

---

## ☁️ Step 3: Deploy to Azure

### 3.1 Prepare Your Azure Environment

1. **Open Azure Portal**: Go to [portal.azure.com](https://portal.azure.com)

2. **Open Cloud Shell**: Click the terminal icon (`>_`) in the top toolbar

3. **Run the setup script** (copy and paste this entire block):

```powershell
# Generate 6-character lowercase suffix for unique naming
$suffix = -join ((97..122) | Get-Random -Count 6 | ForEach-Object {[char]$_})

# Create resource group
$resourceGroupName = "rg-fabrikam-coe-$suffix"
az group create --name $resourceGroupName --location "East US 2"

# Get your user object ID (needed for Key Vault permissions)
$userObjectId = az ad signed-in-user show --query id -o tsv

# Display the values for deployment
$message = @"

===============================================
✅ SETUP COMPLETE - Copy these values:
===============================================

Resource Group: $resourceGroupName
User Object ID: $userObjectId

Keep these values - you'll need them in the next step!
===============================================
"@

Write-Host $message -ForegroundColor Green
```

### 3.2 Deploy Using ARM Template

1. **Update the deployment URL**: Replace `davebirr` with your GitHub username in this URL:
   
   ```
   https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2F[YOUR-USERNAME]%2FFabrikam-Project%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.modular.json
   ```

2. **Click your personalized deploy button** (after updating the URL above)

3. **Fill in the deployment form**:
   - **Subscription**: Choose your Azure subscription
   - **Resource Group**: Use the value from Step 3.1
   - **Resource Prefix**: `fabrikam-coe-[your-initials]`
   - **User Object ID**: Use the value from Step 3.1
   - **Authentication Mode**: Choose one:
     - `Disabled` - For testing and demos (recommended for COE)
     - `BearerToken` - For JWT authentication
     - `EntraExternalId` - For Azure AD integration (advanced)

4. **Click "Review + Create"** then **"Create"**

5. **Wait for deployment** (typically 10-15 minutes)

---

## 🔄 Step 4: Set Up CI/CD Pipeline

### 4.1 Enable GitHub Actions

1. **Go to your forked repository** on GitHub

2. **Navigate to "Actions" tab**

3. **Enable workflows** if prompted

### 4.2 Configure Azure Service Principal

1. **In Azure Cloud Shell**, run this script (replace `[YOUR-SUBSCRIPTION-ID]` with your actual subscription ID):

```bash
# Get your subscription ID
az account show --query id -o tsv

# Create service principal for GitHub Actions
az ad sp create-for-rbac --name "GitHub-Actions-Fabrikam-COE" \
  --role contributor \
  --scopes /subscriptions/[YOUR-SUBSCRIPTION-ID] \
  --sdk-auth
```

2. **Copy the entire JSON output** - you'll need it next

### 4.3 Add GitHub Secrets

1. **In your GitHub repository**, go to **Settings** > **Secrets and variables** > **Actions**

2. **Add these repository secrets**:

   | Secret Name | Value |
   |-------------|-------|
   | `AZURE_CREDENTIALS` | The JSON output from the service principal creation |
   | `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID |
   | `AZURE_RESOURCE_GROUP` | The resource group name from Step 3.1 |

### 4.4 Test the CI/CD Pipeline

1. **Make a small change** to trigger the pipeline:
   - Edit `README.md` in your repository
   - Add a line like: `<!-- Updated by [Your Name] for COE demo -->`
   - Commit the change

2. **Check the Actions tab** to see your pipeline running

3. **Verify deployment** by checking your Azure resource group

---

## 🧪 Step 5: Test Your Deployment

### 5.1 Verify Azure Resources

1. **In Azure Portal**, navigate to your resource group
2. **Confirm these resources exist**:
   - App Service (API)
   - Container App (MCP Server)
   - Key Vault
   - Application Insights
   - Container Registry

### 5.2 Test the API

1. **Find your API URL**:
   - Go to your App Service in Azure Portal
   - Copy the "Default domain" URL
   - It should look like: `https://fabrikam-coe-[suffix].azurewebsites.net`

2. **Test the API**:
   - Open: `https://[your-api-url]/swagger`
   - Try the `/api/info` endpoint
   - You should see application information

### 5.3 Test Authentication (if using BearerToken mode)

1. **Get demo credentials**:
   - Use the `/api/auth/demo-credentials` endpoint in Swagger
   - Copy the provided test credentials

2. **Login and test**:
   - Use `/api/auth/login` endpoint with demo credentials
   - Copy the JWT token from the response
   - Use "Authorize" button in Swagger to set Bearer token
   - Try authenticated endpoints like `/api/customers`

---

## 🤖 Step 6: Set Up Copilot Studio Integration (Optional)

If you want to test the AI capabilities:

1. **Follow the Copilot Studio guide**: [Copilot-Studio-Disabled-Setup-Guide.md](./Copilot-Studio-Disabled-Setup-Guide.md)

2. **Use your deployed MCP endpoint**:
   - Find your Container App URL in Azure Portal
   - Use it in the Copilot Studio configuration

---

## ✅ Step 7: Verification Checklist

Confirm everything is working:

- [ ] ✅ GitHub account created and configured
- [ ] ✅ Fabrikam project forked successfully
- [ ] ✅ Azure resources deployed without errors
- [ ] ✅ CI/CD pipeline configured and working
- [ ] ✅ API accessible via Swagger interface
- [ ] ✅ Authentication working (if configured)
- [ ] ✅ MCP server accessible (if testing Copilot Studio)

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
