# 🔐 Setup ASP.NET Identity Authentication Strategy
# Creates Azure resources and configures authentication for Strategy 2

param(
    [Parameter(Mandatory = $true)]
    [string]$InstanceName = "fabrikam-dev",
    
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup = "rg-fabrikam-dev",
    
    [string]$Location = "eastus2",
    [string]$SqlServerAdmin = "fabrikam-admin",
    [string]$DatabaseSku = "S0",
    [switch]$DryRun,
    [switch]$SkipDatabase
)

Write-Host "🔐 Setting up ASP.NET Identity Authentication Strategy" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

# Validate Azure context
Write-Host "`n📋 Validating Azure Context..." -ForegroundColor Yellow

try {
    $context = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Subscription: $($context.name)" -ForegroundColor Green
    Write-Host "✅ Tenant: $($context.tenantId)" -ForegroundColor Green
    
    # Verify resource group exists
    $rg = az group show --name $ResourceGroup --output json 2>$null | ConvertFrom-Json
    if ($rg) {
        Write-Host "✅ Resource Group: $ResourceGroup exists" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Resource Group: $ResourceGroup not found" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Azure CLI not authenticated" -ForegroundColor Red
    Write-Host "   Run: az login" -ForegroundColor Cyan
    exit 1
}

# Register required resource providers
Write-Host "`n🔧 Checking Resource Providers..." -ForegroundColor Yellow

try {
    # Check if Microsoft.Sql is registered
    $sqlProvider = az provider show --namespace "Microsoft.Sql" --output json | ConvertFrom-Json
    
    if ($sqlProvider.registrationState -eq "Registered") {
        Write-Host "✅ Microsoft.Sql provider already registered" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Microsoft.Sql provider not registered: $($sqlProvider.registrationState)" -ForegroundColor Yellow
        
        if ($DryRun) {
            Write-Host "🧪 Would register Microsoft.Sql resource provider" -ForegroundColor Cyan
        }
        else {
            Write-Host "   Registering Microsoft.Sql provider..." -ForegroundColor White
            az provider register --namespace "Microsoft.Sql" --output none
            
            # Wait for registration to complete (can take a few minutes)
            Write-Host "   Waiting for registration to complete..." -ForegroundColor White
            $timeout = 300 # 5 minutes
            $elapsed = 0
            
            do {
                Start-Sleep -Seconds 10
                $elapsed += 10
                $currentState = az provider show --namespace "Microsoft.Sql" --query "registrationState" --output tsv
                Write-Host "   Registration state: $currentState (${elapsed}s elapsed)" -ForegroundColor Gray
                
                if ($elapsed -ge $timeout) {
                    Write-Host "⚠️  Registration timeout - continuing anyway" -ForegroundColor Yellow
                    break
                }
            } while ($currentState -ne "Registered")
            
            if ($currentState -eq "Registered") {
                Write-Host "✅ Microsoft.Sql provider registered successfully" -ForegroundColor Green
            }
        }
    }
}
catch {
    Write-Host "⚠️  Could not check/register Microsoft.Sql provider: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   You may need to register manually: az provider register --namespace Microsoft.Sql" -ForegroundColor Cyan
}

if ($DryRun) {
    Write-Host "`n🧪 DRY RUN MODE - No resources will be created" -ForegroundColor Yellow
}

# Generate secure passwords and keys
Write-Host "`n🔑 Generating Secure Credentials..." -ForegroundColor Yellow

function New-SecurePassword {
    param([int]$Length = 16)
    $chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*"
    return -join ((1..$Length) | ForEach { $chars[(Get-Random -Maximum $chars.Length)] })
}

function New-JwtSecret {
    $bytes = New-Object byte[] 32
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}

$sqlPassword = New-SecurePassword -Length 20
$jwtSecret = New-JwtSecret

Write-Host "✅ SQL Server password generated" -ForegroundColor Green
Write-Host "✅ JWT secret key generated" -ForegroundColor Green

# Create Azure SQL Database
if (-not $SkipDatabase) {
    Write-Host "`n💾 Setting up Azure SQL Database..." -ForegroundColor Yellow
    
    $sqlServerName = "$InstanceName-sql"
    $databaseName = "$InstanceName-db"
    
    if ($DryRun) {
        Write-Host "🧪 Would create SQL Server: $sqlServerName" -ForegroundColor Cyan
        Write-Host "🧪 Would create Database: $databaseName" -ForegroundColor Cyan
    }
    else {
        try {
            # Check if SQL server already exists
            $existingServer = az sql server show --name $sqlServerName --resource-group $ResourceGroup --output json 2>$null | ConvertFrom-Json
            
            if ($existingServer) {
                Write-Host "⚠️  SQL Server $sqlServerName already exists" -ForegroundColor Yellow
            }
            else {
                Write-Host "   Creating SQL Server: $sqlServerName..." -ForegroundColor White
                az sql server create `
                    --name $sqlServerName `
                    --resource-group $ResourceGroup `
                    --location $Location `
                    --admin-user $SqlServerAdmin `
                    --admin-password $sqlPassword `
                    --output none
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ SQL Server created successfully" -ForegroundColor Green
                }
                else {
                    Write-Host "❌ Failed to create SQL Server" -ForegroundColor Red
                    exit 1
                }
            }
            
            # Check if database already exists
            $existingDb = az sql db show --server $sqlServerName --resource-group $ResourceGroup --name $databaseName --output json 2>$null | ConvertFrom-Json
            
            if ($existingDb) {
                Write-Host "⚠️  Database $databaseName already exists" -ForegroundColor Yellow
            }
            else {
                Write-Host "   Creating Database: $databaseName..." -ForegroundColor White
                az sql db create `
                    --server $sqlServerName `
                    --resource-group $ResourceGroup `
                    --name $databaseName `
                    --service-objective $DatabaseSku `
                    --output none
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Database created successfully" -ForegroundColor Green
                }
                else {
                    Write-Host "❌ Failed to create Database" -ForegroundColor Red
                    exit 1
                }
            }
            
            # Configure firewall for Azure services
            Write-Host "   Configuring firewall rules..." -ForegroundColor White
            az sql server firewall-rule create `
                --server $sqlServerName `
                --resource-group $ResourceGroup `
                --name "AllowAzureServices" `
                --start-ip-address "0.0.0.0" `
                --end-ip-address "0.0.0.0" `
                --output none
            
            # Add current IP for development access
            $currentIp = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
            az sql server firewall-rule create `
                --server $sqlServerName `
                --resource-group $ResourceGroup `
                --name "DeveloperAccess" `
                --start-ip-address $currentIp `
                --end-ip-address $currentIp `
                --output none
            
            Write-Host "✅ Firewall rules configured" -ForegroundColor Green
            
        }
        catch {
            Write-Host "❌ Error creating database resources: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    }
}
else {
    Write-Host "`n💾 Skipping database setup (--SkipDatabase)" -ForegroundColor Yellow
}

# Generate configuration files
Write-Host "`n⚙️ Generating Configuration..." -ForegroundColor Yellow

$connectionString = "Server=$sqlServerName.database.windows.net;Database=$databaseName;User Id=$SqlServerAdmin;Password=$sqlPassword;Encrypt=true;TrustServerCertificate=false;"

$appSettings = @{
    "Logging"           = @{
        "LogLevel" = @{
            "Default"              = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    "Authentication"    = @{
        "Strategy"       = "AspNetIdentity"
        "AspNetIdentity" = @{
            "Jwt"      = @{
                "SecretKey"                  = $jwtSecret
                "Issuer"                     = "https://$InstanceName-api.levelupcsp.com"
                "Audience"                   = "fabrikam-api"
                "ExpirationMinutes"          = 15
                "RefreshTokenExpirationDays" = 7
            }
            "Password" = @{
                "RequiredLength"         = 8
                "RequireDigit"           = $true
                "RequireLowercase"       = $true
                "RequireUppercase"       = $true
                "RequireNonAlphanumeric" = $true
                "MaxFailedAttempts"      = 5
                "LockoutDuration"        = "00:15:00"
            }
        }
    }
    "ConnectionStrings" = @{
        "DefaultConnection" = $connectionString
    }
    "Instance"          = @{
        "Name"          = $InstanceName
        "Environment"   = "Development"
        "ResourceGroup" = $ResourceGroup
    }
}

$configPath = "FabrikamApi/src/appsettings.Authentication.json"
$appSettings | ConvertTo-Json -Depth 10 | Out-File -FilePath $configPath -Encoding UTF8

Write-Host "✅ Configuration saved to: $configPath" -ForegroundColor Green

# Generate user secrets for development
Write-Host "`n🔐 Setting up User Secrets..." -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "🧪 Would set user secrets for development" -ForegroundColor Cyan
}
else {
    try {
        Push-Location "FabrikamApi/src"
        
        # Initialize user secrets if not already done
        dotnet user-secrets init --force
        
        # Set sensitive configuration in user secrets
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString
        dotnet user-secrets set "Authentication:AspNetIdentity:Jwt:SecretKey" $jwtSecret
        
        Pop-Location
        Write-Host "✅ User secrets configured" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️  Could not set user secrets: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   Manual setup may be required" -ForegroundColor Cyan
        if (Get-Location) { Pop-Location }
    }
}

# Generate deployment summary
Write-Host "`n📋 Deployment Summary" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow

$summary = @{
    Timestamp     = Get-Date
    Strategy      = "ASP.NET Core Identity"
    Instance      = $InstanceName
    ResourceGroup = $ResourceGroup
    Resources     = @{
        SqlServer   = "$sqlServerName.database.windows.net"
        Database    = $databaseName
        DatabaseSku = $DatabaseSku
    }
    Configuration = @{
        ConfigFile     = $configPath
        UserSecretsSet = $true
        JwtIssuer      = "https://$InstanceName-api.levelupcsp.com"
    }
    Credentials   = @{
        SqlAdmin    = $SqlServerAdmin
        SqlPassword = "[SECURE - Check deployment notes]"
        JwtSecret   = "[SECURE - Check user secrets]"
    }
    NextSteps     = @(
        "Update FabrikamApi to use Authentication strategy",
        "Run database migrations to create Identity tables",
        "Test user registration and login endpoints", 
        "Configure role-based authorization",
        "Update MCP tools for authenticated access"
    )
}

# Save deployment summary
$summaryPath = "docs/deployment/deployment-summary-$InstanceName.json"
$summary | ConvertTo-Json -Depth 5 | Out-File -FilePath $summaryPath -Encoding UTF8

Write-Host "Instance: $InstanceName" -ForegroundColor White
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "SQL Server: $sqlServerName.database.windows.net" -ForegroundColor White
Write-Host "Database: $databaseName" -ForegroundColor White
Write-Host "Authentication: ASP.NET Core Identity + JWT" -ForegroundColor White

Write-Host "`n📄 Deployment summary saved to: $summaryPath" -ForegroundColor Cyan

# Display secure credentials (one time only)
Write-Host "`n🔐 SECURE CREDENTIALS (Save immediately):" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host "SQL Admin User: $SqlServerAdmin" -ForegroundColor Yellow
Write-Host "SQL Admin Password: $sqlPassword" -ForegroundColor Yellow
Write-Host "JWT Secret Key: $jwtSecret" -ForegroundColor Yellow
Write-Host "`n⚠️  Store these credentials securely - they won't be displayed again!" -ForegroundColor Red

Write-Host "`n🎯 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Store credentials in a secure location" -ForegroundColor White
Write-Host "2. Update FabrikamApi project with authentication code" -ForegroundColor White
Write-Host "3. Run database migrations: dotnet ef database update" -ForegroundColor White
Write-Host "4. Test authentication endpoints" -ForegroundColor White
Write-Host "5. Update GitHub Issue #3 with progress" -ForegroundColor White

Write-Host "`n✅ ASP.NET Identity Authentication Setup Complete!" -ForegroundColor Green
