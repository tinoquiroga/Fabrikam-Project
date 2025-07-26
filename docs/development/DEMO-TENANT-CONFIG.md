# 🔐 Demo Tenant Configuration Guide

## 🎯 Overview
This document provides specific configuration guidance for integrating the Fabrikam Project with your demo tenant infrastructure at `fabrikam.levelupcsp.com`.

## 🏢 Tenant Infrastructure

### Demo Tenant Details
- **Domain**: `fabrikam.levelupcsp.com`
- **Licensed Users**: 25 M365 E5 + M365 Copilot available
- **Shared Mailboxes**: Available for customer communication scenarios
- **Email Capabilities**: Full functionality for authentication flows

### Authentication Integration Benefits
- ✅ **Real Email Workflows**: Password reset, email verification, notifications
- ✅ **Demo Authenticity**: Actual email addresses increase demo credibility
- ✅ **Testing Reliability**: Real email delivery for comprehensive testing
- ✅ **Presentation Value**: Live email demonstrations during presentations

## 📧 Email Allocation Strategy

### Licensed Mailboxes (25 total)

**Authentication Test Users (7 mailboxes)**
- `lee.gu@fabrikam.levelupcsp.com` - Admin Role
- `alex.wilber@fabrikam.levelupcsp.com` - Read-Write Role  
- `henrietta.mueller@fabrikam.levelupcsp.com` - Read-Only Role
- `pradeep.gupta@fabrikam.levelupcsp.com` - Future Role A
- `lidia.holloway@fabrikam.levelupcsp.com` - Future Role B
- `joni.sherman@fabrikam.levelupcsp.com` - Future Role C
- `miriam.graham@fabrikam.levelupcsp.com` - Future Role D

**Demo Presenters/Admins (5-8 mailboxes)**
- Reserved for actual presenters and demo administrators
- Configure with appropriate roles based on demo scenarios

**Customer Scenarios (5-10 shared mailboxes)**
- High-value customer communications
- Order notification capture
- Support ticket demonstrations
- International customer scenarios

### Shared Mailbox Recommendations

**Customer Communications**
- `customers@fabrikam.levelupcsp.com` - General customer inquiries
- `orders@fabrikam.levelupcsp.com` - Order notifications and updates
- `support@fabrikam.levelupcsp.com` - Support ticket scenarios
- `sales@fabrikam.levelupcsp.com` - Sales communication demonstrations

**Regional/Specialized**
- `westcoast@fabrikam.levelupcsp.com` - West Coast customer scenarios
- `international@fabrikam.levelupcsp.com` - International customer (Emily Braun scenarios)

## 🔧 Azure AD B2C Configuration

### Tenant Setup
```json
{
  "TenantDomain": "fabrikam.levelupcsp.com",
  "B2CInstance": "https://fabrikam.b2clogin.com",
  "SignUpSignInPolicyId": "B2C_1_signupsignin",
  "ResetPasswordPolicyId": "B2C_1_passwordreset",
  "EditProfilePolicyId": "B2C_1_profileedit"
}
```

### User Principal Name Format
- Authentication users: `username@fabrikam.levelupcsp.com`
- External B2C users: `username@fabrikam.onmicrosoft.com` (for B2C local accounts)

### Custom Attributes
Configure custom attributes in B2C for business context:
- `extension_Role` - User role (Admin, Read-Write, Read-Only)
- `extension_Department` - User department
- `extension_Region` - User region
- `extension_EmployeeId` - Employee identifier

## 🏗️ Implementation Phases

### Phase 1: Core Authentication
1. **Create Licensed Mailboxes** for the 7 authentication test users
2. **Configure Azure AD B2C** tenant settings
3. **Implement JWT Authentication** in the API
4. **Add User/Role Entities** to the database
5. **Test Email Workflows** (password reset, verification)

### Phase 2: Enhanced Features  
1. **Create Shared Mailboxes** for customer scenarios
2. **Implement Email Notifications** for orders/support
3. **Add Role-Based Authorization** to API endpoints
4. **Configure Email Templates** for notifications

### Phase 3: Demo Optimization
1. **Create Demo Scripts** using real email workflows
2. **Configure Automated Email Scenarios** for presentations
3. **Add Email Monitoring** dashboards for live demos
4. **Optimize Performance** for demo scenarios

## 🧪 Testing Strategy

### Authentication Testing
```powershell
# Test user creation in B2C
# Verify email delivery
# Test password reset flows
# Validate role assignments
```

### Email Integration Testing
- Password reset email delivery
- Order confirmation emails  
- Support ticket notifications
- Welcome/onboarding emails

### Demo Scenarios
- Live login demonstrations
- Real-time email workflows
- Multi-role access testing
- Customer communication flows

## 🔐 Security Considerations

### Demo Environment Security
- ✅ Use B2C policies for user self-service
- ✅ Implement proper role-based access control
- ✅ Configure session timeouts for demo scenarios
- ✅ Use real SSL certificates for authenticity

### Email Security
- ✅ Configure SPF/DKIM/DMARC for email authenticity
- ✅ Use secure email templates
- ✅ Implement email rate limiting
- ✅ Monitor for email delivery issues

## 📊 Monitoring & Analytics

### Email Monitoring
- Track email delivery rates
- Monitor authentication success/failure
- Log demo user activities
- Alert on authentication issues

### Demo Analytics
- User login patterns during demos
- Email open rates (if configured)
- Authentication flow completion rates
- Role usage statistics

## 🚀 Next Steps

1. **Create the 7 licensed mailboxes** for authentication users
2. **Configure Azure AD B2C** with the tenant domain
3. **Update appsettings.json** with tenant-specific configuration
4. **Begin Phase 1 implementation** following the authentication guide
5. **Test email workflows** before proceeding to customer scenarios

---

**Reference Files**:
- `AUTHENTICATION-IMPLEMENTATION-GUIDE.md` - Complete implementation methodology
- `APPROVED-NAMES-ALLOCATION.md` - Name allocation tracking
- `auth-users.json` - Authentication user seed data

**Last Updated**: July 26, 2025  
**Next Review**: After Phase 1 B2C configuration
