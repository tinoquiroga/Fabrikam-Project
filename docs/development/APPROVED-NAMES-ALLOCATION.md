# 📋 Approved Fictitious Names Allocation

## 🎯 Overview
This document tracks the allocation of approved fictitious names from `approvedficticiousnames.json` across different parts of the system to prevent naming conflicts and ensure consistent use of organizationally-approved demo names.

## 🔐 Reserved for Authentication Testing (7 users)

### Current Role System (3 users)
Based on our three-tier authentication role system:

**Admin Role (1 user)**
- **Lee Gu** (ID: 1001) - Overland Park, KS
  - Phone: +1 913 555 0101
  - Address: 10801 Mastin Blvd., Suite 620, Overland Park, KS 66210

**Read-Write Role (1 user)**  
- **Alex Wilber** (ID: 1002) - San Diego, CA
  - Phone: +1 858 555 0110
  - Address: 9256 Towne Center Dr., Suite 400, San Diego, CA 92121

**Read-Only Role (1 user)**
- **Henrietta Mueller** (ID: 1003) - Fort Lauderdale, FL
  - Phone: +1 954 555 0118
  - Address: 6750 North Andrews Ave., Suite 400, Fort Lauderdale, FL 33309

### Future Role Extensions (4 users)
Reserved for roles not yet implemented:

**Future Role A**
- **Pradeep Gupta** (ID: 1004) - Cairo, Egypt
  - Phone: +20 255501070
  - Address: Smart Village, Kilo 28, Cairo/Alex Desert Road, Cairo

**Future Role B**
- **Lidia Holloway** (ID: 1005) - Tulsa, OK
  - Phone: +1 918 555 0107
  - Address: 7633 E. 63rd Place, Suite 300, Tulsa, OK 74133

**Future Role C**
- **Joni Sherman** (ID: 1006) - Charlotte, NC
  - Phone: +1 980 555 0101
  - Address: 8055 Microsoft Way, Charlotte, NC 28273

**Future Role D**
- **Miriam Graham** (ID: 1007) - San Diego, CA
  - Phone: +1 858 555 0109
  - Address: 9255 Towne Center Dr., Suite 400, San Diego, CA 92121

## 👥 Allocated to Customer Seed Data (16 users)

The remaining 16 approved names will replace the current customer data:

1. **Lynne Robbins** (ID: 1008) - Tulsa, OK
2. **Ben Walters** (ID: 1009) - Iselin, NJ  
3. **Johanna Lorenz** (ID: 1010) - Louisville, KY
4. **Adele Vance** (ID: 1011) - Bellevue, WA
5. **Christie Cline** (ID: 1012) - San Diego, CA
6. **Megan Bowen** (ID: 1013) - Pittsburgh, PA
7. **Patti Fernandez** (ID: 1014) - Louisville, KY
8. **Allan Deyoung** (ID: 1015) - Waukesha, WI
9. **Irvin Sayers** (ID: 1016) - Bloomington, IL
10. **Diego Siciliani** (ID: 1017) - Birmingham, AL
11. **Grady Archie** (ID: 1018) - Bloomington, IL
12. **Isaiah Langer** (ID: 1019) - Tulsa, OK
13. **Debra Berger** (ID: 1020) - Bellevue, WA
14. **Emily Braun** (ID: 1021) - Tokyo, Japan
15. **Enrico Cattaneo** (ID: 1022) - Birmingham, AL
16. **Nestor Wilke** (ID: 1023) - Seattle, WA

## 🎯 Regional Distribution

### Customer Data Geographic Spread
- **West Coast**: 4 customers (CA, WA)
- **Midwest**: 6 customers (IL, WI, OK, KY, NJ)
- **South**: 3 customers (AL, PA)
- **International**: 1 customer (Japan)

### Authentication Users Geographic Spread
- **Current Roles**: 3 users across KS, CA, FL
- **Future Roles**: 4 users across Egypt, OK, NC, CA

## 📧 Email Address Strategy

### Demo Tenant Infrastructure
- **Demo Tenant**: `fabrikam.levelupcsp.com`
- **Licensed Users**: 25 user mailboxes available
- **Shared Mailboxes**: Available for customer communication capture
- **Email Capabilities**: Full email functionality for testing and demos

### Email Address Patterns

**Authentication Users (Licensed Mailboxes)**
- Pattern: `firstname.lastname@fabrikam.levelupcsp.com`
- Use licensed mailboxes for actual authentication testing
- These users will have real email capabilities for password reset, notifications, etc.
- Reserve 7 licensed mailboxes for the authentication test users

**Customer Data (Shared Mailboxes + Fictional)**
- Pattern: `firstname.lastname@fabrikam.levelupcsp.com` for active demo scenarios
- Pattern: `firstname.lastname@fabrikam-demo.com` for data-only scenarios
- Shared mailboxes can be created for key customer communications
- Consider using shared mailboxes for high-value demo scenarios

### Email Allocation Strategy
- **7 Licensed Users**: Authentication test users (Admin, Read-Write, Read-Only, + 4 future roles)
- **5-10 Shared Mailboxes**: Key customer accounts for demo scenarios
- **6-8 Remaining Licensed Users**: Available for additional demo users or presenters
- **Remaining Customers**: Use fictional `@fabrikam-demo.com` addresses

## 🔄 Implementation Notes

1. **Customer Data**: Update `customers.json` to use allocated names with proper address/phone mapping
2. **Authentication Seed Data**: Create authentication user seed data when implementing Phase 1
3. **Email Infrastructure**: 
   - Authentication users get `@fabrikam.levelupcsp.com` addresses (licensed mailboxes)
   - Key customers get shared mailboxes at `@fabrikam.levelupcsp.com`
   - Remaining customers use fictional `@fabrikam-demo.com` addresses
4. **ID Management**: Customer IDs will be 1-16, Auth user IDs can start from 1001 (matching source)
5. **Azure B2C Integration**: Use real email addresses for authentication users to enable proper email workflows

## 🎯 Demo Tenant Integration

### Azure AD B2C Configuration
- **Tenant Domain**: `fabrikam.levelupcsp.com`
- **User Principal Names**: `username@fabrikam.levelupcsp.com`
- **Email Verification**: Functional for authentication users
- **Password Reset**: Functional with real email delivery

### Shared Mailbox Strategy
Consider creating shared mailboxes for:
- **High-Value Customers**: Adele Vance, Christie Cline (West Coast scenarios)
- **International Customer**: Emily Braun (Tokyo scenarios)
- **Support Scenarios**: General customer service demonstrations
- **Order Notifications**: Automated email capture for order workflows

## 🚫 Naming Conflicts Prevention

- **DO NOT** use names from the "Reserved for Authentication Testing" section in customer data
- **DO NOT** use names from the "Allocated to Customer Seed Data" section for authentication users
- **UPDATE** this document when implementing new authentication roles
- **VERIFY** against this document before adding any new demo users

---

**Last Updated**: July 26, 2025  
**Next Review**: When implementing authentication Phase 1
