# Test-Shared.ps1 - Enhanced Authentication-Aware Testing Utilities
# Supports Disabled, BearerToken, and EntraExternalId authentication modes

# Enhanced test configuration with authentication detection
$script:TestConfig = @{
    ApiBaseUrl = "https://localhost:7297"
    McpBaseUrl = "https://localhost:5001"
    TimeoutSeconds = 30
    RetryAttempts = 3
    AuthenticationMode = "Unknown"
    AuthenticationEndpoints = @{}
    IsEnabled = $false
    RequiresToken = $false
    TestToken = $null
    TestUserId = $null  # For Disabled mode GUID-based identification
    TestCredentials = @{
        Email = "test@fabrikam.com"
        Password = "Test123!"
        FirstName = "Test"
        LastName = "User"
    }
}

function Initialize-TestEnvironment {
    param(
        [string]$ApiBaseUrl = "https://localhost:7297",
        [string]$McpBaseUrl = "https://localhost:5001",
        [int]$TimeoutSeconds = 30
    )
    
    $script:TestConfig.ApiBaseUrl = $ApiBaseUrl
    $script:TestConfig.McpBaseUrl = $McpBaseUrl
    $script:TestConfig.TimeoutSeconds = $TimeoutSeconds
    
    Write-Host "🔧 Initializing test environment..." -ForegroundColor Cyan
    Write-Host "   API URL: $($script:TestConfig.ApiBaseUrl)" -ForegroundColor Gray
    Write-Host "   MCP URL: $($script:TestConfig.McpBaseUrl)" -ForegroundColor Gray
    
    # Detect authentication mode
    Detect-AuthenticationMode
    
    Write-Host "   Auth Mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Gray
    Write-Host ""
}

function Detect-AuthenticationMode {
    try {
        Write-Host "🔍 Detecting authentication mode..." -ForegroundColor Yellow
        
        $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)/api/info/auth" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
        
        $script:TestConfig.AuthenticationMode = $response.mode
        $script:TestConfig.AuthenticationEndpoints = $response.endpoints
        $script:TestConfig.IsEnabled = $response.isEnabled
        $script:TestConfig.RequiresToken = $response.requiresToken
        
        Write-Host "✅ Authentication mode detected: $($response.mode)" -ForegroundColor Green
        Write-Host "   Description: $($response.description)" -ForegroundColor Gray
        
        switch ($response.mode) {
            "Disabled" {
                Write-Host "   🔓 Authentication is DISABLED - Using GUID-based identification" -ForegroundColor Yellow
                Write-Host "   📝 MCP calls will require userId GUID parameter" -ForegroundColor Gray
            }
            "BearerToken" {
                Write-Host "   🔐 Bearer Token Authentication is ENABLED" -ForegroundColor Yellow
                if ($response.endpoints) {
                    Write-Host "   📋 Available endpoints:" -ForegroundColor Gray
                    $response.endpoints.PSObject.Properties | ForEach-Object {
                        Write-Host "      $($_.Name): $($_.Value)" -ForegroundColor Gray
                    }
                }
            }
            "JwtTokens" {
                Write-Host "   🔐 JWT Authentication is ENABLED" -ForegroundColor Yellow
                if ($response.endpoints) {
                    Write-Host "   📋 Available endpoints:" -ForegroundColor Gray
                    $response.endpoints.PSObject.Properties | ForEach-Object {
                        Write-Host "      $($_.Name): $($_.Value)" -ForegroundColor Gray
                    }
                }
            }
            "EntraExternalId" {
                Write-Host "   🌐 Entra External ID Authentication is ENABLED" -ForegroundColor Yellow
                Write-Host "   ⚠️  Note: Entra External ID testing not yet implemented" -ForegroundColor Yellow
            }
            default {
                Write-Host "   ❓ Unknown authentication mode: $($response.mode)" -ForegroundColor Yellow
            }
        }
        
        return $true
    }
    catch {
        Write-Host "⚠️  Could not detect authentication mode: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   Assuming BearerToken mode for compatibility" -ForegroundColor Gray
        $script:TestConfig.AuthenticationMode = "BearerToken"
        $script:TestConfig.IsEnabled = $true
        $script:TestConfig.RequiresToken = $true
        return $false
    }
}

