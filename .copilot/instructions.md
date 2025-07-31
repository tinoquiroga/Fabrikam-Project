# GitHub Copilot Instructions for Fabrikam Project

## 🚨 CRITICAL: Server Management Protocol

### Rule #1: VS Code Tasks ONLY for Servers
```
✅ ALWAYS use run_task tool for servers:
- Start: run_task(id: "🌐 Start Both Servers")
- Stop: run_task(id: "🛑 Stop All Servers")
- Status: run_task(id: "📊 Project Status")

❌ NEVER use run_in_terminal for: dotnet run --project
```

### Rule #2: Separate Terminals for Commands
```
✅ ALWAYS use run_in_terminal (isBackground: false) for:
- curl commands
- Invoke-RestMethod commands  
- ./test.ps1 commands
- dotnet test commands

❌ NEVER run commands in server terminals
```

### Rule #3: Server-Command Separation
```
SERVER TERMINALS: Only for servers (via VS Code tasks)
COMMAND TERMINALS: Only for commands (via run_in_terminal)
NEVER MIX: Commands will stop servers!
```

## Available Tasks
- `🌐 Start Both Servers` - Start API + MCP
- `🛑 Stop All Servers` - Stop all servers
- `📊 Project Status` - Check what's running
- `🚀 Start API Server` - API only
- `🤖 Start MCP Server` - MCP only

## Safe Commands (Separate Terminal)
- `curl -k https://localhost:7297/api/info`
- `Invoke-RestMethod -Uri "https://localhost:7297/api/info" -SkipCertificateCheck`
- `./test.ps1 -Quick`
- `dotnet test FabrikamTests/`
- `netstat -ano | findstr :7297`

## Project Structure
- **Monorepo**: Always work from workspace root
- **API Server**: Port 7297 (FabrikamApi)
- **MCP Server**: Ports 5000/5001 (FabrikamMcp)
- **Configuration**: Authentication.Mode = "BearerToken"

## Authentication Testing
```powershell
# Correct pattern:
run_task(id: "🌐 Start Both Servers")
# Wait for startup...
run_in_terminal(
    command: "$response = Invoke-RestMethod -Uri 'https://localhost:7297/api/info/auth' -SkipCertificateCheck; $response | ConvertTo-Json",
    isBackground: false
)
```

See: COPILOT-SERVER-RULES.md for full details
