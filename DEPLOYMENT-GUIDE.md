# 🚀 Azure Deployment & CI/CD Setup Guide

This guide walks through setting up automated deployments for both FabrikamApi and FabrikamMcp from a single repository using GitHub Actions.

## 🎯 Deployment Architecture

```
Single Repository (Monorepo)
├── FabrikamApi/          ← Deployed to Azure App Service
├── FabrikamMcp/          ← Deployed to Azure App Service  
├── .github/workflows/    ← CI/CD pipelines
└── Shared resources      ← Documentation, scripts, configs
```

### Benefits of This Approach:
- ✅ **Single source of truth** for both applications
- ✅ **Coordinated deployments** with dependency management
- ✅ **Shared CI/CD configuration** and secrets
- ✅ **Simplified repository management**
- ✅ **Easy cross-service integration** and testing

---

## 🛠️ Pre-Deployment Setup

### 1. Azure Resources Setup

Create Azure resources for each environment:

```powershell
# Set variables
$environment = "dev"  # or "staging", "production"
$location = "East US"
$resourceGroup = "rg-fabrikam-$environment"

# Create resource group
az group create --name $resourceGroup --location $location

# Create App Service Plans
az appservice plan create `
  --name "plan-fabrikam-api-$environment" `
  --resource-group $resourceGroup `
  --sku B1 --is-linux

az appservice plan create `
  --name "plan-fabrikam-mcp-$environment" `
  --resource-group $resourceGroup `
  --sku B1 --is-linux

# Create Web Apps
az webapp create `
  --name "fabrikam-api-$environment" `
  --resource-group $resourceGroup `
  --plan "plan-fabrikam-api-$environment" `
  --runtime "DOTNETCORE:9.0"

az webapp create `
  --name "fabrikam-mcp-$environment" `
  --resource-group $resourceGroup `
  --plan "plan-fabrikam-mcp-$environment" `
  --runtime "DOTNETCORE:9.0"
```

### 2. Service Principal Creation

Create a service principal for GitHub Actions:

```powershell
# Create service principal with contributor access
$sp = az ad sp create-for-rbac `
  --name "sp-fabrikam-deploy" `
  --role "Contributor" `
  --scopes "/subscriptions/{your-subscription-id}" `
  --sdk-auth

# Save the JSON output - you'll need this for GitHub secrets
echo $sp
```

### 3. GitHub Repository Setup

#### Required GitHub Secrets:

Navigate to your GitHub repository → Settings → Secrets and variables → Actions:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `AZURE_CREDENTIALS` | Service principal JSON from step 2 | `{"clientId": "...", "clientSecret": "...", ...}` |
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID | `12345678-1234-1234-1234-123456789012` |
| `AZURE_RESOURCE_GROUP_NAME` | Resource group name | `rg-fabrikam-dev` |

#### Optional GitHub Variables:

| Variable Name | Description | Default Value |
|---------------|-------------|---------------|
| `AZURE_LOCATION` | Azure region for deployments | `East US` |
| `DOTNET_VERSION` | .NET version to use | `9.0.x` |

---

## 🔄 CI/CD Workflows

### Available Workflows:

#### 1. **Individual Service Deployment**
- **`deploy-api.yml`** - Deploys only FabrikamApi
- **`deploy-mcp.yml`** - Deploys only FabrikamMcp

#### 2. **Full Stack Deployment**  
- **`deploy-full-stack.yml`** - Deploys both services with coordination

### Workflow Triggers:

#### Automatic Triggers:
- **Push to main branch** - Deploys to development environment
- **Path-based triggers** - Only deploys services that changed
  - Changes to `FabrikamApi/` → Triggers API deployment
  - Changes to `FabrikamMcp/` → Triggers MCP deployment

#### Manual Triggers:
- **Workflow Dispatch** - Manual deployment with environment selection
- **Environment Selection** - Choose development, staging, or production
- **Service Selection** - Deploy API only, MCP only, or both

### Deployment Process:

```mermaid
graph TD
    A[Code Push] --> B{Changes Detected}
    B -->|FabrikamApi/*| C[Build API]
    B -->|FabrikamMcp/*| D[Build MCP]
    C --> E[Test API]
    D --> F[Test MCP]
    E --> G[Deploy API]
    F --> H[Configure MCP with API URL]
    H --> I[Deploy MCP]
    G --> J[Integration Test]
    I --> J
    J --> K[Health Checks]
```

---

## 🌍 Environment Management

### Environment Strategy:

| Environment | Branch | Purpose | Auto-Deploy |
|-------------|--------|---------|-------------|
| **Development** | `main` | Latest stable code | ✅ Yes |
| **Staging** | `release/*` | Pre-production testing | ✅ Yes |
| **Production** | `production` | Live environment | ❌ Manual only |

### Environment-Specific Configuration:

#### Development
```bash
# App Names
AZURE_API_WEBAPP_NAME=fabrikam-api-dev
AZURE_MCP_WEBAPP_NAME=fabrikam-mcp-dev

# URLs
API_URL=https://fabrikam-api-dev.azurewebsites.net
MCP_URL=https://fabrikam-mcp-dev.azurewebsites.net
```

#### Production
```bash
# App Names  
AZURE_API_WEBAPP_NAME=fabrikam-api-prod
AZURE_MCP_WEBAPP_NAME=fabrikam-mcp-prod

# URLs
API_URL=https://fabrikam-api-prod.azurewebsites.net
MCP_URL=https://fabrikam-mcp-prod.azurewebsites.net
```

---

## 🧪 Testing & Validation

### Automated Testing Pipeline:

