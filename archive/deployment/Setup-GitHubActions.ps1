#Requires -Version 7.0
<#
.SYNOPSIS
    Configure GitHub Actions CI/CD after ARM template deployment

.DESCRIPTION
    This script sets up GitHub Actions workflows for a Fabrikam instance deployment,
    following the fork-and-sync pattern where users maintain their own forks and
    sync updates from the upstream repository.

.PARAMETER ResourceGroup
    The Azure resource group where resources were deployed

.PARAMETER GitHubRepository
    The GitHub repository URL (should be user's fork)

.PARAMETER GitHubToken
    GitHub Personal Access Token with repo and workflow permissions

.PARAMETER UpstreamRepository
    The upstream repository to sync from (default: davebirr/Fabrikam-Project)

.PARAMETER SetupFork
    Whether to configure the fork relationship and upstream remote

.EXAMPLE
    .\Setup-GitHubActions.ps1 -ResourceGroup "rg-fabrikam-test" -GitHubRepository "https://github.com/username/Fabrikam-Project" -GitHubToken "ghp_xxxxx"

.EXAMPLE
    .\Setup-GitHubActions.ps1 -ResourceGroup "rg-fabrikam-test" -GitHubRepository "https://github.com/username/Fabrikam-Project" -GitHubToken "ghp_xxxxx" -SetupFork
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory = $true)]
    [string]$GitHubRepository,
    
    [Parameter(Mandatory = $true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory = $false)]
    [string]$UpstreamRepository = "https://github.com/davebirr/Fabrikam-Project",
    
    [Parameter(Mandatory = $false)]
    [switch]$SetupFork
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Setting Up GitHub Actions CI/CD for Fabrikam Instance" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

