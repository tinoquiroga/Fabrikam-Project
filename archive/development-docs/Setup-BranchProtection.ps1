# 🛡️ Setup Branch Protection for Fabrikam Project
# This script will configure branch protection with status checks

param(
    [switch]$DryRun,
    [switch]$Force
)

Write-Host "🛡️ Setting up branch protection for main branch..." -ForegroundColor Cyan
Write-Host "Repository: davebirr/Fabrikam-Project" -ForegroundColor Yellow
Write-Host ""

# Check if gh CLI is available and authenticated
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "❌ GitHub CLI (gh) is not installed" -ForegroundColor Red
    exit 1
}

gh auth status 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Not authenticated with GitHub CLI" -ForegroundColor Red
    exit 1
}

# Check current protection status
Write-Host "🔍 Checking current branch protection..." -ForegroundColor Cyan
$isProtected = gh api repos/davebirr/Fabrikam-Project/branches/main --jq '.protected' 2>$null

if ($isProtected -eq "true" -and -not $Force) {
    Write-Host "⚠️ Branch protection is already enabled" -ForegroundColor Yellow
    Write-Host "Use -Force flag to update existing protection rules" -ForegroundColor Yellow
    exit 0
}

# Define protection rules
$protectionRules = @{
    required_status_checks           = @{
        strict   = $true
        contexts = @(
            "Testing Pipeline",  # Your main testing workflow
            "Deploy Fabrikam Full Stack"  # Your deployment workflow
        )
    }
    enforce_admins                   = $false
    required_pull_request_reviews    = @{
        required_approving_review_count = 1
        dismiss_stale_reviews           = $true
        require_code_owner_reviews      = $false
        require_last_push_approval      = $false
    }
    restrictions                     = $null
    allow_force_pushes               = $false
    allow_deletions                  = $false
    block_creations                  = $false
    required_conversation_resolution = $true
}

$jsonPayload = $protectionRules | ConvertTo-Json -Depth 10

if ($DryRun) {
    Write-Host "🔍 DRY RUN - Would apply these protection rules:" -ForegroundColor Yellow
    Write-Host $jsonPayload -ForegroundColor Gray
    Write-Host ""
    Write-Host "Run without -DryRun flag to actually apply these rules" -ForegroundColor Yellow
    exit 0
}

# Apply branch protection
Write-Host "🛡️ Applying branch protection rules..." -ForegroundColor Cyan

try {
    $response = $jsonPayload | gh api repos/davebirr/Fabrikam-Project/branches/main/protection --method PUT --input - 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Branch protection successfully configured!" -ForegroundColor Green
        Write-Host ""
        
        # Show what was configured
        Write-Host "📋 Protection rules applied:" -ForegroundColor Cyan
        Write-Host "   • Require pull request reviews (1 approval minimum)" -ForegroundColor Green
        Write-Host "   • Dismiss stale reviews when new commits are pushed" -ForegroundColor Green
        Write-Host "   • Require status checks to pass before merging" -ForegroundColor Green
        Write-Host "   • Require branches to be up to date before merging" -ForegroundColor Green
        Write-Host "   • Require conversation resolution before merging" -ForegroundColor Green
        Write-Host "   • Prevent force pushes" -ForegroundColor Green
        Write-Host "   • Prevent deletions" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "🎯 Status checks required:" -ForegroundColor Cyan
        Write-Host "   • Testing Pipeline" -ForegroundColor Yellow
        Write-Host "   • Deploy Fabrikam Full Stack" -ForegroundColor Yellow
        Write-Host ""
        
        Write-Host "💡 Next steps:" -ForegroundColor Cyan
        Write-Host "   1. Test by creating a pull request" -ForegroundColor Gray
        Write-Host "   2. Verify status checks run automatically" -ForegroundColor Gray
        Write-Host "   3. Confirm approval is required before merging" -ForegroundColor Gray
        
    }
    else {
        Write-Host "❌ Failed to configure branch protection" -ForegroundColor Red
        Write-Host $response -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Error configuring branch protection: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔧 Useful commands:" -ForegroundColor Cyan
Write-Host "   .\Check-RepoStatus.ps1 -BranchProtection  # Check protection status" -ForegroundColor Gray
Write-Host "   gh repo view --web                        # View repository in browser" -ForegroundColor Gray
