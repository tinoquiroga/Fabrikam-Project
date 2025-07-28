# Fabrikam Development Testing Shortcut (Modular Architecture)
# 
# This is a convenient wrapper for the new modular testing script located in scripts/Test-Development-New.ps1
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
    [int]$TimeoutSeconds = 30,
    [string]$ApiBaseUrl = "https://localhost:7297",
    [string]$McpBaseUrl = "https://localhost:5001",
    [switch]$Help
)

# Show help if requested
if ($Help) {
    Write-Host ""
    Write-Host "🧪 Fabrikam Development Testing Shortcut (Modular)" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "This is a convenient wrapper for scripts\Test-Development-New.ps1" -ForegroundColor Cyan
    Write-Host "Using the NEW MODULAR ARCHITECTURE with focused test modules:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Common Usage:" -ForegroundColor Yellow
    Write-Host "  .\test.ps1                    # Full test suite"
    Write-Host "  .\test.ps1 -Quick             # Essential tests only (fast)"
    Write-Host "  .\test.ps1 -ApiOnly           # Test API endpoints only"
    Write-Host "  .\test.ps1 -McpOnly           # Test MCP server and tools only"
    Write-Host "  .\test.ps1 -AuthOnly          # Test authentication only"
    Write-Host "  .\test.ps1 -IntegrationOnly   # Test API-MCP integration only"
    Write-Host "  .\test.ps1 -Verbose           # Detailed output and comprehensive testing"
    Write-Host ""
    Write-Host "Modular Architecture Benefits:" -ForegroundColor Cyan
    Write-Host "  ✅ Focused, maintainable test modules"
    Write-Host "  ✅ GitHub Copilot-friendly code structure"
    Write-Host "  ✅ Comprehensive MCP tool validation"
    Write-Host "  ✅ Enhanced integration testing"
    Write-Host "  ✅ Better error handling and reporting"
    Write-Host ""
    Write-Host "For full documentation, see: scripts\Test-Development-New.ps1" -ForegroundColor Gray
    Write-Host ""
    return
}

# Check if the new modular test script exists
$NewTestScript = "$PSScriptRoot\scripts\Test-Development-New.ps1"
$OldTestScript = "$PSScriptRoot\scripts\Test-Development.ps1"

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
