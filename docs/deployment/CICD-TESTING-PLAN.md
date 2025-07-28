# 🎯 CI/CD Auto-Fix Testing Plan

## Objective
Test the auto-fix CI/CD workflow (`auto-fix-cicd.yml`) by deploying to Azure and observing how it detects and corrects Azure Portal-generated workflows.

## 🚀 Testing Steps

### 1. Deploy to Azure
Use the Deploy to Azure button in `DEPLOY-TO-AZURE.md`:
- Choose **InMemory + Authentication** for fastest testing
- Use resource group pattern: `rg-fabrikam-dev-{4-char-suffix}`
- Note all output values (API name, MCP name, Key Vault name)

### 2. Repository Variables Setup
After deployment, configure these repository variables in GitHub:

```bash
# Required Variables (get from Azure deployment output)
AZURE_SUBSCRIPTION_ID="your-subscription-id"
AZURE_RESOURCE_GROUP="rg-fabrikam-dev-xxxx"  # From deployment
AZURE_API_APP_NAME="fabrikam-api-dev-xxxx"   # From deployment
AZURE_MCP_APP_NAME="fabrikam-mcp-dev-xxxx"   # From deployment

# Optional (for advanced scenarios)
AZURE_TENANT_ID="your-tenant-id"
```

### 3. Trigger Azure Portal CI/CD
In Azure Portal:
1. Go to your API App Service
2. Navigate to **Deployment Center**
3. Select **GitHub** as source
4. Choose your repository and `feature/phase-1-authentication` branch
5. **Save** - This creates a portal-generated workflow

### 4. Observe Auto-Fix Behavior
Monitor `.github/workflows/` for:
- ✅ **Detection**: Auto-fix workflow detects the new portal-generated workflow
- ✅ **Analysis**: Identifies hardcoded values and missing monorepo support
- ✅ **Correction**: Replaces with our enhanced template
- ✅ **Variables**: Uses repository variables instead of hardcoded values

### 5. Verify Enhancements
Check the corrected workflow includes:
- 🔧 **Monorepo support** (correct project paths)
- 🔀 **Repository variables** instead of hardcoded names
- 🔄 **Multi-app deployment** (both API and MCP)
- 📱 **Proper startup commands** for .NET apps

## 🔍 Expected Workflow Filenames

The auto-fix should create/replace files like:
- `feature-phase-1-authentication_fabrikam-api-dev-xxxx.yml`
- `feature-phase-1-authentication_fabrikam-mcp-dev-xxxx.yml`

With our enhanced template patterns:
- Repository variable usage
- Correct monorepo project paths
- Proper .NET startup commands
- Multi-environment support

## ✅ Success Criteria

### Auto-Fix Workflow Works If:
1. **Detects** portal-generated workflows automatically
2. **Replaces** them with enhanced versions
3. **Uses** repository variables correctly
4. **Builds** and deploys both API and MCP successfully
5. **Maintains** branch-specific naming conventions

### Key Vault Integration Works If:
1. **JWT authentication** works via Key Vault secrets
2. **API endpoints** authenticate correctly
3. **MCP tools** can access authenticated API
4. **No secrets** exposed in App Service configuration

## 🐛 Troubleshooting

### If Auto-Fix Doesn't Trigger:
- Check workflow file patterns in `.github/workflows/auto-fix-cicd.yml`
- Manually trigger with `workflow_dispatch`
- Verify permissions (Contents: write)

### If Deployment Fails:
- Check repository variables are set correctly
- Verify resource group and app names match Azure
- Ensure Key Vault secrets are accessible

### If Authentication Fails:
- Verify Key Vault contains JWT secret
- Check Managed Identity permissions
- Test with demo users from seed data

## 📊 Monitoring

Track success via:
- **GitHub Actions** tab for workflow execution
- **Azure Application Insights** for runtime monitoring  
- **Key Vault Access Policies** for secret access
- **App Service Logs** for authentication debugging

---

**This testing validates our complete multi-instance deployment and auto-fix strategy! 🎯**
