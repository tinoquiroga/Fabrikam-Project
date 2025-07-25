# Fabrikam Development Testing Suite
# Comprehensive testing for API endpoints and MCP tools during development
#
# Features:
# - Automatic server management (stop, build, start, test, cleanup)
# - Clean build option for fresh environment
# - Individual component testing (API only, MCP only)
# - Integration testing between API and MCP
# - Production testing against Azure endpoints
# - Version comparison between development and production
# - Comprehensive error reporting and debugging
#
# Usage Examples:
# .\Test-Development.ps1                    # Quick test with existing servers
# .\Test-Development.ps1 -CleanBuild       # Stop servers, clean build, start fresh, test
# .\Test-Development.ps1 -Production       # Test against Azure production endpoints with version check
# .\Test-Development.ps1 -Production -Force # Test production without version check prompts
# .\Test-Development.ps1 -ApiOnly          # Test API endpoints only
# .\Test-Development.ps1 -Quick -Verbose   # Quick test with detailed output

param(
    [switch]$ApiOnly,           # Test API endpoints only
    [switch]$McpOnly,           # Test MCP server only
    [switch]$IntegrationOnly,   # Test integration between API and MCP
    [switch]$Quick,             # Run essential tests only (faster)
    [switch]$Verbose,           # Show detailed output and debugging info
    [switch]$CleanBuild,        # Stop servers, clean build, fresh start
    [switch]$SkipBuild,         # Skip build step, use existing servers
    [switch]$Production,        # Test against Azure production endpoints
    [switch]$Force,             # Skip confirmation prompts
    [int]$TimeoutSeconds = 30   # HTTP request timeout
)

$ErrorActionPreference = "Stop"

# Function to read VS Code settings for URLs
function Test-ProductionVersion {
    param(
        [string]$DevApiUrl,
        [string]$ProdApiUrl
    )
    
    Write-Host "🔍 Checking version differences between development and production..." -ForegroundColor Cyan
    
    try {
        # Get development version
        $devVersionResponse = $null
        $prodVersionResponse = $null
        
        try {
            Write-Debug "Getting development version from: $DevApiUrl/api/info/version"
            $devVersionResponse = Invoke-RestMethod -Uri "$DevApiUrl/api/info/version" -Method Get -TimeoutSec 10
        }
        catch {
            Write-Warning "Could not get development version: $($_.Exception.Message)"
        }
        
        try {
            Write-Debug "Getting production version from: $ProdApiUrl/api/info/version"
            $prodVersionResponse = Invoke-RestMethod -Uri "$ProdApiUrl/api/info/version" -Method Get -TimeoutSec 15
        }
        catch {
            Write-Warning "Could not get production version: $($_.Exception.Message)"
        }
        
        if ($devVersionResponse -and $prodVersionResponse) {
            Write-Host "📊 Version Comparison:" -ForegroundColor Yellow
            Write-Host "   Development - Version: $($devVersionResponse.Version), Build: $($devVersionResponse.BuildTime), Env: $($devVersionResponse.Environment)" -ForegroundColor Green
            Write-Host "   Production  - Version: $($prodVersionResponse.Version), Build: $($prodVersionResponse.BuildTime), Env: $($prodVersionResponse.Environment)" -ForegroundColor Blue
            
            # Compare build times
            if ($devVersionResponse.BuildTime -ne "Unknown" -and $prodVersionResponse.BuildTime -ne "Unknown") {
                try {
                    $devBuildTime = [DateTime]::ParseExact($devVersionResponse.BuildTime, "yyyy-MM-dd HH:mm:ss UTC", $null)
                    $prodBuildTime = [DateTime]::ParseExact($prodVersionResponse.BuildTime, "yyyy-MM-dd HH:mm:ss UTC", $null)
                    
                    if ($devBuildTime -gt $prodBuildTime) {
                        $timeDiff = $devBuildTime - $prodBuildTime
                        Write-Host "⚠️  WARNING: Production appears to be behind development!" -ForegroundColor Red
                        Write-Host "   Development is $($timeDiff.TotalHours.ToString('F1')) hours newer than production" -ForegroundColor Red
                        Write-Host "   Consider deploying the latest changes to production" -ForegroundColor Yellow
                        return $false
                    }
                    elseif ($prodBuildTime -gt $devBuildTime) {
                        $timeDiff = $prodBuildTime - $devBuildTime
                        Write-Host "ℹ️  Production is $($timeDiff.TotalHours.ToString('F1')) hours newer than development" -ForegroundColor Cyan
                        return $true
                    }
                    else {
                        Write-Host "✅ Development and production appear to be in sync" -ForegroundColor Green
                        return $true
                    }
                }
                catch {
                    Write-Warning "Could not parse build times for comparison"
                }
            }
            
            # Fallback to version string comparison
            if ($devVersionResponse.Version -ne $prodVersionResponse.Version) {
                Write-Host "⚠️  WARNING: Version mismatch detected!" -ForegroundColor Red
                Write-Host "   Development: $($devVersionResponse.Version)" -ForegroundColor Red
                Write-Host "   Production:  $($prodVersionResponse.Version)" -ForegroundColor Red
                return $false
            }
            else {
                Write-Host "✅ Versions match: $($devVersionResponse.Version)" -ForegroundColor Green
                return $true
            }
        }
        elseif ($prodVersionResponse) {
            Write-Host "📊 Production Version:" -ForegroundColor Yellow
            Write-Host "   Version: $($prodVersionResponse.Version), Build: $($prodVersionResponse.BuildTime), Env: $($prodVersionResponse.Environment)" -ForegroundColor Blue
            Write-Host "ℹ️  Could not compare with development (server not running)" -ForegroundColor Cyan
            return $true
        }
        else {
            Write-Warning "Could not retrieve version information from either environment"
            return $false
        }
    }
    catch {
        Write-Error "Error during version comparison: $($_.Exception.Message)"
        return $false
    }
}

