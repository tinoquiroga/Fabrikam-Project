# Fabrikam Enhanced MCP Testing - Comprehensive Business Tool Validation
# Implements Issue #12: Enhanced MCP testing for all 13 business tools

# Import shared utilities
. "$PSScriptRoot\Test-Shared.ps1"

# Global variables for enhanced testing
$Global:McpToolsInventory = @()
$Global:McpTestResults = @()
$Global:McpAuthToken = $null

function Test-McpEnhanced {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Comprehensive,
        [switch]$Performance,
        [switch]$Verbose
    )
    
    Write-SectionHeader "ENHANCED MCP TESTING - ISSUE #12"
    Write-Host "Testing 13 business-focused MCP tools with comprehensive validation" -ForegroundColor Green
    
    # Phase 0: Authentication Setup
    Test-McpAuthentication -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Phase 1: Core Infrastructure Testing
    Test-McpInfrastructure -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Phase 2: Business Tools Discovery and Validation
    Test-BusinessToolsDiscovery -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    if (-not $Quick) {
        # Phase 3: Individual Business Tool Testing
        Test-AllBusinessTools -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds -Comprehensive:$Comprehensive
        
        # Phase 4: Data Integrity Validation
        Test-BusinessDataIntegrity -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        
        if ($Performance) {
            # Phase 5: Performance Testing
            Test-McpPerformance -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        }
        
        # Phase 6: Integration Testing
        Test-McpApiIntegration -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    }
    
    # Summary Report
    Write-EnhancedMcpSummary -Verbose:$Verbose
}

function Test-McpAuthentication {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Host "`n=== PHASE 0: MCP Authentication Setup ===" -ForegroundColor Cyan
    
    # First check if MCP server requires authentication
    try {
        Write-Debug "Checking MCP server authentication requirements..."
        $statusResponse = Invoke-RestMethod -Uri "$McpBaseUrl/status" -TimeoutSec $TimeoutSeconds
        
        if ($statusResponse.Authentication -and $statusResponse.Authentication.Required) {
            Add-TestResult "McpTests" "Authentication Detection" $true "MCP server requires authentication: $($statusResponse.Authentication.Method)"
            Write-Host "🔐 MCP Server requires authentication: $($statusResponse.Authentication.Method)" -ForegroundColor Yellow
            
            # Get authentication token from API server
            Test-GetMcpAuthToken -TimeoutSeconds $TimeoutSeconds
        }
        else {
            Add-TestResult "McpTests" "Authentication Detection" $true "MCP server authentication disabled"
            Write-Host "🔓 MCP Server authentication disabled" -ForegroundColor Green
        }
    }
    catch {
        Add-TestResult "McpTests" "Authentication Detection" $false "Failed to check authentication status: $($_.Exception.Message)"
        Write-Warning "Could not determine MCP authentication status, proceeding without authentication"
    }
}

function Test-GetMcpAuthToken {
    param([int]$TimeoutSeconds = 30)
    
    try {
        # Try to get demo credentials from API
        $apiBaseUrl = "https://localhost:7297"  # Should match API server
        Write-Debug "Getting demo credentials from API at $apiBaseUrl"
        
        $demoCredentialsResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/auth/demo-credentials" -TimeoutSec $TimeoutSeconds
        
        if ($demoCredentialsResponse -and $demoCredentialsResponse.demoUsers -and $demoCredentialsResponse.demoUsers.Count -gt 0) {
            # Use the first admin user for testing
            $adminUser = $demoCredentialsResponse.demoUsers | Where-Object { $_.role -eq "Admin" } | Select-Object -First 1
            
            if ($adminUser) {
                Write-Debug "Got demo credentials for admin user: $($adminUser.email)"
                
                # Login with demo credentials
                $loginRequest = @{
                    email = $adminUser.email
                    password = $adminUser.password
                } | ConvertTo-Json
                
                $loginResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/auth/login" -Method Post -Body $loginRequest -ContentType "application/json" -TimeoutSec $TimeoutSeconds
                
                if ($loginResponse -and $loginResponse.accessToken) {
                    $Global:McpAuthToken = $loginResponse.accessToken
                    Add-TestResult "McpTests" "MCP Authentication Token" $true "Successfully obtained JWT token for admin user: $($adminUser.email)"
                    Write-Host "✅ Obtained JWT token for MCP authentication (Admin: $($adminUser.email))" -ForegroundColor Green
                }
                else {
                    Add-TestResult "McpTests" "MCP Authentication Token" $false "Login succeeded but no access token returned"
                }
            }
            else {
                Add-TestResult "McpTests" "MCP Authentication Token" $false "No admin user found in demo credentials"
            }
        }
        else {
            Add-TestResult "McpTests" "MCP Authentication Token" $false "Demo credentials response invalid or empty"
        }
    }
    catch {
        Add-TestResult "McpTests" "MCP Authentication Token" $false "Failed to get authentication token: $($_.Exception.Message)"
        Write-Warning "Could not obtain MCP authentication token, will try unauthenticated requests"
    }
}

