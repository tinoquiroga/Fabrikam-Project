# 🧪 Deployment Testing Tracker

This document tracks our active Azure deployments for testing the three authentication modes.

## 📋 Active Test Deployments

### 🔓 **Disabled Mode** - Legacy 4-character suffix
- **Suffix**: `nvxk` (4-character mixed-case - legacy pattern)
- **Authentication Mode**: `Disabled`
- **Resource Group**: `rg-fabrikam-development-nvxk` (if following new pattern)
- **API URL**: `https://fabrikam-api-development-nvxk.azurewebsites.net`
- **MCP URL**: `https://fabrikam-mcp-development-nvxk.azurewebsites.net`
- **Status**: ⚠️ Uses old 4-character suffix pattern
- **Notes**: 
  - Deployed before 6-character lowercase improvements
  - May need redeployment with new pattern for consistency
  - Workflows need manual fixes (project paths, auth fallbacks)

### 🔐 **BearerToken Mode** - New 6-character suffix
- **Suffix**: `xmrbiq` (6-character lowercase - new pattern)
- **Authentication Mode**: `BearerToken`
- **Resource Group**: `rg-fabrikam-development-xmrbiq`
- **API URL**: `https://fabrikam-api-development-xmrbiq.azurewebsites.net`
- **MCP URL**: `https://fabrikam-mcp-development-xmrbiq.azurewebsites.net`
- **Status**: ✅ Fixed workflows, ready for testing
- **Notes**: 
  - Uses improved 6-character lowercase suffix
  - Workflows manually fixed with project paths and auth fallbacks
  - JWT authentication with Key Vault integration

### 🏢 **EntraExternalId Mode** - Planned
- **Suffix**: TBD (will use 6-character lowercase)
- **Authentication Mode**: `EntraExternalId`
- **Status**: 🔄 Not yet deployed
- **Notes**: Requires Entra External ID tenant setup

## 🔧 Workflow Status Summary

| Deployment | API Workflow | MCP Workflow | Issues Fixed |
|------------|--------------|--------------|---------------|
| **nvxk** (Disabled) | ❌ Needs fixes | ❌ Needs fixes | Project paths, auth fallbacks |
| **xmrbiq** (BearerToken) | ✅ Fixed | ✅ Fixed | Complete |
| **gcpm** (Test) | ✅ Fixed | ✅ Fixed | Complete |

## 🚨 nvxk Deployment Concerns

### Issue: Legacy 4-Character Suffix
The `nvxk` deployment uses the old 4-character mixed-case suffix pattern. This could cause:

1. **Naming Inconsistency**: Other deployments use 6-character lowercase
2. **Higher Collision Risk**: 1 in 1.6M vs 1 in 308M collision probability
3. **Pattern Mismatch**: ARM template now expects 6-character lowercase

### Recommendation: 
**Option A**: Keep for testing legacy compatibility
**Option B**: Redeploy with new 6-character pattern for consistency

## 🛠️ Required Workflow Fixes for nvxk

Both `nvxk` workflows need the same fixes applied to `xmrbiq`:

1. **Project-specific build paths**:
   ```yaml
   # Current (broken)
   run: dotnet build --configuration Release
   
   # Fixed
   run: dotnet build FabrikamApi/src/FabrikamApi.csproj --configuration Release
   ```

2. **Authentication fallback patterns**:
   ```yaml
   # Current (single secret)
   client-id: ${{ secrets.AZUREAPPSERVICE_CLIENTID_SPECIFIC }}
   
   # Fixed (with fallbacks)
   client-id: ${{ secrets.AZUREAPPSERVICE_CLIENTID_SPECIFIC || secrets.AZURE_CLIENT_ID || secrets.AZUREAPPSERVICE_CLIENTID }}
   ```

## 📊 Testing Strategy

1. **Keep nvxk** for legacy suffix testing (fix workflows)
2. **Use xmrbiq** for BearerToken authentication testing
3. **Deploy new** EntraExternalId with 6-character suffix
4. **Compare results** across all three modes

## 🎯 Next Actions

- [ ] Fix nvxk workflows (apply same fixes as xmrbiq)
- [ ] Test BearerToken mode with xmrbiq deployment
- [ ] Decide on nvxk: keep legacy or redeploy with new pattern
- [ ] Deploy EntraExternalId mode with 6-character suffix
- [ ] Document testing results for each authentication mode
