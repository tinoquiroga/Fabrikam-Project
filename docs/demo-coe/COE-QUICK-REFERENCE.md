# 🚀 COE Quick Reference Card

**Print this page or keep it open during the demo for quick reference!**

---

## ⚡ Quick Start Checklist

- [ ] **GitHub Account**: Sign up at [github.com](https://github.com) if needed
- [ ] **Fork Project**: Go to [github.com/davebirr/Fabrikam-Project](https://github.com/davebirr/Fabrikam-Project) → Click "Fork"
- [ ] **Azure Setup**: Open [portal.azure.com](https://portal.azure.com) → Cloud Shell → Run setup script
- [ ] **Deploy**: Use Deploy to Azure button with your fork URL
- [ ] **CI/CD**: Configure GitHub Actions with Azure service principal
- [ ] **Test**: Verify API at `https://[your-app].azurewebsites.net/swagger`

---

## 🔧 Key Commands

### Azure Setup Script
```powershell
$suffix = -join ((97..122) | Get-Random -Count 6 | ForEach-Object {[char]$_})
$resourceGroupName = "rg-fabrikam-coe-$suffix"
az group create --name $resourceGroupName --location "East US 2"
$userObjectId = az ad signed-in-user show --query id -o tsv
Write-Host "Resource Group: $resourceGroupName"
Write-Host "User Object ID: $userObjectId"
```

### Service Principal for CI/CD
```bash
az ad sp create-for-rbac --name "GitHub-Actions-Fabrikam-COE" \
  --role contributor \
  --scopes /subscriptions/[YOUR-SUBSCRIPTION-ID] \
  --sdk-auth
```

---

## 🎯 Important URLs

### GitHub
- **Original Project**: `https://github.com/davebirr/Fabrikam-Project`
- **Your Fork**: `https://github.com/[your-username]/Fabrikam-Project`

### Azure
- **Portal**: `https://portal.azure.com`
- **Cloud Shell**: Click terminal icon in Azure Portal
- **Your API**: `https://[your-app-name].azurewebsites.net`
- **Swagger**: `https://[your-app-name].azurewebsites.net/swagger`

### Deploy Template URL Pattern
```
https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2F[YOUR-USERNAME]%2FFabrikam-Project%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.modular.json
```

---

## 📝 GitHub Secrets Needed

| Secret Name | Where to Get It |
|-------------|-----------------|
| `AZURE_CREDENTIALS` | JSON output from service principal command |
| `AZURE_SUBSCRIPTION_ID` | `az account show --query id -o tsv` |
| `AZURE_RESOURCE_GROUP` | From your setup script output |

---

## 🧪 Demo Credentials (BearerToken mode)

| Role | Email | Password |
|------|-------|----------|
| Admin | `lee.gu@fabrikam.levelupcsp.com` | `AdminsGZJSl0!` |
| Read-Write | `alex.wilber@fabrikam.levelupcsp.com` | `ReadWrite2Re7BJ1!` |
| Read-Only | `henrietta.mueller@fabrikam.levelupcsp.com` | `ReadOnlyru6bKp4!` |

---

## ⚠️ Troubleshooting Quick Fixes

| Problem | Quick Fix |
|---------|-----------|
| Can't fork repo | Check you're logged into GitHub |
| Deploy fails | Verify Contributor permissions in Azure |
| CI/CD fails | Check GitHub secrets are set correctly |
| API not working | Wait 5-10 minutes, check Azure App Service logs |
| Auth not working | Verify BearerToken mode was selected |

---

## 🆘 Help During Demo

- **Ask Dave Birr** for immediate assistance
- **Use chat** for questions
- **Reference the full guide**: [COE-COMPLETE-SETUP-GUIDE.md](./COE-COMPLETE-SETUP-GUIDE.md)

---

**⏱️ Typical timing:**
- GitHub setup: 5 minutes
- Azure deployment: 15 minutes  
- CI/CD configuration: 10 minutes
- Testing: 5 minutes

**🎉 Total: ~35 minutes to fully deployed solution with CI/CD!**
