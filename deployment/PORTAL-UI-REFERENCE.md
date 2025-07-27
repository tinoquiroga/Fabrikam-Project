# 🎯 Azure Portal CI/CD Quick Reference

## 📋 **Exact UI Values for Copy-Paste**

### **API Service Configuration**
```
Source: GitHub
Building with: GitHub Actions (default)
Organization: [your-github-username]
Repository: Fabrikam-Project
Branch: main (or feature/phase-1-authentication for testing)
Workflow Option: Add a workflow
Runtime Stack: .NET (default)
Runtime Version: 9.0 (default)
Authentication Type: User-assigned identity (default)
Subscription: [your-subscription-name]
Identity: (new) oidc-msi-xxxx (auto-generated)
```

### **MCP Service Configuration**  
```
Source: GitHub
Building with: GitHub Actions (default)
Organization: [your-github-username]
Repository: Fabrikam-Project
Branch: main (same as API)
Workflow Option: Add a workflow
Runtime Stack: .NET (default)
Runtime Version: 9.0 (default)
Authentication Type: User-assigned identity (default)
Subscription: [your-subscription-name]
Identity: (new) oidc-msi-yyyy (different from API)
```

## 🔍 **What Azure Creates**

### **GitHub Secrets (Automatic)**
```
AZUREAPPSERVICE_CLIENTID_[random-id]
AZUREAPPSERVICE_TENANTID_[random-id]  
AZUREAPPSERVICE_SUBSCRIPTIONID_[random-id]
```

### **Workflow Files (Automatic)**
```
.github/workflows/main_fabrikam-api-dev-nf66.yml
.github/workflows/main_fabrikam-mcp-dev-nf66.yml
```

### **User-Assigned Identities (Automatic)**
```
oidc-msi-877e (API service)
oidc-msi-xyz9 (MCP service)
```

## ⚠️ **Common UI Gotchas**

### **Repository Selection**
- ✅ **Correct**: Select `Fabrikam-Project` from dropdown
- ❌ **Wrong**: Typing repository name manually may not work

### **Branch Selection**  
- ✅ **For Demo Users**: Always use `main` branch
- ✅ **For Testing**: Use your current feature branch
- ❌ **Wrong**: Mixing branches between API and MCP services

### **Authentication Type**
- ✅ **Recommended**: User-assigned identity (modern approach)
- ⚡ **Alternative**: Basic authentication (CIPP style, if identity fails)
- ❌ **Avoid**: System-assigned identity (less flexible)

### **Identity Naming**
- ✅ **Default**: Let Azure auto-generate (`oidc-msi-xxxx`)
- ⚡ **Advanced**: Pre-create with consistent naming
- ❌ **Don't**: Try to manually type identity names

## 🎯 **Success Indicators**

### **✅ Deployment Center Shows**
```
Status: Connected
Source: GitHub  
Repository: davebirr/Fabrikam-Project
Branch: main
Last deployment: [timestamp]
```

### **✅ GitHub Actions Shows**
```
✅ Build and deploy .NET Core app to Azure Web App - fabrikam-api-dev-nf66
✅ Build and deploy .NET Core app to Azure Web App - fabrikam-mcp-dev-nf66
```

### **✅ Azure Identity Shows**
```
Resource Group: rg-fabrikam-test
Identity Name: oidc-msi-877e
Status: Active
Associated Resources: 1 (your app service)
```

## 🚨 **Troubleshooting Values**

### **If Repository Not Found**
- Check GitHub authorization in Azure Portal
- Verify you have admin access to the repository
- Try disconnecting and reconnecting GitHub

### **If Branch Not Listed**
- Ensure branch exists and has recent commits
- Try refreshing the browser page
- Verify repository permissions

### **If Identity Creation Fails**
- Check subscription permissions (need Contributor)
- Try in different region if current region restricted
- Fall back to Basic authentication

## 📝 **Copy-Paste Templates**

### **For Documentation**
```markdown
1. Source: **GitHub**
2. Repository: **Fabrikam-Project**  
3. Branch: **main**
4. Runtime: **.NET 9.0**
5. Auth: **User-assigned identity**
6. Click **Save**
```

### **For Support/Issues**
```
Environment: Azure Portal Deployment Center
Subscription: [subscription-name]
Resource Group: rg-fabrikam-[suffix]
App Service: fabrikam-api-dev-[suffix]
Authentication: User-assigned identity  
Identity: oidc-msi-[random]
Error: [error-message]
```

This reference card ensures consistent setup across all demo instances and provides troubleshooting context when things don't work as expected.
