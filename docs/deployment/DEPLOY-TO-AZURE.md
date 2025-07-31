# 🚀 Deploy to Azure - Three Authentication Modes

## 🎯 Choose Your Authentication Mode

The Fabrikam Platform supports three authentication modes to fit different use cases:

### 🔓 **Disabled Mode** - Quick Demos
- **Best for**: POCs, demos, rapid testing
- **Security**: GUID-based user tracking only
- **Deploy**: [![Deploy Disabled Mode](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Ffeature%2Fphase-1-authentication%2Fdeployment%2FAzureDeploymentTemplate.modular.json)

### 🔐 **BearerToken Mode** - JWT Authentication  
- **Best for**: Production APIs, secure demos
- **Security**: JWT tokens with Key Vault secrets
- **Deploy**: [![Deploy JWT Mode](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Ffeature%2Fphase-1-authentication%2Fdeployment%2FAzureDeploymentTemplate.modular.json)

### 🏢 **EntraExternalId Mode** - Enterprise OAuth
- **Best for**: Enterprise integration, SSO scenarios  
- **Security**: OAuth 2.0 with Microsoft Entra External ID
- **Deploy**: [![Deploy Entra Mode](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FFabrikam-Project%2Ffeature%2Fphase-1-authentication%2Fdeployment%2FAzureDeploymentTemplate.modular.json)

## 📋 Authentication Mode Comparison

| Feature | Disabled | BearerToken | EntraExternalId |
|---------|----------|-------------|-----------------|
| **Setup Time** | ⚡ Instant | 🔧 5 minutes | 🏢 15-30 minutes |
| **Security Level** | 🔓 Basic tracking | 🔐 JWT tokens | 🏢 Enterprise OAuth |
| **User Management** | 📝 GUID only | 👤 Registration required | 🏢 Azure AD integration |
| **Best For** | Demos, POCs | Production APIs | Enterprise SSO |
| **Prerequisites** | None | None | Entra External ID tenant |

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

## � Deployment Parameters by Mode

### 🔓 Disabled Mode Parameters
| Parameter | Value | Description |
|-----------|-------|-------------|
| **authenticationMode** | `Disabled` | No authentication barriers |
| **enableUserTracking** | `true` | GUID-based tracking |
| **Database Provider** | `InMemory` or `SqlServer` | Choose based on persistence needs |

### 🔐 BearerToken Mode Parameters  
| Parameter | Value | Description |
|-----------|-------|-------------|
| **authenticationMode** | `BearerToken` | JWT token authentication |
| **enableUserTracking** | `true` | User session tracking |
| **Database Provider** | `SqlServer` recommended | For user registration storage |

### 🏢 EntraExternalId Mode Parameters
| Parameter | Value | Description |
|-----------|-------|-------------|
| **authenticationMode** | `EntraExternalId` | OAuth 2.0 with Entra External ID |
| **entraExternalIdTenant** | `yourcompany.onmicrosoft.com` | Your Entra External ID tenant domain |
| **entraExternalIdClientId** | `12345678-1234-1234-1234-123456789012` | Application client ID from Entra |
| **Database Provider** | `SqlServer` recommended | For OAuth user data storage |

## 🏢 EntraExternalId Setup Prerequisites

Before deploying with EntraExternalId mode, you need:

### 1. Create Entra External ID Tenant
```bash
# Option 1: Use existing Azure AD B2C tenant
# Option 2: Create new Entra External ID tenant in Azure Portal
# Go to: Azure Portal > Microsoft Entra ID > External Identities > Overview
```

### 2. Register Your Application
1. In your Entra tenant: **App registrations** > **New registration**
2. **Name**: `Fabrikam-API-Demo`
3. **Supported account types**: Accounts in any organizational directory and personal Microsoft accounts
4. **Redirect URI**: Leave empty (will be set after deployment)
5. **Register** and note the **Application (client) ID**

### 3. Create Client Secret  
1. In your app registration: **Certificates & secrets** > **New client secret**
2. **Description**: `Fabrikam-Demo-Secret`
3. **Expires**: 6 months (for demos)
4. **Add** and **copy the secret value immediately** (you can't see it again)

### 4. Configure After Deployment
After deploying, you'll need to update your app registration:
1. **Redirect URIs**: Add these to your app registration:
   - `https://your-api-app-name.azurewebsites.net/signin-oidc`
   - `https://your-api-app-name.azurewebsites.net/.auth/login/aad/callback`
2. **API permissions**: Configure as needed for your tenant
3. **Token configuration**: Add optional claims if required

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
1. **Open Azure Cloud Shell** in the Azure Portal (click the shell icon `>_` in the top toolbar)
2. **Create Resource Group** with unique suffix:
   ```powershell
   # Generate 4-character suffix
   $suffix = -join ((65..90) + (97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
   
   # Create resource group
   az group create --name "rg-FabrikamAiDemo-$suffix" --location "East US 2"
   ```

3. **Get your Azure User ID** for Key Vault access:
   ```powershell
   # Get your user object ID (needed for Key Vault RBAC permissions)
   $userObjectId = az ad signed-in-user show --query id -o tsv
   Write-Host "Your User Object ID: $userObjectId"
   ```

4. **Note both values** for the ARM template deployment

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
