# Fabrikam Development Testing Shortcut (Modular Architecture)
# 
# This is a convenient wrapper for the modular testing script located in scripts/Test-Development-Modular.ps1
# All parameters are passed through to the main script
#
# Usage Examples:
# .\test.ps1                    # Quick test with existing servers
# .\test.ps1 -Quick             # Quick essential tests only
# .\test.ps1 -ApiOnly           # Test API endpoints only
# .\test.ps1 -McpOnly           # Test MCP server only
# .\test.ps1 -AuthOnly          # Test authentication only
# .\test.ps1 -IntegrationOnly   # Test API-MCP integration only
# .\test.ps1 -Verbose           # Detailed output and comprehensive testing

param(
    [switch]$ApiOnly,
    [switch]$McpOnly,
    [switch]$AuthOnly,
    [switch]$IntegrationOnly,
    [switch]$Quick,
    [switch]$Verbose,
    [switch]$CleanBuild,
    [switch]$SkipBuild,
    [switch]$CleanArtifacts,
    [switch]$Visible,
    [switch]$Production,
    [switch]$Status,
    [switch]$Stop,
    [string]$DeploymentName = "local",
    [int]$TimeoutSeconds = 30,
    [string]$ApiBaseUrl = "",
    [string]$McpBaseUrl = "",
    [switch]$Help
)

# Show help if requested
if ($Help) {
    Write-Host ""
    Write-Host "🧪 Fabrikam Development Testing Shortcut (Modular)" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "This is a convenient wrapper for scripts\Test-Development-Modular.ps1" -ForegroundColor Cyan
    Write-Host "Using the NEW MODULAR ARCHITECTURE with focused test modules:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Common Usage:" -ForegroundColor Yellow
    Write-Host "  .\test.ps1                    # Full test suite with server management"
    Write-Host "  .\test.ps1 -Quick             # Essential tests only (fast)"
    Write-Host "  .\test.ps1 -CleanBuild        # Stop servers, clean build, start fresh, test"
    Write-Host "  .\test.ps1 -SkipBuild         # Use existing servers without rebuilding"
    Write-Host "  .\test.ps1 -Visible           # Run servers in visible terminals"
    Write-Host "  .\test.ps1 -ApiOnly           # Test API endpoints only"
    Write-Host "  .\test.ps1 -McpOnly           # Test MCP server and tools only"
    Write-Host "  .\test.ps1 -AuthOnly          # Test authentication only"
    Write-Host "  .\test.ps1 -IntegrationOnly   # Test API-MCP integration only"
    Write-Host "  .\test.ps1 -Production        # Test against configured production deployment"
    Write-Host "  .\test.ps1 -CleanArtifacts    # Clean build artifacts after testing"
    Write-Host "  .\test.ps1 -Verbose           # Detailed output and comprehensive testing"
    Write-Host ""
    Write-Host "Deployment Testing:" -ForegroundColor Yellow
    Write-Host "  .\test.ps1 -Production                              # Test default production deployment"
    Write-Host "  .\test.ps1 -DeploymentName ""feature-auth-disabled""  # Test specific deployment"
    Write-Host "  .\test.ps1 -DeploymentName ""feature-auth-bearer""    # Test Bearer Token deployment"
    Write-Host "  .\test.ps1 -ApiBaseUrl ""https://...""               # Override with specific URLs"
    Write-Host ""
    Write-Host "Server Management:" -ForegroundColor Yellow
    Write-Host "  .\test.ps1 -Status            # Show current server status (replaces Manage-Project.ps1 status)"
    Write-Host "  .\test.ps1 -Stop              # Stop all running servers (replaces Manage-Project.ps1 stop)"
    Write-Host ""
    Write-Host "Modular Architecture Benefits:" -ForegroundColor Cyan
    Write-Host "  ✅ Focused, maintainable test modules with full server management"
    Write-Host "  ✅ Authentication-aware testing (Disabled, BearerToken, EntraExternalId)"
    Write-Host "  ✅ Automatic server lifecycle management (build, start, stop, cleanup)"
    Write-Host "  ✅ GitHub Copilot-friendly code structure"
    Write-Host "  ✅ Comprehensive MCP tool validation"
    Write-Host "  ✅ Enhanced integration testing"
    Write-Host "  ✅ Production testing capabilities"
    Write-Host "  ✅ Better error handling and reporting"
    Write-Host ""
    Write-Host "For full documentation, see: scripts\Test-Development-Modular.ps1" -ForegroundColor Gray
    Write-Host ""
    return
}