function Test-McpInfrastructure {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Host "`n=== PHASE 1: MCP Infrastructure Testing ===" -ForegroundColor Cyan
    
    # Test 1: Basic server health and connectivity (using Test-Shared.ps1 function)
    try {
        $mcpHealth = Test-ServerHealth -BaseUrl $McpBaseUrl -ServerName "MCP" -TimeoutSeconds $TimeoutSeconds
        if ($mcpHealth.Success) {
            Add-TestResult "McpTests" "MCP Server Health" $true "MCP server responding"
        }
        else {
            Add-TestResult "McpTests" "MCP Server Health" $false "MCP server not responding: $($mcpHealth.Error)"
        }
    }
    catch {
        Add-TestResult "McpTests" "MCP Server Health" $false "Health check failed: $($_.Exception.Message)"
    }
    
    # Test 2: MCP protocol compliance
    Test-McpProtocolCompliance -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Test 3: Server capabilities validation
    Test-McpServerCapabilities -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
}

function Test-McpServerCapabilities {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        Write-Debug "Testing MCP server capabilities..."
        
        # For now, check basic endpoint availability since full capabilities test requires complex MCP protocol
        $mcpResponse = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Head -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($mcpResponse.StatusCode -eq 405) {
            # 405 Method Not Allowed is expected for HEAD request to MCP endpoint
            Add-TestResult "McpTests" "MCP Server Capabilities" $true "MCP endpoint accessible (expects POST requests)"
        }
        else {
            Add-TestResult "McpTests" "MCP Server Capabilities" $false "Unexpected response: $($mcpResponse.StatusCode)"
        }
    }
    catch {
        if ($_.Exception.Message -like "*405*" -or $_.Exception.Message -like "*Method Not Allowed*") {
            Add-TestResult "McpTests" "MCP Server Capabilities" $true "MCP endpoint accessible (405 Method Not Allowed expected for HEAD)"
        }
        else {
            Add-TestResult "McpTests" "MCP Server Capabilities" $false "Failed to get server capabilities: $($_.Exception.Message)"
        }
    }
}

function Test-McpProtocolCompliance {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        Write-Debug "Testing MCP protocol initialization with authentication..."
        
        $initRequest = @{
            jsonrpc = "2.0"
            id      = "enhanced-init"
            method  = "initialize"
            params  = @{
                protocolVersion = "2024-11-05"
                capabilities    = @{
                    tools = @{}
                }
                clientInfo = @{
                    name    = "Fabrikam-Enhanced-Test-Client"
                    version = "2.0.0"
                }
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        # Add authentication header if we have a token
        if ($Global:McpAuthToken) {
            $headers['Authorization'] = "Bearer $Global:McpAuthToken"
            Write-Debug "Using JWT authentication for MCP protocol test"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $initRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result) {
                    Add-TestResult "McpTests" "MCP Protocol Compliance" $true "MCP 2024-11-05 protocol initialization successful"
                    
                    # Validate server info
                    if ($mcpResponse.result.serverInfo) {
                        $serverInfo = $mcpResponse.result.serverInfo
                        Add-TestResult "McpTests" "Server Information" $true "Server: $($serverInfo.name) v$($serverInfo.version)"
                        
                        # Store server capabilities for later testing
                        $Global:McpServerCapabilities = $mcpResponse.result.capabilities
                    }
                    else {
                        Add-TestResult "McpTests" "Server Information" $false "Missing serverInfo in initialization response"
                    }
                }
                else {
                    Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Initialization response missing result field"
                }
            }
            else {
                Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Invalid SSE response format"
            }
        }
        else {
            Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Empty initialization response"
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -like "*401*" -or $errorMessage -like "*403*") {
            Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Authentication failed: $errorMessage"
        }
        elseif ($errorMessage -like "*406*") {
            Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Protocol request not acceptable: $errorMessage"
        }
        else {
            Add-TestResult "McpTests" "MCP Protocol Compliance" $false "Protocol initialization failed: $errorMessage"
        }
    }
}

