# 🔐 Demo User Authentication

Welcome to the demo user authentication guide for Fabrikam MCP demonstrations. This section covers user management, authentication flows, and role-based demo scenarios.

## 🎯 Quick Reference

| Authentication Mode | Demo Users | Setup Guide | Use Case |
|---------------------|------------|-------------|----------|
| **Disabled** | GUID-based sessions | [No Auth Setup](../copilot-studio/NO-AUTH-SETUP.md) | Quick demos, development |
| **JWT** | Predefined demo accounts | [JWT Auth Setup](../copilot-studio/JWT-AUTH-SETUP.md) | Production demos, security |
| **Entra External ID** | Enterprise SSO | [Enterprise Setup](../copilot-studio/ENTRA-AUTH-SETUP.md) | Coming soon |

## 👥 Demo User Accounts

For JWT authentication demos, the system includes predefined demo users with accessible credentials.

### **Getting Demo Credentials**

Demo passwords are dynamically generated per deployment for security. Get current credentials:

```powershell
# Using the demo script
.\Demo-Authentication.ps1 -ShowCredentials

# Or via API call
$creds = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
$creds.demoUsers | Format-Table -Property name, email, roles
```

### **Available Demo Users**

| Role | Name | Email | Purpose |
|------|------|--------|---------|
| **Admin** | Lee Gu | lee.gu@fabrikam.levelupcsp.com | Full system access, ideal for comprehensive demos |
| **Read-Write** | Alex Wilber | alex.wilber@fabrikam.levelupcsp.com | Data modification capabilities, good for business user demos |
| **Read-Only** | Henrietta Mueller | henrietta.mueller@fabrikam.levelupcsp.com | View-only access, perfect for executive dashboard demos |

### **Role Capabilities**

- **Admin**: Full CRUD operations, user management, system configuration
- **Read-Write**: Create, read, update operations on business data
- **Read-Only**: Read-only access to business data and reports

> **🛡️ Security Note**: Demo passwords are unique per deployment instance with no hardcoded credentials in documentation or code.

## 🎭 Role-Based Demo Scenarios

### **Admin User Demos**
Perfect for technical audiences and comprehensive system demonstrations.

**Sample Demo Flow:**
```
# Login as Admin
Email: lee.gu@fabrikam.levelupcsp.com
Password: [from demo credentials]

# Demo Prompts
"Show me the complete business dashboard with all administrative metrics"
"I need to analyze our customer data across all regions and time periods"
"What are our top customers and their complete order histories?"
```

### **Business User Demos**
Ideal for sales team and business stakeholder demonstrations.

**Sample Demo Flow:**
```
# Login as Read-Write User
Email: alex.wilber@fabrikam.levelupcsp.com
Password: [from demo credentials]

# Demo Prompts
"Show me current sales performance for my territory"
"I need to update customer information and check order status"
"Help me analyze customer satisfaction trends"
```

### **Executive Demos**
Designed for C-level and executive presentations.

**Sample Demo Flow:**
```
# Login as Read-Only User
Email: henrietta.mueller@fabrikam.levelupcsp.com
Password: [from demo credentials]

# Demo Prompts
"Give me an executive summary of our business performance"
"What are the key trends I should be aware of for the board meeting?"
"Show me our customer satisfaction and quality metrics"
```

## 🔐 Authentication Flows

### **JWT Token Authentication**

For JWT-based demos, follow this authentication flow:

1. **Get Demo Credentials**
   ```powershell
   $creds = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
   $admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }
   ```

2. **Authenticate to Get JWT Token**
   ```powershell
   $loginBody = @{
       email = $admin.email
       password = $admin.password
   } | ConvertTo-Json
   
   $authResponse = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
   
   Write-Host "JWT Token: $($authResponse.accessToken)"
   ```

3. **Use Token in Connector**
   - Update Power Apps connector connection
   - Set Authorization header: `Bearer {token}`
   - Test authenticated MCP calls

### **Session Management**

**Token Expiration Handling:**
- JWT tokens typically expire after 24 hours
- Monitor token expiration for long demos
- Refresh tokens as needed before presentations