# Check if the modular test script exists
$NewTestScript = "$PSScriptRoot\scripts\Test-Development-Modular.ps1"
$ServerManagementScript = "$PSScriptRoot\scripts\testing\Test-ServerManagement.ps1"

# Handle status and stop commands using modular architecture
if ($Status -or $Stop) {
    if (Test-Path $ServerManagementScript) {
        # Import server management functions
        . $ServerManagementScript
        
        if ($Status) {
            Write-Host ""
            Write-Host "📊 Fabrikam Project Status (Modular Architecture)" -ForegroundColor Cyan
            Write-Host "=" * 50 -ForegroundColor Cyan
            
            $processStatus = Get-ProcessStatus
            
            if ($processStatus.ApiRunning) {
                Write-Host "✅ API Server: Running on port 7297 (PID: $($processStatus.ApiProcess.Id))" -ForegroundColor Green
            } else {
                Write-Host "❌ API Server: Not running on port 7297" -ForegroundColor Red
            }
            
            if ($processStatus.McpRunning) {
                Write-Host "✅ MCP Server: Running on ports 5000/5001 (PID: $($processStatus.McpProcess.Id))" -ForegroundColor Green
            } else {
                Write-Host "❌ MCP Server: Not running on ports 5000/5001" -ForegroundColor Red
            }
            
            Write-Host ""
            Write-Host "📋 Management Commands:" -ForegroundColor Cyan
            Write-Host "  .\test.ps1 -Stop              # Stop all servers"
            Write-Host "  .\test.ps1 -CleanBuild        # Stop, build, start fresh"
            Write-Host "  .\test.ps1 -Quick             # Quick test with existing servers"
            Write-Host ""
            return
        }
        
        if ($Stop) {
            Stop-AllServers
            Write-Host ""
            Write-Host "💡 To restart servers, use: .\test.ps1 -CleanBuild" -ForegroundColor Green
            Write-Host ""
            return
        }
    } else {
        Write-Host "❌ Server management functions not available" -ForegroundColor Red
        Write-Host "   Missing: $ServerManagementScript" -ForegroundColor Gray
        return
    }
}

if (Test-Path $NewTestScript) {
    # Use the new modular architecture
    & $NewTestScript @PSBoundParameters
}
elseif (Test-Path $OldTestScript) {
    # Fallback to old script with warning
    Write-Host "⚠️  WARNING: Using legacy test script. Modular architecture not found." -ForegroundColor Yellow
    Write-Host "   Expected: $NewTestScript" -ForegroundColor Gray
    Write-Host "   Using: $OldTestScript" -ForegroundColor Gray
    Write-Host ""
    
    # Filter parameters that the old script supports
    $OldParams = @{}
    if ($ApiOnly) { $OldParams.ApiOnly = $true }
    if ($McpOnly) { $OldParams.McpOnly = $true }
    if ($Quick) { $OldParams.Quick = $true }
    if ($Verbose) { $OldParams.Verbose = $true }
    
    & $OldTestScript @OldParams
}
else {
    Write-Host "❌ No test script found!" -ForegroundColor Red
    Write-Host "   Looking for: $NewTestScript" -ForegroundColor Gray
    Write-Host "   Or fallback: $OldTestScript" -ForegroundColor Gray
    exit 1
}
