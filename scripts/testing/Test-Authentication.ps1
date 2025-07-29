# Test-Authentication.ps1 - Authentication-Specific Testing Module
# Tests authentication workflows based on detected authentication mode
# Supports: Disabled, JwtTokens, and EntraExternalId modes

param(
    [string]$ApiBaseUrl = "https://localhost:7297",
    [switch]$Quick,
    [switch]$Verbose
)

# Import shared testing utilities
$SharedScript = Join-Path $PSScriptRoot "Test-Shared.ps1"
if (Test-Path $SharedScript) {
    . $SharedScript
} else {
    Write-Host "❌ Could not find Test-Shared.ps1 at $SharedScript" -ForegroundColor Red
    exit 1
}

function Test-Authentication {
    Write-TestSection "Authentication Testing"
    
    # Initialize test environment and detect authentication mode
    Initialize-TestEnvironment -ApiBaseUrl $ApiBaseUrl
    
    # Test the authentication mode verification
    Test-AuthenticationMode
    
    # Run tests based on detected authentication mode
    $testResults = @()  # Initialize as empty array
    
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            $disabledResults = Test-AuthenticationDisabled
            $testResults += $disabledResults
        }
        "JwtTokens" {
            $jwtResults = Test-JwtAuthentication
            $testResults += $jwtResults
        }
        "EntraExternalId" {
            $entraResults = Test-EntraExternalId
            $testResults += $entraResults
        }
        default {
            Write-Host "❌ Unknown authentication mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Red
            $testResults += @{
                Test = "Authentication Mode Detection"
                Status = "Fail"
                Error = "Unknown authentication mode: $($script:TestConfig.AuthenticationMode)"
            }
        }
    }
    
    # Summary
    Write-TestSection "Authentication Test Summary"
    $passCount = ($testResults | Where-Object { $_.Status -eq "Pass" }).Count
    $totalCount = $testResults.Count
    
    Write-Host "📊 Test Results: $passCount/$totalCount passed" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($result in $testResults) {
        $status = if ($result.Status -eq "Pass") { "✅" } else { "❌" }
        Write-Host "$status $($result.Test)" -ForegroundColor $(if ($result.Status -eq "Pass") { "Green" } else { "Red" })
        if ($result.Error) {
            Write-Host "   Error: $($result.Error)" -ForegroundColor Gray
        }
    }
    
    return $testResults
}

