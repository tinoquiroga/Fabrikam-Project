# 🔧 Fabrikam Project Scripts

This directory contains utility scripts for development, testing, and data management that are used occasionally but don't need to be in the project root.

## 📋 Scripts Overview

### 🛠️ **Development & Testing Scripts**

#### **`Fix-Verification.ps1`**
- **Purpose**: Quick verification script for testing fixes
- **Usage**: `.\scripts\Fix-Verification.ps1`
- **When to use**: After making code changes to verify basic functionality
- **Output**: Validation results and system health check

#### **`Inject-Orders.ps1`**
- **Purpose**: Injects test order data into the system for development/testing
- **Usage**: `.\scripts\Inject-Orders.ps1`
- **When to use**: When you need to populate the system with sample order data
- **Dependencies**: Requires API server to be running

#### **`test-mcp-smart-fallback.ps1`**
- **Purpose**: Tests MCP server fallback mechanisms and error handling
- **Usage**: `.\scripts\test-mcp-smart-fallback.ps1`
- **When to use**: To validate MCP server resilience and failover behavior
- **Focus**: Smart fallback logic and error recovery patterns

## 🚀 **Primary Scripts (Remain in Root)**

These essential scripts stay in the project root for easy access:

- **`Test-Development-Modular.ps1`** - Main comprehensive testing suite
- **`Manage-Project.ps1`** - Primary project management (start/stop/status)

**Convenient Shortcuts Available**:
- **`test.ps1`** - PowerShell shortcut for `scripts\Test-Development-Modular.ps1` (**Recommended**)
- **`test.bat`** - Batch file shortcut for command prompt users (use if PowerShell not available)

Both shortcuts pass all parameters through to the main testing script.

**Note**: The PowerShell shortcut (`test.ps1`) is recommended as it handles Unicode properly and has been fully tested.

## 📁 **Demo Scripts (in `docs/demos/`)**

- **`Validate-Demo.ps1`** - Demo readiness validation (moved to `docs/demos/`)

## 💡 **Usage Patterns**

### **Development Workflow**
```powershell
# Daily development testing (root level)
.\test.ps1 -Quick

# After making fixes (utility scripts)
.\scripts\Fix-Verification.ps1

# When needing test data (utility scripts)
.\scripts\Inject-Orders.ps1

# Before demo preparation (demo scripts)
.\docs\demos\Validate-Demo.ps1
```

### **Script Categories**

| **Category** | **Location** | **Purpose** | **Frequency** |
|--------------|--------------|-------------|---------------|
| **Essential** | Root | Daily development commands | Daily |
| **Utility** | `scripts/` | Occasional development tasks | Occasional |
| **Demo** | `docs/demos/` | Demo preparation and validation | Demo prep |

## 🔗 **Integration with Main Scripts**

These utility scripts complement the main project scripts:

- **`Test-Development-Modular.ps1`** - Comprehensive testing and CI/CD integration
- **`Manage-Project.ps1`** - Server lifecycle management and project operations
- **`docs/demos/Validate-Demo.ps1`** - Demo environment validation

## 📝 **Adding New Scripts**

When adding new scripts to this directory:

1. **Document purpose** in this README
2. **Include usage examples** with parameters
3. **Specify dependencies** (API server, MCP server, etc.)
4. **Categorize appropriately** (development, testing, data management)
5. **Update integration points** with main project scripts

## 🧪 **Testing Script Reliability**

All scripts in this directory should:
- ✅ Handle errors gracefully
- ✅ Provide clear output messages
- ✅ Include usage instructions in comments
- ✅ Work from any directory when called with relative paths
- ✅ Not break main project functionality

---

**Note**: For daily development tasks, continue using the main scripts in the project root. These utility scripts are for specialized tasks and occasional use.
