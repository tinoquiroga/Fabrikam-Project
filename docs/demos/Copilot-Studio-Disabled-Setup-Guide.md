# 🔓 Building a Fabrikam MCP Agent (No Authentication)

This guide walks you through creating a Microsoft Copilot Studio agent that connects to your Fabrikam Modular Homes MCP server with **authentication disabled**, enabling quick setup for demos, development, and proof-of-concept scenarios.

## 📋 Prerequisites

- Access to Microsoft Copilot Studio
- Access to Power Apps for creating custom connectors
- Your Fabrikam platform deployed to Azure with **Disabled Mode** enabled
- Your MCP Server URL (from Azure deployment output)
- Basic understanding of Copilot Studio interface

## 🎯 What Your Agent Will Do

Your Fabrikam agent will enable business users to:
- **Sales Operations**: Query orders, track sales trends, analyze customer data
- **Inventory Management**: Check product availability, monitor stock levels, explore product catalog
- **Customer Service**: Manage support tickets, track resolution status, view customer issues

*Note: This setup is ideal for demonstrations, development environments, and proof-of-concept scenarios where authentication complexity isn't needed.*

## 🚀 Step-by-Step Setup

### Step 1: Generate Your User GUID

Since authentication is disabled, you need a GUID for user tracking and session management.

**Option 1: Fabrikam API (Recommended)**

```bash
curl -X POST "https://your-api-app-name.azurewebsites.net/api/UserRegistration/disabled-mode" \
  -H "Content-Type: application/json" \
  -d '{"firstName": "Demo", "lastName": "User", "email": "demo@company.com"}'
```

**PowerShell Alternative**:
```powershell
$apiUrl = "https://your-api-app-name.azurewebsites.net/api/UserRegistration/disabled-mode"
$body = @{
    firstName = "Demo"
    lastName = "User" 
    email = "demo@company.com"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $apiUrl -Method POST -Body $body -ContentType "application/json"
Write-Host "Your GUID: $($response.guid)"
```

**Option 2: Generate Locally**

```powershell
[System.Guid]::NewGuid().ToString()
```

**Save your GUID** - you'll need it for the connector configuration. It will look like:
```
a1b2c3d4-e5f6-7890-abcd-123456789012
```

### Step 2: Create a New Agent

