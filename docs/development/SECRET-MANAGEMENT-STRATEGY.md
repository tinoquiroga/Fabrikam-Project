# 🔐 Fabrikam Project - Secret Management Strategy

> **Updated**: July 26, 2025  
> **Strategy**: Three-Layer Security Architecture

## 🏗️ **Security Architecture Overview**

Your Fabrikam Project uses a **three-layer secret management strategy** for maximum security and development flexibility:

```
🏢 Production (Azure)     ← Azure Key Vault (fabrikam-dev-kv-4651)
🔒 Development (Local)    ← .NET User Secrets (encrypted, isolated)
⚙️ Team/Docker (Shared)   ← .env file (gitignored, convenient)
```

## 🔑 **Layer 1: Azure Key Vault (Production)**

**Purpose**: Secure cloud storage for production applications  
**Location**: `https://fabrikam-dev-kv-4651.vault.azure.net/`  
**Access**: Managed Identity, RBAC permissions

### **Stored Secrets**

```
ConnectionStrings--DefaultConnection    ← Database connection for Azure apps
JwtSecretKey                           ← JWT signing key for token validation
SqlAdminPassword                       ← Direct SQL admin password backup
```

### **Usage**

- **Azure App Service**: Automatically loads via Key Vault references
- **Azure Container Apps**: Uses managed identity for secret retrieval
- **Production CI/CD**: GitHub Actions pulls secrets during deployment

## 🔒 **Layer 2: .NET User Secrets (Development)**

**Purpose**: Secure local development without file-based secrets  
**Location**: `%APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json`  
**Access**: Current Windows user only, encrypted storage

### **Configuration**

```powershell
# Already configured ✅
dotnet user-secrets init --project FabrikamApi\src\FabrikamApi.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
dotnet user-secrets set "Authentication:AspNetIdentity:Jwt:SecretKey" "..."
```

### **Advantages**

- ✅ **Never committed to git** (completely isolated)
- ✅ **Encrypted by Windows** (secure local storage)
- ✅ **User-specific** (each developer has their own)
- ✅ **IDE integration** (VS/VS Code automatically loads)

## ⚙️ **Layer 3: .env File (Convenience)**

**Purpose**: Team development, Docker containers, quick setup  
**Location**: `c:\Users\davidb\1Repositories\Fabrikam-Project\.env`  
**Access**: Gitignored, manually copied from .env.example

### **Current Configuration**

```bash
# Database Connection
ConnectionStrings__DefaultConnection="Server=fabrikam-dev-sql.database.windows.net;Database=fabrikam-dev-db;User Id=fabrikam-admin;Password=bHrAbu#gv*r3FY#k%GKV;Encrypt=true;"

# JWT Configuration
Authentication__AspNetIdentity__Jwt__SecretKey="vXdmrsIouBpYAy44vk2oF8oVVax08cyM8wS/bRXvLMk="
Authentication__AspNetIdentity__Jwt__Issuer="https://localhost:7297"
Authentication__AspNetIdentity__Jwt__Audience="FabrikamApi"

# Azure Key Vault (reference)
AzureKeyVault__VaultUrl="https://fabrikam-dev-kv-4651.vault.azure.net/"
```

### **Use Cases**

- ✅ **Docker development** (container environments)
- ✅ **Team onboarding** (copy from .env.example)
- ✅ **CI/CD override** (temporary testing environments)
- ✅ **Backup configuration** (when Key Vault unavailable)

## 🔄 **Configuration Priority**

ASP.NET Core loads configuration in this order:

```
1. appsettings.json                    ← Base configuration
2. appsettings.{Environment}.json      ← Environment overrides
3. User Secrets (Development only)     ← 🏆 WINS in Development
4. Environment Variables (.env)        ← Docker/CI environments
5. Azure Key Vault                     ← 🏆 WINS in Production
```

### **Effective Strategy**

- **Local Development**: User Secrets take precedence ✅
- **Docker/CI**: .env file provides fallback ✅
- **Azure Production**: Key Vault provides authoritative values ✅

## 🚀 **Why This Architecture?**

### **Development Benefits**

- **Zero git leaks**: User Secrets never committed
- **Easy onboarding**: Copy .env.example → .env
- **Flexible testing**: Override any secret locally
- **IDE integration**: Automatic secret loading

### **Production Benefits**

- **Azure native**: Key Vault integrates with all Azure services
- **RBAC security**: Role-based access control
- **Audit trail**: Who accessed what secrets when
- **Rotation support**: Update secrets without redeployment

### **Team Benefits**

- **Consistent values**: Everyone uses same Key Vault secrets
- **Environment parity**: Dev matches production configuration
- **Security layering**: Multiple fallback mechanisms
- **Deployment flexibility**: Works across permission scenarios

## ⚡ **Quick Commands**

### **User Secrets Management**

```powershell
# View current secrets
dotnet user-secrets list --project FabrikamApi\src\FabrikamApi.csproj

# Update a secret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "new-value" --project FabrikamApi\src\FabrikamApi.csproj

# Remove a secret
dotnet user-secrets remove "Authentication:AspNetIdentity:Jwt:SecretKey" --project FabrikamApi\src\FabrikamApi.csproj
```

### **Key Vault Access**

```powershell
# List all secrets
az keyvault secret list --vault-name "fabrikam-dev-kv-4651" --query "[].name" --output table

# Get a secret value
az keyvault secret show --vault-name "fabrikam-dev-kv-4651" --name "JwtSecretKey" --query "value" --output tsv

# Update a secret
az keyvault secret set --vault-name "fabrikam-dev-kv-4651" --name "JwtSecretKey" --value "new-secret-value"
```

### **Environment File Management**

```powershell
# Create .env from template
Copy-Item .env.example .env

# Validate .env is gitignored
git status  # Should not show .env file
```

## 🛡️ **Security Best Practices**

### **DO**

- ✅ Use User Secrets for local development
- ✅ Keep .env in .gitignore
- ✅ Rotate secrets periodically via Key Vault
- ✅ Use managed identity in Azure environments
- ✅ Store connection strings with minimal permissions

### **DON'T**

- ❌ Commit .env to git (already gitignored ✅)
- ❌ Share User Secrets files between developers
- ❌ Use production secrets in development environments
- ❌ Store secrets in appsettings.json
- ❌ Use root/admin database accounts in applications

---

## 📝 **Next Steps**

1. **Current Status**: ✅ All three layers configured and working
2. **ASP.NET Identity**: Ready to implement with secure secret access
3. **Authentication Flow**: JWT secrets available in all environments
4. **Database Access**: Connection strings secured and accessible

**Ready to proceed with Issue #4**: ASP.NET Identity database schema implementation! 🚀