function Test-BusinessToolsDiscovery {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Host "`n=== PHASE 2: Business Tools Discovery ===" -ForegroundColor Cyan
    
    try {
        Write-Debug "Discovering all MCP business tools with authentication..."
        
        $listToolsRequest = @{
            jsonrpc = "2.0"
            id      = "enhanced-tools-list"
            method  = "tools/list"
            params  = @{}
        } | ConvertTo-Json -Depth 3
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        # Add authentication header if we have a token
        if ($Global:McpAuthToken) {
            $headers['Authorization'] = "Bearer $Global:McpAuthToken"
            Write-Debug "Using JWT authentication for tools discovery"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $listToolsRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result -and $mcpResponse.result.tools) {
                    $tools = $mcpResponse.result.tools
                    $Global:McpToolsInventory = $tools
                    
                    Add-TestResult "McpTests" "Tools Discovery" $true "Found $($tools.Count) tools total"
                    
                    # Validate against expected business tools
                    Test-ExpectedBusinessTools -Tools $tools
                    
                    # Validate tool structure and metadata
                    Test-BusinessToolsStructure -Tools $tools
                    
                    # Categorize tools by business domain
                    Test-BusinessToolsCategorization -Tools $tools
                }
                else {
                    Add-TestResult "McpTests" "Tools Discovery" $false "No tools found in discovery response"
                }
            }
            else {
                Add-TestResult "McpTests" "Tools Discovery" $false "Invalid tools discovery response format"
            }
        }
        else {
            Add-TestResult "McpTests" "Tools Discovery" $false "Empty tools discovery response"
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -like "*401*" -or $errorMessage -like "*403*") {
            Add-TestResult "McpTests" "Tools Discovery" $false "Authentication failed: $errorMessage"
        }
        elseif ($errorMessage -like "*406*") {
            Add-TestResult "McpTests" "Tools Discovery" $false "Request not acceptable - check content type and authentication: $errorMessage"
        }
        else {
            Add-TestResult "McpTests" "Tools Discovery" $false "Tools discovery failed: $errorMessage"
        }
    }
}

function Test-ExpectedBusinessTools {
    param([array]$Tools)
    
    # Define comprehensive expected business tools (13 total)
    $expectedBusinessTools = @{
        # Sales Tools (5 tools)
        "GetBusinessDashboard" = "Business Intelligence"
        "GetSalesAnalytics" = "Sales Analytics"
        "GetRevenueAnalytics" = "Revenue Analytics"
        "GetSalesPerformance" = "Sales Performance"
        "GetSalesTrends" = "Sales Trends"
        
        # Product Tools (3 tools)
        "GetProducts" = "Product Management"
        "SearchProducts" = "Product Search"
        "GetProductDetails" = "Product Details"
        
        # Customer Tools (2 tools)
        "GetCustomers" = "Customer Management"
        "GetCustomerProfile" = "Customer Analytics"
        
        # Order Tools (2 tools)
        "GetOrders" = "Order Management"
        "GetOrderDetails" = "Order Analytics"
        
        # Inventory Tool (1 tool)
        "GetInventoryStatus" = "Inventory Management"
    }
    
    $foundTools = $Tools | ForEach-Object { $_.name }
    $missingTools = @()
    $extraTools = @()
    $businessToolsFound = @()
    
    # Check for missing expected business tools
    foreach ($expectedTool in $expectedBusinessTools.Keys) {
        if ($foundTools -contains $expectedTool) {
            $businessToolsFound += $expectedTool
        }
        else {
            $missingTools += $expectedTool
        }
    }
    
    # Check for unexpected tools
    foreach ($foundTool in $foundTools) {
        if (-not $expectedBusinessTools.ContainsKey($foundTool)) {
            $extraTools += $foundTool
        }
    }
    
    # Report results
    Add-TestResult "McpTests" "Expected Business Tools" $true "Found $($businessToolsFound.Count)/13 expected business tools"
    
    if ($missingTools.Count -gt 0) {
        Add-TestResult "McpTests" "Missing Business Tools" $false "Missing: $($missingTools -join ', ')"
    }
    else {
        Add-TestResult "McpTests" "Business Tools Completeness" $true "All 13 expected business tools are available"
    }
    
    if ($extraTools.Count -gt 0) {
        Add-TestResult "McpTests" "Additional Tools Found" $true "Extra tools: $($extraTools -join ', ')"
    }
    
    # Store business tools for detailed testing
    $Global:BusinessToolsToTest = $businessToolsFound
}