1. **Unit Tests** - Run during build for both services
2. **Integration Tests** - Verify API-MCP communication
3. **Health Checks** - Ensure services are responding
4. **Smoke Tests** - Basic functionality validation

### Health Check Endpoints:

```bash
# API Health Check
curl https://fabrikam-api-{env}.azurewebsites.net/health

# MCP Status Check  
curl https://fabrikam-mcp-{env}.azurewebsites.net/status

# API Documentation
https://fabrikam-api-{env}.azurewebsites.net/swagger
```

### Post-Deployment Validation:

```powershell
# Test API connectivity
$apiUrl = "https://fabrikam-api-dev.azurewebsites.net"
$mcpUrl = "https://fabrikam-mcp-dev.azurewebsites.net"

# Check API health
Invoke-RestMethod "$apiUrl/health"

# Check MCP status
Invoke-RestMethod "$mcpUrl/status"

# Verify MCP can reach API
$mcpStatus = Invoke-RestMethod "$mcpUrl/status"
Write-Host "MCP connected to API: $($mcpStatus.ApiConnected)"
```

---

## 🛠️ Manual Deployment

### Using Azure Developer CLI (AZD):

```powershell
# Option 1: Deploy individually
cd FabrikamApi
azd up

cd ../FabrikamMcp
azd env set FABRIKAM_API_BASE_URL "https://fabrikam-api-dev.azurewebsites.net"
azd up

# Option 2: Use coordinated script
.\Deploy-Integrated.ps1 -EnvironmentName "dev" -Location "eastus"
```

### Using GitHub Actions Manually:

1. Go to **Actions** tab in your GitHub repository
2. Select **Deploy Full Stack** workflow
3. Click **Run workflow**
4. Choose environment and services to deploy
5. Monitor deployment progress

---

## 🔧 Configuration Management

### Service-to-Service Communication:

The MCP server needs to know the API URL. This is configured automatically during deployment:

```yaml
# In deploy-mcp.yml
- name: ⚙️ Configure App Settings
  run: |
    az webapp config appsettings set \
      --name ${{ env.AZURE_WEBAPP_NAME }} \
      --resource-group ${{ secrets.AZURE_RESOURCE_GROUP_NAME }} \
      --settings "FabrikamApi__BaseUrl=${{ steps.get-api-url.outputs.api_url }}"
```

### Environment Variables in Azure:

| Service | Setting | Purpose |
|---------|---------|---------|
| **FabrikamApi** | `ASPNETCORE_ENVIRONMENT` | Runtime environment |
| **FabrikamMcp** | `FabrikamApi__BaseUrl` | API connection string |
| **Both** | `AZURE_CLIENT_ID` | Managed identity (if used) |

---

## 🚨 Troubleshooting

### Common Issues:

#### 1. **Deployment Fails**
```bash
# Check GitHub Actions logs
# Look for specific error messages in the workflow run

# Check Azure deployment logs
az webapp log tail --name fabrikam-api-dev --resource-group rg-fabrikam-dev
```

#### 2. **MCP Can't Connect to API**
```bash
# Verify MCP configuration
az webapp config appsettings list --name fabrikam-mcp-dev --resource-group rg-fabrikam-dev

# Test API accessibility
curl https://fabrikam-api-dev.azurewebsites.net/health
```

#### 3. **GitHub Actions Permission Issues**
```bash
# Verify service principal has correct permissions
az role assignment list --assignee {service-principal-client-id}

# Check if resource group exists
az group show --name rg-fabrikam-dev
```

### Debugging Steps:

1. **Check workflow logs** in GitHub Actions
2. **Verify Azure resources** exist and are running
3. **Test health endpoints** manually
4. **Check application logs** in Azure portal
5. **Validate configuration** settings

---

## 📊 Monitoring & Observability

### Application Insights:

Both services are configured with Application Insights for monitoring:

- **Performance metrics**
- **Error tracking** 
- **Request/response logging**
- **Custom telemetry**

### Key Metrics to Monitor:

| Metric | API | MCP | Purpose |
|--------|-----|-----|---------|
| **Response Time** | ✅ | ✅ | Performance monitoring |
| **Error Rate** | ✅ | ✅ | Reliability tracking |
| **Request Volume** | ✅ | ✅ | Usage analytics |
| **Dependency Calls** | ❌ | ✅ | MCP→API communication |

### Alerts Configuration:

```powershell
# Create alert for high error rate
az monitor metrics alert create \
  --name "High Error Rate - FabrikamApi" \
  --resource-group rg-fabrikam-dev \
  --scopes "/subscriptions/{sub-id}/resourceGroups/rg-fabrikam-dev/providers/Microsoft.Web/sites/fabrikam-api-dev" \
  --condition "avg requests/failed greater than 5" \
  --description "Alert when API error rate is high"
```

---

## 🎯 Best Practices Summary

### Repository Management:
- ✅ Use monorepo for related services
- ✅ Implement path-based deployment triggers
- ✅ Maintain separate build artifacts
- ✅ Use consistent naming conventions

### Deployment Strategy:
- ✅ Deploy API before MCP (dependency order)
- ✅ Use environment-specific configurations
- ✅ Implement health checks and integration tests
- ✅ Use staging slots for zero-downtime deployments

### Security:
- ✅ Use managed identities where possible
- ✅ Store secrets in GitHub Secrets
- ✅ Implement least-privilege access
- ✅ Enable application logs and monitoring

This setup provides a robust, scalable deployment pipeline for your Fabrikam project that can grow with your needs while maintaining separation of concerns and proper CI/CD practices.
