# 🔐 JWT Security Strategy

## Overview

This document explains the multi-layered JWT secret management strategy for different environments, balancing security with developer productivity.

## 🎯 Security Strategy Summary

| Environment | Secret Source | Security Level | Use Case |
|-------------|---------------|----------------|----------|
| **Local Development** | `appsettings.Development.json` | Low (Known Secret) | Quick local testing |
| **Shared Development** | `.env` file | Medium (Real Secret) | Team collaboration with real API |
| **Azure/Production** | Azure Key Vault | High (Managed Secret) | Production deployment |

## 🔧 Configuration Hierarchy

The application loads JWT secrets in this order:

1. **Environment Variables** (highest priority)
2. **`.env` file** (loaded by DotNetEnv)
3. **appsettings.Development.json** (fallback for local dev)
4. **Azure Key Vault** (production)

## 📁 Local Development Setup

### Option 1: Quick Testing (Development Secret)
```json
// appsettings.Development.json (already configured)
{
  "Authentication": {
    "AspNetIdentity": {
      "Jwt": {
        "SecretKey": "development-jwt-secret-key-for-local-testing-only-not-for-production-use"
      }
    }
  }
}
```

**✅ Pros:** 
- Works immediately
- No setup required
- Safe for source control

**⚠️ Cons:** 
- Not suitable for integration with live APIs
- Cannot authenticate with deployed instances

### Option 2: Real Secret (.env file)
```bash
# Create .env file in project root (NOT committed to git)
Authentication__AspNetIdentity__Jwt__SecretKey="your-real-jwt-secret-here"
```

**✅ Pros:** 
- Works with deployed APIs
- Real production-like authentication
- Secure (not in source control)

**⚠️ Setup Required:**
1. Copy `.env.example` to `.env`
2. Get real JWT secret from team/Azure Key Vault
3. Fill in actual values

## 🏗️ Project Configuration

### .env File Loading (Program.cs)
```csharp
// Load .env file if it exists (for local development with real secrets)
if (File.Exists(".env"))
{
    Env.Load();
}
```

### Configuration Priority
```csharp
// 1. Environment variables override everything
// 2. .env file values loaded into environment
// 3. appsettings.Development.json as fallback
// 4. Azure Key Vault in production
```

## 🚫 Security Best Practices

### ❌ NEVER Do This:
```json
// DON'T: Real secrets in source control
{
  "Jwt": {
    "SecretKey": "real-production-secret-abc123" // ❌ Never commit real secrets
  }
}
```

### ✅ ALWAYS Do This:
```json
// DO: Development-only secrets in appsettings.Development.json
{
  "Jwt": {
    "SecretKey": "development-jwt-secret-key-for-local-testing-only-not-for-production-use"
  }
}
```

```bash
# DO: Real secrets in .env file (gitignored)
Authentication__AspNetIdentity__Jwt__SecretKey="real-secret-from-keyvault"
```

## 🔄 Environment-Specific Secrets

### Development Environment
- **Source**: `appsettings.Development.json`
- **Secret**: Known development key
- **Purpose**: Local testing, unit tests, quick demos

### Shared Development (.env)
- **Source**: `.env` file
- **Secret**: Real JWT secret from team
- **Purpose**: Integration testing with deployed APIs

### Azure Production
- **Source**: Azure Key Vault
- **Secret**: Managed by Azure, auto-rotated
- **Purpose**: Production security

## 📋 Setup Instructions

### For New Developers (Quick Start):
1. Clone repository
2. Run `dotnet run --project FabrikamApi\src\FabrikamApi.csproj`
3. ✅ Development secret works automatically

### For Team Integration:
1. Copy `.env.example` to `.env`
2. Get real JWT secret from:
   - Team lead
   - Azure Key Vault
   - Secure team documentation
3. Paste secret into `.env` file
4. ✅ Now works with deployed APIs

### For Production Deployment:
1. Store JWT secret in Azure Key Vault
2. Configure App Service to load from Key Vault
3. ✅ Managed security with auto-rotation

## 🔍 Verification

### Check Current Configuration:
```bash
# Test which secret is being used
curl -X POST "https://localhost:7297/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"lee.gu@fabrikam.levelupcsp.com","password":"AdminzpYZVP0!"}'
```

### Development Secret (works locally only):
- ✅ Login succeeds
- ❌ Token won't work with deployed APIs

### Real Secret (works everywhere):
- ✅ Login succeeds  
- ✅ Token works with deployed APIs
- ✅ Can authenticate across environments

## 🔧 Troubleshooting

### "JWT SecretKey is not configured" Error:
1. Check `appsettings.Development.json` has `SecretKey`
2. Verify `.env` file format (use `__` for nested config)
3. Ensure secret is at least 32 characters

### Token Rejected by API:
1. Verify using same JWT secret as API
2. Check token expiration
3. Confirm audience/issuer settings match

### .env File Not Loading:
1. Verify file exists in project root
2. Check file encoding (UTF-8)
3. Ensure no BOM (Byte Order Mark)

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Integration](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [JWT Security Best Practices](https://tools.ietf.org/html/rfc7519)

---

**Remember**: The goal is secure production deployment while maintaining developer productivity. Choose the right secret source for your current task!