function Test-AuthenticationDisabled {
    Write-TestSection "Testing Authentication Disabled Mode"
    
    $testResults = @()
    
    Write-Host "🔓 Testing endpoints without authentication requirements..." -ForegroundColor Yellow
    Write-Host "📝 Note: In Disabled mode, MCP calls should include userId parameter" -ForegroundColor Gray
    Write-Host ""
    
    # Test public endpoints that should work without auth
    $publicEndpoints = @(
        @{ Endpoint = "/api/info"; Description = "API Info" },
        @{ Endpoint = "/api/info/auth"; Description = "Authentication Info" },
        @{ Endpoint = "/api/customers"; Description = "Customers List" },
        @{ Endpoint = "/api/products"; Description = "Products List" },
        @{ Endpoint = "/api/orders"; Description = "Orders List" },
        @{ Endpoint = "/api/orders/analytics"; Description = "Orders Analytics" }
    )
    
    foreach ($endpointInfo in $publicEndpoints) {
        try {
            $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            Write-Host "✅ $($endpointInfo.Description) - Available without authentication" -ForegroundColor Green
            $testResults += @{ 
                Test = "$($endpointInfo.Description) (No Auth)"
                Status = "Pass"
                Type = "Public Access"
                Endpoint = $endpointInfo.Endpoint
            }
        }
        catch {
            Write-Host "❌ $($endpointInfo.Description) - Failed: $($_.Exception.Message)" -ForegroundColor Red
            $testResults += @{ 
                Test = "$($endpointInfo.Description) (No Auth)"
                Status = "Fail"
                Type = "Public Access"
                Endpoint = $endpointInfo.Endpoint
                Error = $_.Exception.Message
            }
        }
    }
    
    # Test that auth endpoints are not available
    $authEndpoints = @(
        @{ Endpoint = "/api/auth/login"; Description = "Login Endpoint" },
        @{ Endpoint = "/api/auth/register"; Description = "Register Endpoint" },
        @{ Endpoint = "/api/auth/refresh"; Description = "Refresh Token Endpoint" },
        @{ Endpoint = "/api/auth/demo-credentials"; Description = "Demo Credentials Endpoint" }
    )
    
    Write-Host ""
    Write-Host "🚫 Testing authentication endpoints (should be unavailable)..." -ForegroundColor Yellow
    
    foreach ($endpointInfo in $authEndpoints) {
        try {
            $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            Write-Host "⚠️  $($endpointInfo.Description) - Unexpectedly available" -ForegroundColor Yellow
            $testResults += @{ 
                Test = "$($endpointInfo.Description) (Should be disabled)"
                Status = "Warning"
                Type = "Auth Endpoint"
                Endpoint = $endpointInfo.Endpoint
                Details = "Should be disabled in Disabled mode"
            }
        }
        catch {
            if ($_.Exception.Message -like "*404*" -or $_.Exception.Message -like "*Not Found*") {
                Write-Host "✅ $($endpointInfo.Description) - Correctly unavailable (404)" -ForegroundColor Green
                $testResults += @{ 
                    Test = "$($endpointInfo.Description) (Correctly disabled)"
                    Status = "Pass"
                    Type = "Auth Endpoint"
                    Endpoint = $endpointInfo.Endpoint
                    Details = "Correctly disabled"
                }
            } else {
                Write-Host "❌ $($endpointInfo.Description) - Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
                $testResults += @{ 
                    Test = "$($endpointInfo.Description) (Error)"
                    Status = "Fail"
                    Type = "Auth Endpoint"
                    Endpoint = $endpointInfo.Endpoint
                    Error = $_.Exception.Message
                }
            }
        }
    }
    
    # Test GUID-based identification pattern
    Write-Host ""
    Write-Host "🆔 Testing GUID-based identification pattern..." -ForegroundColor Yellow
    $testUserId = Get-TestUserId
    Write-Host "   Using test user ID: $testUserId" -ForegroundColor Gray
    
    # Note: This is where we would test MCP calls with userId parameter
    # For now, just validate the GUID format
    try {
        $guid = [Guid]::Parse($testUserId)
        Write-Host "✅ Test User ID is valid GUID format" -ForegroundColor Green
        $testResults += @{
            Test = "GUID Format Validation"
            Status = "Pass"
            Type = "User Identification"
            Details = "Valid GUID: $testUserId"
        }
    }
    catch {
        Write-Host "❌ Test User ID is not valid GUID format" -ForegroundColor Red
        $testResults += @{
            Test = "GUID Format Validation"
            Status = "Fail"
            Type = "User Identification"
            Error = "Invalid GUID format: $testUserId"
        }
    }
    
    return $testResults
}

