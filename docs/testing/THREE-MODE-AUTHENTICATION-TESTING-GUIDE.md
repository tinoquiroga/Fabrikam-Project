# 🧪 Three-Mode Authentication Testing Guide

## 🎯 Overview

This guide provides step-by-step instructions for testing all three authentication modes of the Fabrikam Platform after Azure deployment. Each mode requires different configuration in Copilot Studio's MCP-Streamable-HTTP connection.

## 🔓 Mode 1: Disabled Authentication (GUID Tracking)

### 📋 Configuration Summary
- **Security**: GUID-based user tracking only
- **Best for**: Quick demos, POCs, rapid testing
- **Setup time**: ⚡ Instant
- **Prerequisites**: None

### 🚀 Deployment Parameters
When deploying via "Deploy to Azure" button:
```json
{
  "authenticationMode": "Disabled",
  "databaseType": "InMemory",
  "environment": "Development"
}
```

### 🔗 Copilot Studio MCP Connection (Disabled Mode)
```yaml
swagger: '2.0'
info:
  title: Fabrikam MCP Server - Disabled Mode
  description: >-
    Fabrikam MCP Server with GUID tracking (no authentication barriers)
  version: 1.0.0
host: [YOUR-MCP-DEPLOYMENT-URL]  # e.g., fabrikam-mcp-development-abc123.azurewebsites.net
basePath: /mcp
schemes:
  - https
consumes: []
produces: []
paths:
  /:
    post:
      summary: MCP Server Streamable HTTP
      x-ms-agentic-protocol: mcp-streamable-1.0
      operationId: InvokeMCP
      parameters:
        - name: X-Tracking-Guid
          in: header
          type: string
          required: false
          description: "Optional GUID for user tracking (auto-generated if not provided)"
      responses:
        '200':
          description: Success
definitions: {}
parameters: {}
responses: {}
securityDefinitions: {}
security: []
tags: []
```

### ✅ Testing Steps
1. **Deploy to Azure** with `authenticationMode: "Disabled"`
2. **Get deployment URLs** from Azure portal outputs
3. **Configure Copilot Studio** with the above swagger
4. **Test MCP tools** - they should work immediately without any authentication
5. **Optional**: Add `X-Tracking-Guid` header with a UUID for user tracking

### 🧪 Verification Commands
```bash
# Test API directly (should work without auth)
curl -k https://[YOUR-API-URL]/api/customers

# Test MCP endpoint
curl -k -X POST https://[YOUR-MCP-URL]/mcp \
  -H "Content-Type: application/json" \
  -H "X-Tracking-Guid: 12345678-1234-1234-1234-123456789abc" \
  -d '{"method": "tools/list"}'
```

---

## 🔐 Mode 2: BearerToken Authentication (JWT)

### 📋 Configuration Summary
- **Security**: JWT tokens with Key Vault secrets
- **Best for**: Production APIs, secure demos
- **Setup time**: 🔧 5 minutes
- **Prerequisites**: None (demo users automatically created)

### 🚀 Deployment Parameters
When deploying via "Deploy to Azure" button:
```json
{
  "authenticationMode": "BearerToken",
  "databaseType": "InMemory",
  "environment": "Development"
}
```

### 🔑 Getting Demo Credentials
After deployment, get demo credentials from your API:

```bash
# Get demo credentials
curl -k https://[YOUR-API-URL]/api/auth/demo-credentials
```

**Example Response:**
```json
{
  "message": "Demo credentials for testing",
  "users": [
    {
      "email": "admin@fabrikam.levelupcsp.com",
      "password": "SecurePass123!",
      "roles": ["Admin"]
    },
    {
      "email": "lee.gu@fabrikam.levelupcsp.com", 
      "password": "SecurePass123!",
      "roles": ["Sales"]
    },
    {
      "email": "support@fabrikam.levelupcsp.com",
      "password": "SecurePass123!", 
      "roles": ["CustomerService"]
    }
  ]
}
```

