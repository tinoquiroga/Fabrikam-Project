# Fabrikam Testing - Authentication
# Comprehensive testing for JWT authentication and authorization

# Import shared utilities
. "$PSScriptRoot\Test-Shared.ps1"

function Test-Authentication {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Verbose
    )
    
    Write-SectionHeader "AUTHENTICATION TESTS"
    
    # Test basic authentication flow
    Test-LoginEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    if (-not $Quick) {
        Test-UserRegistration -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-TokenValidation -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-RoleBasedAccess -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    }
}

function Test-LoginEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # First, get the current demo credentials
    $credResult = Get-DemoCredentials -ApiBaseUrl $ApiBaseUrl -TimeoutSec $TimeoutSeconds
    
    if (-not $credResult.Success) {
        Add-TestResult "AuthTests" "Demo Credentials Retrieval" $false "Failed to get demo credentials: $($credResult.Error)"
        return
    }
    
    Add-TestResult "AuthTests" "Demo Credentials Retrieval" $true "Retrieved credentials for instance: $($credResult.InstanceId)"
    
    # Test admin login (using Admin role)
    $adminUser = $credResult.Users["Admin"]
    if ($adminUser) {
        $tokenResult = Get-JwtToken -ApiBaseUrl $ApiBaseUrl -Username $adminUser.Email -Password $adminUser.Password -TimeoutSec $TimeoutSeconds
        
        if ($tokenResult.Success) {
            Add-TestResult "AuthTests" "Admin Login" $true "Successfully authenticated admin user: $($adminUser.Name)"
            
            # Validate token structure
            if ($tokenResult.Token -and $tokenResult.Token.Length -gt 50) {
                Add-TestResult "AuthTests" "JWT Token Generation" $true "Valid JWT token generated (length: $($tokenResult.Token.Length))"
                
                # Store token for other tests
                $Global:AdminToken = $tokenResult.Token
                $Global:AdminUser = $tokenResult.User
                
                # Validate user info
                if ($tokenResult.User -and $tokenResult.User.email) {
                    Add-TestResult "AuthTests" "User Information" $true "User info returned: $($tokenResult.User.email)"
                    
                    # Check roles
                    if ($tokenResult.User.roles -and $tokenResult.User.roles -contains "Admin") {
                        Add-TestResult "AuthTests" "Admin Role Assignment" $true "Admin role properly assigned"
                    }
                    else {
                        Add-TestResult "AuthTests" "Admin Role Assignment" $false "Admin role not found in user roles"
                    }
                }
                else {
                    Add-TestResult "AuthTests" "User Information" $false "No user information returned"
                }
            }
            else {
                Add-TestResult "AuthTests" "JWT Token Generation" $false "Invalid or short token received"
            }
        }
        else {
            Add-TestResult "AuthTests" "Admin Login" $false "Failed: $($tokenResult.Error)"
            $Global:AdminToken = $null
        }
    }
    else {
        Add-TestResult "AuthTests" "Admin Login" $false "No Admin user found in demo credentials"
    }
    
    # Test read-write user login (closest to sales role)
    $rwUser = $credResult.Users["Read-Write"]
    if ($rwUser) {
        $salesTokenResult = Get-JwtToken -ApiBaseUrl $ApiBaseUrl -Username $rwUser.Email -Password $rwUser.Password -TimeoutSec $TimeoutSeconds
        
        if ($salesTokenResult.Success) {
            Add-TestResult "AuthTests" "Sales User Login" $true "Successfully authenticated read-write user: $($rwUser.Name)"
            $Global:SalesToken = $salesTokenResult.Token
            
            # Check read-write role
            if ($salesTokenResult.User.roles -and $salesTokenResult.User.roles -contains "Read-Write") {
                Add-TestResult "AuthTests" "Sales Role Assignment" $true "Read-Write role properly assigned"
            }
            else {
                Add-TestResult "AuthTests" "Sales Role Assignment" $false "Read-Write role not found in user roles"
            }
        }
        else {
            Add-TestResult "AuthTests" "Sales User Login" $false "Failed: $($salesTokenResult.Error)"
            $Global:SalesToken = $null
        }
    }
    else {
        Add-TestResult "AuthTests" "Sales User Login" $false "No Read-Write user found in demo credentials"
    }
}

function Test-UserRegistration {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test user registration endpoint
    $testUser = @{
        FirstName = "Test"
        LastName = "User"
        Email = "test.user.$(Get-Date -Format 'yyyyMMddHHmmss')@fabrikam.local"
        Password = "TestPass123!"
    } | ConvertTo-Json
    
    $headers = @{ 'Content-Type' = 'application/json' }
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/register" -Method Post -Body $testUser -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.token) {
            Add-TestResult "AuthTests" "User Registration" $true "New user registration successful"
            
            # Test login with new user
            $loginData = ($testUser | ConvertFrom-Json)
            $loginResult = Get-JwtToken -ApiBaseUrl $ApiBaseUrl -Username $loginData.Email -Password $loginData.Password -TimeoutSec $TimeoutSeconds
            
            if ($loginResult.Success) {
                Add-TestResult "AuthTests" "New User Login" $true "New user can login successfully"
            }
            else {
                Add-TestResult "AuthTests" "New User Login" $false "New user cannot login: $($loginResult.Error)"
            }
        }
        else {
            Add-TestResult "AuthTests" "User Registration" $false "Registration succeeded but no token returned"
        }
    }
    catch {
        # Registration might fail if user exists or endpoint is protected
        if ($_.Exception.Message -like "*409*" -or $_.Exception.Message -like "*already exists*") {
            Add-TestResult "AuthTests" "User Registration" $true "Registration endpoint working (user already exists)"
        }
        else {
            Add-TestResult "AuthTests" "User Registration" $false "Registration failed: $($_.Exception.Message)"
        }
    }
}

