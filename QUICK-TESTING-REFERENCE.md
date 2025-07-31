# 🎯 Quick Reference: Testing Three Authentication Modes

## 📋 Essential Information for Copilot Studio Testing

### 🔓 **Mode 1: Disabled Authentication**

**Deploy Parameter**: `"authenticationMode": "Disabled"`

**Copilot Studio MCP Connection:**
```yaml
# Add this parameter to your swagger:
parameters:
  - name: X-Tracking-Guid
    in: header
    type: string
    required: false
    description: "Optional GUID for user tracking"

# No security required:
security: []
```

**Test Command:**
```bash
# Should work immediately after deployment
curl -k -X POST https://[YOUR-MCP-URL]/mcp \
  -H "Content-Type: application/json" \
  -d '{"method": "tools/list"}'
```

---

### 🔐 **Mode 2: BearerToken Authentication (JWT)**

**Deploy Parameter**: `"authenticationMode": "BearerToken"`

**Get Demo Credentials:**
```bash
curl -k https://[YOUR-API-URL]/api/auth/demo-credentials
```

**Get JWT Token:**
```bash
curl -k -X POST https://[YOUR-API-URL]/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "lee.gu@fabrikam.levelupcsp.com", "password": "SecurePass123!"}'
```

**Copilot Studio MCP Connection:**
```yaml
# Add this parameter to your swagger:
parameters:
  - name: Authorization
    in: header
    type: string
    required: true
    description: "Bearer token (format: Bearer YOUR_JWT_TOKEN)"

# Add security definition:
securityDefinitions:
  Bearer:
    type: apiKey
    name: Authorization
    in: header

security:
  - Bearer: []
```

---

### 🏢 **Mode 3: EntraExternalId Authentication (OAuth)**

**Deploy Parameters**: 
```json
{
  "authenticationMode": "EntraExternalId",
  "entraExternalIdTenant": "your-tenant.onmicrosoft.com",
  "entraExternalIdClientId": "your-client-id-guid"
}
```

**Prerequisites:**
1. Create Entra External ID tenant
2. Register app with redirect URI: `https://[YOUR-MCP-URL]/signin-oidc`
3. Add API permissions: User.Read, profile, openid, email

**Get OAuth Token:**
- Visit: `https://[YOUR-API-URL]/signin-oidc`
- Login with Entra credentials
- Extract token from browser developer tools

**Copilot Studio MCP Connection:**
```yaml
# Add this parameter to your swagger:
parameters:
  - name: Authorization
    in: header
    type: string
    required: true
    description: "Bearer token from Entra External ID"

# Add OAuth security:
securityDefinitions:
  OAuth2:
    type: oauth2
    authorizationUrl: https://[YOUR-TENANT].onmicrosoft.com/oauth2/v2.0/authorize
    tokenUrl: https://[YOUR-TENANT].onmicrosoft.com/oauth2/v2.0/token
    flow: accessCode

security:
  - OAuth2: [openid, profile, email]
```

---

## 🔧 **Key Points for Testing:**

1. **Base Swagger Structure**: Use your existing swagger as template, just add the authentication parameters above

2. **Demo Users (BearerToken mode)**:
   - `lee.gu@fabrikam.levelupcsp.com` (Sales role)
   - `admin@fabrikam.levelupcsp.com` (Admin role)  
   - `support@fabrikam.levelupcsp.com` (CustomerService role)
   - Password: `SecurePass123!`

3. **Token Formats**:
   - **JWT**: `Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...`
   - **OAuth**: `Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9...`

4. **Quick Health Check**:
   ```bash
   curl -k https://[YOUR-API-URL]/api/info
   ```

## 📚 **Full Documentation**

Complete testing guide: `docs/testing/THREE-MODE-AUTHENTICATION-TESTING-GUIDE.md`

**Ready to test! 🚀**
