# 🔐 Demo User Authentication Guide

> **Status**: Implementation Complete ✅  
> **Purpose**: Demo user management and authentication testing for Fabrikam Project  
> **Last Updated**: January 2025

## 📋 Overview

This guide provides comprehensive information about the demo user authentication system implemented in the Fabrikam Project. The system includes predefined demo users with accessible credentials for demonstration and testing purposes, while maintaining production-ready security architecture.

## 🎯 Demo User Credentials

> **🔒 IMPORTANT SECURITY UPDATE:** Demo passwords are now **dynamically generated** and **instance-specific**!
>
> **📋 To Get Current Passwords:**
>
> ```powershell
> .\Demo-Authentication.ps1 -ShowCredentials
> ```

### **Available Demo Users**

| Role              | Name              | Email                                     | Password   | Purpose                               |
| ----------------- | ----------------- | ----------------------------------------- | ---------- | ------------------------------------- |
| **Admin**         | Lee Gu            | lee.gu@fabrikam.levelupcsp.com            | `Dynamic*` | Full system access - Licensed mailbox |
| **Read-Write**    | Alex Wilber       | alex.wilber@fabrikam.levelupcsp.com       | `Dynamic*` | Data modification - Licensed mailbox  |
| **Read-Only**     | Henrietta Mueller | henrietta.mueller@fabrikam.levelupcsp.com | `Dynamic*` | View-only access - Licensed mailbox   |
| **Future-Role-A** | Pradeep Gupta     | pradeep.gupta@fabrikam.levelupcsp.com     | `Dynamic*` | Future expansion - Licensed mailbox   |
| **Future-Role-B** | Lidia Holloway    | lidia.holloway@fabrikam.levelupcsp.com    | `Dynamic*` | Future expansion - Licensed mailbox   |
| **Future-Role-C** | Joni Sherman      | joni.sherman@fabrikam.levelupcsp.com      | `Dynamic*` | Future expansion - Licensed mailbox   |
| **Future-Role-D** | Miriam Graham     | miriam.graham@fabrikam.levelupcsp.com     | `Dynamic*` | Future expansion - Licensed mailbox   |

> **🛡️ Security Features:**
>
> - Passwords are unique per deployment instance
> - No hardcoded credentials in documentation or code
> - Automatic password updates on deployment
> - API endpoint: `GET /api/auth/demo-credentials` (development environments only)

### **Role Capabilities**

- **Admin**: Full CRUD operations, user management, system configuration
- **Read-Write**: Create, read, update operations on business data
- **Read-Only**: Read-only access to business data and reports
- **Future Roles**: Placeholder accounts for demonstration of role expansion

## 🛠️ Demo User Management Tools

### **1. Demo-Authentication.ps1 Script**

**Quick Credential Display**:

```powershell
# Show all demo user credentials in formatted table
.\Demo-Authentication.ps1 -ShowCredentials
```

**Authentication Testing**:

```powershell
# Test authentication for all demo users (local API)
.\Demo-Authentication.ps1 -TestAuth

# Test authentication against production API
.\Demo-Authentication.ps1 -TestAuth -ApiUrl "https://fabrikam-api.azurewebsites.net"

# Verbose testing with detailed output
.\Demo-Authentication.ps1 -TestAuth -Verbose
```

### **2. Enhanced Test-Development.ps1**

**Authentication-Only Testing**:

```powershell
# Test only authentication functionality
.\Test-Development.ps1 -AuthOnly

# Full system testing including authentication
.\Test-Development.ps1 -Verbose
```

### **3. Manual API Testing (api-tests.http)**

**Authentication Endpoints**:

```http
### User Registration
POST https://localhost:7297/api/auth/register
Content-Type: application/json

{
  "email": "test@fabrikam.levelupcsp.com",
  "password": "TestPassword123!",
  "firstName": "Test",
  "lastName": "User"
}

### User Login (Demo User)
POST https://localhost:7297/api/auth/login
Content-Type: application/json

{
  "email": "lee.gu@fabrikam.levelupcsp.com",
  "password": "GET_FROM_DEMO_SCRIPT"
}

# 📋 To get current password, run: .\Demo-Authentication.ps1 -ShowCredentials
# Or call: GET https://localhost:7297/api/auth/demo-credentials (dev only)

### Validate Token
GET https://localhost:7297/api/auth/validate
Authorization: Bearer {{jwt_token}}

### Get Current User
GET https://localhost:7297/api/auth/me
Authorization: Bearer {{jwt_token}}

### Logout
POST https://localhost:7297/api/auth/logout
Authorization: Bearer {{jwt_token}}
```

