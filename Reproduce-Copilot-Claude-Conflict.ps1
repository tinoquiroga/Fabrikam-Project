# Reproduce-Copilot-Claude-Conflict.ps1
# Script to systematically reproduce the GitHub Copilot + Claude Desktop MCP conflict

param(
    [switch]$Baseline,      # Test Copilot baseline only
    [switch]$FullTest,      # Run complete reproduction test
    [switch]$Cleanup        # Cleanup test environment
)

function Write-Step {
    param([string]$Message, [string]$Color = "Yellow")
    Write-Host ""
    Write-Host "🔸 $Message" -ForegroundColor $Color
    Write-Host ""
}

function Write-Action {
    param([string]$Message)
    Write-Host "  → $Message" -ForegroundColor Cyan
}

function Wait-ForUser {
    param([string]$Message = "Press Enter to continue...")
    Write-Host "  ⏸️  $Message" -ForegroundColor DarkYellow
    Read-Host | Out-Null
}

function Test-CopilotBaseline {
    Write-Step "Testing GitHub Copilot Baseline" "Green"
    Write-Action "1. Open VS Code in a new window"
    Write-Action "2. Create a new file (e.g., test.js or test.cs)"
    Write-Action "3. Type some code and check for Copilot suggestions"
    Write-Action "4. Try Ctrl+I for Copilot chat"
    Write-Action "5. Document if Copilot is working normally"
    Wait-ForUser
}

function Start-FabrikamMcp {
    Write-Step "Starting Fabrikam MCP Server" "Green"
    
    # Check if we're in the right directory
    if (-not (Test-Path "FabrikamMcp/src/FabrikamMcp.csproj")) {
        Write-Host "❌ Not in Fabrikam-Project directory!" -ForegroundColor Red
        Write-Host "Current location: $PWD"
        Write-Host "Please run this script from the Fabrikam-Project root directory."
        return $false
    }

    Write-Action "Starting MCP server at http://localhost:5000..."
    
    # Start MCP server in a new PowerShell window
    $mcpProcess = Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command", 
        "Write-Host 'Fabrikam MCP Server' -ForegroundColor Green; cd '$PWD'; dotnet run --project FabrikamMcp/src/FabrikamMcp.csproj"
    ) -PassThru
    
    Write-Action "MCP Server started in new window (PID: $($mcpProcess.Id))"
    Write-Action "Wait for server to start completely..."
    Start-Sleep -Seconds 10
    
    # Test if server is responding
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/status" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Action "✅ MCP Server is responding at http://localhost:5000"
            return $true
        }
    } catch {
        Write-Action "⚠️  MCP Server may still be starting up..."
    }
    
    Wait-ForUser "Verify MCP server is running"
    return $true
}

function Configure-ClaudeDesktop {
    Write-Step "Configure Claude Desktop with MCP Server" "Green"
    
    $claudeConfigPath = "$env:APPDATA\Claude"
    Write-Action "Claude Desktop config location: $claudeConfigPath"
    
    Write-Action "1. Open Claude Desktop application"
    Write-Action "2. Go to Settings/Configuration"
    Write-Action "3. Add MCP server configuration:"
    Write-Host ""
    Write-Host "    Server Name: fabrikam" -ForegroundColor White
    Write-Host "    Server URL:  http://localhost:5000/mcp" -ForegroundColor White
    Write-Host ""
    Write-Action "4. Save configuration"
    Write-Action "5. Restart Claude Desktop if prompted"
    Write-Action "6. Verify MCP server appears in Claude's available tools"
    
    Wait-ForUser "Complete Claude Desktop MCP configuration"
}

function Test-CopilotAfterClaude {
    Write-Step "Test GitHub Copilot After Claude MCP Configuration" "Red"
    
    Write-Action "1. Return to VS Code"
    Write-Action "2. Try the same Copilot tests as baseline:"
    Write-Action "   - Inline suggestions"
    Write-Action "   - Ctrl+I for Copilot chat"
    Write-Action "   - Code completions"
    Write-Action "3. Document any differences in behavior"
    Write-Action "4. Check VS Code Developer Tools (Help > Toggle Developer Tools)"
    Write-Action "5. Look for any errors in Console tab"
    
    Wait-ForUser "Complete Copilot testing"
}

