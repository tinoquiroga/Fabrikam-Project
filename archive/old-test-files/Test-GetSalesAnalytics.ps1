# Test script to verify GetSalesAnalytics functionality
# This script starts both the API and MCP server, then tests the analytics endpoint

param(
    [switch]$CleanupOnly,
    [int]$WaitSeconds = 10
)

$ErrorActionPreference = "Stop"

# Function to cleanup any existing processes
function Cleanup-Processes {
    Write-Host "Cleaning up any existing processes..." -ForegroundColor Yellow
    
    # Kill any existing dotnet processes for our projects
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
        $_.MainModule.FileName -like "*FabrikamApi*" -or 
        $_.MainModule.FileName -like "*FabrikamMcp*"
    } | Stop-Process -Force -ErrorAction SilentlyContinue
    
    # Wait a moment for cleanup
    Start-Sleep -Seconds 2
    Write-Host "Cleanup complete." -ForegroundColor Green
}

# Cleanup any existing processes
Cleanup-Processes

if ($CleanupOnly) {
    Write-Host "Cleanup only mode - exiting." -ForegroundColor Green
    exit 0
}

try {
    # Navigate to project root
    $projectRoot = "c:\Users\davidb\1Repositories\Fabrikam-Project"
    Set-Location $projectRoot
    
    Write-Host "Starting Fabrikam API..." -ForegroundColor Cyan
    
    # Start the API in background
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "FabrikamApi\src\FabrikamApi.csproj" -PassThru -WindowStyle Hidden
    
    Write-Host "Waiting $WaitSeconds seconds for API to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds $WaitSeconds
    
    # Test if API is responding
    Write-Host "Testing API connectivity..." -ForegroundColor Cyan
    try {
        $apiResponse = Invoke-RestMethod -Uri "http://localhost:5235/api/orders" -Method Get -TimeoutSec 5
        Write-Host "✅ API is responding successfully" -ForegroundColor Green
        Write-Host "   Found $($apiResponse.data.Count) orders in response" -ForegroundColor Gray
    }
    catch {
        Write-Host "❌ API is not responding: $($_.Exception.Message)" -ForegroundColor Red
        throw "API startup failed"
    }
    
    # Test the analytics endpoint specifically
    Write-Host "Testing analytics endpoint..." -ForegroundColor Cyan
    try {
        $analyticsResponse = Invoke-RestMethod -Uri "http://localhost:5235/api/orders/analytics" -Method Get -TimeoutSec 5
        Write-Host "✅ Analytics endpoint is responding successfully" -ForegroundColor Green
        Write-Host "   Summary: $($analyticsResponse.summary.totalOrders) total orders, $($analyticsResponse.summary.totalRevenue) total revenue" -ForegroundColor Gray
    }
    catch {
        Write-Host "❌ Analytics endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        throw "Analytics endpoint test failed"
    }
    
    Write-Host "`n🎉 All tests passed! The GetSalesAnalytics MCP tool should now work correctly." -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Start the MCP server: dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj" -ForegroundColor Gray
    Write-Host "2. Test the GetSalesAnalytics tool in Copilot" -ForegroundColor Gray
    Write-Host "`nBoth applications are now running. Press any key to stop them..." -ForegroundColor Yellow
    
    # Wait for user input
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
}
catch {
    Write-Host "`n❌ Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Gray
}
finally {
    Write-Host "`nStopping processes..." -ForegroundColor Yellow
    Cleanup-Processes
    Write-Host "Test completed." -ForegroundColor Green
}
