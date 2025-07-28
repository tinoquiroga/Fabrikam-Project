# 🗂️ Repository Organization Plan

**Date**: July 27, 2025  
**Goal**: Clean repository structure with logical organization

## 📋 **Files to Move**

### **🗂️ Move to `docs/` directory**
- `AUTHENTICATION-IMPLEMENTATION-COMPLETE.md` → `docs/development/AUTHENTICATION-IMPLEMENTATION-COMPLETE.md`
- `AUTHENTICATION-IMPLEMENTATION-SUMMARY.md` → `docs/development/AUTHENTICATION-IMPLEMENTATION-SUMMARY.md` 
- `BUSINESS-MODEL-SUMMARY.md` → `docs/architecture/BUSINESS-MODEL-SUMMARY.md`
- `CICD-TESTING-PLAN.md` → `docs/deployment/CICD-TESTING-PLAN.md`
- `COMMIT-SUMMARY.md` → `docs/development/COMMIT-SUMMARY.md`
- `DEPLOY-TO-AZURE.md` → `docs/deployment/DEPLOY-TO-AZURE.md`
- `DOCUMENTATION-INDEX.md` → `docs/DOCUMENTATION-INDEX.md`

### **📜 Move to `scripts/` directory**  
- `apply-issue4-migration.ps1` → `scripts/apply-issue4-migration.ps1`
- `Demo-Authentication.ps1` → `scripts/Demo-Authentication.ps1`
- `Manage-Project.ps1` → `scripts/Manage-Project.ps1`
- `quick-migration-fix.ps1` → `scripts/quick-migration-fix.ps1`
- `Setup-Instance.ps1` → `scripts/Setup-Instance.ps1`
- `Test-Development.ps1` → `scripts/Test-Development.ps1`

### **🗄️ Move to `archive/` directory**
- `issue4-manual-migration.sql` → `archive/old-sql-files/issue4-manual-migration.sql`
- `migration_script.sql` → `archive/old-sql-files/migration_script.sql`
- `SESSION-SUMMARY-ISSUE-ALIGNMENT.md` → `archive/session-summaries/SESSION-SUMMARY-ISSUE-ALIGNMENT.md`
- `PROJECT-STATUS-SUMMARY.md` → Keep in root (active status document)

### **🔧 Keep in Root (Essential Files)**
- `README.md` - Main project documentation
- `LICENSE.md` - License information  
- `CHANGELOG.md` - Version history
- `CONTRIBUTING.md` - Contribution guidelines
- `PROJECT-STATUS-SUMMARY.md` - Current project status
- `api-tests.http` - Quick API testing
- `trigger-autofix.txt` - CI/CD trigger

### **🧹 Files Already Well-Organized**
- `.github/` - GitHub configuration and workflows ✅
- `.vscode/` - VS Code settings ✅  
- `docs/` - Comprehensive documentation ✅
- `scripts/` - Utility scripts ✅
- `archive/` - Archived content ✅
- `deployment/` - Deployment resources ✅
- Project folders (`FabrikamApi/`, `FabrikamMcp/`, etc.) ✅

## 🎯 **Expected Benefits**

### **Cleaner Root Directory**
- Only essential project files in root
- Clear navigation for new contributors
- Reduced cognitive load when browsing

### **Better Organization**
- Documentation centralized in `docs/`
- Scripts centralized in `scripts/`
- Historical content in `archive/`
- Logical grouping by function

### **Improved Developer Experience**
- Easier to find relevant files
- Clear separation of concerns
- Better IDE/editor navigation
- Consistent with monorepo best practices

## 📂 **Final Root Structure**
```
├── README.md                      # Main documentation
├── LICENSE.md                     # License
├── CHANGELOG.md                   # Version history
├── CONTRIBUTING.md                # Contribution guide
├── PROJECT-STATUS-SUMMARY.md     # Current status
├── api-tests.http                 # Quick API tests
├── trigger-autofix.txt           # CI/CD trigger
├── Fabrikam.sln                  # Solution file
├── .github/                      # GitHub configuration
├── .vscode/                      # VS Code settings
├── docs/                         # All documentation
├── scripts/                      # All utility scripts
├── archive/                      # Historical content
├── deployment/                   # Deployment resources
├── FabrikamApi/                  # API project
├── FabrikamMcp/                  # MCP project
├── FabrikamContracts/            # Shared contracts
└── FabrikamTests/                # Test project
```

## ✅ **Execution Steps**
1. Create new subdirectories as needed
2. Move files to new locations
3. Update any references in documentation
4. Test that scripts still work from new locations
5. Commit organized structure
