# 🚀 Fabrikam AI Demo - One-Click Azure Deployment

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.json)

## ⚡ **Quick Start: 15-Minute Setup**

**📋 Want the fastest path to a working demo? Follow our [Deployment Checklist](DEPLOYMENT-CHECKLIST.md)**

The checklist walks you through:
- ✅ **5 min**: Deploy infrastructure (click button above)
- ✅ **5 min**: Configure automatic CI/CD via Azure Portal  
- ✅ **5 min**: Verify everything works and start developing

**For detailed technical information, continue reading below.**

---

## 🎯 What This Deploys

This ARM template creates a complete Fabrikam AI Demo environment with:

- ✅ **FabrikamApi** - REST API service for modular homes business operations
- ✅ **FabrikamMcp** - Model Context Protocol server for AI integrations  
- ✅ **Application Insights** - Monitoring and telemetry for both services
- ✅ **Log Analytics** - Centralized logging workspace
- ✅ **App Service Plans** - Hosting infrastructure for web applications

## 🏗️ Resource Naming Convention

All resources are created with a unique 4-character suffix to ensure global uniqueness:

```
Instance Suffix: [4 random characters, e.g., "k3m9"]

Resource Group: rg-fabrikamAIDemo-k3m9
API Service: fabrikam-api-dev-k3m9
MCP Service: fabrikam-mcp-dev-k3m9
App Insights: appi-api-dev-k3m9, appi-mcp-dev-k3m9
```

## 📋 Prerequisites

- **Azure Subscription** with Contributor access
- **GitHub Account** (for CI/CD setup after deployment)
- **GitHub Fork** of this repository (recommended for your own deployments)

## 🎛️ Deployment Parameters

| Parameter | Description | Default | Options |
|-----------|-------------|---------|---------|
| **baseName** | Base name for all resources | `FabrikamAIDemo` | 3-15 characters |
| **environment** | Deployment environment | `dev` | `dev`, `staging`, `prod` |
| **githubRepository** | Your GitHub repository URL | Fork URL | Your fork URL |
| **githubToken** | GitHub PAT for CI/CD | Empty | Optional for setup |
| **location** | Azure region | Resource Group location | Any Azure region |
| **skuName** | App Service pricing tier | `B1` | `F1`, `B1`, `B2`, `S1`, `S2`, `P1v2`, `P2v2` |

## 🚀 Quick Deployment Options

### Option 1: One-Click Deployment (Recommended)
Click the "Deploy to Azure" button above and fill in the parameters in the Azure portal.

### Option 2: Azure CLI Deployment
```bash
# Clone and navigate to repository
git clone https://github.com/davebirr/Fabrikam-Project.git
cd Fabrikam-Project

# Create resource group
az group create --name "rg-fabrikam-demo" --location "East US 2"

# Deploy template
az deployment group create \
  --resource-group "rg-fabrikam-demo" \
  --template-file "deployment/AzureDeploymentTemplate.json" \
  --parameters "deployment/AzureDeploymentTemplate.parameters.json"
```

### Option 3: PowerShell Deployment
```powershell
# Create resource group
New-AzResourceGroup -Name "rg-fabrikam-demo" -Location "East US 2"

# Deploy template
New-AzResourceGroupDeployment `
  -ResourceGroupName "rg-fabrikam-demo" `
  -TemplateFile "deployment/AzureDeploymentTemplate.json" `
  -TemplateParameterFile "deployment/AzureDeploymentTemplate.parameters.json"
```

## 📊 Post-Deployment Steps

After successful deployment, you'll receive these outputs:

```json
{
  "apiUrl": "https://fabrikam-api-dev-k3m9.azurewebsites.net",
  "mcpUrl": "https://fabrikam-mcp-dev-k3m9.azurewebsites.net",
  "apiHealthCheck": "https://fabrikam-api-dev-k3m9.azurewebsites.net/health",
  "mcpHealthCheck": "https://fabrikam-mcp-dev-k3m9.azurewebsites.net/status",
  "instanceSuffix": "k3m9"
}
```

### Next Steps:

1. **📝 Note Your Instance Suffix** - You'll need this for CI/CD setup
2. **🔄 Set Up GitHub Actions** - Follow the [CI/CD Setup Guide](../docs/deployment/DEPLOYMENT-GUIDE.md#github-repository-setup)
3. **🧪 Test Health Endpoints** - Verify services are running
4. **🚀 Deploy Your Code** - Use GitHub Actions to deploy your application code

## 🛠️ CI/CD Setup After Deployment

### **🎯 Recommended: Azure Portal Setup** (Like CIPP)
**The user-friendly approach that avoids CLI security restrictions:**

```
1. 🍴 Fork the repository on GitHub
2. 🔗 Use Azure Portal Deployment Center  
3. 📱 Point-and-click configuration
4. ✅ Automatic GitHub Actions setup
```

**📖 See [Portal CI/CD Setup Guide](PORTAL-CICD-SETUP.md) for step-by-step instructions**

### **⚡ Alternative: PowerShell Setup** (Advanced Users)
For users comfortable with command-line tools:

```powershell
# 1. Configure VS Code settings
.\deployment\Configure-VSCodeSettings.ps1 -ResourceGroup "your-resource-group"

