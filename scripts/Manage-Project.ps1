# Fabrikam Project Manager Script
# Helps GitHub Copilot and developers manage the monorepo structure

param(
    [Parameter(Position=0)]
    [ValidateSet("start", "stop", "restart", "status", "test", "build", "help")]
    [string]$Action = "help",
    
    [ValidateSet("api", "mcp", "both", "all")]
    [string]$Project = "both",
    
    [switch]$Quick,
    [switch]$VerboseOutput
)

$ErrorActionPreference = "Stop"

# Project Configuration
$WorkspaceRoot = $PWD.Path
$ApiProject = "FabrikamApi\src\FabrikamApi.csproj"
$McpProject = "FabrikamMcp\src\FabrikamMcp.csproj"
$TestsProject = "FabrikamTests\FabrikamTests.csproj"
$SolutionFile = "Fabrikam.sln"

# Color functions
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Warning($message) { Write-Host "⚠️ $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️ $message" -ForegroundColor Cyan }
function Write-Header($message) { 
    Write-Host "`n🚀 $message" -ForegroundColor Yellow
    Write-Host "=" * ($message.Length + 4) -ForegroundColor Yellow
}

function Test-WorkspaceRoot {
    if (!(Test-Path $SolutionFile)) {
        Write-Error "Not in workspace root! Please run from: c:\Users\davidb\1Repositories\Fabrikam-Project"
        Write-Info "Current directory: $WorkspaceRoot"
        exit 1
    }
}

function Get-ProcessStatus {
    $apiProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
                 Where-Object { 
                     try {
                         $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
                         $cmdLine -like "*FabrikamApi*"
                     } catch { $false }
                 }
    
    $mcpProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
                 Where-Object { 
                     try {
                         $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
                         $cmdLine -like "*FabrikamMcp*"
                     } catch { $false }
                 }
    
    return @{
        ApiRunning = $apiProcess -ne $null
        McpRunning = $mcpProcess -ne $null
        ApiProcess = $apiProcess
        McpProcess = $mcpProcess
    }
}

function Show-Status {
    Write-Header "Fabrikam Project Status"
    
    $status = Get-ProcessStatus
    
    Write-Host "📁 Workspace: $WorkspaceRoot"
    Write-Host "📄 Solution: $SolutionFile"
    Write-Host ""
    
    if ($status.ApiRunning) {
        Write-Success "🌐 API Server: Running (PID: $($status.ApiProcess.Id)) → https://localhost:7297"
    } else {
        Write-Warning "🌐 API Server: Not running"
    }
    
    if ($status.McpRunning) {
        Write-Success "🤖 MCP Server: Running (PID: $($status.McpProcess.Id)) → https://localhost:5001"
    } else {
        Write-Warning "🤖 MCP Server: Not running"
    }
    
    Write-Host ""
    Write-Info "💡 Use 'Manage-Project.ps1 start' to start servers"
    Write-Info "💡 Use 'Manage-Project.ps1 test' to test everything"
}

function Start-Projects {
    Write-Header "Starting Fabrikam Projects"
    
    $status = Get-ProcessStatus
    
    if ($Project -eq "api" -or $Project -eq "both" -or $Project -eq "all") {
        if ($status.ApiRunning) {
            Write-Warning "🌐 API Server already running (PID: $($status.ApiProcess.Id))"
        } else {
            Write-Info "🌐 Starting API Server..."
            Write-Host "Command: dotnet run --project $ApiProject --launch-profile https" -ForegroundColor Gray
            Write-Info "🚀 API will be available at: https://localhost:7297"
            Write-Warning "⚠️ This will start in the current terminal. Use Ctrl+C to stop."
            Write-Host ""
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project $ApiProject --launch-profile https"
        }
    }
    
    if ($Project -eq "mcp" -or $Project -eq "both" -or $Project -eq "all") {
        if ($status.McpRunning) {
            Write-Warning "🤖 MCP Server already running (PID: $($status.McpProcess.Id))"
        } else {
            Write-Info "🤖 Starting MCP Server..."
            Write-Host "Command: dotnet run --project $McpProject" -ForegroundColor Gray
            Write-Info "🚀 MCP will be available at: https://localhost:5001"
            Write-Warning "⚠️ This will start in a new terminal. Use Ctrl+C to stop."
            Write-Host ""
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project $McpProject"
        }
    }
    
    Write-Success "✅ Project startup initiated!"
    Write-Info "💡 Use 'Manage-Project.ps1 status' to check running status"
}

