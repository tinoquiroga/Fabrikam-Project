# Fabrikam Testing - MCP Server and Tools
# Comprehensive testing for MCP server functionality, tools, and authentication

# Import shared utilities
. "$PSScriptRoot\Test-Shared.ps1"

function Test-McpServer {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30,
        [switch]$Quick,
        [switch]$Verbose
    )
    
    Write-SectionHeader "MCP SERVER TESTS"
    
    # Basic server health and connectivity
    Test-McpServerHealth -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    # Basic endpoint availability check (like unified script does)
    Test-McpEndpointAvailability -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    
    if (-not $Quick) {
        # Comprehensive MCP protocol testing (complex, often fails in dev)
        Test-McpProtocol -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-McpToolsDiscovery -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-McpToolsExecution -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
        Test-McpAuthentication -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
    }
}

function Test-McpEndpointAvailability {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test basic MCP endpoint availability (matching unified script behavior)
    try {
        Write-Debug "Testing basic MCP endpoint availability..."
        $mcpResponse = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Head -TimeoutSec $TimeoutSeconds -ErrorAction SilentlyContinue
        if ($mcpResponse.StatusCode -eq 405) {
            # 405 Method Not Allowed is expected for HEAD request to MCP endpoint
            Add-TestResult "McpTests" "MCP Endpoint Availability" $true "MCP endpoint accessible (expects POST requests)"
        }
        elseif ($mcpResponse.StatusCode -eq 200) {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $true "MCP endpoint accessible"
        }
        else {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $false "Unexpected response from MCP endpoint: $($mcpResponse.StatusCode)"
        }
    }
    catch {
        # Try with GET to see if endpoint exists
        try {
            $getResponse = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Get -TimeoutSec $TimeoutSeconds -ErrorAction SilentlyContinue
            if ($getResponse.StatusCode -eq 405) {
                Add-TestResult "McpTests" "MCP Endpoint Availability" $true "MCP endpoint accessible (method restrictions apply)"
            }
            else {
                Add-TestResult "McpTests" "MCP Endpoint Availability" $false "MCP endpoint accessibility unclear: $($getResponse.StatusCode)"
            }
        }
        catch {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $false "Cannot reach MCP endpoint: $($_.Exception.Message)"
        }
    }
}

function Test-McpServerHealth {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test MCP status endpoint (MCP servers use /status not /health)
    $healthResult = Test-ServerHealth -BaseUrl $McpBaseUrl -ServerName "MCP" -TimeoutSeconds $TimeoutSeconds
    
    if ($healthResult.Success) {
        Add-TestResult "McpTests" "MCP Server Health" $true "MCP server status endpoint responding"
        
        # Check status response structure
        if ($healthResult.Response -and $healthResult.Response.status) {
            Add-TestResult "McpTests" "MCP Health Status" $true "Status: $($healthResult.Response.status) - $($healthResult.Response.service)"
        }
        else {
            Add-TestResult "McpTests" "MCP Health Status" $false "Status endpoint responded but missing status field"
        }
    }
    else {
        Add-TestResult "McpTests" "MCP Server Health" $false "Health check failed: $($healthResult.Error)"
    }
    
    # Test MCP endpoint availability
    try {
        $mcpResponse = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Head -TimeoutSec 5 -SkipCertificateCheck -ErrorAction SilentlyContinue
        if ($mcpResponse.StatusCode -eq 405) {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $true "MCP endpoint accessible (expects POST requests)"
        }
        else {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $false "Unexpected response: $($mcpResponse.StatusCode)"
        }
    }
    catch {
        if ($_.Exception.Message -like "*405*") {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $true "MCP endpoint accessible (405 Method Not Allowed expected for HEAD)"
        }
        else {
            Add-TestResult "McpTests" "MCP Endpoint Availability" $false "MCP endpoint not accessible: $($_.Exception.Message)"
        }
    }
}