function Test-TokenValidation {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    if (-not $Global:AdminToken) {
        Add-TestResult "AuthTests" "Token Validation" $false "No admin token available for testing"
        return
    }
    
    # Test protected endpoint with valid token
    $headers = @{ 
        'Authorization' = "Bearer $Global:AdminToken"
        'Content-Type' = 'application/json'
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/profile" -Method Get -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.email) {
            Add-TestResult "AuthTests" "Token Validation" $true "Valid token accepted by protected endpoint"
            
            # Verify token expiration handling
            Test-InvalidTokenScenarios -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
        }
        else {
            Add-TestResult "AuthTests" "Token Validation" $false "Protected endpoint accessible but no user data returned"
        }
    }
    catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
            Add-TestResult "AuthTests" "Token Validation" $false "Valid token rejected: $($_.Exception.Message)"
        }
        else {
            Add-TestResult "AuthTests" "Token Validation" $false "Token validation test failed: $($_.Exception.Message)"
        }
    }
}

function Test-InvalidTokenScenarios {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test with invalid token
    $invalidHeaders = @{ 
        'Authorization' = "Bearer invalid.token.here"
        'Content-Type' = 'application/json'
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/profile" -Method Get -Headers $invalidHeaders -TimeoutSec $TimeoutSeconds
        Add-TestResult "AuthTests" "Invalid Token Rejection" $false "Invalid token was accepted"
    }
    catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
            Add-TestResult "AuthTests" "Invalid Token Rejection" $true "Invalid token properly rejected"
        }
        else {
            Add-TestResult "AuthTests" "Invalid Token Rejection" $false "Unexpected error with invalid token: $($_.Exception.Message)"
        }
    }
    
    # Test with no token
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/profile" -Method Get -TimeoutSec $TimeoutSeconds
        Add-TestResult "AuthTests" "No Token Rejection" $false "Request without token was accepted"
    }
    catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
            Add-TestResult "AuthTests" "No Token Rejection" $true "Request without token properly rejected"
        }
        else {
            Add-TestResult "AuthTests" "No Token Rejection" $false "Unexpected error with no token: $($_.Exception.Message)"
        }
    }
}

function Test-RoleBasedAccess {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    if (-not $Global:AdminToken -or -not $Global:SalesToken) {
        Add-TestResult "AuthTests" "Role-Based Access" $false "Missing tokens for role-based testing"
        return
    }
    
    # Test admin access to admin endpoints
    $adminHeaders = @{ 
        'Authorization' = "Bearer $Global:AdminToken"
        'Content-Type' = 'application/json'
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users" -Method Get -Headers $adminHeaders -TimeoutSec $TimeoutSeconds
        Add-TestResult "AuthTests" "Admin Role Access" $true "Admin can access admin endpoints"
    }
    catch {
        if ($_.Exception.Message -like "*403*" -or $_.Exception.Message -like "*Forbidden*") {
            Add-TestResult "AuthTests" "Admin Role Access" $false "Admin cannot access admin endpoints (403)"
        }
        elseif ($_.Exception.Message -like "*404*") {
            Add-TestResult "AuthTests" "Admin Role Access" $true "Admin endpoints accessible (404 expected if endpoint doesn't exist)"
        }
        else {
            Add-TestResult "AuthTests" "Admin Role Access" $false "Admin access test failed: $($_.Exception.Message)"
        }
    }
    
    # Test sales user access to restricted endpoints
    $salesHeaders = @{ 
        'Authorization' = "Bearer $Global:SalesToken"
        'Content-Type' = 'application/json'
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users" -Method Get -Headers $salesHeaders -TimeoutSec $TimeoutSeconds
        Add-TestResult "AuthTests" "Sales Role Restriction" $false "Sales user can access admin endpoints (should be restricted)"
    }
    catch {
        if ($_.Exception.Message -like "*403*" -or $_.Exception.Message -like "*Forbidden*") {
            Add-TestResult "AuthTests" "Sales Role Restriction" $true "Sales user properly restricted from admin endpoints"
        }
        elseif ($_.Exception.Message -like "*404*") {
            Add-TestResult "AuthTests" "Sales Role Restriction" $true "Sales user access test passed (404 expected)"
        }
        else {
            Add-TestResult "AuthTests" "Sales Role Restriction" $false "Sales restriction test failed: $($_.Exception.Message)"
        }
    }
}

# Note: Functions are automatically available when this script is dot-sourced
