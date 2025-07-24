# 🤖 Building a Fabrikam MCP Agent in Copilot Studio

This guide walks you through creating a Microsoft Copilot Studio agent that connects to your Fabrikam Modular Homes MCP server, enabling natural language interactions with business data for Sales, Inventory, and Customer Service operations.

## 📋 Prerequisites

- Access to Microsoft Copilot Studio
- Access to Power Apps for creating custom connectors
- Your Fabrikam MCP Server deployed and running at: `https://fabrikam-mcp-dev.levelupcsp.com`
- Basic understanding of Copilot Studio interface

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

   🏢 SALES OPERATIONS:
   - View and filter orders by status (Pending, Processing, Shipped, Delivered, Cancelled)
   - Analyze sales by region (Northeast, Southeast, Southwest, Northwest, Central)
   - Track customer order history and details

   📦 INVENTORY MANAGEMENT:
   - Check product availability and stock levels
   - Browse products by category (Studio, One-Bedroom, Two-Bedroom, Three-Bedroom, Accessories)
   - Monitor pricing and product details

   🎧 CUSTOMER SERVICE:
   - View and manage support tickets
   - Filter tickets by status (Open, In Progress, Resolved, Closed)
   - Track ticket priority (Low, Medium, High, Critical)
   - View ticket categories (Technical, Billing, Delivery, General)

   Always be helpful, professional, and provide actionable insights. When displaying data, format it clearly and highlight key information that would be valuable for business decision-making.
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

5. **Configure General Information**
   - **Scheme**: HTTPS
   - **Host**: `fabrikam-mcp-dev.levelupcsp.com`
   - **Base URL**: `/mcp` (standard MCP protocol endpoint)
   - Configure any additional authentication settings if required

6. **Test Connection**
   - Test the connector to verify it can access your MCP server
   - Ensure all tools/actions are properly imported

### Step 3: Configure Available Tools

Your MCP server provides these business tools:

#### 🏢 Sales Tools
- **GetOrders**: Retrieve orders with filtering options
- **GetCustomers**: View customer information and history
- **GetSalesAnalytics**: Access sales trends and performance data

#### 📦 Inventory Tools  
- **GetProducts**: Browse product catalog with filtering
- **CheckStock**: Verify product availability
- **GetLowStockItems**: Identify inventory that needs attention

#### 🎧 Customer Service Tools
- **GetSupportTickets**: View and filter support tickets
- **GetTicketDetails**: Access detailed ticket information
- **UpdateTicketStatus**: Manage ticket resolution workflow

### Step 4: Test Your Agent

1. **Start a Test Conversation**
   - Click "**Test**" in the top-right corner
   - Try these sample conversations:

#### Sales Scenarios
```
"Show me all pending orders from the Northeast region"

"What are our top-selling products this month?"

"Find orders placed in the last 7 days"

"Show me all orders over $50,000"
```

#### Inventory Scenarios
```
"Do we have any Two-Bedroom models in stock?"

"Show me products under $30,000"

"What accessories are available?"

"Check stock levels for all Studio models"
```

#### Customer Service Scenarios
```
"Show me all open support tickets"

"Find high priority tickets that need attention"

"What technical issues are customers reporting?"

"Show me tickets assigned to Sarah Johnson"
```

### Step 5: Advanced Configuration

#### Custom Greeting
Update your agent's greeting:
```
Hello! I'm your Fabrikam Business Assistant. I can help you with:

🏢 **Sales**: View orders, track performance, analyze customer data
📦 **Inventory**: Check product availability and stock levels  
🎧 **Customer Service**: Manage support tickets and customer issues

What would you like to explore today?
```

#### Topic Triggers
Create specific topics for common business scenarios:

1. **Sales Dashboard Topic**
   - Trigger phrases: "sales report", "order status", "revenue analysis"
   - Automatically show recent orders and key metrics

2. **Inventory Check Topic**
   - Trigger phrases: "check stock", "product availability", "inventory levels"
   - Guide users through product category selection

3. **Support Ticket Topic**
   - Trigger phrases: "customer issues", "support tickets", "help desk"
   - Display open tickets and resolution workflow

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

### 🎯 CEO Dashboard
**User**: "Give me a business overview for this week"
**Agent Response**: *Retrieves recent orders, shows revenue trends, highlights any critical support issues*

### 👩‍💼 Sales Manager
**User**: "Which regions are performing best this quarter?"
**Agent Response**: *Analyzes orders by region, shows top-performing areas, identifies opportunities*

### 📦 Inventory Manager
**User**: "What products are running low on stock?"
**Agent Response**: *Checks inventory levels, identifies items needing reorder, suggests restocking priorities*

### 🎧 Customer Service Lead
**User**: "Show me all critical support tickets that haven't been resolved"
**Agent Response**: *Filters tickets by priority and status, displays actionable list with customer details*

## 🔧 Troubleshooting

### Common Issues

**Custom Connector Creation Failed**
- Ensure you have proper permissions in Power Apps
- Verify GitHub access for importing MCP-Streamable-HTTP connector
- Check that the base URL is accessible: `https://fabrikam-mcp-dev.levelupcsp.com`

**GitHub Import Issues**
- Confirm you have access to the GitHub repository containing MCP-Streamable-HTTP
- Verify the "dev" branch is available and accessible
- Check that the connector type is set to "Custom"

**Connector Configuration Failed**
- Ensure Scheme is set to HTTPS
- Verify Host is set to: `fabrikam-mcp-dev.levelupcsp.com`
- **IMPORTANT**: Set Base URL to `/mcp` (standard MCP protocol endpoint)
- Test the connector connection from Power Apps

**"RequestFailure" or "notFound" Errors**
- This typically indicates an incorrect Base URL configuration
- Ensure Base URL is set to `/mcp` for proper MCP protocol compliance
- Verify the MCP server is responding at: `https://fabrikam-mcp-dev.levelupcsp.com/status`
- Test MCP endpoint directly: `https://fabrikam-mcp-dev.levelupcsp.com/mcp`

**No Data Returned**
- The Fabrikam API simulates realistic business data
- Data is generated dynamically for demonstration purposes
- If no results appear, try broader search criteria
- Check that the custom connector actions are properly configured

**Tool Not Working**
- Verify the custom connector is properly connected in Tools settings
- Check that the action names match the available MCP functions
- Review agent instructions for proper tool usage guidance
- Test individual connector actions in Power Apps

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

- **MCP Server Status**: https://fabrikam-mcp-dev.levelupcsp.com/status
- **API Health Check**: https://fabrikam-api-dev.levelupcsp.com/health
- **Fabrikam Business Documentation**: See `FabrikamApi/Docs/images/Fabrikam Modular Homes API.md`

---

**Ready to transform your business operations with AI?** 🚀

Your Fabrikam agent is now ready to demonstrate the power of Copilot in real business scenarios. Use this setup to show customers and partners how AI can streamline Sales, Inventory, and Customer Service operations with natural language interactions.