## 🔧 Architecture Overview

### **AuthenticationSeedService**

**Purpose**: Automatically create demo users during application startup

**Key Features**:

- ✅ **JSON Data Source**: Loads user data from `auth-users.json`
- ✅ **Role-Based Passwords**: Predefined passwords based on user roles
- ✅ **Conditional Logging**: Development-only credential logging
- ✅ **Error Handling**: Graceful handling of duplicate users and errors
- ✅ **Production Safety**: Environment-aware execution

**Implementation Location**: `FabrikamApi/src/Services/AuthenticationSeedService.cs`

### **Data Sources**

**auth-users.json**: Contains complete user profile data

```json
{
  "users": [
    {
      "firstName": "Lee",
      "lastName": "Gu",
      "email": "lee.gu@fabrikam.levelupcsp.com",
      "roles": ["Admin"],
      "department": "Executive Leadership",
      "title": "Chief Executive Officer",
      "location": "Seattle, WA"
    }
    // ... additional users
  ]
}
```

### **Integration Points**

**Program.cs Registration**:

```csharp
// Service registration
builder.Services.AddScoped<AuthenticationSeedService>();

// Startup execution
var seedService = scope.ServiceProvider.GetRequiredService<AuthenticationSeedService>();
await seedService.SeedAuthenticationDataAsync();
```

## 🧪 Testing Scenarios

### **Authentication Flow Testing**

**1. User Registration**:

- Test user registration with valid data
- Validate password requirements
- Confirm duplicate email handling

**2. User Login**:

- Test login with demo user credentials
- Validate JWT token generation
- Confirm role assignment in token claims

**3. Token Validation**:

- Test protected endpoint access with valid tokens
- Validate token expiration handling
- Confirm role-based authorization

**4. User Logout**:

- Test logout functionality
- Validate token invalidation

### **Role-Based Access Testing**

**Admin Role Testing**:

```powershell
# Get current dynamic passwords first
$credentials = .\Demo-Authentication.ps1 -ShowCredentials
# Or get directly from API: $creds = Invoke-RestMethod -Uri "http://localhost:7296/api/auth/demo-credentials"

# Login as admin user (use current dynamic password)
$adminToken = (Invoke-RestMethod -Uri "https://localhost:7297/api/auth/login" -Method POST -Body (@{
    email = "lee.gu@fabrikam.levelupcsp.com"
    password = "USE_DYNAMIC_PASSWORD_FROM_ABOVE"
} | ConvertTo-Json) -ContentType "application/json").token

# Test admin-only endpoints
Invoke-RestMethod -Uri "https://localhost:7297/api/admin/users" -Headers @{Authorization="Bearer $adminToken"}
```

**Read-Write Role Testing**:

```powershell
# Login as read-write user
$rwToken = (Invoke-RestMethod -Uri "https://localhost:7297/api/auth/login" -Method POST -Body (@{
    email = "kai.mueller@fabrikam.levelupcsp.com"
    password = "ReadWrite123!"
} | ConvertTo-Json) -ContentType "application/json").token

# Test data modification endpoints
Invoke-RestMethod -Uri "https://localhost:7297/api/products" -Method POST -Headers @{Authorization="Bearer $rwToken"} -Body $productData
```

**Read-Only Role Testing**:

```powershell
# Login as read-only user
$roToken = (Invoke-RestMethod -Uri "https://localhost:7297/api/auth/login" -Method POST -Body (@{
    email = "elena.volkov@fabrikam.levelupcsp.com"
    password = "ReadOnly123!"
} | ConvertTo-Json) -ContentType "application/json").token

# Test read-only access (should succeed)
Invoke-RestMethod -Uri "https://localhost:7297/api/products" -Headers @{Authorization="Bearer $roToken"}

# Test write access (should fail)
try {
    Invoke-RestMethod -Uri "https://localhost:7297/api/products" -Method POST -Headers @{Authorization="Bearer $roToken"} -Body $productData
} catch {
    Write-Host "Expected authorization failure: $($_.Exception.Message)" -ForegroundColor Yellow
}
```

