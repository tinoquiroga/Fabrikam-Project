# 🤖 **GitHub Copilot CI/CD Quick Reference**

## ✅ **Always Remember When Coding:**

1. **This project has automated CI/CD pipelines**
2. **Every push/PR triggers automated testing**
3. **Code must build and pass tests in GitHub Actions**
4. **Deployments are automated to Azure App Services**

## 🚀 **Current Deployment Status:**
- **API**: Azure App Service (fabrikam-api-dev-izbd)
- **MCP**: Azure App Service (fabrikam-mcp-dev-izbd)
- **Testing**: Automated via GitHub Actions
- **Monitoring**: Azure Application Insights

## 📋 **Before Suggesting Code Changes:**
- [ ] Will this compile in .NET 9.0?
- [ ] Are async/await patterns maintained?
- [ ] Will existing tests still pass?
- [ ] Does this follow Azure deployment patterns?
- [ ] Are new features covered by tests?

## 🔗 **Full Documentation:**
- See `.github/copilot-cicd-context.md` for complete CI/CD details
- See `.github/copilot-instructions.md` for coding standards
