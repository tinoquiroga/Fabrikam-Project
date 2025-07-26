# Script to inject new orders into Fabrikam API
# This script adds new orders to the seed data and triggers a re-seed

param(
    [string]$ApiBaseUrl = "http://localhost:7296",
    [string]$NewOrdersFile = "FabrikamApi\src\Data\SeedData\new-orders-2025.json",
    [string]$ExistingOrdersFile = "FabrikamApi\src\Data\SeedData\orders.json",
    [switch]$Production,
    [switch]$DryRun,
    [switch]$ReseedOnly  # Only trigger re-seed without merging files
)

# Set API URL based on environment
if ($Production) {
    $ApiBaseUrl = "https://fabrikam-api-dev.levelupcsp.com"
    Write-Host "🌍 Using PRODUCTION environment: $ApiBaseUrl" -ForegroundColor Yellow
    Write-Host "⚠️  This will add orders to the production database!" -ForegroundColor Red
    
    if (-not $DryRun) {
        $confirm = Read-Host "Are you sure you want to proceed? (y/N)"
        if ($confirm -ne "y" -and $confirm -ne "Y") {
            Write-Host "❌ Operation cancelled" -ForegroundColor Red
            exit 1
        }
    }
}
else {
    Write-Host "🏠 Using LOCAL development environment: $ApiBaseUrl" -ForegroundColor Cyan
}

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No actual API calls will be made" -ForegroundColor Magenta
}

Write-Host "`n📦 Injecting New Orders into Fabrikam Database" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Green

if ($ReseedOnly) {
    Write-Host "🔄 Re-seed only mode - triggering database re-seed without file changes" -ForegroundColor Cyan
}
else {
    # Check if new orders file exists
    if (-not (Test-Path $NewOrdersFile)) {
        Write-Host "❌ New orders file not found: $NewOrdersFile" -ForegroundColor Red
        exit 1
    }

    # Check if existing orders file exists
    if (-not (Test-Path $ExistingOrdersFile)) {
        Write-Host "❌ Existing orders file not found: $ExistingOrdersFile" -ForegroundColor Red
        exit 1
    }

    # Read new orders from JSON file
    try {
        $newOrdersJson = Get-Content $NewOrdersFile -Raw
        $newOrders = $newOrdersJson | ConvertFrom-Json
        Write-Host "✅ Loaded $($newOrders.Count) new orders from $NewOrdersFile" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error reading new orders file: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }

    # Read existing orders
    try {
        $existingOrdersJson = Get-Content $ExistingOrdersFile -Raw
        $existingOrders = $existingOrdersJson | ConvertFrom-Json
        Write-Host "✅ Loaded $($existingOrders.Count) existing orders from $ExistingOrdersFile" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error reading existing orders file: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }

    # Check for duplicate order numbers
    $existingOrderNumbers = $existingOrders | ForEach-Object { $_.orderNumber }
    $duplicates = $newOrders | Where-Object { $_.orderNumber -in $existingOrderNumbers }
    
    if ($duplicates.Count -gt 0) {
        Write-Host "⚠️  Found $($duplicates.Count) duplicate order numbers:" -ForegroundColor Yellow
        $duplicates | ForEach-Object { Write-Host "   - $($_.orderNumber)" -ForegroundColor Yellow }
        
        if (-not $DryRun) {
            $confirm = Read-Host "Continue anyway? (y/N)"
            if ($confirm -ne "y" -and $confirm -ne "Y") {
                Write-Host "❌ Operation cancelled" -ForegroundColor Red
                exit 1
            }
        }
    }

    # Merge orders (new orders will be added to existing ones)
    $allOrders = @()
    $allOrders += $existingOrders
    $allOrders += $newOrders

    Write-Host "📊 Total orders after merge: $($allOrders.Count)" -ForegroundColor Cyan

    if (-not $DryRun) {
        # Backup existing orders file
        $backupFile = $ExistingOrdersFile + ".backup." + (Get-Date -Format "yyyyMMdd-HHmmss")
        Copy-Item $ExistingOrdersFile $backupFile
        Write-Host "💾 Created backup: $backupFile" -ForegroundColor Green

        # Write merged orders to the existing orders file
        try {
            $mergedJson = $allOrders | ConvertTo-Json -Depth 10
            Set-Content $ExistingOrdersFile $mergedJson -Encoding UTF8
            Write-Host "✅ Updated $ExistingOrdersFile with merged orders" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Error writing merged orders: $($_.Exception.Message)" -ForegroundColor Red
            # Restore backup
            Copy-Item $backupFile $ExistingOrdersFile -Force
            Write-Host "🔄 Restored from backup" -ForegroundColor Yellow
            exit 1
        }
    }
    else {
        Write-Host "🔍 [DRY RUN] Would merge $($newOrders.Count) new orders with $($existingOrders.Count) existing orders" -ForegroundColor Magenta
        Write-Host "🔍 [DRY RUN] Would update: $ExistingOrdersFile" -ForegroundColor Magenta
    }
}