function Get-EndpointConfiguration {
    param([switch]$Production)
    
    $settingsPath = ".vscode\settings.json"
    $defaultLocal = @{
        ApiBaseUrl  = "http://localhost:7296"
        McpBaseUrl  = "http://localhost:5000"
        Environment = "Local Development"
    }
    
    $defaultAzure = @{
        ApiBaseUrl  = "https://fabrikam-api-dev-izbd.azurewebsites.net"
        McpBaseUrl  = "https://fabrikam-mcp-dev-izbd.azurewebsites.net"
        Environment = "Azure Production"
    }
    
    if ($Production) {
        if (Test-Path $settingsPath) {
            try {
                Write-Debug "Reading production URLs from $settingsPath"
                $settingsContent = Get-Content $settingsPath -Raw
                # Remove JSON comments more carefully
                $lines = $settingsContent -split "`n"
                $cleanLines = @()
                foreach ($line in $lines) {
                    # Remove line comments, but preserve content before them
                    if ($line -match '^([^"]*(?:"[^"]*"[^"]*)*?)//.*$') {
                        $cleanLines += $matches[1].TrimEnd()
                    }
                    else {
                        $cleanLines += $line
                    }
                }
                $cleanContent = $cleanLines -join "`n"
                
                $settings = $cleanContent | ConvertFrom-Json
                
                if ($settings.'rest-client.environmentVariables'.azure) {
                    $azureConfig = $settings.'rest-client.environmentVariables'.azure
                    return @{
                        ApiBaseUrl  = $azureConfig.apiUrl
                        McpBaseUrl  = $azureConfig.mcpUrl
                        Environment = "Azure Production (from settings.json)"
                    }
                }
                else {
                    Write-Warning "Azure configuration not found in settings.json, using defaults"
                    return $defaultAzure
                }
            }
            catch {
                Write-Warning "Error reading settings.json: $($_.Exception.Message), using defaults"
                return $defaultAzure
            }
        }
        else {
            Write-Warning "Settings file not found, using default Azure URLs"
            return $defaultAzure
        }
    }
    else {
        if (Test-Path $settingsPath) {
            try {
                Write-Debug "Reading local URLs from $settingsPath"
                $settingsContent = Get-Content $settingsPath -Raw
                # Remove JSON comments more carefully
                $lines = $settingsContent -split "`n"
                $cleanLines = @()
                foreach ($line in $lines) {
                    # Remove line comments, but preserve content before them
                    if ($line -match '^([^"]*(?:"[^"]*"[^"]*)*?)//.*$') {
                        $cleanLines += $matches[1].TrimEnd()
                    }
                    else {
                        $cleanLines += $line
                    }
                }
                $cleanContent = $cleanLines -join "`n"
                
                $settings = $cleanContent | ConvertFrom-Json
                
                if ($settings.'rest-client.environmentVariables'.local) {
                    $localConfig = $settings.'rest-client.environmentVariables'.local
                    return @{
                        ApiBaseUrl  = $localConfig.apiUrl
                        McpBaseUrl  = $localConfig.mcpUrl
                        Environment = "Local Development (from settings.json)"
                    }
                }
                else {
                    Write-Debug "Local configuration not found in settings.json, using defaults"
                    return $defaultLocal
                }
            }
            catch {
                Write-Debug "Error reading settings.json: $($_.Exception.Message), using defaults"
                return $defaultLocal
            }
        }
        else {
            Write-Debug "Settings file not found, using default local URLs"
            return $defaultLocal
        }
    }
}

