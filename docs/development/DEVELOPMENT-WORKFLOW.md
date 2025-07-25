# Development & Testing Workflow Guide

## 🔄 Daily Development Workflow

### 1. **Before Starting Development**
```powershell
# Quick health check
.\Test-Development.ps1 -Quick

# If tests fail, fix issues before continuing
```

### 2. **During Development**
```powershell
# Test API changes
.\Test-Development.ps1 -ApiOnly

# Test MCP changes  
.\Test-Development.ps1 -McpOnly

# Full integration test
.\Test-Development.ps1
```

### 3. **Before Committing**
```powershell
# Run all tests
dotnet test FabrikamTests/

# Manual verification
.\Test-Development.ps1 -Verbose
```

## 🧪 Testing Scenarios

### **API Development**
1. **Adding New Endpoint:**
   - Add test case to `api-tests.http`
   - Write unit test in `FabrikamTests/Api/`
   - Run API tests: `.\Test-Development.ps1 -ApiOnly`
   - Test in browser/Postman

2. **Modifying Existing Endpoint:**
   - Check existing tests still pass
   - Update tests if structure changes
   - Verify MCP tools still work
   - Run integration tests

3. **Changing DTOs:**
   - Update API response tests
   - Update MCP parsing tests
   - Run full test suite
   - Test with actual MCP server

### **MCP Development**
1. **Adding New Tool:**
   - Write unit tests for tool logic
   - Test API connectivity
   - Test MCP protocol compliance
   - Verify in Copilot

2. **Modifying Existing Tool:**
   - Update unit tests
   - Test against current API
   - Run integration tests
   - Test error scenarios

## 🔧 Testing Tools Usage

### **Automated Testing**
```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=Api"
dotnet test --filter "Category=Mcp"
dotnet test --filter "Category=Integration"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### **Manual API Testing**
```bash
# Using HTTP files (with REST Client extension)
# Open api-tests.http in VS Code
# Click "Send Request" on any request

# Using PowerShell
.\Test-Development.ps1 -ApiOnly -Verbose

# Using curl
curl http://localhost:5235/api/orders/analytics | jq
```

### **MCP Testing**
```bash
# Start MCP server
cd FabrikamMcp/src
dotnet run

# Test MCP tools in Copilot
# Use GetSalesAnalytics and other tools

# Automated MCP tests
dotnet test --filter "Category=Mcp"
```

## 🚨 Troubleshooting Guide

### **Common Issues & Solutions**

1. **"Connection refused" errors:**
   - Check if API is running: `.\Test-Development.ps1 -ApiOnly`
   - Verify port configuration (5235 for API)
   - Start API: `cd FabrikamApi/src && dotnet run --urls "http://localhost:5235"`

2. **"GetSalesAnalytics" MCP error:**
   - Verify API analytics endpoint: `curl http://localhost:5235/api/orders/analytics`
   - Check MCP configuration: `FabrikamMcp/src/appsettings.Development.json`
   - Ensure BaseUrl points to correct API port

3. **Test failures:**
   - Run verbose tests: `.\Test-Development.ps1 -Verbose`
   - Check individual HTTP requests in `api-tests.http`
   - Verify database seeding worked correctly

4. **Build errors:**
   - Clean solution: `dotnet clean && dotnet build`
   - Restore packages: `dotnet restore`
   - Check .NET 9.0 SDK installed

## 📊 Performance Testing

### **API Load Testing**
```bash
# Using the load testing script
.\create_load_test_script

# Monitor during development
# - Response times < 200ms for most endpoints
# - Analytics endpoint < 500ms
# - No memory leaks
```

### **MCP Performance**
```bash
# Test tool response times
# - Most tools < 1 second
# - Complex analytics < 3 seconds
# - Error handling graceful
```

## 🎯 Quality Gates

### **Before Pull Request**
- [ ] All unit tests pass
- [ ] Integration tests pass  
- [ ] API contract tests pass
- [ ] MCP tools work in Copilot
- [ ] Performance benchmarks met
- [ ] No new linting errors

### **Before Deployment**
- [ ] Full test suite passes
- [ ] Load testing completed
- [ ] Security scanning clean
- [ ] Documentation updated
- [ ] Monitoring configured

## 🔄 Continuous Integration

The GitHub Actions workflow automatically:
1. Runs all tests on push/PR
2. Validates API contracts
3. Checks MCP protocol compliance
4. Generates test reports
5. Blocks merge if tests fail

Monitor the Actions tab for automated test results.
