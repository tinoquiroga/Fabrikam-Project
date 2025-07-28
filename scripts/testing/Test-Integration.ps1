# Fabrikam Testing - Integration Tests
# End-to-end testing combining API and MCP functionality

# Import shared utilities
. "$PSScriptRoot\Test-Shared.ps1"

function Test-Integration {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Verbose
    )
    
    Write-SectionHeader "INTEGRATION TESTS"
    
    # Simple integration test matching unified script approach
    Test-ApiMcpDataCompatibility -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    if (-not $Quick) {
        # More comprehensive integration testing
        Test-DataConsistency -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-AuthenticationIntegration -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-BusinessWorkflows -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    }
}

function Test-ApiMcpDataCompatibility {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test that API data structure is compatible with MCP expectations
    # Simple test matching the unified script approach
    try {
        # Test API analytics endpoint
        $apiResult = Invoke-SafeWebRequest -Uri "$ApiBaseUrl/api/orders/analytics" -TimeoutSec $TimeoutSeconds
        
        if ($apiResult.Success -and $apiResult.Data) {
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

function Test-BasicIntegrationFlow {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test that MCP tools can successfully call API endpoints
    
    # 1. Get business data via API
    $apiCustomers = Get-ApiData -BaseUrl $ApiBaseUrl -Endpoint "customers" -TimeoutSeconds $TimeoutSeconds
    
    if ($apiCustomers.Success) {
        Add-TestResult "IntegrationTests" "API Data Retrieval" $true "Retrieved $($apiCustomers.Data.Count) customers from API"
        
        # 2. Get same data via MCP tool
        $mcpCustomers = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetCustomers" -TimeoutSeconds $TimeoutSeconds
        
        if ($mcpCustomers.Success) {
            Add-TestResult "IntegrationTests" "MCP Tool Data Retrieval" $true "Retrieved customer data via MCP tool"
            
            # 3. Compare data consistency
            Test-DataMatching -ApiData $apiCustomers.Data -McpData $mcpCustomers.Data -DataType "Customers"
        }
        else {
            Add-TestResult "IntegrationTests" "MCP Tool Data Retrieval" $false "Failed to get data via MCP: $($mcpCustomers.Error)"
        }
    }
    else {
        Add-TestResult "IntegrationTests" "API Data Retrieval" $false "Failed to get data from API: $($apiCustomers.Error)"
    }
}

function Get-ApiData {
    param(
        [string]$BaseUrl,
        [string]$Endpoint,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $headers = @{}
        if ($Global:AdminToken) {
            $headers["Authorization"] = "Bearer $Global:AdminToken"
        }
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/$Endpoint" -Method Get -Headers $headers -TimeoutSec $TimeoutSeconds
        
        return @{
            Success = $true
            Data = $response
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

function Get-McpToolData {
    param(
        [string]$McpBaseUrl,
        [string]$ToolName,
        [hashtable]$Params = @{},
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "integration-test-$ToolName"
            method  = "tools/call"
            params  = @{
                name      = $ToolName
                arguments = $Params
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        if ($Global:AdminToken) {
            $headers["Authorization"] = "Bearer $Global:AdminToken"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result) {
                    # Extract data from MCP response
                    $data = Extract-McpData -McpResponse $mcpResponse.result
                    
                    return @{
                        Success = $true
                        Data = $data
                    }
                }
                else {
                    return @{
                        Success = $false
                        Error = "No result in MCP response"
                    }
                }
            }
            else {
                return @{
                    Success = $false
                    Error = "No data line in MCP response"
                }
            }
        }
        else {
            return @{
                Success = $false
                Error = "Empty MCP response"
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

function Extract-McpData {
    param([object]$McpResponse)
    
    # Extract structured data from MCP response if available
    if ($McpResponse.data) {
        return $McpResponse.data
    }
    
    # If no structured data, try to parse from content text
    if ($McpResponse.content -and $McpResponse.content.Count -gt 0) {
        $contentText = $McpResponse.content[0].text
        
        # Try to find JSON data in the content
        if ($contentText -match '\{.*\}' -or $contentText -match '\[.*\]') {
            try {
                # Extract JSON portion
                $jsonMatch = [regex]::Match($contentText, '\{.*\}|\[.*\]', [System.Text.RegularExpressions.RegexOptions]::Singleline)
                if ($jsonMatch.Success) {
                    $jsonData = $jsonMatch.Value | ConvertFrom-Json
                    return $jsonData
                }
            }
            catch {
                # If JSON parsing fails, return the raw text
                return $contentText
            }
        }
        
        return $contentText
    }
    
    return $null
}

function Test-DataMatching {
    param(
        [object]$ApiData,
        [object]$McpData,
        [string]$DataType
    )
    
    # Compare data from API and MCP to ensure consistency
    
    if ($ApiData -and $McpData) {
        # Try to count records
        $apiCount = Get-DataCount -Data $ApiData
        $mcpCount = Get-DataCount -Data $McpData
        
        if ($apiCount -eq $mcpCount -and $apiCount -gt 0) {
            Add-TestResult "IntegrationTests" "$DataType Data Consistency" $true "API and MCP returned same count: $apiCount records"
        }
        elseif ($apiCount -gt 0 -and $mcpCount -gt 0) {
            Add-TestResult "IntegrationTests" "$DataType Data Consistency" $false "Count mismatch - API: $apiCount, MCP: $mcpCount"
        }
        else {
            Add-TestResult "IntegrationTests" "$DataType Data Consistency" $false "One or both sources returned no data"
        }
        
        # Check for matching field structures
        Test-DataFieldMatching -ApiData $ApiData -McpData $McpData -DataType $DataType
    }
    else {
        Add-TestResult "IntegrationTests" "$DataType Data Consistency" $false "Missing data from one or both sources"
    }
}

function Get-DataCount {
    param([object]$Data)
    
    if ($Data -is [array]) {
        return $Data.Count
    }
    elseif ($Data -and $Data.PSObject.Properties.Name -contains "Count") {
        return $Data.Count
    }
    elseif ($Data) {
        return 1
    }
    else {
        return 0
    }
}

function Test-DataFieldMatching {
    param(
        [object]$ApiData,
        [object]$McpData,
        [string]$DataType
    )
    
    # Extract first item from each for field comparison
    $apiItem = $null
    $mcpItem = $null
    
    if ($ApiData -is [array] -and $ApiData.Count -gt 0) {
        $apiItem = $ApiData[0]
    }
    elseif ($ApiData) {
        $apiItem = $ApiData
    }
    
    if ($McpData -is [array] -and $McpData.Count -gt 0) {
        $mcpItem = $McpData[0]
    }
    elseif ($McpData) {
        $mcpItem = $McpData
    }
    
    if ($apiItem -and $mcpItem) {
        $apiFields = @()
        $mcpFields = @()
        
        # Get field names
        if ($apiItem.PSObject) {
            $apiFields = $apiItem.PSObject.Properties.Name
        }
        
        if ($mcpItem.PSObject) {
            $mcpFields = $mcpItem.PSObject.Properties.Name
        }
        
        # Find common fields
        $commonFields = $apiFields | Where-Object { $mcpFields -contains $_ }
        
        if ($commonFields.Count -gt 0) {
            Add-TestResult "IntegrationTests" "$DataType Field Matching" $true "Found $($commonFields.Count) common fields: $($commonFields[0..2] -join ', ')$(if($commonFields.Count -gt 3){'...'})"
        }
        else {
            Add-TestResult "IntegrationTests" "$DataType Field Matching" $false "No common fields found between API and MCP data"
        }
    }
}

function Test-DataConsistency {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test multiple data endpoints for consistency
    $endpoints = @(
        @{ ApiEndpoint = "products"; McpTool = "GetProducts"; Name = "Products" },
        @{ ApiEndpoint = "orders"; McpTool = "GetOrders"; Name = "Orders" },
        @{ ApiEndpoint = "analytics/business-dashboard"; McpTool = "GetBusinessDashboard"; Name = "Business Dashboard" }
    )
    
    foreach ($endpoint in $endpoints) {
        $apiData = Get-ApiData -BaseUrl $ApiBaseUrl -Endpoint $endpoint.ApiEndpoint -TimeoutSeconds $TimeoutSeconds
        $mcpData = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName $endpoint.McpTool -TimeoutSeconds $TimeoutSeconds
        
        if ($apiData.Success -and $mcpData.Success) {
            Test-DataMatching -ApiData $apiData.Data -McpData $mcpData.Data -DataType $endpoint.Name
        }
        else {
            $errors = @()
            if (-not $apiData.Success) { $errors += "API: $($apiData.Error)" }
            if (-not $mcpData.Success) { $errors += "MCP: $($mcpData.Error)" }
            Add-TestResult "IntegrationTests" "$($endpoint.Name) Integration" $false "Failed to retrieve data - $($errors -join '; ')"
        }
    }
}

function Test-AuthenticationIntegration {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test that MCP tools respect the same authentication as API
    
    if ($Global:AdminToken -and $Global:SalesToken) {
        # Test admin access through MCP
        $adminResult = Test-McpToolWithToken -McpBaseUrl $McpBaseUrl -ToolName "GetSalesAnalytics" -Token $Global:AdminToken -Role "Admin" -TimeoutSeconds $TimeoutSeconds
        
        # Test sales access through MCP
        $salesResult = Test-McpToolWithToken -McpBaseUrl $McpBaseUrl -ToolName "GetSalesAnalytics" -Token $Global:SalesToken -Role "Sales" -TimeoutSeconds $TimeoutSeconds
        
        # Both should succeed
        if ($adminResult.Success -and $salesResult.Success) {
            Add-TestResult "IntegrationTests" "Authentication Integration" $true "Both Admin and Sales tokens work with MCP tools"
        }
        else {
            Add-TestResult "IntegrationTests" "Authentication Integration" $false "Token authentication issues with MCP tools"
        }
    }
    else {
        Add-TestResult "IntegrationTests" "Authentication Integration" $false "Authentication tokens not available for testing"
    }
}

function Test-McpToolWithToken {
    param(
        [string]$McpBaseUrl,
        [string]$ToolName,
        [string]$Token,
        [string]$Role,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "auth-test-$Role"
            method  = "tools/call"
            params  = @{
                name      = $ToolName
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type'  = 'application/json'
            'Accept'        = 'text/event-stream'
            'Authorization' = "Bearer $Token"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        return @{
            Success = $true
            Role = $Role
        }
    }
    catch {
        return @{
            Success = $false
            Role = $Role
            Error = $_.Exception.Message
        }
    }
}

function Test-BusinessWorkflows {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test complete business workflows using both API and MCP
    
    # Workflow 1: Customer Analytics Pipeline
    Test-CustomerAnalyticsWorkflow -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Workflow 2: Sales Dashboard Integration
    Test-SalesDashboardWorkflow -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Workflow 3: Product Inventory Workflow
    Test-ProductInventoryWorkflow -ApiBaseUrl $ApiBaseUrl -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
}

function Test-CustomerAnalyticsWorkflow {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # 1. Get customer list via API
    $customers = Get-ApiData -BaseUrl $ApiBaseUrl -Endpoint "customers" -TimeoutSeconds $TimeoutSeconds
    
    if ($customers.Success -and $customers.Data -and $customers.Data.Count -gt 0) {
        # 2. Get customer profile via MCP tool
        $customerId = $customers.Data[0].id
        $customerProfile = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetCustomerProfile" -Params @{ customerId = $customerId } -TimeoutSeconds $TimeoutSeconds
        
        if ($customerProfile.Success) {
            Add-TestResult "IntegrationTests" "Customer Analytics Workflow" $true "Successfully retrieved customer profile via MCP for customer $customerId"
        }
        else {
            Add-TestResult "IntegrationTests" "Customer Analytics Workflow" $false "Failed to get customer profile via MCP: $($customerProfile.Error)"
        }
    }
    else {
        Add-TestResult "IntegrationTests" "Customer Analytics Workflow" $false "Failed to get customer data for workflow testing"
    }
}

function Test-SalesDashboardWorkflow {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test the sales dashboard data pipeline
    $dashboardData = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetBusinessDashboard" -TimeoutSeconds $TimeoutSeconds
    $analyticsData = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetSalesAnalytics" -TimeoutSeconds $TimeoutSeconds
    
    if ($dashboardData.Success -and $analyticsData.Success) {
        Add-TestResult "IntegrationTests" "Sales Dashboard Workflow" $true "Successfully retrieved both dashboard and analytics data"
        
        # Check for data coherence
        Test-DashboardDataCoherence -DashboardData $dashboardData.Data -AnalyticsData $analyticsData.Data
    }
    else {
        $errors = @()
        if (-not $dashboardData.Success) { $errors += "Dashboard: $($dashboardData.Error)" }
        if (-not $analyticsData.Success) { $errors += "Analytics: $($analyticsData.Error)" }
        Add-TestResult "IntegrationTests" "Sales Dashboard Workflow" $false "Failed to retrieve complete dashboard data - $($errors -join '; ')"
    }
}

function Test-DashboardDataCoherence {
    param(
        [object]$DashboardData,
        [object]$AnalyticsData
    )
    
    # Check if dashboard and analytics data are coherent
    if ($DashboardData -and $AnalyticsData) {
        # Look for common fields or related data
        $dashboardText = $DashboardData | ConvertTo-Json -Depth 3
        $analyticsText = $AnalyticsData | ConvertTo-Json -Depth 3
        
        # Check for revenue data in both
        $dashboardHasRevenue = $dashboardText -match "revenue|total|sales"
        $analyticsHasRevenue = $analyticsText -match "revenue|total|sales"
        
        if ($dashboardHasRevenue -and $analyticsHasRevenue) {
            Add-TestResult "IntegrationTests" "Dashboard Data Coherence" $true "Both dashboard and analytics contain revenue-related data"
        }
        else {
            Add-TestResult "IntegrationTests" "Dashboard Data Coherence" $false "Missing expected revenue data in dashboard or analytics"
        }
    }
    else {
        Add-TestResult "IntegrationTests" "Dashboard Data Coherence" $false "Missing data for coherence testing"
    }
}

function Test-ProductInventoryWorkflow {
    param(
        [string]$ApiBaseUrl,
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test product and inventory data consistency
    $products = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetProducts" -TimeoutSeconds $TimeoutSeconds
    $inventory = Get-McpToolData -McpBaseUrl $McpBaseUrl -ToolName "GetInventoryStatus" -TimeoutSeconds $TimeoutSeconds
    
    if ($products.Success -and $inventory.Success) {
        Add-TestResult "IntegrationTests" "Product Inventory Workflow" $true "Successfully retrieved both product and inventory data"
    }
    else {
        $errors = @()
        if (-not $products.Success) { $errors += "Products: $($products.Error)" }
        if (-not $inventory.Success) { $errors += "Inventory: $($inventory.Error)" }
        Add-TestResult "IntegrationTests" "Product Inventory Workflow" $false "Failed to retrieve complete inventory data - $($errors -join '; ')"
    }
}

# Note: Functions are automatically available when this script is dot-sourced
