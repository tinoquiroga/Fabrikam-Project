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

> **🔐 Security Note**: JWT tokens expire after a configured time (usually 24 hours). For demos, you may need to refresh the token. In production, implement automatic token renewal.

### Step 2: Create a New Agent

1. **Navigate to Copilot Studio**
   - Go to [https://copilotstudio.microsoft.com](https://copilotstudio.microsoft.com)
   - Sign in with your Microsoft account

2. **Create New Agent**
   - Click "**Create**" → "**New agent**"
   - Enter name: `Fabrikam Business Assistant (Secure)`
   - Enter description: `Secure AI assistant for Fabrikam Modular Homes business operations with JWT authentication`

3. **Configure Agent Instructions**
   ```
   You are a secure AI assistant for Fabrikam Modular Homes, a company that designs and sells modular homes. You have access to authenticated business data and can help with:

   🏢 COMPANY CONTEXT:
   - Fabrikam Modular Homes specializes in sustainable, customizable modular housing solutions
   - You operate with JWT authentication for secure access to business data
   - Focus on providing authenticated, real-time business metrics and analytics

   📊 YOUR CAPABILITIES:
   You can help users with:

   💰 SALES & ANALYTICS:
   - Retrieve authenticated sales performance data and revenue trends
   - Analyze customer demographics and buying patterns with user context
   - Generate secure sales reports and forecasting insights
   - Track order status and delivery timelines with proper authorization

   👥 CUSTOMER MANAGEMENT:
   - Access customer profiles and contact information (authenticated)
   - Review customer order history and preferences securely
   - Analyze customer satisfaction and feedback data
   - Manage customer segmentation with role-based access

   📦 INVENTORY & PRODUCTS:
   - Browse the complete product catalog with specifications
   - Monitor inventory alerts and restocking needs
   - Track product performance and demand patterns
   - Access secure inventory management data

   🎧 CUSTOMER SERVICE:
   - Manage customer support tickets with authenticated access
   - View ticket status and resolution progress securely
   - Analyze customer feedback and satisfaction metrics
   - Access customer communication history with proper authorization

   🎯 SECURITY & BEHAVIORAL GUIDELINES:
   - All data access is authenticated and traceable to specific users
   - Stay focused on Fabrikam business operations and data
   - For off-topic questions, redirect users back to business capabilities
   - Do not answer general knowledge questions unrelated to the business
   - Maintain professional, business-focused identity at all times
   - Respect user permissions and data access levels

   Always provide accurate, helpful information based on authenticated real-time data. When users ask for specific data, use the available tools to retrieve the most current information. Be professional, friendly, and focused on helping users accomplish their business goals efficiently while maintaining security protocols.

   IMPORTANT: If users ask questions unrelated to Fabrikam business (like abstract questions about animals, colors, or hypothetical scenarios), politely redirect them to business-related topics you can help with.

   NOTE: This system uses JWT authentication for secure access to business data.
   ```

4. **Save Initial Configuration**
   - Click "**Create**" to save your agent

### Step 2.5: Configure Knowledge Sources (Recommended)

For secure environments, disable generic knowledge sources to ensure all responses come from authenticated business data:

1. **Navigate to Overview**
   - In your agent, click "**Overview**" in the left navigation
   - Scroll down to find the "**Web search**" setting

2. **Disable Web Search**
   - Toggle **OFF** the "**Web search**" option
   - This ensures only authenticated business data is used

3. **Security Benefits**
   - **Data Integrity**: Ensures all responses use authenticated business data
   - **Clear Authentication**: When tokens expire, you get clear authentication errors
   - **Compliance**: Maintains data access controls and audit trails

### Step 2.6: Configure Secure Response Behavior

Configure the agent to maintain secure, business-focused behavior:

1. **Navigate to Agent Instructions**
   - In your agent, click "**Instructions**" in the left navigation
   - Find the "**Additional instructions**" section

2. **Add Security-Focused Behavior Rules**
   ```
   SECURITY & BEHAVIORAL GUIDELINES:
   - You are a secure business intelligence system for Fabrikam operations
   - All data access requires proper JWT authentication
   - For questions unrelated to business, respond with:
   "I'm the Fabrikam Secure Business Intelligence Assistant. I provide authenticated access to sales analytics, customer data, and business insights. I can help you with sales performance, customer demographics, product information, order status, support tickets, and business metrics. How can I assist you with Fabrikam business data today?"
   - Do not engage with off-topic questions about animals, colors, hypothetical scenarios, or general knowledge
   - Always redirect users back to authenticated business capabilities
   - Respect user permissions and data access levels at all times
   ```

3. **Save Security Configuration**
   - Click "**Save**" to apply the security guidelines

### Step 3: Create the Secure MCP Custom Connector

1. **Open Power Apps Maker**
   - Go to [https://make.powerapps.com](https://make.powerapps.com)
   - Ensure you're in the **same environment** as your Copilot Studio agent

2. **Navigate to Custom Connectors**
   - In the left navigation, click "**Data**" → "**Custom connectors**"
   - Click "**+ New custom connector**" → "**Create from blank**"

3. **General Information**
   - **Connector name**: `Fabrikam Secure MCP Connector`
   - **Description**: `Secure JWT-authenticated connector to Fabrikam Modular Homes MCP server`
   - **Host**: `your-mcp-app-name.azurewebsites.net` (replace with your actual MCP app name)
   - **Base URL**: `/`

4. **Security Configuration**
   - **Authentication type**: `API Key`
   - **Parameter label**: `Authorization`
   - **Parameter name**: `Authorization`
   - **Parameter location**: `Header`
   - Click "**Next step**" (Security)

5. **Definition Configuration**
   - Click "**New action**"
   - **Summary**: `Call Secure MCP Tools`
   - **Description**: `Execute MCP tools with JWT authentication for secure business operations`
   - **Operation ID**: `callSecureMcpTools`
   - **Visibility**: `normal`

6. **Request Configuration**
   - **Verb**: `POST`
   - **URL**: `/mcp/call`
   - **Headers**:
     - Add header: `Content-Type` = `application/json`

7. **Request Body**
   - **Body**: 
   ```json
   {
     "method": "tools/call",
     "params": {
       "name": "{toolName}",
       "arguments": {dynamicArguments}
     }
   }
   ```

8. **Response Configuration**
   - Add a sample response (you can test this later)
   - Click "**Create connector**"

### Step 4: Create the Secure Connection

1. **Test the Connector**
   - In the connector details page, click "**Test**"
   - Click "**+ New connection**"

2. **Connection Configuration**
   - For the `Authorization` API key, enter: `Bearer YOUR_JWT_TOKEN`
   - Replace `YOUR_JWT_TOKEN` with the JWT token from Step 1
   - Click "**Create connection**"

3. **Test the Secure Connection**
   - Try a simple test call with:
     - **toolName**: `get_sales_analytics`
     - **arguments**: `{}`
   - You should see a successful response with authenticated business data

> **🔑 Token Management**: JWT tokens have expiration times. You'll need to update the connection with a fresh token when it expires. For production, consider implementing automatic token refresh.

### Step 5: Test the Secure Connector

1. **Navigate to Test Tab**
   - In your custom connector, click the "**Test**" tab
   - Select your secure connection

2. **Test Authenticated MCP Tool Call**
   - **Operation**: `Call Secure MCP Tools`
   - **toolName**: `get_sales_analytics`
   - **dynamicArguments**: `{}`
   - Click "**Test operation**"

3. **Verify Authenticated Response**
   - You should see a 200 response with authenticated business data
   - The response should include user context and proper data access
   - If you get a 401 error, check your JWT token format and expiration

### Step 6: Add Secure Connector to Your Agent

1. **Return to Copilot Studio**
   - Go back to your Fabrikam agent
   - Click "**Actions**" in the left navigation

2. **Add Secure Custom Connector**
   - Click "**+ Add an action**"
   - Choose "**Connectors**"
   - Find your "**Fabrikam Secure MCP Connector**"
   - Select the "**Call Secure MCP Tools**" action

3. **Configure the Secure Action**
   - **Connection**: Select the authenticated connection you created
   - **Action name**: `Call Secure MCP Tools`
   - Configure any additional parameters as needed

### Step 7: Test Your Secure Agent

1. **Start a New Conversation**
   - Click "**Test**" in the top right of Copilot Studio
   - Start a new conversation

2. **Test Authenticated Business Queries**
   ```
   What are the current sales trends with user context?
   ```
   
   ```
   Show me customer demographics data for my access level
   ```
   
   ```
   What products do we have in inventory?
   ```

3. **Expected Behavior**
   - Agent should call your MCP tools with JWT authentication
   - Responses should include authenticated business data
   - Data should be contextualized to the authenticated user
   - Off-topic questions should be redirected to business capabilities

4. **Verify Secure MCP Integration**
   - Look for data that includes user context and permissions
   - Confirm the agent is using authenticated business data
   - Test that business-focused questions get detailed, authorized responses

## 🔧 Troubleshooting

### Authentication Issues

**🚫 "401 Unauthorized" responses**
- Check that your JWT token is valid and not expired
- Verify the token format includes "Bearer " prefix
- Ensure the token was generated correctly from the login endpoint
- Test the token directly against the API endpoints

**🚫 "Token expired" errors**
- Generate a new JWT token using the login process
- Update the connector connection with the new token
- Consider the token expiration time for demo planning

**🚫 "Forbidden" or permission errors**
- Verify the demo user has appropriate roles and permissions
- Check that the user account is active and not disabled
- Ensure the API endpoints support the authenticated user's access level

### Connection Issues

**🚫 Connector connection fails**
- Verify your MCP server URL is correct and accessible
- Check that the `/mcp/call` endpoint responds to authenticated POST requests
- Ensure your Azure MCP app is deployed and running with JWT authentication enabled
- Test the endpoint directly with curl using your JWT token

**🚫 Agent gives "access denied" responses**
- Confirm the custom connector is properly added to the agent
- Check that the JWT authentication is working (look at server logs)
- Verify the agent instructions are configured correctly
- Ensure the connection uses the correct JWT token format

### Security Validation

**🔐 Verify JWT Authentication**
```bash
# Test your JWT token directly
curl -X GET "https://your-api-app-name.azurewebsites.net/api/customers" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**🔐 Check Token Expiration**
```powershell
# Decode JWT to check expiration (requires JWT module)
$token = "YOUR_JWT_TOKEN"
$payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($token.Split('.')[1] + '=='))
$decoded = $payload | ConvertFrom-Json
Write-Host "Token expires: $(Get-Date -UnixTimeSeconds $decoded.exp)"
```

### Token Management

**Refreshing Expired Tokens**
```powershell
# Re-authenticate to get a fresh token
$creds = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
$admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }

