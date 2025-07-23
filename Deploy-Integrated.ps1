# Deploy FabrikamApi and FabrikamMcp with Integration
# This script deploys both services and configures them to work together

param(
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentName = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus"
)

Write-Host "🚀 Starting deployment of FabrikamApi and FabrikamMcp" -ForegroundColor Green
Write-Host "Environment: $EnvironmentName" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan

# Check prerequisites
Write-Host "`n📋 Checking prerequisites..." -ForegroundColor Yellow
if (-not (Get-Command azd -ErrorAction SilentlyContinue)) {
    Write-Error "Azure Developer CLI (azd) is not installed. Please install it from https://aka.ms/azd"
    exit 1
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
}

Write-Host "✅ Prerequisites check passed" -ForegroundColor Green

# Deploy FabrikamApi first
Write-Host "`n📦 Step 1: Deploying FabrikamApi..." -ForegroundColor Yellow
Push-Location "FabrikamApi"

try {
    # Check if azd is initialized
    if (-not (Test-Path ".azure")) {
        Write-Host "Initializing azd environment..." -ForegroundColor Cyan
        azd env new $EnvironmentName
    }

    # Deploy the API
    Write-Host "Deploying FabrikamApi to Azure..." -ForegroundColor Cyan
    azd up --environment $EnvironmentName

    if ($LASTEXITCODE -ne 0) {
        throw "FabrikamApi deployment failed"
    }

    # Get the API URL
    Write-Host "Getting API URL..." -ForegroundColor Cyan
    $apiOutput = azd show --output json | ConvertFrom-Json
    $apiUrl = $apiOutput.outputs.API_URI.value
    
    Write-Host "✅ FabrikamApi deployed successfully" -ForegroundColor Green
    Write-Host "API URL: $apiUrl" -ForegroundColor Cyan
}
catch {
    Write-Error "Failed to deploy FabrikamApi: $_"
    Pop-Location
    exit 1
}
finally {
    Pop-Location
}

# Deploy FabrikamMcp with API URL
Write-Host "`n📦 Step 2: Deploying FabrikamMcp..." -ForegroundColor Yellow
Push-Location "FabrikamMcp"

try {
    # Check if azd is initialized
    if (-not (Test-Path ".azure")) {
        Write-Host "Initializing azd environment..." -ForegroundColor Cyan
        azd env new $EnvironmentName
    }

    # Set the API URL environment variable
    Write-Host "Configuring MCP to connect to API..." -ForegroundColor Cyan
    azd env set FABRIKAM_API_BASE_URL $apiUrl

    # Deploy the MCP
    Write-Host "Deploying FabrikamMcp to Azure..." -ForegroundColor Cyan
    azd up --environment $EnvironmentName

    if ($LASTEXITCODE -ne 0) {
        throw "FabrikamMcp deployment failed"
    }

    # Get the MCP URL
    $mcpOutput = azd show --output json | ConvertFrom-Json
    $mcpUrl = $mcpOutput.outputs.WEBSITE_URL.value
    
    Write-Host "✅ FabrikamMcp deployed successfully" -ForegroundColor Green
    Write-Host "MCP URL: $mcpUrl" -ForegroundColor Cyan
}
catch {
    Write-Error "Failed to deploy FabrikamMcp: $_"
    Pop-Location
    exit 1
}
finally {
    Pop-Location
}

# Summary
Write-Host "`n🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "═══════════════════════════════════" -ForegroundColor Green
Write-Host "FabrikamApi URL:  $apiUrl" -ForegroundColor White
Write-Host "FabrikamMcp URL:  $mcpUrl" -ForegroundColor White
Write-Host "═══════════════════════════════════" -ForegroundColor Green

Write-Host "`n🔗 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test the API: curl $apiUrl/status" -ForegroundColor White
Write-Host "2. Test the MCP: curl $mcpUrl/status" -ForegroundColor White
Write-Host "3. Connect to MCP in VS Code using: $mcpUrl" -ForegroundColor White
Write-Host "4. Use MCP Inspector: npx @modelcontextprotocol/inspector" -ForegroundColor White

Write-Host "`nFor detailed integration information, see MCP-API-INTEGRATION.md" -ForegroundColor Cyan
