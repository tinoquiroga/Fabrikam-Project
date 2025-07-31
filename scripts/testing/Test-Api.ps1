# Test-Api.ps1 - Authentication-Aware API Testing Module
# Supports all authentication modes: Disabled, BearerToken, EntraExternalId

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

# Import DTO validation utilities
$DtoValidationScript = Join-Path $PSScriptRoot "Test-DtoValidation.ps1"
if (Test-Path $DtoValidationScript) {
    . $DtoValidationScript
} else {
    Write-Host "⚠️  Could not find Test-DtoValidation.ps1 - DTO validation will be limited" -ForegroundColor Yellow
}

function Test-ApiEndpoints {
    Write-TestSection "API Endpoint Testing"
    
    $testResults = @()
    
    Write-TestSubsection "Basic Information Endpoints"
    
    # Test info endpoint (always available)
    $result = Test-EndpointWithAuth -Endpoint "/api/info" -Description "API Information"
    $testResults += $result
    
    # Test authentication info endpoint
    $result = Test-EndpointWithAuth -Endpoint "/api/info/auth" -Description "Authentication Information"
    $testResults += $result
    
    # Test data endpoints based on authentication mode
    Write-TestSubsection "Data Endpoints"
    
    if ($script:TestConfig.AuthenticationMode -eq "Disabled") {
        Write-Host "🔓 Testing endpoints without authentication..." -ForegroundColor Yellow
        
        # All endpoints should work without authentication
        $result = Test-EndpointWithAuth -Endpoint "/api/customers" -Description "Get Customers (No Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/products" -Description "Get Products (No Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/orders" -Description "Get Orders (No Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/orders/analytics" -Description "Order Analytics (No Auth)"
        $testResults += $result
        
    } elseif ($script:TestConfig.AuthenticationMode -eq "BearerToken" -or $script:TestConfig.AuthenticationMode -eq "JwtTokens") {
        Write-Host "🔐 Testing endpoints with Bearer Token authentication..." -ForegroundColor Yellow
        
        # Test with authentication
        $result = Test-EndpointWithAuth -Endpoint "/api/customers" -Description "Get Customers (JWT Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/products" -Description "Get Products (JWT Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/orders" -Description "Get Orders (JWT Auth)"
        $testResults += $result
        
        $result = Test-EndpointWithAuth -Endpoint "/api/orders/analytics" -Description "Order Analytics (JWT Auth)"
        $testResults += $result
        
        # Test authentication endpoints
        Write-TestSubsection "Authentication Endpoints"
        
        $result = Test-EndpointWithAuth -Endpoint "/api/auth/demo-credentials" -Description "Demo Credentials"
        $testResults += $result
        
        # Test user profile endpoint
        $result = Test-EndpointWithAuth -Endpoint "/api/auth/profile" -Description "User Profile"
        $testResults += $result
        
    } elseif ($script:TestConfig.AuthenticationMode -eq "EntraExternalId") {
        Write-Host "🔐 Testing endpoints with Entra External ID..." -ForegroundColor Yellow
        Write-Host "⚠️  Entra External ID testing not yet implemented" -ForegroundColor Yellow
        
        # For now, test without authentication to check availability
        $result = Test-EndpointWithAuth -Endpoint "/api/customers" -Description "Get Customers (Entra - No Auth Test)"
        $testResults += $result
        
    } else {
        Write-Host "❌ Unknown authentication mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Red
    }
    
    # Test error handling
    Write-TestSubsection "Error Handling"
    
    $result = Test-EndpointWithAuth -Endpoint "/api/customers/99999" -Description "Non-existent Customer" -ExpectSuccess:$false
    $testResults += $result
    
    $result = Test-EndpointWithAuth -Endpoint "/api/products/99999" -Description "Non-existent Product" -ExpectSuccess:$false
    $testResults += $result
    
    return $testResults
}