# Get configuration based on Production switch
$config = Get-EndpointConfiguration -Production:$Production
$ApiBaseUrl = $config.ApiBaseUrl
$McpBaseUrl = $config.McpBaseUrl
$TestEnvironment = $config.Environment

# Project configuration (only used for local development)
$ApiProject = "FabrikamApi\src\FabrikamApi.csproj"
$McpProject = "FabrikamMcp\src\FabrikamMcp.csproj"
$SolutionFile = "Fabrikam.sln"

# Color functions
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Warning($message) { Write-Host "⚠️ $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️ $message" -ForegroundColor Cyan }
function Write-Debug($message) { if ($Verbose) { Write-Host "🔍 $message" -ForegroundColor Gray } }
function Write-Header($message) { 
    Write-Host "`n🚀 $message" -ForegroundColor Yellow
    Write-Host "=" * ($message.Length + 4) -ForegroundColor Yellow
}

# Test Results Tracking
$TestResults = @{
    ApiTests         = @()
    McpTests         = @()
    IntegrationTests = @()
    TotalPassed      = 0
    TotalFailed      = 0
}

function Add-TestResult {
    param($Category, $Name, $Passed, $Details = "")
    
    $result = @{
        Name      = $Name
        Passed    = $Passed
        Details   = $Details
        Timestamp = Get-Date
    }
    
    $TestResults[$Category] += $result
    
    if ($Passed) {
        $TestResults.TotalPassed++
        Write-Success "$Name"
        if ($Details -and $Verbose) { Write-Debug $Details }
    }
    else {
        $TestResults.TotalFailed++
        Write-Error "$Name"
        if ($Details) { Write-Host "   $Details" -ForegroundColor Gray }
    }
}

function Get-ProcessStatus {
    $apiProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { 
        try {
            $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
            $cmdLine -like "*FabrikamApi*"
        }
        catch { $false }
    }
    
    $mcpProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { 
        try {
            $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
            $cmdLine -like "*FabrikamMcp*"
        }
        catch { $false }
    }
    
    return @{
        ApiRunning = $apiProcess -ne $null
        McpRunning = $mcpProcess -ne $null
        ApiProcess = $apiProcess
        McpProcess = $mcpProcess
    }
}

function Stop-AllServers {
    Write-Header "Stopping All Running Servers"
    
    $status = Get-ProcessStatus
    $stopped = $false
    
    if ($status.ApiRunning) {
        Write-Info "🌐 Stopping API Server (PID: $($status.ApiProcess.Id))"
        try {
            Stop-Process -Id $status.ApiProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Write-Success "✅ API Server stopped"
            $stopped = $true
        }
        catch {
            Write-Warning "⚠️ Error stopping API server: $($_.Exception.Message)"
        }
    }
    
    if ($status.McpRunning) {
        Write-Info "🤖 Stopping MCP Server (PID: $($status.McpProcess.Id))"
        try {
            Stop-Process -Id $status.McpProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Write-Success "✅ MCP Server stopped"
            $stopped = $true
        }
        catch {
            Write-Warning "⚠️ Error stopping MCP server: $($_.Exception.Message)"
        }
    }
    
    if (-not $stopped) {
        Write-Info "ℹ️ No servers were running"
    }
    
    # Wait a bit for ports to be released
    Start-Sleep -Seconds 3
}

