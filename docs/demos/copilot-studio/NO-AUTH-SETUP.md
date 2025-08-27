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
   - Go to [https://copilotstudio.microsoft.com](https://copilotstudio.microsoft.com)
   - Sign in with your Microsoft account

2. **Create New Agent**
   - Click "**Create**" → "**New agent**"
   - Enter name: `Fabrikam Business Intelligence Agent`
   - Enter description: `AI assistant for Fabrikam Modular Homes business operations, sales analytics, and customer support`

3. **Configure Agent Instructions**
   ```
   You are the Fabrikam Business Intelligence Assistant, a specialized AI agent for Fabrikam Modular Homes business operations. You help users access and analyze real-time business data including sales, customers, orders, products, and support tickets.

   🏢 COMPANY CONTEXT:
   - Fabrikam Modular Homes specializes in sustainable, customizable modular housing solutions
   - Focus on data-driven insights for sales teams, customer service, and operations
   - Emphasis on providing accurate, real-time business metrics and analytics

   📊 YOUR CAPABILITIES:
   You can help users with:

   💰 SALES & ANALYTICS:
   - Retrieve sales performance data and revenue trends
   - Analyze customer demographics and buying patterns  
   - Generate sales reports and forecasting insights
   - Track order status and delivery timelines

   👥 CUSTOMER MANAGEMENT:
   - Access customer profiles and contact information
   - Review customer order history and preferences
   - Analyze customer satisfaction and feedback data
   - Manage customer segmentation and targeting

   📦 INVENTORY & PRODUCTS:
   - Browse the complete product catalog with specifications
   - Monitor inventory alerts and restocking needs
   - Track product performance and demand patterns

   🎧 CUSTOMER SERVICE:
   - Manage customer support tickets and issue tracking
   - View ticket status and resolution progress
   - Analyze customer feedback and satisfaction metrics
   - Access customer communication history

   🎯 BEHAVIORAL GUIDELINES:
   - Stay focused on Fabrikam business operations and data
   - For off-topic questions, redirect users back to business capabilities
   - Do not answer general knowledge questions unrelated to the business
   - Maintain professional, business-focused identity at all times

   Always provide accurate, helpful information based on real-time data. When users ask for specific data, use the available tools to retrieve the most current information. Be professional, friendly, and focused on helping users accomplish their business goals efficiently.

   IMPORTANT: If users ask questions unrelated to Fabrikam business (like abstract questions about animals, colors, or hypothetical scenarios), politely redirect them to business-related topics you can help with.

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

### Step 2.6: Configure Response Behavior (Limit Off-Topic Questions)

To prevent the agent from answering silly or unrelated questions, add these behavioral guidelines:

1. **Navigate to Agent Instructions**
   - In your agent, click "**Instructions**" in the left navigation
   - Find the "**Additional instructions**" section

2. **Add Focused Behavior Rules**
   - Add this text to the instructions:
   ```
   BEHAVIORAL GUIDELINES:
   - You are focused exclusively on Fabrikam business operations and data
   - For questions unrelated to business (like hypothetical scenarios, general knowledge, or abstract topics), respond with:
   "I'm the Fabrikam Business Intelligence Assistant, focused on providing sales analytics, customer data, and business insights. I can help you with sales performance, customer demographics, product information, order status, support tickets, and business metrics. How can I assist you with Fabrikam business data today?"
   - Do not engage with off-topic questions about animals, colors, hypothetical scenarios, or general knowledge
   - Always redirect users back to business capabilities you can provide
   ```

3. **Save Configuration**
   - Click "**Save**" to apply the behavioral guidelines

> **💡 Professional Benefit**: This keeps your agent focused and professional, preventing it from answering irrelevant questions and maintaining its business intelligence identity.

### Step 3: Create the MCP Custom Connector

1. **Open Power Apps Maker**
   - Go to [https://make.powerapps.com](https://make.powerapps.com)
   - Ensure you're in the **same environment** as your Copilot Studio agent

2. **Navigate to Custom Connectors**
   - In the left navigation, click "**Data**" → "**Custom connectors**"
   - Click "**+ New custom connector**" → "**Create from blank**"

3. **General Information**
   - **Connector name**: `Fabrikam MCP Connector`
   - **Description**: `Connects to Fabrikam Modular Homes MCP server for business operations`
   - **Host**: `your-mcp-app-name.azurewebsites.net` (replace with your actual MCP app name)
   - **Base URL**: `/`

4. **Security Configuration**
   - **Authentication type**: `No authentication`
   - Click "**Next step**" (Security)

5. **Definition Configuration**
   - Click "**New action**"
   - **Summary**: `Call MCP Tools`
   - **Description**: `Execute MCP tools for business operations`
   - **Operation ID**: `callMcpTools`
   - **Visibility**: `normal`

6. **Request Configuration**
   - **Verb**: `POST`
   - **URL**: `/mcp/call`
   - **Headers**:
     - Add header: `Content-Type` = `application/json`
     - Add header: `X-User-Id` = `{userGuid}` (you'll set this when creating the connection)

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

### Step 4: Create the Connection

1. **Test the Connector**
   - In the connector details page, click "**Test**"
   - Click "**+ New connection**"

2. **Connection Configuration**
   - For the `X-User-Id` header, enter your GUID from Step 1
   - Click "**Create connection**"

3. **Test the Connection**
   - Try a simple test call with:
     - **toolName**: `get_sales_analytics`
     - **arguments**: `{}`
   - You should see a successful response with MCP tool data

### Step 5: Test the Connector

1. **Navigate to Test Tab**
   - In your custom connector, click the "**Test**" tab
   - Select your connection

2. **Test MCP Tool Call**
   - **Operation**: `Call MCP Tools`
   - **toolName**: `get_sales_analytics`
   - **dynamicArguments**: `{}`
   - Click "**Test operation**"

3. **Verify Response**
   - You should see a 200 response with business data
   - If you get an error, check your MCP server URL and GUID

### Step 6: Add Connector to Your Agent

1. **Return to Copilot Studio**
   - Go back to your Fabrikam agent
   - Click "**Actions**" in the left navigation

2. **Add Custom Connector**
   - Click "**+ Add an action**"
   - Choose "**Connectors**"
   - Find your "**Fabrikam MCP Connector**"
   - Select the "**Call MCP Tools**" action

3. **Configure the Action**
   - **Connection**: Select the connection you created
   - **Action name**: `Call MCP Tools`
   - Configure any additional parameters as needed

### Step 7: Test Your Agent

1. **Start a New Conversation**
   - Click "**Test**" in the top right of Copilot Studio
   - Start a new conversation

2. **Test Business Queries**
   ```
   What are the current sales trends?
   ```
   
   ```
   Show me customer demographics data
   ```
   
   ```
   What products do we have in inventory?
   ```

3. **Expected Behavior**
   - Agent should call your MCP tools to get real business data
   - Responses should be business-focused and data-driven
   - Off-topic questions should be redirected to business capabilities

4. **Verify MCP Integration**
   - Look for data that comes from your Fabrikam API
   - Confirm the agent is using real business data, not generic content
   - Test that business-focused questions get detailed responses

## 🔧 Troubleshooting

### Common Issues

**🚫 "I don't have access to that information"**
- Check that your MCP server is running and accessible
- Verify the connector connection is configured correctly
- Ensure your GUID is properly set in the connection header
- Try starting a new chat session

**🚫 Connector connection fails**
- Verify your MCP server URL is correct and accessible
- Check that the `/mcp/call` endpoint responds to POST requests
- Ensure your Azure MCP app is deployed and running
- Test the endpoint directly with curl or Postman

**🚫 Agent gives generic responses instead of business data**
- Confirm the custom connector is properly added to the agent
- Check that the MCP tools are being called (look at server logs)
- Verify the agent instructions are configured correctly
- Ensure web search is disabled to prevent fallback content

**🚫 Off-topic responses**
- Review and strengthen the behavioral guidelines in agent instructions
- Disable web search to prevent generic knowledge fallback
- Test specific business queries to verify the agent stays focused

### GUID Management

**Generating New GUIDs**
```powershell
# Generate a fresh GUID
[System.Guid]::NewGuid().ToString()
```

**Using the Same GUID Across Sessions**
- Save your GUID in a secure location
- Use the same GUID for consistent user context
- Generate new GUIDs for different demo scenarios or users

## 🚀 Next Steps

### Moving to Production

Once your demo is working, consider upgrading to authenticated modes:

1. **[JWT Authentication](./JWT-AUTH-SETUP.md)** - Production-ready with demo users
2. **[Entra External ID](./ENTRA-AUTH-SETUP.md)** - Enterprise SSO (coming soon)

### Advanced Demo Features

- **[Demo Prompts Library](../prompts/README.md)** - Ready-made demo scenarios
- **[Workshop Materials](../../workshops/README.md)** - Structured training sessions
- **[Authentication Guide](../../architecture/AUTHENTICATION.md)** - Full auth architecture

### Business Value Demonstrations

Your agent is now ready for business demonstrations:
- **Sales Team**: Show real-time analytics and customer insights
- **Customer Service**: Demonstrate ticket management and customer data access  
- **Operations**: Display inventory management and product catalog features
- **Executives**: Present business intelligence and reporting capabilities

## 📞 Support

### Quick Validation

Use this checklist to ensure your setup is working:

- [ ] Agent responds to business queries with real data
- [ ] MCP connector successfully calls your server
- [ ] Off-topic questions are redirected appropriately
- [ ] Demo scenarios work consistently
- [ ] Session management works with your GUID

### Additional Resources

- **[Main Setup Guide](./README.md)** - Choose different authentication modes
- **[Troubleshooting Guide](./TROUBLESHOOTING.md)** - Comprehensive problem solving
- **[Demo Validation](../validation/README.md)** - Automated testing tools

---

**🎉 Congratulations!** Your Fabrikam MCP agent is ready for demos and development. The agent will provide real-time business intelligence while maintaining professional focus on Fabrikam operations.
