# 🔧 Copilot Studio Troubleshooting Guide

Common issues and solutions for Fabrikam MCP agent integration with Microsoft Copilot Studio.

## 🚨 Quick Diagnosis

### Check Your Setup Type

First, identify which authentication mode you're using:

```bash
curl -k https://your-api-app-name.azurewebsites.net/api/info
```

Look for `authentication.mode`:
- `"Disabled"` → [No Authentication Issues](#-no-authentication-issues)
- `"BearerToken"` → [JWT Authentication Issues](#-jwt-authentication-issues)
- `"EntraExternalId"` → [Enterprise Auth Issues](#-enterprise-authentication-issues)

## 🔓 No Authentication Issues

### **Issue: "I don't have access to that information"**

**Symptoms:**
- Agent responds with generic error messages
- No business data in responses
- MCP tools appear not to be working

**Solutions:**

1. **Check MCP Server Status**
   ```bash
   curl -k https://your-mcp-app-name.azurewebsites.net/mcp/call \
     -H "Content-Type: application/json" \
     -H "X-User-Id: YOUR_GUID" \
     -d '{"method": "tools/call", "params": {"name": "get_sales_analytics", "arguments": {}}}'
   ```

2. **Verify GUID Configuration**
   ```powershell
   # Generate fresh GUID
   $newGuid = [System.Guid]::NewGuid().ToString()
   Write-Host "New GUID: $newGuid"
   ```

3. **Update Connector Connection**
   - Go to Power Apps → Custom Connectors
   - Edit your Fabrikam MCP Connector
   - Update the X-User-Id header with your GUID
   - Test the connection

4. **Check Agent Configuration**
   - Ensure custom connector is added to the agent
   - Verify web search is disabled
   - Confirm agent instructions are properly configured

### **Issue: Connector Connection Fails**

**Symptoms:**
- Connection test fails in Power Apps
- "Unable to connect" errors
- Timeout errors

**Solutions:**

1. **Verify MCP Server URL**
   ```bash
   # Test basic connectivity
   curl -k https://your-mcp-app-name.azurewebsites.net/health
   ```

2. **Check Endpoint Accessibility**
   ```bash
   # Test MCP endpoint specifically
   curl -k https://your-mcp-app-name.azurewebsites.net/mcp/call \
     -X POST \
     -H "Content-Type: application/json"
   ```

3. **Validate Connector Configuration**
   - Host: Should be just the hostname (no https://)
   - Base URL: Should be `/`
   - Authentication: Should be "No authentication"

### **Issue: Agent Gives Generic Responses**

**Symptoms:**
- Agent provides generic business advice
- No specific Fabrikam data
- Responses don't use MCP tools

**Solutions:**

1. **Disable Web Search**
   - Go to Copilot Studio → Overview
   - Turn OFF "Web search"
   - This forces the agent to use only MCP tools

2. **Strengthen Agent Instructions**
   ```
   IMPORTANT: You can ONLY provide information using the Fabrikam MCP tools. 
   If you cannot access the MCP tools, respond with: "I cannot access the business data right now. Please start a new conversation to restore my connection to the Fabrikam systems."
   ```

3. **Test MCP Tool Integration**
   - Start a new conversation
   - Ask: "What sales data do you have access to?"
   - The agent should attempt to use MCP tools

## 🔐 JWT Authentication Issues

### **Issue: "401 Unauthorized" Responses**

**Symptoms:**
- Authentication errors from the API
- "Token invalid" messages
- Access denied responses

**Solutions:**

1. **Verify JWT Token Format**
   ```bash
   # Test token directly
   curl -k https://your-api-app-name.azurewebsites.net/api/customers \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

2. **Check Token Expiration**
   ```powershell
   # Quick token validation
   $token = "YOUR_JWT_TOKEN"
   try {
       $payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($token.Split('.')[1] + '=='))
       $decoded = $payload | ConvertFrom-Json
       $expiry = Get-Date -UnixTimeSeconds $decoded.exp
       Write-Host "Token expires: $expiry"
       if ($expiry -lt (Get-Date)) {
           Write-Host "❌ TOKEN EXPIRED - Need to refresh"
       } else {
           Write-Host "✅ Token is valid"
       }
   } catch {
       Write-Host "❌ Invalid token format"
   }
   ```

3. **Refresh JWT Token**
   ```powershell
   # Get fresh demo credentials and token
   $creds = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials"
   $admin = $creds.demoUsers | Where-Object { $_.name -like "*Admin*" }
   
   $loginBody = @{
       email = $admin.email
       password = $admin.password
   } | ConvertTo-Json
   
   $authResponse = Invoke-RestMethod -Uri "https://your-api-app-name.azurewebsites.net/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
   
   Write-Host "New Token: $($authResponse.accessToken)"
   ```

4. **Update Connector Connection**
   - Go to Power Apps → Custom Connectors → Your Connector
   - Edit the connection
   - Update Authorization header: `Bearer NEW_JWT_TOKEN`
   - Test the connection

### **Issue: "Forbidden" or Permission Errors**

**Symptoms:**
- Some endpoints work, others return 403
- "Insufficient permissions" messages
- Limited data access

**Solutions:**

1. **Check User Roles**
   ```bash
   # Get demo user information
   curl -k https://your-api-app-name.azurewebsites.net/api/auth/demo-credentials
   ```

2. **Use Admin Account**
   - Ensure you're using the admin user credentials
   - Admin users typically have access to all endpoints
   - Check the demo credentials response for admin users

3. **Verify API Endpoint Permissions**
   ```bash
   # Test specific endpoints
   curl -k https://your-api-app-name.azurewebsites.net/api/customers \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

### **Issue: Token Expires During Demos**

**Symptoms:**
- Agent works initially, then stops
- Authentication errors after some time
- Need to restart conversations frequently

**Solutions:**

1. **Pre-Demo Token Refresh**
   ```powershell
   # Always refresh token before demos
   # Use the token refresh script above
   ```

2. **Monitor Token Expiration**
   ```powershell
   # Set reminder for token refresh
   $token = "YOUR_JWT_TOKEN"
   $payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($token.Split('.')[1] + '=='))
   $decoded = $payload | ConvertFrom-Json
   $expiry = Get-Date -UnixTimeSeconds $decoded.exp
   $timeLeft = $expiry - (Get-Date)
   Write-Host "Token expires in: $($timeLeft.Hours) hours, $($timeLeft.Minutes) minutes"
   ```

3. **Plan Demo Duration**
   - Check token expiration before starting
   - For long demos, refresh tokens during breaks
   - Have backup tokens ready

## 🛡️ Enterprise Authentication Issues

**Status:** Enterprise authentication with Entra External ID is coming soon.

**Current Alternative:** Use [JWT Authentication](#-jwt-authentication-issues) for production-ready security.

## 🔗 Connection & Networking Issues

### **Issue: MCP Server Not Responding**

**Symptoms:**
- Timeouts when calling MCP endpoints
- Server unreachable errors
- Intermittent connectivity

**Solutions:**

1. **Check Server Status**
   ```bash
   # Check if server is running
   curl -k https://your-mcp-app-name.azurewebsites.net/health
   ```

2. **Verify Azure App Service**
   - Go to Azure Portal → App Services
   - Check if your MCP app service is running
   - Review application logs for errors

3. **Test Network Connectivity**
   ```powershell
   # Test network path
   Test-NetConnection your-mcp-app-name.azurewebsites.net -Port 443
   ```

### **Issue: CORS Errors**

**Symptoms:**
- Cross-origin request errors
- Browser blocks requests
- "CORS policy" error messages

**Solutions:**

1. **Check MCP CORS Configuration**
   - CORS should be configured to allow Power Apps domains
   - Common allowed origins: `https://make.powerapps.com`, `https://flow.microsoft.com`

2. **Use Connector Instead of Direct Calls**
   - Always use the Power Apps custom connector
   - Don't make direct API calls from browser-based tools

## 🤖 Agent Behavior Issues

### **Issue: Agent Answers Off-Topic Questions**

**Symptoms:**
- Agent responds to questions about animals, colors, etc.
- Generic knowledge instead of business focus
- Doesn't maintain professional identity

**Solutions:**

1. **Strengthen Instructions**
   ```
   CRITICAL BEHAVIORAL RULE:
   You are ONLY a Fabrikam business intelligence system. For ANY question not related to Fabrikam business operations, respond EXACTLY with:
   
   "I'm the Fabrikam Business Intelligence Assistant, focused exclusively on Fabrikam Modular Homes business data. I can help you with sales analytics, customer information, product details, order tracking, and support tickets. What Fabrikam business information can I help you with today?"
   
   Do NOT engage with questions about animals, colors, hypothetical scenarios, general knowledge, or any non-business topics.
   ```

2. **Disable Web Search**
   - Copilot Studio → Overview → Web search = OFF
   - This prevents fallback to generic web content

3. **Test Business Focus**
   ```
   Test queries:
   ✅ "What are our sales trends?"
   ✅ "Show me customer data"
   ❌ "What color is the sky?" (should redirect)
   ❌ "Tell me about cats" (should redirect)
   ```

### **Issue: Agent Doesn't Use MCP Tools**

**Symptoms:**
- Agent gives generic business advice
- No specific Fabrikam data in responses
- Tools not being called

**Solutions:**

1. **Check Tool Integration**
   - Copilot Studio → Actions
   - Verify custom connector is properly added
   - Test individual actions

2. **Simplify Instructions**
   ```
   When users ask for business data, you MUST use the available MCP tools to get current information. Always use tools for:
   - Sales data: Use get_sales_analytics
   - Customer data: Use get_customers  
   - Product data: Use get_products
   - Order data: Use get_orders
   ```

3. **Test Tool Calls**
   - Ask: "Use your tools to get sales data"
   - Monitor if MCP tools are actually called

## 📊 Performance Issues

### **Issue: Slow Response Times**

**Symptoms:**
- Long delays before agent responses
- Timeout errors
- Poor user experience

**Solutions:**

1. **Check MCP Server Performance**
   ```bash
   # Test response time
   time curl -k https://your-mcp-app-name.azurewebsites.net/api/info
   ```

2. **Optimize API Calls**
   - Use specific queries instead of broad data requests
   - Implement proper indexing on database queries
   - Monitor server logs for slow queries

3. **Review Agent Configuration**
   - Simplify agent instructions if too complex
   - Reduce number of tools if not needed
   - Optimize connector configuration

### **Issue: Rate Limiting**

**Symptoms:**
- "Too many requests" errors
- Intermittent failures
- Quota exceeded messages

**Solutions:**

1. **Implement Request Throttling**
   - Add delays between rapid requests
   - Use proper retry logic with exponential backoff

2. **Check Service Limits**
   - Review Azure App Service quotas
   - Monitor API call patterns
   - Implement caching where appropriate

## 🛠️ Development & Testing Issues

### **Issue: Testing Connector in Power Apps**

**Symptoms:**
- Test operations fail
- Can't validate connector configuration
- Unclear error messages

**Solutions:**

1. **Use Proper Test Data**
   ```json
   {
     "toolName": "get_sales_analytics",
     "dynamicArguments": {}
   }
   ```

2. **Check Response Format**
   - Ensure MCP server returns proper JSON
   - Validate response structure matches connector definition

3. **Test Incrementally**
   - Start with simple tools (get_sales_analytics)
   - Add complexity once basic tools work
   - Test each authentication mode separately

### **Issue: Debugging MCP Tool Calls**

**Symptoms:**
- Unclear why tools aren't working
- Can't see what's being sent to MCP server
- Difficult to trace issues

**Solutions:**

1. **Enable Logging**
   - Check Azure App Service logs
   - Monitor application insights if configured
   - Use console logging in development

2. **Test Tools Directly**
   ```bash
   # Test MCP tool directly
   curl -k https://your-mcp-app-name.azurewebsites.net/mcp/call \
     -X POST \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{
       "method": "tools/call",
       "params": {
         "name": "get_sales_analytics",
         "arguments": {}
       }
     }'
   ```

3. **Use Development Tools**
   - Browser developer tools for network inspection
   - Postman for API testing
   - Power Apps maker portal for connector testing

## 📞 Getting Additional Help

### **Escalation Path**

1. **Self-Service Diagnostics**
   - Use this troubleshooting guide
   - Test with provided scripts
   - Check all configuration steps

2. **Community Support**
   - GitHub Issues for bug reports
   - Discussions for usage questions
   - Share logs and error messages

3. **Documentation Resources**
   - [Main Setup Guide](./README.md)
   - [Authentication Guide](../../architecture/AUTHENTICATION.md)
   - [API Documentation](../../../FabrikamApi/README.md)

### **Providing Effective Bug Reports**

When reporting issues, include:

1. **Environment Information**
   - Authentication mode (Disabled/JWT/Entra)
   - Azure deployment details
   - Copilot Studio environment

2. **Reproduction Steps**
   - Exact steps to reproduce the issue
   - Expected vs actual behavior
   - Screenshots if relevant

3. **Error Details**
   - Complete error messages
   - Server logs if available
   - Network traces if needed

4. **Configuration Details**
   - Connector configuration (without sensitive data)
   - Agent instructions
   - Connection settings

---

**💡 Still having issues?** Create a GitHub issue with detailed information about your problem, including error messages and steps to reproduce.