function Test-McpProtocol {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test MCP protocol initialization
    try {
        $initRequest = @{
            jsonrpc = "2.0"
            id      = "test-init"
            method  = "initialize"
            params  = @{
                protocolVersion = "2024-11-05"
                capabilities    = @{
                    tools = @{}
                }
                clientInfo = @{
                    name    = "Fabrikam-Test-Client"
                    version = "1.0.0"
                }
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $initRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            $lines = $response.Content -split "`n"
            $dataLine = $lines | Where-Object { $_ -match "^data: " } | Select-Object -First 1
            
            if ($dataLine) {
                $jsonData = $dataLine -replace "^data: ", ""
                $mcpResponse = $jsonData | ConvertFrom-Json
                
                if ($mcpResponse.result) {
                    Add-TestResult "McpTests" "MCP Protocol Initialize" $true "MCP protocol initialization successful"
                    
                    # Check server capabilities
                    if ($mcpResponse.result.capabilities) {
                        $capabilities = $mcpResponse.result.capabilities
                        $toolsSupported = $capabilities.tools -ne $null
                        Add-TestResult "McpTests" "MCP Server Capabilities" $true "Tools supported: $toolsSupported"
                        
                        # Check server info
                        if ($mcpResponse.result.serverInfo) {
                            $serverInfo = $mcpResponse.result.serverInfo
                            Add-TestResult "McpTests" "MCP Server Info" $true "Server: $($serverInfo.name) v$($serverInfo.version)"
                        }
                    }
                }
                else {
                    Add-TestResult "McpTests" "MCP Protocol Initialize" $false "No result in initialization response"
                }
            }
            else {
                Add-TestResult "McpTests" "MCP Protocol Initialize" $false "No data line in SSE response"
            }
        }
        else {
            Add-TestResult "McpTests" "MCP Protocol Initialize" $false "Empty response from initialization"
        }
    }
    catch {
        Add-TestResult "McpTests" "MCP Protocol Initialize" $false "Initialization failed: $($_.Exception.Message)"
    }
}

function Test-McpToolsDiscovery {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test tools/list endpoint
    try {
        $listToolsRequest = @{
            jsonrpc = "2.0"
            id      = "test-list-tools"
            method  = "tools/list"
            params  = @{}
        } | ConvertTo-Json -Depth 3
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
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
                    $toolCount = $tools.Count
                    $toolNames = $tools | ForEach-Object { $_.name }
                    
                    Add-TestResult "McpTests" "MCP Tools Discovery" $true "Found $toolCount tools: $($toolNames -join ', ')"
                    
                    # Store tools for other tests
                    $Global:McpTools = $tools
                    
                    # Validate expected tools exist
                    Test-ExpectedTools -Tools $tools
                    
                    # Validate tool structure
                    Test-ToolStructure -Tools $tools
                }
                else {
                    Add-TestResult "McpTests" "MCP Tools Discovery" $false "No tools found in response"
                    $Global:McpTools = @()
                }
            }
            else {
                Add-TestResult "McpTests" "MCP Tools Discovery" $false "No data line in tools list response"
            }
        }
        else {
            Add-TestResult "McpTests" "MCP Tools Discovery" $false "Empty response from tools/list"
        }
    }
    catch {
        Add-TestResult "McpTests" "MCP Tools Discovery" $false "Tools discovery failed: $($_.Exception.Message)"
        $Global:McpTools = @()
    }
}

function Test-ExpectedTools {
    param([array]$Tools)
    
    # Define expected tools for Fabrikam MCP server
    $expectedTools = @(
        "GetBusinessDashboard",
        "GetSalesAnalytics", 
        "GetCustomers",
        "GetProducts",
        "GetOrders",
        "GetOrderDetails",
        "GetCustomerProfile",
        "SearchProducts",
        "GetInventoryStatus",
        "GetRevenueAnalytics"
    )
    
    $foundTools = $Tools | ForEach-Object { $_.name }
    $missingTools = @()
    $extraTools = @()
    
    # Check for missing expected tools
    foreach ($expectedTool in $expectedTools) {
        if ($foundTools -notcontains $expectedTool) {
            $missingTools += $expectedTool
        }
    }
    
    # Check for unexpected tools (not necessarily bad, but worth noting)
    foreach ($foundTool in $foundTools) {
        if ($expectedTools -notcontains $foundTool) {
            $extraTools += $foundTool
        }
    }
    
    if ($missingTools.Count -eq 0) {
        Add-TestResult "McpTests" "Expected Tools Present" $true "All expected tools found"
    }
    else {
        Add-TestResult "McpTests" "Expected Tools Present" $false "Missing tools: $($missingTools -join ', ')"
    }
    
    if ($extraTools.Count -gt 0) {
        Add-TestResult "McpTests" "Additional Tools" $true "Extra tools found: $($extraTools -join ', ')"
    }
}

