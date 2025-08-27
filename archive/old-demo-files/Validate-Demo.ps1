# Fabrikam MCP Demo Validation Script
# Ensures all components are ready for a successful 3-minute demo

param(
    [switch]$Verbose           # Show detailed validation output
)

$ErrorActionPreference = "Stop"

# Color functions
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Warning($message) { Write-Host "⚠️ $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️ $message" -ForegroundColor Cyan }

Write-Host "🎥 FABRIKAM MCP DEMO VALIDATION" -ForegroundColor Yellow
Write-Host "=" * 50 -ForegroundColor Yellow

# Test 1: Server Health Check
Write-Info "🔍 Checking server health..."
try {
    $apiResponse = Invoke-RestMethod -Uri "http://localhost:7296/api/info" -Method Get -TimeoutSec 5
    $mcpResponse = Invoke-RestMethod -Uri "http://localhost:5000/status" -Method Get -TimeoutSec 5
    
    if ($apiResponse -and $mcpResponse.status -eq "Ready") {
        Write-Success "Servers are running and healthy"
        if ($Verbose) {
            Write-Host "   API: $($apiResponse.name) v$($apiResponse.version)" -ForegroundColor Gray
            Write-Host "   MCP: $($mcpResponse.service) v$($mcpResponse.version)" -ForegroundColor Gray
        }
    }
    else {
        Write-Error "Server health check failed"
        exit 1
    }
}
catch {
    Write-Error "Servers not responding. Run: .\Test-Development.ps1 -Quick"
    exit 1
}

# Test 2: Data Availability Check
Write-Info "📊 Checking business data availability..."
try {
    # Check orders
    $orders = Invoke-RestMethod -Uri "http://localhost:7296/api/orders" -Method Get -TimeoutSec 10
    $orderCount = $orders.Count
    
    # Check customers  
    $customers = Invoke-RestMethod -Uri "http://localhost:7296/api/customers" -Method Get -TimeoutSec 10
    $customerCount = $customers.Count
    
    # Check support tickets
    $tickets = Invoke-RestMethod -Uri "http://localhost:7296/api/supporttickets" -Method Get -TimeoutSec 10
    $ticketCount = $tickets.Count
    
    # Check products
    $products = Invoke-RestMethod -Uri "http://localhost:7296/api/products" -Method Get -TimeoutSec 10
    $productCount = $products.Count
    
    Write-Success "Business data loaded successfully"
    if ($Verbose) {
        Write-Host "   Orders: $orderCount" -ForegroundColor Gray
        Write-Host "   Customers: $customerCount" -ForegroundColor Gray
        Write-Host "   Support Tickets: $ticketCount" -ForegroundColor Gray
        Write-Host "   Products: $productCount" -ForegroundColor Gray
    }
    
    # Validate minimum data requirements for demo
    if ($orderCount -lt 10) { Write-Warning "Low order count may impact demo quality" }
    if ($customerCount -lt 5) { Write-Warning "Low customer count may impact demo quality" }
    if ($ticketCount -lt 15) { Write-Warning "Support ticket count should be ~24 for full demo" }
    if ($productCount -lt 5) { Write-Warning "Low product count may impact demo quality" }
}
catch {
    Write-Error "Business data check failed: $($_.Exception.Message)"
    exit 1
}

# Test 3: Support Ticket Timeline Validation
Write-Info "🎫 Validating support ticket timeline (2020-2025)..."
try {
    $ticketsByYear = @{}
    foreach ($ticket in $tickets) {
        $year = [DateTime]::Parse($ticket.createdDate).Year
        if (-not $ticketsByYear.ContainsKey($year)) {
            $ticketsByYear[$year] = 0
        }
        $ticketsByYear[$year]++
    }
    
    $hasOldTickets = $ticketsByYear.ContainsKey(2020) -or $ticketsByYear.ContainsKey(2021)
    $hasNewTickets = $ticketsByYear.ContainsKey(2024) -or $ticketsByYear.ContainsKey(2025)
    
    if ($hasOldTickets -and $hasNewTickets) {
        Write-Success "Support ticket timeline spans startup to current era"
        if ($Verbose) {
            foreach ($year in $ticketsByYear.Keys | Sort-Object) {
                Write-Host "   $year`: $($ticketsByYear[$year]) tickets" -ForegroundColor Gray
            }
        }
    }
    else {
        Write-Warning "Missing timeline data - may impact business evolution demo"
    }
}
catch {
    Write-Warning "Could not validate support ticket timeline"
}

