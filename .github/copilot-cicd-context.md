# 🔄 **GitHub Actions CI/CD Pipeline Documentation**

## 📋 **Overview**

This repository uses GitHub Actions for automated testing, building, and deployment. GitHub Copilot should be aware of this CI/CD infrastructure when suggesting code changes, deployment strategies, and development workflows.

---

## 🏗️ **Pipeline Architecture**

### **Core Workflows**
1. **`testing.yml`** - Automated testing on every push/PR
2. **`deploy-full-stack.yml`** - Coordinated deployment of API + MCP services
3. **`main_fabrikam-api-dev-izbd.yml`** - Azure App Service deployment for API
4. **`main_fabrikam-mcp-dev-izbd.yml`** - Azure App Service deployment for MCP

### **Trigger Patterns**
- **On Push**: `main`, `develop` branches trigger testing
- **On PR**: All PRs to `main` trigger testing
- **Path-based**: Changes to `FabrikamApi/**` or `FabrikamMcp/**` trigger deployment
- **Manual**: Workflow dispatch available for controlled deployments

---

## 🎯 **GitHub Copilot Integration Guidelines**

### **When Copilot Suggests Code Changes:**
1. **Always consider CI/CD impact** - Will this break the build pipeline?
2. **Test compatibility** - Ensure changes work with the automated testing in `testing.yml`
3. **Deployment readiness** - Consider if changes require deployment pipeline updates

### **Key Patterns Copilot Should Remember:**

#### **Testing Infrastructure**
```yaml
# Our testing pipeline expects this structure:
- dotnet restore
- dotnet build --no-restore  
- dotnet test FabrikamTests/FabrikamTests.csproj --logger "trx;LogFileName=test-results.trx"
```

#### **Build Requirements**
- **.NET 9.0** runtime required
- **Test results** in TRX format expected
- **Project dependencies** must restore cleanly
- **No build warnings** should be introduced

#### **Deployment Considerations**
- **Azure App Service** targets for both API and MCP
- **Environment variables** managed through Azure configuration
- **Connection strings** configured in Azure, not hardcoded
- **Health checks** expected at `/health` endpoints

---

## 🔧 **Development Workflow Integration**

### **Pre-Commit Checklist (Copilot should remind about):**
- [ ] Code builds locally with `dotnet build`
- [ ] Tests pass locally with `dotnet test`
- [ ] No new compiler warnings introduced
- [ ] API contracts maintained (no breaking changes)
- [ ] MCP protocol compliance preserved

### **Common CI/CD Scenarios:**

#### **Adding New API Endpoints**
```csharp
// Copilot should suggest patterns like this that work with our CI/CD:
[HttpGet("new-endpoint")]
public async Task<ActionResult<ResponseDto>> GetNewData()
{
    try
    {
        // Implementation
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in GetNewData");
        return StatusCode(500, "Internal server error");
    }
}
```

#### **Adding New MCP Tools**
```csharp
// Copilot should follow this pattern for CI/CD compatibility:
[McpServerTool, Description("Tool description for AI understanding")]
public async Task<object> NewTool(string? parameter = null)
{
    try
    {
        // Async implementation
        return new { content = new[] { /* MCP structure */ } };
    }
    catch (Exception ex)
    {
        return new { error = new { message = ex.Message } };
    }
}
```

#### **Database/Schema Changes**
- Use Entity Framework migrations
- Ensure backward compatibility during deployments
- Test migration rollback scenarios

---

## 🧪 **Testing Strategy Integration**

### **Current Test Infrastructure:**
- **Unit Tests**: `FabrikamTests/Api/`
- **Integration Tests**: `FabrikamTests/Mcp/`
- **Manual Testing**: `api-tests.http`, `Test-Development.ps1`
- **CI Testing**: Automated via GitHub Actions