try {
    # Verify prerequisites
    Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow
    
    # Check Azure CLI
    $azAccount = az account show --output json 2>$null | ConvertFrom-Json
    if (-not $azAccount) {
        Write-Error "❌ Azure CLI not logged in. Please run 'az login' first."
        exit 1
    }
    
    # Check GitHub CLI
    $ghVersion = gh --version 2>$null
    if (-not $ghVersion) {
        Write-Error "❌ GitHub CLI not found. Please install GitHub CLI: https://cli.github.com/"
        exit 1
    }
    
    # Check if logged into GitHub
    $ghUser = gh auth status 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Not logged into GitHub CLI. Attempting login with token..." -ForegroundColor Yellow
        echo $GitHubToken | gh auth login --with-token
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Failed to authenticate with GitHub"
            exit 1
        }
    }
    
    Write-Host "✅ Prerequisites verified" -ForegroundColor Green
    
    # Get deployment outputs
    Write-Host "🔍 Getting deployment information..." -ForegroundColor Yellow
    $deployments = az deployment group list --resource-group $ResourceGroup --output json | ConvertFrom-Json
    $latestDeployment = $deployments | Where-Object { $_.properties.provisioningState -eq "Succeeded" } | Sort-Object { $_.properties.timestamp } -Descending | Select-Object -First 1
    
    if (-not $latestDeployment) {
        Write-Error "❌ No successful deployments found in resource group: $ResourceGroup"
        exit 1
    }
    
    $outputs = $latestDeployment.properties.outputs
    $apiAppName = $outputs.apiAppName.value
    $mcpAppName = $outputs.mcpAppName.value
    $instanceSuffix = $outputs.instanceSuffix.value
    $subscriptionId = $azAccount.id
    
    Write-Host "📊 Instance Information:" -ForegroundColor Cyan
    Write-Host "  Suffix: $instanceSuffix" -ForegroundColor White
    Write-Host "  API App: $apiAppName" -ForegroundColor White
    Write-Host "  MCP App: $mcpAppName" -ForegroundColor White
    Write-Host "  Subscription: $subscriptionId" -ForegroundColor White
    
    # Parse repository information
    if ($GitHubRepository -match "github\.com[:/]([^/]+)/([^/]+?)(?:\.git)?/?$") {
        $repoOwner = $matches[1]
        $repoName = $matches[2]
    } else {
        Write-Error "❌ Invalid GitHub repository URL format"
        exit 1
    }
    
    Write-Host "🔗 Repository Information:" -ForegroundColor Cyan
    Write-Host "  Owner: $repoOwner" -ForegroundColor White
    Write-Host "  Repository: $repoName" -ForegroundColor White
    
    # Create service principal for deployment
    Write-Host "🔐 Creating service principal for deployment..." -ForegroundColor Yellow
    $spName = "sp-fabrikam-$instanceSuffix"
    
    # Check if service principal already exists
    $existingSp = az ad sp list --display-name $spName --output json | ConvertFrom-Json
    if ($existingSp -and $existingSp.Count -gt 0) {
        Write-Host "♻️  Service principal already exists, using existing one" -ForegroundColor Yellow
        $spAppId = $existingSp[0].appId
    } else {
        $spOutput = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroup" --output json | ConvertFrom-Json
        $spAppId = $spOutput.appId
        $spPassword = $spOutput.password
        $spTenant = $spOutput.tenant
        
        Write-Host "✅ Service principal created: $spAppId" -ForegroundColor Green
    }
    
    # Create Azure credentials object
    $azureCredentials = @{
        clientId = $spAppId
        clientSecret = $spPassword
        subscriptionId = $subscriptionId
        tenantId = $spTenant
    } | ConvertTo-Json -Compress
    
    # Set up GitHub repository secrets
    Write-Host "🔒 Setting up GitHub repository secrets..." -ForegroundColor Yellow
    
    $secrets = @{
        "AZURE_CREDENTIALS" = $azureCredentials
        "AZURE_SUBSCRIPTION_ID" = $subscriptionId
        "AZURE_RESOURCE_GROUP" = $ResourceGroup
        "API_APP_NAME" = $apiAppName
        "MCP_APP_NAME" = $mcpAppName
        "INSTANCE_SUFFIX" = $instanceSuffix
    }
    
    foreach ($secretName in $secrets.Keys) {
        $secretValue = $secrets[$secretName]
        Write-Host "  Setting secret: $secretName" -ForegroundColor Gray
        echo $secretValue | gh secret set $secretName --repo "$repoOwner/$repoName"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Failed to set GitHub secret: $secretName"
            exit 1
        }
    }
    
    Write-Host "✅ GitHub secrets configured" -ForegroundColor Green
    
    # Set up fork relationship if requested
    if ($SetupFork) {
        Write-Host "🍴 Setting up fork relationship..." -ForegroundColor Yellow
        
        # Add upstream remote if it doesn't exist
        $remotes = git remote -v 2>$null
        if ($remotes -notmatch "upstream") {
            git remote add upstream $UpstreamRepository
            Write-Host "✅ Added upstream remote: $UpstreamRepository" -ForegroundColor Green
        } else {
            Write-Host "ℹ️  Upstream remote already exists" -ForegroundColor Blue
        }
        
        # Fetch from upstream
        git fetch upstream
        Write-Host "✅ Fetched from upstream" -ForegroundColor Green
    }
    
    # Create or update GitHub Actions workflows
    Write-Host "⚙️  Setting up GitHub Actions workflows..." -ForegroundColor Yellow
    
    # Ensure .github/workflows directory exists
    $workflowDir = ".github/workflows"
    if (-not (Test-Path $workflowDir)) {
        New-Item -Path $workflowDir -ItemType Directory -Force | Out-Null
        Write-Host "📁 Created workflows directory" -ForegroundColor Gray
    }
    
    # Create instance-specific deployment workflow
    $workflowContent = @"
