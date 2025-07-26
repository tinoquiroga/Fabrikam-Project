# Repository Configuration Guide for Multi-Instance Deployments

This guide explains how to configure repository variables for different production instances of the Fabrikam Project.

## 🎯 **Overview**

Each fork/instance of this project should configure these repository variables to avoid hard-coded domain names:

### **GitHub Repository Variables** (Settings → Secrets and Variables → Actions → Variables)

| Variable Name           | Description                       | Example Value                     |
| ----------------------- | --------------------------------- | --------------------------------- |
| `API_DOMAIN`            | Custom domain for API server      | `fabrikam-api-dev.levelupcsp.com` |
| `MCP_DOMAIN`            | Custom domain for MCP server      | `fabrikam-mcp-dev.levelupcsp.com` |
| `AZURE_API_APP_NAME`    | Azure App Service name for API    | `fabrikam-api-dev-izbD`           |
| `AZURE_MCP_APP_NAME`    | Azure App Service name for MCP    | `fabrikam-mcp-dev-izbD`           |
| `AZURE_RESOURCE_GROUP`  | Azure Resource Group name         | `rg-fabrikam-dev-izbd`            |
| `PROJECT_INSTANCE_NAME` | Instance identifier for this fork | `levelup-demo`                    |

### **Example Values for Different Instances**

#### **Instance 1: Level Up CSP Demo**

```
API_DOMAIN=fabrikam-api-dev.levelupcsp.com
MCP_DOMAIN=fabrikam-mcp-dev.levelupcsp.com
PROJECT_INSTANCE_NAME=levelup-demo
```

#### **Instance 2: Customer A**

```
API_DOMAIN=fabrikam-api.customer-a.com
MCP_DOMAIN=fabrikam-mcp.customer-a.com
PROJECT_INSTANCE_NAME=customer-a
```

#### **Instance 3: Customer B**

```
API_DOMAIN=api.customer-b-fabrikam.net
MCP_DOMAIN=mcp.customer-b-fabrikam.net
PROJECT_INSTANCE_NAME=customer-b
```

## 🔧 **How to Configure for Your Fork**

1. **Fork the Repository**
2. **Go to Repository Settings** → Secrets and Variables → Actions → Variables tab
3. **Add the required variables** with your custom values
4. **Update any remaining hard-coded references** in documentation files

## 📁 **Files That Use These Variables**

### **Automatically Updated by CI/CD:**

- GitHub Actions workflows (`.github/workflows/*.yml`)
- VS Code REST Client settings (`.vscode/settings.json`)
- MCP server configuration (`FabrikamMcp/src/appsettings*.json`)
- Testing scripts (`Test-Development.ps1`)

### **Manually Update in Documentation:**

- README files
- Deployment guides
- Demo setup guides

## 🚀 **Benefits**

- ✅ **No hard-coded domains** - Each fork configures its own
- ✅ **Single point of truth** - Repository variables control everything
- ✅ **Easy deployment** - Just set variables and deploy
- ✅ **Instance isolation** - Each fork is completely independent
- ✅ **Professional domains** - Use any domain pattern you want

## ⚙️ **Setting Up Repository Variables**

### **Via GitHub UI:**

1. Go to your forked repository
2. Settings → Secrets and Variables → Actions
3. Click "Variables" tab
4. Click "New repository variable"
5. Add each variable name and value

### **Via GitHub CLI:**

```bash
gh variable set API_DOMAIN --body "your-api-domain.com"
gh variable set MCP_DOMAIN --body "your-mcp-domain.com"
gh variable set AZURE_API_APP_NAME --body "your-api-app-name"
gh variable set AZURE_MCP_APP_NAME --body "your-mcp-app-name"
gh variable set AZURE_RESOURCE_GROUP --body "your-resource-group"
gh variable set PROJECT_INSTANCE_NAME --body "your-instance-name"
```

## 🔄 **Migration from Hard-Coded Values**

If you're updating an existing instance:

1. **Set the repository variables** as shown above
2. **Re-run the GitHub Actions** to deploy with new configuration
3. **Update any documentation** with your specific domains
4. **Test all endpoints** to ensure they work with new domains

---

**Note**: This configuration makes the project truly multi-tenant ready. Each fork becomes an independent instance with its own domain configuration.