function Test-ToolStructure {
    param([array]$Tools)
    
    $validStructure = $true
    $structureIssues = @()
    
    foreach ($tool in $Tools) {
        # Check required fields
        if (-not $tool.name) {
            $structureIssues += "Tool missing name"
            $validStructure = $false
        }
        
        if (-not $tool.description) {
            $structureIssues += "Tool '$($tool.name)' missing description"
            $validStructure = $false
        }
        
        # Check inputSchema exists (even if empty)
        if (-not $tool.PSObject.Properties.Name -contains "inputSchema") {
            $structureIssues += "Tool '$($tool.name)' missing inputSchema"
            $validStructure = $false
        }
        
        # Validate description is meaningful
        if ($tool.description -and $tool.description.Length -lt 10) {
            $structureIssues += "Tool '$($tool.name)' has very short description"
        }
    }
    
    if ($validStructure) {
        Add-TestResult "McpTests" "Tool Structure Validation" $true "All tools have valid structure"
    }
    else {
        Add-TestResult "McpTests" "Tool Structure Validation" $false "Structure issues: $($structureIssues -join '; ')"
    }
}

function Test-McpToolsExecution {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    if (-not $Global:McpTools -or $Global:McpTools.Count -eq 0) {
        Add-TestResult "McpTests" "MCP Tools Execution" $false "No tools available for execution testing"
        return
    }
    
    # Test execution of key tools
    $testTools = @(
        @{ Name = "GetBusinessDashboard"; Params = @{} },
        @{ Name = "GetSalesAnalytics"; Params = @{} },
        @{ Name = "GetCustomers"; Params = @{} }
    )
    
    foreach ($testTool in $testTools) {
        $tool = $Global:McpTools | Where-Object { $_.name -eq $testTool.Name } | Select-Object -First 1
        
        if ($tool) {
            Test-ToolExecution -McpBaseUrl $McpBaseUrl -Tool $tool -Params $testTool.Params -TimeoutSeconds $TimeoutSeconds
        }
        else {
            Add-TestResult "McpTests" "Tool Execution: $($testTool.Name)" $false "Tool not found in available tools"
        }
    }
}

function Test-ToolExecution {
    param(
        [string]$McpBaseUrl,
        [object]$Tool,
        [hashtable]$Params,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "test-tool-$($Tool.name)"
            method  = "tools/call"
            params  = @{
                name      = $Tool.name
                arguments = $Params
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
                
                if ($mcpResponse.result) {
                    Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $true "Tool executed successfully"
                    
                    # Validate response structure
                    Test-ToolResponseStructure -ToolName $Tool.name -Response $mcpResponse.result
                }
                else {
                    Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Tool execution returned no result"
                }
            }
            else {
                Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "No data line in tool execution response"
            }
        }
        else {
            Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Empty response from tool execution"
        }
    }
    catch {
        Add-TestResult "McpTests" "Tool Execution: $($Tool.name)" $false "Tool execution failed: $($_.Exception.Message)"
    }
}

function Test-ToolResponseStructure {
    param(
        [string]$ToolName,
        [object]$Response
    )
    
    # Check for MCP-standard response structure
    if ($Response.content) {
        if ($Response.content -is [array] -and $Response.content.Count -gt 0) {
            Add-TestResult "McpTests" "Response Structure: $ToolName" $true "Tool response has valid content array"
            
            # Check first content item
            $firstContent = $Response.content[0]
            if ($firstContent.type -and $firstContent.text) {
                Add-TestResult "McpTests" "Content Structure: $ToolName" $true "Content has type and text fields"
                
                # Validate content contains business data
                Test-BusinessDataContent -ToolName $ToolName -Content $firstContent.text
            }
            else {
                Add-TestResult "McpTests" "Content Structure: $ToolName" $false "Content missing type or text fields"
            }
        }
        else {
            Add-TestResult "McpTests" "Response Structure: $ToolName" $false "Content is not a valid array or is empty"
        }
    }
    else {
        Add-TestResult "McpTests" "Response Structure: $ToolName" $false "Tool response missing content field"
    }
}

