# 🚀 Azure Deployment Testing Guide

This guide explains how to test your Azure deployments using the enhanced deployment configuration system.

## 📋 Quick Reference

### **Basic Commands**

```powershell
# Test production deployment (default: Bearer Token auth)
.\test.ps1 -Production -Quick -ApiOnly

# Test staging deployment (Disabled auth)  
.\test.ps1 -DeploymentName "staging" -Quick -ApiOnly

# Test specific deployment by name
.\test.ps1 -DeploymentName "feature-auth-bearer" -Quick

# Test with custom URLs (override config)
.\test.ps1 -Production -ApiBaseUrl "https://your-api.azurewebsites.net" -McpBaseUrl "https://your-mcp.azurewebsites.net"
```

### **Deployment Management**

```powershell
# List all configured deployments
.\Manage-Deployments.ps1 list

# Add a new deployment
.\Manage-Deployments.ps1 add -Name "my-deployment" -ApiUrl "https://api.azurewebsites.net" -McpUrl "https://mcp.azurewebsites.net" -AuthMode "BearerToken" -Suffix "abc123"

# Update an existing deployment
.\Manage-Deployments.ps1 update -Name "my-deployment" -Suffix "xyz789"

# Test a specific deployment
.\Manage-Deployments.ps1 test -Name "my-deployment"
```

## 🎯 Current Deployments

Based on your feature branch setup:

| Deployment | Authentication | Suffix | Alias | URLs |
|------------|---------------|--------|--------|------|
| **feature-auth-bearer** | BearerToken | `xmrbiq` | `production` | `fabrikam-*-development-xmrbiq.azurewebsites.net` |
| **feature-auth-disabled** | Disabled | `rxnmcw` | `staging` | `fabrikam-*-development-rxnmcw.azurewebsites.net` |
| **local** | BearerToken | - | `local` | `localhost:7297/5001` |

## 🔧 Configuration Files

- **`deployment-config.json`** - Main deployment configuration
- **`.env`** - Environment variables (optional, for secrets)
- **`Manage-Deployments.ps1`** - Configuration management script

## 🧪 Testing Scenarios

### **1. Test Production Deployment (Bearer Token)**
```powershell
.\test.ps1 -Production -Quick
```
- ✅ Tests JWT authentication
- ✅ Validates token-based endpoints  
- ✅ Checks user profile access

### **2. Test Staging Deployment (Disabled Auth)**
```powershell
.\test.ps1 -DeploymentName "staging" -Quick
```
- ✅ Tests open endpoints
- ✅ Validates GUID-based identification
- ✅ Confirms auth endpoints are disabled

### **3. Compare Authentication Modes**
```powershell
# Test both environments back-to-back
.\test.ps1 -Production -Quick -ApiOnly
.\test.ps1 -DeploymentName "staging" -Quick -ApiOnly
```

## 🔄 Workflow for New Deployments

1. **Deploy to Azure** (using your deployment process)
2. **Add to configuration**:
   ```powershell
   .\Manage-Deployments.ps1 add -Name "new-deployment" -ApiUrl "https://fabrikam-api-development-SUFFIX.azurewebsites.net" -McpUrl "https://fabrikam-mcp-development-SUFFIX.azurewebsites.net" -AuthMode "BearerToken" -Suffix "SUFFIX"
   ```
3. **Test the deployment**:
   ```powershell
   .\test.ps1 -DeploymentName "new-deployment" -Quick
   ```
4. **Set as production** (if needed):
   ```powershell
   .\Manage-Deployments.ps1 update -Name "new-deployment" -SetAsProduction
   ```

## 🆔 Authentication Mode Detection

The test system **automatically detects** the actual authentication mode from the API, so even if your configuration says one thing, the tests will adapt to what's actually running:

- **BearerToken detected**: Uses JWT tokens, tests authentication endpoints
- **Disabled detected**: Uses GUID identification, tests open endpoints  
- **EntraExternalId detected**: Uses Azure AD tokens (future)

## 🎉 Benefits

✅ **Dynamic URL Management** - No hardcoded URLs in test scripts  
✅ **Multiple Environment Support** - Test production, staging, and local  
✅ **Authentication-Aware** - Automatically adapts to auth mode  
✅ **Easy Deployment Updates** - Simple configuration management  
✅ **Branch-Aware Testing** - Support multiple feature branch deployments  
✅ **Alias Support** - Use friendly names like "production" and "staging"

## 🔍 Troubleshooting

### **URL Not Found**
```
❌ API Server: Not accessible at https://...
```
- Check if the deployment is running in Azure
- Verify the URL in `deployment-config.json`
- Update the configuration if the suffix changed

### **Authentication Mismatch**
```
🔍 Expected: BearerToken, Detected: Disabled
```
- The configuration is informational - tests adapt automatically
- Update config for clarity: `.\Manage-Deployments.ps1 update -Name "my-deployment" -AuthMode "Disabled"`

### **Environment Variables**
If you prefer environment variables over configuration files:
```powershell
$env:API_DOMAIN = "fabrikam-api-development-SUFFIX.azurewebsites.net"
$env:MCP_DOMAIN = "fabrikam-mcp-development-SUFFIX.azurewebsites.net"
.\test.ps1 -Production -Quick
```
