# Launch Settings Templates

These are the standardized `launchSettings.json` files for Fabrikam Project instances.

## 🚀 **API Server Launch Settings**

**Location**: `FabrikamApi/src/Properties/launchSettings.json`

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:7296",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7297;http://localhost:7296",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## 🤖 **MCP Server Launch Settings**

**Location**: `FabrikamMcp/src/Properties/launchSettings.json`

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## 🔧 **Configuration Alignment**

These launch settings align with:

- **VS Code settings.json**: `apiUrl: "https://localhost:7297"`, `mcpUrl: "http://localhost:5000"`
- **Test-Development.ps1**: Uses same URLs for health checks
- **MCP appsettings.Development.json**: `BaseUrl: "https://localhost:7297"`

## 🎯 **URL Standards**

| Service | HTTP | HTTPS | Used By |
|---------|------|-------|---------|
| API Server | `http://localhost:7296` | `https://localhost:7297` | VS Code tasks use HTTPS profile |
| MCP Server | `http://localhost:5000` | - | VS Code tasks use HTTP profile |

**Note**: API server supports both HTTP and HTTPS, but HTTPS is used by default for security. MCP server uses HTTP only for simplicity in development.
