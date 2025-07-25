# 🔧 GitHub Repository Status Checker
# Use this script to verify your repository configuration

param(
    [switch]$Detailed,
    [switch]$BranchProtection,
    [switch]$Actions,
    [switch]$All
)

Write-Host "🔍 Checking GitHub Repository Status..." -ForegroundColor Cyan
Write-Host "Repository: davebirr/Fabrikam-Project" -ForegroundColor Yellow
Write-Host ""

# Check if gh CLI is available
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "❌ GitHub CLI (gh) is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Install with: winget install --id GitHub.cli" -ForegroundColor Yellow
    exit 1
}

# Check authentication
gh auth status 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Not authenticated with GitHub CLI" -ForegroundColor Red
    Write-Host "Run: gh auth login" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ GitHub CLI authenticated" -ForegroundColor Green
Write-Host ""

# Basic Repository Info
Write-Host "📋 Repository Features:" -ForegroundColor Cyan
$repoInfo = gh repo view davebirr/Fabrikam-Project --json name,isPrivate,hasIssuesEnabled,hasProjectsEnabled,hasWikiEnabled,hasDiscussionsEnabled | ConvertFrom-Json

$visibility = if ($repoInfo.isPrivate) { "Private" } else { "Public" }
Write-Host "   Visibility: $visibility" -ForegroundColor $(if ($repoInfo.isPrivate) { "Yellow" } else { "Green" })
Write-Host "   Issues: $(if ($repoInfo.hasIssuesEnabled) { '✅ Enabled' } else { '❌ Disabled' })" -ForegroundColor $(if ($repoInfo.hasIssuesEnabled) { "Green" } else { "Red" })
Write-Host "   Projects: $(if ($repoInfo.hasProjectsEnabled) { '✅ Enabled' } else { '❌ Disabled' })" -ForegroundColor $(if ($repoInfo.hasProjectsEnabled) { "Green" } else { "Red" })
Write-Host "   Discussions: $(if ($repoInfo.hasDiscussionsEnabled) { '✅ Enabled' } else { '❌ Disabled' })" -ForegroundColor $(if ($repoInfo.hasDiscussionsEnabled) { "Green" } else { "Red" })
Write-Host "   Wiki: $(if ($repoInfo.hasWikiEnabled) { '✅ Enabled' } else { '❌ Disabled' })" -ForegroundColor $(if ($repoInfo.hasWikiEnabled) { "Green" } else { "Red" })
Write-Host ""

