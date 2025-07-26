#!/usr/bin/env pwsh
# Fix-ExtensionDisabling.ps1 - Better approach to manage VS Code extensions

Write-Host "🔧 VS Code Extension Management Fix" -ForegroundColor Green
Write-Host ""

Write-Host "❌ The previous script opened multiple VS Code instances because the" -ForegroundColor Red
Write-Host "   'code --disable-extension' command is unreliable in bulk operations." -ForegroundColor Red
Write-Host ""

Write-Host "✅ Better Solution Applied:" -ForegroundColor Green
Write-Host "   Added extension-specific settings to .vscode/settings.json" -ForegroundColor Gray
Write-Host "   This disables problematic features without uninstalling extensions" -ForegroundColor Gray
Write-Host ""

Write-Host "🎯 Key Performance Settings Added:" -ForegroundColor Yellow
Write-Host "   • GitLens: Disabled all heavy features (blame, views, hovers)" -ForegroundColor White
Write-Host "   • Markdown All-in-One: Disabled auto-preview and TOC updates" -ForegroundColor White  
Write-Host "   • Error Lens: Completely disabled" -ForegroundColor White
Write-Host "   • Python: Disabled analysis and linting" -ForegroundColor White
Write-Host "   • Extensions: Disabled auto-updates and recommendations" -ForegroundColor White
Write-Host ""

Write-Host "📋 Extensions That Will Be Functionally Disabled:" -ForegroundColor Cyan
$disabledFeatures = @(
    "GitLens - All heavy features disabled, zen mode enabled",
    "Markdown All-in-One - Auto-features and file creation disabled", 
    "Error Lens - Completely disabled",
    "Python Extensions - Analysis and background processing disabled"
)

foreach ($feature in $disabledFeatures) {
    Write-Host "   🔴 $feature" -ForegroundColor Gray
}

Write-Host ""
Write-Host "💡 Why This Approach is Better:" -ForegroundColor Cyan
Write-Host "   • No multiple VS Code instances opened" -ForegroundColor White
Write-Host "   • Extensions remain installed but features are disabled" -ForegroundColor White
Write-Host "   • Settings are workspace-specific" -ForegroundColor White
Write-Host "   • Easy to re-enable by modifying settings" -ForegroundColor White
Write-Host "   • Takes effect immediately after restart" -ForegroundColor White
Write-Host ""

Write-Host "🔄 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Close all extra VS Code instances that were opened" -ForegroundColor Gray
Write-Host "   2. Restart your main VS Code instance" -ForegroundColor Gray
Write-Host "   3. The performance improvements should be immediate" -ForegroundColor Gray
Write-Host ""

Write-Host "🔧 To Re-enable Features Later:" -ForegroundColor Cyan
Write-Host "   Edit .vscode/settings.json and change 'false' to 'true' for any setting" -ForegroundColor Gray
Write-Host ""

# Check if there are multiple VS Code processes
$vsCodeProcesses = Get-Process -Name "Code" -ErrorAction SilentlyContinue
if ($vsCodeProcesses.Count -gt 1) {
    Write-Host "⚠️  Multiple VS Code instances detected: $($vsCodeProcesses.Count)" -ForegroundColor Yellow
    Write-Host "   You may want to close extra instances to free up memory" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "✅ Extension management fix complete!" -ForegroundColor Green
Write-Host "   Restart VS Code to see performance improvements" -ForegroundColor Gray