function Collect-DiagnosticInfo {
    Write-Step "Collecting Diagnostic Information" "Blue"
    
    Write-Action "Gathering system information..."
    
    # VS Code version
    try {
        $vscodeVersion = & code --version 2>$null
        Write-Host "  VS Code Version: $($vscodeVersion[0])" -ForegroundColor Gray
    } catch {
        Write-Host "  VS Code Version: Not found in PATH" -ForegroundColor Gray
    }
    
    # Check for Claude Desktop process
    $claudeProcess = Get-Process -Name "*Claude*" -ErrorAction SilentlyContinue
    if ($claudeProcess) {
        Write-Host "  Claude Desktop: Running (PID: $($claudeProcess.Id))" -ForegroundColor Gray
    } else {
        Write-Host "  Claude Desktop: Not running" -ForegroundColor Gray
    }
    
    # Check MCP server
    try {
        $mcpResponse = Invoke-WebRequest -Uri "http://localhost:5000/status" -UseBasicParsing -TimeoutSec 2
        Write-Host "  MCP Server: Running (Status: $($mcpResponse.StatusCode))" -ForegroundColor Gray
    } catch {
        Write-Host "  MCP Server: Not responding" -ForegroundColor Gray
    }
    
    # Check Claude config directory
    $claudeConfigPath = "$env:APPDATA\Claude"
    if (Test-Path $claudeConfigPath) {
        Write-Host "  Claude Config Dir: Exists" -ForegroundColor Gray
        $configFiles = Get-ChildItem $claudeConfigPath -File -ErrorAction SilentlyContinue
        if ($configFiles) {
            Write-Host "    Config files: $($configFiles.Count) files found" -ForegroundColor Gray
        }
    } else {
        Write-Host "  Claude Config Dir: Not found" -ForegroundColor Gray
    }
    
    Write-Action "Manual checks to perform:"
    Write-Action "1. Check VS Code extensions for any errors"
    Write-Action "2. Check Windows Event Viewer for application errors"
    Write-Action "3. Check VS Code Developer Tools console"
    Write-Action "4. Note any error messages or behavior changes"
    
    Wait-ForUser
}

function Cleanup-TestEnvironment {
    Write-Step "Cleaning Up Test Environment" "Yellow"
    
    Write-Action "1. Stop MCP server processes"
    $mcpProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*FabrikamMcp*" }
    foreach ($process in $mcpProcesses) {
        Write-Action "  Stopping MCP process (PID: $($process.Id))"
        $process | Stop-Process -Force
    }
    
    Write-Action "2. Remove Claude Desktop MCP configuration (manual)"
    Write-Action "   - Open Claude Desktop settings"
    Write-Action "   - Remove or disable the Fabrikam MCP server"
    Write-Action "   - Restart Claude Desktop"
    
    Write-Action "3. Test Copilot functionality returns to normal"
    
    Wait-ForUser "Complete cleanup steps"
}

# Main execution
Write-Host "🔍 GitHub Copilot + Claude Desktop MCP Conflict Reproduction Tool" -ForegroundColor Magenta
Write-Host "=================================================================" -ForegroundColor Magenta

if ($Cleanup) {
    Cleanup-TestEnvironment
    exit
}

if ($Baseline) {
    Test-CopilotBaseline
    exit
}

if ($FullTest -or $true) {
    Write-Step "Starting Full Reproduction Test" "Magenta"
    
    # Step 1: Baseline test
    Test-CopilotBaseline
    
    # Step 2: Start MCP server
    if (-not (Start-FabrikamMcp)) {
        Write-Host "❌ Failed to start MCP server. Exiting." -ForegroundColor Red
        exit 1
    }
    
    # Step 3: Configure Claude Desktop
    Configure-ClaudeDesktop
    
    # Step 4: Test Copilot after Claude configuration
    Test-CopilotAfterClaude
    
    # Step 5: Collect diagnostic information
    Collect-DiagnosticInfo
    
    Write-Step "Reproduction Test Complete" "Green"
    Write-Action "Review your findings and update the bug report:"
    Write-Action "BUG-REPORT-COPILOT-CLAUDE-CONFLICT.md"
    Write-Host ""
    Write-Action "To cleanup: .\Reproduce-Copilot-Claude-Conflict.ps1 -Cleanup"
}