**Demo Best Practices:**
- Always get fresh credentials before demos
- Test authentication flow during setup
- Have backup users ready for different demo scenarios

## 🛠️ Demo Setup Tools

### **Demo-Authentication.ps1 Script**

The project includes a PowerShell script for demo user management:

```powershell
# Show all demo users and credentials
.\Demo-Authentication.ps1 -ShowCredentials

# Test authentication for specific user
.\Demo-Authentication.ps1 -TestLogin -UserEmail "lee.gu@fabrikam.levelupcsp.com"

# Validate all demo users
.\Demo-Authentication.ps1 -ValidateAllUsers
```

### **Authentication Testing**

Test authentication setup before demos:

```powershell
# Test API connectivity
curl -k https://your-api-app-name.azurewebsites.net/api/info

# Test demo credentials endpoint
curl -k https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials

# Test login with admin user
curl -X POST "https://your-api-app-name.azurewebsites.net/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "lee.gu@fabrikam.levelupcsp.com", "password": "your-password"}'
```

## 🎯 Demo Scenarios by Role

### **Security-Focused Demos**

**Demonstrate Role-Based Access Control:**
1. Login as Read-Only user - show limited access
2. Login as Read-Write user - show additional capabilities
3. Login as Admin user - show full system access

**Authentication Security Demo:**
1. Show JWT token generation
2. Demonstrate token-based API access
3. Show secure Copilot Studio integration

### **Business Process Demos**

**Customer Service Representative Flow:**
- Read-Write access for ticket management
- Customer data access with appropriate permissions
- Order status and update capabilities

**Sales Manager Flow:**
- Read access to sales analytics
- Customer performance reporting
- Territory-based data access

**Executive Dashboard Flow:**
- High-level business metrics
- Strategic insights and trends
- Summary reporting capabilities

## 🔧 Troubleshooting Authentication

### **Common Issues**

**"401 Unauthorized" Errors:**
- Check demo credentials are current
- Verify JWT token format and expiration
- Test login endpoint directly

**"Demo credentials not found":**
- Verify API deployment is complete
- Check authentication mode configuration
- Ensure demo data seeding completed

**"Token expired" Errors:**
- Refresh JWT token before demos
- Check token expiration time
- Plan demo duration accordingly

### **Validation Scripts**

```powershell
# Quick authentication validation
function Test-DemoAuthentication {
    param($ApiUrl)
    
    # Test credentials endpoint
    try {
        $creds = Invoke-RestMethod -Uri "$ApiUrl/api/auth/demo-credentials"
        Write-Host "✅ Demo credentials available: $($creds.demoUsers.Count) users"
    } catch {
        Write-Host "❌ Demo credentials failed: $($_.Exception.Message)"
    }
    
    # Test admin login
    try {
        $admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }
        $loginBody = @{
            email = $admin.email
            password = $admin.password
        } | ConvertTo-Json
        
        $auth = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
        Write-Host "✅ Admin login successful, token expires: $($auth.expiresAt)"
    } catch {
        Write-Host "❌ Admin login failed: $($_.Exception.Message)"
    }
}

# Usage
Test-DemoAuthentication -ApiUrl "https://your-api-app-name.azurewebsites.net"
```

## 📞 Support

### **Quick Help**

1. **Get Demo Credentials**: Use `.\Demo-Authentication.ps1 -ShowCredentials`
2. **Test Authentication**: Use validation scripts provided above
3. **Check API Status**: Verify `/api/info` endpoint responds
4. **Refresh Tokens**: Generate new JWT tokens before demos

### **Additional Resources**

- **[JWT Authentication Setup](../copilot-studio/JWT-AUTH-SETUP.md)** - Complete JWT integration guide
- **[Authentication Architecture](../../architecture/AUTHENTICATION.md)** - Comprehensive auth documentation
- **[Troubleshooting Guide](../copilot-studio/TROUBLESHOOTING.md)** - Common authentication issues

---

**🔑 Ready to authenticate!** Choose your demo user role and start demonstrating secure business intelligence with Fabrikam MCP.
