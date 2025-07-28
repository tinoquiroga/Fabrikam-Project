# Fabrikam Testing - Shared Utilities
# Common functions and utilities used across all test modules

# Global test results tracking
$Global:TestResults = @{
    ApiTests         = @()
    AuthTests        = @()
    McpTests         = @()
    IntegrationTests = @()
    ProductionTests  = @()
}

function Initialize-TestResults {
    param([switch]$Verbose)
    
    $Global:TestResults = @{
        ApiTests         = @()
        AuthTests        = @()
        McpTests         = @()
        IntegrationTests = @()
        ProductionTests  = @()
    }
    $Global:VerboseLogging = $Verbose
}

function Get-TestResults {
    return $Global:TestResults
}

# Common configuration and settings
function Get-TestConfiguration {
    param(
        [switch]$Production
    )
    
    if ($Production) {
        # Production configuration
        $apiUrl = if ($env:API_DOMAIN) { "https://$env:API_DOMAIN" } else { "https://fabrikam-api-dev-y32g.azurewebsites.net" }
        $mcpUrl = if ($env:MCP_DOMAIN) { "https://$env:MCP_DOMAIN" } else { "https://fabrikam-mcp-dev-y32g.azurewebsites.net" }
        
        return @{
            ApiBaseUrl  = $apiUrl
            McpBaseUrl  = $mcpUrl
            Environment = "Production"
        }
    }
    else {
        # Local development configuration
        return @{
            ApiBaseUrl  = "https://localhost:7297"
            McpBaseUrl  = "https://localhost:5001"
            Environment = "Development"
        }
    }
}

# Test result management
function Add-TestResult {
    param(
        [string]$Category,
        [string]$TestName,
        [bool]$Success,
        [string]$Message,
        [object]$Data = $null
    )
    
    $result = @{
        TestName  = $TestName
        Success   = $Success
        Message   = $Message
        Data      = $Data
        Timestamp = Get-Date
    }
    
    $Global:TestResults[$Category] += $result
    
    # Display result immediately
    if ($Success) {
        Write-Host "✅ $TestName" -ForegroundColor Green
        if ($Global:VerboseLogging) {
            Write-Host "   $Message" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "❌ $TestName" -ForegroundColor Red
        Write-Host "   $Message" -ForegroundColor Yellow
    }
}

# Logging utilities
function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️ $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-SectionHeader {
    param(
        [string]$Title,
        [int]$Width = 30
    )
    Write-Host "`n$Title" -ForegroundColor Cyan
    Write-Host ("=" * $Width) -ForegroundColor Cyan
}

# Common API request wrapper with error handling
function Invoke-SafeWebRequest {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [int]$TimeoutSec = 30
    )
    
    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            TimeoutSec = $TimeoutSec
            SkipCertificateCheck = $true  # For self-signed dev certificates
            ErrorAction = 'Stop'
        }
        
        if ($Headers.Count -gt 0) {
            $params.Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        
        return @{
            Success = $true
            Data = $response
            StatusCode = 200  # RestMethod only returns on success
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        
        return @{
            Success = $false
            Data = $null
            StatusCode = $statusCode
            Error = $_.Exception.Message
        }
    }
}

