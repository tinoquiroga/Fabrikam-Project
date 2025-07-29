# Reproduce-CIPP-MCP-Copilot-Conflict.ps1
# Script to reproduce the specific GitHub Copilot + Claude Desktop CIPP MCP conflict

param(
    [switch]$Test,         # Run the reproduction test
    [switch]$Baseline,     # Test Copilot baseline only  
    [switch]$Configure,    # Just configure Claude Desktop
    [switch]$Cleanup       # Remove the problematic configuration
)

$claudeConfigPath = "$env:APPDATA\Claude\claude_desktop_config.json"
$cippMcpPath = "C:\Users\DavidBjurman-Birr\1Repositories\CIPP-Project\CIPP-MCP\Proxy\cipp.local_mcp.py"

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
    Write-Step "Testing GitHub Copilot Baseline (Before Claude MCP)" "Green"
    Write-Action "Current Claude config status:"
    
    if (Test-Path $claudeConfigPath) {
        $config = Get-Content $claudeConfigPath -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($config) -or $config -eq "{}") {
            Write-Host "    ✅ Claude config is empty/default" -ForegroundColor Green
        } else {
            Write-Host "    ⚠️  Claude config has content:" -ForegroundColor Yellow
            Write-Host "    $($config.Length) characters" -ForegroundColor Gray
        }
    } else {
        Write-Host "    ✅ No Claude config file exists" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Action "Now test GitHub Copilot in VS Code:"
    Write-Action "1. Open VS Code"
    Write-Action "2. Create or open a code file (e.g., test.js, test.cs)"
    Write-Action "3. Press Ctrl+I to open Copilot Chat"
    Write-Action "4. Ask a simple question like 'how to create a function'"
    Write-Action "5. Verify Copilot Chat responds normally"
    Write-Action "6. Also test inline suggestions by typing some code"
    
    Wait-ForUser "Document baseline Copilot behavior"
}

function Add-CippMcpConfiguration {
    Write-Step "Adding CIPP MCP Configuration to Claude Desktop" "Red"
    
    # Check if CIPP MCP file exists
    if (-not (Test-Path $cippMcpPath)) {
        Write-Host "❌ CIPP MCP file not found at: $cippMcpPath" -ForegroundColor Red
        Write-Host "   This file is required to reproduce the issue." -ForegroundColor Red
        return $false
    }
    
    Write-Action "CIPP MCP file found: ✅"
    Write-Action "File: $cippMcpPath"
    
    # Create the problematic configuration
    $problematicConfig = @{
        mcpServers = @{
            "cipp-mcp" = @{
                command = "python"
                args = @($cippMcpPath)
                env = @{
                    PYTHONPATH = Split-Path $cippMcpPath -Parent
                }
            }
        }
    }
    
    # Ensure Claude directory exists
    $claudeDir = Split-Path $claudeConfigPath -Parent
    if (-not (Test-Path $claudeDir)) {
        New-Item -Path $claudeDir -ItemType Directory -Force | Out-Null
        Write-Action "Created Claude directory: $claudeDir"
    }
    
    # Backup existing config if it exists
    if (Test-Path $claudeConfigPath) {
        $backupPath = "$claudeConfigPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Copy-Item $claudeConfigPath $backupPath
        Write-Action "Backed up existing config to: $backupPath"
    }
    
    # Write the problematic configuration
    $configJson = $problematicConfig | ConvertTo-Json -Depth 10
    $configJson | Set-Content $claudeConfigPath -Encoding UTF8
    
    Write-Action "✅ Added CIPP MCP configuration to Claude Desktop"
    Write-Action "Config location: $claudeConfigPath"
    Write-Host ""
    Write-Host "Configuration added:" -ForegroundColor White
    Write-Host $configJson -ForegroundColor Gray
    Write-Host ""
    
    Write-Action "1. Restart Claude Desktop application now"
    Write-Action "2. Verify the CIPP MCP server appears in Claude's tools"
    
    Wait-ForUser "Complete Claude Desktop restart and MCP verification"
    return $true
}

function Test-CopilotAfterClaude {
    Write-Step "Testing GitHub Copilot After Claude CIPP MCP Configuration" "Red"
    
    Write-Action "Key observation points:"
    Write-Action "1. VS Code may automatically discover the MCP server from Claude's config"
    Write-Action "2. Check if VS Code shows MCP server as 'running' even without backend"
    Write-Action "3. Note the timing of when Copilot stops working"
    Write-Host ""
    
    Write-Action "Now test GitHub Copilot again:"
    Write-Action "1. Open VS Code (or return to existing window)"
    Write-Action "2. Check VS Code status bar or MCP indicators"
    Write-Action "3. Press Ctrl+I to open Copilot Chat"
    Write-Action "4. Try the same question as baseline test"
    Write-Action "5. Observe behavior differences:"
    Write-Action "   - Does Copilot Chat load at all?"
    Write-Action "   - Does it respond to prompts?"
    Write-Action "   - Are there timeout errors?"
    Write-Action "   - Does VS Code show MCP server as running?"
    Write-Host ""
    Write-Action "6. If Copilot isn't working, try stopping MCP server in VS Code"
    Write-Action "7. Check if stopping MCP immediately restores Copilot"
    Write-Action "8. Check VS Code Developer Tools (Help > Toggle Developer Tools)"
    Write-Action "9. Look in Console tab for MCP or Copilot related errors"
    
    Wait-ForUser "Document detailed behavior and MCP server status"
}

