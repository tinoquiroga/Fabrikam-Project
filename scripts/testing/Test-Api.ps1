# Fabrikam Testing - API Endpoints
# Comprehensive testing for API endpoints using FabrikamContracts DTO validation
# DTOs used: OrderDto, CustomerListItemDto, ProductDto, SalesAnalyticsDto

# Import shared utilities
. "$PSScriptRoot\Test-Shared.ps1"

function Test-ApiEndpoints {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Verbose
    )
    
    Write-SectionHeader "API ENDPOINT TESTS"
    
    # Test core business endpoints
    Test-OrdersEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    Test-CustomersEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    Test-ProductsEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    Test-AnalyticsEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    if (-not $Quick) {
        Test-HealthEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-InfoEndpoint -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds
    }
}

function Test-OrdersEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test orders list
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/orders" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API /api/orders" $true "Orders endpoint accessible"
        
        # Validate response structure
        if ($result.Data -is [array]) {
            $orderCount = $result.Data.Count
            Add-TestResult "ApiTests" "Orders Response Structure" $true "Found $orderCount orders with valid structure"
            
            # Check order properties - API returns 'customer' object with embedded data
            if ($orderCount -gt 0) {
                $firstOrder = $result.Data[0]
                $hasRequiredFields = $firstOrder.PSObject.Properties.Name -contains "id" -and 
                                   $firstOrder.PSObject.Properties.Name -contains "customer" -and
                                   $firstOrder.PSObject.Properties.Name -contains "total" -and
                                   $firstOrder.PSObject.Properties.Name -contains "orderNumber"
                
                if ($hasRequiredFields) {
                    Add-TestResult "ApiTests" "Order Data Structure" $true "Orders contain required fields (id, customer, total, orderNumber)"
                    
                    # Validate customer object structure
                    if ($firstOrder.customer -and $firstOrder.customer.PSObject.Properties.Name -contains "name") {
                        Add-TestResult "ApiTests" "Order Customer Structure" $true "Order customer object contains name field"
                    }
                    else {
                        Add-TestResult "ApiTests" "Order Customer Structure" $false "Order customer object missing name field"
                    }
                }
                else {
                    Add-TestResult "ApiTests" "Order Data Structure" $false "Orders missing required fields"
                }
            }
        }
        else {
            Add-TestResult "ApiTests" "Orders Response Structure" $false "Expected array but got: $($result.Data.GetType().Name)"
        }
    }
    else {
        Add-TestResult "ApiTests" "API /api/orders" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

function Test-CustomersEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test customers list
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/customers" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API /api/customers" $true "Customers endpoint accessible"
        
        # Validate response structure
        if ($result.Data -is [array]) {
            $customerCount = $result.Data.Count
            Add-TestResult "ApiTests" "Customers Response Structure" $true "Found $customerCount customers with valid structure"
            
            # Check customer properties - API returns 'name' field (from CustomerListItemDto)
            if ($customerCount -gt 0) {
                $firstCustomer = $result.Data[0]
                $hasRequiredFields = $firstCustomer.PSObject.Properties.Name -contains "id" -and 
                                   $firstCustomer.PSObject.Properties.Name -contains "name" -and
                                   $firstCustomer.PSObject.Properties.Name -contains "email"
                
                if ($hasRequiredFields) {
                    Add-TestResult "ApiTests" "Customer Data Structure" $true "Customers contain required fields (id, name, email)"
                }
                else {
                    Add-TestResult "ApiTests" "Customer Data Structure" $false "Customers missing required fields"
                }
            }
        }
        else {
            Add-TestResult "ApiTests" "Customers Response Structure" $false "Expected array but got: $($result.Data.GetType().Name)"
        }
    }
    else {
        Add-TestResult "ApiTests" "API /api/customers" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

function Test-ProductsEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test products list
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/products" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API /api/products" $true "Products endpoint accessible"
        
        # Validate response structure
        if ($result.Data -is [array]) {
            $productCount = $result.Data.Count
            Add-TestResult "ApiTests" "Products Response Structure" $true "Found $productCount products with valid structure"
            
            # Check product properties - following ProductDto structure
            if ($productCount -gt 0) {
                $firstProduct = $result.Data[0]
                $hasRequiredFields = $firstProduct.PSObject.Properties.Name -contains "id" -and 
                                   $firstProduct.PSObject.Properties.Name -contains "name" -and
                                   $firstProduct.PSObject.Properties.Name -contains "price" -and
                                   $firstProduct.PSObject.Properties.Name -contains "category" -and
                                   $firstProduct.PSObject.Properties.Name -contains "stockQuantity"
                
                if ($hasRequiredFields) {
                    Add-TestResult "ApiTests" "Product Data Structure" $true "Products contain required fields (id, name, price, category, stockQuantity)"
                }
                else {
                    Add-TestResult "ApiTests" "Product Data Structure" $false "Products missing required fields"
                }
            }
        }
        else {
            Add-TestResult "ApiTests" "Products Response Structure" $false "Expected array but got: $($result.Data.GetType().Name)"
        }
    }
    else {
        Add-TestResult "ApiTests" "API /api/products" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

function Test-AnalyticsEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test analytics endpoint
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/orders/analytics" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API /api/orders/analytics" $true "Analytics endpoint accessible"
        
        # Validate analytics structure - following SalesAnalyticsDto
        $analytics = $result.Data
        if ($analytics) {
            $hasRequiredFields = $analytics.PSObject.Properties.Name -contains "summary" -and
                               $analytics.PSObject.Properties.Name -contains "byStatus" -and
                               $analytics.PSObject.Properties.Name -contains "byRegion"
            
            if ($hasRequiredFields) {
                Add-TestResult "ApiTests" "Analytics Response Structure" $true "Analytics contains required fields (summary, byStatus, byRegion)"
                
                # Check summary structure matches SalesSummaryDto
                if ($analytics.summary -and 
                    $analytics.summary.PSObject.Properties.Name -contains "totalRevenue" -and
                    $analytics.summary.PSObject.Properties.Name -contains "totalOrders" -and
                    $analytics.summary.PSObject.Properties.Name -contains "averageOrderValue") {
                    Add-TestResult "ApiTests" "Analytics Summary Structure" $true "Analytics summary includes totalRevenue, totalOrders, averageOrderValue"
                }
                else {
                    Add-TestResult "ApiTests" "Analytics Summary Structure" $false "Analytics summary missing required fields"
                }
                
                # Check byStatus array structure
                if ($analytics.byStatus -is [array] -and $analytics.byStatus.Count -gt 0) {
                    $firstStatus = $analytics.byStatus[0]
                    if ($firstStatus.PSObject.Properties.Name -contains "status" -and
                        $firstStatus.PSObject.Properties.Name -contains "count" -and
                        $firstStatus.PSObject.Properties.Name -contains "revenue") {
                        Add-TestResult "ApiTests" "Analytics ByStatus Structure" $true "ByStatus contains status, count, revenue fields"
                    }
                    else {
                        Add-TestResult "ApiTests" "Analytics ByStatus Structure" $false "ByStatus missing required fields"
                    }
                }
            }
            else {
                Add-TestResult "ApiTests" "Analytics Response Structure" $false "Analytics missing required fields (summary, byStatus, byRegion)"
            }
        }
        else {
            Add-TestResult "ApiTests" "Analytics Response Structure" $false "Empty analytics response"
        }
    }
    else {
        Add-TestResult "ApiTests" "API /api/orders/analytics" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

function Test-HealthEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/health" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API Health Check" $true "Health endpoint responding"
        
        # Check health status
        if ($result.Data -and $result.Data.status) {
            if ($result.Data.status -eq "Healthy") {
                Add-TestResult "ApiTests" "API Health Status" $true "API reports healthy status"
            }
            else {
                Add-TestResult "ApiTests" "API Health Status" $false "API reports unhealthy status: $($result.Data.status)"
            }
        }
    }
    else {
        Add-TestResult "ApiTests" "API Health Check" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

function Test-InfoEndpoint {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    $result = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/info/version" -TimeoutSec $TimeoutSeconds
    
    if ($result.Success) {
        Add-TestResult "ApiTests" "API Version Info" $true "Version endpoint accessible"
        
        # Validate version info structure
        if ($result.Data -and $result.Data.version) {
            Add-TestResult "ApiTests" "Version Data Structure" $true "Version info contains version field: $($result.Data.version)"
        }
        else {
            Add-TestResult "ApiTests" "Version Data Structure" $false "Version info missing version field"
        }
    }
    else {
        Add-TestResult "ApiTests" "API Version Info" $false "Failed: $($result.Error) (Status: $($result.StatusCode))"
    }
}

# Note: Functions are automatically available when this script is dot-sourced

# Main wrapper function for API testing
function Test-Api {
    param(
        [string]$ApiBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Verbose
    )
    
    Test-ApiEndpoints -ApiBaseUrl $ApiBaseUrl -TimeoutSeconds $TimeoutSeconds -Quick:$Quick -Verbose:$Verbose
}