# Test API connectivity first
Write-Host "`n🔍 Testing API connectivity..." -ForegroundColor Cyan
try {
    if (-not $DryRun) {
        $healthResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/orders" -Method Get -TimeoutSec 10
        Write-Host "✅ API is accessible and responding" -ForegroundColor Green
    }
    else {
        Write-Host "🔍 [DRY RUN] Would test: GET $ApiBaseUrl/api/orders" -ForegroundColor Magenta
    }
}
catch {
    Write-Host "❌ API connectivity test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "ℹ️  Make sure the API server is running" -ForegroundColor Yellow
    exit 1
}

# Trigger database re-seed using the API
Write-Host "`n🌱 Triggering database re-seed..." -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "🔍 [DRY RUN] Would POST to: $ApiBaseUrl/api/seed/json" -ForegroundColor Magenta
    Write-Host "✅ [DRY RUN] Re-seed operation would be triggered" -ForegroundColor Green
}
else {
    try {
        $seedResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/seed/json" -Method Post -TimeoutSec 30
        Write-Host "✅ Database re-seed completed successfully!" -ForegroundColor Green
        Write-Host "   Message: $($seedResponse.Message)" -ForegroundColor Cyan
        Write-Host "   Method: $($seedResponse.Method)" -ForegroundColor Cyan
        Write-Host "   Timestamp: $($seedResponse.Timestamp)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "❌ Re-seed operation failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Summary
Write-Host "`n" + "=" * 50 -ForegroundColor Green
Write-Host "📊 INJECTION SUMMARY" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Green

if ($ReseedOnly) {
    Write-Host "🔄 Re-seed operation completed" -ForegroundColor Green
}
else {
    $newOrderCount = if ($DryRun) { (Get-Content $NewOrdersFile -Raw | ConvertFrom-Json).Count } else { $newOrders.Count }
    Write-Host "📦 New Orders Added: $newOrderCount" -ForegroundColor Green
    Write-Host "🌱 Database Re-seed: Completed" -ForegroundColor Green
}

if (-not $DryRun -and -not $ReseedOnly) {
    Write-Host "`n🎉 Orders have been successfully injected into the database!" -ForegroundColor Green
    Write-Host "💡 You can now test the enhanced date filtering with recent 2025 orders" -ForegroundColor Yellow
    Write-Host "🧪 Try: .\Test-Development.ps1 -Quick to verify the new data" -ForegroundColor Cyan
    Write-Host "📊 Try: curl ""$ApiBaseUrl/api/orders?fromDate=2025-06-01"" to see the new orders" -ForegroundColor Cyan
}
elseif ($DryRun) {
    Write-Host "`n🔍 Dry run completed successfully!" -ForegroundColor Magenta
    Write-Host "💡 Run without -DryRun to actually inject the orders" -ForegroundColor Yellow
}
elseif ($ReseedOnly) {
    Write-Host "`n🌱 Re-seed completed! Database refreshed with current seed data." -ForegroundColor Green
}

Write-Host "`n🚀 Script completed!" -ForegroundColor Green