name: Deploy to Azure ($instanceSuffix)

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_RESOURCE_GROUP: $ResourceGroup
  API_APP_NAME: $apiAppName
  MCP_APP_NAME: $mcpAppName
  INSTANCE_SUFFIX: $instanceSuffix

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal

  deploy-api:
    if: github.ref == 'refs/heads/main'
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: production
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Build API
      run: dotnet publish FabrikamApi/src/FabrikamApi.csproj -c Release -o ./api-publish
    
    - name: Login to Azure
      uses: azure/login@v1
      with:
        creds: `${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: `${{ env.API_APP_NAME }}
        package: ./api-publish

  deploy-mcp:
    if: github.ref == 'refs/heads/main'
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: production
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Build MCP
      run: dotnet publish FabrikamMcp/src/FabrikamMcp.csproj -c Release -o ./mcp-publish
    
    - name: Login to Azure
      uses: azure/login@v1
      with:
        creds: `${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: `${{ env.MCP_APP_NAME }}
        package: ./mcp-publish
"@
    
    $workflowFile = "$workflowDir/deploy-instance-$instanceSuffix.yml"
    $workflowContent | Out-File -FilePath $workflowFile -Encoding UTF8
    Write-Host "✅ Created workflow: $workflowFile" -ForegroundColor Green
    
    # Create sync-from-upstream workflow if setting up fork
    if ($SetupFork) {
        $syncWorkflowContent = @"
name: Sync from Upstream

on:
  schedule:
    # Run daily at 2 AM UTC
    - cron: '0 2 * * *'
  workflow_dispatch:

jobs:
  sync:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      with:
        token: `${{ secrets.GITHUB_TOKEN }}
        fetch-depth: 0
    
    - name: Configure Git
      run: |
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
    
    - name: Add upstream remote
      run: git remote add upstream $UpstreamRepository || true
    
    - name: Fetch upstream
      run: git fetch upstream
    
    - name: Sync main branch
      run: |
        git checkout main
        git merge upstream/main --no-edit || echo "No changes to merge"
        git push origin main
    
    - name: Create PR for develop branch if changes exist
      run: |
        git checkout develop || git checkout -b develop
        if git merge upstream/main --no-edit; then
          git push origin develop
          gh pr create --title "Sync from upstream" --body "Automated sync from upstream repository" --base main --head develop || echo "PR already exists or no changes"
        fi
      env:
        GITHUB_TOKEN: `${{ secrets.GITHUB_TOKEN }}
"@
        
        $syncWorkflowFile = "$workflowDir/sync-upstream.yml"
        $syncWorkflowContent | Out-File -FilePath $syncWorkflowFile -Encoding UTF8
        Write-Host "✅ Created upstream sync workflow: $syncWorkflowFile" -ForegroundColor Green
    }
    
    Write-Host "`n🎉 GitHub Actions CI/CD Setup Complete!" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    
    Write-Host "`n📋 Summary:" -ForegroundColor Cyan
    Write-Host "  ✅ Service principal created for deployment" -ForegroundColor White
    Write-Host "  ✅ GitHub repository secrets configured" -ForegroundColor White
    Write-Host "  ✅ GitHub Actions workflows created" -ForegroundColor White
    if ($SetupFork) {
        Write-Host "  ✅ Fork relationship configured with upstream" -ForegroundColor White
    }
    
    Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Commit and push the new workflow files" -ForegroundColor White
    Write-Host "  2. Push to main branch to trigger deployment" -ForegroundColor White
    Write-Host "  3. Monitor deployment in GitHub Actions tab" -ForegroundColor White
    if ($SetupFork) {
        Write-Host "  4. Upstream sync will run daily automatically" -ForegroundColor White
    }
    
    Write-Host "`n📊 Repository URLs:" -ForegroundColor Cyan
    Write-Host "  Your Repository: $GitHubRepository" -ForegroundColor White
    Write-Host "  Actions: $GitHubRepository/actions" -ForegroundColor White
    if ($SetupFork) {
        Write-Host "  Upstream: $UpstreamRepository" -ForegroundColor White
    }
    
} catch {
    Write-Error "❌ Error setting up CI/CD: $($_.Exception.Message)"
    exit 1
}
