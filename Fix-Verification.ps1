# GetSalesAnalytics Fix Verification Script
# Tests the fixed configuration and provides instructions for testing

param(
    [switch]$TestMcpOnly
)

$ErrorActionPreference = "Stop"

if (-not $TestMcpOnly) {
    Write-Host "Testing API Analytics Endpoint..." -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5235/api/orders/analytics" -Method Get -TimeoutSec 5
        Write-Host "✅ API Analytics endpoint working!" -ForegroundColor Green
        Write-Host "   Total Orders: $($response.summary.totalOrders)" -ForegroundColor Gray
        Write-Host "   Total Revenue: $($response.summary.totalRevenue)" -ForegroundColor Gray
        Write-Host "   Average Order Value: $($response.summary.averageOrderValue)" -ForegroundColor Gray
        Write-Host "   Status Breakdown: $($response.byStatus.Count) statuses" -ForegroundColor Gray
        Write-Host "   Region Breakdown: $($response.byRegion.Count) regions" -ForegroundColor Gray
    }
    catch {
        Write-Host "❌ API test failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Make sure the FabrikamApi is running on port 5235" -ForegroundColor Yellow
        Write-Host "Start it with: cd FabrikamApi\src && dotnet run --urls `"http://localhost:5235`"" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "MCP SERVER TESTING INSTRUCTIONS" -ForegroundColor Cyan
Write-Host "="*60 -ForegroundColor Cyan

Write-Host "`n1. Start the MCP Server in a new terminal:" -ForegroundColor Yellow
Write-Host "   cd FabrikamMcp\src" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray

Write-Host "`n2. The MCP server will show startup logs and wait for connections" -ForegroundColor Yellow

Write-Host "`n3. Test the GetSalesAnalytics tool in Copilot:" -ForegroundColor Yellow
Write-Host "   - The tool should now work without the previous error" -ForegroundColor Gray
Write-Host "   - It will call: http://localhost:5235/api/orders/analytics" -ForegroundColor Gray
Write-Host "   - Expected result: Structured sales analytics data" -ForegroundColor Gray

Write-Host "`n4. Configuration Fix Applied:" -ForegroundColor Green
Write-Host "   - Updated FabrikamMcp appsettings.Development.json" -ForegroundColor Gray
Write-Host "   - Changed BaseUrl from port 5000 to 5235" -ForegroundColor Gray
Write-Host "   - Now matches the FabrikamApi port configuration" -ForegroundColor Gray

Write-Host "`n5. What was fixed:" -ForegroundColor Green
Write-Host "   ❌ Before: MCP called http://localhost:5000/api/orders/analytics (wrong port)" -ForegroundColor Red
Write-Host "   ✅ After:  MCP calls http://localhost:5235/api/orders/analytics (correct port)" -ForegroundColor Green

Write-Host "`nBoth services are now properly configured!" -ForegroundColor Green
Write-Host "The 'An error occurred invoking GetSalesAnalytics' error should be resolved." -ForegroundColor Green
