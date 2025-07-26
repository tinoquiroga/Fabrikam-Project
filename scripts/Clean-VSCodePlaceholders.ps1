#!/usr/bin/env pwsh
# Clean-VSCodePlaceholders.ps1 - Remove empty placeholder files created by VS Code extensions

param(
    [switch]$DryRun,    # Show what would be removed without actually removing
    [switch]$Verbose    # Show detailed output
)

Write-Host "🧹 VS Code Placeholder File Cleanup" -ForegroundColor Green
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No files will be removed" -ForegroundColor Yellow
    Write-Host ""
}

# Known problematic file patterns that VS Code extensions recreate
$problemPatterns = @{
    "Empty DTO files (moved to FabrikamContracts)"  = @(
        "FabrikamApi/src/DTOs/*.cs"
    )
    "Empty contract files (duplicates)"             = @(
        "FabrikamContracts/AlignedDTOs.cs",
        "FabrikamContracts/Class1.cs"
    )
    "Misplaced markdown files (should be in docs/)" = @(
        "COPILOT-DEMO-PROMPTS.md",
        "COPILOT-REMINDER.md", 
        "DEMO-READY-SUMMARY.md",
        "TODO-FUTURE-ENHANCEMENTS.md",
        "QUICK-DEMO-PROMPTS.md",
        "AUTHENTICATION-IMPLEMENTATION-GUIDE.md",
        "BUSINESS-DASHBOARD-FIX.md",
        "DEPLOYMENT-GUIDE.md",
        "DEVELOPMENT-WORKFLOW.md",
        "DOCUMENTATION-CLEANUP-SUMMARY.md",
        "DTO-ARCHITECTURE-SCHEMA.md",
        "ENHANCED-TESTING-COMPLETE.md",
        "HYBRID-SEED-SERVICE-SUCCESS.md",
        "JSON-SEED-DATA-SUCCESS.md",
        "ORDER-TIMELINE-IMPLEMENTATION-COMPLETE.md",
        "PRODUCTION-TESTING-GUIDE.md",
        "PROJECT-TESTING-SUMMARY.md",
        "README-NEW.md",
        "RELEASE-GUIDE.md",
        "REPOSITORY-CLEANUP-ANALYSIS.md",
        "REPOSITORY-CLEANUP-COMPLETE.md",
        "REVENUE-CALCULATION-FIX-COMPLETE.md",
        "TESTING-STRATEGY.md"
    )
    "Misplaced script files"                        = @(
        "Check-RepoStatus.ps1",
        "Deploy-Azure-Resources.ps1", 
        "Deploy-Integrated.ps1",
        "Fix-Verification.ps1",
        "Inject-Orders.ps1",
        "Setup-BranchProtection.ps1",
        "Test-BusinessDashboard.ps1",
        "Test-GetSalesAnalytics.ps1",
        "Test-Integration.ps1",
        "Validate-Demo.ps1",
        "test-mcp-smart-fallback.ps1"
    )
    "Misplaced guide files"                         = @(
        "Copilot-Studio-Agent-Setup-Guide.md",
        "azure-b2c-setup-issue.md",
        "database-schema-issue.md", 
        "phase1-issue.md"
    )
    "Misplaced asset files"                         = @(
        "FabrikamApi/ASSET-MANAGEMENT-GUIDE.md"
    )
    "VS Code extension artifacts"                   = @(
        "FabrikamApi/Controllers/*",
        "FabrikamApi/Properties/*", 
        "FabrikamMcp/Properties/*"
    )
    "Empty test files (recreated by VS Code)"       = @(
        "test-*.json",
        "Test*.cs"
    )
    "Archive empty files (recreation artifacts)"    = @(
        "archive/completed-tasks/*.md",
        "archive/duplicate-files/*.md", 
        "archive/old-test-files/*.json",
        "archive/old-test-files/TestMcpTools/*.cs"
    )
    "Documentation empty files"                     = @(
        "docs/*.md",
        "docs/development/*.md"
    )
    "Seed data empty files"                         = @(
        "FabrikamApi/src/Data/SeedData/*.json"
    )
    "Test project empty files"                      = @(
        "FabrikamTests/**/*.cs"
    )
    "Script empty files"                            = @(
        "scripts/*.ps1"
    )
}

$totalFilesFound = 0
$totalFilesRemoved = 0
$totalFilesWouldRemove = 0

foreach ($category in $problemPatterns.Keys) {
    Write-Host "📁 $category" -ForegroundColor Cyan
    
    $categoryFiles = @()
    foreach ($pattern in $problemPatterns[$category]) {
        if ($pattern.Contains("*")) {
            # Handle wildcard patterns
            $files = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue
            $categoryFiles += $files
        }
        else {
            # Handle specific files
            if (Test-Path $pattern) {
                $categoryFiles += Get-Item $pattern
            }
        }
    }
    
    if ($categoryFiles.Count -eq 0) {
        Write-Host "  ✅ No problematic files found" -ForegroundColor Green
    }
    else {
        foreach ($file in $categoryFiles) {
            $totalFilesFound++
            
            # Check if file is empty or very small (likely placeholder)
            $isEmpty = $file.Length -eq 0
            $isSmall = $file.Length -lt 100
            $isPlaceholder = $isEmpty -or ($isSmall -and (Get-Content $file -Raw -ErrorAction SilentlyContinue) -match "^\s*$")
            
            if ($isPlaceholder) {
                if ($DryRun) {
                    Write-Host "  🗑️  Would remove: $($file.FullName) ($($file.Length) bytes)" -ForegroundColor Yellow
                    $totalFilesWouldRemove++
                }
                else {
                    try {
                        Remove-Item $file.FullName -Force
                        $totalFilesRemoved++
                        Write-Host "  ✅ Removed: $($file.Name) ($($file.Length) bytes)" -ForegroundColor Green
                        
                        if ($Verbose) {
                            Write-Host "     Path: $($file.FullName)" -ForegroundColor Gray
                        }
                    }
                    catch {
                        Write-Host "  ❌ Failed to remove: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
                    }
                }
            }
            else {
                Write-Host "  ⚠️  Skipped (has content): $($file.Name) ($($file.Length) bytes)" -ForegroundColor Yellow
                if ($Verbose) {
                    Write-Host "     Path: $($file.FullName)" -ForegroundColor Gray
                }
            }
        }
    }
    Write-Host ""
}

