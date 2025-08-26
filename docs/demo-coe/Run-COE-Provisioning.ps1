# 🚀 Quick COE User Provisioning Execution Script
# This script runs the provisioning with the correct parameters for the COE demo

# Configuration for COE Demo
$UserDataFile = "coe-users-actual.csv"  # Use actual user data file (gitignored)
$TenantDomain = "fabrikam.cspsecurityaccelerate.com"
$AzureSubscriptionId = "b7699934-0c99-4899-8799-763fffc90878"

Write-Host "🎯 COE User Provisioning for Fabrikam Demo" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if actual user data file exists
if (-not (Test-Path $UserDataFile)) {
    Write-Host "❌ Actual user data file not found: $UserDataFile" -ForegroundColor Red
    Write-Host ""
    Write-Host "📋 To create the user data file:" -ForegroundColor Yellow
    Write-Host "1. Copy the template: Copy-Item 'coe-users.csv' '$UserDataFile'" -ForegroundColor White
    Write-Host "2. Edit with real user data: notepad '$UserDataFile'" -ForegroundColor White
    Write-Host "3. The actual file is gitignored for security" -ForegroundColor White
    Write-Host ""
    
    $createFile = Read-Host "Would you like to create the file from template now? (yes/no)"
    if ($createFile -eq "yes") {
        if (Test-Path "coe-users.csv") {
            Copy-Item "coe-users.csv" $UserDataFile
            Write-Host "✅ Created $UserDataFile from template" -ForegroundColor Green
            Write-Host "📝 Please edit this file with actual user data before proceeding" -ForegroundColor Yellow
            Start-Process notepad $UserDataFile
            Read-Host "Press Enter after editing the file to continue"
        }
        else {
            Write-Host "❌ Template file 'coe-users.csv' not found" -ForegroundColor Red
            exit 1
        }
    }
    else {
        exit 1
    }
}

if (-not (Test-Path "Provision-COE-Users.ps1")) {
    Write-Host "❌ Provisioning script not found: Provision-COE-Users.ps1" -ForegroundColor Red
    Write-Host "Please ensure the PowerShell script is in the current directory." -ForegroundColor Yellow
    exit 1
}

# Ask user what they want to do
Write-Host "Choose an option:" -ForegroundColor Yellow
Write-Host "1. 🧪 Test run (WhatIf mode - no changes made)" -ForegroundColor White
Write-Host "2. 🚀 Execute provisioning (creates users and resources)" -ForegroundColor White
Write-Host "3. ❌ Cancel" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Enter your choice (1, 2, or 3)"

switch ($choice) {
    "1" {
        Write-Host "🧪 Running in test mode..." -ForegroundColor Yellow
        .\Provision-COE-Users.ps1 -UserDataFile $UserDataFile `
            -TenantDomain $TenantDomain `
            -AzureSubscriptionId $AzureSubscriptionId `
            -WhatIf
    }
    "2" {
        Write-Host "⚠️  WARNING: This will create users and Azure resources!" -ForegroundColor Magenta
        $confirm = Read-Host "Are you sure you want to proceed? (yes/no)"
        
        if ($confirm -eq "yes") {
            Write-Host "🚀 Executing provisioning..." -ForegroundColor Green
            .\Provision-COE-Users.ps1 -UserDataFile $UserDataFile `
                -TenantDomain $TenantDomain `
                -AzureSubscriptionId $AzureSubscriptionId
        }
        else {
            Write-Host "❌ Provisioning cancelled by user." -ForegroundColor Yellow
        }
    }
    "3" {
        Write-Host "❌ Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host "❌ Invalid choice. Please run the script again." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "📚 For more information, see COE-PROVISIONING.md" -ForegroundColor Cyan
