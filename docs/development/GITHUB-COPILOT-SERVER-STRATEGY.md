# 🤖 GitHub Copilot Server Management Strategy

## 📋 **Problem Statement**

GitHub Copilot frequently needs to run commands that interfere with running servers by:
- Starting servers in terminals that should be kept free for commands
- Running commands in the same terminal where servers are running (stopping them)
- Inconsistent server management leading to port conflicts

## 🎯 **Solution: VS Code Tasks + Dedicated Terminals**

### **Strategy Overview**

1. **VS Code Tasks for Servers**: Use dedicated VS Code tasks to start/stop servers
2. **Free Terminals for Commands**: Keep regular terminals available for GitHub Copilot commands
3. **Clear Task Commands**: Provide simple, consistent task commands for server management

## 🔧 **Implementation**

### **Server Management Tasks**

| Task Name | Purpose | Terminal Type | GitHub Copilot Usage |
|-----------|---------|---------------|---------------------|
| `🚀 Start API Server` | Start API on port 7297 | Dedicated background | Use for API server |
| `🤖 Start MCP Server` | Start MCP on ports 5000/5001 | Dedicated background | Use for MCP server |
| `🌐 Start Both Servers` | Start both servers parallel | Dedicated background | Use for full stack |
| `🛑 Stop All Servers` | Stop all running servers | Shared terminal | Use to clean up |

### **Testing & Command Tasks**

| Task Name | Purpose | Terminal Type | GitHub Copilot Usage |
|-----------|---------|---------------|---------------------|
| `🏗️ Build Solution` | Build entire solution | Shared terminal | Use before starting servers |
| `⚡ Quick Test` | Fast health check | Shared terminal | Use for quick validation |
| `🧪 Full Tests` | Comprehensive testing | Shared terminal | Use for full validation |
| `📊 Project Status` | Check server status | Shared terminal | Use to check what's running |

## 🎮 **GitHub Copilot Usage Patterns**

### **✅ CORRECT: Server Management Pattern**

```powershell
# 1. Build first (if needed)
# Use VS Code Task: Ctrl+Shift+P → "Tasks: Run Task" → "🏗️ Build Solution"

# 2. Start servers using VS Code tasks (NOT terminal commands)
# Use VS Code Task: Ctrl+Shift+P → "Tasks: Run Task" → "🌐 Start Both Servers"

# 3. Run commands in FREE terminals (not server terminals)
# These are safe to use in any terminal:
curl -k https://localhost:7297/api/info/auth
Invoke-RestMethod -Uri "https://localhost:7297/api/info" -SkipCertificateCheck
./test.ps1 -Quick
dotnet test FabrikamTests/

# 4. Stop servers when done
# Use VS Code Task: Ctrl+Shift+P → "Tasks: Run Task" → "🛑 Stop All Servers"
```

### **❌ INCORRECT: Terminal Management Anti-Patterns**

```powershell
# DON'T: Start servers in regular terminals
dotnet run --project FabrikamApi/src/FabrikamApi.csproj  # GitHub Copilot shouldn't do this

# DON'T: Run commands in server terminals
# This stops the server and causes port conflicts

# DON'T: Start multiple instances
# Check status first: Use VS Code Task "📊 Project Status"
```

## 🤖 **GitHub Copilot Integration Instructions**

### **For Server Management**

When GitHub Copilot needs to start servers:

```markdown
**ALWAYS use VS Code Tasks for server management:**

1. **To start servers**: Use VS Code Task "🌐 Start Both Servers"
2. **To check status**: Use VS Code Task "📊 Project Status" 
3. **To stop servers**: Use VS Code Task "🛑 Stop All Servers"
4. **Never use**: `dotnet run` commands directly in terminals

**How to run VS Code Tasks:**
- Use the `run_task` tool with task names from .vscode/tasks.json
- Task names: "🚀 Start API Server", "🤖 Start MCP Server", "🌐 Start Both Servers"
```

### **For Testing & Commands**

When GitHub Copilot needs to run commands:

```markdown
**ALWAYS use separate terminals for commands:**

1. **Safe commands** (can run in any terminal):
   - `curl -k https://localhost:7297/api/info`
   - `Invoke-RestMethod -Uri "https://localhost:7297/api/info" -SkipCertificateCheck`
   - `./test.ps1 -Quick`
   - `dotnet test FabrikamTests/`
   - `netstat -ano | findstr :7297`

