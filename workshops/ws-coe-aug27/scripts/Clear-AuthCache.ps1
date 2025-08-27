# 🧹 Clear Authentication Cache Script
# Use this script to clear PowerShell authentication cache when experiencing account selection issues

Write-Host "🧹 Clearing PowerShell Authentication Cache..." -ForegroundColor Cyan
Write-Host "This will disconnect all active Graph and Azure sessions" -ForegroundColor Yellow
Write-Host ""

try {
    # Disconnect Microsoft Graph
    Write-Host "📱 Disconnecting Microsoft Graph..." -ForegroundColor White
    Disconnect-MgGraph -ErrorAction SilentlyContinue | Out-Null
    
    # Disconnect Azure Account
    Write-Host "☁️  Disconnecting Azure Account..." -ForegroundColor White  
    Disconnect-AzAccount -ErrorAction SilentlyContinue | Out-Null
    
    # Clear Azure Context
    Write-Host "🗑️  Clearing Azure Context..." -ForegroundColor White
    Clear-AzContext -Force -ErrorAction SilentlyContinue | Out-Null
    
    Write-Host ""
    Write-Host "✅ Authentication cache cleared successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Next steps:" -ForegroundColor Yellow
    Write-Host "1. Run .\Run-COE-Provisioning.ps1 again" -ForegroundColor White
    Write-Host "2. Choose option 1 for test run or option 2 for actual provisioning" -ForegroundColor White
    Write-Host "3. When prompted, authenticate with your target tenant account" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host "⚠️  Cache clearing completed (some modules may not be loaded)" -ForegroundColor Yellow
    Write-Host "This is normal if you haven't used the authentication modules yet" -ForegroundColor Gray
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