function Test-ApiConnection {
    param([string]$Url = $script:TestConfig.ApiBaseUrl)
    
    try {
        $response = Invoke-RestMethod -Uri "$Url/api/info" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
        Write-Host "✅ API connection successful" -ForegroundColor Green
        Write-Host "   Name: $($response.Name)" -ForegroundColor Gray
        Write-Host "   Version: $($response.Version)" -ForegroundColor Gray
        Write-Host "   Environment: $($response.Environment)" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Host "❌ API connection failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-McpConnection {
    param([string]$Url = $script:TestConfig.McpBaseUrl)
    
    try {
        # Test MCP status endpoint (MCP servers use /status not /health)
        $response = Invoke-RestMethod -Uri "$Url/status" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
        Write-Host "✅ MCP connection successful" -ForegroundColor Green
        Write-Host "   Service: $($response.service)" -ForegroundColor Gray
        Write-Host "   Version: $($response.version)" -ForegroundColor Gray
        Write-Host "   Status: $($response.status)" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Host "❌ MCP connection failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Get-AuthenticationToken {
    if ($script:TestConfig.AuthenticationMode -eq "Disabled") {
        Write-Host "🔓 Authentication disabled - no token needed" -ForegroundColor Yellow
        return $null
    }
    
    if ($script:TestConfig.AuthenticationMode -eq "EntraExternalId") {
        Write-Host "⚠️  EntraExternalId authentication not yet implemented in tests" -ForegroundColor Yellow
        return $null
    }
    
    if ($script:TestConfig.AuthenticationMode -eq "BearerToken") {
        return Get-JwtToken
    }
    
    if ($script:TestConfig.AuthenticationMode -eq "JwtTokens") {
        return Get-JwtToken
    }
    
    Write-Host "❌ Unknown authentication mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Red
    return $null
}

function Get-JwtToken {
    try {
        Write-Host "🔑 Getting JWT token..." -ForegroundColor Cyan
        
        # Try to get demo credentials first
        try {
            $demoResponse = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)/api/auth/demo-credentials" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            if ($demoResponse.demoUsers -and $demoResponse.demoUsers.Count -gt 0) {
                $firstUser = $demoResponse.demoUsers[0]
                $script:TestConfig.TestCredentials.Email = $firstUser.email
                $script:TestConfig.TestCredentials.Password = $firstUser.password
                Write-Host "✅ Using demo credentials: $($firstUser.email)" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "ℹ️  Demo credentials not available, using default test credentials" -ForegroundColor Gray
        }
        
        # Login with credentials
        $loginRequest = @{
            email = $script:TestConfig.TestCredentials.Email
            password = $script:TestConfig.TestCredentials.Password
        }
        
        $loginResponse = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)/api/auth/login" -Method Post -Body ($loginRequest | ConvertTo-Json) -ContentType "application/json" -TimeoutSec $script:TestConfig.TimeoutSeconds
        
        if ($loginResponse.accessToken) {
            $script:TestConfig.TestToken = $loginResponse.accessToken
            Write-Host "✅ JWT token obtained successfully" -ForegroundColor Green
            return $loginResponse.accessToken
        } else {
            throw "No token in login response"
        }
    }
    catch {
        Write-Host "❌ Failed to get JWT token: $($_.Exception.Message)" -ForegroundColor Red
        
        # Try to register user if login failed
        try {
            Write-Host "🔄 Attempting to register test user..." -ForegroundColor Yellow
            
            $registerRequest = @{
                email = $script:TestConfig.TestCredentials.Email
                password = $script:TestConfig.TestCredentials.Password
                firstName = $script:TestConfig.TestCredentials.FirstName
                lastName = $script:TestConfig.TestCredentials.LastName
            }
            
            $registerResponse = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)/api/auth/register" -Method Post -Body ($registerRequest | ConvertTo-Json) -ContentType "application/json" -TimeoutSec $script:TestConfig.TimeoutSeconds
            
            if ($registerResponse.accessToken) {
                $script:TestConfig.TestToken = $registerResponse.accessToken
                Write-Host "✅ User registered and token obtained" -ForegroundColor Green
                return $registerResponse.accessToken
            }
        }
        catch {
            Write-Host "❌ Registration also failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        return $null
    }
}

