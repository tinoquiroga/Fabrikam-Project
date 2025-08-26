# 🚀 Quick COE User Provisioning Execution Script
# This script runs the provisioning with configuration from coe-config.json

Write-Host "🎯 COE User Provisioning for Fabrikam Demo" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if config file exists
$configFile = "coe-config.json"
if (-not (Test-Path $configFile)) {
    Write-Host "❌ Configuration file not found: $configFile" -ForegroundColor Red
    Write-Host ""
    Write-Host "📋 To create the configuration file:" -ForegroundColor Yellow
    Write-Host "1. Copy the template: Copy-Item 'coe-config-template.json' '$configFile'" -ForegroundColor White
    Write-Host "2. Edit with your tenant details: notepad '$configFile'" -ForegroundColor White
    Write-Host "3. The config file is gitignored for security" -ForegroundColor White
    Write-Host ""
    
    $createFile = Read-Host "Would you like to create the config file from template now? (yes/no)"
    if ($createFile -eq "yes") {
        if (Test-Path "coe-config-template.json") {
            Copy-Item "coe-config-template.json" $configFile
            Write-Host "✅ Created $configFile from template" -ForegroundColor Green
            Write-Host "📝 Please edit this file with actual tenant details before proceeding" -ForegroundColor Yellow
            Start-Process notepad $configFile
            Read-Host "Press Enter after editing the file to continue"
        }
        else {
            Write-Host "❌ Template file 'coe-config-template.json' not found" -ForegroundColor Red
            exit 1
        }
    }
    else {
        exit 1
    }
}