function Test-ApiPerformance {
    param([int]$MaxResponseTimeMs = 5000)
    
    Write-TestSection "API Performance Testing"
    
    $performanceResults = @()
    
    $endpoints = @(
        @{ Path = "/api/info"; Name = "API Info" }
        @{ Path = "/api/customers"; Name = "Customers List" }
        @{ Path = "/api/products"; Name = "Products List" }
        @{ Path = "/api/orders"; Name = "Orders List" }
    )
    
    foreach ($endpoint in $endpoints) {
        try {
            Write-Host "⏱️  Testing $($endpoint.Name) response time..." -ForegroundColor Cyan
            
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            $response = Invoke-AuthenticatedRequest -Uri "$($script:TestConfig.ApiBaseUrl)$($endpoint.Path)"
            $stopwatch.Stop()
            
            $responseTime = $stopwatch.ElapsedMilliseconds
            
            if ($responseTime -le $MaxResponseTimeMs) {
                Write-Host "✅ $($endpoint.Name): ${responseTime}ms" -ForegroundColor Green
                $performanceResults += @{ Endpoint = $endpoint.Name; ResponseTime = $responseTime; Status = "Pass" }
            } else {
                Write-Host "⚠️  $($endpoint.Name): ${responseTime}ms (exceeds ${MaxResponseTimeMs}ms)" -ForegroundColor Yellow
                $performanceResults += @{ Endpoint = $endpoint.Name; ResponseTime = $responseTime; Status = "Slow" }
            }
        }
        catch {
            Write-Host "❌ $($endpoint.Name): Failed to test performance" -ForegroundColor Red
            $performanceResults += @{ Endpoint = $endpoint.Name; ResponseTime = -1; Status = "Error" }
        }
    }
    
    return $performanceResults
}

function Test-ApiDataIntegrity {
    Write-TestSection "API Data Integrity Testing"
    
    $integrityResults = @()
    
    try {
        Write-Host "🔍 Checking data relationships..." -ForegroundColor Cyan
        
        # Get customers and verify they have valid data
        $customers = Invoke-AuthenticatedRequest -Uri "$($script:TestConfig.ApiBaseUrl)/api/customers"
        if ($customers -and $customers.Count -gt 0) {
            Write-Host "✅ Customers data retrieved: $($customers.Count) customers" -ForegroundColor Green
            $integrityResults += @{ Test = "Customers Data"; Status = "Pass"; Details = "$($customers.Count) customers" }
            
            # Check first customer structure
            $firstCustomer = $customers[0]
            if ($firstCustomer.id -and $firstCustomer.name -and $firstCustomer.email) {
                Write-Host "✅ Customer data structure valid" -ForegroundColor Green
                $integrityResults += @{ Test = "Customer Data Structure"; Status = "Pass"; Details = "Required fields present" }
            } else {
                Write-Host "❌ Customer data structure invalid" -ForegroundColor Red
                $integrityResults += @{ Test = "Customer Data Structure"; Status = "Fail"; Details = "Missing required fields" }
            }
        } else {
            Write-Host "⚠️  No customers found or data retrieval failed" -ForegroundColor Yellow
            $integrityResults += @{ Test = "Customers Data"; Status = "Warning"; Details = "No data available" }
        }
        
        # Get products and verify they have valid data
        $products = Invoke-AuthenticatedRequest -Uri "$($script:TestConfig.ApiBaseUrl)/api/products"
        if ($products -and $products.Count -gt 0) {
            Write-Host "✅ Products data retrieved: $($products.Count) products" -ForegroundColor Green
            $integrityResults += @{ Test = "Products Data"; Status = "Pass"; Details = "$($products.Count) products" }
            
            # Check first product structure
            $firstProduct = $products[0]
            if ($firstProduct.id -and $firstProduct.name -and $firstProduct.price -ne $null) {
                Write-Host "✅ Product data structure valid" -ForegroundColor Green
                $integrityResults += @{ Test = "Product Data Structure"; Status = "Pass"; Details = "Required fields present" }
            } else {
                Write-Host "❌ Product data structure invalid" -ForegroundColor Red
                $integrityResults += @{ Test = "Product Data Structure"; Status = "Fail"; Details = "Missing required fields" }
            }
        } else {
            Write-Host "⚠️  No products found or data retrieval failed" -ForegroundColor Yellow
            $integrityResults += @{ Test = "Products Data"; Status = "Warning"; Details = "No data available" }
        }
        
        # Get orders and verify relationships
        $orders = Invoke-AuthenticatedRequest -Uri "$($script:TestConfig.ApiBaseUrl)/api/orders"
        if ($orders -and $orders.Count -gt 0) {
            Write-Host "✅ Orders data retrieved: $($orders.Count) orders" -ForegroundColor Green
            $integrityResults += @{ Test = "Orders Data"; Status = "Pass"; Details = "$($orders.Count) orders" }
            
            # Check first order structure
            $firstOrder = $orders[0]
            if ($firstOrder.id -and $firstOrder.customer -and $firstOrder.orderDate) {
                Write-Host "✅ Order data structure valid" -ForegroundColor Green
                $integrityResults += @{ Test = "Order Data Structure"; Status = "Pass"; Details = "Required fields present" }
            } else {
                Write-Host "❌ Order data structure invalid" -ForegroundColor Red
                $integrityResults += @{ Test = "Order Data Structure"; Status = "Fail"; Details = "Missing required fields" }
            }
        } else {
            Write-Host "⚠️  No orders found or data retrieval failed" -ForegroundColor Yellow
            $integrityResults += @{ Test = "Orders Data"; Status = "Warning"; Details = "No data available" }
        }
        
    }
    catch {
        Write-Host "❌ Data integrity testing failed: $($_.Exception.Message)" -ForegroundColor Red
        $integrityResults += @{ Test = "Data Integrity"; Status = "Error"; Details = $_.Exception.Message }
    }
    
    return $integrityResults
}