### 🔗 Copilot Studio MCP Connection (BearerToken Mode)
```yaml
swagger: '2.0'
info:
  title: Fabrikam MCP Server - JWT Mode
  description: >-
    Fabrikam MCP Server with JWT Bearer Token authentication
  version: 1.0.0
host: [YOUR-MCP-DEPLOYMENT-URL]  # e.g., fabrikam-mcp-development-abc123.azurewebsites.net
basePath: /mcp
schemes:
  - https
consumes: []
produces: []
paths:
  /:
    post:
      summary: MCP Server Streamable HTTP
      x-ms-agentic-protocol: mcp-streamable-1.0
      operationId: InvokeMCP
      parameters:
        - name: Authorization
          in: header
          type: string
          required: true
          description: "Bearer token (format: Bearer YOUR_JWT_TOKEN)"
      responses:
        '200':
          description: Success
        '401':
          description: Unauthorized - JWT token required
definitions: {}
parameters: {}
responses: {}
securityDefinitions:
  Bearer:
    type: apiKey
    name: Authorization
    in: header
    description: "JWT Bearer Token (format: Bearer YOUR_JWT_TOKEN)"
security:
  - Bearer: []
tags: []
```

### ✅ Testing Steps
1. **Deploy to Azure** with `authenticationMode: "BearerToken"`
2. **Get demo credentials** using the API endpoint above
3. **Get JWT token**:
   ```bash
   curl -k -X POST https://[YOUR-API-URL]/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "lee.gu@fabrikam.levelupcsp.com",
       "password": "SecurePass123!"
     }'
   ```
4. **Copy the JWT token** from the response
5. **Configure Copilot Studio** with the above swagger
6. **Add Authorization header** in Copilot Studio: `Bearer YOUR_JWT_TOKEN`
7. **Test MCP tools** - they should work with the JWT token

### 🧪 Verification Commands
```bash
# Login and get JWT token
TOKEN=$(curl -k -X POST https://[YOUR-API-URL]/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "lee.gu@fabrikam.levelupcsp.com", "password": "SecurePass123!"}' | \
  jq -r '.token')

# Test API with JWT token
curl -k https://[YOUR-API-URL]/api/customers \
  -H "Authorization: Bearer $TOKEN"

# Test MCP with JWT token
curl -k -X POST https://[YOUR-MCP-URL]/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"method": "tools/list"}'
```

### 🔄 Token Refresh
JWT tokens expire after 15 minutes. To refresh:
```bash
# Refresh token
curl -k -X POST https://[YOUR-API-URL]/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "YOUR_REFRESH_TOKEN"}'
```

---

## 🏢 Mode 3: EntraExternalId Authentication (OAuth 2.0)

### 📋 Configuration Summary
- **Security**: OAuth 2.0 with Microsoft Entra External ID
- **Best for**: Enterprise integration, SSO scenarios
- **Setup time**: 🏢 15-30 minutes
- **Prerequisites**: Entra External ID tenant and app registration

### 🚀 Deployment Parameters
When deploying via "Deploy to Azure" button:
```json
{
  "authenticationMode": "EntraExternalId",
  "databaseType": "SqlServer",
  "environment": "Production",
  "entraExternalIdTenant": "your-tenant.onmicrosoft.com",
  "entraExternalIdClientId": "your-client-id-guid"
}
```

### 🏢 Prerequisites Setup

