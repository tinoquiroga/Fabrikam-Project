# ✅ Fabrikam Deployment Checklist

## 🎯 **Complete Setup in 15 Minutes**

Follow this checklist to deploy your own Fabrikam AI Demo instance with automatic CI/CD.

---

## 📋 **Phase 1: Prerequisites** (2 minutes)

### **✅ GitHub Setup**
- [ ] **Fork Repository**: Go to [github.com/davebirr/Fabrikam-Project](https://github.com/davebirr/Fabrikam-Project) → Click **Fork**
- [ ] **Create PAT**: GitHub Settings → Developer settings → Personal access tokens → Generate new token
  - ✅ Scopes needed: `repo`, `workflow`, `admin:repo_hook`
  - ✅ Save the token securely (you'll need it later)

### **✅ Azure Setup**  
- [ ] **Azure Subscription**: Ensure you have Contributor access to an Azure subscription
- [ ] **Choose Region**: Pick an Azure region close to you (e.g., East US 2, West Europe)

---

## 🚀 **Phase 2: Deploy Infrastructure** (5 minutes)

### **✅ One-Click Deployment**
- [ ] **Click Deploy Button**: Use the "Deploy to Azure" button in [deployment/README.md](README.md)
- [ ] **Fill Parameters**:
  - ✅ **Resource Group**: Create new (e.g., `rg-fabrikam-demo`)
  - ✅ **Base Name**: Keep default `FabrikamAIDemo`
  - ✅ **Environment**: Keep default `dev`  
  - ✅ **Custom Domain**: Enter your domain (e.g., `levelupcsp.com`) or leave empty
  - ✅ **Enable Custom Domains**: `true` if you entered a domain
  - ✅ **GitHub Repository**: URL to YOUR fork (not the original)
  - ✅ **SKU Name**: `B1` for demo, `S1` for production

### **✅ Wait for Deployment**
- [ ] **Monitor Progress**: Watch the deployment in Azure Portal (usually 3-5 minutes)
- [ ] **Note Instance Suffix**: Save the 4-character suffix (e.g., `nf66`) from deployment outputs
- [ ] **Save Resource Names**: Copy the API and MCP app service names

---

## ⚙️ **Phase 3: Configure VS Code** (2 minutes)

### **✅ Development Environment**
- [ ] **Clone Your Fork**: `git clone https://github.com/YOUR-USERNAME/Fabrikam-Project.git`
- [ ] **Run Configuration Script**:
  ```powershell
  cd Fabrikam-Project
  .\deployment\Configure-VSCodeSettings.ps1 -ResourceGroup "your-resource-group-name"
  ```
- [ ] **Verify Settings**: Check that `.vscode/settings.json` has your instance URLs

---

## 🔄 **Phase 4: Setup CI/CD** (5 minutes)

### **✅ Choose Your Method**

#### **🎯 Option A: Azure Portal (Recommended)**
- [ ] **API Service Setup**:
  1. Azure Portal → Your Resource Group → API App Service  
  2. Deployment Center → GitHub → Building with GitHub Actions
  3. Repository: `Fabrikam-Project` → Branch: `main`
  4. Runtime: `.NET 9.0` → Auth: `User-assigned identity` → Save
- [ ] **MCP Service Setup**:
  1. Azure Portal → Your Resource Group → MCP App Service
  2. Same configuration as API service → Save  
- [ ] **Verify**: Check GitHub Actions tab (2 new workflows running)

#### **⚡ Option B: PowerShell (Advanced)**
- [ ] **Run Setup Script**:
  ```powershell
  .\deployment\Setup-GitHubActions.ps1 `
    -ResourceGroup "your-resource-group" `
    -GitHubRepository "https://github.com/YOUR-USERNAME/Fabrikam-Project" `
    -GitHubToken "your-github-pat" `
    -SetupFork
  ```

---

## 🧪 **Phase 5: Verify Deployment** (1 minute)

### **✅ Test Your Instance**
- [ ] **Check API Health**: Visit `https://your-api-app.azurewebsites.net/health`
  - ✅ Should return: `{"status": "Healthy"}`
- [ ] **Check MCP Status**: Visit `https://your-mcp-app.azurewebsites.net/status`  
  - ✅ Should return: `{"status": "Running"}`
- [ ] **GitHub Actions**: Verify workflows completed successfully
- [ ] **Application Insights**: Check telemetry is flowing in Azure Portal

---

## 🎉 **Success! You're Ready**

### **✅ What You Now Have**
- ✅ **Complete Fabrikam Instance** running in Azure
- ✅ **Automatic Deployments** via GitHub Actions
- ✅ **Monitoring & Logging** with Application Insights
- ✅ **Custom Domain Support** (if configured)
- ✅ **VS Code Environment** configured for development

### **✅ Next Steps**
- [ ] **Explore the API**: Use the REST Client extension with `api-tests.http`
- [ ] **Test MCP Tools**: Use GitHub Copilot Chat to interact with your MCP server
- [ ] **Make Changes**: Edit code → push to main → automatic deployment
- [ ] **Stay Updated**: Your fork will sync updates from upstream repository

---

## 🔧 **Common Issues & Solutions**

### **❌ "Deploy to Azure" Button Doesn't Work**
- ✅ **Solution**: Use Azure CLI deployment method from README
- ✅ **Alternative**: Download ARM template and upload manually in Portal

### **❌ GitHub Authorization Fails**
- ✅ **Solution**: Check PAT permissions include `repo`, `workflow`, `admin:repo_hook`
- ✅ **Alternative**: Try "Basic Authentication" instead of managed identity

### **❌ Apps Show Azure Welcome Page**
- ✅ **Solution**: Wait 5-10 minutes after first deployment
- ✅ **Check**: Verify GitHub Actions completed successfully
- ✅ **Debug**: Check App Service logs in Azure Portal

### **❌ VS Code Configuration Script Fails**
- ✅ **Solution**: Ensure you're logged into Azure CLI: `az login`
- ✅ **Check**: Verify resource group name is correct
- ✅ **Manual**: Copy settings from deployment outputs if needed

---

## 📚 **Documentation Links**

- **[Main Deployment Guide](README.md)** - Complete ARM template documentation
- **[Portal CI/CD Setup](PORTAL-CICD-SETUP.md)** - Step-by-step Azure Portal configuration
- **[CI/CD Strategy](CI-CD-STRATEGY.md)** - Advanced automation and fork management
- **[Project Architecture](../docs/architecture/)** - System design and technical details

---

## 🏆 **Pro Tips**

### **🔄 Keeping Updated**
- ✅ **GitHub Sync**: Use the "Sync fork" button regularly
- ✅ **Automatic Updates**: Consider setting up the automated sync workflow
- ✅ **Monitor Upstream**: Watch the original repository for new features

### **🚀 Scaling Up**
- ✅ **Production Deployment**: Change SKU to S1 or higher
- ✅ **Multiple Environments**: Deploy dev/staging/prod with different environment parameters
- ✅ **Custom Branding**: Modify the fork with your own branding and features

### **👥 Team Collaboration**
- ✅ **Multiple Contributors**: Add team members to your GitHub fork
- ✅ **Branch Protection**: Set up PR reviews for the main branch
- ✅ **Environment Secrets**: Use Azure Key Vault for sensitive configuration

**🎯 Total Time: ~15 minutes to a fully working, automatically deploying Fabrikam instance!**