## 🔒 Security Considerations

### **Development vs. Production**

**Development Features**:

- ✅ **Credential Logging**: Demo passwords logged to console for easy access
- ✅ **Known Passwords**: Predefined, accessible passwords for testing
- ✅ **Comprehensive Logging**: Detailed authentication flow logging

**Production Security**:

- ✅ **No Credential Logging**: Passwords never logged in production
- ✅ **Secure Hashing**: ASP.NET Identity secure password storage
- ✅ **JWT Security**: Secure token generation and validation
- ✅ **Environment Separation**: Clear development/production boundaries

### **Demo User Isolation**

**Best Practices**:

- ✅ **Separate Credentials**: Demo users have distinct, recognizable credentials
- ✅ **Clear Naming**: Email addresses clearly indicate demo/test purposes
- ✅ **Role Isolation**: Each demo user represents a specific role scenario
- ✅ **Data Separation**: Demo users operate on demo data sets

## 📊 Troubleshooting

### **Common Issues**

**1. Demo Users Not Created**:

```powershell
# Check authentication service logs
.\Manage-Project.ps1 logs

# Verify auth-users.json exists and is valid
Get-Content auth-users.json | ConvertFrom-Json

# Manually test seeding service
.\Test-Development.ps1 -AuthOnly
```

**2. Authentication Failures**:

```powershell
# Verify API server is running
.\Manage-Project.ps1 status

# Test with Demo-Authentication.ps1
.\Demo-Authentication.ps1 -TestAuth -Verbose

# Check JWT configuration
.\Test-Development.ps1 -ApiOnly
```

**3. Role Authorization Issues**:

```powershell
# Verify user roles
.\Demo-Authentication.ps1 -ShowCredentials

# Test specific role endpoints
# Use api-tests.http for manual testing
```

### **Debugging Steps**

**1. Check Service Registration**:

- Verify AuthenticationSeedService is registered in Program.cs
- Confirm Identity services are properly configured
- Check database connection and migrations

**2. Validate JSON Data**:

- Ensure auth-users.json is properly formatted
- Verify email addresses and role assignments
- Check for duplicate entries

**3. Test Authentication Flow**:

- Use Demo-Authentication.ps1 for automated testing
- Check JWT token generation and validation
- Verify role claims in tokens

## 🚀 Usage Examples

### **Demo Scenario: Role-Based Access**

**Setup**:

```powershell
# Start the API server
.\Manage-Project.ps1 start

# Verify demo users are loaded
.\Demo-Authentication.ps1 -ShowCredentials

# Test authentication
.\Demo-Authentication.ps1 -TestAuth
```

**Demonstration Flow**:

1. **Admin User**: Show full system access
2. **Read-Write User**: Demonstrate data modification capabilities
3. **Read-Only User**: Show restricted access and authorization failures
4. **Token Validation**: Demonstrate JWT security and validation

### **Demo Scenario: Authentication Testing**

**Automated Testing**:

```powershell
# Quick authentication validation
.\Test-Development.ps1 -Quick

# Comprehensive authentication testing
.\Test-Development.ps1 -AuthOnly

# Full system testing
.\Test-Development.ps1 -Verbose
```

### **Demo Scenario: API Endpoint Testing**

**Manual Testing with VSCode REST Client**:

1. Open `api-tests.http` in VSCode
2. Use demo credentials for authentication endpoints
3. Test protected endpoints with generated JWT tokens
4. Demonstrate role-based authorization failures

## 📚 Related Documentation

- **[Authentication Implementation Guide](../development/AUTHENTICATION-IMPLEMENTATION-GUIDE.md)** - Complete implementation details
- **[Testing Strategy](../development/testing/TESTING-STRATEGY.md)** - Comprehensive testing approach
- **[Development Workflow](../getting-started/DEVELOPMENT-WORKFLOW.md)** - Daily development procedures
- **[Quick Demo Prompts](QUICK-DEMO-PROMPTS.md)** - Ready-to-use demo scenarios

---

**🎯 Quick Reference**: Use `.\Demo-Authentication.ps1 -ShowCredentials` to display all demo user credentials instantly!