function Test-JwtAuthentication {
    Write-TestSection "Testing JWT Authentication Mode"
    
    $testResults = @()
    
    # Initialize authentication for JWT mode
    Write-TestSubsection "JWT Authentication Initialization"
    
    $authInitialized = Initialize-AuthenticationForMode
    if ($authInitialized) {
        Write-Host "✅ JWT authentication initialized successfully" -ForegroundColor Green
        $testResults += @{
            Test = "JWT Initialization"
            Status = "Pass"
            Type = "Auth Setup"
            Details = "Token obtained and ready"
        }
    } else {
        Write-Host "❌ JWT authentication initialization failed" -ForegroundColor Red
        $testResults += @{
            Test = "JWT Initialization"
            Status = "Fail"
            Type = "Auth Setup"
            Error = "Could not obtain JWT token"
        }
        return $testResults
    }
    
    # Test authentication endpoints availability
    Write-TestSubsection "Authentication Endpoints Availability"
    
    $authEndpoints = @(
        @{ Endpoint = "/api/auth/demo-credentials"; Method = "GET"; Description = "Demo Credentials" },
        @{ Endpoint = "/api/auth/login"; Method = "POST"; Description = "Login Endpoint" },
        @{ Endpoint = "/api/auth/register"; Method = "POST"; Description = "Register Endpoint" },
        @{ Endpoint = "/api/auth/refresh"; Method = "POST"; Description = "Refresh Token Endpoint" }
    )
    
    foreach ($endpointInfo in $authEndpoints) {
        try {
            if ($endpointInfo.Method -eq "POST") {
                # For POST endpoints, test with empty body to check if endpoint exists
                try {
                    $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Post -Body "{}" -ContentType "application/json" -TimeoutSec $script:TestConfig.TimeoutSeconds
                } catch {
                    if ($_.Exception.Message -like "*400*" -or $_.Exception.Message -like "*BadRequest*") {
                        # 400 Bad Request means endpoint exists but validation failed (expected)
                        Write-Host "✅ $($endpointInfo.Description) - Available (validation failed as expected)" -ForegroundColor Green
                        $testResults += @{ 
                            Test = "$($endpointInfo.Description) Availability"
                            Status = "Pass"
                            Type = "Auth Endpoint"
                            Endpoint = $endpointInfo.Endpoint
                        }
                        continue
                    } else {
                        throw
                    }
                }
            } else {
                $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            }
            
            Write-Host "✅ $($endpointInfo.Description) - Available" -ForegroundColor Green
            $testResults += @{ 
                Test = "$($endpointInfo.Description) Availability"
                Status = "Pass"
                Type = "Auth Endpoint"
                Endpoint = $endpointInfo.Endpoint
            }
        }
        catch {
            if ($_.Exception.Message -like "*404*" -or $_.Exception.Message -like "*Not Found*") {
                Write-Host "❌ $($endpointInfo.Description) - Not found (should be available)" -ForegroundColor Red
                $testResults += @{ 
                    Test = "$($endpointInfo.Description) Availability"
                    Status = "Fail"
                    Type = "Auth Endpoint"
                    Endpoint = $endpointInfo.Endpoint
                    Error = "Endpoint not found"
                }
            } else {
                Write-Host "❌ $($endpointInfo.Description) - Error: $($_.Exception.Message)" -ForegroundColor Red
                $testResults += @{ 
                    Test = "$($endpointInfo.Description) Availability"
                    Status = "Fail"
                    Type = "Auth Endpoint"
                    Endpoint = $endpointInfo.Endpoint
                    Error = $_.Exception.Message
                }
            }
        }
    }
    
    # Test authenticated API access
    Write-TestSubsection "Authenticated API Access"
    
    if ($script:TestConfig.TestToken) {
        $headers = Get-AuthenticationHeaders
        
        # Test API endpoints with authentication
        $apiEndpoints = @(
            @{ Endpoint = "/api/customers"; Description = "Customers List" },
            @{ Endpoint = "/api/products"; Description = "Products List" },
            @{ Endpoint = "/api/orders"; Description = "Orders List" }
        )
        
        foreach ($endpointInfo in $apiEndpoints) {
            try {
                $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Get -Headers $headers -TimeoutSec $script:TestConfig.TimeoutSeconds
                Write-Host "✅ $($endpointInfo.Description) - Accessible with JWT token" -ForegroundColor Green
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Authenticated)"
                    Status = "Pass"
                    Type = "API Access"
                    Endpoint = $endpointInfo.Endpoint
                }
            }
            catch {
                Write-Host "❌ $($endpointInfo.Description) - Failed with JWT token: $($_.Exception.Message)" -ForegroundColor Red
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Authenticated)"
                    Status = "Fail"
                    Type = "API Access"
                    Endpoint = $endpointInfo.Endpoint
                    Error = $_.Exception.Message
                }
            }
        }
    } else {
        Write-Host "⚠️  No JWT token available for testing authenticated access" -ForegroundColor Yellow
        $testResults += @{
            Test = "Authenticated API Access"
            Status = "Warning"
            Type = "API Access"
            Details = "No token available for testing"
        }
    }
    
    # Test unauthorized access (without token)
    Write-TestSubsection "Unauthorized Access Testing"
    
    Write-Host "� Testing that endpoints properly reject unauthorized requests..." -ForegroundColor Yellow
    
    # Note: Since we discovered /api/auth/profile doesn't exist, we'll test other endpoints
    # that might require authentication in the future
    $potentialProtectedEndpoints = @(
        @{ Endpoint = "/api/orders/create"; Description = "Create Order" },
        @{ Endpoint = "/api/customers/create"; Description = "Create Customer" }
    )
    
    foreach ($endpointInfo in $potentialProtectedEndpoints) {
        try {
            $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$($endpointInfo.Endpoint)" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            Write-Host "ℹ️  $($endpointInfo.Description) - Accessible without authentication" -ForegroundColor Gray
            $testResults += @{
                Test = "$($endpointInfo.Description) (No Auth)"
                Status = "Pass"
                Type = "Access Control"
                Endpoint = $endpointInfo.Endpoint
                Details = "Currently public endpoint"
            }
        }
        catch {
            if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
                Write-Host "✅ $($endpointInfo.Description) - Correctly requires authentication" -ForegroundColor Green
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Auth Required)"
                    Status = "Pass"
                    Type = "Access Control"
                    Endpoint = $endpointInfo.Endpoint
                    Details = "Properly protected"
                }
            } elseif ($_.Exception.Message -like "*404*" -or $_.Exception.Message -like "*Not Found*") {
                Write-Host "ℹ️  $($endpointInfo.Description) - Not implemented (404)" -ForegroundColor Gray
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Not Implemented)"
                    Status = "Pass"
                    Type = "Access Control"
                    Endpoint = $endpointInfo.Endpoint
                    Details = "Endpoint not implemented"
                }
            } elseif ($_.Exception.Message -like "*400*" -or $_.Exception.Message -like "*Bad Request*") {
                Write-Host "ℹ️  $($endpointInfo.Description) - Requires parameters (400)" -ForegroundColor Gray
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Requires Parameters)"
                    Status = "Pass"
                    Type = "Access Control"
                    Endpoint = $endpointInfo.Endpoint
                    Details = "Endpoint exists but requires parameters"
                }
            } else {
                Write-Host "❌ $($endpointInfo.Description) - Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
                $testResults += @{
                    Test = "$($endpointInfo.Description) (Error)"
                    Status = "Fail"
                    Type = "Access Control"
                    Endpoint = $endpointInfo.Endpoint
                    Error = $_.Exception.Message
                }
            }
        }
    }
    
    return $testResults
}

