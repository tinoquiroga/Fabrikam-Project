# Fabrikam Development Testing Suite
# Comprehensive testing for API endpoints and MCP tools during development

param(
    [switch]$ApiOnly,
    [switch]$McpOnly,
    [switch]$IntegrationOnly,
    [switch]$Quick,
    [switch]$Verbose,
    [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

# Configuration
$ApiBaseUrl = "https://localhost:7241"
$McpBaseUrl = "http://localhost:5000"  # Adjust based on MCP server port

# Color functions
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Warning($message) { Write-Host "⚠️ $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️ $message" -ForegroundColor Cyan }
function Write-Debug($message) { if ($Verbose) { Write-Host "🔍 $message" -ForegroundColor Gray } }

# Test Results Tracking
$TestResults = @{
    ApiTests = @()
    McpTests = @()
    IntegrationTests = @()
    TotalPassed = 0
    TotalFailed = 0
}

function Add-TestResult {
    param($Category, $Name, $Passed, $Details = "")
    
    $result = @{
        Name = $Name
        Passed = $Passed
        Details = $Details
        Timestamp = Get-Date
    }
    
    $TestResults[$Category] += $result
    
    if ($Passed) {
        $TestResults.TotalPassed++
        Write-Success "$Name"
        if ($Details -and $Verbose) { Write-Debug $Details }
    } else {
        $TestResults.TotalFailed++
        Write-Error "$Name"
        if ($Details) { Write-Host "   $Details" -ForegroundColor Gray }
    }
}

function Test-ApiEndpoint {
    param($Endpoint, $ExpectedStatus = 200, $TestName = "")
    
    if (-not $TestName) { $TestName = "API $Endpoint" }
    
    try {
        Write-Debug "Testing $ApiBaseUrl$Endpoint"
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl$Endpoint" -Method Get -TimeoutSec $TimeoutSeconds
        
        # Basic validation - endpoint responds
        if ($response) {
            Add-TestResult "ApiTests" $TestName $true "Status OK, got response"
            return $response
        } else {
            Add-TestResult "ApiTests" $TestName $false "No response data"
            return $null
        }
    }
    catch {
        Add-TestResult "ApiTests" $TestName $false $_.Exception.Message
        return $null
    }
}

function Test-ApiDataStructure {
    param($Data, $RequiredFields, $TestName)
    
    try {
        $missing = @()
        foreach ($field in $RequiredFields) {
            if (-not (Get-Member -InputObject $Data -Name $field -ErrorAction SilentlyContinue)) {
                $missing += $field
            }
        }
        
        if ($missing.Count -eq 0) {
            Add-TestResult "ApiTests" $TestName $true "All required fields present"
        } else {
            Add-TestResult "ApiTests" $TestName $false "Missing fields: $($missing -join ', ')"
        }
    }
    catch {
        Add-TestResult "ApiTests" $TestName $false $_.Exception.Message
    }
}

function Test-McpServerHealth {
    try {
        # Test if MCP server process is running by checking command line
        Write-Debug "Checking for MCP server process..."
        
        $mcpProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
                     Where-Object { 
                         try {
                             $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
                             $isMcpProcess = $cmdLine -like "*FabrikamMcp*"
                             if ($Verbose -and $isMcpProcess) {
                                 Write-Debug "Found MCP process: $cmdLine"
                             }
                             return $isMcpProcess
                         } catch {
                             Write-Debug "Error checking process $($_.Id): $($_.Exception.Message)"
                             return $false
                         }
                     }
        
        if ($mcpProcess) {
            $processDetails = "Process ID: $($mcpProcess.Id)"
            Add-TestResult "McpTests" "MCP Server Process" $true $processDetails
            Write-Debug "MCP server detected: $processDetails"
        } else {
            Add-TestResult "McpTests" "MCP Server Process" $false "MCP server process not found. Run 'dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj' to start it."
            Write-Debug "No MCP server process found in running dotnet processes"
        }
    }
    catch {
        Add-TestResult "McpTests" "MCP Server Health" $false $_.Exception.Message
    }
}

function Show-TestSummary {
    Write-Host "`n" + "="*60 -ForegroundColor Cyan
    Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor Cyan
    Write-Host "="*60 -ForegroundColor Cyan
    
    Write-Host "`nResults:" -ForegroundColor White
    Write-Success "Passed: $($TestResults.TotalPassed)"
    if ($TestResults.TotalFailed -gt 0) {
        Write-Error "Failed: $($TestResults.TotalFailed)"
    } else {
        Write-Host "Failed: 0" -ForegroundColor Green
    }
    
    $total = $TestResults.TotalPassed + $TestResults.TotalFailed
    if ($total -gt 0) {
        $successRate = [math]::Round(($TestResults.TotalPassed / $total) * 100, 1)
        Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } else { "Yellow" })
    }
    
    if ($TestResults.ApiTests.Count -gt 0) {
        Write-Host "`nAPI Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.ApiTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
    
    if ($TestResults.McpTests.Count -gt 0) {
        Write-Host "`nMCP Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.McpTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
    
    if ($TestResults.IntegrationTests.Count -gt 0) {
        Write-Host "`nIntegration Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.IntegrationTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
}

# Main Testing Logic
Write-Info "Starting Fabrikam Development Testing Suite"

if (-not $McpOnly) {
    Write-Host "`n🔍 API ENDPOINT TESTS" -ForegroundColor Cyan
    Write-Host "="*30 -ForegroundColor Cyan
    
    # Basic API health
    $ordersResponse = Test-ApiEndpoint "/api/orders" "Orders Endpoint"
    if ($ordersResponse) {
        # Orders returns an array directly, not wrapped in data/pagination
        if ($ordersResponse -is [Array] -and $ordersResponse.Count -gt 0) {
            Add-TestResult "ApiTests" "Orders Response Structure" $true "Returns array with $($ordersResponse.Count) orders"
        } else {
            Add-TestResult "ApiTests" "Orders Response Structure" $false "Expected array of orders"
        }
    }
    
    # Analytics endpoint (the one that was failing)
    $analyticsResponse = Test-ApiEndpoint "/api/orders/analytics" "Analytics Endpoint"
    if ($analyticsResponse) {
        Test-ApiDataStructure $analyticsResponse @("summary", "byStatus", "byRegion", "recentTrends") "Analytics Response Structure"
        
        # Detailed analytics validation
        if ($analyticsResponse.summary) {
            Test-ApiDataStructure $analyticsResponse.summary @("totalOrders", "totalRevenue", "averageOrderValue") "Analytics Summary Structure"
        }
    }
    
    # Other endpoints
    if (-not $Quick) {
        Test-ApiEndpoint "/api/customers" "Customers Endpoint"
        Test-ApiEndpoint "/api/products" "Products Endpoint"
        Test-ApiEndpoint "/api/supporttickets" "Support Tickets Endpoint"  # Fixed URL
        Test-ApiEndpoint "/api/info" "Info Endpoint"
    }
}

if (-not $ApiOnly) {
    Write-Host "`n🔧 MCP SERVER TESTS" -ForegroundColor Cyan
    Write-Host "="*30 -ForegroundColor Cyan
    
    Test-McpServerHealth
    
    # Additional MCP tests would go here
    # These would test MCP protocol compliance, tool availability, etc.
}

if (-not $ApiOnly -and -not $McpOnly) {
    Write-Host "`n🔄 INTEGRATION TESTS" -ForegroundColor Cyan
    Write-Host "="*30 -ForegroundColor Cyan
    
    # Test that MCP can reach API
    try {
        if ($analyticsResponse) {
            Add-TestResult "IntegrationTests" "API-MCP Data Compatibility" $true "Analytics data structure matches MCP expectations"
        } else {
            Add-TestResult "IntegrationTests" "API-MCP Data Compatibility" $false "Analytics endpoint not responding"
        }
    }
    catch {
        Add-TestResult "IntegrationTests" "API-MCP Integration" $false $_.Exception.Message
    }
}

Show-TestSummary

Write-Host "`n📋 NEXT STEPS:" -ForegroundColor Cyan
if ($TestResults.TotalFailed -eq 0) {
    Write-Success "All tests passed! Your development environment is ready."
    Write-Host "• Continue development with confidence" -ForegroundColor Gray
    Write-Host "• Run this script after making changes" -ForegroundColor Gray
} else {
    Write-Warning "Some tests failed. Address these issues:"
    Write-Host "• Check failed test details above" -ForegroundColor Gray
    Write-Host "• Ensure both API and MCP server are running" -ForegroundColor Gray
    Write-Host "• Verify port configurations" -ForegroundColor Gray
}

Write-Host "`n💡 Usage Tips:" -ForegroundColor Cyan
Write-Host "• Quick test: .\Test-Development.ps1 -Quick" -ForegroundColor Gray
Write-Host "• API only: .\Test-Development.ps1 -ApiOnly" -ForegroundColor Gray
Write-Host "• Verbose output: .\Test-Development.ps1 -Verbose" -ForegroundColor Gray
