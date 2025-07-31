# GitHub Copilot - Claude Desktop MCP Configuration Conflict Bug Report

## 🐛 **Issue Summary**

GitHub Copilot Chat (Ctrl+I) stops working when Claude Desktop has ANY MCP server configu## 🚀 **Reproduction Script**rs because VS Code automatically discovers MCP servers from Claude Desktop's configuration file and attempts to start them, creating a conflict that prevents Copilot Chat from functioning across ALL VS Code projects.

**Confirmed Issue Timeline**: Weekend of July 26-27, 2025 on Windows 11 Enterprise system.

## 📋 **Bug Details**

### **Environment Information**
- **OS**: Windows 11 Enterprise (Build 26100.1.amd64fre.ge_release.240331-1435)
- **VS Code Version**: 1.102.2 (c306e94f98122556ca081f527b466015e1bc37b0, x64)
- **GitHub Copilot Extension Version**: 1.346.0
- **GitHub Copilot Chat Extension Version**: 0.29.1
- **Azure GitHub Copilot Extension Version**: 1.0.58
- **Claude Desktop Version**: 0.12.55
- **User Profile**: [userprofile]
- **Report Date**: July 28, 2025 (Issue occurred weekend of July 26-27, 2025)

### **Configuration Details**
- **Claude Desktop Config**: `C:\Users\[userprofile]\AppData\Roaming\Claude\claude_desktop_config.json`
- **MCP Server**: CIPP-MCP Proxy (FastMCP stdio-to-http bridge for Claude Desktop)
- **Script Path**: `C:\Users\[userprofile]\1Repositories\CIPP-Project\CIPP-MCP\Proxy\cipp.local_mcp.py`

## 🎯 **Root Cause Analysis**

**The core issue appears to be:**

1. **Claude Desktop configuration** defines MCP servers for Claude's use
2. **VS Code/GitHub Copilot automatically discovers** MCP servers from Claude's config file
3. **VS Code attempts to start** the MCP server (FastMCP based MCP Proxy) for its own use
4. **Backend MCP server is not running** because it was only intended for Claude Desktop
5. **Conflict occurs** when GitHub Copilot tries to use a partially functional MCP setup

**Key observation**: VS Code shows the MCP server as "running" even when the backend service isn't available, and attempts to automatically restart the MCP process when stopped. Current behavior shows MCP icon in Copilot Chat endlessly "discovering tools" rather than complete failure.

**Architecture Problem**: The FastMCP Proxy is designed as a stdio-to-http bridge for Claude Desktop, but VS Code treats it as a standalone MCP server, creating a broken state where:
- Proxy process starts successfully (appears "running" to VS Code)
- Backend MCP endpoint is not running 
- Proxy cannot fulfill MCP requests, causing Copilot Chat to malfunction or enter infinite discovery loop
- **VS Code automatically restarts the proxy** when manually stopped, creating persistent conflict

## 🔄 **Reproduction Steps**

### **Prerequisites**
1. Install VS Code with GitHub Copilot extension
2. Install Claude Desktop application
3. Have a MCP server configured for Claude Desktop

**Note**: The specific MCP server doesn't matter - ANY MCP configuration in Claude Desktop triggers this conflict but it is especially apparent if the MCP server is not functioning or in development.

### **Exact Configuration That Caused the Issue**
The problematic Claude Desktop configuration that causes the endless discovering tools loop is below. Another variation breaks copilot chat completely:

```json
{
  "mcpServers": {
    "CIPP-MCP Proxy": {
      "command": "python",
      "args": ["C:\\Users\\[userprofile]\\1Repositories\\CIPP-Project\\CIPP-MCP\\Proxy\\cipp.local_mcp.py"],
      "env": {
        "PYTHONPATH": "C:\\Users\\[userprofile]\\1Repositories\\CIPP-Project\\CIPP-MCP\\Proxy"
      }
    }
  }
}
```

**Note**: The original problematic configuration may have used a `run` command instead of `command`/`args`. This one prevented copilot chat from responding at all. That chat interface was available but it would simply ignore any converstatons. As soon as I would stop the CIPP-MCP copilot chat would immediately respond to whatever I asked it last and keep working for a few minutes until VS Code restarted CIPP-MCP. The current configuration still causes MCP discovery issues in VS Code, demonstrating that any Claude Desktop MCP configuration triggers the conflict.

### **Steps to Reproduce**
1. **Verify Copilot Working**: Open VS Code, test GitHub Copilot chat (Ctrl+I) and inline suggestions
2. **Add Claude Desktop MCP Config**: Edit the config file above with the problematic configuration
4. **Test Copilot**: Return to VS Code and observe Copilot Chat malfunction

### **Expected Behavior**
- GitHub Copilot should ignore Claude Desktop MCP configurations or have the ability to toggle them on/off
- VS Code should not attempt to start MCP servers from Claude Desktop's config unless explitly requested by the user
- Both tools should operate independently without cross-contamination of configurations

