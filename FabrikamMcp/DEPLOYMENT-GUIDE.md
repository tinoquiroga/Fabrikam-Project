# Fabrikam MCP Server - Azure Deployment Guide

This project contains a Model Context Protocol (MCP) server built with .NET 9 and designed for easy deployment to Azure App Service.

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) or [Azure PowerShell](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)
- [Azure Developer CLI (azd)](https://docs.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Azure subscription

## Quick Deployment

### Option 1: Using Azure Developer CLI (Recommended)

1. **Clone and navigate to the project:**
   ```bash
   git clone <your-repo-url>
   cd Fabrikam-Project/FabrikamMcp
   ```

2. **Login to Azure:**
   ```bash
   azd auth login
   ```

3. **Initialize and deploy:**
   ```bash
   azd init
   azd up
   ```

   The `azd up` command will:
   - Provision Azure resources (App Service, Application Insights, Log Analytics)
   - Build and deploy your application
   - Configure all necessary settings

4. **Access your application:**
   After deployment completes, you'll see the URL in the output. The MCP server will be available at:
   ```
   https://app-<unique-id>.azurewebsites.net
   ```

### Option 2: Manual Deployment

1. **Set environment variables:**
   ```bash
   export AZURE_ENV_NAME="your-unique-env-name"
   export AZURE_LOCATION="eastus"  # or your preferred region
   ```

2. **Deploy infrastructure:**
   ```bash
   az deployment sub create \
     --location $AZURE_LOCATION \
     --template-file infra/main.bicep \
     --parameters infra/main.parameters.json
   ```

3. **Deploy application:**
   ```bash
   # Build the application
   dotnet publish src/FabrikamMcp.csproj -c Release -o ./publish
   
   # Deploy to App Service
   az webapp deploy \
     --resource-group rg-$AZURE_ENV_NAME \
     --name app-<resource-token> \
     --src-path ./publish
   ```

## Infrastructure Overview

The deployment creates the following Azure resources:

### Core Resources
- **App Service Plan** (B1 SKU by default) - Hosts the MCP server
- **App Service** - Runs the .NET 9 MCP server application
- **User-Assigned Managed Identity** - For secure access to Azure services

### Monitoring & Logging
- **Log Analytics Workspace** - Centralized logging
- **Application Insights** - Application performance monitoring
- **Diagnostic Settings** - Routes logs to Log Analytics

### Security Features
- **HTTPS Only** - All traffic encrypted in transit
- **TLS 1.2 minimum** - Modern encryption standards
- **Managed Identity** - Secure service authentication
- **CORS enabled** - Cross-origin support for web clients

## Configuration

### App Service Settings
The deployment automatically configures:
- `.NET 9 runtime`
- Application Insights integration
- HTTP/2.0 support
- Always On (for non-free tiers)
- Build during deployment

### Custom Configuration
To customize the deployment:

1. **Change App Service Plan SKU:**
   ```bash
   azd deploy --set appServicePlanSku=S1
   ```

2. **Different region:**
   ```bash
   azd deploy --set location=westus2
   ```

## Monitoring

### Application Insights
- Navigate to the Application Insights resource in Azure portal
- View real-time metrics, traces, and logs
- Set up alerts for failures or performance issues

### Log Analytics
- Query application logs using KQL
- Create custom dashboards
- Monitor MCP server health and usage

## Troubleshooting

### Common Issues

1. **Deployment fails with "resource already exists":**
   ```bash
   azd down  # Remove existing resources
   azd up    # Redeploy
   ```

2. **Application not starting:**
   - Check Application Insights for startup errors
   - Verify .NET 9 runtime is configured
   - Review App Service logs in Azure portal

3. **CORS issues:**
   - The template enables CORS for all origins (`*`)
   - Customize CORS settings in `infra/resources.bicep` if needed

### Getting Help
- View deployment logs: `azd show`
- Check application logs in Azure portal
- Monitor with Application Insights

## Development

### Local Development
```bash
cd src
dotnet run
```

The MCP server will be available at `http://localhost:5000` (or the configured port).

### Project Structure
- `src/` - .NET 9 MCP server application
- `infra/` - Bicep infrastructure templates
- `azure.yaml` - Azure Developer CLI configuration

## Security Considerations

- The deployment uses User-Assigned Managed Identity for secure service authentication
- HTTPS is enforced for all traffic
- Secrets should be stored in Azure Key Vault (not included in base template)
- Consider network restrictions for production deployments

## Cost Optimization

- Default deployment uses B1 App Service Plan (~$13/month)
- Free tier (F1) available but has limitations (no Always On, limited compute)
- Application Insights charges based on data ingestion
- Consider scaling down non-production environments

## Next Steps

1. **Customize the MCP server** - Add your business logic and tools
2. **Set up CI/CD** - Configure GitHub Actions or Azure DevOps
3. **Add authentication** - Implement proper authentication for production use
4. **Monitor and scale** - Set up alerts and auto-scaling rules

For more information about MCP servers, visit the [Model Context Protocol documentation](https://modelcontextprotocol.io/).