# 2. Set up GitHub Actions (requires GitHub PAT)
.\deployment\Setup-GitHubActions.ps1 `
  -ResourceGroup "your-resource-group" `
  -GitHubRepository "https://github.com/YOUR-USERNAME/Fabrikam-Project" `
  -GitHubToken "ghp_your_token_here" `
  -SetupFork
```

**📖 See [CI/CD Strategy Guide](CI-CD-STRATEGY.md) for complete automation details**

### **🔄 Fork-and-Sync Pattern Benefits**
Both approaches enable the **professional demo pattern** where:
- ✅ **Demo users fork** your repository
- ✅ **Get upstream updates** automatically  
- ✅ **Maintain custom configs** in their fork
- ✅ **Deploy independently** to their Azure instance

## 🔧 Customization Options

### Environment-Specific Deployments
Deploy multiple environments by changing the `environment` parameter:

```bash
# Development
az deployment group create --parameters environment=dev

# Staging  
az deployment group create --parameters environment=staging

# Production
az deployment group create --parameters environment=prod
```

### Scaling Options
Adjust the `skuName` parameter based on your needs:

| SKU | Description | Use Case |
|-----|-------------|----------|
| `F1` | Free tier | Development/Testing |
| `B1` | Basic tier | Small workloads |
| `B2` | Basic tier | Medium workloads |
| `S1` | Standard tier | Production workloads |
| `P1v2` | Premium tier | High-performance production |

## 🔍 Resource Details

### What Gets Created:

#### **API Service (fabrikam-api-{env}-{suffix})**
- **Runtime**: .NET 9.0 on Linux
- **Features**: Application Insights, HTTPS only, Always On (non-F1)
- **Purpose**: REST API for modular homes business operations

#### **MCP Service (fabrikam-mcp-{env}-{suffix})**  
- **Runtime**: .NET 9.0 on Linux
- **Features**: Application Insights, HTTPS only, Auto-configured to API
- **Purpose**: Model Context Protocol server for AI integrations

#### **Monitoring Stack**
- **Log Analytics Workspace**: Centralized logging (30-day retention)
- **Application Insights**: Performance monitoring and telemetry
- **Managed Identity**: Secure access between services

## 🚨 Troubleshooting

### Common Issues:

#### **Deployment Fails**
- Check Azure subscription permissions (need Contributor role)
- Verify all required providers are registered
- Try different Azure region if resources unavailable

#### **Resource Names Already Taken**
- The unique suffix should prevent this, but if it occurs:
- Delete the resource group and try again
- The new deployment will generate a different suffix

#### **GitHub Integration Issues**
- Ensure you have Admin access to the GitHub repository
- Generate a GitHub Personal Access Token if needed
- Check that repository contains the correct folder structure

### **Getting Help**
- Check the [detailed deployment guide](../docs/deployment/DEPLOYMENT-GUIDE.md)
- Review Azure Portal deployment logs
- Test health endpoints after deployment
- Verify Application Insights telemetry

## 🎉 Success Validation

After deployment, validate everything is working:

```bash
# Check API health
curl https://fabrikam-api-dev-{your-suffix}.azurewebsites.net/health

# Check MCP status  
curl https://fabrikam-mcp-dev-{your-suffix}.azurewebsites.net/status

# Verify Application Insights data
# (Check Azure Portal → Application Insights → Live Metrics)
```

## � Setting Up CI/CD (Recommended Next Step)

The ARM template deploys infrastructure only. For a complete demo environment, set up GitHub Actions CI/CD:

### **Quick CI/CD Setup**
```powershell
# 1. Configure VS Code settings
.\deployment\Configure-VSCodeSettings.ps1 -ResourceGroup "your-resource-group"

# 2. Set up GitHub Actions (requires GitHub PAT)
.\deployment\Setup-GitHubActions.ps1 `
  -ResourceGroup "your-resource-group" `
  -GitHubRepository "https://github.com/YOUR-USERNAME/Fabrikam-Project" `
  -GitHubToken "ghp_your_token_here" `
  -SetupFork
```

### **Fork-and-Sync Pattern**
This setup enables the **professional demo pattern** where:
- ✅ **Demo users fork** your repository
- ✅ **Get upstream updates** automatically  
- ✅ **Maintain custom configs** in their fork
- ✅ **Deploy independently** to their Azure instance

**📖 See [CI/CD Strategy Guide](CI-CD-STRATEGY.md) for complete details**

## �📖 Related Documentation

- [Detailed Deployment Guide](../docs/deployment/DEPLOYMENT-GUIDE.md)
- [CI/CD Setup Instructions](../docs/deployment/DEPLOYMENT-GUIDE.md#ci-cd-workflows)
- [Architecture Overview](../docs/architecture/SYSTEM-ARCHITECTURE.md)
- [Development Guide](../docs/development/DEVELOPMENT-GUIDE.md)

---

**🎯 Ready to deploy? Click the "Deploy to Azure" button at the top of this README!**