function Stop-Projects {
    Write-Header "Stopping Fabrikam Projects"
    
    $status = Get-ProcessStatus
    
    if ($Project -eq "api" -or $Project -eq "both" -or $Project -eq "all") {
        if ($status.ApiRunning) {
            Write-Info "🌐 Stopping API Server (PID: $($status.ApiProcess.Id))"
            Stop-Process -Id $status.ApiProcess.Id -Force
            Write-Success "✅ API Server stopped"
        } else {
            Write-Warning "🌐 API Server not running"
        }
    }
    
    if ($Project -eq "mcp" -or $Project -eq "both" -or $Project -eq "all") {
        if ($status.McpRunning) {
            Write-Info "🤖 Stopping MCP Server (PID: $($status.McpProcess.Id))"
            Stop-Process -Id $status.McpProcess.Id -Force
            Write-Success "✅ MCP Server stopped"
        } else {
            Write-Warning "🤖 MCP Server not running"
        }
    }
}

function Build-Projects {
    Write-Header "Building Fabrikam Projects"
    
    Write-Info "🏗️ Building solution: $SolutionFile"
    dotnet build $SolutionFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✅ Build completed successfully"
    } else {
        Write-Error "❌ Build failed"
        exit $LASTEXITCODE
    }
}

function Test-Projects {
    Write-Header "Testing Fabrikam Projects"
    
    if ($Quick) {
        Write-Info "🧪 Running quick tests..."
        & ".\Test-Development.ps1" -Quick
    } elseif ($VerboseOutput) {
        Write-Info "🧪 Running verbose tests..."
        & ".\Test-Development.ps1" -Verbose
    } else {
        Write-Info "🧪 Running standard tests..."
        & ".\Test-Development.ps1"
    }
}

function Show-Help {
    Write-Header "Fabrikam Project Manager Help"
    
    Write-Host @"
🎯 USAGE:
    .\Manage-Project.ps1 <action> [options]

📋 ACTIONS:
    start     Start project servers
    stop      Stop project servers  
    restart   Restart project servers
    status    Show current status
    test      Run project tests
    build     Build all projects
    help      Show this help

🎛️ OPTIONS:
    -Project  Which project(s) to target:
              • api   - API server only
              • mcp   - MCP server only  
              • both  - Both servers (default)
              • all   - All projects
              
    -Quick    Quick test mode
    -VerboseOutput  Verbose test mode

💡 EXAMPLES:
    .\Manage-Project.ps1 start              # Start both servers
    .\Manage-Project.ps1 start -Project api # Start API only
    .\Manage-Project.ps1 stop -Project mcp  # Stop MCP only
    .\Manage-Project.ps1 test -VerboseOutput # Verbose tests
    .\Manage-Project.ps1 build               # Build solution
    .\Manage-Project.ps1 status              # Check status

🌐 ENDPOINTS:
    API:  https://localhost:7297 (HTTPS)
    MCP:  https://localhost:5001 (HTTPS)

📁 WORKSPACE ROOT:
    Always run from: c:\Users\davidb\1Repositories\Fabrikam-Project

🚨 MONOREPO REMINDER:
    This script helps manage the monorepo structure.
    See .github\MONOREPO-GUIDE.md for complete details.
"@
}

# Main execution
try {
    Test-WorkspaceRoot
    
    switch ($Action) {
        "start" { Start-Projects }
        "stop" { Stop-Projects }
        "restart" { 
            Stop-Projects
            Start-Sleep -Seconds 2
            Start-Projects
        }
        "status" { Show-Status }
        "test" { Test-Projects }
        "build" { Build-Projects }
        "help" { Show-Help }
        default { Show-Help }
    }
} catch {
    Write-Error "Script failed: $($_.Exception.Message)"
    exit 1
}