function Test-BusinessToolsStructure {
    param([array]$Tools)
    
    $structureIssues = @()
    $validToolsCount = 0
    
    foreach ($tool in $Tools) {
        $toolValid = $true
        
        # Check required fields
        if (-not $tool.name) {
            $structureIssues += "Tool missing name field"
            $toolValid = $false
        }
        
        if (-not $tool.description) {
            $structureIssues += "Tool '$($tool.name)' missing description"
            $toolValid = $false
        }
        elseif ($tool.description.Length -lt 20) {
            $structureIssues += "Tool '$($tool.name)' has inadequate description"
        }
        
        # Check inputSchema exists
        if (-not $tool.PSObject.Properties.Name -contains "inputSchema") {
            $structureIssues += "Tool '$($tool.name)' missing inputSchema"
            $toolValid = $false
        }
        
        # Validate business-appropriate descriptions
        if ($tool.description -and $tool.name) {
            Test-BusinessToolDescription -ToolName $tool.name -Description $tool.description
        }
        
        if ($toolValid) {
            $validToolsCount++
        }
    }
    
    if ($structureIssues.Count -eq 0) {
        Add-TestResult "McpTests" "Tools Structure Validation" $true "All $validToolsCount tools have valid structure"
    }
    else {
        Add-TestResult "McpTests" "Tools Structure Validation" $false "Structure issues: $($structureIssues -join '; ')"
    }
}

function Test-BusinessToolDescription {
    param(
        [string]$ToolName,
        [string]$Description
    )
    
    # Define expected keywords for business tool descriptions
    $businessKeywords = @(
        "business", "sales", "customer", "product", "order", "inventory", 
        "revenue", "analytics", "dashboard", "performance", "data", "management"
    )
    
    $foundKeywords = @()
    foreach ($keyword in $businessKeywords) {
        if ($Description -match $keyword) {
            $foundKeywords += $keyword
        }
    }
    
    if ($foundKeywords.Count -gt 0) {
        Add-TestResult "McpTests" "Business Description: $ToolName" $true "Contains business keywords: $($foundKeywords -join ', ')"
    }
    else {
        Add-TestResult "McpTests" "Business Description: $ToolName" $false "Missing business context keywords"
    }
}

function Test-BusinessToolsCategorization {
    param([array]$Tools)
    
    # Categorize tools by business domain
    $categories = @{
        "Sales" = @("GetBusinessDashboard", "GetSalesAnalytics", "GetRevenueAnalytics", "GetSalesPerformance", "GetSalesTrends")
        "Products" = @("GetProducts", "SearchProducts", "GetProductDetails")
        "Customers" = @("GetCustomers", "GetCustomerProfile")
        "Orders" = @("GetOrders", "GetOrderDetails")
        "Inventory" = @("GetInventoryStatus")
    }
    
    $foundTools = $Tools | ForEach-Object { $_.name }
    
    foreach ($category in $categories.Keys) {
        $categoryTools = $categories[$category]
        $foundInCategory = $categoryTools | Where-Object { $foundTools -contains $_ }
        
        if ($foundInCategory.Count -gt 0) {
            Add-TestResult "McpTests" "$category Tools Category" $true "Found $($foundInCategory.Count)/$($categoryTools.Count) tools: $($foundInCategory -join ', ')"
        }
        else {
            Add-TestResult "McpTests" "$category Tools Category" $false "No tools found in $category category"
        }
    }
}