# Remove empty directories
Write-Host "📁 Checking for empty directories to remove:" -ForegroundColor Cyan
$emptyDirs = @(
    "FabrikamApi/src/DTOs",
    "FabrikamApi/Controllers", 
    "FabrikamApi/Properties",
    "FabrikamMcp/Properties"
)
foreach ($dir in $emptyDirs) {
    if (Test-Path $dir) {
        $items = Get-ChildItem $dir -ErrorAction SilentlyContinue
        if ($items.Count -eq 0) {
            if ($DryRun) {
                Write-Host "  🗑️  Would remove empty directory: $dir" -ForegroundColor Yellow
            }
            else {
                try {
                    Remove-Item $dir -Force
                    Write-Host "  ✅ Removed empty directory: $dir" -ForegroundColor Green
                }
                catch {
                    Write-Host "  ❌ Failed to remove directory: $dir - $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    }
}

# Additional scan for any empty directories that match problematic patterns
Write-Host ""
Write-Host "📁 Scanning for additional problematic empty directories:" -ForegroundColor Cyan
$problematicDirPatterns = @(
    "**/Controllers",
    "**/Properties", 
    "**/DTOs",
    "**/TestMcpTools"
)

$foundEmptyDirs = @()
foreach ($pattern in $problematicDirPatterns) {
    $dirs = Get-ChildItem -Directory -Recurse -Path $pattern -ErrorAction SilentlyContinue
    foreach ($dir in $dirs) {
        $items = Get-ChildItem $dir.FullName -ErrorAction SilentlyContinue
        if ($items.Count -eq 0 -and $dir.FullName -notmatch "\.git" -and $dir.FullName -notmatch "node_modules") {
            $foundEmptyDirs += $dir
        }
    }
}

if ($foundEmptyDirs.Count -eq 0) {
    Write-Host "  ✅ No additional empty directories found" -ForegroundColor Green
}
else {
    foreach ($dir in $foundEmptyDirs) {
        if ($DryRun) {
            Write-Host "  🗑️  Would remove empty directory: $($dir.FullName)" -ForegroundColor Yellow
        }
        else {
            try {
                Remove-Item $dir.FullName -Force
                Write-Host "  ✅ Removed empty directory: $($dir.Name)" -ForegroundColor Green
            }
            catch {
                Write-Host "  ❌ Failed to remove directory: $($dir.Name) - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

# Comprehensive empty file scan (catch any missed by patterns)
Write-Host ""
Write-Host "📁 Scanning for any additional empty files:" -ForegroundColor Cyan
$allEmptyFiles = Get-ChildItem -File -Recurse | Where-Object { 
    $_.Length -eq 0 -and 
    $_.FullName -notmatch "\.git" -and
    $_.FullName -notmatch "bin\\|obj\\|node_modules\\" 
}

if ($allEmptyFiles.Count -eq 0) {
    Write-Host "  ✅ No additional empty files found" -ForegroundColor Green
}
else {
    Write-Host "  ⚠️  Found $($allEmptyFiles.Count) additional empty files:" -ForegroundColor Yellow
    foreach ($file in $allEmptyFiles) {
        $totalFilesFound++
        if ($DryRun) {
            Write-Host "  🗑️  Would remove: $($file.FullName) (0 bytes)" -ForegroundColor Yellow
            $totalFilesWouldRemove++
        }
        else {
            try {
                Remove-Item $file.FullName -Force
                $totalFilesRemoved++
                Write-Host "  ✅ Removed: $($file.Name) (0 bytes)" -ForegroundColor Green
                if ($Verbose) {
                    Write-Host "     Path: $($file.FullName)" -ForegroundColor Gray
                }
            }
            catch {
                Write-Host "  ❌ Failed to remove: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Green
Write-Host "  Files found: $totalFilesFound" -ForegroundColor White
if ($DryRun) {
    Write-Host "  Files that would be removed: $totalFilesWouldRemove" -ForegroundColor Yellow
}
else {
    Write-Host "  Files removed: $totalFilesRemoved" -ForegroundColor Green
}

if (-not $DryRun -and $totalFilesRemoved -gt 0) {
    Write-Host ""
    Write-Host "💡 To prevent these files from reappearing:" -ForegroundColor Cyan
    Write-Host "1. Clear VS Code workspace cache: Close VS Code, restart" -ForegroundColor White
    Write-Host "2. Add patterns to .gitignore if they persist" -ForegroundColor White
    Write-Host "3. Check VS Code extensions (C# Dev Kit, markdown processors)" -ForegroundColor White
}