# Load configuration
try {
    $config = Get-Content $configFile | ConvertFrom-Json
    
    # Validate required fields
    if (-not $config.tenantDomain) { throw "tenantDomain is required in config" }
    if (-not $config.azureSubscriptionId) { throw "azureSubscriptionId is required in config" }
    if (-not $config.users -or $config.users.Count -eq 0) { throw "users array is required and must contain at least one user" }
    
    Write-Host "✅ Loaded configuration from $configFile" -ForegroundColor Green
    Write-Host "   Tenant: $($config.tenantDomain)" -ForegroundColor White
    Write-Host "   Subscription: $($config.azureSubscriptionId)" -ForegroundColor White
    Write-Host "   Users: $($config.users.Count)" -ForegroundColor White
}
catch {
    Write-Host "❌ Failed to load configuration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check and install required PowerShell modules
Write-Host ""
Write-Host "📦 Checking PowerShell module dependencies..." -ForegroundColor Cyan

# Define required modules with minimum versions
$requiredModules = @(
    @{ Name = "Microsoft.Graph.Authentication"; MinVersion = "2.0.0" },
    @{ Name = "Microsoft.Graph.Users"; MinVersion = "2.0.0" },
    @{ Name = "Microsoft.Graph.Identity.DirectoryManagement"; MinVersion = "2.0.0" },
    @{ Name = "Az.Accounts"; MinVersion = "2.0.0" },
    @{ Name = "Az.Resources"; MinVersion = "2.0.0" }
)

$modulesToInstall = @()
$modulesInstalled = @()

foreach ($moduleInfo in $requiredModules) {
    $moduleName = $moduleInfo.Name
    $minVersion = [version]$moduleInfo.MinVersion
    
    Write-Host "   Checking $moduleName..." -ForegroundColor Gray
    
    # Check if module is installed
    $installedModule = Get-Module -ListAvailable -Name $moduleName | 
                      Where-Object { [version]$_.Version -ge $minVersion } | 
                      Sort-Object Version -Descending | 
                      Select-Object -First 1
    
    if ($installedModule) {
        Write-Host "     ✅ $moduleName v$($installedModule.Version) (required: v$minVersion+)" -ForegroundColor Green
        $modulesInstalled += $moduleName
    }
    else {
        Write-Host "     ❌ $moduleName not found or version too old (required: v$minVersion+)" -ForegroundColor Red
        $modulesToInstall += $moduleInfo
    }
}

# Install missing modules if needed
if ($modulesToInstall.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️  Missing required modules detected!" -ForegroundColor Yellow
    Write-Host "The following modules need to be installed:" -ForegroundColor White
    
    foreach ($moduleInfo in $modulesToInstall) {
        Write-Host "   • $($moduleInfo.Name) (v$($moduleInfo.MinVersion)+)" -ForegroundColor White
    }
    
    Write-Host ""
    $installChoice = Read-Host "Would you like to install missing modules now? (yes/no)"
    
    if ($installChoice -eq "yes") {
        Write-Host ""
        Write-Host "🔄 Installing missing modules..." -ForegroundColor Cyan
        
        foreach ($moduleInfo in $modulesToInstall) {
            $moduleName = $moduleInfo.Name
            try {
                Write-Host "   Installing $moduleName..." -ForegroundColor Gray
                Install-Module -Name $moduleName -Force -AllowClobber -Scope CurrentUser -Repository PSGallery
                Write-Host "     ✅ $moduleName installed successfully" -ForegroundColor Green
            }
            catch {
                Write-Host "     ❌ Failed to install $moduleName`: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host ""
                Write-Host "💡 Try running PowerShell as Administrator or manually install:" -ForegroundColor Yellow
                Write-Host "   Install-Module $moduleName -Force" -ForegroundColor White
                exit 1
            }
        }
        
        Write-Host ""
        Write-Host "✅ All required modules installed successfully!" -ForegroundColor Green
        
        # Import newly installed modules
        Write-Host "📥 Loading newly installed modules..." -ForegroundColor Cyan
        foreach ($moduleInfo in $modulesToInstall) {
            $moduleName = $moduleInfo.Name
            try {
                Import-Module $moduleName -Force
                Write-Host "   ✅ $moduleName loaded" -ForegroundColor Green
            }
            catch {
                Write-Host "   ⚠️  Warning: Could not load $moduleName - may need to restart PowerShell" -ForegroundColor Yellow
            }
        }
    }
    else {
        Write-Host ""
        Write-Host "❌ Cannot proceed without required modules" -ForegroundColor Red
        Write-Host "💡 Please install the missing modules manually:" -ForegroundColor Yellow
        foreach ($moduleInfo in $modulesToInstall) {
            Write-Host "   Install-Module $($moduleInfo.Name) -Force" -ForegroundColor White
        }
        exit 1
    }
}
else {
    Write-Host "   ✅ All required modules are available!" -ForegroundColor Green
}

# Final verification - check if critical commands are available
Write-Host ""
Write-Host "🔍 Final module verification..." -ForegroundColor Cyan
$criticalCommands = @("Connect-MgGraph", "New-MgUser", "Connect-AzAccount", "New-AzResourceGroup")
$unavailableCommands = @()

foreach ($cmd in $criticalCommands) {
    if (Get-Command $cmd -ErrorAction SilentlyContinue) {
        Write-Host "   ✅ $cmd available" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ $cmd not available" -ForegroundColor Red
        $unavailableCommands += $cmd
    }
}

if ($unavailableCommands.Count -gt 0) {
    Write-Host ""
    Write-Host "❌ Some critical commands are still not available!" -ForegroundColor Red
    Write-Host "💡 You may need to:" -ForegroundColor Yellow
    Write-Host "   1. Restart PowerShell completely" -ForegroundColor White
    Write-Host "   2. Run PowerShell as Administrator" -ForegroundColor White
    Write-Host "   3. Manually install modules: Install-Module Microsoft.Graph.Users, Az.Resources -Force" -ForegroundColor White
    
    $continueAnyway = Read-Host "Do you want to continue anyway? (yes/no)"
    if ($continueAnyway -ne "yes") {
        exit 1
    }
}
else {
    Write-Host "   ✅ All critical commands verified!" -ForegroundColor Green
}

if (-not (Test-Path "Provision-COE-Users.ps1")) {
    Write-Host "❌ Provisioning script not found: Provision-COE-Users.ps1" -ForegroundColor Red
    Write-Host "Please ensure the PowerShell script is in the current directory." -ForegroundColor Yellow
    exit 1
}

# Ask user what they want to do
Write-Host "Choose an option:" -ForegroundColor Yellow
Write-Host "1. 🧪 Test run (WhatIf mode - no changes made)" -ForegroundColor White
Write-Host "2. 🚀 Actual provisioning (creates users and resource groups)" -ForegroundColor White
Write-Host "3. 🧹 Clear authentication cache (fixes account selection issues)" -ForegroundColor White
Write-Host "4. ❌ Cancel" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Enter your choice (1, 2, 3, or 4)"

switch ($choice) {
    "1" {
        Write-Host "🧪 Running in test mode..." -ForegroundColor Cyan
        .\Provision-COE-Users.ps1 -ConfigFile $configFile -WhatIf
    }
    "2" {
        Write-Host "🚀 Running actual provisioning..." -ForegroundColor Cyan
        Write-Host "⚠️  This will create real users and resource groups!" -ForegroundColor Yellow
        $confirm = Read-Host "Are you sure? (yes/no)"
        if ($confirm -eq "yes") {
            .\Provision-COE-Users.ps1 -ConfigFile $configFile
        }
        else {
            Write-Host "❌ Cancelled" -ForegroundColor Red
        }
    }
    "3" {
        Write-Host "🧹 Clearing authentication cache..." -ForegroundColor Cyan
        try {
            Disconnect-MgGraph -ErrorAction SilentlyContinue | Out-Null
            Disconnect-AzAccount -ErrorAction SilentlyContinue | Out-Null
            Clear-AzContext -Force -ErrorAction SilentlyContinue | Out-Null
            Write-Host "✅ Authentication cache cleared" -ForegroundColor Green
            Write-Host "💡 You can now run the provisioning script with fresh authentication" -ForegroundColor Yellow
        }
        catch {
            Write-Host "⚠️  Cache clearing completed (some commands may not be available)" -ForegroundColor Yellow
        }
    }
    "4" {
        Write-Host "❌ Cancelled" -ForegroundColor Red
        exit 0
    }
    default {
        Write-Host "❌ Invalid choice" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "📚 For more information, see COE-PROVISIONING.md" -ForegroundColor Cyan
