# FabrikamApi Azure Deployment

This directory contains the infrastructure as code (IaC) templates and configuration needed to deploy the FabrikamApi .NET 9.0 Web API to Microsoft Azure using the Azure Developer CLI (azd).

## Architecture Overview

The deployment creates the following Azure resources:

- **App Service**: Hosts the .NET 9.0 Web API with Windows runtime
- **App Service Plan**: Basic tier compute infrastructure
- **Application Insights**: Application performance monitoring and telemetry
- **Log Analytics Workspace**: Centralized logging and monitoring
- **User-Assigned Managed Identity**: Secure authentication for Azure services

## Prerequisites

Before deploying, ensure you have:

1. **Azure Developer CLI (azd)** - [Install azd](https://docs.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
2. **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
3. **.NET 9.0 SDK** - [Download .NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
4. **Azure Subscription** - Valid Azure subscription with appropriate permissions

## Quick Start

1. **Clone and navigate to the project**:
   ```bash
   cd FabrikamApi
   ```

2. **Login to Azure**:
   ```bash
   azd auth login
   ```

3. **Initialize the project**:
   ```bash
   azd init
   # When prompted, use environment name like "fabrikam-api-dev"
   ```

4. **Deploy to Azure**:
   ```bash
   azd up
   ```

   This command will:
   - Provision all Azure resources
   - Build the .NET 9.0 application
   - Deploy the application to App Service

## File Structure

```
FabrikamApi/
├── infra/                          # Infrastructure as Code templates
│   ├── main.bicep                  # Main deployment template
│   ├── resources.bicep             # Azure resources definitions
│   └── main.parameters.json        # Parameter values for deployment
├── src/                            # Source code directory
│   ├── FabrikamApi.csproj         # .NET project file
│   ├── Program.cs                  # Application entry point
│   └── [other source files]       # Additional application files
├── azure.yaml                     # Azure Developer CLI configuration
└── DEPLOYMENT-GUIDE.md            # This file
```

## Infrastructure Components

### Main Template (`main.bicep`)
- **Scope**: Subscription-level deployment
- **Purpose**: Creates resource group and orchestrates resource deployment
- **Key Features**:
  - Unique resource naming using subscription ID and location
  - Comprehensive tagging strategy
  - Environment-specific configuration

### Resources Template (`resources.bicep`)
- **Scope**: Resource group-level deployment
- **Purpose**: Defines all Azure resources and their configuration
- **Key Resources**:
  - Log Analytics Workspace for centralized logging
  - Application Insights for APM
  - User-Assigned Managed Identity for security
  - App Service Plan (Basic tier)
  - App Service with .NET 9.0 configuration

### Key Features

1. **Security Best Practices**:
   - HTTPS-only enforcement
   - Managed Identity authentication
   - Minimal TLS 1.2 requirement
   - Secure FTPS-only access
   - Comprehensive diagnostic logging

2. **Monitoring & Observability**:
   - Application Insights integration
   - Detailed logging configuration
   - Health check endpoint (`/health`)
   - Performance metrics collection

3. **Production Readiness**:
   - Always On enabled for consistent performance
   - HTTP/2.0 support
   - CORS configuration for API access
   - Proper error logging and tracing

## Configuration

### Environment Variables
The deployment automatically configures these environment variables:

- `ASPNETCORE_ENVIRONMENT`: Set to "Production"
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection
- `AZURE_CLIENT_ID`: Managed Identity client ID
- `Logging__LogLevel__Default`: Application logging level

### App Settings
Key application settings configured during deployment:

- Health check monitoring
- Application Insights telemetry
- Managed Identity integration
- Detailed logging for troubleshooting

## API Endpoints

Once deployed, your API will be available at:
- **Primary URL**: `https://app-{resourceToken}.azurewebsites.net/`
- **Health Check**: `https://app-{resourceToken}.azurewebsites.net/health`
- **Swagger UI**: `https://app-{resourceToken}.azurewebsites.net/` (root serves Swagger UI)

## API Features

The FabrikamApi includes:
- **Swagger/OpenAPI** documentation
- **Health check** endpoint for monitoring
- **CORS** enabled for cross-origin requests
- **Entity Framework** with in-memory database
- **Comprehensive logging** to Application Insights

## Management Commands

### View Deployment Status
```bash
azd show
```

### Update Application
```bash
azd deploy
```

### View Application Logs
```bash
azd logs
```

### Destroy Resources
```bash
azd down
```

## Monitoring

Access monitoring through:

1. **Azure Portal**: View resource group `rg-FabrikamApi-{environment}`
2. **Application Insights**: Monitor API performance and usage
3. **Log Analytics**: Query detailed application logs
4. **Health Check**: Monitor `/health` endpoint

## Customization

### Scaling
To modify the App Service Plan tier, edit `resources.bicep`:
```bicep
sku: {
  name: 'S1'        # Change from B1 to S1 for Standard tier
  tier: 'Standard'  # Change from Basic to Standard
}
```

### CORS Configuration
To restrict CORS origins, modify the `cors.allowedOrigins` in `resources.bicep`:
```bicep
cors: {
  allowedOrigins: [
    'https://yourdomain.com'
    'https://localhost:3000'
  ]
  supportCredentials: false
}
```

### Environment-Specific Settings
Modify environment variables in the `appSettings` array within `resources.bicep`.

## Troubleshooting

### Common Issues

1. **Deployment Fails**: Check Azure CLI and azd authentication
2. **Build Errors**: Ensure .NET 9.0 SDK is installed
3. **Runtime Errors**: Check Application Insights for detailed error logs

### Debugging

1. **View Logs**:
   ```bash
   azd logs
   ```

2. **Check Resource Status**:
   ```bash
   az webapp show --name {app-name} --resource-group {resource-group}
   ```

3. **Application Insights**: Use the Azure Portal to query telemetry

## Security Considerations

- **Managed Identity**: Used for secure Azure service authentication
- **HTTPS Only**: All traffic enforced over HTTPS
- **IP Restrictions**: Currently open (modify for production)
- **Application Insights**: Monitors security events and access patterns

## Cost Optimization

The deployment uses cost-effective tiers:
- **App Service Plan**: Basic B1 ($~13/month)
- **Application Insights**: Pay-per-use
- **Log Analytics**: 5GB free tier included

For production, consider Premium tiers for enhanced features and SLA guarantees.

## Next Steps

1. **Custom Domain**: Configure custom domain and SSL certificate
2. **Authentication**: Add Azure AD authentication if needed
3. **Database**: Replace in-memory database with Azure SQL or Cosmos DB
4. **CI/CD**: Set up GitHub Actions for automated deployments
5. **Monitoring**: Configure Application Insights alerts

## Support

For issues with this deployment template:
1. Check the [Azure Developer CLI documentation](https://docs.microsoft.com/en-us/azure/developer/azure-developer-cli/)
2. Review [Azure App Service documentation](https://docs.microsoft.com/en-us/azure/app-service/)
3. Consult [.NET 9.0 deployment guides](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/)

---

This deployment template provides a production-ready foundation for hosting .NET 9.0 Web APIs on Azure with comprehensive monitoring, security, and scalability features.
