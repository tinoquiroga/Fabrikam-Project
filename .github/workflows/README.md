# 🔄 GitHub Actions Workflows

This directory contains the core CI workflows for the Fabrikam project. **Deployment workflows are automatically created by Azure Portal's Deployment Center.**

## 📋 **Current Workflows**

### 🧪 **`testing.yml`** - Continuous Integration
- **Triggers**: Push to `main`/`develop`, Pull Requests to `main`
- **Purpose**: Runs automated tests to ensure code quality
- **Actions**: Build solution, run unit tests, validate integration tests
- **Status**: Always runs on code changes

### 🔐 **`authentication-validation.yml`** - Security Validation  
- **Triggers**: Pull Requests affecting auth code, Manual dispatch
- **Purpose**: Security analysis and authentication pattern validation
- **Actions**: Security scanning, auth flow validation, vulnerability checks
- **Status**: Runs on auth-related changes

## 🚀 **Deployment Workflows** (Auto-Generated)

When you set up CI/CD via **Azure Portal > Deployment Center**, Azure automatically creates deployment workflows with names like:
- `main_fabrikam-api-development-[suffix].yml` (API deployment)
- `main_fabrikam-mcp-development-[suffix].yml` (MCP deployment)

These workflows are **automatically configured** with:
- ✅ Correct build paths for monorepo structure
- ✅ Proper Azure credentials and service principal
- ✅ Environment-specific deployment targets
- ✅ Optimized build and deployment steps

## 🎯 **Workshop Benefits**

This clean separation provides:
1. **CI**: Core testing and validation (runs on every change)
2. **CD**: Deployment automation (configured per environment via Portal)
3. **Clean Fork**: Workshop participants get minimal, focused workflows
4. **No Conflicts**: Portal-generated workflows don't interfere with existing ones

## 💡 **For Workshop Participants**

When you fork this repository, you'll start with:
- ✅ **2 essential workflows** for testing and security
- ✅ **Clean slate** for deployment - Portal will add your specific deployment workflows
- ✅ **No confusion** about which workflows to modify or ignore

After setting up Deployment Center, you'll have:
- ✅ **4 total workflows**: 2 core + 2 deployment (API + MCP)
- ✅ **All properly configured** for your specific Azure environment
- ✅ **Ready for development** - make changes and watch them deploy automatically!

---

**🔧 Technical Note**: The Portal-generated workflows are optimized for the monorepo structure and handle the `--project` flags correctly for building the right components.