function Test-BusinessDataContent {
    param(
        [string]$ToolName,
        [string]$Content
    )
    
    # Define expected content patterns for different tools
    $expectedPatterns = @{
        "GetBusinessDashboard" = @("revenue", "orders", "customers", "products")
        "GetSalesAnalytics"    = @("total", "revenue", "sales", "analytics")
        "GetCustomers"         = @("customer", "name", "email", "id")
        "GetProducts"          = @("product", "name", "price", "category")
        "GetOrders"            = @("order", "customer", "total", "date")
    }
    
    if ($expectedPatterns.ContainsKey($ToolName)) {
        $patterns = $expectedPatterns[$ToolName]
        $foundPatterns = @()
        
        foreach ($pattern in $patterns) {
            if ($Content -match $pattern) {
                $foundPatterns += $pattern
            }
        }
        
        if ($foundPatterns.Count -gt 0) {
            Add-TestResult "McpTests" "Business Data: $ToolName" $true "Found expected data patterns: $($foundPatterns -join ', ')"
        }
        else {
            Add-TestResult "McpTests" "Business Data: $ToolName" $false "No expected business data patterns found"
        }
    }
    else {
        # Generic business data validation
        if ($Content.Length -gt 50) {
            Add-TestResult "McpTests" "Business Data: $ToolName" $true "Tool returned substantial content ($($Content.Length) characters)"
        }
        else {
            Add-TestResult "McpTests" "Business Data: $ToolName" $false "Tool returned minimal content ($($Content.Length) characters)"
        }
    }
}

function Test-McpAuthentication {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    # Test MCP authentication if tokens are available
    if ($Global:AdminToken) {
        Test-AuthenticatedToolAccess -McpBaseUrl $McpBaseUrl -Token $Global:AdminToken -Role "Admin" -TimeoutSeconds $TimeoutSeconds
    }
    
    if ($Global:SalesToken) {
        Test-AuthenticatedToolAccess -McpBaseUrl $McpBaseUrl -Token $Global:SalesToken -Role "Sales" -TimeoutSeconds $TimeoutSeconds
    }
    
    # Test unauthenticated access
    Test-UnauthenticatedAccess -McpBaseUrl $McpBaseUrl -TimeoutSeconds $TimeoutSeconds
}

function Test-AuthenticatedToolAccess {
    param(
        [string]$McpBaseUrl,
        [string]$Token,
        [string]$Role,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "test-auth-$Role"
            method  = "tools/call"
            params  = @{
                name      = "GetSalesAnalytics"
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type'  = 'application/json'
            'Accept'        = 'text/event-stream'
            'Authorization' = "Bearer $Token"
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        if ($response.Content) {
            Add-TestResult "McpTests" "Authenticated Access ($Role)" $true "Tool accessible with $Role token"
        }
        else {
            Add-TestResult "McpTests" "Authenticated Access ($Role)" $false "Empty response with $Role token"
        }
    }
    catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*403*") {
            Add-TestResult "McpTests" "Authenticated Access ($Role)" $false "$Role token rejected: $($_.Exception.Message)"
        }
        else {
            Add-TestResult "McpTests" "Authenticated Access ($Role)" $false "Authentication test failed: $($_.Exception.Message)"
        }
    }
}

function Test-UnauthenticatedAccess {
    param(
        [string]$McpBaseUrl,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        $toolCallRequest = @{
            jsonrpc = "2.0"
            id      = "test-unauth"
            method  = "tools/call"
            params  = @{
                name      = "GetSalesAnalytics"
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 4
        
        $headers = @{
            'Content-Type' = 'application/json'
            'Accept'       = 'text/event-stream'
        }
        
        $response = Invoke-WebRequest -Uri "$McpBaseUrl/mcp" -Method Post -Body $toolCallRequest -Headers $headers -TimeoutSec $TimeoutSeconds
        
        # If we get here, unauthenticated access was allowed
        Add-TestResult "McpTests" "Unauthenticated Access Control" $false "Unauthenticated access was allowed"
    }
    catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
            Add-TestResult "McpTests" "Unauthenticated Access Control" $true "Unauthenticated access properly blocked"
        }
        else {
            Add-TestResult "McpTests" "Unauthenticated Access Control" $false "Unexpected error: $($_.Exception.Message)"
        }
    }
}

# Note: Functions are automatically available when this script is dot-sourced
