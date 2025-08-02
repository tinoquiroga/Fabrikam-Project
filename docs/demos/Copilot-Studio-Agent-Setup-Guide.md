# 🤖 Building a Fabrikam MCP Agent in Copilot Studio

This guide walks you through creating a Microsoft Copilot Studio agent that connects to your Fabrikam Modular Homes MCP server, enabling natural language interactions with business data for Sales, Inventory, and Customer Service operations.

## 📋 Prerequisites

- Access to Microsoft Copilot Studio
- Access to Power Apps for creating custom connectors
- Your Fabrikam platform deployed to Azure with one of the three authentication modes:
  - **Disabled Mode**: No authentication required (fastest setup)
  - **BearerToken Mode**: JWT authentication with demo credentials
  - **EntraExternalId Mode**: OAuth 2.0 with Microsoft Entra External ID
- Your MCP Server URL (from Azure deployment output)
- Basic understanding of Copilot Studio interface
- For **Disabled Mode**: A valid GUID for user tracking (we'll show you how to generate one)

## 🎯 What Your Agent Will Do

Your Fabrikam agent will enable business users to:
- **Sales Operations**: Query orders, track sales trends, analyze customer data
- **Inventory Management**: Check product availability, monitor stock levels, explore product catalog
- **Customer Service**: Manage support tickets, track resolution status, view customer issues

## 🚀 Step-by-Step Setup

### Step 1: Create a New Agent

1. **Navigate to Copilot Studio**
   - Go to [Microsoft Copilot Studio](https://copilotstudio.microsoft.com/)
   - Sign in with your Microsoft 365 account

2. **Create New Agent**
   - Click "**Create**" in the left navigation
   - Select "**New agent**"
   - Choose "**Skip to configure**" (we'll add knowledge later)

3. **Agent Configuration**
   - **Name**: `Fabrikam Business Assistant`
   - **Description**: `AI assistant for Fabrikam Modular Homes business operations including sales, inventory, and customer service`
   - **Instructions**: 
   ```
   You are a helpful AI assistant for Fabrikam Modular Homes, a company that designs and sells modular homes online. You have access to real-time business data and can help with:

   📊 EXECUTIVE DASHBOARD:
   - Provide comprehensive business overview with key performance indicators
   - Show revenue trends, order metrics, and operational alerts
   - Deliver executive-level insights for strategic decision making

   🏢 SALES OPERATIONS:
   - View and filter orders by status (Pending, Processing, Shipped, Delivered, Cancelled)
   - Analyze sales by region (Northeast, Southeast, Southwest, Northwest, Central)
   - Track customer order history and detailed customer profiles
   - Provide sales analytics with performance trends and revenue breakdowns

   📦 INVENTORY MANAGEMENT:
   - Check product availability and stock levels across all categories
   - Browse products by category (Studio, One-Bedroom, Two-Bedroom, Three-Bedroom, Accessories)
   - Monitor pricing, product analytics, and inventory performance
   - Identify stock issues and provide inventory recommendations

   🎧 CUSTOMER SERVICE:
   - View and manage support tickets with comprehensive filtering
   - Track ticket priorities (Low, Medium, High, Critical) and categories (Quality, Delivery, Technical, Billing)
   - Analyze historical issue patterns and customer satisfaction trends
   - Update ticket status and add internal/external notes

   Always be helpful, professional, and provide actionable business insights. Focus on strategic value and highlight patterns that support executive decision-making. When displaying data, format it clearly and emphasize key metrics that drive business performance.
   ```

### Step 2: Add MCP Server Connection

1. **Navigate to Tools**
   - In your agent, go to the "**Tools**" tab
   - Click "**Add a tool**"

2. **Create New Tool**
   - Select "**New tool**"
   - Select "**Custom connector**"
   - You'll be taken to Power Apps to create a new custom connector

3. **Create Custom Connector in Power Apps**
   - Select "**+ New custom connector**"
   - Select "**Import from GitHub**"

4. **Configure GitHub Import**
   - **Connector type**: Custom
   - **Branch**: dev
   - **Connector**: MCP-Streamable-HTTP
   - Select "**Continue**"

5. **Customize Swagger Definition for Your Authentication Mode**

   After importing, you'll see a Swagger editor. You need to customize it based on your Fabrikam deployment's authentication mode. Here's what to modify:

   #### 🔓 **Disabled Mode Customization** (Recommended for Initial Testing)
   
   **When to use**: Quick demos, POCs, initial testing without authentication complexity
   
   **Required Changes in Swagger Editor**:
   1. **Update host**: Change `host:` to your MCP server domain (e.g., `your-mcp-app-name.azurewebsites.net`)
   2. **Add GUID header**: Configure the `X-User-GUID` header for user tracking
   
   **💡 Domain Name Helper**: Your Azure deployment generates predictable domain names based on your resource group name:
   - **Resource Group**: `rg-fabrikamaidemo-gcpm`
   - **API Domain**: `fabrikam-api-development-gcpm.azurewebsites.net`
   - **MCP Domain**: `fabrikam-mcp-development-gcpm.azurewebsites.net`
   
   Replace the suffix (`gcpm` in this example) with your actual resource group suffix.
   
   **🆔 Generate Your GUID**:
   
   **Option 1: PowerShell**
   ```powershell
   [System.Guid]::NewGuid().ToString()
   ```
   
   **Option 2: Online GUID Generator**
   - Visit: <https://www.guidgenerator.com/>
   - Copy the generated GUID (e.g., `a1b2c3d4-e5f6-7890-abcd-123456789012`)
   
   **Swagger Customization**:
   ```yaml
   swagger: '2.0'
   info:
     title: Fabrikam MCP Server  # Optional: Customize title for better branding
     description: MCP Server for Fabrikam Business Operations
     version: 1.0.0
   host: fabrikam-mcp-development-gcpm.azurewebsites.net
   basePath: /mcp
   schemes:
     - https
   paths:
     /:
       post:
         summary: MCP Server Streamable HTTP
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         parameters:
           - name: X-User-GUID
             in: header
             required: true
             type: string
             default: a1b2c3d4-e5f6-7890-abcd-123456789012
             description: User tracking GUID for Disabled authentication mode
         responses:
           '200':
             description: Success
   securityDefinitions: {}
   security: []
   ```
   
   **Replace the sample GUID** (`a1b2c3d4-e5f6-7890-abcd-123456789012`) with your actual generated GUID.
   
   **🔐 Important Security Notes**:
   - The GUID acts as a user identifier in Disabled authentication mode
   - Each user/session should have a unique GUID for proper tracking
   - The MCP server will reject requests without a valid GUID format
     - https
   paths:
     /:
       post:
         summary: MCP Server Streamable HTTP
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         parameters:
           - name: userGuid
             in: query
             required: true
             type: string
             default: a1b2c3d4-e5f6-7890-abcd-123456789012
             description: User tracking GUID for Disabled authentication mode
         responses:
           '200':
             description: Success
   securityDefinitions: {}
   security: []
   ```
   
   **Replace the sample GUID** (`a1b2c3d4-e5f6-7890-abcd-123456789012`) with your actual generated GUID.

   #### 🔐 **BearerToken Mode Customization** (Production-like Authentication)
   
   **When to use**: Production APIs, secure demos, when demonstrating JWT authentication
   
   **Required Changes in Swagger Editor**:
   1. **Update host**: Change to your MCP server domain
   2. **Add API Key security**: Configure JWT Bearer token authentication
   
   **💡 Domain Name Helper**: Your domain names follow the same pattern:
   - **Resource Group**: `rg-fabrikamaidemo-gcpm`
   - **API Domain**: `fabrikam-api-development-gcpm.azurewebsites.net` (for getting JWT tokens)
   - **MCP Domain**: `fabrikam-mcp-development-gcpm.azurewebsites.net` (for Swagger host)
   
   **Getting Your JWT Token**:
   1. Navigate to: `https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials`
   2. Use demo credentials to login at: `https://your-api-app-name.azurewebsites.net/api/auth/login`
   3. Copy the `accessToken` from the response
   
   **💡 Troubleshooting Demo Credentials**:
   - If you get **HTTP 404** on `/api/auth/demo-credentials`, check your API is running: `https://your-api-app-name.azurewebsites.net/health`
   - If you get **HTTP 403** (Forbidden), your Azure environment might not be set to "Development"
   - Alternative: Check what environment is running: `https://your-api-app-name.azurewebsites.net/api/info`
   - **Demo credentials are only available in Development or dev environments for security**
   
   **Swagger Customization**:
   ```yaml
   swagger: '2.0'
   info:
     title: Fabrikam MCP Server  # Optional: Customize title for better branding
     description: MCP Server for Fabrikam Business Operations (JWT Auth)
     version: 1.0.0
   host: your-mcp-app-name.azurewebsites.net
   basePath: /mcp
   schemes:
     - https
   paths:
     /:
       post:
         summary: MCP Server Streamable HTTP
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         responses:
           '200':
             description: Success
   securityDefinitions:
     api_key:
       type: apiKey
       in: header
       name: Authorization
   security:
     - api_key: []
   ```
   
   **Authentication Configuration**:
   - In the Power Apps connector Security tab, you'll see the API Key field
   - Enter: `Bearer YOUR_JWT_TOKEN` (replace with actual token from login)

   #### 🏢 **EntraExternalId Mode Customization** (Enterprise OAuth)
   
   **When to use**: Enterprise integration, SSO scenarios, OAuth 2.0 demonstrations
   
   **Required Changes in Swagger Editor**:
   1. **Update host**: Change to your MCP server domain
   2. **Add OAuth2 security**: Configure for Entra External ID
   
   **💡 Domain Name Helper**: Use the MCP domain for your OAuth configuration:
   - **Resource Group**: `rg-fabrikamaidemo-gcpm`
   - **MCP Domain**: `fabrikam-mcp-development-gcpm.azurewebsites.net` (for Swagger host)
   
   **Swagger Customization**:
   ```yaml
   swagger: '2.0'
   info:
     title: Fabrikam MCP Server  # Optional: Customize title for better branding
     description: MCP Server for Fabrikam Business Operations (OAuth)
     version: 1.0.0
   host: your-mcp-app-name.azurewebsites.net
   basePath: /mcp
   schemes:
     - https
   paths:
     /:
       post:
         summary: MCP Server Streamable HTTP
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         responses:
           '200':
             description: Success
   securityDefinitions:
     oauth2:
       type: oauth2
       flow: accessCode
       authorizationUrl: https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/oauth2/v2.0/authorize
       tokenUrl: https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/oauth2/v2.0/token
       scopes:
         openid: OpenID Connect scope
         profile: Profile information
         email: Email address
   security:
     - oauth2: [openid, profile, email]
   ```
   
   **OAuth Configuration**:
   - Replace `your-tenant` with your actual Entra External ID tenant name
   - Configure client ID and secret in the Security tab

6. **Test Connection**
   - Click "**Test operation**" to verify connectivity
   - For **Disabled Mode**: Should connect immediately with your GUID
   - For **BearerToken Mode**: Verify your JWT token is valid
   - For **EntraExternalId Mode**: Complete the OAuth flow

### Step 3: Configure Available Tools

Your MCP server provides these optimized business tools:

#### 📊 Executive Intelligence
- **GetBusinessDashboard**: Comprehensive business overview with KPIs and performance metrics
- **GetBusinessAlerts**: Performance alerts and actionable recommendations for operations

#### 🏢 Sales & Customer Analytics
- **GetOrders**: Retrieve and filter orders with detailed information and status tracking
- **GetSalesAnalytics**: Access comprehensive sales performance, trends, and regional analysis
- **GetCustomers**: View customer profiles with order history and support ticket summaries

#### 📦 Product & Inventory Management
- **GetProducts**: Browse product catalog with advanced filtering and detailed specifications
- **GetProductAnalytics**: Analyze product performance, inventory levels, and category insights
- **GetInventoryOperations**: Comprehensive inventory management including availability checks

#### 🎧 Customer Service Operations
- **GetSupportTickets**: View and filter support tickets with comprehensive status tracking
- **GetCustomerServiceAnalytics**: Analyze support trends, resolution times, and performance metrics
- **AddTicketNote**: Add internal or customer-facing notes to support tickets
- **UpdateTicketStatus**: Manage ticket workflow including status, priority, and assignment changes

### Step 4: Test Your Agent

1. **Start a Test Conversation**
   - Click "**Test**" in the top-right corner
   - Try these sample conversations:

#### Executive Dashboard Scenarios
```
```text
"Show me the current business dashboard for Fabrikam Modular Homes. I need to understand our overall performance, key metrics, and any areas that need attention."
```

**Note**: This prompt works in all authentication modes:
- **Disabled Mode**: Uses your configured GUID for user tracking
- **BearerToken Mode**: Uses your JWT token for authenticated access  
- **EntraExternalId Mode**: Uses OAuth token for enterprise authentication

"Based on our current data trends, what recommendations would you make for our business strategy? Where should we focus our attention in the next quarter?"

"What performance alerts do we have? Are there any business issues that need immediate attention?"
```

#### Sales Intelligence Scenarios
```
"Show me all pending orders from the Northeast region"

"I'm preparing for our quarterly board meeting. Can you analyze our sales performance by region and identify our top-performing products?"

"Find orders placed in the last 7 days with order values over $50,000"

"What are our revenue trends and which customer segments are performing best?"
```

#### Customer Service Intelligence Scenarios
```
"We're having a leadership meeting about customer satisfaction. Show me our current support ticket situation - what are the major issues customers are facing?"

"I heard we have some ongoing HVAC issues with certain models. Can you investigate this for me - which customers are affected and what's the business impact?"

"Show me all critical priority tickets that haven't been resolved"

"Can you analyze our customer complaints from 2020-2021 versus recent years? Has our quality improved?"
```

#### Inventory & Product Scenarios
```
"Do we have any Two-Bedroom models in stock for immediate delivery?"

"Show me products under $30,000 that are currently available"

"What's our inventory status and which products need restocking attention?"

"Analyze our product performance - which categories are selling best and what's the profit margin?"
```

### Step 5: Advanced Configuration

#### Custom Greeting
Update your agent's greeting:
```
Hello! I'm your Fabrikam Business Assistant, powered by real-time business intelligence. I can help you with:

📊 **Executive Dashboard**: Get comprehensive business overviews and strategic insights
🏢 **Sales Intelligence**: Analyze orders, revenue trends, and customer performance  
📦 **Inventory Operations**: Check product availability and analyze inventory performance
🎧 **Customer Service**: Manage support tickets and track customer satisfaction trends

What business insights can I provide for you today?
```

#### Topic Triggers
Create specific topics for common business scenarios:

1. **Executive Dashboard Topic**
   - Trigger phrases: "business dashboard", "business overview", "performance metrics", "key indicators"
   - Automatically shows comprehensive business metrics and strategic alerts

2. **Sales Intelligence Topic**
   - Trigger phrases: "sales report", "order analysis", "revenue trends", "customer performance"
   - Guides users through sales analytics and performance insights

3. **Customer Service Intelligence Topic**
   - Trigger phrases: "customer issues", "support tickets", "satisfaction trends", "service analytics"
   - Displays comprehensive customer service insights and historical analysis

4. **Inventory Operations Topic**
   - Trigger phrases: "inventory status", "product availability", "stock levels", "product analytics"
   - Provides complete inventory management and product performance insights

### Step 6: Publish Your Agent

1. **Review Settings**
   - Go to "**Settings**" → "**General**"
   - Verify agent name and description
   - Check that MCP connection is active

2. **Publish**
   - Click "**Publish**" in the top-right
   - Choose your publication channel (Teams, Web, etc.)
   - Test the published agent

## 💡 Business Use Cases & Example Conversations

### 🎯 Executive Dashboard
**User**: "Show me the current business dashboard for Fabrikam Modular Homes"
**Agent Response**: *Displays comprehensive KPIs, revenue trends, operational alerts, and strategic insights for executive decision-making*

### 👩‍💼 Sales Intelligence
**User**: "I'm preparing for our quarterly board meeting. Can you analyze our sales performance by region and identify our top-performing products?"
**Agent Response**: *Provides detailed sales analytics, regional performance comparison, product rankings, and trend analysis*

### 📦 Inventory Operations
**User**: "What's our inventory status and which products need restocking attention?"
**Agent Response**: *Shows comprehensive inventory overview, stock levels, reorder alerts, and product performance insights*

### 🎧 Customer Service Analytics
**User**: "We're having a leadership meeting about customer satisfaction. Show me our current support ticket situation and any patterns I should be concerned about"
**Agent Response**: *Analyzes support trends, issue categorization, resolution performance, and identifies areas needing attention*

### 🔍 Strategic Business Intelligence
**User**: "Based on our current data trends, what recommendations would you make for our business strategy?"
**Agent Response**: *Provides data-driven strategic recommendations, identifies growth opportunities, and highlights risk areas*

## 🔧 Troubleshooting

### Authentication Mode Specific Issues

#### 🔓 Disabled Mode Issues

**Custom Connector Creation Failed**
- Ensure you have proper permissions in Power Apps
- Verify your MCP server URL is accessible: `https://your-mcp-app-name.azurewebsites.net/mcp`
- Make sure **no authentication** is configured in the Security tab

**GUID Configuration Issues**
- Ensure you've generated a valid GUID using PowerShell or an online generator
- Verify the GUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- The GUID will be automatically passed as `userGuid` parameter in MCP requests

**Domain Name Issues**
- **Finding your domains**: Use the resource group suffix pattern:
  - Resource Group: `rg-fabrikamaidemo-SUFFIX` 
  - MCP Domain: `fabrikam-mcp-development-SUFFIX.azurewebsites.net`
  - API Domain: `fabrikam-api-development-SUFFIX.azurewebsites.net`
- Replace `SUFFIX` with your actual resource group suffix (e.g., `gcpm`, `demo`, etc.)

**No Data Returned in Disabled Mode**
- Verify your GUID is properly configured in the connector
- Check that the MCP server is running and accessible
- Try testing the connector directly in Power Apps

#### 🔐 BearerToken Mode Issues

**Authentication Failed**
- Ensure you've obtained a valid JWT token from `/api/auth/demo-credentials` and `/api/auth/login`
- Verify the Authorization header is set to: `Bearer YOUR_ACCESS_TOKEN`
- Check that the JWT token hasn't expired (tokens expire after 60 minutes by default)

**Token Refresh Required**
- JWT tokens expire for security - get a new token from the login endpoint
- Consider implementing token refresh logic for production scenarios
- Demo credentials are available at: `https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials`

**Connector Security Configuration**
- Go to Security tab in Power Apps custom connector
- Select "API Key" authentication type
- Set Parameter location to "Header"
- Set Parameter name to "Authorization"

#### 🏢 EntraExternalId Mode Issues

**OAuth Configuration Failed**
- Verify your Entra External ID tenant configuration
- Ensure the client ID and client secret are correct
- Check that redirect URIs include your Copilot Studio callback URLs

**OAuth Flow Failed**
- Complete the OAuth consent flow in your browser
- Ensure your Entra application has proper permissions
- Verify the scope includes: `openid profile email`

**Token Validation Failed**
- Check that your Entra External ID tenant is properly configured
- Verify the OAuth endpoints are accessible
- Ensure the application registration includes necessary API permissions

### Common Issues (All Modes)

**Swagger Import Issues**
- Ensure you're using "Create from blank" instead of GitHub import
- Verify the host parameter matches your actual MCP server URL
- Replace `YOUR_MCP_SERVER_HOST` with your actual domain

**Connector Configuration Failed**
- Ensure Scheme is set to HTTPS
- Verify Host is set to your actual MCP app service domain (e.g., `fabrikam-mcp-dev-x1y2.azurewebsites.net`)
- **IMPORTANT**: Set Base URL to `/mcp` (standard MCP protocol endpoint)
- Test the connector connection from Power Apps

**"LimitTools" or "Limiting number of tools to 15" Errors**
- Copilot Studio has a maximum limit of 15 tools per custom connector
- The Fabrikam MCP server has been optimized to provide 12 consolidated business tools (well under the limit)
- **Consolidated tools include**:
  - **Executive**: GetBusinessDashboard, GetBusinessAlerts
  - **Sales**: GetOrders, GetSalesAnalytics, GetCustomers  
  - **Products**: GetProducts, GetProductAnalytics, GetInventoryOperations
  - **Support**: GetSupportTickets, GetCustomerServiceAnalytics, AddTicketNote, UpdateTicketStatus
- If you still see the error, ensure you're using the latest version of the custom connector
- Delete and recreate the custom connector if the tool count hasn't updated

**No Data Returned**
- The Fabrikam API simulates realistic business data with JSON seed files
- Data is generated dynamically for demonstration purposes
- If no results appear, try broader search criteria
- Verify your authentication mode is correctly configured
- Check that the custom connector actions are properly configured

**Tool Not Working**
- Verify the custom connector is properly connected in Tools settings
- Check that the action names match the available MCP functions
- Review agent instructions for proper tool usage guidance
- Test individual connector actions in Power Apps
- Ensure authentication is properly configured for your chosen mode

## 🎉 Next Steps

1. **Customize for Your Demo**
   - Adjust agent instructions for your specific use case
   - Create custom topics for your audience
   - Add company branding and messaging

2. **Extend Functionality**
   - Connect additional data sources
   - Create custom workflows
   - Integrate with other business systems

3. **Train Your Team**
   - Share example conversations
   - Document common use cases
   - Provide training materials for end users

## 🔗 Additional Resources

### Authentication Mode Testing

- **Test Disabled Mode**: Perfect for initial setup and quick demos
- **Test BearerToken Mode**: Demonstrates JWT authentication with security
- **Test EntraExternalId Mode**: Enterprise OAuth 2.0 integration (framework ready)

### Server Endpoints (Replace with your actual deployment URLs)

- **MCP Server Status**: `https://your-mcp-app-name.azurewebsites.net/status`
- **API Health Check**: `https://your-api-app-name.azurewebsites.net/health`
- **Authentication Info**: `https://your-api-app-name.azurewebsites.net/api/info/auth`
- **Demo Credentials** (BearerToken mode): `https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials`

### Deployment & Architecture

- **Azure Deployment Guide**: See [`docs/deployment/DEPLOY-TO-AZURE.md`](../deployment/DEPLOY-TO-AZURE.md) for complete deployment instructions
- **Authentication Architecture**: See [`docs/architecture/AUTHENTICATION-ARCHITECTURE.md`](../architecture/AUTHENTICATION-ARCHITECTURE.md) for technical details
- **API Architecture**: See [`docs/architecture/API-ARCHITECTURE.md`](../architecture/API-ARCHITECTURE.md) for complete business context

### Demo Resources

- **Quick Demo Prompts**: See [`docs/demos/QUICK-DEMO-PROMPTS.md`](../QUICK-DEMO-PROMPTS.md) for 3-minute demo scenarios
- **Comprehensive Demo Guide**: See [`docs/demos/COPILOT-DEMO-PROMPTS.md`](../COPILOT-DEMO-PROMPTS.md) for detailed demo strategies
- **Three-Mode Testing Guide**: See [`docs/testing/THREE-MODE-AUTHENTICATION-TESTING-GUIDE.md`](../testing/THREE-MODE-AUTHENTICATION-TESTING-GUIDE.md)

---

**Ready to transform your business operations with AI?** 🚀

Your Fabrikam agent is now ready to demonstrate the power of Copilot in real business scenarios. Use this setup to show customers and partners how AI can streamline Sales, Inventory, and Customer Service operations with natural language interactions.
