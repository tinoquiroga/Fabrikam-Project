# Critical Files Protection List

## ⚠️ CRITICAL: Never Delete These Files

These files are essential for the project's core functionality and should **NEVER** be deleted or moved without explicit approval:

### Deployment Templates
- `deployment/AzureDeploymentTemplate.modular.json` - **PRIMARY ARM TEMPLATE** for Deploy to Azure button
- `deployment/AzureDeploymentTemplate.parameters.*.json` - Parameter files for different environments
- `deployment/modules/*.json` - Modular ARM template components

### Core Configuration
- `azure.yaml` - Azure Developer CLI configuration
- `.github/workflows/deploy-*.yml` - CI/CD deployment workflows
- `api-tests.http` - API testing suite for manual validation

### Authentication Infrastructure
- `FabrikamApi/src/Services/Authentication/*.cs` - Authentication service implementations
- `FabrikamContracts/DTOs/AuthenticationModels.cs` - Authentication contracts
- `FabrikamApi/src/Configuration/*.cs` - Authentication configuration

### Testing Infrastructure
- `test.ps1` - Main testing script
- `scripts/testing/Test-*.ps1` - Modular testing scripts
- `FabrikamTests/Integration/Authentication/*.cs` - Authentication integration tests

## 🔒 Protection Rules

1. **Before moving any file to `archive/`:**
   - Check if it's in this CRITICAL-FILES.md list
   - Verify no active functionality depends on it
   - Get explicit approval for critical infrastructure files

2. **Before deleting any deployment file:**
   - Ensure there's a modern replacement (Bicep → ARM is acceptable)
   - Verify Deploy to Azure button still works
   - Test with all three authentication modes

3. **Repository cleanup guidelines:**
   - Old documentation → `archive/docs/`
   - Obsolete test files → `archive/old-test-files/`
   - **NEVER** archive active deployment templates
   - **NEVER** archive core authentication infrastructure

## 🛡️ Recovery Process

If critical files are accidentally moved/deleted:

1. **Check git status first:**
   ```powershell
   git status --porcelain | Where-Object { $_ -match "deployment" }
   ```

2. **Restore from git if not committed:**
   ```powershell
   git checkout HEAD -- deployment/AzureDeploymentTemplate.modular.json
   ```

3. **If files are in archive, move them back:**
   ```powershell
   Move-Item archive/deployment/*.json deployment/
   ```

4. **Test deployment after restoration:**
   ```powershell
   .\test.ps1 -Quick
   ```

## 📋 Current Issue (July 31, 2025)

**Status**: Deployment files were moved to `archive/deployment/` but not committed
**Action needed**: Restore from archive or git checkout to restore functionality
**Impact**: Deploy to Azure button will not work until resolved

**Files to restore:**
- `deployment/AzureDeploymentTemplate.modular.json`
- `deployment/AzureDeploymentTemplate.parameters.*.json`
- `deployment/modules/*.json`
