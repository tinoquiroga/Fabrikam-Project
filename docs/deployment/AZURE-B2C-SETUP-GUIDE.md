# 🔐 Authentication Setup Guide - Fabrikam Project

> **Project**: Fabrikam Project Phase 1 Authentication  
> **Issue**: [GitHub Issue #3](https://github.com/davebirr/Fabrikam-Project/issues/3)  
> **Updated**: July 26, 2025 - Dual Strategy Implementation  
> **Status**: ASP.NET Identity Implementation (Strategy 2)

## 📋 Strategy Decision

**Permission Assessment Results**:

- ❌ No Entra Global Administrator permissions
- ✅ Azure subscription access available
- ✅ Can create applications and resources

**Selected Strategy**: **ASP.NET Core Identity + JWT (Strategy 2)**  
**Reasoning**: Provides authentication without requiring Entra External ID permissions

## 🏗️ Prerequisites

**Azure Environment**:

- **Subscription**: MCAPS-Hybrid-REQ-59531-2023-davidb
- **Subscription ID**: `1ae622b1-c33c-457f-a2bb-351fed78922f`
- **Tenant**: Microsoft Non-Production (fpdo.microsoft.com)
- **Tenant ID**: `16b3c013-d300-468d-ac64-7eda0820b6d3`
- **Resource Group**: `rg-fabrikam-dev` ✅ **Exists**
- **Instance**: `fabrikam-dev`

**Resource Provider Requirements**:

- **Microsoft.Sql**: Required for Azure SQL Database creation
  - The setup script will automatically register this provider if needed
  - Manual registration: `az provider register --namespace Microsoft.Sql`

## 🚀 Step 1: Azure SQL Database Setup

### **1.1 Create Azure SQL Database**

```bash
# Create SQL Server
az sql server create \
  --name "fabrikam-dev-sql" \
  --resource-group "rg-fabrikam-dev" \
  --location "eastus2" \
  --admin-user "fabrikam-admin" \
  --admin-password "[Secure-Password]"

# Create Database
az sql db create \
  --server "fabrikam-dev-sql" \
  --resource-group "rg-fabrikam-dev" \
  --name "fabrikam-dev-db" \
  --service-objective "S0"

# Configure firewall for Azure services
az sql server firewall-rule create \
  --server "fabrikam-dev-sql" \
  --resource-group "rg-fabrikam-dev" \
  --name "AllowAzureServices" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0"
```

### **1.2 Database Schema Setup**

The application will automatically create Identity tables on first run:

- **AspNetUsers**: User accounts with custom Fabrikam fields
- **AspNetRoles**: Three-tier role system (ReadOnly, ReadWrite, Admin)
- **AspNetUserRoles**: User-role assignments

## � Step 2: JWT Configuration

### **2.1 Generate JWT Secret Key**

```powershell
# Generate secure 256-bit key
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$jwtSecret = [Convert]::ToBase64String($bytes)
Write-Host "JWT Secret: $jwtSecret"
```

### **2.2 Application Configuration**

````json
{
  "Authentication": {
    "Strategy": "AspNetIdentity",
    "AspNetIdentity": {
      "Jwt": {
        "SecretKey": "[Generated-JWT-Secret]",
        "Issuer": "https://fabrikam-api-dev.levelupcsp.com",
        "Audience": "fabrikam-api",
        "ExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=fabrikam-dev-sql.database.windows.net;Database=fabrikam-dev-db;User Id=fabrikam-admin;Password=[Password];Encrypt=true;TrustServerCertificate=false;"
  }
}

## 🏗️ Step 3: Configure User Flows

### **3.1 Registration & Login Flow**
1. Navigate to **"Azure AD B2C"** → **"User flows"**
2. Click **"+ New user flow"**
3. Select **"Sign up and sign in"**
4. Choose **"Recommended"** version

**Flow Configuration**:
- **Name**: `B2C_1_signup_signin`
- **Identity providers**:
  - ✅ Email signup
  - ✅ Local account
- **User attributes and claims**:
  - **Collect**: Given Name, Surname, Email Address
  - **Return**: Given Name, Surname, Email Address, User's Object ID

### **3.2 Password Reset Flow**
1. Create **"Password reset"** user flow
2. **Name**: `B2C_1_password_reset`
3. **Claims**: Email Address, User's Object ID

### **3.3 Profile Editing Flow**
1. Create **"Profile editing"** user flow
2. **Name**: `B2C_1_profile_edit`
3. **Claims**: Given Name, Surname, Email Address

## 🔑 Step 4: Application Registration

### **4.1 Register Fabrikam API Application**
1. Navigate to **"App registrations"**
2. Click **"+ New registration"**

**Application Details**:
- **Name**: `Fabrikam-API`
- **Account types**: **"Accounts in this organizational directory only"**
- **Redirect URI**:
  - **Type**: Web
  - **URI**: `https://localhost:7297/signin-oidc` (development)

### **4.2 Configure Application Settings**

**Authentication**:
- Add redirect URIs:
  - `https://localhost:7297/signin-oidc` (local development)
  - `https://fabrikam-api-dev.levelupcsp.com/signin-oidc` (Azure deployment)
- **Logout URL**: `https://localhost:7297/signout-oidc`
- **Implicit grant**: ✅ ID tokens

**API Permissions**:
- Microsoft Graph: `User.Read` (already added)
- No additional permissions needed for basic authentication

### **4.3 Client Secret Creation**
1. Go to **"Certificates & secrets"**
2. Click **"+ New client secret"**
3. **Description**: `Fabrikam-API-Secret`
4. **Expires**: `6 months`
5. **Copy the secret value immediately** (store securely)

## 🛠️ Step 5: JWT Token Configuration

### **5.1 Token Configuration**
1. Navigate to **"Token configuration"**
2. Click **"+ Add optional claim"**
3. **Token type**: ID
4. **Claims to add**:
   - `email`
   - `given_name`
   - `family_name`
   - `oid` (Object ID)

### **5.2 Application ID URI**
1. Go to **"Expose an API"**
2. Click **"Set"** next to Application ID URI
3. Accept default: `https://fabrikam-modular-homes.onmicrosoft.com/fabrikam-api`

## 👥 Step 6: Test User Creation

### **6.1 Create Test Users**
1. Navigate to **"Users"** in B2C tenant
2. Click **"+ New user"**
3. **Create user**

**Test User 1 - Admin**:
- **Username**: `admin@fabrikam-test.local`
- **Name**: `Test Admin`
- **First name**: `Test`
- **Last name**: `Admin`
- **Password**: Create strong password
- **Role**: Will be configured in database

**Test User 2 - Regular User**:
- **Username**: `user@fabrikam-test.local`
- **Name**: `Test User`
- **First name**: `Test`
- **Last name**: `User`

## 📝 Step 7: Configuration Values Collection

After completing setup, collect these values for application configuration:

```json
{
  "AzureB2C": {
    "TenantId": "[B2C-Tenant-ID]",
    "TenantName": "fabrikam-modular-homes.onmicrosoft.com",
    "ClientId": "[Fabrikam-API-App-ID]",
    "ClientSecret": "[Client-Secret-Value]",
    "Instance": "https://fabrikam-modular-homes.b2clogin.com/",
    "Domain": "fabrikam-modular-homes.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1_signup_signin",
    "ResetPasswordPolicyId": "B2C_1_password_reset",
    "EditProfilePolicyId": "B2C_1_profile_edit"
  }
}
````

## ✅ Verification Checklist

**B2C Tenant Setup**:

- [ ] B2C tenant created and linked to subscription
- [ ] Tenant appears in rg-AiSkilling resource group
- [ ] Can switch to B2C tenant successfully

**User Flows**:

- [ ] Sign up & sign in flow configured
- [ ] Password reset flow configured
- [ ] Profile editing flow configured

**Application Registration**:

- [ ] Fabrikam-API application registered
- [ ] Redirect URIs configured (local + Azure)
- [ ] Client secret created and stored
- [ ] Token claims configured

**Test Users**:

- [ ] Admin test user created
- [ ] Regular test user created
- [ ] Test login works with user flows

**Configuration**:

- [ ] All configuration values collected
- [ ] Ready for .NET application integration

## 🔄 Next Steps

1. **Complete this B2C setup** following the guide above
2. **Collect configuration values** for application settings
3. **Move to Issue #4**: Database schema implementation
4. **Configure .NET application** with B2C authentication
5. **Test end-to-end authentication flow**

## 📚 References

- [Azure AD B2C Documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)
- [User Flow Configuration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview)
- [Application Registration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-register-applications)
- [GitHub Issue #3](https://github.com/davebirr/Fabrikam-Project/issues/3)

---

**Created**: July 26, 2025  
**For**: Phase 1 Authentication Implementation  
**Status**: Ready for implementation