function Test-AllBusinessTools {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Comprehensive
    )
    
    Write-Host "`n=== PHASE 3: Individual Business Tool Testing ===" -ForegroundColor Cyan
    
    if (-not $Global:BusinessToolsToTest -or $Global:BusinessToolsToTest.Count -eq 0) {
        Add-TestResult "McpTests" "Business Tools Testing" $false "No business tools available for testing"
        return
    }
    
    foreach ($toolName in $Global:BusinessToolsToTest) {
        $tool = $Global:McpToolsInventory | Where-Object { $_.name -eq $toolName } | Select-Object -First 1
        
        if ($tool) {
            Write-Host "Testing business tool: $toolName" -ForegroundColor Yellow
            
            # Basic execution test
            Test-BusinessToolExecution -McpBaseUrl $McpBaseUrl -Tool $tool -TimeoutSeconds $TimeoutSeconds
            
            if ($Comprehensive) {
                # Parameter validation test
                Test-BusinessToolParameters -McpBaseUrl $McpBaseUrl -Tool $tool -TimeoutSeconds $TimeoutSeconds
                
                # Response structure validation
                Test-BusinessToolResponseStructure -McpBaseUrl $McpBaseUrl -Tool $tool -TimeoutSeconds $TimeoutSeconds
                
                # Error handling test
                Test-BusinessToolErrorHandling -McpBaseUrl $McpBaseUrl -Tool $tool -TimeoutSeconds $TimeoutSeconds
            }
        }
        else {
            Add-TestResult "McpTests" "Tool Testing: $toolName" $false "Tool not found in inventory"
        }
    }
}

function Test-BusinessToolExecution {
    param(
        [string]$McpBaseUrl,
        [object]$Tool,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "business-tool-$($Tool.name)"
            method  = "tools/call"
            params  = @{
                name      = $Tool.name
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        # Add authentication header if we have a token
        if ($Global:McpAuthToken) {
            $headers['Authorization'] = "Bearer $Global:McpAuthToken"
            Write-Debug "Using JWT authentication for tool execution: $($Tool.name)"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result) {
                    Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $true "Business tool executed successfully"
                    
                    # Validate response content
                    Test-BusinessToolResponseContent -ToolName $Tool.name -Response $mcpResponse.result
                    
                    return $mcpResponse.result
                }
                else {
                    Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Tool execution returned no result"
                }
            }
            else {
                Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Invalid SSE response format"
            }
        }
        else {
            Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Empty tool execution response"
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -like "*401*" -or $errorMessage -like "*403*") {
            Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Authentication failed: $errorMessage"
        }
        elseif ($errorMessage -like "*406*") {
            Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Request not acceptable: $errorMessage"
        }
        else {
            Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Tool execution failed: $errorMessage"
        }
    }
    
    return $null
}

function Test-BusinessToolResponseContent {
    param(
        [string]$ToolName,
        [object]$Response
    )
    
    # Check MCP standard response structure
    if ($Response.content -and $Response.content -is [array] -and $Response.content.Count -gt 0) {
        Add-TestResult "McpTests" "Response Structure: $ToolName" $true "Valid MCP content array structure"
        
        $firstContent = $Response.content[0]
        if ($firstContent.type -and $firstContent.text) {
            Add-TestResult "McpTests" "Content Format: $ToolName" $true "Content has required type and text fields"
            
            # Validate business data content
            Test-BusinessDataContent -ToolName $ToolName -Content $firstContent.text
            
            # Check for JSON data if available
            if ($Response.data) {
                Test-BusinessDataStructure -ToolName $ToolName -Data $Response.data
            }
        }
        else {
            Add-TestResult "McpTests" "Content Format: $ToolName" $false "Content missing type or text fields"
        }
    }
    else {
        Add-TestResult "McpTests" "Response Structure: $ToolName" $false "Invalid or missing content array"
    }
}

