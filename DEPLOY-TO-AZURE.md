# 🚀 Deploy to Azure - Key Vault Enhanced Template

## Quick Deployment

Click the button below to deploy the enhanced Fabrikam platform with Key Vault integration:

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Ffeature%2Fphase-1-authentication%2Fdeployment%2FAzureDeploymentTemplate.json)

## 🔐 Key Features

### Enhanced Security:
- ✅ **Azure Key Vault** deployed with **RBAC authorization** (not legacy access policies)
- ✅ **JWT secrets** secured in Key Vault (never in app configuration)
- ✅ **SQL connection strings** secured for SQL Server deployments
- ✅ **Managed Identity** authentication to Key Vault
- ✅ **Auto-assigned permissions**: App Services get Key Vault Secrets User role
- ✅ **Deployment user permissions**: Deployer gets Key Vault Secrets Officer role

### Flexible Database Options:
- 🚀 **InMemory**: Quick demos, no persistence, instant startup
- 🗃️ **SQL Server**: Production-like, persistent data, full features

### Smart Resource Naming:
- 📋 **Pattern**: `rg-fabrikam-{environment}-{suffix}`
- 🔀 **Example**: `rg-FabrikamAiDemo-y32g` (matches your actual deployment pattern)
- ✅ **Benefits**: Unique isolation, easy identification

## 📋 Deployment Parameters

| Parameter | Description | Options | Recommended |
|-----------|-------------|---------|-------------|
| **Database Provider** | Database backend choice | InMemory, SqlServer | InMemory (demo), SqlServer (prod) |
| **Enable Authentication** | JWT authentication system | true, false | true |
| **Environment** | Deployment environment | dev, staging, prod | dev |
| **SKU Name** | App Service pricing tier | F1, B1, B2, S1, S2 | B1 |
| **Deployment User** | Your Azure user object ID | (auto-detected) | Required for Key Vault access |

## 🏗️ What Gets Deployed

### InMemory + Authentication (Recommended for testing):
- 🌐 **API App Service** (with authentication)
- 🤖 **MCP App Service** (Model Context Protocol)
- 🔐 **Key Vault** (with JWT secret)
- 📊 **Application Insights** (monitoring)
- 📱 **App Service Plan**

### SQL Server + Authentication (Production-like):
- 🌐 **API App Service** (with authentication)
- 🤖 **MCP App Service** (Model Context Protocol)
- 🔐 **Key Vault** (with JWT + SQL secrets)
- 🗃️ **SQL Server & Database**
- 📊 **Application Insights** (monitoring)
- 📱 **App Service Plan**

## 🔧 Recommended Resource Group Setup

### Before Deployment:
1. **Create Resource Group** with unique suffix:
   ```powershell
   # Generate 4-character suffix
   $suffix = -join ((65..90) + (97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
   
   # Create resource group
   az group create --name "rg-FabrikamAiDemo-$suffix" --location "East US 2"
   ```

2. **Get your Azure User ID** for Key Vault access:
   ```powershell
   # Get your user object ID (needed for Key Vault RBAC permissions)
   $userObjectId = az ad signed-in-user show --query id -o tsv
   Write-Host "Your User Object ID: $userObjectId"
   ```

3. **Note both values** for the ARM template deployment

### After Deployment:
1. **Test the deployed API** endpoints
2. **Verify Key Vault** contains secrets and you have access
3. **Set up CI/CD** using the auto-fix workflows

## 🔄 CI/CD Integration Testing

After deployment, test the auto-fix CI/CD functionality:

1. **Repository Variables Setup**:
   - `AZURE_RESOURCE_GROUP`: Your deployed resource group name
   - `AZURE_API_APP_NAME`: API app name from deployment output
   - `AZURE_MCP_APP_NAME`: MCP app name from deployment output

2. **Trigger Auto-Fix Workflow**:
   - Push changes to trigger detection
   - Verify workflow auto-corrects any Azure Portal-generated workflows

3. **Validate Key Vault Integration**:
   - Check API can read JWT secrets from Key Vault
   - Verify authentication endpoints work correctly

## 🔍 Testing Your Deployment

### Quick Health Check:
```bash
# API Health Check
curl https://your-api-app-name.azurewebsites.net/api/orders

# MCP Health Check  
curl https://your-mcp-app-name.azurewebsites.net/mcp/v1/info
```

### Authentication Testing:
```bash
# Register a test user
curl -X POST https://your-api-app-name.azurewebsites.net/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}'

# Login to get JWT token
curl -X POST https://your-api-app-name.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

## 🎯 Next Steps

1. ✅ **Deploy** using the button above
2. 🔧 **Configure** repository variables for CI/CD
3. 🧪 **Test** the auto-fix workflow functionality
4. 📊 **Monitor** via Application Insights
5. 🔄 **Iterate** with the enhanced CI/CD pipeline

---

**Ready to deploy with enhanced security and Key Vault integration! 🚀**
