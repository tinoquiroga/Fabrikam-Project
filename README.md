# 🏗️ Fabrikam Modular Homes - Complete Business Simulation

A comprehensive .NET-based business simulation platform designed for **Microsoft Copilot demonstrations**, **partner training**, and **hands-on AI labs**. This project simulates the operations of Fabrikam, a fictional modular home builder, providing realistic business data and workflows for immersive AI experiences.

## 🎯 Project Overview

### Purpose
- **Accelerate Copilot adoption** in SME&C scenarios with realistic business context
- **Enable hands-on labs** that showcase tangible AI business value
- **Support partner training** with ready-to-deploy demonstration environments
- **Demonstrate Model Context Protocol (MCP)** integration with business systems

### Business Context
**Fabrikam** is a fictional modular home builder that:
- Designs and sells modular homes online
- Offers rapid delivery and installation
- Operates with a lean, tech-forward business model
- Serves as an ideal SME business example for AI demonstrations

---

## 🏗️ Architecture

The solution consists of two main components:

### 1. **FabrikamApi** - Business Operations API
A RESTful API simulating core business functions:
- **Sales Module**: Order management and customer analytics
- **Inventory Module**: Product catalog and stock monitoring  
- **Customer Service Module**: Support ticket management and resolution

### 2. **FabrikamMcp** - Model Context Protocol Server
An MCP server that enables Copilot to interact with the business API:
- **Natural language interface** to business data
- **Structured tool definitions** for AI agents
- **HTTP transport** for easy integration

```
┌─────────────────┐    HTTP/MCP    ┌──────────────────┐
│                 │◄──────────────►│                  │
│   FabrikamMcp   │                │   FabrikamApi    │
│  (MCP Server)   │                │ (Business API)   │
│                 │                │                  │
└─────────────────┘                └──────────────────┘
        ▲                                    ▲
        │                                    │
    MCP Tools                          REST Endpoints
        │                                    │
        ▼                                    ▼
┌─────────────────┐                ┌──────────────────┐
│     Copilot     │                │   Swagger UI     │
│   Integration   │                │   /swagger       │
└─────────────────┘                └──────────────────┘
```

---

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Visual Studio Code or Visual Studio 2022

### 1. Clone and Build
```powershell
git clone <repository-url>
cd Fabrikam-Project

# Build both components
dotnet build FabrikamApi/src/FabrikamApi.csproj
dotnet build FabrikamMcp/src/FabrikamMcp.csproj
```

### 2. Start the Business API
```powershell
cd FabrikamApi/src
dotnet run
# API starts at http://localhost:5000
```

### 3. Start the MCP Server (in new terminal)
```powershell
$env:ASPNETCORE_URLS="http://localhost:5001"
cd FabrikamMcp/src  
dotnet run
# MCP Server starts at http://localhost:5001
```

### 4. Verify Everything Works
```powershell
# Test API health
curl http://localhost:5000/health

# Test MCP server status  
curl http://localhost:5001/status

# View API documentation
# Open browser to http://localhost:5000 (Swagger UI)
```

---

## 📊 Business Modules & Features

### 🛒 Sales Module
**Endpoints**: `/api/orders`, `/api/customers`

**Features**:
- Complete order lifecycle management
- Customer relationship tracking
- Sales analytics and reporting
- Regional sales breakdowns

**Sample Data**:
- 8 customers across different regions
- 15+ orders with various statuses
- Realistic order values and items

### 📦 Inventory Module  
**Endpoints**: `/api/products`

**Features**:
- Product catalog management
- Real-time stock tracking
- Low stock alerts
- Inventory value calculations

**Product Categories**:
- Single Family Homes (1,200 - 2,500 sq ft)
- Duplex Units
- Accessory Dwelling Units (ADUs)
- Commercial Spaces
- Upgrade Components (kitchens, solar systems)

### 🎧 Customer Service Module
**Endpoints**: `/api/supporttickets`

**Features**:
- Support ticket lifecycle management
- Priority and category tracking
- Internal and external notes
- Resolution time analytics