#### 1. Create Entra External ID Tenant
1. Go to [Azure Portal](https://portal.azure.com)
2. Create new **Entra External ID** tenant
3. Note your tenant domain (e.g., `contoso.onmicrosoft.com`)

#### 2. Register Application
1. In your Entra External ID tenant, go to **App registrations**
2. Create **New registration**:
   - Name: `Fabrikam MCP Client`
   - Account types: `Accounts in this organizational directory only`
   - Redirect URI: `https://[YOUR-MCP-URL]/signin-oidc`
3. Note the **Application (client) ID**
4. Go to **Authentication** → Add platform → **Web**
5. Add redirect URIs:
   - `https://[YOUR-API-URL]/signin-oidc`
   - `https://[YOUR-MCP-URL]/signin-oidc`

#### 3. Configure API Permissions
1. Go to **API permissions**
2. Add **Microsoft Graph** permissions:
   - `User.Read`
   - `profile`
   - `openid`
   - `email`

### 🔗 Copilot Studio MCP Connection (EntraExternalId Mode)
```yaml
swagger: '2.0'
info:
  title: Fabrikam MCP Server - OAuth Mode
  description: >-
    Fabrikam MCP Server with OAuth 2.0 Entra External ID authentication
  version: 1.0.0
host: [YOUR-MCP-DEPLOYMENT-URL]  # e.g., fabrikam-mcp-development-abc123.azurewebsites.net
basePath: /mcp
schemes:
  - https
consumes: []
produces: []
paths:
  /:
    post:
      summary: MCP Server Streamable HTTP
      x-ms-agentic-protocol: mcp-streamable-1.0
      operationId: InvokeMCP
      parameters:
        - name: Authorization
          in: header
          type: string
          required: true
          description: "Bearer token from Entra External ID (format: Bearer YOUR_OAUTH_TOKEN)"
      responses:
        '200':
          description: Success
        '401':
          description: Unauthorized - OAuth token required
definitions: {}
parameters: {}
responses: {}
securityDefinitions:
  OAuth2:
    type: oauth2
    authorizationUrl: https://[YOUR-TENANT].onmicrosoft.com/oauth2/v2.0/authorize
    tokenUrl: https://[YOUR-TENANT].onmicrosoft.com/oauth2/v2.0/token
    flow: accessCode
    scopes:
      openid: OpenID Connect
      profile: User profile
      email: User email
security:
  - OAuth2: [openid, profile, email]
tags: []
```

### ✅ Testing Steps
1. **Set up Entra External ID** tenant and app registration (see prerequisites)
2. **Deploy to Azure** with `authenticationMode: "EntraExternalId"`
3. **Create test users** in your Entra External ID tenant
4. **Get OAuth token** via browser flow:
   - Visit: `https://[YOUR-API-URL]/signin-oidc`
   - Login with Entra External ID credentials
   - Extract token from browser developer tools or response
5. **Configure Copilot Studio** with the above swagger
6. **Add Authorization header** with OAuth token
7. **Test MCP tools** with enterprise authentication

### 🧪 Verification Commands
```bash
# Test OAuth login redirect
curl -k -L https://[YOUR-API-URL]/signin-oidc

# Test MCP with OAuth token (token obtained via browser flow)
curl -k -X POST https://[YOUR-MCP-URL]/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OAUTH_TOKEN" \
  -d '{"method": "tools/list"}'
```

---

## 📊 Quick Reference Comparison

| Feature | Disabled | BearerToken | EntraExternalId |
|---------|----------|-------------|-----------------|
| **Copilot Studio Header** | `X-Tracking-Guid: [UUID]` | `Authorization: Bearer [JWT]` | `Authorization: Bearer [OAuth]` |
| **Token Endpoint** | None | `/api/auth/login` | Browser OAuth flow |
| **Demo Credentials** | None needed | `/api/auth/demo-credentials` | Create in Entra tenant |
| **Token Lifespan** | N/A | 15 minutes | OAuth standard |
| **Refresh Method** | N/A | `/api/auth/refresh` | OAuth refresh flow |

## 🔧 Troubleshooting Tips

### Common Issues:
1. **401 Unauthorized**: Check token format and expiration
2. **404 Not Found**: Verify deployment URLs are correct
3. **CORS Issues**: Ensure proper redirect URIs in Entra app registration
4. **Token Expired**: Use refresh endpoints or re-authenticate

### Debug Commands:
```bash
# Check API health
curl -k https://[YOUR-API-URL]/api/info

# Check authentication mode
curl -k https://[YOUR-API-URL]/api/auth/info

# Test MCP health
curl -k https://[YOUR-MCP-URL]/health
```

## 📚 Additional Resources
- [Azure Deployment Guide](../deployment/DEPLOY-TO-AZURE.md)
- [Authentication Architecture](../development/AUTHENTICATION-IMPLEMENTATION-GUIDE.md)
- [Copilot Studio MCP Documentation](https://docs.microsoft.com/copilot-studio/mcp)

---

**Happy Testing! 🎉**

*This guide covers all three authentication modes. Choose the mode that best fits your testing scenario and follow the respective configuration steps.*