$loginBody = @{
    email = $admin.email
    password = $admin.password
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

Write-Host "New JWT Token: $($authResponse.accessToken)"
```

## 🚀 Next Steps

### Production Security

For production deployments:

1. **Implement Token Refresh** - Automatic JWT token renewal
2. **Role-Based Access Control** - User-specific data access
3. **Audit Logging** - Track all authenticated interactions
4. **Token Storage** - Secure token management and rotation

### Enterprise Authentication

Consider upgrading to enterprise-grade authentication:

1. **[Entra External ID](./ENTRA-AUTH-SETUP.md)** - Enterprise SSO (coming soon)
2. **[Authentication Architecture](../../architecture/AUTHENTICATION.md)** - Complete auth strategy

### Advanced Security Features

- **[Security Best Practices](../../architecture/AUTHENTICATION.md#security-considerations)** - Production security guidelines
- **[Compliance Features](../../architecture/AUTHENTICATION.md#compliance-audit-logging)** - Audit and compliance tools
- **[User Management](../authentication/README.md)** - Demo user administration

### Secure Demo Scenarios

Your authenticated agent is now ready for secure business demonstrations:
- **Executive Access**: Show authenticated sales analytics and business insights
- **Role-Based Demos**: Demonstrate different access levels for different user roles
- **Secure Customer Data**: Present customer information with proper authorization
- **Audit Trail**: Show how all interactions are logged and traceable

## 📞 Support

### Security Validation Checklist

Use this checklist to ensure your secure setup is working:

- [ ] JWT token authenticates successfully with the API
- [ ] Agent responds to business queries with authenticated data
- [ ] MCP connector calls include proper authorization headers
- [ ] User context is maintained throughout conversations
- [ ] Token expiration is handled gracefully
- [ ] Off-topic questions are redirected appropriately
- [ ] Audit trails are maintained for all interactions

### Additional Resources

- **[Main Setup Guide](./README.md)** - Choose different authentication modes
- **[Authentication Architecture](../../architecture/AUTHENTICATION.md)** - Complete security overview
- **[Troubleshooting Guide](./TROUBLESHOOTING.md)** - Comprehensive problem solving
- **[Demo Validation](../validation/README.md)** - Automated testing tools

---

**🎉 Congratulations!** Your secure Fabrikam MCP agent is ready for production-style demos with JWT authentication. The agent will provide authenticated real-time business intelligence while maintaining security controls and audit trails.
