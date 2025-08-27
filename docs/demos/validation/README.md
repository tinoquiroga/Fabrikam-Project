# 🔍 Demo Validation Tools

Welcome to the demo validation suite for Fabrikam MCP demonstrations. These tools ensure your demo environment is ready for successful presentations.

## 🎯 Quick Validation

### **Pre-Demo Checklist**

Run this checklist before every demo:

```powershell
# Quick validation script
.\Validate-Demo.ps1

# Full development test
.\Test-Development.ps1 -Quick

# Authentication test (if using JWT)
.\Demo-Authentication.ps1 -ShowCredentials
```

**Expected Results:**
- ✅ API and MCP servers responding
- ✅ Business data available (orders, customers, tickets)
- ✅ MCP tools responding correctly
- ✅ Authentication working (if enabled)

### **Automated Validation Script**

The project includes `Validate-Demo.ps1` for comprehensive pre-demo validation:

**Features:**
- Server health checks
- Data availability validation
- MCP tool responsiveness testing
- Authentication verification
- Performance benchmarking

**Usage:**
```powershell
# Basic validation
.\Validate-Demo.ps1

# Detailed output
.\Validate-Demo.ps1 -Verbose

# Quick health check
.\Validate-Demo.ps1 -Quick
```

## 🛠️ Manual Validation Steps

### **1. Server Connectivity**

**Test API Server:**
```bash
# Check API health
curl -k http://localhost:7296/api/info

# Expected response:
{
  "name": "Fabrikam API",
  "version": "1.0.0",
  "environment": "Development",
  "authentication": {
    "mode": "Disabled"  # or "BearerToken"
  }
}
```

**Test MCP Server:**
```bash
# Check MCP status
curl -k http://localhost:5000/status

# Expected response:
{
  "status": "Ready",
  "service": "Fabrikam MCP Server",
  "version": "1.0.0"
}
```

### **2. Business Data Validation**

**Test Core Data Endpoints:**
```powershell
# Check business data availability
$apiBase = "http://localhost:7296/api"

# Test orders data
$orders = Invoke-RestMethod -Uri "$apiBase/orders"
Write-Host "Orders available: $($orders.Count)"

# Test customers data
$customers = Invoke-RestMethod -Uri "$apiBase/customers"
Write-Host "Customers available: $($customers.Count)"

# Test support tickets
$tickets = Invoke-RestMethod -Uri "$apiBase/supporttickets"
Write-Host "Support tickets available: $($tickets.Count)"

# Test products
$products = Invoke-RestMethod -Uri "$apiBase/products"
Write-Host "Products available: $($products.Count)"
```

### **3. MCP Tool Testing**

**Test MCP Tools Directly:**
```bash
# Test sales analytics tool
curl -k http://localhost:5000/mcp/call \
  -X POST \
  -H "Content-Type: application/json" \
  -H "X-User-Id: demo-guid-12345" \
  -d '{
    "method": "tools/call",
    "params": {
      "name": "get_sales_analytics",
      "arguments": {}
    }
  }'

# Test customer data tool
curl -k http://localhost:5000/mcp/call \
  -X POST \
  -H "Content-Type: application/json" \
  -H "X-User-Id: demo-guid-12345" \
  -d '{
    "method": "tools/call",
    "params": {
      "name": "get_customers",
      "arguments": {}
    }
  }'
```

### **4. Authentication Testing** (JWT Mode)

**Test JWT Authentication:**
```powershell
# Get demo credentials
$creds = Invoke-RestMethod -Uri "http://localhost:7296/api/auth/demo-credentials"
$admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }

# Login to get JWT token
$loginBody = @{
    email = $admin.email
    password = $admin.password
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "http://localhost:7296/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

Write-Host "JWT Token received: $($authResponse.accessToken.Substring(0,20))..."
Write-Host "Token expires: $($authResponse.expiresAt)"

# Test authenticated API call
$headers = @{ "Authorization" = "Bearer $($authResponse.accessToken)" }
$secureData = Invoke-RestMethod -Uri "http://localhost:7296/api/customers" -Headers $headers
Write-Host "Authenticated API call successful: $($secureData.Count) customers"
```

## 📊 Performance Validation

### **Response Time Testing**

**Benchmark Critical Endpoints:**
```powershell
function Test-EndpointPerformance {
    param($Url, $Name)
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-RestMethod -Uri $Url -TimeoutSec 10
        $stopwatch.Stop()
        $responseTime = $stopwatch.ElapsedMilliseconds
        
        if ($responseTime -lt 1000) {
            Write-Host "✅ $Name: ${responseTime}ms" -ForegroundColor Green
        } elseif ($responseTime -lt 3000) {
            Write-Host "⚠️ $Name: ${responseTime}ms (slow)" -ForegroundColor Yellow
        } else {
            Write-Host "❌ $Name: ${responseTime}ms (too slow)" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ $Name: Failed - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test critical demo endpoints
Test-EndpointPerformance "http://localhost:7296/api/info" "API Info"
Test-EndpointPerformance "http://localhost:5000/status" "MCP Status"
Test-EndpointPerformance "http://localhost:7296/api/orders" "Orders Data"
Test-EndpointPerformance "http://localhost:7296/api/customers" "Customer Data"
```

