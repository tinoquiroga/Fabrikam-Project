# 🔐 HTTPS Certificate Quick Fix

## **Problem**: "The ASP.NET Core developer certificate is not trusted"

When you see this warning in the terminal or get browser security warnings:

## **Quick Solution:**

```powershell
# Trust the development certificate
dotnet dev-certs https --trust

# Click "Yes" when Windows asks for permission
```

## **If That Doesn't Work:**

```powershell
# Reset certificates completely
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Verify it worked
dotnet dev-certs https --check --trust
```

## **Test the Fix:**

1. Start the API server: `dotnet run --project FabrikamApi\src\FabrikamApi.csproj`
2. Open browser to: `https://localhost:7297`
3. Should see no security warnings

## **Why This Matters:**

- **API runs on HTTPS** by default for security
- **MCP server needs to trust the certificate** to call API endpoints
- **Browser warnings** are eliminated
- **VS Code REST client** works without SSL errors

## **Alternative - Use HTTP:**

If certificate issues persist, temporarily use HTTP:

```powershell
# Start with HTTP profile
dotnet run --project FabrikamApi\src\FabrikamApi.csproj --launch-profile http

# Update MCP config to use HTTP:
# FabrikamMcp/src/appsettings.Development.json
# "BaseUrl": "http://localhost:7296"
```

---

**This fix is usually needed once per development machine.**