### **Actual Behavior** *(Confirmed on July 26-27, 2025)*
- VS Code/GitHub Copilot discovers MCP server from Claude Desktop config immediately upon VS Code startup
- **Current behavior**: MCP icon appears in Copilot Chat but shows "discovering tools" indefinitely
- **Previous severe behavior**: GitHub Copilot Chat would accept input but would not respond at all until I 'stopped' CIPP-MCP Proxy from VS Code
- **Configuration dependency**: Removing Claude Desktop config immediately restores Copilot functionality
- **Restart cycle**: Restoring the config breaks Copilot again, confirming direct correlation
- **Automatic restart issue**: Even when manually stopping the CIPP-MCP Proxy process, VS Code automatically restarts it within minutes
- **Persistent conflict**: VS Code continues attempting to manage the MCP server, creating an ongoing conflict state
- The conflict persists until Claude Desktop config is completely removed

## 🔍 **Diagnostic Information**

### **Files to Check During Reproduction**
- `C:\Users\[userprofile]\AppData\Roaming\Claude\claude_desktop_config.json` (Claude config)
- VS Code settings: `%APPDATA%\Code\User\settings.json`
- VS Code extension logs: `%APPDATA%\Code\logs\`
- GitHub Copilot extension logs: Check VS Code Developer Tools > Console
- Windows Event Viewer: Application and System logs for any MCP or Python process errors
- Task Manager: Check for python.exe processes related to Claude MCP servers/proxies

### **Enhanced Logging for Bug Report**
To capture detailed logs for Microsoft:

1. **Start VS Code with trace logging**:
   ```bash
   code --log trace
   ```
   OR for Copilot-specific logging:
   ```bash
   code --log "github.copilot:trace"
   ```

2. **Log locations**:
   - Main logs: `%APPDATA%\Code\logs\[timestamp]_[session]\`
   - Extension logs: `%APPDATA%\Code\logs\[timestamp]_[session]\exthost\`
   
3. **Reproduce the issue** with logging enabled
4. **Collect logs** from the session folder for Microsoft

### **Potential Conflict Areas**
1. **Configuration Cross-Contamination**: VS Code reading Claude Desktop's MCP config
2. **Process Management**: VS Code trying to manage MCP servers intended for Claude
3. **Architecture Mismatch**: FastMCP Proxy designed for Claude's stdio ↔ HTTP bridge, not direct VS Code usage
4. **Resource Conflicts**: Proxy processes started without proper backend connections (real MCP server unavailable)
5. **Automatic Restart Logic**: VS Code persistently restarts stopped MCP processes, preventing manual resolution
6. **State Management**: Partial MCP initialization causing Copilot to enter infinite discovery loops
7. **IPC/Communication**: Interference between Copilot and improperly initialized MCP servers

## 🔍 **CIPP MCP Proxy Code** *(The Python script VS Code incorrectly tries to execute)*

The proxy script that VS Code attempts to start is a FastMCP bridge designed only for Claude Desktop:

```python
from fastmcp import FastMCP

# Create a proxy directly from a config dictionary
config = {
    "mcpServers": {
        "default": {  # For single server configs, 'default' is commonly used
            "url": "http://localhost:3001/mcp",
            "transport": "streamable-http"
        }
    }
}

# Create a proxy to the configured server
proxy = FastMCP.as_proxy(config, name="CIPP-MCP Proxy")

# Run the proxy with stdio transport for local access
if __name__ == "__main__":
    proxy.run()
```

**Architecture Issue**: This proxy is designed to:
1. Accept stdio input from Claude Desktop
2. Bridge to a streamable-http MCP server at `http://localhost:3001/mcp` or the real MCP endpoint
3. The backend MCP server (`localhost:3001`) is NOT running when VS Code starts the proxy
4. This creates a "zombie" proxy process that VS Code shows as "running" but is non-functional

## 📊 **Impact Assessment**

### **Severity**: High
- **Reason**: Completely breaks GitHub Copilot functionality
- **Workaround**: Remove Claude Desktop MCP configuration

### **User Impact** *(Real-World Impact)*
- Developers cannot use both GitHub Copilot and Claude Desktop with MCP servers simultaneously
- Forces choice between AI tools instead of allowing complementary usage
- Significant impact on development productivity, especially for users relying on Copilot Chat
- Issue occurs silently - users may not realize Claude Desktop MCP config is the cause
- Affects enterprise users who may have both tools configured for different purposes

## 🔧 **Workaround**

**Only complete removal of Claude Desktop MCP configuration provides permanent resolution:**
1. Edit `C:\Users\[userprofile]\AppData\Roaming\Claude\claude_desktop_config.json`
2. Remove the `mcpServers` section entirely or set to `{}`
3. Restart Claude Desktop application

**Note**: Manual process termination is insufficient - VS Code automatically restarts the MCP process within minutes.

## 🎯 **Expected Resolution**

- GitHub Copilot should work independently of Claude Desktop MCP configuration
- Both tools should be able to coexist without interference
- No configuration conflicts should occur between separate AI tools