# Test 4: HVAC Long-running Issue Check
Write-Info "🔧 Checking for long-running HVAC case study..."
try {
    $hvacTickets = $tickets | Where-Object { $_.subject -like "*HVAC*" -or $_.description -like "*HVAC*" }
    $ongoingTickets = $hvacTickets | Where-Object { $_.status -eq "InProgress" } # InProgress status as string
    
    if ($ongoingTickets.Count -gt 0) {
        Write-Success "Long-running HVAC case study available ($($ongoingTickets.Count) ongoing tickets)"
        if ($Verbose) {
            foreach ($ticket in $ongoingTickets) {
                $duration = ([DateTime]::Now - [DateTime]::Parse($ticket.createdDate)).Days
                Write-Host "   Ticket $($ticket.ticketNumber): $duration days old" -ForegroundColor Gray
            }
        }
    }
    else {
        Write-Warning "No ongoing HVAC tickets found - engineering case study may be limited"
    }
}
catch {
    Write-Warning "Could not validate HVAC case study data"
}

# Test 5: Analytics Endpoint Validation
Write-Info "📈 Checking analytics capabilities..."
try {
    $analytics = Invoke-RestMethod -Uri "http://localhost:7296/api/orders/analytics" -Method Get -TimeoutSec 10
    
    $hasRequiredFields = $analytics.summary -and $analytics.byStatus -and $analytics.byRegion
    
    if ($hasRequiredFields) {
        Write-Success "Analytics endpoint ready for executive dashboard demo"
        if ($Verbose) {
            Write-Host "   Total Orders: $($analytics.summary.totalOrders)" -ForegroundColor Gray
            Write-Host "   Total Revenue: $($analytics.summary.totalRevenue)" -ForegroundColor Gray
            Write-Host "   Regions: $($analytics.byRegion.Count)" -ForegroundColor Gray
        }
    }
    else {
        Write-Warning "Analytics endpoint missing required fields"
    }
}
catch {
    Write-Warning "Analytics validation failed - executive dashboard may be limited"
}

# Test 6: Sample MCP Tool Validation (if possible to test directly)
Write-Info "🤖 Validating MCP tool availability..."
try {
    # This would ideally test MCP tools directly, but we'll just verify the endpoint
    $mcpHealthy = $mcpResponse.status -eq "Ready"
    
    if ($mcpHealthy) {
        Write-Success "MCP tools ready for Copilot integration"
    }
    else {
        Write-Error "MCP server not ready for demo"
    }
}
catch {
    Write-Warning "Could not fully validate MCP tool availability"
}

# Demo Readiness Summary
Write-Host "`n🎯 DEMO READINESS SUMMARY" -ForegroundColor Yellow
Write-Host "=" * 40 -ForegroundColor Yellow

$readinessChecks = @(
    @{ Name = "Server Health"; Status = $true },
    @{ Name = "Business Data"; Status = ($orderCount -gt 5 -and $customerCount -gt 3 -and $ticketCount -gt 10) },
    @{ Name = "Timeline Data"; Status = ($hasOldTickets -and $hasNewTickets) },
    @{ Name = "Case Studies"; Status = ($ongoingTickets.Count -gt 0) },
    @{ Name = "Analytics"; Status = $hasRequiredFields },
    @{ Name = "MCP Ready"; Status = $mcpHealthy }
)

$passedChecks = ($readinessChecks | Where-Object { $_.Status }).Count
$totalChecks = $readinessChecks.Count

foreach ($check in $readinessChecks) {
    $status = if ($check.Status) { "✅" } else { "⚠️" }
    $color = if ($check.Status) { "Green" } else { "Yellow" }
    Write-Host "$status $($check.Name)" -ForegroundColor $color
}

Write-Host "`nReadiness Score: $passedChecks/$totalChecks" -ForegroundColor $(if ($passedChecks -eq $totalChecks) { "Green" } else { "Yellow" })

if ($passedChecks -eq $totalChecks) {
    Write-Success "`n🎬 DEMO READY! You're all set for a successful 3-minute demo."
    Write-Host "📋 Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Open Copilot Studio and ensure MCP server is connected" -ForegroundColor Gray
    Write-Host "   2. Clear conversation history for a clean demo" -ForegroundColor Gray
    Write-Host "   3. Have docs/demos/QUICK-DEMO-PROMPTS.md ready for reference" -ForegroundColor Gray
    Write-Host "   4. Test the first prompt to ensure everything is working" -ForegroundColor Gray
}
else {
    Write-Warning "`n⚠️ Demo has some limitations. Review warnings above."
    Write-Host "💡 Suggestions:" -ForegroundColor Cyan
    Write-Host "   • Run .\Test-Development.ps1 -CleanBuild to refresh data" -ForegroundColor Gray
    Write-Host "   • Check that all seed data is properly loaded" -ForegroundColor Gray
    Write-Host "   • Verify servers are running with latest changes" -ForegroundColor Gray
}

Write-Host "`n🚀 Ready to showcase the power of conversational business intelligence!" -ForegroundColor Green
