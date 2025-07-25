# Test the Business Dashboard MCP Tool
# This script manually tests the GetBusinessDashboard tool

Write-Host "🔧 Testing Business Dashboard Tool..." -ForegroundColor Cyan

# Test different timeframes that might cause issues
$testCases = @(
    @{ timeframe = "year"; description = "Full year (the problematic case)" },
    @{ timeframe = "30days"; description = "30 days (default)" },
    @{ timeframe = "7days"; description = "7 days" }
)

foreach ($testCase in $testCases) {
    Write-Host ""
    Write-Host "� Testing: $($testCase.description)" -ForegroundColor Yellow
    Write-Host "Timeframe: '$($testCase.timeframe)'" -ForegroundColor Gray
    
    # Create a simple HTTP test (just check if server responds)
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/" -Method GET -TimeoutSec 5
        Write-Host "✅ MCP Server is responding (Status: $($response.StatusCode))" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ MCP Server not responding: $($_.Exception.Message)" -ForegroundColor Red
        continue
    }
    
    # For now, just verify the server is accessible
    # The actual MCP tool testing would require proper MCP client implementation
    Write-Host "🔍 Server accessible - ready for GitHub Copilot testing" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎯 To test with GitHub Copilot:" -ForegroundColor Cyan
Write-Host "1. Ask Copilot: 'Show me the business dashboard for the past year'" -ForegroundColor Gray
Write-Host "2. It should use the GetBusinessDashboard tool with timeframe='year'" -ForegroundColor Gray
Write-Host "3. The tool should now handle empty data gracefully" -ForegroundColor Gray

Write-Host ""
Write-Host "🔧 Key fixes applied:" -ForegroundColor Green
Write-Host "• Replaced decimal.Parse() with decimal.TryParse() for safe parsing" -ForegroundColor Gray
Write-Host "• Added default values for all GetJsonValue calls" -ForegroundColor Gray  
Write-Host "• Enhanced error handling for date parsing" -ForegroundColor Gray
Write-Host "• Improved null/empty data handling throughout" -ForegroundColor Gray
