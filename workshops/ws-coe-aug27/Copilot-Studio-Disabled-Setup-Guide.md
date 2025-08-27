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

**🎯 Generate a GUID (Simple & Reliable):**

```powershell
# Generate a new GUID
$userGuid = [System.Guid]::NewGuid().ToString()
Write-Host "Your GUID: $userGuid"

# Copy this GUID - you'll need it for the connector configuration
Write-Host "Save this GUID for later: $userGuid"
```

**Expected Output:**
```
Your GUID: a1b2c3d4-e5f6-7890-abcd-123456789012
Save this GUID for later: a1b2c3d4-e5f6-7890-abcd-123456789012
```

**💾 Save your GUID** - you'll need it for the connector configuration in the next steps.

> **💡 Why a GUID?** In disabled authentication mode, the system uses a GUID to track your session and associate API calls with a virtual user context, enabling personalized responses without requiring full authentication.

### Step 2: Create a New Agent

1. **Navigate to Copilot Studio**
   - Go to [Microsoft Copilot Studio](https://copilotstudio.microsoft.com/)
   - Sign in with your Microsoft 365 account

2. **Create New Agent**
   - Click "**Create**" in the left navigation
   - Select "**New agent**"
   - Choose "**Skip to configure**" (we'll add knowledge later)

3. **Agent Configuration**
   - **Name**: `Business Assistant (username)`
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

### Step 2.5: Configure Knowledge Sources (Recommended)

For workshop clarity, disable generic knowledge sources so participants can clearly see when MCP tools are working vs. failing:

1. **Navigate to Overview**
   - In your agent, click "**Overview**" in the left navigation
   - Scroll down to find the "**Web search**" setting

2. **Disable Web Search**
   - Toggle **OFF** the "**Web search**" option
   - This prevents fallback to generic sales analytics content

3. **Why Disable?**
   - **Clear Failure Mode**: When MCP session expires, you'll get a clear "I don't have access to that information" instead of generic web content
   - **Workshop Focus**: Participants see the difference between live business data vs. no data
   - **Realistic Behavior**: In production, you wouldn't want generic web content mixed with your business data

> **💡 Workshop Benefit**: With web search disabled, session timeouts will result in clear "I cannot access that information right now" responses, making it obvious when participants need to start a new chat to restore MCP functionality.

### Step 3: Create the MCP Custom Connector

1. **Navigate to Tools in Copilot Studio**
   - In your Copilot Studio agent, click "**Tools**" in the left-hand navigation
   - Click "**+ New Tool**"
   - Select "**Custom Connector**" (this opens the connector page at make.powerapps.com)

2. **Create New Custom Connector**
   - Click "**+ New Custom Connector**"
   - Select "**Import from Github**"
   - **Connector type**: Select "**Custom**"
   - **Branch**: Select "**dev**"
   - **Connector**: Select "**MCP-Streamable-HTTP**"

3. **Configure Connector Settings**
   - **Change Name**: From "MCP-Streamable-HTTP" to "**Fabrikam MCP-[username]**"
     - Example: `Fabrikam MCP-imatest`
   
   - **Set Host**: Enter your MCP default domain (⚠️ **Note**: This is the MCP server, not the API)
     - Example: `fabrikam-mcp-development-bb7fsc.azurewebsites.net`

4. **Configure Security**
   - Press "**Security**" tab
   - Leave option as "**No Authentication**"

5. **Update Definition**
   - Press "**Definition**" tab
   - **Change Summary**: From "MCP Server Streamable HTTP" to "**Fabrikam MCP -[username]-**"

6. **Modify Code Configuration**
   - Click "**Code**" tab
   - Click "**Create Connector**"

7. **Add GUID Parameter Using Swagger Editor**
   - **Open the Swagger Editor** (slider on top)
   - **Locate** the `operationId: InvokeMCP` section
   - **Add the GUID parameter** under `parameters:` (⚠️ **Indentation is important!**)

   ```yaml
   parameters:
     - name: X-User-GUID
       in: header
       required: true
       type: string
       default: a1b2c3d4-e5f6-7890-abcd-123456789012
       description: User tracking GUID for session management
   ```

   > **💡 Replace the default GUID** with the one you generated in Step 1

8. **Save and Verify**
   - Press "**Update connector**" again
   - **Open Swagger editor** to confirm:
     - ✅ All changes saved correctly
     - ✅ The 'host' matches your MCP app service domain
     - ✅ The GUID parameter is properly formatted

### Step 5: Create the Connection

1. **Navigate to Test Page**
   - Click on the "**Test**" tab in your custom connector

2. **Create New Connection**
   - Press "**+ New Connection**"
   - A dialog box will appear with your connector name (e.g., "Fabrikam MCP-imatest")
   - Press "**Create**"

3. **Verify Connection**
   - The connection should be created successfully
   - You should see your new connection listed in the connections area

### Step 6: Test the Connector

1. **Verify Connector Setup**
   - The connector is now created and the connection is established
   - **Note**: Full MCP testing requires a session, so we'll verify the basic connectivity

2. **Optional: Test Basic Connectivity**
   - You can test that the MCP server is responding by checking if your Azure deployment is accessible
   - The connector is ready to be added to your Copilot Studio agent
   - Full functionality testing will happen when the agent uses the connector with proper session management

> **💡 Why not test the MCP endpoint directly?**  
> MCP servers require session establishment before handling requests. Testing the raw endpoint would return a "Session not found" error. The connector will work properly when used within Copilot Studio, which handles session management automatically.

### Step 7: Add Connector to Your Agent

1. **Return to Copilot Studio**
   - Open your Fabrikam Business Assistant agent
   - Go to "**Tools**" in the left navigation or refresh the page that took you to Power Apps

2. **Add Custom Connector**
   - You should see the custom connector you just made (there may be others from other users)
   - Click the "**...**" (three dots) on your connector
   - Select "**Add to agent**"

3. **Configure the Connection**
   - The X-User-GUID should be automatically configured
   - Click "**Save**"

### Step 8: Test Your Agent

1. **Start a Test Conversation**
   - In Copilot Studio, click "**Test**" in the top-right corner
   - Try these example queries:

2. **First-Time Authorization (Important!)**
   - ⚠️ **Expected**: Your first test query will fail - this is normal!
   - You'll see a link in the test dialog to "**open connection manager**"
   - Click this link to open a new tab for user connections
   - **Authorize the connection** in the new tab
   - Return to Copilot Studio and start a **new test** to retry your query

   > **💡 Connection Authorization Notes:**
   > - This authorization step is required for all new custom connectors
   > - If your agent has been idle for some time, you may need to re-authorize
   > - Always start a **new test** after authorization (don't retry in the same conversation)

3. **Test Queries**
   ```
   "Show me the executive dashboard"
   "What orders do we have pending?"
   "Check inventory levels for popular products"
   "Get sales analytics for the last month"
   "What's our product analytics showing?"
   ```

   > **💡 Note about Analytics Queries:**  
   > • **Customer Analytics**: Not yet implemented - use "Get customers by region" for customer directory  
   > • **Customer Service**: Technical issue - use "Show me the executive dashboard" for support metrics  
   > • **Sales & Product Analytics**: ✅ Working perfectly

4. **Verify Functionality**
   - After authorization, all responses should include business data from your Fabrikam deployment
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

**❌ "Session not found" Error After Inactivity**
- **Cause**: MCP session has expired due to inactivity or timeout
- **Error Details**: `{"reasonCode":"RequestFailure","errorMessage":"Connector request failed","HttpStatusCode":"notFound","errorResponse":"[{\"jsonrpc\":\"2.0\",\"id\":\"\",\"error\":{\"Code\":-32001,\"Message\":\"Session not found\"}}]"}`
- **Agent Behavior (Web Search Enabled)**: The agent will fall back to knowledge sources and return generic information instead of live business data
- **Agent Behavior (Web Search Disabled)**: The agent will clearly state it cannot access the information, making the failure obvious
- **Solution**: Start a **new chat conversation** in the test panel
- **Note**: This is normal MCP behavior - sessions have limited lifespans

> **💡 Session Management & Fallback Behavior:**  
> MCP sessions are temporary and will expire after periods of inactivity. With web search disabled (recommended for workshops), session failures result in clear "I cannot access that information" messages rather than confusing generic content. Always start a new chat conversation when you encounter session errors to restore MCP tool functionality.

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