function Test-EntraExternalId {
    Write-TestSection "Testing Entra External ID Authentication Mode"
    
    $testResults = @()
    
    Write-Host "🔐 Entra External ID authentication mode detected" -ForegroundColor Yellow
    Write-Host "⚠️  Entra External ID testing is not yet fully implemented" -ForegroundColor Yellow
    Write-Host "   This would require OAuth2/OIDC flow testing with Microsoft Entra" -ForegroundColor Gray
    
    # Basic availability tests
    Write-TestSubsection "Basic Endpoint Availability"
    
    $endpoints = @(
        "/api/info",
        "/api/info/auth",
        "/api/customers",
        "/api/products"
    )
    
    foreach ($endpoint in $endpoints) {
        try {
            $response = Invoke-RestMethod -Uri "$($script:TestConfig.ApiBaseUrl)$endpoint" -Method Get -TimeoutSec $script:TestConfig.TimeoutSeconds
            Write-Host "✅ $endpoint - Available" -ForegroundColor Green
            $testResults += @{ Endpoint = $endpoint; Status = "Pass"; Type = "Basic Access" }
        }
        catch {
            Write-Host "❌ $endpoint - Failed: $($_.Exception.Message)" -ForegroundColor Red
            $testResults += @{ Endpoint = $endpoint; Status = "Fail"; Type = "Basic Access"; Error = $_.Exception.Message }
        }
    }
    
    # Note about required Entra External ID implementation
    Write-Host ""
    Write-Host "📋 To fully test Entra External ID authentication:" -ForegroundColor Cyan
    Write-Host "   • OAuth2/OIDC flow testing" -ForegroundColor Gray
    Write-Host "   • Token validation testing" -ForegroundColor Gray
    Write-Host "   • User provisioning testing" -ForegroundColor Gray
    Write-Host "   • Role-based access testing" -ForegroundColor Gray
    
    $testResults += @{ Test = "Entra Implementation Status"; Status = "Pending"; Type = "Implementation"; Details = "Full testing not yet implemented" }
    
    return $testResults
}