2. **Use run_in_terminal tool** with `isBackground: false` for commands
3. **Never run commands** in the same terminal as servers
```

## 🛠️ **Task Definitions Reference**

### **Core Server Tasks**

```json
{
    "label": "🚀 Start API Server",
    "type": "shell",
    "command": "dotnet",
    "args": ["run", "--project", "FabrikamApi/src/FabrikamApi.csproj", "--launch-profile", "https"],
    "isBackground": true,
    "presentation": { "panel": "dedicated" }
}
```

```json
{
    "label": "🤖 Start MCP Server",
    "type": "shell", 
    "command": "dotnet",
    "args": ["run", "--project", "FabrikamMcp/src/FabrikamMcp.csproj", "--launch-profile", "https"],
    "isBackground": true,
    "presentation": { "panel": "dedicated" }
}
```

```json
{
    "label": "🌐 Start Both Servers",
    "dependsOrder": "parallel",
    "dependsOn": ["🚀 Start API Server", "🤖 Start MCP Server"]
}
```

## 📊 **Verification Commands**

### **Check Server Status**

```powershell
# Check API server (port 7297)
netstat -ano | findstr :7297

# Check MCP server (ports 5000/5001)
netstat -ano | findstr :5000
netstat -ano | findstr :5001

# Test API endpoint
curl -k https://localhost:7297/api/info

# Test MCP endpoint  
curl -k http://localhost:5000/mcp
```

### **Emergency Cleanup**

```powershell
# Stop all dotnet processes (emergency only)
Get-Process dotnet | Stop-Process -Force

# Or use the proper task
# VS Code Task: "🛑 Stop All Servers"
```

## 🎯 **Authentication Mode Testing**

### **Current Configuration**

- **File**: `FabrikamApi/src/appsettings.Development.json`
- **Mode**: `"Authentication.Mode": "BearerToken"`
- **Expected API Response**: `{"mode": "BearerToken"}`

### **Testing Pattern**

```powershell
# 1. Start servers using VS Code Task
# VS Code Task: "🌐 Start Both Servers"

# 2. Test authentication endpoint in separate terminal
$response = Invoke-RestMethod -Uri "https://localhost:7297/api/info/auth" -SkipCertificateCheck
$response | ConvertTo-Json -Depth 10

# Expected output:
# {
#   "mode": "BearerToken",
#   "description": "JWT token-based authentication...",
#   "isEnabled": true
# }
```

## 🚀 **Quick Start Guide**

### **For GitHub Copilot**

```markdown
**Step 1**: Build (if needed)
- Run VS Code Task: "🏗️ Build Solution"

**Step 2**: Start Servers  
- Run VS Code Task: "🌐 Start Both Servers"

**Step 3**: Run Commands (in separate terminals)
- Use `run_in_terminal` tool with `isBackground: false`
- Never use terminals where servers are running

**Step 4**: Stop Servers (when done)
- Run VS Code Task: "🛑 Stop All Servers"
```

### **For Manual Development**

1. **Ctrl+Shift+P** → "Tasks: Run Task" → "🌐 Start Both Servers"
2. **Open new terminal** for commands
3. **Ctrl+Shift+P** → "Tasks: Run Task" → "🛑 Stop All Servers" when done

## 📝 **Troubleshooting**

### **Port Already in Use**

```powershell
# Check what's using ports
netstat -ano | findstr :7297
netstat -ano | findstr :5000

# Kill specific process (replace PID)
taskkill /F /PID <PID>

# Or use cleanup task
# VS Code Task: "🛑 Stop All Servers"
```

### **Server Not Responding**

```powershell
# Check if servers are actually running
# VS Code Task: "📊 Project Status"

# Restart servers
# VS Code Task: "🛑 Stop All Servers"
# Then: VS Code Task: "🌐 Start Both Servers"
```

## 🎯 **Success Metrics**

✅ **Servers run in dedicated terminals**  
✅ **Commands run in separate terminals**  
✅ **No port conflicts**  
✅ **Consistent server management**  
✅ **GitHub Copilot can test without breaking servers**

---

**Remember**: VS Code Tasks for servers, separate terminals for commands!