function Show-ApiTestSummary {
    param(
        [array]$EndpointResults,
        [array]$PerformanceResults,
        [array]$IntegrityResults
    )
    
    Write-TestSection "API Test Summary"
    
    # Endpoint test summary
    $endpointPassed = ($EndpointResults | Where-Object { $_.Success -eq $true }).Count
    $endpointTotal = $EndpointResults.Count
    
    Write-Host "📊 Endpoint Tests: $endpointPassed/$endpointTotal passed" -ForegroundColor $(if ($endpointPassed -eq $endpointTotal) { "Green" } else { "Yellow" })
    
    # Performance test summary
    if ($PerformanceResults.Count -gt 0) {
        $performancePassed = ($PerformanceResults | Where-Object { $_.Status -eq "Pass" }).Count
        $performanceTotal = $PerformanceResults.Count
        
        Write-Host "⏱️  Performance Tests: $performancePassed/$performanceTotal within limits" -ForegroundColor $(if ($performancePassed -eq $performanceTotal) { "Green" } else { "Yellow" })
    }
    
    # Integrity test summary
    if ($IntegrityResults.Count -gt 0) {
        $integrityPassed = ($IntegrityResults | Where-Object { $_.Status -eq "Pass" }).Count
        $integrityTotal = $IntegrityResults.Count
        
        Write-Host "🔍 Data Integrity Tests: $integrityPassed/$integrityTotal passed" -ForegroundColor $(if ($integrityPassed -eq $integrityTotal) { "Green" } else { "Yellow" })
    }
    
    # Authentication mode summary
    Write-Host "🔐 Authentication Mode: $($script:TestConfig.AuthenticationMode)" -ForegroundColor Cyan
    
    Write-Host ""
    
    # Overall status
    $overallPassed = $endpointPassed -eq $endpointTotal
    if ($PerformanceResults.Count -gt 0) {
        $overallPassed = $overallPassed -and (($PerformanceResults | Where-Object { $_.Status -ne "Error" }).Count -eq $PerformanceResults.Count)
    }
    if ($IntegrityResults.Count -gt 0) {
        $overallPassed = $overallPassed -and (($IntegrityResults | Where-Object { $_.Status -ne "Fail" }).Count -eq $IntegrityResults.Count)
    }
    
    if ($overallPassed) {
        Write-Host "✅ All API tests completed successfully!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "⚠️  Some API tests failed or had warnings" -ForegroundColor Yellow
        return $false
    }
}

# Main execution
function Start-ApiTesting {
    Write-Host "🚀 Starting Authentication-Aware API Testing" -ForegroundColor Magenta
    Write-Host "   Target: $ApiBaseUrl" -ForegroundColor Gray
    Write-Host "   Mode: $(if ($Quick) { 'Quick' } else { 'Full' })" -ForegroundColor Gray
    Write-Host ""
    
    # Initialize test environment with authentication detection
    Initialize-TestEnvironment -ApiBaseUrl $ApiBaseUrl
    
    # Test API connection
    if (-not (Test-ApiConnection -Url $ApiBaseUrl)) {
        Write-Host "❌ Cannot connect to API. Please ensure the API is running." -ForegroundColor Red
        return $false
    }
    
    # Run endpoint tests
    $endpointResults = Test-ApiEndpoints
    
    # Run performance tests (unless quick mode)
    $performanceResults = @()
    if (-not $Quick) {
        $performanceResults = Test-ApiPerformance
    }
    
    # Run data integrity tests (unless quick mode)
    $integrityResults = @()
    if (-not $Quick) {
        $integrityResults = Test-ApiDataIntegrity
    }
    
    # Show summary
    $success = Show-ApiTestSummary -EndpointResults $endpointResults -PerformanceResults $performanceResults -IntegrityResults $integrityResults
    
    return $success
}

# Run tests if script is executed directly
if ($MyInvocation.InvocationName -ne '.') {
    $success = Start-ApiTesting
    exit $(if ($success) { 0 } else { 1 })
}