**Sample Scenarios**:
- Delivery inquiries
- Installation questions
- Product defects
- Billing issues

---

## 🤖 MCP Tools for Copilot Integration

### Sales Tools (`FabrikamSalesTools`)
- `GetOrders()` - Retrieve orders with filtering
- `GetOrderById()` - Detailed order information
- `GetSalesAnalytics()` - Revenue and performance metrics
- `GetCustomers()` - Customer directory and profiles
- `GetCustomerById()` - Complete customer history

### Inventory Tools (`FabrikamInventoryTools`)
- `GetProducts()` - Product catalog with filtering
- `GetProductById()` - Detailed product specifications
- `GetInventorySummary()` - Stock levels and valuations
- `CheckProductAvailability()` - Stock validation for orders
- `GetLowStockProducts()` - Inventory alerts

### Customer Service Tools (`FabrikamCustomerServiceTools`)
- `GetSupportTickets()` - Ticket management with filtering
- `GetSupportTicketById()` - Complete ticket history
- `GetCustomerServiceAnalytics()` - Performance metrics
- `AddTicketNote()` - Update ticket conversations
- `UpdateTicketStatus()` - Workflow management
- `GetUrgentTickets()` - Priority queue management

---

## 💬 Sample Copilot Interactions

Once the MCP server is integrated with Copilot, users can ask natural language questions:

### Sales Scenarios
- *"What are our top-selling products this quarter?"*
- *"Show me all pending orders in the Pacific Northwest"*
- *"What's our average order value for single-family homes?"*

### Inventory Scenarios  
- *"Do we have enough Cozy Cottage 1200 units to fulfill order #12345?"*
- *"Which products are running low on stock?"*
- *"What's the total value of our current inventory?"*

### Customer Service Scenarios
- *"Show me all open support tickets assigned to John Smith"*  
- *"What's the average resolution time for delivery issues?"*
- *"Add a note to ticket #67890 about the customer callback"*

---

## 🧪 Development & Testing

### API Testing
Use the included `FabrikamApi.http` file with the REST Client extension:

```http
### Get all customers
GET http://localhost:5000/api/customers

### Get inventory summary  
GET http://localhost:5000/api/products/inventory

### Create new support ticket
POST http://localhost:5000/api/supporttickets
Content-Type: application/json

{
  "customerId": 1,
  "subject": "Delivery delay question",
  "description": "When can I expect my order?",
  "priority": "Medium",
  "category": "DeliveryIssue"
}
```

### Database
- Uses **in-memory database** for development
- **Auto-seeded** with realistic sample data
- Data resets on each application restart

### Sample Data Includes:
- 8 customers across 5 regions
- 8 product models with realistic specifications  
- 15 orders in various stages
- 20 support tickets with notes and assignments

### 📚 **Deployment Documentation**

For comprehensive deployment instructions, CI/CD setup, and environment management, see:
- 📖 **[DEPLOYMENT-GUIDE.md](./DEPLOYMENT-GUIDE.md)** - Complete Azure deployment and CI/CD setup
- 🔗 **[MCP-API-INTEGRATION.md](./MCP-API-INTEGRATION.md)** - Service integration details

### 🏗️ **Monorepo Benefits**

This project uses a **monorepo strategy** for deploying two related services:

| Benefit | Description |
|---------|-------------|
| **Coordinated Deployments** | Deploy both services together with proper dependency management |
| **Shared Configuration** | Common CI/CD pipelines, secrets, and deployment scripts |
| **Integration Testing** | Automated testing of service-to-service communication |
| **Simplified Management** | Single repository, unified documentation and versioning |
| **Path-Based Triggers** | Deploy only services that changed |

---

## 🔧 Configuration