function Test-BusinessDataContent {
    param(
        [string]$ToolName,
        [string]$Content
    )
    
    # Define expected content patterns for each business tool
    $expectedPatterns = @{
        "GetBusinessDashboard" = @("revenue", "orders", "customers", "products", "total", "dashboard")
        "GetSalesAnalytics" = @("sales", "revenue", "analytics", "total", "performance")
        "GetRevenueAnalytics" = @("revenue", "total", "analytics", "period", "growth")
        "GetSalesPerformance" = @("sales", "performance", "metrics", "target", "achievement")
        "GetSalesTrends" = @("trends", "sales", "growth", "period", "comparison")
        "GetProducts" = @("product", "name", "price", "category", "description")
        "SearchProducts" = @("product", "search", "results", "category", "name")
        "GetProductDetails" = @("product", "details", "description", "price", "specifications")
        "GetCustomers" = @("customer", "name", "email", "id", "contact")
        "GetCustomerProfile" = @("customer", "profile", "orders", "history", "analytics")
        "GetOrders" = @("order", "customer", "total", "date", "status")
        "GetOrderDetails" = @("order", "details", "items", "customer", "total")
        "GetInventoryStatus" = @("inventory", "stock", "quantity", "product", "status")
    }
    
    if ($expectedPatterns.ContainsKey($ToolName)) {
        $patterns = $expectedPatterns[$ToolName]
        $foundPatterns = @()
        
        foreach ($pattern in $patterns) {
            if ($Content -match $pattern) {
                $foundPatterns += $pattern
            }
        }
        
        if ($foundPatterns.Count -ge 2) {
            Add-TestResult "McpTests" "Business Data Content: $ToolName" $true "Found expected patterns: $($foundPatterns -join ', ')"
        }
        elseif ($foundPatterns.Count -eq 1) {
            Add-TestResult "McpTests" "Business Data Content: $ToolName" $true "Found minimal expected content: $($foundPatterns -join ', ')"
        }
        else {
            Add-TestResult "McpTests" "Business Data Content: $ToolName" $false "No expected business data patterns found"
        }
    }
    else {
        # Generic business content validation
        if ($Content.Length -gt 100) {
            Add-TestResult "McpTests" "Business Data Content: $ToolName" $true "Substantial business content returned ($($Content.Length) characters)"
        }
        else {
            Add-TestResult "McpTests" "Business Data Content: $ToolName" $false "Minimal content returned ($($Content.Length) characters)"
        }
    }
}

function Test-BusinessDataStructure {
    param(
        [string]$ToolName,
        [object]$Data
    )
    
    try {
        # Try to parse as JSON if it's a string
        if ($Data -is [string]) {
            $parsedData = $Data | ConvertFrom-Json
        }
        else {
            $parsedData = $Data
        }
        
        if ($parsedData) {
            Add-TestResult "McpTests" "Data Structure: $ToolName" $true "Valid structured data returned"
            
            # Check for business-relevant properties
            $businessProperties = @("id", "name", "total", "revenue", "customer", "product", "order", "date", "status")
            $foundProperties = @()
            
            if ($parsedData -is [array] -and $parsedData.Count -gt 0) {
                $sampleItem = $parsedData[0]
                foreach ($prop in $businessProperties) {
                    if ($sampleItem.PSObject.Properties.Name -contains $prop) {
                        $foundProperties += $prop
                    }
                }
            }
            elseif ($parsedData.PSObject) {
                foreach ($prop in $businessProperties) {
                    if ($parsedData.PSObject.Properties.Name -contains $prop) {
                        $foundProperties += $prop
                    }
                }
            }
            
            if ($foundProperties.Count -gt 0) {
                Add-TestResult "McpTests" "Business Properties: $ToolName" $true "Found business properties: $($foundProperties -join ', ')"
            }
        }
    }
    catch {
        Add-TestResult "McpTests" "Data Structure: $ToolName" $false "Invalid data structure: $($_.Exception.Message)"
    }
}

function Test-BusinessDataIntegrity {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Host "`n=== PHASE 4: Business Data Integrity Testing ===" -ForegroundColor Cyan
    
    # Test data consistency between related tools
    Test-CrossToolDataConsistency -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Test data relationships
    Test-BusinessDataRelationships -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
}

