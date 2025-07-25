# Test FabrikamMcp and FabrikamApi Integration
# This script validates that the MCP server can communicate with the API

param(
    [Parameter(Mandatory=$false)]
    [string]$McpUrl = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = ""
)

function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Name
    )
    
    Write-Host "Testing $Name endpoint: $Url" -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "$Url/status" -Method Get -TimeoutSec 30
        Write-Host "✅ $Name is responding" -ForegroundColor Green
        
        if ($response.Status -eq "Ready") {
            Write-Host "✅ $Name status: $($response.Status)" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $Name status: $($response.Status)" -ForegroundColor Yellow
        }
        
        return $true
    }
    catch {
        Write-Host "❌ $Name is not responding: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Get-AzdOutputs {
    param([string]$ServicePath)
    
    Push-Location $ServicePath
    try {
        $output = azd show --output json 2>$null | ConvertFrom-Json
        return $output.outputs
    }
    catch {
        return $null
    }
    finally {
        Pop-Location
    }
}

Write-Host "🧪 Testing FabrikamMcp and FabrikamApi Integration" -ForegroundColor Green

# Auto-discover URLs if not provided
if (-not $ApiUrl -or -not $McpUrl) {
    Write-Host "`n🔍 Auto-discovering service URLs..." -ForegroundColor Yellow
    
    if (-not $ApiUrl) {
        $apiOutputs = Get-AzdOutputs "FabrikamApi"
        if ($apiOutputs -and $apiOutputs.API_URI) {
            $ApiUrl = $apiOutputs.API_URI.value
            Write-Host "Found FabrikamApi URL: $ApiUrl" -ForegroundColor Cyan
        }
    }
    
    if (-not $McpUrl) {
        $mcpOutputs = Get-AzdOutputs "FabrikamMcp"
        if ($mcpOutputs -and $mcpOutputs.WEBSITE_URL) {
            $McpUrl = $mcpOutputs.WEBSITE_URL.value
            Write-Host "Found FabrikamMcp URL: $McpUrl" -ForegroundColor Cyan
        }
    }
}

if (-not $ApiUrl) {
    Write-Error "Could not determine FabrikamApi URL. Please provide -ApiUrl parameter."
    exit 1
}

if (-not $McpUrl) {
    Write-Error "Could not determine FabrikamMcp URL. Please provide -McpUrl parameter."
    exit 1
}

Write-Host "`n🌐 Testing Endpoints" -ForegroundColor Yellow
Write-Host "═══════════════════════" -ForegroundColor Gray

# Test API
$apiOk = Test-Endpoint -Url $ApiUrl -Name "FabrikamApi"

# Test MCP
$mcpOk = Test-Endpoint -Url $McpUrl -Name "FabrikamMcp"

# Test MCP configuration
Write-Host "`n🔧 Testing MCP Configuration" -ForegroundColor Yellow
Write-Host "═══════════════════════════════" -ForegroundColor Gray

try {
    $mcpStatus = Invoke-RestMethod -Uri "$McpUrl/status" -Method Get -TimeoutSec 30
    
    Write-Host "MCP Service: $($mcpStatus.Service)" -ForegroundColor White
    Write-Host "MCP Version: $($mcpStatus.Version)" -ForegroundColor White
    Write-Host "MCP Transport: $($mcpStatus.Transport)" -ForegroundColor White
    
    if ($mcpStatus.BusinessModules) {
        Write-Host "`n📋 Available Business Modules:" -ForegroundColor Cyan
        foreach ($module in $mcpStatus.BusinessModules) {
            Write-Host "  • $module" -ForegroundColor White
        }
    }
    
    Write-Host "`n✅ MCP configuration looks good" -ForegroundColor Green
}
catch {
    Write-Host "❌ Could not retrieve MCP configuration: $($_.Exception.Message)" -ForegroundColor Red
}

# Test API connectivity from MCP perspective
Write-Host "`n🔗 Testing API Connectivity (Future Enhancement)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Gray
Write-Host "To fully test MCP → API communication, you can:" -ForegroundColor White
Write-Host "1. Use VS Code with the MCP server: $McpUrl" -ForegroundColor Cyan  
Write-Host "2. Use MCP Inspector: npx @modelcontextprotocol/inspector" -ForegroundColor Cyan
Write-Host "3. Test business tools like 'get_sales_analytics'" -ForegroundColor Cyan

# Summary
Write-Host "`n📊 Test Results Summary" -ForegroundColor Yellow
Write-Host "══════════════════════" -ForegroundColor Gray

if ($apiOk -and $mcpOk) {
    Write-Host "🎉 All tests passed! Both services are running." -ForegroundColor Green
    Write-Host "`n🚀 Ready to use:" -ForegroundColor Green
    Write-Host "• Connect to MCP in VS Code: $McpUrl" -ForegroundColor White
    Write-Host "• Use MCP Inspector with: $McpUrl" -ForegroundColor White
    Write-Host "• API documentation: $ApiUrl/swagger" -ForegroundColor White
    exit 0
} else {
    Write-Host "❌ Some tests failed. Check the output above for details." -ForegroundColor Red
    exit 1
}
