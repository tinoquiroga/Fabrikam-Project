# 🏗️ Separate Authentication Configuration Proposal

## Problem Statement

Currently, both API and MCP services share the same `Authentication` configuration section locally, but have separate environment variables in Azure. This creates inconsistencies and makes it difficult to catch configuration issues during local development.

## Proposed Architecture

### 🎯 Separate Configuration Sections

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

### 🔧 Implementation Changes

#### 1. Configuration Classes
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

#### 2. Service Registration
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

#### 3. Azure Deployment Template Updates
```json
// API App Service Environment Variables
{
  "name": "ApiAuthentication__Mode",
  "value": "[parameters('authenticationMode')]"
},
{
  "name": "ApiAuthentication__Strategy", 
  "value": "[if(variables('isAuthenticationEnabled'), 'AspNetIdentity', 'Disabled')]"
}

// MCP App Service Environment Variables  
{
  "name": "McpAuthentication__Mode",
  "value": "[parameters('authenticationMode')]"
},
{
  "name": "McpAuthentication__Strategy",
  "value": "[if(variables('isAuthenticationEnabled'), 'AspNetIdentity', 'Disabled')]"
}
```

## Benefits

### ✅ **Local Development Consistency**
- API and MCP can have different authentication modes locally
- Matches Azure deployment architecture exactly
- Configuration issues caught during development

### ✅ **Better Testing**
- Test scenarios like "API=BearerToken, MCP=Disabled" locally
- More realistic development environment
- Easier to validate different authentication combinations

### ✅ **Deployment Safety**  
- Configuration template issues caught during local testing
- No more surprises when deploying to Azure
- Clear separation of concerns between services

### ✅ **Flexibility**
- Independent authentication evolution for each service
- Different security requirements for API vs MCP
- Service-specific configuration validation

## Example Scenarios

### Scenario 1: Demo Setup
```json
{
  "ApiAuthentication": { "Mode": "BearerToken" },
  "McpAuthentication": { "Mode": "Disabled" }
}
```

### Scenario 2: Production Setup  
```json
{
  "ApiAuthentication": { "Mode": "BearerToken" },
  "McpAuthentication": { "Mode": "BearerToken" }
}
```

### Scenario 3: Enterprise Setup
```json
{
  "ApiAuthentication": { "Mode": "EntraExternalId" },
  "McpAuthentication": { "Mode": "EntraExternalId" }
}
```

## Migration Strategy

1. **Phase 1**: Add new configuration sections alongside existing ones
2. **Phase 2**: Update service registration to use new sections  
3. **Phase 3**: Update deployment templates
4. **Phase 4**: Remove old shared configuration section

## Implementation Priority

**High Priority**: This change would have caught the current Azure deployment template bug during local development, preventing the production issue we just discovered.