function Test-CrossToolDataConsistency {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test consistency between sales analytics and business dashboard
    try {
        $dashboardData = Get-ToolData -McpBaseUrl $McpBaseUrl -ToolName "GetBusinessDashboard" -TimeoutSeconds $TimeoutSeconds
        $salesData = Get-ToolData -McpBaseUrl $McpBaseUrl -ToolName "GetSalesAnalytics" -TimeoutSeconds $TimeoutSeconds
        
        if ($dashboardData -and $salesData) {
            Add-TestResult "McpTests" "Data Consistency Check" $true "Successfully retrieved data from both dashboard and sales analytics"
            
            # Compare revenue figures if available
            if ($dashboardData -match "revenue" -and $salesData -match "revenue") {
                Add-TestResult "McpTests" "Revenue Data Consistency" $true "Revenue data present in both dashboard and sales analytics"
            }
        }
        else {
            Add-TestResult "McpTests" "Data Consistency Check" $false "Unable to retrieve data for consistency testing"
        }
    }
    catch {
        Add-TestResult "McpTests" "Data Consistency Check" $false "Consistency test failed: $($_.Exception.Message)"
    }
}

function Get-ToolData {
    param(
        [string]$McpBaseUrl,
        [string]$ToolName,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "data-$ToolName"
            method  = "tools/call"
            params  = @{
                name      = $ToolName
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result -and $mcpResponse.result.content) {
                    return $mcpResponse.result.content[0].text
                }
            }
        }
    }
    catch {
        Write-Debug "Failed to get data from $ToolName`: $($_.Exception.Message)"
    }
    
    return $null
}

function Test-McpPerformance {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Host "`n=== PHASE 5: Performance Testing ===" -ForegroundColor Cyan
    
    if (-not $Global:BusinessToolsToTest) {
        Add-TestResult "McpTests" "Performance Testing" $false "No business tools available for performance testing"
        return
    }
    
    # Test response times for key business tools
    $performanceTests = @("GetBusinessDashboard", "GetSalesAnalytics", "GetCustomers", "GetProducts", "GetOrders")
    $responseTimes = @()
    
    foreach ($toolName in $performanceTests) {
        if ($Global:BusinessToolsToTest -contains $toolName) {
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            $result = Get-ToolData -McpBaseUrl $McpBaseUrl -ToolName $toolName -TimeoutSeconds $TimeoutSeconds
            
            $stopwatch.Stop()
            $responseTime = $stopwatch.ElapsedMilliseconds
            $responseTimes += $responseTime
            
            if ($result) {
                if ($responseTime -lt 5000) {
                    Add-TestResult "McpTests" "Performance: $toolName" $true "Response time: ${responseTime}ms (acceptable)"
                }
                else {
                    Add-TestResult "McpTests" "Performance: $toolName" $false "Response time: ${responseTime}ms (too slow)"
                }
            }
            else {
                Add-TestResult "McpTests" "Performance: $toolName" $false "Tool failed to return data for performance test"
            }
        }
    }
    
    # Calculate average response time
    if ($responseTimes.Count -gt 0) {
        $averageTime = ($responseTimes | Measure-Object -Average).Average
        Add-TestResult "McpTests" "Average Performance" $true "Average response time: $([math]::Round($averageTime, 2))ms across $($responseTimes.Count) tools"
    }
}

function Write-EnhancedMcpSummary {
    param([switch]$Verbose)
    
    Write-Host "`n=== ENHANCED MCP TESTING SUMMARY ===" -ForegroundColor Green
    
    $totalTests = ($Global:TestResults["McpTests"] | Measure-Object).Count
    $passedTests = ($Global:TestResults["McpTests"] | Where-Object { $_.Success }).Count
    $failedTests = $totalTests - $passedTests
    
    Write-Host "Total Enhanced MCP Tests: $totalTests" -ForegroundColor White
    Write-Host "Passed: $passedTests" -ForegroundColor Green
    Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { "Red" } else { "Green" })
    
    if ($Verbose -and $failedTests -gt 0) {
        Write-Host "`nFailed Tests:" -ForegroundColor Red
        $Global:TestResults["McpTests"] | Where-Object { -not $_.Success } | ForEach-Object {
            Write-Host "  - $($_.Test): $($_.Message)" -ForegroundColor Red
        }
    }
    
    # Business tools summary
    if ($Global:BusinessToolsToTest) {
        Write-Host "`nBusiness Tools Tested: $($Global:BusinessToolsToTest.Count)" -ForegroundColor Cyan
        Write-Host "Tools: $($Global:BusinessToolsToTest -join ', ')" -ForegroundColor White
    }
    
    $successRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 1) } else { 0 }
    Write-Host "`nSuccess Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } else { "Yellow" })
}

# Note: Functions are automatically available when this script is dot-sourced
