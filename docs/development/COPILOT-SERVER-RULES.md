# 🤖 GITHUB COPILOT - READ THIS FIRST

## 🚨 **CRITICAL SERVER MANAGEMENT RULES**

**⚠️ TO AVOID BREAKING SERVERS, ALWAYS FOLLOW THESE PATTERNS ⚠️**

### **✅ CORRECT: Use VS Code Tasks for Servers**

```powershell
# Start servers (ALWAYS use run_task tool)
run_task(id: "🌐 Start Both Servers", workspaceFolder: "d:\1Repositories\Fabrikam-Project")

# Stop servers (ALWAYS use run_task tool)  
run_task(id: "🛑 Stop All Servers", workspaceFolder: "d:\1Repositories\Fabrikam-Project")

# Check status (ALWAYS use run_task tool)
run_task(id: "📊 Project Status", workspaceFolder: "d:\1Repositories\Fabrikam-Project")
```

### **✅ CORRECT: Use Separate Terminals for Commands**

```powershell
# Test API (ALWAYS use run_in_terminal with isBackground: false)
run_in_terminal(
    command: "curl -k https://localhost:7297/api/info/auth",
    isBackground: false
)

# PowerShell testing (ALWAYS use run_in_terminal with isBackground: false)
run_in_terminal(
    command: "$response = Invoke-RestMethod -Uri 'https://localhost:7297/api/info' -SkipCertificateCheck; $response | ConvertTo-Json",
    isBackground: false
)

# Run tests (ALWAYS use run_in_terminal with isBackground: false)
run_in_terminal(
    command: "./test.ps1 -Quick",
    isBackground: false
)
```

### **❌ NEVER DO THESE THINGS**

```powershell
# ❌ DON'T: Start servers with run_in_terminal
run_in_terminal(command: "dotnet run --project FabrikamApi/src/FabrikamApi.csproj")  # BREAKS STRATEGY

# ❌ DON'T: Run commands in server terminals  
# This stops the server and causes port conflicts!

# ❌ DON'T: Mix server and command operations in same terminal
```

## 🎯 **Why This Matters**

1. **VS Code Tasks** create dedicated terminals for servers
2. **run_in_terminal** creates separate terminals for commands
3. **This prevents** commands from stopping servers
4. **Result**: Stable servers + successful command testing

## 📊 **Available VS Code Tasks**

| Task ID | Purpose | When to Use |
|---------|---------|-------------|
| `🌐 Start Both Servers` | Start API + MCP servers | Full stack testing |
| `🚀 Start API Server` | Start API server only | API-only testing |
| `🤖 Start MCP Server` | Start MCP server only | MCP-only testing |
| `🛑 Stop All Servers` | Stop all servers | Cleanup/restart |
| `📊 Project Status` | Check what's running | Status verification |
| `🏗️ Build Solution` | Build entire solution | Before starting servers |

## 🔧 **Quick Troubleshooting**

### **Port Already in Use**
```powershell
# Check what's using ports
run_in_terminal(command: "netstat -ano | findstr :7297", isBackground: false)

# Emergency cleanup
run_in_terminal(command: "Get-Process dotnet | Stop-Process -Force", isBackground: false)
```

### **Servers Not Responding**
```powershell
# Stop and restart
run_task(id: "🛑 Stop All Servers")
run_task(id: "🌐 Start Both Servers")
```

## 🎮 **Step-by-Step Example**

```powershell
# 1. Stop any existing servers
run_task(id: "🛑 Stop All Servers", workspaceFolder: "d:\1Repositories\Fabrikam-Project")

# 2. Start both servers
run_task(id: "🌐 Start Both Servers", workspaceFolder: "d:\1Repositories\Fabrikam-Project")

# 3. Wait a moment, then test in SEPARATE terminal
run_in_terminal(
    command: "Start-Sleep 5; curl -k https://localhost:7297/api/info",
    isBackground: false
)

# 4. When done, stop servers
run_task(id: "🛑 Stop All Servers", workspaceFolder: "d:\1Repositories\Fabrikam-Project")
```

---

**🔗 Full Documentation**: `docs/development/GITHUB-COPILOT-SERVER-STRATEGY.md`
**🔗 Project Instructions**: `.github/copilot-instructions.md`

**Remember**: VS Code Tasks for servers, separate terminals for commands!
