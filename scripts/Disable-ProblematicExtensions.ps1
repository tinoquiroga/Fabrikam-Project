#!/usr/bin/env pwsh
# Disable-ProblematicExtensions.ps1 - Disable VS Code extensions causing UI freezes

Write-Host "🧹 Disabling Problematic VS Code Extensions" -ForegroundColor Green
Write-Host ""

# Extensions that are most likely causing UI freezes and file recreation
$problematicExtensions = @(
    "yzhang.markdown-all-in-one",     # Creates markdown files from broken links
    "eamodio.gitlens",                # Heavy git processing
    "usernamehw.errorlens"            # Constant error processing
)

# Heavy Azure extensions (disable unless specifically needed)
$azureExtensions = @(
    "ms-azuretools.vscode-azureappservice",
    "ms-azuretools.vscode-azurecontainerapps", 
    "ms-azuretools.vscode-azurefunctions",
    "ms-azuretools.vscode-azurestaticwebapps",
    "ms-azuretools.vscode-azurestorage",
    "ms-azuretools.vscode-azurevirtualmachines",
    "ms-azuretools.vscode-cosmosdb",
    "ms-azuretools.vscode-containers",
    "ms-azuretools.vscode-docker"
)

# Non-.NET language extensions
$languageExtensions = @(
    "ms-python.python",
    "ms-python.debugpy", 
    "ms-python.vscode-pylance",
    "ms-python.vscode-python-envs",
    "ms-playwright.playwright",
    "vue.volar",
    "vitest.explorer"
)

# Optional tools that might cause performance issues
$optionalExtensions = @(
    "alefragnani.bookmarks",
    "alefragnani.project-manager",
    "christian-kohler.path-intellisense",
    "ms-azure-load-testing.microsoft-testing",
    "ms-windows-ai-studio.windows-ai-studio",
    "teamsdevapp.ms-teams-vscode-extension",
    "teamsdevapp.vscode-ai-foundry"
)

function Disable-Extension($extensionId, $reason) {
    Write-Host "🔴 Disabling: $extensionId" -ForegroundColor Red
    Write-Host "   Reason: $reason" -ForegroundColor Gray
    try {
        & code --disable-extension $extensionId
        Write-Host "   ✅ Disabled successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "   ❌ Failed to disable: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "📋 Phase 1: Critical Performance Issues" -ForegroundColor Yellow
foreach ($ext in $problematicExtensions) {
    switch ($ext) {
        "yzhang.markdown-all-in-one" { 
            Disable-Extension $ext "Creates empty markdown files from broken links"
        }
        "eamodio.gitlens" { 
            Disable-Extension $ext "Heavy git processing causes UI freezes"
        }
        "usernamehw.errorlens" { 
            Disable-Extension $ext "Constant error processing impacts performance"
        }
    }
}

Write-Host "📋 Phase 2: Heavy Azure Extensions" -ForegroundColor Yellow
foreach ($ext in $azureExtensions) {
    Disable-Extension $ext "Heavy Azure tooling - disable unless specifically needed"
}

Write-Host "📋 Phase 3: Non-.NET Language Support" -ForegroundColor Yellow
foreach ($ext in $languageExtensions) {
    Disable-Extension $ext "Not needed for .NET development"
}

Write-Host "📋 Phase 4: Optional Tools" -ForegroundColor Yellow
foreach ($ext in $optionalExtensions) {
    Disable-Extension $ext "Optional tool that may impact performance"
}

Write-Host ""
Write-Host "✅ Extension cleanup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Essential extensions kept:" -ForegroundColor Cyan
Write-Host "  • C# Dev Kit (ms-dotnettools.csdevkit)" -ForegroundColor White
Write-Host "  • C# Language Support (ms-dotnettools.csharp)" -ForegroundColor White
Write-Host "  • GitHub Copilot (github.copilot)" -ForegroundColor White
Write-Host "  • GitHub Copilot Chat (github.copilot-chat)" -ForegroundColor White
Write-Host "  • REST Client (humao.rest-client)" -ForegroundColor White
Write-Host "  • Azure Developer CLI (ms-azuretools.azure-dev)" -ForegroundColor White
Write-Host "  • Bicep (ms-azuretools.vscode-bicep)" -ForegroundColor White
Write-Host "  • GitHub PR & Issues (github.vscode-pull-request-github)" -ForegroundColor White
Write-Host ""
Write-Host "🔄 Please restart VS Code to apply changes" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 You can re-enable extensions later if needed:" -ForegroundColor Cyan
Write-Host "   code --enable-extension <extension-id>" -ForegroundColor Gray
