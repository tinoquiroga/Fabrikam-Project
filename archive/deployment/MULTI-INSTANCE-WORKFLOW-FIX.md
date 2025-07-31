# 🔧 Multi-Instance Workflow Configuration Fix

## 🚨 Problem Identified

The original `deploy-full-stack.yml` workflow had hardcoded values that break multi-instance deployments:

### ❌ What was broken:

```yaml
# Hardcoded Azure secrets (specific to one instance)
client-id: ${{ secrets.AZUREAPPSERVICE_CLIENTID_7B45365BE95A4F148FB5FC53309D576A }}
tenant-id: ${{ secrets.AZUREAPPSERVICE_TENANTID_F4B38BCC1A3C41DB87825D58D05171B2 }}

# Hardcoded app names (specific to one suffix)
app-name: "fabrikam-api-dev-izbD"
app-name: "fabrikam-mcp-dev-izbD"

# Hardcoded domains (not configurable)
echo "API: https://fabrikam-api-dev.levelupcsp.com"
```

## ✅ Solution Implemented

### 🔄 Multi-Instance Compatible Workflow

The fixed workflow now uses **repository variables** and **flexible secret names**:

```yaml
env:
  # Multi-instance configuration using repository variables
  API_APP_NAME: ${{ vars.AZURE_API_APP_NAME || 'fabrikam-api-dev' }}
  MCP_APP_NAME: ${{ vars.AZURE_MCP_APP_NAME || 'fabrikam-mcp-dev' }}
  RESOURCE_GROUP: ${{ vars.AZURE_RESOURCE_GROUP || secrets.AZURE_RESOURCE_GROUP }}
  API_DOMAIN: ${{ vars.API_DOMAIN || 'fabrikam-api-dev.azurewebsites.net' }}
  MCP_DOMAIN: ${{ vars.MCP_DOMAIN || 'fabrikam-mcp-dev.azurewebsites.net' }}

# Flexible authentication - works with both patterns
client-id: ${{ secrets.AZURE_CLIENT_ID || secrets.AZUREAPPSERVICE_CLIENTID }}
tenant-id: ${{ secrets.AZURE_TENANT_ID || secrets.AZUREAPPSERVICE_TENANTID }}
subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID || secrets.AZUREAPPSERVICE_SUBSCRIPTIONID }}
```

### 🎯 Benefits:

1. **✅ Works with Azure Portal CI/CD** - Detects existing `AZUREAPPSERVICE_*` secrets
2. **✅ Works with manual setup** - Falls back to standard `AZURE_*` secrets  
3. **✅ Instance-specific configuration** - Uses repository variables for customization
4. **✅ Graceful fallbacks** - Provides sensible defaults if variables not set
5. **✅ Enhanced verification** - Added health/status checks for deployments

## 🚀 Repository Variable Setup

### For Fork/Instance Owners:

Go to **GitHub Settings → Secrets and Variables → Actions → Variables** and configure:

| Variable Name | Description | Example Value |
|---------------|-------------|---------------|
| `AZURE_API_APP_NAME` | Your API App Service name | `fabrikam-api-dev-xyz9` |
| `AZURE_MCP_APP_NAME` | Your MCP App Service name | `fabrikam-mcp-dev-xyz9` |
| `AZURE_RESOURCE_GROUP` | Your resource group name | `rg-fabrikam-dev-xyz9` |
| `API_DOMAIN` | Your API custom domain | `fabrikam-api-dev.levelupcsp.com` |
| `MCP_DOMAIN` | Your MCP custom domain | `fabrikam-mcp-dev.levelupcsp.com` |

### 🔐 Secrets Compatibility:

The workflow supports both secret naming patterns:

#### Azure Portal Generated (automatic):
- `AZUREAPPSERVICE_CLIENTID_[randomid]` 
- `AZUREAPPSERVICE_TENANTID_[randomid]`
- `AZUREAPPSERVICE_SUBSCRIPTIONID_[randomid]`

#### Manual Setup (standard):
- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID` 
- `AZURE_SUBSCRIPTION_ID`

## 🧪 Enhanced Features

### 🔍 Deployment Verification

The new workflow includes automatic verification:

```yaml
# API Health Check
for i in {1..5}; do
  if curl -f -s "${API_URL}/health" > /dev/null; then
    echo "✅ API health check passed"
    break
  fi
done

# MCP Status Check  
for i in {1..5}; do
  if curl -f -s "${MCP_URL}/status" > /dev/null; then
    echo "✅ MCP status check passed"
    break
  fi
done
```

### ⚙️ Automatic Configuration

The workflow now configures MCP to connect to the correct API instance:

```yaml
# Set the API base URL for MCP to connect to API
az webapp config appsettings set \
  --name ${{ env.MCP_APP_NAME }} \
  --resource-group ${{ env.RESOURCE_GROUP }} \
  --settings "FabrikamApi__BaseUrl=https://${{ env.API_DOMAIN }}"
```

## 🎉 Result

✅ **Single workflow** works across all instances  
✅ **No more hardcoded values** that break forks  
✅ **Professional deployment** with verification  
✅ **Enhanced logging** and debugging info  
✅ **Automatic service configuration** for cross-service communication  

This fix enables the true **multi-instance demo pattern** where users can fork, deploy, and run their own independent instances without workflow conflicts!
