# Fabrikam Testing Workflow Guide
# Demonstrates consolidated approach for Issue #11

param(
    [Parameter(Position=0)]
    [ValidateSet("unified", "modular-standalone", "modular-managed", "help")]
    [string]$Mode = "help"
)

function Show-Usage {
    Write-Host @"
🧪 Fabrikam Testing - Consolidated Workflow Guide

THREE TESTING APPROACHES:

1️⃣ UNIFIED TESTING (Original - Proven Reliable)
   Command: .\Test-Development.ps1
   • Complete testing suite (1740 lines, 20+ functions)
   • Manages entire server lifecycle automatically
   • Comprehensive authentication, API, MCP, and integration tests
   • Stops servers after testing (clean environment)
   • Best for: Complete validation, CI/CD, comprehensive testing

2️⃣ MODULAR STANDALONE (Issue #11 - Temporary Servers)
   Command: .\scripts\Test-Modular.ps1
   • Focused test modules (easier maintenance)
   • Starts servers temporarily, stops them after testing
   • Run specific test categories (API, Auth, MCP, Integration)
   • Benefits: Better maintainability, focused testing
   • Best for: Development, debugging specific components

3️⃣ MODULAR WITH MANAGED SERVERS (Issue #11 - Persistent Servers)
   Commands: 
      .\scripts\Manage-Project.ps1 start      # Start servers in visible terminals
      .\scripts\Test-Modular.ps1 -UseRunningServers  # Test against running servers
      .\scripts\Manage-Project.ps1 stop       # Stop servers when done
   • Servers run in visible terminals (easy monitoring)
   • Tests run against persistent servers (faster iterations)
   • Manual server lifecycle control
   • Best for: Interactive development, repeated testing sessions

EXAMPLES:

   # Complete testing (original approach)
   .\Test-Development.ps1 -Verbose

   # Quick modular testing with temporary servers
   .\scripts\Test-Modular.ps1 -Quick

   # Development workflow with persistent servers
   .\scripts\Manage-Project.ps1 start
   .\scripts\Test-Modular.ps1 -ApiOnly -UseRunningServers
   .\scripts\Test-Modular.ps1 -AuthOnly -UseRunningServers  
   .\scripts\Manage-Project.ps1 stop

SOLUTION TO MAINTAINABILITY:
   ✅ Keep Test-Development.ps1 for comprehensive testing
   ✅ Use Test-Modular.ps1 for focused development testing
   ✅ Server management is now clear and flexible
"@ -ForegroundColor Cyan
}

switch ($Mode) {
    "unified" {
        Write-Host "🔄 Running Unified Testing (Original Test-Development.ps1)" -ForegroundColor Green
        Write-Host "This will manage servers automatically and run comprehensive tests." -ForegroundColor Gray
        Write-Host ""
        & "$PSScriptRoot\Test-Development.ps1" -Verbose
    }
    
    "modular-standalone" {
        Write-Host "🧩 Running Modular Testing (Standalone)" -ForegroundColor Yellow
        Write-Host "This will start servers temporarily and run focused tests." -ForegroundColor Gray
        Write-Host ""
        & "$PSScriptRoot\scripts\Test-Modular.ps1" -Verbose
    }
    
    "modular-managed" {
        Write-Host "🏗️ Running Modular Testing with Managed Servers" -ForegroundColor Cyan
        Write-Host "This will start persistent servers, then run tests against them." -ForegroundColor Gray
        Write-Host ""
        
        # Start servers
        Write-Host "1️⃣ Starting servers..." -ForegroundColor Magenta
        & "$PSScriptRoot\scripts\Manage-Project.ps1" start
        
        Start-Sleep -Seconds 3
        
        # Run tests
        Write-Host "2️⃣ Running tests against running servers..." -ForegroundColor Magenta
        & "$PSScriptRoot\scripts\Test-Modular.ps1" -UseRunningServers -Verbose
        
        Write-Host ""
        Write-Host "💡 Servers are still running. Use 'Manage-Project.ps1 stop' to stop them." -ForegroundColor Green
    }
    
    default {
        Show-Usage
    }
}
