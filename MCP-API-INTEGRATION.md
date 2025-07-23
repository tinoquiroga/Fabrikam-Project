# FabrikamMcp and FabrikamApi Integration Guide

This guide explains how to configure the FabrikamMcp server to communicate with the deployed FabrikamApi after both services are deployed to Azure.

## Overview

The FabrikamMcp server is designed to consume REST APIs from the FabrikamApi service. The MCP tools (Sales, Inventory, and Customer Service) make HTTP requests to the FabrikamApi endpoints to provide real-time business data and operations.

## Configuration

### Environment Variable Configuration

The MCP server uses the `FabrikamApi__BaseUrl` environment variable (note the double underscore, which is the .NET configuration convention for nested configuration sections) to determine where to connect to the API service.

### Deployment Sequence

**Option 1: Deploy FabrikamApi First (Recommended)**

1. **Deploy FabrikamApi**:
   ```bash
   cd FabrikamApi
   azd auth login
   azd init
   azd up
   ```

2. **Get the API URL** from the deployment output:
   ```bash
   azd show --output json | findstr "API_URI"
   ```

3. **Deploy FabrikamMcp with the API URL**:
   ```bash
   cd ../FabrikamMcp
   azd env set FABRIKAM_API_BASE_URL "https://your-fabrikam-api.azurewebsites.net"
   azd up
   ```

**Option 2: Deploy Both Separately**

1. Deploy both services independently
2. Update the MCP environment variable after deployment:
   ```bash
   cd FabrikamMcp
   azd env set FABRIKAM_API_BASE_URL "https://your-fabrikam-api.azurewebsites.net"
   azd deploy
   ```

### Manual Azure Portal Configuration

If you need to manually configure the environment variable in the Azure Portal:

1. Navigate to your MCP App Service in the Azure Portal
2. Go to **Configuration** → **Application Settings**
3. Add or update the setting:
   - **Name**: `FabrikamApi__BaseUrl`
   - **Value**: `https://your-fabrikam-api.azurewebsites.net`
4. Click **Save** and restart the application

## Configuration Details

### MCP Server Configuration

The MCP server reads the API base URL from configuration in this order:
1. Environment variable: `FabrikamApi__BaseUrl`
2. Configuration file: `appsettings.json` → `FabrikamApi.BaseUrl`
3. Default fallback: `https://localhost:7297`

### Development vs Production

- **Development** (`appsettings.Development.json`): Points to `http://localhost:5000`
- **Production** (Environment Variable): Points to the deployed Azure App Service URL

### Network Requirements

- Both services should be deployed in the same Azure region for optimal performance
- The MCP server makes outbound HTTPS requests to the API service
- No special network configuration is required as both are public App Services
- Consider using Azure Private Endpoints for production environments with sensitive data

## Validation

### Test the Connection

1. **Check MCP Status**:
   ```bash
   curl https://your-mcp-server.azurewebsites.net/status
   ```

2. **Test API Connectivity** from the MCP server:
   The status endpoint will show the configured API base URL and connection status.

3. **Test MCP Tools**:
   Use an MCP client to test the business tools:
   - `get_sales_analytics` - Tests Sales API connectivity
   - `search_inventory` - Tests Inventory API connectivity  
   - `get_support_tickets` - Tests Customer Service API connectivity

### Troubleshooting

**Common Issues:**

1. **Configuration Format**: Ensure you use `FabrikamApi__BaseUrl` (double underscore) in environment variables
2. **HTTPS**: Production API URLs should use HTTPS
3. **CORS**: Both services have CORS enabled for cross-origin requests
4. **Timeout**: Default HTTP client timeout is 100 seconds

**Logs and Diagnostics:**

- Check Application Insights for both services
- Review App Service logs in the Azure Portal
- Use the `/status` endpoint to verify configuration

## Security Considerations

### Managed Identity (Future Enhancement)

For production deployments, consider using Managed Identity for service-to-service authentication:

1. Enable Managed Identity on the MCP App Service
2. Configure the API to accept Managed Identity tokens
3. Update the MCP HTTP client to use Managed Identity authentication

### Network Security

- Consider Azure Private Link for service-to-service communication
- Use Azure Application Gateway for additional security layers
- Implement proper API key management if required

## Example URLs

- **FabrikamApi**: `https://fabrikam-api-dev.azurewebsites.net`
- **FabrikamMcp**: `https://fabrikam-mcp-dev.azurewebsites.net`

## Related Files

- **MCP Configuration**: `FabrikamMcp/src/appsettings.json`
- **MCP Infrastructure**: `FabrikamMcp/infra/resources.bicep`
- **MCP Tools**: `FabrikamMcp/src/Tools/*.cs`
- **API Infrastructure**: `FabrikamApi/infra/resources.bicep`