### FabrikamApi Configuration
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### FabrikamMcp Configuration  
```json  
// appsettings.Development.json
{
  "FabrikamApi": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

### Production Configuration
```bash
# Environment variables for production
FabrikamApi__BaseUrl=https://fabrikam-api-prod.azurewebsites.net
ASPNETCORE_ENVIRONMENT=Production
```

---

## 🚀 Deployment to Azure

This project supports **multiple deployment strategies** from a single repository (monorepo approach):

### 🎯 **Deployment Options**

#### Option 1: Individual Azure Developer CLI (Recommended)
Deploy each service independently using their own `azure.yaml` configurations:

```powershell
# Deploy FabrikamApi
cd FabrikamApi
azd auth login
azd up

# Deploy FabrikamMcp (in new terminal)
cd FabrikamMcp  
azd auth login
azd env set FABRIKAM_API_BASE_URL "https://your-api-url.azurewebsites.net"
azd up
```

#### Option 2: Coordinated Deployment Script
Use the provided automation script:

```powershell
# From root directory - deploys both with proper integration
.\Deploy-Integrated.ps1 -EnvironmentName "production" -Location "eastus"
```

#### Option 3: CI/CD with GitHub Actions
Automated deployment on code changes (see CI/CD section below).

### 🏗️ **Azure Resources Created**

Each deployment creates:

**FabrikamApi Resources:**
- App Service Plan (Linux, B1 SKU)
- App Service (ASP.NET Core 9.0)
- Application Insights
- Log Analytics Workspace
- User-Assigned Managed Identity

**FabrikamMcp Resources:**  
- App Service Plan (Linux, B1 SKU)
- App Service (ASP.NET Core 9.0)
- Application Insights
- Log Analytics Workspace
- User-Assigned Managed Identity

### 🔧 **Environment Configuration**

#### Production Environment Variables
```bash
# FabrikamApi
ASPNETCORE_ENVIRONMENT=Production
AZURE_CLIENT_ID=<managed-identity-id>

# FabrikamMcp
ASPNETCORE_ENVIRONMENT=Production
FABRIKAM_API_BASE_URL=https://your-api-url.azurewebsites.net
AZURE_CLIENT_ID=<managed-identity-id>
```

#### Development Environment Variables
```bash
# Local development
FabrikamApi__BaseUrl=http://localhost:5000
ASPNETCORE_URLS=http://localhost:5001
```

---

## 🔄 CI/CD with GitHub Actions

### Monorepo CI/CD Strategy

This project uses a **monorepo with independent deployments** approach:

```yaml
# Workflow triggers on changes to specific paths
on:
  push:
    paths:
    - 'FabrikamApi/**'        # API changes
    - 'FabrikamMcp/**'        # MCP changes  
    - '.github/workflows/**'  # Pipeline changes