### **Copilot Test Suggestions Should:**
1. **Follow existing patterns** in `FabrikamTests/`
2. **Include both positive and negative test cases**
3. **Mock external dependencies** (HttpClient, databases)
4. **Use FluentAssertions** for readable assertions
5. **Include async/await testing patterns**

#### **Example Test Pattern:**
```csharp
[Fact]
public async Task NewFeature_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var expectedResult = new SomeDto { /* test data */ };
    
    // Act
    var result = await _service.NewFeature(validInput);
    
    // Assert
    result.Should().NotBeNull();
    result.Should().BeEquivalentTo(expectedResult);
}
```

---

## 🚀 **Deployment Environment Awareness**

### **Azure Resources (Copilot should know about):**
- **App Services**: Hosting API and MCP
- **Application Insights**: Logging and monitoring
- **Key Vault**: Secrets management
- **SQL Database**: Data persistence
- **Storage Account**: File/blob storage

### **Configuration Management:**
- **Local Development**: `appsettings.Development.json`
- **Azure Deployment**: Environment variables and Key Vault
- **CI/CD**: GitHub Secrets for deployment credentials

### **Copilot Should Suggest:**
- Using `IConfiguration` for settings access
- Implementing proper logging with structured data
- Following Azure-friendly patterns (connection resilience, etc.)
- Health check endpoints for load balancer integration

---

## 📊 **Performance and Monitoring**

### **Logging Patterns (for Azure Application Insights):**
```csharp
// Copilot should suggest structured logging:
_logger.LogInformation("Processing {OperationType} for {EntityId}", 
    "OrderCreation", orderId);

_logger.LogError(ex, "Failed to process {Operation} for {CustomerId}", 
    operationName, customerId);
```

### **Health Check Patterns:**
```csharp
// Copilot should suggest health checks for new services:
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<ExternalApiHealthCheck>("external-api");
```

---

## 🔄 **Version Control Integration**

### **Branch Strategy:**
- **`main`**: Production-ready code, triggers deployment
- **`develop`**: Integration branch, triggers testing
- **Feature branches**: Individual development, creates PRs

### **Copilot PR Suggestions Should Include:**
- Clear commit messages describing CI/CD impact
- Updated tests for new functionality
- Documentation updates for API changes
- Consideration of deployment timing and dependencies

---

## 🛡️ **Security and Compliance**

### **Copilot Should Remind About:**
- **Secrets**: Never hardcode, use Azure Key Vault
- **Authentication**: Implement proper auth patterns
- **Input Validation**: Always validate user inputs
- **HTTPS**: Enforce secure connections
- **CORS**: Configure appropriately for cross-origin requests

### **Security Patterns:**
```csharp
// Copilot should suggest secure patterns like:
[HttpPost]
public async Task<ActionResult> SecureEndpoint([FromBody] ValidatedRequestDto request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    // Secure processing
    return Ok(result);
}
```

---

## 📈 **Continuous Improvement**

### **Metrics Copilot Should Consider:**
- **Build time**: Keep workflows under 5 minutes
- **Test coverage**: Maintain high coverage for critical paths
- **Deployment frequency**: Enable multiple deployments per day
- **Recovery time**: Ensure quick rollback capabilities

### **When Suggesting Optimizations:**
- Consider impact on CI/CD pipeline performance
- Suggest caching strategies for faster builds
- Recommend parallel testing where appropriate
- Consider Azure resource optimization

---

## 🎯 **Summary for GitHub Copilot**

**Key Reminders:**
1. **All code changes** should be compatible with automated CI/CD
2. **Testing is mandatory** - suggest tests with every feature
3. **Azure deployment patterns** should be followed
4. **Async/await patterns** are standard in this codebase
5. **Structured logging** is required for monitoring
6. **MCP protocol compliance** must be maintained
7. **API contract stability** is critical for deployments

**This documentation helps GitHub Copilot understand the full development lifecycle and suggest changes that work harmoniously with our automated pipeline infrastructure.**
