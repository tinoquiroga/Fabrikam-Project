#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Demo Authentication Helper Script for Fabrikam API
.DESCRIPTION
    This script provides utilities for working with demo authentication users,
    including displaying credentials and testing authentication endpoints.
.PARAMETER ShowCredentials
    Display the demo user credentials
.PARAMETER TestAuth
    Test authentication endpoints with demo users
.PARAMETER ApiUrl
    The base URL of the Fabrikam API (default: https://localhost:7297)
.EXAMPLE
    .\Demo-Authentication.ps1 -ShowCredentials
    Display demo user credentials
.EXAMPLE
    .\Demo-Authentication.ps1 -TestAuth
    Test authentication with all demo users
.EXAMPLE
    .\Demo-Authentication.ps1 -TestAuth -ApiUrl "https://fabrikam-api.azurewebsites.net"
    Test authentication against production API
#>

[CmdletBinding(DefaultParameterSetName = "Help")]
param(
    [Parameter(ParameterSetName = "ShowCredentials")]
    [switch]$ShowCredentials,
    
    [Parameter(ParameterSetName = "TestAuth")]
    [switch]$TestAuth,
    
    [Parameter()]
    [string]$ApiUrl = "https://localhost:7297"
)

# Demo user credentials (fetched dynamically from API)
$DemoUsers = @()

# Function to fetch demo credentials from API
function Get-DemoCredentials {
    param(
        [string]$ApiUrl = "https://localhost:7297"
    )
    
    try {
        Write-Verbose "Fetching demo credentials from $ApiUrl/api/auth/demo-credentials"
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/auth/demo-credentials" -Method GET -SkipCertificateCheck -ErrorAction Stop
        
        Write-Verbose "Retrieved credentials for instance: $($response.instanceId)"
        return $response.demoUsers
    }
    catch {
        Write-Warning "Could not fetch dynamic credentials from API: $($_.Exception.Message)"
        Write-Host "Falling back to default local credentials..." -ForegroundColor Yellow
        
        # Fallback to default credentials if API is not available
        return @(
            @{
                Name        = "Lee Gu (Admin)"
                Email       = "lee.gu@fabrikam.levelupcsp.com"
                Password    = "Please start API server to get instance passwords"
                Role        = "Admin"
                Description = "Full system access - Licensed mailbox"
            },
            @{
                Name        = "Alex Wilber (Read-Write)"
                Email       = "alex.wilber@fabrikam.levelupcsp.com"
                Password    = "Please start API server to get instance passwords"
                Role        = "Read-Write"
                Description = "Can view and modify data - Licensed mailbox"
            },
            @{
                Name        = "Henrietta Mueller (Read-Only)"
                Email       = "henrietta.mueller@fabrikam.levelupcsp.com"
                Password    = "Please start API server to get instance passwords"
                Role        = "Read-Only"
                Description = "View access only - Licensed mailbox"
            }
        )
    }
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "🔐 $Title" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

function Write-UserCredentials {
    param($User)
    
    Write-Host "📧 $($User.Name):" -ForegroundColor Yellow
    Write-Host "   Email: $($User.Email)" -ForegroundColor White
    Write-Host "   Password: $($User.Password)" -ForegroundColor Green
    Write-Host "   Role: $($User.Role)" -ForegroundColor Blue
    Write-Host "   Description: $($User.Description)" -ForegroundColor Gray
    Write-Host ""
}

function Test-Authentication {
    param($User, $ApiUrl)
    
    Write-Host "Testing authentication for $($User.Name)..." -ForegroundColor Yellow
    
    try {
        $loginData = @{
            Email    = $User.Email
            Password = $User.Password
        }
        
        $json = $loginData | ConvertTo-Json
        $headers = @{ "Content-Type" = "application/json" }
        
        Write-Verbose "Sending login request to $ApiUrl/api/auth/login"
        
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" -Method POST -Body $json -Headers $headers -SkipCertificateCheck -ErrorAction Stop
        
        if ($response.accessToken) {
            Write-Host "  ✅ Login successful" -ForegroundColor Green
            Write-Host "     Token length: $($response.accessToken.Length) characters" -ForegroundColor Gray
            Write-Host "     Expires: $($response.expiresAt)" -ForegroundColor Gray
            Write-Host "     User: $($response.user.displayName) ($($response.user.email))" -ForegroundColor Gray
            Write-Host "     Roles: $($response.user.roles -join ', ')" -ForegroundColor Gray
            
            # Test token validation
            Write-Verbose "Testing token validation..."
            $authHeaders = @{
                "Authorization" = "Bearer $($response.accessToken)"
                "Content-Type"  = "application/json"
            }
            
            try {
                $meResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/me" -Method GET -Headers $authHeaders -SkipCertificateCheck
                Write-Verbose "Token validation successful for user: $($meResponse.email)"
                Write-Host "  ✅ Token validation successful" -ForegroundColor Green
                Write-Host "     Validated user: $($meResponse.email)" -ForegroundColor Gray
            }
            catch {
                Write-Host "  ❌ Token validation failed: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        else {
            Write-Host "  ❌ Login failed: No access token received" -ForegroundColor Red
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode
            Write-Host "  ❌ Login failed ($statusCode): $errorMessage" -ForegroundColor Red
        }
        else {
            Write-Host "  ❌ Login failed: $errorMessage" -ForegroundColor Red
        }
        
        if ($errorMessage -like "*connection*refused*") {
            Write-Host "     💡 Ensure the API server is running at $ApiUrl" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
}

# Main execution
if ($ShowCredentials) {
    Write-Header "FABRIKAM DEMO AUTHENTICATION CREDENTIALS"
    
    # Fetch current instance credentials
    $DemoUsers = Get-DemoCredentials -ApiUrl $ApiUrl
    
    Write-Host "🔑 Instance-Specific Credentials" -ForegroundColor Yellow
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($user in $DemoUsers) {
        Write-UserCredentials $user
    }
    
    Write-Host "🔗 Test these credentials at: $ApiUrl/swagger" -ForegroundColor Cyan
    Write-Host "📋 Use the /api/auth/login endpoint to get JWT tokens" -ForegroundColor Cyan
    Write-Host "💡 Copy the token from login response and use 'Bearer <token>' in Authorization header" -ForegroundColor Cyan
    Write-Host "🛡️ Passwords are unique to this instance for security" -ForegroundColor Cyan
    Write-Host ""
}
elseif ($TestAuth) {
    Write-Header "TESTING DEMO USER AUTHENTICATION"
    
    # Fetch current instance credentials
    $DemoUsers = Get-DemoCredentials -ApiUrl $ApiUrl
    
    Write-Host "🌐 API Base URL: $ApiUrl" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($user in $DemoUsers) {
        Test-Authentication $user $ApiUrl
    }
    
    Write-Host "✅ Authentication testing completed" -ForegroundColor Green
    Write-Host "💡 Use -Verbose flag for detailed token validation output" -ForegroundColor Yellow
    Write-Host ""
}
else {
    Write-Header "FABRIKAM DEMO AUTHENTICATION HELPER"
    
    Write-Host "Available commands:" -ForegroundColor White
    Write-Host ""
    Write-Host "  Display demo user credentials:" -ForegroundColor Yellow
    Write-Host "    .\Demo-Authentication.ps1 -ShowCredentials" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Test authentication endpoints:" -ForegroundColor Yellow
    Write-Host "    .\Demo-Authentication.ps1 -TestAuth" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Test against different API URL:" -ForegroundColor Yellow
    Write-Host "    .\Demo-Authentication.ps1 -TestAuth -ApiUrl 'https://your-api.com'" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Verbose output with detailed logging:" -ForegroundColor Yellow
    Write-Host "    .\Demo-Authentication.ps1 -TestAuth -Verbose" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor White
    Write-Host "  .\Demo-Authentication.ps1 -ShowCredentials | Out-File demo-creds.txt" -ForegroundColor Gray
    Write-Host "  .\Demo-Authentication.ps1 -TestAuth -ApiUrl 'https://localhost:7297'" -ForegroundColor Gray
    Write-Host ""
}
