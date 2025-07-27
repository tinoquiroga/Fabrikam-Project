# 🔐 Check Entra Permissions for External ID Setup
# Determines which authentication strategy is feasible based on current permissions

param(
    [switch]$Verbose,
    [switch]$Export
)

Write-Host "🔐 Entra Permissions Assessment" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

# Initialize results object
$assessment = @{
    Timestamp           = Get-Date
    TenantInfo          = @{}
    UserInfo            = @{}
    Permissions         = @{}
    Recommendations     = @{}
    CanCreateExternalId = $false
    RecommendedStrategy = "AspNetIdentity"
}

try {
    # Get current user information
    Write-Host "`n👤 Current User Assessment..." -ForegroundColor Yellow
    
    $currentUser = az ad signed-in-user show --output json | ConvertFrom-Json
    $assessment.UserInfo = @{
        Id                = $currentUser.id
        UserPrincipalName = $currentUser.userPrincipalName
        DisplayName       = $currentUser.displayName
        JobTitle          = $currentUser.jobTitle
        Department        = $currentUser.department
    }
    
    Write-Host "✅ User: $($currentUser.displayName) ($($currentUser.userPrincipalName))" -ForegroundColor Green
    
    # Get tenant information
    Write-Host "`n🏢 Tenant Information..." -ForegroundColor Yellow
    
    $tenantInfo = az account show --output json | ConvertFrom-Json
    $assessment.TenantInfo = @{
        TenantId         = $tenantInfo.tenantId
        Name             = $tenantInfo.name
        SubscriptionId   = $tenantInfo.id
        SubscriptionName = $tenantInfo.name
    }
    
    Write-Host "✅ Tenant ID: $($tenantInfo.tenantId)" -ForegroundColor Green
    Write-Host "✅ Subscription: $($tenantInfo.name)" -ForegroundColor Green
    
}
catch {
    Write-Host "❌ Failed to get basic Azure information" -ForegroundColor Red
    Write-Host "   Run: az login" -ForegroundColor Cyan
    exit 1
}

# Check directory roles
Write-Host "`n🔑 Directory Role Assessment..." -ForegroundColor Yellow

try {
    # Get directory roles for current user
    $userRoles = az rest --method GET --url "https://graph.microsoft.com/v1.0/me/memberOf" --output json | ConvertFrom-Json
    
    $directoryRoles = @()
    $hasGlobalAdmin = $false
    $hasUserAdmin = $false
    $hasApplicationAdmin = $false
    
    foreach ($role in $userRoles.value) {
        if ($role.'@odata.type' -eq '#microsoft.graph.directoryRole') {
            $directoryRoles += $role.displayName
            
            switch ($role.displayName) {
                "Global Administrator" { $hasGlobalAdmin = $true }
                "User Administrator" { $hasUserAdmin = $true }
                "Application Administrator" { $hasApplicationAdmin = $true }
            }
        }
    }
    
    $assessment.Permissions = @{
        DirectoryRoles      = $directoryRoles
        HasGlobalAdmin      = $hasGlobalAdmin
        HasUserAdmin        = $hasUserAdmin
        HasApplicationAdmin = $hasApplicationAdmin
    }
    
    if ($hasGlobalAdmin) {
        Write-Host "✅ Global Administrator - Full Entra External ID permissions" -ForegroundColor Green
        $assessment.CanCreateExternalId = $true
        $assessment.RecommendedStrategy = "EntraExternalId"
    }
    elseif ($hasApplicationAdmin -and $hasUserAdmin) {
        Write-Host "✅ Application + User Administrator - Likely sufficient for External ID" -ForegroundColor Green
        $assessment.CanCreateExternalId = $true
        $assessment.RecommendedStrategy = "EntraExternalId"
    }
    elseif ($directoryRoles.Count -gt 0) {
        Write-Host "⚠️  Has directory roles but not Global Admin:" -ForegroundColor Yellow
        foreach ($role in $directoryRoles) {
            Write-Host "   - $role" -ForegroundColor White
        }
        Write-Host "⚠️  May have limited Entra External ID capabilities" -ForegroundColor Yellow
        $assessment.RecommendedStrategy = "AspNetIdentity"
    }
    else {
        Write-Host "❌ No directory administrator roles found" -ForegroundColor Red
        Write-Host "   Cannot create Entra External ID tenant" -ForegroundColor Cyan
        $assessment.RecommendedStrategy = "AspNetIdentity"
    }
    
}
catch {
    Write-Host "⚠️  Could not assess directory roles (may lack Graph permissions)" -ForegroundColor Yellow
    Write-Host "   Assuming limited permissions" -ForegroundColor Cyan
    $assessment.RecommendedStrategy = "AspNetIdentity"
}

# Check application registration permissions
Write-Host "`n📱 Application Registration Permissions..." -ForegroundColor Yellow