# Branch Protection
if ($BranchProtection -or $All) {
    Write-Host "🛡️ Branch Protection (main):" -ForegroundColor Cyan
    try {
        $branchInfo = gh api repos/davebirr/Fabrikam-Project/branches/main --jq '.protected' 2>$null
        if ($branchInfo -eq "true") {
            $protection = gh api repos/davebirr/Fabrikam-Project/branches/main/protection 2>$null | ConvertFrom-Json
            Write-Host "   ✅ Branch protection is enabled" -ForegroundColor Green
            
            if ($Detailed) {
                Write-Host "   Required Status Checks: $(if ($protection.required_status_checks.strict) { 'Strict' } else { 'Flexible' })" -ForegroundColor Yellow
                Write-Host "   Required Reviews: $($protection.required_pull_request_reviews.required_approving_review_count)" -ForegroundColor Yellow
                Write-Host "   Dismiss Stale Reviews: $(if ($protection.required_pull_request_reviews.dismiss_stale_reviews) { 'Yes' } else { 'No' })" -ForegroundColor Yellow
                Write-Host "   Enforce Admins: $(if ($protection.enforce_admins.enabled) { 'Yes' } else { 'No' })" -ForegroundColor Yellow
            }
        } else {
            Write-Host "   ❌ Branch protection is NOT enabled" -ForegroundColor Red
            Write-Host "   💡 Recommendation: Enable branch protection for main branch" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ Branch protection is NOT enabled" -ForegroundColor Red
        Write-Host "   💡 Recommendation: Enable branch protection for main branch" -ForegroundColor Yellow
    }
    Write-Host ""
}

# GitHub Actions
if ($Actions -or $All) {
    Write-Host "🚀 GitHub Actions:" -ForegroundColor Cyan
    
    # List workflows
    Write-Host "   Active Workflows:" -ForegroundColor Yellow
    $workflows = gh workflow list --json name,state,id | ConvertFrom-Json
    foreach ($workflow in $workflows) {
        $status = if ($workflow.state -eq "active") { "✅" } else { "❌" }
        Write-Host "     $status $($workflow.name)" -ForegroundColor $(if ($workflow.state -eq "active") { "Green" } else { "Red" })
    }
    
    Write-Host ""
    Write-Host "   Recent Workflow Runs:" -ForegroundColor Yellow
    $runs = gh run list --limit 3 --json status,conclusion,workflowName,headBranch,createdAt | ConvertFrom-Json
    foreach ($run in $runs) {
        $statusIcon = switch ($run.conclusion) {
            "success" { "✅" }
            "failure" { "❌" }
            "cancelled" { "⚠️" }
            default { "🔄" }
        }
        $time = [DateTime]::Parse($run.createdAt).ToString("MM/dd HH:mm")
        Write-Host "     $statusIcon $($run.workflowName) ($($run.headBranch)) - $time" -ForegroundColor $(if ($run.conclusion -eq "success") { "Green" } else { "Red" })
    }
    Write-Host ""
}

# Issue Templates Check
Write-Host "📝 Issue Templates:" -ForegroundColor Cyan
$templatePath = ".github/ISSUE_TEMPLATE"
if (Test-Path $templatePath) {
    $templates = Get-ChildItem $templatePath -Filter "*.yml" | Select-Object -ExpandProperty Name
    Write-Host "   ✅ Templates found: $($templates.Count)" -ForegroundColor Green
    foreach ($template in $templates) {
        Write-Host "     • $template" -ForegroundColor Gray
    }
} else {
    Write-Host "   ❌ No issue templates found" -ForegroundColor Red
}
Write-Host ""

# PR Template Check
Write-Host "📋 Pull Request Template:" -ForegroundColor Cyan
if (Test-Path ".github/PULL_REQUEST_TEMPLATE.md") {
    Write-Host "   ✅ PR template exists" -ForegroundColor Green
} else {
    Write-Host "   ❌ No PR template found" -ForegroundColor Red
}
Write-Host ""

# Contributing Guide Check
Write-Host "📖 Contributing Guide:" -ForegroundColor Cyan
if (Test-Path "CONTRIBUTING.md") {
    Write-Host "   ✅ CONTRIBUTING.md exists" -ForegroundColor Green
} else {
    Write-Host "   ❌ No CONTRIBUTING.md found" -ForegroundColor Red
}
Write-Host ""

# Quick Setup Recommendations
Write-Host "💡 Recommendations:" -ForegroundColor Cyan

$recommendations = @()

# Check branch protection
try {
    $branchProtected = gh api repos/davebirr/Fabrikam-Project/branches/main --jq '.protected' 2>$null
    if ($branchProtected -eq "false") {
        $recommendations += "Set up branch protection for main branch"
    }
} catch {
    $recommendations += "Set up branch protection for main branch"
}

# Check for recent failed runs
$recentRuns = gh run list --limit 5 --json conclusion | ConvertFrom-Json
$failedRuns = $recentRuns | Where-Object { $_.conclusion -eq "failure" }
if ($failedRuns.Count -gt 0) {
    $recommendations += "Investigate recent workflow failures"
}

if ($recommendations.Count -eq 0) {
    Write-Host "   ✅ Repository is well configured!" -ForegroundColor Green
} else {
    foreach ($rec in $recommendations) {
        Write-Host "   • $rec" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "🎯 Quick Commands:" -ForegroundColor Cyan
Write-Host "   gh repo view davebirr/Fabrikam-Project --web  # Open repository in browser" -ForegroundColor Gray
Write-Host "   gh issue list                                  # List open issues" -ForegroundColor Gray
Write-Host "   gh pr list                                     # List open pull requests" -ForegroundColor Gray
Write-Host "   gh run list                                    # List recent workflow runs" -ForegroundColor Gray
Write-Host "   gh workflow list                               # List all workflows" -ForegroundColor Gray
Write-Host ""
Write-Host "✅ Repository status check complete!" -ForegroundColor Green