function Invoke-AuthenticatedRequest {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    if ($script:TestConfig.AuthenticationMode -ne "Disabled") {
        if (-not $script:TestConfig.TestToken) {
            $token = Get-AuthenticationToken
            if (-not $token) {
                throw "Could not obtain authentication token"
            }
        }
        
        $Headers["Authorization"] = "Bearer $($script:TestConfig.TestToken)"
    }
    
    $requestParams = @{
        Uri = $Uri
        Method = $Method
        Headers = $Headers
        TimeoutSec = $script:TestConfig.TimeoutSeconds
    }
    
    if ($Body) {
        $requestParams.Body = ($Body | ConvertTo-Json)
        $requestParams.ContentType = "application/json"
    }
    
    return Invoke-RestMethod @requestParams
}

function Test-EndpointWithAuth {
    param(
        [string]$Endpoint,
        [string]$Description,
        [string]$Method = "GET",
        [object]$Body = $null,
        [switch]$ExpectSuccess = $true
    )
    
    try {
        Write-Host "🧪 Testing: $Description" -ForegroundColor Cyan
        
        $response = Invoke-AuthenticatedRequest -Uri "$($script:TestConfig.ApiBaseUrl)$Endpoint" -Method $Method -Body $Body
        
        if ($ExpectSuccess) {
            Write-Host "✅ $Description - Success" -ForegroundColor Green
            return @{ Success = $true; Response = $response }
        } else {
            Write-Host "⚠️  $Description - Unexpected success" -ForegroundColor Yellow
            return @{ Success = $false; Response = $response }
        }
    }
    catch {
        if ($ExpectSuccess) {
            Write-Host "❌ $Description - Failed: $($_.Exception.Message)" -ForegroundColor Red
            return @{ Success = $false; Error = $_.Exception.Message }
        } else {
            Write-Host "✅ $Description - Expected failure" -ForegroundColor Green
            return @{ Success = $true; Error = $_.Exception.Message }
        }
    }
}

function Write-TestSection {
    param([string]$Title)
    
    Write-Host ""
    Write-Host "=" * 60 -ForegroundColor Magenta
    Write-Host " $Title" -ForegroundColor Magenta
    Write-Host "=" * 60 -ForegroundColor Magenta
    Write-Host ""
}

function Write-TestSubsection {
    param([string]$Title)
    
    Write-Host ""
    Write-Host "📋 $Title" -ForegroundColor Yellow
    Write-Host "-" * 40 -ForegroundColor Gray
}

function Get-TestUserId {
    # Generate or return a consistent test user GUID for Disabled authentication mode
    if (-not $script:TestConfig.TestUserId) {
        $script:TestConfig.TestUserId = [Guid]::NewGuid().ToString()
        Write-Host "🆔 Generated test user ID: $($script:TestConfig.TestUserId)" -ForegroundColor Cyan
    }
    return $script:TestConfig.TestUserId
}

function Get-AuthenticationHeaders {
    # Return appropriate headers based on authentication mode
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            # No authentication headers needed for Disabled mode
            return @{}
        }
        "BearerToken" {
            if ($script:TestConfig.TestToken) {
                return @{ "Authorization" = "Bearer $($script:TestConfig.TestToken)" }
            } else {
                Write-Host "⚠️  Bearer Token mode but no token available" -ForegroundColor Yellow
                return @{}
            }
        }
        "JwtTokens" {
            if ($script:TestConfig.TestToken) {
                return @{ "Authorization" = "Bearer $($script:TestConfig.TestToken)" }
            } else {
                Write-Host "⚠️  JWT mode but no token available" -ForegroundColor Yellow
                return @{}
            }
        }
        "EntraExternalId" {
            Write-Host "⚠️  Entra External ID authentication not yet implemented" -ForegroundColor Yellow
            return @{}
        }
        default {
            return @{}
        }
    }
}