function Remove-CippMcpConfiguration {
    Write-Step "Cleaning Up - Removing CIPP MCP Configuration" "Green"
    
    if (Test-Path $claudeConfigPath) {
        # Check if there are backups to restore from
        $backupFiles = Get-ChildItem -Path (Split-Path $claudeConfigPath -Parent) -Filter "claude_desktop_config.json.backup.*" | Sort-Object LastWriteTime -Descending
        
        if ($backupFiles.Count -gt 0) {
            $latestBackup = $backupFiles[0]
            Write-Action "Restoring from backup: $($latestBackup.Name)"
            Copy-Item $latestBackup.FullName $claudeConfigPath -Force
        } else {
            Write-Action "No backup found, clearing configuration"
            "{}" | Set-Content $claudeConfigPath -Encoding UTF8
        }
        
        Write-Action "✅ Removed CIPP MCP configuration"
    } else {
        Write-Action "✅ No Claude config file found (already clean)"
    }
    
    Write-Action "1. Restart Claude Desktop application"
    Write-Action "2. Verify CIPP MCP server no longer appears in Claude"
    Write-Action "3. Test GitHub Copilot functionality returns to normal"
    
    Wait-ForUser "Complete cleanup verification"
}

function Show-DiagnosticInfo {
    Write-Step "Diagnostic Information" "Blue"
    
    Write-Action "System Information:"
    Write-Host "  VS Code Version: " -NoNewline -ForegroundColor Gray
    try {
        $vscodeVersion = & code --version 2>$null
        Write-Host $vscodeVersion[0] -ForegroundColor White
    } catch {
        Write-Host "Not found" -ForegroundColor Red
    }
    
    Write-Host "  Python Version: " -NoNewline -ForegroundColor Gray
    try {
        $pythonVersion = & python --version 2>$null
        Write-Host $pythonVersion -ForegroundColor White
    } catch {
        Write-Host "Not found" -ForegroundColor Red
    }
    
    Write-Host "  Claude Desktop: " -NoNewline -ForegroundColor Gray
    $claudeProcess = Get-Process -Name "*Claude*" -ErrorAction SilentlyContinue
    if ($claudeProcess) {
        Write-Host "Running" -ForegroundColor Green
    } else {
        Write-Host "Not running" -ForegroundColor Yellow
    }
    
    Write-Host "  CIPP MCP File: " -NoNewline -ForegroundColor Gray
    if (Test-Path $cippMcpPath) {
        Write-Host "Exists" -ForegroundColor Green
    } else {
        Write-Host "Missing" -ForegroundColor Red
    }
    
    Write-Host "  Claude Config: " -NoNewline -ForegroundColor Gray
    if (Test-Path $claudeConfigPath) {
        $configSize = (Get-Item $claudeConfigPath).Length
        Write-Host "$configSize bytes" -ForegroundColor White
    } else {
        Write-Host "Not found" -ForegroundColor Yellow
    }
}

# Main execution
Write-Host "🔍 GitHub Copilot + Claude Desktop CIPP MCP Conflict Reproduction" -ForegroundColor Magenta
Write-Host "=================================================================" -ForegroundColor Magenta

Show-DiagnosticInfo

if ($Cleanup) {
    Remove-CippMcpConfiguration
    exit
}

if ($Baseline) {
    Test-CopilotBaseline
    exit
}

if ($Configure) {
    Add-CippMcpConfiguration
    exit
}

if ($Test -or $true) {
    Write-Step "Starting CIPP MCP Conflict Reproduction Test" "Magenta"
    
    # Step 1: Baseline test
    Test-CopilotBaseline
    
    # Step 2: Add problematic configuration
    if (-not (Add-CippMcpConfiguration)) {
        Write-Host "❌ Failed to configure CIPP MCP. Exiting." -ForegroundColor Red
        exit 1
    }
    
    # Step 3: Test Copilot after configuration
    Test-CopilotAfterClaude
    
    Write-Step "Reproduction Test Complete" "Green"
    Write-Action "Document your findings in the bug report:"
    Write-Action "BUG-REPORT-COPILOT-CLAUDE-CONFLICT.md"
    Write-Host ""
    Write-Action "To remove the problematic config: .\Reproduce-CIPP-MCP-Copilot-Conflict.ps1 -Cleanup"
}

Write-Host ""
Write-Host "💡 Quick commands:" -ForegroundColor Yellow
Write-Host "  Baseline test only:  .\Reproduce-CIPP-MCP-Copilot-Conflict.ps1 -Baseline" -ForegroundColor Gray
Write-Host "  Configure only:      .\Reproduce-CIPP-MCP-Copilot-Conflict.ps1 -Configure" -ForegroundColor Gray  
Write-Host "  Cleanup:             .\Reproduce-CIPP-MCP-Copilot-Conflict.ps1 -Cleanup" -ForegroundColor Gray
