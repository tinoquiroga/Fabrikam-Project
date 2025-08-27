# 🔐 Dual Authentication Strategy - Fabrikam Project

> **Project**: Fabrikam Project Phase 1 Authentication  
> **Issue**: [GitHub Issue #3](https://github.com/davebirr/Fabrikam-Project/issues/3)  
> **Updated**: July 26, 2025 - Dual Strategy Implementation

## 📋 Overview

The Fabrikam Project implements a **dual authentication strategy** to accommodate different deployment scenarios and organizational constraints. This approach provides flexibility for various customer environments while maintaining security best practices.

## 🎯 Authentication Strategy Selection

### **Strategy 1: Microsoft Entra External ID (Preferred)**

**When to use**: Global Admin or appropriate Entra permissions available

**Benefits**:

- ✅ Modern Microsoft identity platform
- ✅ Future-proof (replaces Azure AD B2C)
- ✅ Enhanced security features
- ✅ Better integration with Microsoft ecosystem
- ✅ Advanced conditional access policies

**Requirements**:

- Global Administrator role in Entra tenant
- Or custom role with External ID permissions
- Tenant-level permissions to create External ID tenants

### **Strategy 2: ASP.NET Core Identity + JWT (Fallback)**

**When to use**: Limited Entra permissions or air-gapped environments

**Benefits**:

- ✅ No external tenant dependencies
- ✅ Full control over user management
- ✅ Works in any Azure subscription
- ✅ Simplified deployment process
- ✅ Cost-effective for small deployments

**Trade-offs**:

- ⚠️ Manual user management required
- ⚠️ Additional security implementation needed
- ⚠️ Less integration with Microsoft services

## 🏗️ Architecture Decision Framework

### **Runtime Strategy Detection**

The application will automatically detect which authentication strategy to use based on:

1. **Configuration Priority**:

   ```json
   {
     "Authentication": {
       "Strategy": "Auto", // Auto, EntraExternalId, AspNetIdentity
       "PreferredStrategy": "EntraExternalId",
       "FallbackStrategy": "AspNetIdentity"
     }
   }
   ```

2. **Auto-Detection Logic**:

   - Check for Entra External ID configuration
   - Validate tenant permissions
   - Fall back to ASP.NET Identity if Entra unavailable

3. **Manual Override**: Environment variable or configuration setting

### **Strategy Implementation**

Both strategies implement the same `IAuthenticationService` interface:

```csharp
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserInfo> GetUserInfoAsync(string userId);
    Task<bool> LogoutAsync(string userId);
    string GetLoginUrl(string returnUrl = null);
}
```

## 🔧 Strategy 1: Microsoft Entra External ID Implementation

### **Prerequisites**

- **Tenant Role**: Global Administrator or custom External ID role
- **Subscription**: Any Azure subscription for resource hosting
- **Resource Group**: `rg-fabrikam-dev` (instance-specific)

### **Setup Process**

#### **1. Create Entra External ID Tenant**

```bash
# Check current permissions
az ad signed-in-user show --query "assignedRoles"

# Create External ID tenant (requires Global Admin)
az rest --method POST --url "https://graph.microsoft.com/v1.0/tenantManagement/tenants" \
  --body '{
    "displayName": "Fabrikam External Users",
    "defaultDomainName": "fabrikam-external-users"
  }'
```

#### **2. Configure User Flows**

- **Sign-up/Sign-in**: Email verification required
- **Password Reset**: Self-service enabled
- **Profile Management**: Limited to essential fields

#### **3. Application Registration**

```json
{
  "name": "Fabrikam-API-External",
  "redirectUris": [
    "https://localhost:7297/signin-oidc",
    "https://fabrikam-api-dev.levelupcsp.com/signin-oidc"
  ],
  "scopes": ["User.Read", "Profile"],
  "tokenVersion": 2
}
```

### **Configuration Template**

```json
{
  "Authentication": {
    "Strategy": "EntraExternalId",
    "EntraExternalId": {
      "TenantId": "[External-ID-Tenant-ID]",
      "TenantDomain": "fabrikam-external-users.onmicrosoft.com",
      "ClientId": "[App-Registration-Client-ID]",
      "ClientSecret": "[Client-Secret]",
      "Instance": "https://login.microsoftonline.com/",
      "CallbackPath": "/signin-oidc",
      "SignedOutCallbackPath": "/signout-oidc"
    }
  }
}
```

## 🔧 Strategy 2: ASP.NET Core Identity Implementation

### **Prerequisites**

- **Subscription**: Any Azure subscription
- **Database**: Azure SQL Database or SQL Server
- **Resource Group**: `rg-fabrikam-dev`
- **Resource Provider**: Microsoft.Sql (for Azure SQL Database creation)

### **Database Schema**

```sql
-- Users table (extends AspNetUsers)
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,

    -- Fabrikam-specific fields
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Role NVARCHAR(50) NOT NULL DEFAULT 'ReadOnly',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Roles table
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256),
    NormalizedName NVARCHAR(256),
    ConcurrencyStamp NVARCHAR(MAX)
);

-- Insert default roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES
('1', 'ReadOnly', 'READONLY'),
('2', 'ReadWrite', 'READWRITE'),
('3', 'Admin', 'ADMIN');
```

### **JWT Configuration**

```json
{
  "Authentication": {
    "Strategy": "AspNetIdentity",
    "AspNetIdentity": {
      "Jwt": {
        "SecretKey": "[JWT-Secret-Key-256-bit]",
        "Issuer": "https://fabrikam-api-dev.levelupcsp.com",
        "Audience": "fabrikam-api",
        "ExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
      },
      "Password": {
        "RequiredLength": 8,
        "RequireDigit": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": true,
        "MaxFailedAttempts": 5,
        "LockoutDuration": "00:15:00"
      }
    }
  }
}
```

### **Implementation Components**

#### **1. Authentication Controller**

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _authService.AuthenticateAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result.Error);
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        // Implementation based on active strategy
    }
}
```

#### **2. JWT Token Service**

```csharp
public class JwtTokenService : ITokenService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