function Get-McpParameters {
    # Return appropriate parameters for MCP calls based on authentication mode
    param([hashtable]$BaseParameters = @{})
    
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            # Add userId parameter for Disabled mode
            $BaseParameters["userId"] = Get-TestUserId
            return $BaseParameters
        }
        "BearerToken" {
            # Bearer Token mode uses token in headers, no additional parameters needed
            return $BaseParameters
        }
        "JwtTokens" {
            # JWT mode uses token in headers, no additional parameters needed
            return $BaseParameters
        }
        "EntraExternalId" {
            # Entra External ID implementation TBD
            return $BaseParameters
        }
        default {
            return $BaseParameters
        }
    }
}

function Initialize-AuthenticationForMode {
    # Initialize authentication based on detected mode
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            Write-Host "🔓 Initializing for Disabled authentication mode..." -ForegroundColor Yellow
            $testUserId = Get-TestUserId
            Write-Host "   Test User ID: $testUserId" -ForegroundColor Gray
            return $true
        }
        "BearerToken" {
            Write-Host "🔐 Initializing for Bearer Token authentication mode..." -ForegroundColor Yellow
            return (Get-JwtToken) -ne $null
        }
        "EntraExternalId" {
            Write-Host "🌐 Entra External ID mode detected but not yet implemented" -ForegroundColor Yellow
            return $false
        }
        default {
            Write-Host "❓ Unknown authentication mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Red
            return $false
        }
    }
}

function Test-AuthenticationMode {
    # Test the current authentication mode setup
    Write-TestSection "Authentication Mode Verification"
    
    Write-Host "🔍 Current Authentication Configuration:" -ForegroundColor Cyan
    Write-Host "   Mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor White
    Write-Host "   Enabled: $($script:TestConfig.IsEnabled)" -ForegroundColor White
    Write-Host "   Requires Token: $($script:TestConfig.RequiresToken)" -ForegroundColor White
    
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            Write-Host "   Test User ID: $(Get-TestUserId)" -ForegroundColor White
            Write-Host ""
            Write-Host "✅ Disabled mode: API should accept requests without authentication" -ForegroundColor Green
            Write-Host "📝 MCP calls should include userId parameter: $(Get-TestUserId)" -ForegroundColor Gray
        }
        "BearerToken" {
            if ($script:TestConfig.TestToken) {
                Write-Host "   Token: Available (length: $($script:TestConfig.TestToken.Length))" -ForegroundColor White
                Write-Host ""
                Write-Host "✅ Bearer Token mode: Authentication token ready" -ForegroundColor Green
            } else {
                Write-Host "   Token: Not available" -ForegroundColor Red
                Write-Host ""
                Write-Host "❌ Bearer Token mode: No authentication token" -ForegroundColor Red
            }
        }
        "JwtTokens" {
            if ($script:TestConfig.TestToken) {
                Write-Host "   Token: Available (length: $($script:TestConfig.TestToken.Length))" -ForegroundColor White
                Write-Host ""
                Write-Host "✅ JWT mode: Authentication token ready" -ForegroundColor Green
            } else {
                Write-Host "   Token: Not available" -ForegroundColor Red
                Write-Host ""
                Write-Host "❌ JWT mode: No authentication token" -ForegroundColor Red
            }
        }
        "EntraExternalId" {
            Write-Host ""
            Write-Host "⚠️  Entra External ID mode: Not yet implemented" -ForegroundColor Yellow
        }
        default {
            Write-Host ""
            Write-Host "❌ Unknown authentication mode" -ForegroundColor Red
        }
    }
    
    Write-Host ""
}