### **MCP Tool Performance**

**Benchmark MCP Tool Response Times:**
```powershell
function Test-McpToolPerformance {
    param($ToolName)
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $body = @{
            method = "tools/call"
            params = @{
                name = $ToolName
                arguments = @{}
            }
        } | ConvertTo-Json -Depth 3
        
        $response = Invoke-RestMethod -Uri "http://localhost:5000/mcp/call" -Method POST -Body $body -ContentType "application/json" -Headers @{"X-User-Id" = "demo-guid-12345"}
        $stopwatch.Stop()
        $responseTime = $stopwatch.ElapsedMilliseconds
        
        if ($responseTime -lt 2000) {
            Write-Host "✅ $ToolName: ${responseTime}ms" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $ToolName: ${responseTime}ms (slow for demo)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ $ToolName: Failed - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test demo-critical MCP tools
Test-McpToolPerformance "get_sales_analytics"
Test-McpToolPerformance "get_customers"
Test-McpToolPerformance "get_support_tickets"
Test-McpToolPerformance "get_orders"
```

## 🎭 Demo-Specific Validation

### **Copilot Studio Integration Validation**

**Pre-Demo Copilot Studio Checklist:**

1. **Custom Connector Status**
   - [ ] Power Apps connector created and tested
   - [ ] Connection configured with correct authentication
   - [ ] Test operation returns business data

2. **Agent Configuration**
   - [ ] Agent instructions configured for business focus
   - [ ] Web search disabled (recommended)
   - [ ] Custom connector properly added to agent

3. **Authentication Setup** (if using JWT)
   - [ ] Fresh JWT token generated
   - [ ] Connector connection updated with new token
   - [ ] Test authenticated MCP calls work

### **Demo Scenario Validation**

**Test Demo Prompts:**
```
# Test each demo prompt to ensure expected responses

✅ "Show me the current business dashboard for Fabrikam Modular Homes"
Expected: Sales data, customer metrics, business KPIs

✅ "Show me our current support ticket situation"  
Expected: Support ticket analysis, issue categorization

✅ "Can you analyze our customer complaints from 2020-2021 versus recent years?"
Expected: Historical analysis, quality improvements

✅ "Can you find examples of customers who had early challenges but are now happy?"
Expected: Customer success stories, journey analysis
```

### **Demo Environment Checklist**

**Before Starting Demo:**
- [ ] All servers running and responsive
- [ ] Business data available and current
- [ ] MCP tools responding within 2 seconds
- [ ] Authentication working (if enabled)
- [ ] Copilot Studio agent configured correctly
- [ ] Demo prompts tested and working
- [ ] Backup plan ready for technical issues

## 🔧 Troubleshooting Validation Issues

### **Common Validation Failures**

**Server Not Responding:**
```powershell
# Start the servers
.\Manage-Project.ps1 start

# Verify startup
Start-Sleep 10
.\Validate-Demo.ps1
```

**Data Not Available:**
```powershell
# Rebuild and seed data
.\Test-Development.ps1 -Quick
```

**MCP Tools Failing:**
```powershell
# Check MCP server logs
# Restart MCP server if needed
.\Manage-Project.ps1 restart
```

**Authentication Issues:**
```powershell
# Refresh demo credentials
.\Demo-Authentication.ps1 -ShowCredentials

# Test authentication
.\Demo-Authentication.ps1 -TestLogin -UserEmail "lee.gu@fabrikam.levelupcsp.com"
```

## 📈 Validation Reporting

### **Generate Validation Report**

```powershell
# Comprehensive validation with report
.\Validate-Demo.ps1 -Verbose > demo-validation-report.txt

# Review the report
Get-Content demo-validation-report.txt
```

### **Continuous Validation**

For ongoing demo readiness, consider setting up:
- Automated validation before demo sessions
- Performance monitoring during demos
- Quick health checks between demo segments

## 📞 Support

### **Quick Help**

1. **Run Quick Validation**: `.\Validate-Demo.ps1`
2. **Check Server Health**: `.\Test-Development.ps1 -Quick`
3. **Test Authentication**: `.\Demo-Authentication.ps1 -ShowCredentials`
4. **Restart Everything**: `.\Manage-Project.ps1 restart`

### **Additional Resources**

- **[Main Demo Guide](./README.md)** - Complete demo setup
- **[Copilot Studio Setup](../copilot-studio/README.md)** - Integration configuration
- **[Troubleshooting Guide](../copilot-studio/TROUBLESHOOTING.md)** - Common issues and solutions

---

**🎯 Validation complete!** Your demo environment is ready for successful Fabrikam MCP demonstrations.
