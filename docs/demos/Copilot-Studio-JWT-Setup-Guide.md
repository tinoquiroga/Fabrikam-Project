# 🔐 Building a Fabrikam MCP Agent with JWT Authentication

This guide walks you through creating a Microsoft Copilot Studio agent that connects to your **JWT-authenticated** Fabrikam Modular Homes MCP server, enabling secure natural language interactions with business data.

## 📋 Prerequisites

- Access to Microsoft Copilot Studio
- Access to Power Apps for creating custom connectors
- Your Fabrikam platform deployed to Azure with **BearerToken Mode** enabled
- Your MCP Server URL (from Azure deployment output)
- Demo user credentials (we'll show you how to get these)
- Basic understanding of Copilot Studio interface

## 🎯 What Your Agent Will Do

Your secure Fabrikam agent will enable business users to:
- **Sales Operations**: Query orders, track sales trends, analyze customer data
- **Inventory Management**: Check product availability, monitor stock levels, explore product catalog
- **Customer Service**: Manage support tickets, track resolution status, view customer issues

All interactions are authenticated and traced to specific users for security and auditing.

## 🚀 Step-by-Step Setup

### Step 1: Get Your JWT Token

Before setting up Copilot Studio, you need to get a JWT token from your Fabrikam API.

1. **Get Demo User Credentials**
   
   First, retrieve the demo user credentials from your API:
   
   ```bash
   curl -X GET "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
   ```
   
   **PowerShell Alternative**:
   ```powershell
   $credentials = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
   $credentials.demoUsers | Format-Table -Property name, email, password, roles
   ```

2. **Authenticate and Get JWT Token**
   
   Use the admin user credentials to get a JWT token:
   
   ```bash
   curl -X POST "https://your-api-app-name.azurewebsites.net/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "lee.gu@fabrikam.levelupcsp.com",
       "password": "your-admin-password"
     }'
   ```
   
   **PowerShell Alternative**:
   ```powershell
   # Get demo credentials
   $creds = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
   $admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }
   
   # Login to get JWT token
   $loginBody = @{
       email = $admin.email
       password = $admin.password
   } | ConvertTo-Json
   
   $authResponse = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
   
   Write-Host "JWT Token: $($authResponse.accessToken)"
   Write-Host "Expires At: $($authResponse.expiresAt)"
   ```

3. **Save Your JWT Token**
   
   From the response, save the `accessToken` value. It will look like:
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIuLi4...
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
   - **Name**: `Fabrikam Business Assistant (Secure)`
   - **Description**: `Secure AI assistant for Fabrikam Modular Homes business operations with JWT authentication`
   - **Instructions**: 
   ```
   You are a secure AI assistant for Fabrikam Modular Homes, a company that designs and sells modular homes online. You have access to authenticated business data and can help with:

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

   IMPORTANT: All data access is authenticated and logged for security and compliance purposes.
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
   - **OpenAPI URL**: Use this template (replace with your domain):
     ```
     https://raw.githubusercontent.com/your-repo/swagger-mcp-jwt.yaml
     ```
   
   Or copy and paste this Swagger definition:

   ```yaml
   swagger: '2.0'
   info:
     title: Fabrikam MCP Server (JWT Auth)
     description: >-
       Secure MCP Server for Fabrikam Business Operations with JWT authentication.
       Requires Bearer token authentication for all requests.
     version: 1.0.0
   host: your-mcp-app-name.azurewebsites.net
   basePath: /mcp
   schemes:
     - https
   consumes: []
   produces: []
   securityDefinitions:
     BearerAuth:
       type: apiKey
       in: header
       name: Authorization
       description: "JWT Bearer token from Fabrikam API authentication"
   security:
     - BearerAuth: []
   paths:
     /:
       post:
         summary: MCP Server with JWT Authentication
         x-ms-agentic-protocol: mcp-streamable-1.0
         operationId: InvokeMCP
         security:
           - BearerAuth: []
         responses:
           '200':
             description: Success
           '401':
             description: Unauthorized - Invalid or missing JWT token
           '403':
             description: Forbidden - Insufficient permissions
   definitions: {}
   parameters: {}
   responses: {}
   tags: []
   ```

4. **Configure the Connector**
   - **Update host**: Change `host:` to your MCP server domain (e.g., `fabrikam-mcp-development-xyz.azurewebsites.net`)
   - **Connector Name**: `Fabrikam MCP Server (JWT)`
   - **Description**: `Secure connection to Fabrikam MCP Server with JWT authentication`

### Step 4: Configure Authentication

1. **Set Up Authentication**
   - In the custom connector configuration, go to the "**Security**" tab
   - **Authentication Type**: `API Key`
   - **Parameter Label**: `Authorization`
   - **Parameter Name**: `Authorization`
   - **Parameter Location**: `Header`

2. **Test Authentication**
   - Click "**Update connector**"
   - Go to the "**Test**" tab
   - In the **Authorization** field, enter: `Bearer YOUR_JWT_TOKEN`
   - Replace `YOUR_JWT_TOKEN` with the actual JWT token from Step 1
   - Click "**Test operation**"
   - You should see a successful response with MCP capabilities

### Step 5: Add Connector to Your Agent

1. **Return to Copilot Studio**
   - Open your Fabrikam Business Assistant agent
   - Go to "**Actions**" in the left navigation

2. **Add Custom Connector**
   - Click "**+ Add an action**"
   - Select "**Connectors**"
   - Find "**Fabrikam MCP Server (JWT)**" in your custom connectors
   - Click "**Add**"

3. **Configure the Connection**
   - When prompted for the Authorization header, enter: `Bearer YOUR_JWT_TOKEN`
   - Replace `YOUR_JWT_TOKEN` with your actual JWT token
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

3. **Verify Authentication**
   - All responses should include data specific to your authenticated user
   - Check that the agent can access all business data securely

## 🔧 Troubleshooting

### Common Issues

**❌ "Unauthorized" Error**
- **Cause**: JWT token is expired or invalid
- **Solution**: Get a new JWT token using the login process in Step 1

**❌ "Forbidden" Error**
- **Cause**: User doesn't have sufficient permissions
- **Solution**: Use an admin user account or check user roles

**❌ "Connection Failed" Error**
- **Cause**: Incorrect MCP server URL or network issues
- **Solution**: Verify your Azure deployment URLs and network connectivity

### JWT Token Management

**Token Expiration**: JWT tokens expire after 1 hour by default. For production use, consider:
- Implementing automatic token refresh
- Using longer-lived service accounts
- Setting up token refresh workflows

**Security Best Practices**:
- Never share JWT tokens
- Use environment-specific tokens
- Monitor token usage in logs
- Rotate tokens regularly

## 🚀 Next Steps

### Production Deployment
1. **Set up proper user accounts** instead of demo credentials
2. **Implement token refresh** for long-running sessions
3. **Configure role-based access control** for different user types
4. **Enable audit logging** for compliance requirements

### Advanced Features
1. **Multi-user support** with individual JWT tokens
2. **Permission-based data filtering** by user role
3. **Custom business workflows** with authenticated actions
4. **Integration with existing identity systems**

## 📞 Support

If you encounter issues:
1. Check your JWT token validity and expiration
2. Verify your Azure deployment is running with BearerToken mode
3. Review the authentication logs in your Azure deployment
4. Ensure your demo users are properly configured

For additional help, consult the Fabrikam deployment documentation or contact your system administrator.

---

**🔒 Security Note**: This guide uses demo credentials for illustration. In production environments, use proper user management and authentication systems appropriate for your organization's security requirements.
