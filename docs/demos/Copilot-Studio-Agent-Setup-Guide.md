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

5. **Configure General Information**
   - **Scheme**: HTTPS
   - **Host**: `fabrikam-mcp-dev.levelupcsp.com`
   - **Base URL**: `/mcp` (standard MCP protocol endpoint)
   - Configure any additional authentication settings if required

6. **Test Connection**
   - Test the connector to verify it can access your MCP server
   - Ensure all tools/actions are properly imported

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
"Show me the current business dashboard for Fabrikam Modular Homes. I need to understand our overall performance, key metrics, and any areas that need attention."

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

**"LimitTools" or "Limiting number of tools to 15" Errors**
- Copilot Studio has a maximum limit of 15 tools per custom connector
- The Fabrikam MCP server has been optimized to provide 11 consolidated business tools (well under the limit)
- **Consolidated tools include**:
  - **Executive**: GetBusinessDashboard, GetBusinessAlerts
  - **Sales**: GetOrders, GetSalesAnalytics, GetCustomers  
  - **Products**: GetProducts, GetProductAnalytics, GetInventoryOperations
  - **Support**: GetSupportTickets, GetCustomerServiceAnalytics, AddTicketNote, UpdateTicketStatus
- If you still see the error, ensure you're using the latest version of the custom connector
- Delete and recreate the custom connector if the tool count hasn't updated

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
- **Fabrikam Demo Prompts**: See [`docs/demos/QUICK-DEMO-PROMPTS.md`](../QUICK-DEMO-PROMPTS.md) for 3-minute demo scenarios
- **Comprehensive Demo Guide**: See [`docs/demos/COPILOT-DEMO-PROMPTS.md`](../COPILOT-DEMO-PROMPTS.md) for detailed demo strategies
- **API Architecture**: See [`docs/architecture/API-ARCHITECTURE.md`](../architecture/API-ARCHITECTURE.md) for complete business context

---

**Ready to transform your business operations with AI?** 🚀

Your Fabrikam agent is now ready to demonstrate the power of Copilot in real business scenarios. Use this setup to show customers and partners how AI can streamline Sales, Inventory, and Customer Service operations with natural language interactions.