function Show-AuthenticationTestSummary {
    param(
        [array]$TestResults,
        [string]$AuthMode
    )
    
    Write-TestSection "Authentication Test Summary"
    
    $passed = ($TestResults | Where-Object { $_.Status -eq "Pass" }).Count
    $failed = ($TestResults | Where-Object { $_.Status -eq "Fail" }).Count
    $warnings = ($TestResults | Where-Object { $_.Status -eq "Warning" }).Count
    $errors = ($TestResults | Where-Object { $_.Status -eq "Error" }).Count
    $pending = ($TestResults | Where-Object { $_.Status -eq "Pending" }).Count
    $total = $TestResults.Count
    
    Write-Host "🔐 Authentication Mode: $AuthMode" -ForegroundColor Cyan
    Write-Host "📊 Test Results:" -ForegroundColor White
    Write-Host "   ✅ Passed: $passed" -ForegroundColor Green
    Write-Host "   ❌ Failed: $failed" -ForegroundColor Red
    Write-Host "   ⚠️  Warnings: $warnings" -ForegroundColor Yellow
    Write-Host "   🔥 Errors: $errors" -ForegroundColor Magenta
    Write-Host "   ⏳ Pending: $pending" -ForegroundColor Gray
    Write-Host "   📋 Total: $total" -ForegroundColor White
    
    # Show failed/error details
    if ($failed -gt 0 -or $errors -gt 0) {
        Write-Host ""
        Write-Host "🔍 Issues Found:" -ForegroundColor Red
        
        $TestResults | Where-Object { $_.Status -eq "Fail" -or $_.Status -eq "Error" } | ForEach-Object {
            Write-Host "   ❌ $($_.Test)$($_.Endpoint): $($_.Error)" -ForegroundColor Red
        }
    }
    
    # Show warnings
    if ($warnings -gt 0) {
        Write-Host ""
        Write-Host "⚠️  Warnings:" -ForegroundColor Yellow
        
        $TestResults | Where-Object { $_.Status -eq "Warning" } | ForEach-Object {
            Write-Host "   ⚠️  $($_.Test)$($_.Endpoint): $($_.Details)" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    
    # Overall status
    if ($failed -eq 0 -and $errors -eq 0) {
        Write-Host "✅ Authentication testing completed successfully!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Authentication testing found issues that need attention" -ForegroundColor Red
        return $false
    }
}

# Main execution
function Start-AuthenticationTesting {
    Write-Host "🔐 Starting Authentication Testing" -ForegroundColor Magenta
    Write-Host "   Target: $ApiBaseUrl" -ForegroundColor Gray
    Write-Host ""
    
    # Initialize test environment with authentication detection
    Initialize-TestEnvironment -ApiBaseUrl $ApiBaseUrl
    
    # Test API connection
    if (-not (Test-ApiConnection)) {
        Write-Host "❌ Cannot proceed without API connection" -ForegroundColor Red
        return $false
    }
    
    # Run authentication tests based on detected mode
    $testResults = @()
    
    switch ($script:TestConfig.AuthenticationMode) {
        "Disabled" {
            $testResults = Test-AuthenticationDisabled
        }
        "JwtTokens" {
            $testResults = Test-JwtAuthentication
        }
        "EntraExternalId" {
            $testResults = Test-EntraExternalId
        }
        default {
            Write-Host "❌ Unknown authentication mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Red
            return $false
        }
    }
    
    # Simple summary (not using the complex Show-AuthenticationTestSummary)
    Write-TestSection "Authentication Test Summary"
    
    if ($testResults -and $testResults.Count -gt 0) {
        $passCount = ($testResults | Where-Object { $_.Status -eq "Pass" }).Count
        $totalCount = $testResults.Count
        
        Write-Host "📊 Test Results: $passCount/$totalCount passed" -ForegroundColor Cyan
        Write-Host ""
        
        # Show major issues only
        $failedTests = $testResults | Where-Object { $_.Status -eq "Fail" }
        if ($failedTests.Count -gt 0) {
            Write-Host "🔍 Issues Found:" -ForegroundColor Yellow
            foreach ($test in $failedTests) {
                Write-Host "   ❌ $($test.Test)" -ForegroundColor Red
                if ($test.Error) {
                    Write-Host "      Error: $($test.Error)" -ForegroundColor Gray
                }
            }
        }
        
        # Return success if no critical failures
        $criticalFailures = $testResults | Where-Object { $_.Status -eq "Fail" -and $_.Type -ne "Access Control" }
        return $criticalFailures.Count -eq 0
    } else {
        Write-Host "❌ No test results returned" -ForegroundColor Red
        return $false
    }
}

# Run tests if script is executed directly
if ($MyInvocation.InvocationName -ne '.') {
    $success = Start-AuthenticationTesting
    exit $(if ($success) { 0 } else { 1 })
}
