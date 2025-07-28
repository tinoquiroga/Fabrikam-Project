# Fabrikam Project - Unified Development Testing
# Simplified orchestrator for modular testing architecture
# Replaces the previous monolithic Test-Development.ps1 script

param(
    [switch]$Quick,
    [switch]$ApiOnly,
    [switch]$McpOnly,
    [switch]$AuthOnly,
    [switch]$IntegrationOnly,
    [switch]$Verbose,
    [switch]$Help,
    [int]$TimeoutSeconds = 30,
    [string]$ApiBaseUrl = "https://localhost:7297",
    [string]$McpBaseUrl = "https://localhost:5001"
)

# Show help if requested
if ($Help) {
    Write-Host @"
🧪 Fabrikam Development Testing Suite

USAGE:
    .\Test-Development.ps1 [OPTIONS]

OPTIONS:
    -Quick              Fast health check only
    -ApiOnly            Test API endpoints only
    -McpOnly            Test MCP server and tools only
    -AuthOnly           Test authentication only
    -IntegrationOnly    Test API-MCP integration only
    -Verbose            Detailed output and comprehensive testing
    -TimeoutSeconds     Request timeout (default: 30)
    -ApiBaseUrl         API base URL (default: https://localhost:7297)
    -McpBaseUrl         MCP base URL (default: https://localhost:5001)
    -Help               Show this help message

EXAMPLES:
    .\Test-Development.ps1 -Quick           # Fast health check
    .\Test-Development.ps1 -ApiOnly         # Test API only
    .\Test-Development.ps1 -McpOnly         # Test MCP only
    .\Test-Development.ps1 -Verbose         # Full testing suite

MODULAR ARCHITECTURE:
    This script orchestrates focused testing modules:
    - Test-Shared.ps1: Common utilities and helpers
    - Test-Api.ps1: API endpoint testing
    - Test-Authentication.ps1: JWT authentication testing
    - Test-Mcp.ps1: MCP server and tools testing
    - Test-Integration.ps1: End-to-end integration testing

    Each module is GitHub Copilot-friendly and maintainable.

"@
    exit 0
}

# Determine script directory and import shared utilities
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TestingDir = Join-Path $ScriptDir "testing"

# Check if modular testing files exist
$SharedPath = Join-Path $TestingDir "Test-Shared.ps1"
$ApiPath = Join-Path $TestingDir "Test-Api.ps1"
$AuthPath = Join-Path $TestingDir "Test-Authentication.ps1"
$McpPath = Join-Path $TestingDir "Test-Mcp.ps1"
$IntegrationPath = Join-Path $TestingDir "Test-Integration.ps1"

$missingFiles = @()
if (-not (Test-Path $SharedPath)) { $missingFiles += "Test-Shared.ps1" }
if (-not (Test-Path $ApiPath)) { $missingFiles += "Test-Api.ps1" }
if (-not (Test-Path $AuthPath)) { $missingFiles += "Test-Authentication.ps1" }
if (-not (Test-Path $McpPath)) { $missingFiles += "Test-Mcp.ps1" }
if (-not (Test-Path $IntegrationPath)) { $missingFiles += "Test-Integration.ps1" }

if ($missingFiles.Count -gt 0) {
    Write-Host "❌ Missing testing modules: $($missingFiles -join ', ')" -ForegroundColor Red
    Write-Host "   Expected location: $TestingDir" -ForegroundColor Yellow
    Write-Host "   Please ensure all modular testing files are present." -ForegroundColor Yellow
    exit 1
}

# Import testing modules
try {
    Write-Host "Importing: $SharedPath" -ForegroundColor Gray
    . $SharedPath
    Write-Host "Importing: $ApiPath" -ForegroundColor Gray
    . $ApiPath
    Write-Host "Importing: $AuthPath" -ForegroundColor Gray
    . $AuthPath
    Write-Host "Importing: $McpPath" -ForegroundColor Gray
    . $McpPath
    Write-Host "Importing: $IntegrationPath" -ForegroundColor Gray
    . $IntegrationPath
    Write-Host "All modules imported successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to import testing modules: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   At: $($_.ScriptStackTrace)" -ForegroundColor Gray
    exit 1
}

# Initialize test tracking
Initialize-TestResults

Write-Host @"
🧪 FABRIKAM DEVELOPMENT TESTING SUITE
========================================
🌐 API URL: $ApiBaseUrl
🤖 MCP URL: $McpBaseUrl
⏱️  Timeout: $TimeoutSeconds seconds
📁 Testing Directory: $TestingDir

"@ -ForegroundColor Cyan

# Determine what to test based on parameters
$runApi = (-not $McpOnly -and -not $AuthOnly -and -not $IntegrationOnly) -or $ApiOnly
$runAuth = (-not $ApiOnly -and -not $McpOnly -and -not $IntegrationOnly) -or $AuthOnly
$runMcp = (-not $ApiOnly -and -not $AuthOnly -and -not $IntegrationOnly) -or $McpOnly
$runIntegration = (-not $ApiOnly -and -not $McpOnly -and -not $AuthOnly) -or $IntegrationOnly

# Show test plan
Write-Host "📋 TEST PLAN:" -ForegroundColor Yellow
if ($runApi) { Write-Host "   ✅ API Endpoints" -ForegroundColor Green }
if ($runAuth) { Write-Host "   ✅ Authentication" -ForegroundColor Green }
if ($runMcp) { Write-Host "   ✅ MCP Server & Tools" -ForegroundColor Green }
if ($runIntegration) { Write-Host "   ✅ Integration Testing" -ForegroundColor Green }
Write-Host ""

# Run tests based on parameters
try {
    if ($runApi) {
        Test-Api -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds -Quick:$Quick -Verbose:$Verbose
    }
    
    if ($runAuth) {
        Test-Authentication -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds -Quick:$Quick -Verbose:$Verbose
    }
    
    if ($runMcp) {
        Test-McpServer -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds -Quick:$Quick -Verbose:$Verbose
    }
    
    if ($runIntegration) {
        Test-Integration -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds -Quick:$Quick -Verbose:$Verbose
    }
    
    # Show final results
    Show-TestSummary -Verbose:$Verbose
    
    # Set exit code based on test results
    $totalResults = Get-TestResults
    $failedTests = $totalResults.Values | Where-Object { -not $_.Success }
    
    if ($failedTests.Count -eq 0) {
        Write-Host "🎉 ALL TESTS PASSED" -ForegroundColor Green
        exit 0
    }
    else {
        Write-Host "❌ $($failedTests.Count) TESTS FAILED" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "💥 TESTING SUITE ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Stack trace:" -ForegroundColor Yellow
    Write-Host $_.ScriptStackTrace -ForegroundColor Gray
    exit 2
}
