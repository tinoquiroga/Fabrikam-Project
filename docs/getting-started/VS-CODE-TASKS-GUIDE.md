# VS Code Tasks Template

This is the standardized VS Code tasks configuration for Fabrikam Project instances.

## 🎯 **Task Overview**

| Task | Purpose | Panel | Notes |
|------|---------|-------|-------|
| 🚀 Start API Server | Run API in dedicated terminal | Dedicated | Background task, shows all output |
| 🤖 Start MCP Server | Run MCP in dedicated terminal | Dedicated | Background task, shows all output |
| 🌐 Start Both Servers | Run both servers in parallel | Dedicated | **Default build task** |
| 🏗️ Build Solution | Build entire solution | Shared | Clean output panel |
| 🧪 Quick Test | Run Test-Development.ps1 -Quick | Shared | Fast validation |
| 🧪 Full Tests | Run Test-Development.ps1 -Verbose | Shared | Comprehensive testing |
| 👁️ Test with Visible Servers | Run tests with visible server windows | Shared | Uses `-Visible` parameter |
| 📊 Project Status | Show server status | Shared | Uses Manage-Project.ps1 |
| 🛑 Stop All Servers | Stop all running servers | Shared | Uses Manage-Project.ps1 |
| 🧹 Auto Cleanup Empty Files | Clean VS Code placeholders | Silent | **Runs on folder open** |

## 📋 **Quick Access**

**Keyboard shortcuts:**
- `Ctrl+Shift+P` → `Tasks: Run Task`
- `Ctrl+Shift+P` → `Tasks: Run Build Task` (runs 🌐 Start Both Servers)

**Most common workflows:**
1. **🌐 Start Both Servers** - Daily development
2. **👁️ Test with Visible Servers** - When you need to see server output
3. **🛑 Stop All Servers** - Clean shutdown

## 🔧 **Alignment with PowerShell Scripts**

The tasks align perfectly with PowerShell script parameters:

| Task | PowerShell Equivalent |
|------|----------------------|
| 🧪 Quick Test | `.\Test-Development.ps1 -Quick` |
| 🧪 Full Tests | `.\Test-Development.ps1 -Verbose` |
| 👁️ Test with Visible Servers | `.\Test-Development.ps1 -Visible -Quick` |
| 📊 Project Status | `.\Manage-Project.ps1 status` |
| 🛑 Stop All Servers | `.\Manage-Project.ps1 stop` |

## 🔧 **Template Usage**

To use this template in a new Fabrikam instance:

1. Copy `vscode-tasks-template.json` to `.vscode/tasks.json`
2. Copy launch settings templates to `{Project}/src/Properties/launchSettings.json`
3. **Trust HTTPS certificate**: Run `dotnet dev-certs https --trust`
4. Ensure scripts exist: `Test-Development.ps1`, `Manage-Project.ps1`, `scripts/Clean-VSCodePlaceholders.ps1`
5. **Important**: Make sure `launchSettings.json` is NOT in your `.gitignore` files

### 📋 **Required Setup Files:**
- `FabrikamApi/src/Properties/launchSettings.json`
- `FabrikamMcp/src/Properties/launchSettings.json`
- **Trusted HTTPS certificate** (see `HTTPS-CERTIFICATE-QUICK-FIX.md`)

See `LAUNCH-SETTINGS-GUIDE.md` for the exact content of these files.

## 🏗️ **Architecture Benefits**

- **Dedicated terminals** for servers = isolated output
- **Shared terminal** for builds/tests = consolidated output  
- **Background tasks** for servers = VS Code integration
- **Auto-cleanup** on folder open = clean development environment
- **PowerShell script integration** = consistent tooling

This template maintains the same functionality across all Fabrikam instances while leveraging VS Code's task system for better developer experience.
