## 🎯 **Problem Statement**

Currently, both API and MCP services share the same `Authentication` configuration section locally, but have separate environment variables in Azure. This creates inconsistencies and makes it difficult to catch configuration issues during local development.

### **Current Issue Example**
- Local: Both services read from shared `Authentication` section
- Azure: Services have separate environment variables (`Authentication__Mode`)
- **Result**: MCP deployment template was missing authentication config, only discovered in production

## 🏗️ **Proposed Architecture**

### **Separate Configuration Sections**
```json
{
  "ApiAuthentication": {
    "Mode": "BearerToken",
    "Strategy": "AspNetIdentity", 
    "AspNetIdentity": {
      "Jwt": {
        "SecretKey": "...",
        "Issuer": "https://localhost:7297",
        "Audience": "FabrikamApi"
      }
    }
  },
  "McpAuthentication": {
    "Mode": "Disabled",
    "Strategy": "Disabled",
    "GuidValidation": {
      "Enabled": true,
      "ValidateInDatabase": false
    },
    "ServiceJwt": {
      "SecretKey": "...",
      "Issuer": "https://localhost:7297"
    }
  }
}
```

## ✅ **Benefits**

### **🔧 Local Development Consistency**
- API and MCP can have different authentication modes locally
- Matches Azure deployment architecture exactly
- Configuration issues caught during development

### **🧪 Better Testing**
- Test scenarios like "API=BearerToken, MCP=Disabled" locally
- More realistic development environment
- Easier to validate different authentication combinations

### **🚀 Deployment Safety**
- Configuration template issues caught during local testing
- No more surprises when deploying to Azure
- Clear separation of concerns between services

### **🔄 Flexibility**
- Independent authentication evolution for each service
- Different security requirements for API vs MCP
- Service-specific configuration validation

## 🛠️ **Implementation Plan**

### **Phase 1: Configuration Classes**
```csharp
// New: Separate configuration classes
public class ApiAuthenticationSettings : AuthenticationSettings
{
    public const string SectionName = "ApiAuthentication";
}

public class McpAuthenticationSettings : AuthenticationSettings  
{
    public const string SectionName = "McpAuthentication";
}
```

### **Phase 2: Service Registration Updates**
```csharp
// FabrikamApi/Program.cs
var apiAuthSettings = builder.Configuration
    .GetSection(ApiAuthenticationSettings.SectionName)
    .Get<ApiAuthenticationSettings>();

// FabrikamMcp/Program.cs  
var mcpAuthSettings = builder.Configuration
    .GetSection(McpAuthenticationSettings.SectionName)
    .Get<McpAuthenticationSettings>();
```

### **Phase 3: Azure Deployment Template Updates**
```json
// API App Service Environment Variables
{
  "name": "ApiAuthentication__Mode",
  "value": "[parameters('authenticationMode')]"
},

// MCP App Service Environment Variables  
{
  "name": "McpAuthentication__Mode",
  "value": "[parameters('authenticationMode')]"
}
```

### **Phase 4: Configuration File Updates**
Update `appsettings.json`, `appsettings.Development.json` in both projects

## 📊 **Example Scenarios**

### **Demo Setup**
```json
{
  "ApiAuthentication": { "Mode": "BearerToken" },
  "McpAuthentication": { "Mode": "Disabled" }
}
```

### **Production Setup**  
```json
{
  "ApiAuthentication": { "Mode": "BearerToken" },
  "McpAuthentication": { "Mode": "BearerToken" }
}
```

### **Enterprise Setup**
```json
{
  "ApiAuthentication": { "Mode": "EntraExternalId" },
  "McpAuthentication": { "Mode": "EntraExternalId" }
}
```

## 🎯 **Success Criteria**

- [ ] Local development matches Azure deployment architecture
- [ ] Can test different authentication modes per service locally
- [ ] Configuration template issues caught during development
- [ ] Clear separation between API and MCP authentication settings
- [ ] Backward compatibility maintained during transition

## 🔗 **Related Issues**

This improvement would have prevented the deployment template issue where MCP services were missing authentication environment variables, requiring manual Azure configuration fixes.

---

**Priority**: High - This architectural improvement prevents production configuration issues and improves development workflow consistency.