## 🔄 Implementation Roadmap

### **Phase 1: Infrastructure Setup (Current)**

#### **Step 1: Permission Assessment**

```powershell
# Check Entra permissions
.\scripts\Check-EntraPermissions.ps1

# Results determine strategy selection
```

#### **Step 2: Strategy Selection**

- **If Entra permissions available**: Proceed with Strategy 1
- **If limited permissions**: Implement Strategy 2
- **If uncertain**: Implement both with auto-detection

#### **Step 3: Database Schema (Common)**

- Create user management tables
- Implement role-based access control
- Add audit logging

### **Phase 2: Authentication Implementation**

#### **Strategy 1 Path: Entra External ID**

1. Create External ID tenant
2. Configure user flows
3. Register application
4. Implement OIDC integration
5. Test with external users

#### **Strategy 2 Path: ASP.NET Identity**

1. Configure Identity services
2. Implement JWT token generation
3. Create authentication endpoints
4. Add password policies
5. Implement user management

### **Phase 3: Integration & Testing**

1. Unified authentication interface
2. Role-based API authorization
3. End-to-end testing
4. Security validation
5. Performance optimization

## 📁 Multi-Instance Configuration

### **Instance-Specific Resource Groups**

Each deployment instance uses its own resource group:

- **Development**: `rg-fabrikam-dev`
- **Staging**: `rg-fabrikam-staging`
- **Production**: `rg-fabrikam-prod`
- **Customer A**: `rg-fabrikam-customer-a`

### **Configuration Management**

```json
{
  "Instance": {
    "Name": "fabrikam-dev",
    "Environment": "Development",
    "ResourceGroup": "rg-fabrikam-dev",
    "Authentication": {
      "Strategy": "Auto",
      "EntraCapable": false,
      "FallbackToIdentity": true
    }
  }
}
```

## 🧪 Testing Strategy

### **Strategy Detection Testing**

```csharp
[Test]
public async Task AuthStrategy_AutoDetection_SelectsApproprateStrategy()
{
    // Test auto-detection logic
    // Verify fallback behavior
    // Validate configuration switching
}
```

### **Cross-Strategy Compatibility**

- User data migration between strategies
- Token validation consistency
- Role mapping compatibility

## 📚 Implementation Scripts

I'll create the following helper scripts:

1. **`Check-EntraPermissions.ps1`** - Assess current permissions
2. **`Setup-Authentication-Strategy.ps1`** - Automated setup based on permissions
3. **`Test-Authentication-Strategy.ps1`** - Validate chosen strategy
4. **`Migrate-Authentication-Data.ps1`** - Switch between strategies

## 🎯 Success Criteria

### **Functional Requirements**

- ✅ Automatic strategy selection based on permissions
- ✅ Seamless user experience regardless of strategy
- ✅ Consistent API interface for authentication
- ✅ Multi-instance deployment support

### **Security Requirements**

- ✅ Secure token handling in both strategies
- ✅ Proper session management
- ✅ Role-based access control
- ✅ Audit logging for authentication events

### **Operational Requirements**

- ✅ Easy deployment in constrained environments
- ✅ Configuration-driven strategy selection
- ✅ Monitoring and alerting for auth failures
- ✅ Documentation for both strategies

---

## 🔄 Next Steps

1. **Create permission assessment script**
2. **Implement Strategy 2 (ASP.NET Identity) first** - more universally deployable
3. **Add Strategy 1 (Entra External ID)** when permissions available
4. **Implement auto-detection logic**
5. **Update GitHub Issue #3** with dual strategy approach

**Updated Timeline**: 2 weeks for dual implementation vs. 1 week for single strategy  
**Risk Mitigation**: Always have working authentication regardless of tenant permissions
