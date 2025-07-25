# 🏗️ Fabrikam Modular Homes - Business AI Demonstration Platform

> **🤖 GitHub Copilot Notice**: This project uses **automated CI/CD** with GitHub Actions. Every code change triggers testing and deployment. See `.github/copilot-cicd-context.md` for full pipeline details.

A comprehensive .NET-based business simulation platform designed for **Microsoft Copilot demonstrations**, **partner training**, and **AI business value showcases**. This project simulates Fabrikam, a fictional modular home builder, with realistic business operations and AI-powered tools.

## 🎯 Purpose

- **Accelerate Copilot adoption** with realistic SME business scenarios
- **Enable hands-on AI labs** demonstrating tangible business value  
- **Support partner training** with ready-to-deploy environments
- **Showcase Model Context Protocol (MCP)** integration patterns

## ⚡ Quick Start

### **Option 1: Use Deployed Services** (Recommended)
- **API**: https://fabrikam-api-dev-izbd.azurewebsites.net
- **MCP**: https://fabrikam-mcp-dev.levelupcsp.com/mcp
- **Copilot Studio**: See [Setup Guide](Copilot-Studio-Agent-Setup-Guide.md)

### **Option 2: Local Development**
```powershell
# Run API locally
cd FabrikamApi/src
dotnet run

# Run MCP server locally  
cd FabrikamMcp/src
dotnet run
```

## 🏗️ Architecture

Two integrated services providing complete business functionality:

### **FabrikamApi** - Business Operations API
- **Sales Analytics**: Order management and revenue tracking ($829K+ in sample data)
- **Product Catalog**: Modular homes with specifications and inventory tracking
- **Customer Management**: CRM with order history and support integration
- **Asset Management**: Images, blueprints, and marketing materials

### **FabrikamMcp** - AI Integration Server  
- **Natural Language Interface**: Query business data conversationally
- **10 Consolidated Tools**: Optimized for Copilot Studio's 15-tool limit
- **Intelligent Defaults**: Works without parameters for robust AI interactions
- **Real-time Data**: Connects to live API for current business information

## 📊 Business Data

Comprehensive sample data for realistic demonstrations:
- **8 Customers**: Business and individual buyers across regions
- **45 Products**: Modular homes, ADUs, materials with specifications  
- **15 Orders**: $829,482.50 in sample revenue with regional breakdowns
- **20 Support Tickets**: Customer service cases with realistic workflows
- **Asset Library**: House images, blueprints, and marketing materials

## 🧪 Testing Your Setup

### **API Endpoints**
```powershell
# Health check
curl https://fabrikam-api-dev-izbd.azurewebsites.net/health

# Sales analytics  
curl https://fabrikam-api-dev-izbd.azurewebsites.net/api/orders/analytics

# Product inventory
curl https://fabrikam-api-dev-izbd.azurewebsites.net/api/products/inventory
```

### **MCP Integration**
```powershell
# Server status
curl https://fabrikam-mcp-dev.levelupcsp.com/status

# List available tools
curl https://fabrikam-mcp-dev.levelupcsp.com/mcp
```

## 📖 Documentation

### **Essential Guides**
- [**Copilot Studio Setup**](Copilot-Studio-Agent-Setup-Guide.md) - Create your business AI assistant
- [**Asset Management**](FabrikamApi/ASSET-MANAGEMENT-GUIDE.md) - Add images, blueprints, and files
- [**Deployment Guide**](DEPLOYMENT-GUIDE.md) - Azure deployment instructions
- [**Future Enhancements**](TODO-FUTURE-ENHANCEMENTS.md) - Planned features and TODO items

### **Implementation Status**
- ✅ **API Controllers**: All business endpoints implemented and tested
- ✅ **MCP Tools**: 10 consolidated tools optimized for Copilot Studio
- ✅ **Azure Deployment**: Both services deployed and accessible
- ✅ **Asset Structure**: Directory structure ready for house designs and blueprints
- 🔄 **Content Expansion**: Adding product images and architectural drawings

## 🎯 Business Scenarios

Perfect for demonstrating AI value in:
- **Sales Analytics**: "What are our sales numbers?" → Real revenue data with breakdowns
- **Inventory Management**: "What products need restocking?" → Live stock levels and alerts  
- **Customer Service**: "Any urgent support tickets?" → Priority ticket management
- **Product Information**: "Show me our ADU options" → Complete product specifications

## 🤝 Contributing

This project is designed for demonstrations and training. To extend functionality:

1. **Fork the repository** for your own customizations
2. **Follow the asset naming conventions** in the Asset Management Guide
3. **Add your business scenarios** by extending the sample data
4. **Test with Copilot Studio** to ensure AI agent compatibility

---
*Fabrikam Modular Homes - Showcasing AI-powered business operations*