```

### 🚀 **Automated Deployment Pipelines**

#### FabrikamApi Pipeline
- **Trigger**: Changes to `FabrikamApi/` directory
- **Steps**: Build → Test → Deploy to Azure App Service
- **Environment**: Separate staging and production slots

#### FabrikamMcp Pipeline  
- **Trigger**: Changes to `FabrikamMcp/` directory
- **Steps**: Build → Test → Configure API URL → Deploy
- **Dependencies**: Can wait for API deployment if both changed

### 📋 **CI/CD Setup Instructions**

1. **Fork/Clone this repository**
2. **Set up Azure Service Principal**:
   ```bash
   az ad sp create-for-rbac --name "Fabrikam-Deploy" --role contributor \
     --scopes /subscriptions/{subscription-id} --sdk-auth
   ```

3. **Configure GitHub Secrets**:
   - `AZURE_CREDENTIALS` - Service principal JSON
   - `AZURE_SUBSCRIPTION_ID` - Your subscription ID
   - `AZURE_RESOURCE_GROUP_NAME` - Target resource group

4. **Enable GitHub Actions**:
   - Push code changes to trigger deployments
   - Monitor deployments in GitHub Actions tab

### 🏷️ **Environment Management**

| Environment | Branch | Azure Resources | URL Pattern |
|-------------|--------|-----------------|-------------|
| **Development** | `main` | dev-fabrikam-* | *-dev.azurewebsites.net |
| **Staging** | `release/*` | staging-fabrikam-* | *-staging.azurewebsites.net |  
| **Production** | `production` | prod-fabrikam-* | *-prod.azurewebsites.net |

---

## 📚 Lab Scenarios & Use Cases

### For Business Decision Makers
| Role | Scenario | Copilot Interaction |
|------|----------|-------------------|
| **CEO** | Strategic overview | *"Summarize our sales performance and top challenges"* |
| **Sales Manager** | Pipeline review | *"Show me all orders that need attention this week"* |  
| **Operations** | Inventory planning | *"Which products should we reorder based on current demand?"* |

### For Technical Audiences
| Scenario | Learning Objective |
|----------|-------------------|
| **API Integration** | How AI agents connect to business systems |
| **MCP Development** | Building custom AI tools and connectors |
| **Business Context** | Translating real workflows into AI experiences |

---

## 🤝 Contributing & Customization

### Adding New Business Modules
1. **Create new controller** in `FabrikamApi/src/Controllers/`
2. **Add corresponding MCP tool** in `FabrikamMcp/src/Tools/`  
3. **Register tool** in MCP server Program.cs
4. **Update sample data** in DataSeedService.cs

### Extending Sample Data
The `DataSeedService` class contains all sample data generation. Customize:
- Customer demographics and regions
- Product catalog and pricing
- Order patterns and seasonality  
- Support ticket scenarios

### Custom Business Rules
Implement custom logic in:
- **Order processing** (inventory validation, pricing)
- **Support routing** (auto-assignment, escalation)
- **Analytics calculations** (KPIs, dashboards)

---

## 📖 API Documentation

### Full REST API documentation available at:
- **Development**: http://localhost:5000/swagger
- **Production**: https://your-api-url/swagger

### Key Endpoint Patterns:
```
GET    /api/{resource}           # List with filtering
GET    /api/{resource}/{id}      # Get by ID  
POST   /api/{resource}           # Create new
PUT    /api/{resource}/{id}      # Update existing
PATCH  /api/{resource}/{id}      # Partial update
DELETE /api/{resource}/{id}      # Remove
```

---

## ⚡ Performance & Scalability

### Development Setup
- **In-memory database** for fast prototyping
- **Auto-seeded data** for consistent demos
- **CORS enabled** for browser-based tools

### Production Considerations  
- Replace with **Azure SQL Database** or **Cosmos DB**
- Implement **proper authentication** and authorization
- Add **monitoring and telemetry** with Application Insights
- Configure **auto-scaling** based on demand

---

## 🔒 Security Notes

### Current Setup (Demo/Lab Use)
- **Open CORS policy** for development ease
- **No authentication** required for API access
- **In-memory data** that resets on restart

### Production Hardening  
- Implement **Azure AD authentication**
- Add **API key management** 
- Enable **request throttling**
- Configure **network security groups**
- Use **managed identities** for service communication

---

## 📞 Support & Resources

### Getting Help
- **Issues**: Report bugs or feature requests via GitHub Issues
- **Discussions**: Join community discussions for best practices
- **Documentation**: Comprehensive guides in `/docs` folder

### Related Resources  
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Microsoft Copilot Documentation](https://docs.microsoft.com/copilot/)
- [.NET Minimal APIs Guide](https://docs.microsoft.com/aspnet/core/fundamentals/minimal-apis)

---

## 📊 Project Status

### Current Version: 1.0.0
✅ **Complete**: Core business modules (Sales, Inventory, Customer Service)  
✅ **Complete**: MCP server with HTTP transport  
✅ **Complete**: Sample data generation and seeding  
✅ **Complete**: API documentation and testing tools  

### Roadmap  
🔄 **In Progress**: Azure deployment automation  
📋 **Planned**: Advanced analytics and reporting  
📋 **Planned**: Real-time event simulation  
📋 **Planned**: Security scenarios and incident response

---

**Built with ❤️ for the Microsoft AI ecosystem**

*This project demonstrates the power of combining structured business APIs with AI-native interfaces to create compelling, realistic demonstrations of AI business value.*