1. **Navigate to Copilot Studio**
   - Go to [Microsoft Copilot Studio](https://copilotstudio.microsoft.com/)
   - Sign in with your Microsoft 365 account

2. **Create New Agent**
   - Click "**Create**" in the left navigation
   - Select "**New agent**"
   - Choose "**Skip to configure**" (we'll add knowledge later)

3. **Agent Configuration**
   - **Name**: `Fabrikam Business Assistant (Demo)`
   - **Description**: `Demo AI assistant for Fabrikam Modular Homes business operations`
   - **Instructions**: 
   ```
   You are a helpful AI assistant for Fabrikam Modular Homes, a company that designs and sells modular homes online. You have access to real-time business data and can help with:

   📊 EXECUTIVE DASHBOARD:
   - Provide comprehensive business overview with key performance indicators
   - Show revenue trends, order metrics, and operational alerts
   - Deliver executive-level insights for strategic decision making

   🏢 SALES OPERATIONS:
   - View and filter orders by status (Pending, Processing, Shipped, Delivered, Cancelled)
   - Track sales performance metrics and customer analytics
   - Generate sales reports and identify trends
   - Monitor revenue streams and customer acquisition

   📦 INVENTORY MANAGEMENT:
   - Check real-time product availability and stock levels
   - Browse the complete product catalog with specifications
   - Monitor inventory alerts and restocking needs
   - Track product performance and demand patterns

   🎧 CUSTOMER SERVICE:
   - Manage customer support tickets and issue tracking
   - View ticket status and resolution progress
   - Analyze customer feedback and satisfaction metrics
   - Access customer communication history

   Always provide accurate, helpful information based on real-time data. When users ask for specific data, use the available tools to retrieve the most current information. Be professional, friendly, and focused on helping users accomplish their business goals efficiently.

   NOTE: This is a demonstration environment with sample data for evaluation purposes.
   ```

4. **Save Initial Configuration**
   - Click "**Create**" to save your agent

### Step 3: Create the MCP Custom Connector

1. **Open Power Platform Admin Center**
   - Navigate to [Power Platform Admin Center](https://admin.powerplatform.microsoft.com/)
   - Go to "**Data**" > "**Custom connectors**"

2. **Create New Custom Connector**
   - Click "**+ New custom connector**"
   - Select "**Import an OpenAPI from URL**"

3. **Import MCP Swagger Definition**
   
   Copy and paste this Swagger definition:

   ```yaml
   swagger: '2.0'
   info:
     title: Fabrikam MCP Server (Demo Mode)
     description: >-
       Demo MCP Server for Fabrikam Business Operations with no authentication required.
       Perfect for demonstrations and proof-of-concept scenarios.
     version: 1.0.0
   host: your-mcp-app-name.azurewebsites.net
   basePath: /mcp
   schemes:
     - https
   consumes: []
   produces: []
   paths:
     /:
       post:
         summary: MCP Server Demo Mode
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         parameters:
           - name: X-User-GUID
             in: header
             required: true
             type: string
             default: a1b2c3d4-e5f6-7890-abcd-123456789012
             description: User tracking GUID for session management
         responses:
           '200':
             description: Success
           '400':
             description: Bad Request - Missing or invalid GUID
   definitions: {}
   parameters: {}
   responses: {}
   securityDefinitions: {}
   security: []
   tags: []
   ```

4. **Configure the Connector**
   - **Update host**: Change `host:` to your MCP server domain (e.g., `fabrikam-mcp-development-xyz.azurewebsites.net`)
   - **Update default GUID**: Replace the default GUID with the one you generated in Step 1
   - **Connector Name**: `Fabrikam MCP Server (Demo)`
   - **Description**: `Demo connection to Fabrikam MCP Server with no authentication`

### Step 4: Test the Connector

1. **Update and Test**
   - Click "**Update connector**"
   - Go to the "**Test**" tab
   - The **X-User-GUID** field should be pre-filled with your GUID
   - Click "**Test operation**"
   - You should see a successful response with MCP capabilities

### Step 5: Add Connector to Your Agent

1. **Return to Copilot Studio**
   - Open your Fabrikam Business Assistant agent
   - Go to "**Actions**" in the left navigation

2. **Add Custom Connector**
   - Click "**+ Add an action**"
   - Select "**Connectors**"
   - Find "**Fabrikam MCP Server (Demo)**" in your custom connectors
   - Click "**Add**"

3. **Configure the Connection**
   - The X-User-GUID should be automatically configured
   - Click "**Save**"

### Step 6: Test Your Agent

1. **Start a Test Conversation**
   - In Copilot Studio, click "**Test**" in the top-right corner
   - Try these example queries:

2. **Test Queries**
   ```
   "Show me the executive dashboard"
   "What orders do we have pending?"
   "Check inventory levels for popular products"
   "Are there any open customer service tickets?"
   "What's our revenue trend this month?"
   ```

3. **Verify Functionality**
   - All responses should include business data from your Fabrikam deployment
   - The agent should be able to access all available business functions

## 🔧 Troubleshooting

### Common Issues

**❌ "Bad Request" Error**
- **Cause**: Missing or invalid GUID in the header
- **Solution**: Verify your GUID is properly configured in the connector

**❌ "Connection Failed" Error**
- **Cause**: Incorrect MCP server URL or network issues
- **Solution**: Verify your Azure deployment URLs and network connectivity

**❌ "No Data Returned" Error**
- **Cause**: Fabrikam deployment might not be running or configured properly
- **Solution**: Check your Azure deployment status and ensure demo data is seeded

### GUID Management

**GUID Validation**: The system validates that your GUID follows Microsoft GUID format:
- 32 hexadecimal digits
- Displayed in groups separated by hyphens
- Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**Session Tracking**: Your GUID is used for:
- Session management and tracking
- User activity logging
- Demo data isolation (in multi-tenant scenarios)

## 🚀 Next Steps

### Moving to Production
When you're ready to add security:
1. **Enable JWT authentication** in your Fabrikam deployment
2. **Follow the JWT setup guide** for secure user authentication
3. **Implement proper user management** with role-based access
4. **Configure audit logging** for compliance requirements

### Advanced Demo Features
1. **Multiple user scenarios** with different GUIDs
2. **Custom demo data** tailored to your presentation needs
3. **Integration testing** with different business scenarios
4. **Performance testing** with various query patterns

## 📞 Support

If you encounter issues:
1. Verify your GUID format and configuration
2. Check your Azure deployment is running with Disabled mode
3. Ensure your MCP server URL is correct and accessible
4. Review the Azure deployment logs for any errors

For additional help, consult the Fabrikam deployment documentation.

---

**⚠️ Important**: This setup is intended for demonstration and development purposes only. For production environments, always enable proper authentication and security measures.