# Demo credentials retrieval for testing
function Get-DemoCredentials {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSec = 30
    )
    
    try {
        $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/auth/demo-credentials" -Method Get -TimeoutSec $TimeoutSec
        
        if ($result.Success -and $result.Data.demoUsers) {
            # Parse users into easier to use format
            $users = @{}
            foreach ($user in $result.Data.demoUsers) {
                $users[$user.role] = @{
                    Name = $user.name
                    Email = $user.email
                    Password = $user.password
                    Role = $user.role
                    Description = $user.description
                }
            }
            
            return @{
                Success = $true
                Users = $users
                InstanceId = $result.Data.instanceId
                ApiUrl = $result.Data.apiUrl
                Note = $result.Data.note
            }
        }
        else {
            return @{
                Success = $false
                Error = "Demo credentials endpoint responded but no users found"
            }
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

# JWT token retrieval utility with dynamic credentials
function Get-JwtToken {
    param(
        [string]$ApiBaseUrl,
        [string]$Username,  # Email address
        [string]$Password,
        [int]$TimeoutSec = 30
    )
    
    # If no credentials provided, default to Admin role
    if (-not $Username -or -not $Password) {
        return Get-AuthenticatedToken -ApiBaseUrl $ApiBaseUrl -Role "Admin" -TimeoutSec $TimeoutSec
    }
    
    try {
        $loginData = @{
            Email = $Username  # API expects 'Email' field, not 'Username'
            Password = $Password
        } | ConvertTo-Json
        
        $headers = @{ 'Content-Type' = 'application/json' }
        
        $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/auth/login" -Method Post -Body $loginData -Headers $headers -TimeoutSec $TimeoutSec
        
        if ($result.Success -and $result.Data.accessToken) {
            return @{
                Success = $true
                Token = $result.Data.accessToken
                User = $result.Data.user
            }
        }
        else {
            return @{
                Success = $false
                Error = $result.Error ?? "Login succeeded but no token returned"
            }
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

# Authentication utilities with dynamic credential retrieval
function Get-AuthenticatedToken {
    param(
        [string]$ApiBaseUrl,
        [string]$Role = "Admin",  # Admin, Read-Write, Read-Only
        [int]$TimeoutSec = 30
    )
    
    try {
        # Get current demo credentials
        $credResult = Get-DemoCredentials -ApiBaseUrl $ApiBaseUrl -TimeoutSec $TimeoutSec
        
        if (-not $credResult.Success) {
            return @{ Success = $false; Error = "Failed to get demo credentials: $($credResult.Error)" }
        }
        
        $user = $credResult.Users[$Role]
        if (-not $user) {
            return @{ Success = $false; Error = "No user found for role: $Role" }
        }
        
        # Login with the dynamic credentials
        $loginData = @{
            Email = $user.Email
            Password = $user.Password
        } | ConvertTo-Json
        
        $headers = @{ 'Content-Type' = 'application/json' }
        
        $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/auth/login" -Method Post -Body $loginData -Headers $headers -TimeoutSec $TimeoutSec
        
        if ($result.Success -and $result.Data.accessToken) {
            return @{
                Success = $true
                Token = $result.Data.accessToken
                User = $result.Data.user
            }
        }
        else {
            return @{
                Success = $false
                Error = $result.Error ?? "Login succeeded but no token returned"
            }
        }
    }
    catch {
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

# Server management utilities
function Get-ServerProcess {
    param(
        [string]$ServerType  # "API" or "MCP"
    )
    
    switch ($ServerType) {
        "API" {
            return Get-Process -Name "FabrikamApi" -ErrorAction SilentlyContinue
        }
        "MCP" {
            return Get-Process -Name "FabrikamMcp" -ErrorAction SilentlyContinue
        }
    }
}

function Test-ServerHealth {
    param(
        [string]$BaseUrl,
        [string]$ServerName,
        [int]$TimeoutSeconds = 10,
        [string]$HealthEndpoint = "/health"
    )
    
    try {
        # Use custom health endpoint for MCP servers (they use /status)
        if ($ServerName -eq "MCP") {
            $HealthEndpoint = "/status"
        }
        
        $healthUrl = "$BaseUrl$HealthEndpoint"
        Write-Debug "Testing $ServerName server health at: $healthUrl"
        
        $response = Invoke-RestMethod -Uri $healthUrl -Method Get -TimeoutSec $TimeoutSeconds -SkipCertificateCheck
        return @{ Success = $true; Response = $response }
    }
    catch {
        Write-Debug "$ServerName health check failed: $($_.Exception.Message)"
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

# Test summary and reporting
function Show-TestSummary {
    param(
        [string[]]$Categories = @("ApiTests", "AuthTests", "McpTests", "IntegrationTests")
    )
    
    Write-SectionHeader "TEST EXECUTION SUMMARY" 60
    
    $totalPassed = 0
    $totalFailed = 0
    $categoryResults = @{}
    
    foreach ($category in $Categories) {
        $tests = $Global:TestResults[$category]
        $passed = ($tests | Where-Object { $_.Success }).Count
        $failed = ($tests | Where-Object { -not $_.Success }).Count
        
        $categoryResults[$category] = @{
            Passed = $passed
            Failed = $failed
            Tests = $tests
        }
        
        $totalPassed += $passed
        $totalFailed += $failed
    }
    
    # Overall results
    $successRate = if (($totalPassed + $totalFailed) -gt 0) { 
        [math]::Round(($totalPassed / ($totalPassed + $totalFailed)) * 100, 1) 
    } else { 0 }
    
    Write-Host "`nResults:" -ForegroundColor White
    Write-Host "✅ Passed: $totalPassed" -ForegroundColor Green
    Write-Host "❌ Failed: $totalFailed" -ForegroundColor Red
    Write-Host "📊 Success Rate: $successRate%" -ForegroundColor $(if ($successRate -gt 80) { "Green" } elseif ($successRate -gt 60) { "Yellow" } else { "Red" })
    
    # Category breakdown
    foreach ($category in $Categories) {
        $results = $categoryResults[$category]
        if ($results.Passed -gt 0 -or $results.Failed -gt 0) {
            Write-Host "`n$($category.Replace('Tests', ' Tests')):" -ForegroundColor Cyan
            
            foreach ($test in $results.Tests) {
                $icon = if ($test.Success) { "✅" } else { "❌" }
                Write-Host "  $icon $($test.TestName)" -ForegroundColor $(if ($test.Success) { "Green" } else { "Red" })
            }
        }
    }
    
    return @{
        TotalPassed = $totalPassed
        TotalFailed = $totalFailed
        SuccessRate = $successRate
        Categories = $categoryResults
    }
}

# Server startup utilities
function Wait-ForServerStartup {
    param($Url, $ServerName, $MaxWaitSeconds = 30)
    
    Write-Info "⏳ Waiting for $ServerName to start..."
    
    $elapsed = 0
    while ($elapsed -lt $MaxWaitSeconds) {
        try {
            # Use GET instead of HEAD for better compatibility
            # MCP status endpoint and API info endpoint both work with GET
            $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "✅ $ServerName is ready"
                Write-Debug "$ServerName responded successfully to $Url"
                return $true
            }
        }
        catch {
            # Expected during startup - log for debugging
            Write-Debug "$ServerName startup attempt failed: $($_.Exception.Message)"
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Debug "$ServerName startup check: $elapsed/$MaxWaitSeconds seconds"
    }
    
    Write-Warning "⚠️ $ServerName did not respond within $MaxWaitSeconds seconds"
    Write-Warning "⚠️ $ServerName may not be fully ready"
    return $false
}

function Start-TestServers {
    param(
        [string]$ApiProject = "FabrikamApi\src\FabrikamApi.csproj",
        [string]$McpProject = "FabrikamMcp\src\FabrikamMcp.csproj",
        [switch]$ApiOnly,
        [switch]$McpOnly,
        [switch]$Visible
    )
    
    $jobs = @()
    
    if (-not $McpOnly) {
        Write-Info "🌐 Starting API Server..."
        if ($Visible) {
            Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", "dotnet run --project $ApiProject --launch-profile https"
        }
        else {
            $apiJob = Start-Job -ScriptBlock {
                param($project)
                Set-Location $using:PWD
                dotnet run --project $project --launch-profile https
            } -ArgumentList $ApiProject
            $jobs += @{ Type = "API"; Job = $apiJob }
        }
    }
    
    if (-not $ApiOnly) {
        Write-Info "🤖 Starting MCP Server..."
        if ($Visible) {
            Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", "dotnet run --project $McpProject --launch-profile https"
        }
        else {
            $mcpJob = Start-Job -ScriptBlock {
                param($project)
                Set-Location $using:PWD
                dotnet run --project $project --launch-profile https
            } -ArgumentList $McpProject
            $jobs += @{ Type = "MCP"; Job = $mcpJob }
        }
    }
    
    return $jobs
}

function Wait-ForServers {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [switch]$ApiOnly,
        [switch]$McpOnly,
        [int]$MaxWaitSeconds = 60
    )
    
    $startTime = Get-Date
    
    if (-not $McpOnly) {
        Write-Info "⏳ Waiting for API Server to start..."
        do {
            Start-Sleep -Seconds 2
            $apiHealth = Test-ServerHealth -BaseUrl $ApiBaseUrl -ServerName "API" -TimeoutSeconds 5
            if ($apiHealth.Success) {
                Write-Success "✅ API Server is ready"
                break
            }
            
            if ((Get-Date) - $startTime -gt [TimeSpan]::FromSeconds($MaxWaitSeconds)) {
                Write-Error "❌ API Server failed to start within $MaxWaitSeconds seconds"
                return $false
            }
        } while ($true)
    }
    
    if (-not $ApiOnly) {
        Write-Info "⏳ Waiting for MCP Server to start..."
        do {
            Start-Sleep -Seconds 2
            $mcpHealth = Test-ServerHealth -BaseUrl $McpBaseUrl -ServerName "MCP" -TimeoutSeconds 5
            if ($mcpHealth.Success) {
                Write-Success "✅ MCP Server is ready"
                break
            }
            
            if ((Get-Date) - $startTime -gt [TimeSpan]::FromSeconds($MaxWaitSeconds)) {
                Write-Error "❌ MCP Server failed to start within $MaxWaitSeconds seconds"
                return $false
            }
        } while ($true)
    }
    
    return $true
}

function Stop-TestServers {
    param([array]$Jobs)
    
    Write-Info "🛑 Stopping test servers..."
    
    foreach ($jobInfo in $Jobs) {
        if ($jobInfo.Job.State -eq "Running") {
            Write-Info "🛑 Stopping $($jobInfo.Type) Server job..."
            Stop-Job -Job $jobInfo.Job
            Remove-Job -Job $jobInfo.Job -Force
        }
    }
    
    # Also stop any running processes
    $apiProcess = Get-ServerProcess -ServerType "API"
    $mcpProcess = Get-ServerProcess -ServerType "MCP"
    
    if ($apiProcess) {
        Write-Info "🛑 Stopping API Server process..."
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
    
    if ($mcpProcess) {
        Write-Info "🛑 Stopping MCP Server process..."
        Stop-Process -Id $mcpProcess.Id -Force -ErrorAction SilentlyContinue
    }
}

# Note: Functions are automatically available when this script is dot-sourced