try {
    # Try to list applications (requires Application.Read.All or Application.ReadWrite.All)
    $apps = az ad app list --display-name "NonExistentTestApp" --output json | ConvertFrom-Json
    Write-Host "✅ Can list applications - Basic app registration permissions available" -ForegroundColor Green
    $assessment.Permissions.CanListApplications = $true
}
catch {
    Write-Host "⚠️  Cannot list applications - Limited app registration permissions" -ForegroundColor Yellow
    $assessment.Permissions.CanListApplications = $false
}

# Check Azure resource permissions
Write-Host "`n☁️ Azure Resource Permissions..." -ForegroundColor Yellow

try {
    # Check if can create resources in target resource group
    $resourceGroup = "rg-fabrikam-dev"
    $rgExists = az group show --name $resourceGroup --output json 2>$null
    
    if ($rgExists) {
        Write-Host "✅ Resource group '$resourceGroup' exists and accessible" -ForegroundColor Green
        $assessment.Permissions.CanAccessResourceGroup = $true
    }
    else {
        Write-Host "⚠️  Resource group '$resourceGroup' not found or not accessible" -ForegroundColor Yellow
        Write-Host "   May need to create resource group first" -ForegroundColor Cyan
        $assessment.Permissions.CanAccessResourceGroup = $false
    }
    
    # Check subscription-level permissions
    $subscription = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Can access subscription: $($subscription.name)" -ForegroundColor Green
    $assessment.Permissions.CanAccessSubscription = $true
    
}
catch {
    Write-Host "❌ Limited Azure resource permissions" -ForegroundColor Red
    $assessment.Permissions.CanAccessSubscription = $false
}

# Generate recommendations
Write-Host "`n🎯 Strategy Recommendations..." -ForegroundColor Yellow

$assessment.Recommendations = @{
    PrimaryStrategy = $assessment.RecommendedStrategy
    Reasoning       = ""
    NextSteps       = @()
    Alternatives    = @()
}

if ($assessment.RecommendedStrategy -eq "EntraExternalId") {
    $assessment.Recommendations.Reasoning = "Sufficient Entra permissions detected for External ID implementation"
    $assessment.Recommendations.NextSteps = @(
        "Proceed with Entra External ID tenant creation",
        "Configure user flows and application registration",
        "Implement OIDC authentication in API",
        "Keep ASP.NET Identity as backup option"
    )
    $assessment.Recommendations.Alternatives = @(
        "Implement dual strategy for maximum flexibility",
        "Use ASP.NET Identity for air-gapped deployments"
    )
    
    Write-Host "✅ Recommended: Entra External ID (Strategy 1)" -ForegroundColor Green
    Write-Host "   Sufficient permissions for modern identity platform" -ForegroundColor White
}
else {
    $assessment.Recommendations.Reasoning = "Limited Entra permissions - recommend ASP.NET Identity approach"
    $assessment.Recommendations.NextSteps = @(
        "Implement ASP.NET Core Identity with JWT tokens",
        "Set up Azure SQL Database for user storage",
        "Configure role-based access control",
        "Plan for future Entra External ID migration"
    )
    $assessment.Recommendations.Alternatives = @(
        "Request Global Administrator access for Entra External ID",
        "Implement dual strategy for future flexibility",
        "Use Azure AD B2C if organization permits"
    )
    
    Write-Host "⚠️  Recommended: ASP.NET Core Identity (Strategy 2)" -ForegroundColor Yellow
    Write-Host "   Limited Entra permissions detected" -ForegroundColor White
}

# Display next steps
Write-Host "`n📋 Immediate Next Steps:" -ForegroundColor Yellow
foreach ($step in $assessment.Recommendations.NextSteps) {
    Write-Host "   • $step" -ForegroundColor White
}

# Export results if requested
if ($Export) {
    $exportPath = "docs/deployment/entra-permissions-assessment.json"
    $assessment | ConvertTo-Json -Depth 5 | Out-File -FilePath $exportPath -Encoding UTF8
    Write-Host "`n📄 Assessment exported to: $exportPath" -ForegroundColor Cyan
}

# Display configuration recommendations
Write-Host "`n⚙️ Configuration Recommendations:" -ForegroundColor Yellow

if ($assessment.RecommendedStrategy -eq "EntraExternalId") {
    Write-Host @"
   Set in appsettings.json:
   {
     "Authentication": {
       "Strategy": "EntraExternalId",
       "FallbackStrategy": "AspNetIdentity"
     }
   }
"@ -ForegroundColor Cyan
}
else {
    Write-Host @"
   Set in appsettings.json:
   {
     "Authentication": {
       "Strategy": "AspNetIdentity",
       "EnableEntraFallback": false
     }
   }
"@ -ForegroundColor Cyan
}

Write-Host "`n🔄 Script completed successfully" -ForegroundColor Green
Write-Host "Ready to proceed with $($assessment.RecommendedStrategy) implementation" -ForegroundColor Green

# Return assessment for further processing
return $assessment
