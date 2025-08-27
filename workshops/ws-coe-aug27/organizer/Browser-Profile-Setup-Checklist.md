# 🌐 Browser Profile Setup for COE Users

**Quick checklist for setting up your dedicated COE browser profile**

## 📋 Setup Checklist for User: `[Your COE Username]`

### Step 1: Create Browser Profile
- [ ] **Browser**: Chrome / Edge / Firefox
- [ ] **Profile Name**: Use your COE username (e.g., `imatest`)
- [ ] **Profile Created**: New clean browser window opened

### Step 2: Initial Login & Password Change
- [ ] **Navigate to**: [portal.azure.com](https://portal.azure.com)
- [ ] **Login with**:
  ```
  Email: [your-username]@fabrikam.cspsecurityaccelerate.com
  Password: TempPassword123!
  ```
- [ ] **Change password** when prompted
- [ ] **Write down new password**: ___________________

### Step 3: MFA Setup
- [ ] **MFA method chosen**: Phone app / SMS / Call
- [ ] **MFA verification completed**
- [ ] **Test MFA**: Successfully logged in with new password + MFA

### Step 4: Service Access Verification
- [ ] **Azure Portal**: ✅ Can access without additional prompts
- [ ] **Copilot Studio**: [copilotstudio.microsoft.com](https://copilotstudio.microsoft.com) ✅
- [ ] **Microsoft 365**: [portal.office.com](https://portal.office.com) ✅

### Step 5: Bookmark Key URLs
- [ ] **Azure Portal**: `https://portal.azure.com`
- [ ] **Copilot Studio**: `https://copilotstudio.microsoft.com` 
- [ ] **Microsoft 365**: `https://portal.office.com`
- [ ] **GitHub**: `https://github.com`
- [ ] **Your Resource Group**: Direct link to `rg-fabrikam-coe-[username]`

## 🎯 Success Criteria

✅ **You should be able to**:
- Switch between Azure, Copilot Studio, and M365 without additional login prompts
- Access your dedicated resource group: `rg-fabrikam-coe-[username]`
- Have Reader access to see other team members' resources
- Have Contributor access only to your own resource group

## 🚨 Troubleshooting

**If you get login prompts between services**:
1. Clear cookies/cache for your COE profile
2. Start fresh with Step 2 above
3. Ensure you're using the same browser profile for all services

**If you can't access a resource group**:
1. Verify you can see it in Azure Portal (should have Reader access)
2. Try creating a test resource in your own group (should have Contributor access)
3. If issues persist, contact the COE administrator

**If MFA isn't working**:
1. Try the "Problems signing in?" link on the login page
2. You may need to set up an alternative MFA method
3. Contact IT support if completely blocked

## 📝 Notes Section
*Use this space for any additional notes or issues encountered*

---
---
---

## 💡 Example: Setup for User `imatest`

**Profile**: `imatest` 
**Email**: `imatest@fabrikam.cspsecurityaccelerate.com`
**Resource Group**: `rg-fabrikam-coe-imatest`
**New Password**: `[Written down securely]`
**MFA Method**: Microsoft Authenticator app
**Status**: ✅ All services accessible