function Build-Solution {
    param([switch]$Clean)
    
    if ($Clean) {
        Write-Header "Performing Clean Build"
        Write-Info "🧹 Cleaning solution..."
        
        try {
            dotnet clean $SolutionFile --verbosity quiet
            Write-Success "✅ Solution cleaned"
        }
        catch {
            Write-Error "❌ Clean failed: $($_.Exception.Message)"
            return $false
        }
    }
    else {
        Write-Header "Building Solution"
    }
    
    Write-Info "🔨 Building solution..."
    
    try {
        $buildOutput = dotnet build $SolutionFile --verbosity minimal --nologo 2>&1
        
        # Check if build succeeded
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✅ Solution built successfully"
            if ($Verbose -and $buildOutput) {
                Write-Debug "Build output: $($buildOutput -join "`n")"
            }
            return $true
        }
        else {
            Write-Error "❌ Build failed"
            Write-Host "Build output:" -ForegroundColor Gray
            $buildOutput | Write-Host
            return $false
        }
    }
    catch {
        Write-Error "❌ Build error: $($_.Exception.Message)"
        return $false
    }
}

function Wait-ForServerStartup {
    param($Url, $ServerName, $MaxWaitSeconds = 30)
    
    Write-Info "⏳ Waiting for $ServerName to start..."
    
    $elapsed = 0
    while ($elapsed -lt $MaxWaitSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Head -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "✅ $ServerName is ready"
                return $true
            }
        }
        catch {
            # Expected during startup
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Debug "$ServerName startup check: $elapsed/$MaxWaitSeconds seconds"
    }
    
    Write-Warning "⚠️ $ServerName did not respond within $MaxWaitSeconds seconds"
    return $false
}

function Start-ServersForTesting {
    Write-Header "Starting Servers for Testing"
    
    if (-not $ApiOnly) {
        Write-Info "🤖 Starting MCP Server in background..."
        try {
            $mcpJob = Start-Job -ScriptBlock {
                param($Project, $WorkingDir)
                Set-Location $WorkingDir
                dotnet run --project $Project --launch-profile http --verbosity quiet
            } -ArgumentList $McpProject, $PWD.Path
            
            # Wait for MCP server to be ready
            if (Wait-ForServerStartup "$McpBaseUrl/mcp" "MCP Server" 15) {
                Write-Success "✅ MCP Server started (Job ID: $($mcpJob.Id))"
            }
            else {
                Write-Warning "⚠️ MCP Server may not be fully ready"
            }
        }
        catch {
            Write-Error "❌ Failed to start MCP Server: $($_.Exception.Message)"
        }
    }
    
    if (-not $McpOnly) {
        Write-Info "🌐 Starting API Server in background..."
        try {
            $apiJob = Start-Job -ScriptBlock {
                param($Project, $WorkingDir)
                Set-Location $WorkingDir
                $env:ASPNETCORE_ENVIRONMENT = "Development"
                $env:ASPNETCORE_URLS = "https://localhost:7297;http://localhost:7296"
                dotnet run --project $Project --verbosity quiet
            } -ArgumentList $ApiProject, $PWD.Path
            
            # Wait for API server to be ready
            if (Wait-ForServerStartup "$ApiBaseUrl/api/info" "API Server" 20) {
                Write-Success "✅ API Server started (Job ID: $($apiJob.Id))"
            }
            else {
                Write-Warning "⚠️ API Server may not be fully ready"
            }
        }
        catch {
            Write-Error "❌ Failed to start API Server: $($_.Exception.Message)"
        }
    }
    
    return @{
        ApiJob = if (-not $McpOnly) { $apiJob } else { $null }
        McpJob = if (-not $ApiOnly) { $mcpJob } else { $null }
    }
}

function Stop-TestJobs {
    param($Jobs)
    
    Write-Header "Cleaning Up Test Environment"
    
    if ($Jobs.ApiJob) {
        Write-Info "🌐 Stopping API Server job..."
        Stop-Job -Job $Jobs.ApiJob -ErrorAction SilentlyContinue
        Remove-Job -Job $Jobs.ApiJob -Force -ErrorAction SilentlyContinue
    }
    
    if ($Jobs.McpJob) {
        Write-Info "🤖 Stopping MCP Server job..."
        Stop-Job -Job $Jobs.McpJob -ErrorAction SilentlyContinue
        Remove-Job -Job $Jobs.McpJob -Force -ErrorAction SilentlyContinue
    }
    
    # Also stop any lingering processes
    Stop-AllServers
    Write-Success "✅ Test environment cleaned up"
}

function Test-ApiEndpoint {
    param($Endpoint, $ExpectedStatus = 200, $TestName = "")
    
    if (-not $TestName) { $TestName = "API $Endpoint" }
    
    try {
        Write-Debug "Testing $ApiBaseUrl$Endpoint"
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl$Endpoint" -Method Get -TimeoutSec $TimeoutSeconds
        
        # Basic validation - endpoint responds
        if ($response) {
            Add-TestResult "ApiTests" $TestName $true "Status OK, got response"
            return $response
        }
        else {
            Add-TestResult "ApiTests" $TestName $false "No response data"
            return $null
        }
    }
    catch {
        Add-TestResult "ApiTests" $TestName $false $_.Exception.Message
        return $null
    }
}

function Test-ApiDataStructure {
    param($Data, $RequiredFields, $TestName)
    
    try {
        $missing = @()
        foreach ($field in $RequiredFields) {
            if (-not (Get-Member -InputObject $Data -Name $field -ErrorAction SilentlyContinue)) {
                $missing += $field
            }
        }
        
        if ($missing.Count -eq 0) {
            Add-TestResult "ApiTests" $TestName $true "All required fields present"
        }
        else {
            Add-TestResult "ApiTests" $TestName $false "Missing fields: $($missing -join ', ')"
        }
    }
    catch {
        Add-TestResult "ApiTests" $TestName $false $_.Exception.Message
    }
}

function Test-McpServerHealth {
    if ($Production) {
        # In production mode, test MCP status endpoint instead of the protocol endpoint
        try {
            Write-Debug "Testing production MCP server connectivity..."
            $mcpStatusUrl = "$McpBaseUrl/status"
            
            $response = Invoke-RestMethod -Uri $mcpStatusUrl -Method Get -TimeoutSec $TimeoutSeconds -ErrorAction SilentlyContinue
            if ($response -and $response.status -eq "Ready") {
                Add-TestResult "McpTests" "MCP Server Connectivity" $true "Production MCP server ready - Version: $($response.version), Environment: $($response.environment)"
                Write-Debug "Production MCP server is ready and responding"
            }
            else {
                Add-TestResult "McpTests" "MCP Server Connectivity" $false "Production MCP server status check failed - unexpected response"
            }
        }
        catch {
            Add-TestResult "McpTests" "MCP Server Connectivity" $false "Production MCP server not reachable: $($_.Exception.Message)"
            Write-Debug "Production MCP server connectivity failed: $($_.Exception.Message)"
        }
    }
    else {
        # Local development mode - check for running process
        try {
            Write-Debug "Checking for MCP server process..."
            
            $mcpProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
            Where-Object { 
                try {
                    $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
                    $isMcpProcess = $cmdLine -like "*FabrikamMcp*"
                    if ($Verbose -and $isMcpProcess) {
                        Write-Debug "Found MCP process: $cmdLine"
                    }
                    return $isMcpProcess
                }
                catch {
                    Write-Debug "Error checking process $($_.Id): $($_.Exception.Message)"
                    return $false
                }
            }
            
            if ($mcpProcess) {
                $processDetails = "Process ID: $($mcpProcess.Id)"
                Add-TestResult "McpTests" "MCP Server Process" $true $processDetails
                Write-Debug "MCP server detected: $processDetails"
            }
            else {
                Add-TestResult "McpTests" "MCP Server Process" $false "MCP server process not found. Run 'dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj' to start it."
                Write-Debug "No MCP server process found in running dotnet processes"
            }
        }
        catch {
            Add-TestResult "McpTests" "MCP Server Health" $false $_.Exception.Message
        }
    }
}

function Show-TestSummary {
    Write-Host "`n" + "="*60 -ForegroundColor Cyan
    Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor Cyan
    Write-Host "="*60 -ForegroundColor Cyan
    
    Write-Host "`nResults:" -ForegroundColor White
    Write-Success "Passed: $($TestResults.TotalPassed)"
    if ($TestResults.TotalFailed -gt 0) {
        Write-Error "Failed: $($TestResults.TotalFailed)"
    }
    else {
        Write-Host "Failed: 0" -ForegroundColor Green
    }
    
    $total = $TestResults.TotalPassed + $TestResults.TotalFailed
    if ($total -gt 0) {
        $successRate = [math]::Round(($TestResults.TotalPassed / $total) * 100, 1)
        Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } else { "Yellow" })
    }
    
    if ($TestResults.ApiTests.Count -gt 0) {
        Write-Host "`nAPI Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.ApiTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
    
    if ($TestResults.McpTests.Count -gt 0) {
        Write-Host "`nMCP Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.McpTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
    
    if ($TestResults.IntegrationTests.Count -gt 0) {
        Write-Host "`nIntegration Tests:" -ForegroundColor Yellow
        foreach ($test in $TestResults.IntegrationTests) {
            $status = if ($test.Passed) { "✅" } else { "❌" }
            Write-Host "  $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { "Green" } else { "Red" })
        }
    }
}

# Main Testing Logic
Write-Info "Starting Fabrikam Development Testing Suite"
Write-Info "🌍 Test Environment: $TestEnvironment"
Write-Info "🌐 API Base URL: $ApiBaseUrl"
Write-Info "🤖 MCP Base URL: $McpBaseUrl"

# Environment Preparation
if ($Production) {
    Write-Header "Production Testing Mode"
    Write-Info "🚀 Testing against Azure production endpoints"
    Write-Info "⚠️ Skipping local server management (not applicable for production)"
    
    # Version comparison check
    Write-Host ""
    Write-Info "🔍 Checking if production is up-to-date..."
    $devApiUrl = "http://localhost:7296"  # Local development URL for comparison
    $versionSyncOk = Test-ProductionVersion -DevApiUrl $devApiUrl -ProdApiUrl $ApiBaseUrl
    
    if (-not $versionSyncOk) {
        Write-Host ""
        Write-Warning "🚨 Production may be behind development!"
        if (-not $Force) {
            $continue = Read-Host "Continue with testing anyway? (y/N)"
            if ($continue -notmatch '^[Yy]') {
                Write-Error "❌ Testing cancelled by user"
                exit 1
            }
        }
        else {
            Write-Info "🔄 Continuing due to -Force parameter"
        }
    }
    
    # Test connectivity to production endpoints
    Write-Host ""
    Write-Info "🔍 Verifying production endpoint connectivity..."
    try {
        $apiHealthCheck = Invoke-WebRequest -Uri "$ApiBaseUrl/api/info" -Method Head -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($apiHealthCheck.StatusCode -eq 200) {
            Write-Success "✅ API endpoint is reachable"
        }
        else {
            Write-Warning "⚠️ API endpoint returned status: $($apiHealthCheck.StatusCode)"
        }
    }
    catch {
        Write-Warning "⚠️ API endpoint connectivity check failed: $($_.Exception.Message)"
    }
    
    $testJobs = $null  # No local jobs in production mode
    
}
elseif ($CleanBuild -or (-not $SkipBuild)) {
    Write-Header "Local Development Testing Mode"
    
    # Stop any running servers first
    Stop-AllServers
    
    # Build the solution
    if (-not $SkipBuild) {
        $buildSuccess = Build-Solution -Clean:$CleanBuild
        if (-not $buildSuccess) {
            Write-Error "❌ Build failed. Cannot proceed with testing."
            exit 1
        }
    }
    
    # Start servers for testing
    $testJobs = Start-ServersForTesting
    
    # Give servers a moment to fully initialize
    Write-Info "⏳ Allowing servers to fully initialize..."
    Start-Sleep -Seconds 5
}
else {
    Write-Header "Local Development Testing Mode"
    Write-Info "🔍 Using existing local servers (SkipBuild mode)"
    $testJobs = $null
}

# Wrap testing in try/finally to ensure cleanup
try {

    if (-not $McpOnly) {
        Write-Host "`n🔍 API ENDPOINT TESTS" -ForegroundColor Cyan
        Write-Host "="*30 -ForegroundColor Cyan
    
        # Basic API health
        $ordersResponse = Test-ApiEndpoint "/api/orders" "Orders Endpoint"
        if ($ordersResponse) {
            # Orders returns an array directly, not wrapped in data/pagination
            if ($ordersResponse -is [Array] -and $ordersResponse.Count -gt 0) {
                Add-TestResult "ApiTests" "Orders Response Structure" $true "Returns array with $($ordersResponse.Count) orders"
            }
            else {
                Add-TestResult "ApiTests" "Orders Response Structure" $false "Expected array of orders"
            }
        }
    
        # Analytics endpoint (the one that was failing)
        $analyticsResponse = Test-ApiEndpoint "/api/orders/analytics" "Analytics Endpoint"
        if ($analyticsResponse) {
            Test-ApiDataStructure $analyticsResponse @("summary", "byStatus", "byRegion", "recentTrends") "Analytics Response Structure"
        
            # Detailed analytics validation
            if ($analyticsResponse.summary) {
                Test-ApiDataStructure $analyticsResponse.summary @("totalOrders", "totalRevenue", "averageOrderValue") "Analytics Summary Structure"
            }
        }
    
        # Other endpoints
        if (-not $Quick) {
            Test-ApiEndpoint "/api/customers" "Customers Endpoint"
            Test-ApiEndpoint "/api/products" "Products Endpoint"
            Test-ApiEndpoint "/api/supporttickets" "Support Tickets Endpoint"  # Fixed URL
            Test-ApiEndpoint "/api/info" "Info Endpoint"
        }
    }

    if (-not $ApiOnly) {
        Write-Host "`n🔧 MCP SERVER TESTS" -ForegroundColor Cyan
        Write-Host "="*30 -ForegroundColor Cyan
    
        Test-McpServerHealth
    
        # Additional MCP tests would go here
        # These would test MCP protocol compliance, tool availability, etc.
    }

    if (-not $ApiOnly -and -not $McpOnly) {
        Write-Host "`n🔄 INTEGRATION TESTS" -ForegroundColor Cyan
        Write-Host "="*30 -ForegroundColor Cyan
    
        # Test that MCP can reach API
        try {
            if ($analyticsResponse) {
                Add-TestResult "IntegrationTests" "API-MCP Data Compatibility" $true "Analytics data structure matches MCP expectations"
            }
            else {
                Add-TestResult "IntegrationTests" "API-MCP Data Compatibility" $false "Analytics endpoint not responding"
            }
        }
        catch {
            Add-TestResult "IntegrationTests" "API-MCP Integration" $false $_.Exception.Message
        }
    }

    Show-TestSummary

    Write-Host "`n📋 NEXT STEPS:" -ForegroundColor Cyan
    if ($TestResults.TotalFailed -eq 0) {
        Write-Success "All tests passed! Your development environment is ready."
        Write-Host "• Continue development with confidence" -ForegroundColor Gray
        Write-Host "• Run this script after making changes" -ForegroundColor Gray
    }
    else {
        Write-Warning "Some tests failed. Address these issues:"
        Write-Host "• Check failed test details above" -ForegroundColor Gray
        Write-Host "• Ensure both API and MCP server are running" -ForegroundColor Gray
        Write-Host "• Verify port configurations" -ForegroundColor Gray
    }

    Write-Host "`n💡 Usage Tips:" -ForegroundColor Cyan
    Write-Host "• Quick test: .\Test-Development.ps1 -Quick" -ForegroundColor Gray
    Write-Host "• API only: .\Test-Development.ps1 -ApiOnly" -ForegroundColor Gray
    Write-Host "• Clean build + test: .\Test-Development.ps1 -CleanBuild" -ForegroundColor Gray
    Write-Host "• Skip build (use running servers): .\Test-Development.ps1 -SkipBuild" -ForegroundColor Gray
    Write-Host "• Production testing: .\Test-Development.ps1 -Production" -ForegroundColor Gray
    Write-Host "• Production API only: .\Test-Development.ps1 -Production -ApiOnly" -ForegroundColor Gray
    Write-Host "• Skip version check prompts: .\Test-Development.ps1 -Production -Force" -ForegroundColor Gray
    Write-Host "• Verbose output: .\Test-Development.ps1 -Verbose" -ForegroundColor Gray

}
finally {
    # Cleanup: Stop any jobs we started (only for local development)
    if ($testJobs -and -not $Production) {
        Stop-TestJobs $testJobs
    }
}
