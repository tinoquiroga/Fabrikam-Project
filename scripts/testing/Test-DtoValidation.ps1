# Test-DtoValidation.ps1 - DTO Contract Validation for Testing
# Validates API responses against FabrikamContracts DTOs

# Customer DTO validation based on CustomerDetailDto and CustomerListItemDto
function Test-CustomerDtoStructure {
    param(
        [object]$Customer,
        [string]$DtoType = "Detail" # "Detail" or "ListItem"
    )
    
    if (-not $Customer) {
        return @{ IsValid = $false; Errors = @("Customer object is null") }
    }
    
    $errors = @()
    
    # Common fields for both DTOs
    if (-not $Customer.id) { $errors += "Missing required field: id" }
    if (-not $Customer.name) { $errors += "Missing required field: name" }
    if (-not $Customer.email) { $errors += "Missing required field: email" }
    
    if ($DtoType -eq "Detail") {
        # Additional fields for CustomerDetailDto
        if (-not $Customer.phone) { $errors += "Missing field: phone" }
        if (-not $Customer.region) { $errors += "Missing field: region" }
        if (-not $Customer.createdDate) { $errors += "Missing field: createdDate" }
        
        # Nested objects
        if (-not $Customer.address) { $errors += "Missing field: address" }
        if (-not $Customer.orderSummary) { $errors += "Missing field: orderSummary" }
        if (-not $Customer.supportTicketSummary) { $errors += "Missing field: supportTicketSummary" }
        
        # Validate address structure if present
        if ($Customer.address) {
            if (-not $Customer.address.address) { $errors += "Missing address.address field" }
            if (-not $Customer.address.city) { $errors += "Missing address.city field" }
            if (-not $Customer.address.state) { $errors += "Missing address.state field" }
        }
        
        # Validate order summary structure if present
        if ($Customer.orderSummary) {
            if ($Customer.orderSummary.totalOrders -eq $null) { $errors += "Missing orderSummary.totalOrders field" }
            if ($Customer.orderSummary.totalSpent -eq $null) { $errors += "Missing orderSummary.totalSpent field" }
        }
    }
    
    return @{
        IsValid = $errors.Count -eq 0
        Errors = $errors
        ValidatedFields = @("id", "name", "email") + $(if ($DtoType -eq "Detail") { @("phone", "region", "createdDate", "address", "orderSummary") } else { @() })
    }
}

# Product DTO validation based on ProductDto
function Test-ProductDtoStructure {
    param([object]$Product)
    
    if (-not $Product) {
        return @{ IsValid = $false; Errors = @("Product object is null") }
    }
    
    $errors = @()
    
    # Required fields based on ProductDto
    if (-not $Product.id) { $errors += "Missing required field: id" }
    if (-not $Product.name) { $errors += "Missing required field: name" }
    if (-not $Product.modelNumber) { $errors += "Missing required field: modelNumber" }
    if (-not $Product.category) { $errors += "Missing required field: category" }
    if ($Product.price -eq $null) { $errors += "Missing required field: price" }
    if ($Product.stockQuantity -eq $null) { $errors += "Missing required field: stockQuantity" }
    if ($Product.reorderLevel -eq $null) { $errors += "Missing required field: reorderLevel" }
    if (-not $Product.stockStatus) { $errors += "Missing required field: stockStatus" }
    if ($Product.deliveryDaysEstimate -eq $null) { $errors += "Missing required field: deliveryDaysEstimate" }
    
    # Type validation
    if ($Product.id -and -not ($Product.id -is [int])) { $errors += "Field 'id' should be integer" }
    if ($Product.price -ne $null -and -not ($Product.price -is [decimal] -or $Product.price -is [double])) { $errors += "Field 'price' should be decimal" }
    if ($Product.stockQuantity -ne $null -and -not ($Product.stockQuantity -is [int])) { $errors += "Field 'stockQuantity' should be integer" }
    
    # Optional fields (nullable in DTO)
    $optionalFields = @("dimensions", "squareFeet", "bedrooms", "bathrooms")
    
    return @{
        IsValid = $errors.Count -eq 0
        Errors = $errors
        ValidatedFields = @("id", "name", "modelNumber", "category", "price", "stockQuantity", "reorderLevel", "stockStatus", "deliveryDaysEstimate")
        OptionalFields = $optionalFields
    }
}

# Order DTO validation based on OrderDto
function Test-OrderDtoStructure {
    param([object]$Order)
    
    if (-not $Order) {
        return @{ IsValid = $false; Errors = @("Order object is null") }
    }
    
    $errors = @()
    
    # Required fields based on OrderDto
    if (-not $Order.id) { $errors += "Missing required field: id" }
    if (-not $Order.orderNumber) { $errors += "Missing required field: orderNumber" }
    if (-not $Order.status) { $errors += "Missing required field: status" }
    if (-not $Order.orderDate) { $errors += "Missing required field: orderDate" }
    if ($Order.total -eq $null) { $errors += "Missing required field: total" }
    if (-not $Order.customer) { $errors += "Missing required field: customer" }
    
    # Validate customer nested object
    if ($Order.customer) {
        if (-not $Order.customer.id) { $errors += "Missing customer.id field" }
        if (-not $Order.customer.name) { $errors += "Missing customer.name field" }
        if (-not $Order.customer.email) { $errors += "Missing customer.email field" }
    }
    
    # Type validation
    if ($Order.id -and -not ($Order.id -is [int])) { $errors += "Field 'id' should be integer" }
    if ($Order.total -ne $null -and -not ($Order.total -is [decimal] -or $Order.total -is [double])) { $errors += "Field 'total' should be decimal" }
    
    return @{
        IsValid = $errors.Count -eq 0
        Errors = $errors
        ValidatedFields = @("id", "orderNumber", "status", "orderDate", "total", "customer")
    }
}

# Authentication DTO validation
function Test-AuthenticationDtoStructure {
    param(
        [object]$AuthResponse,
        [string]$DtoType = "AuthenticationResponse" # "AuthenticationResponse", "UserInfo", etc.
    )
    
    if (-not $AuthResponse) {
        return @{ IsValid = $false; Errors = @("Authentication response object is null") }
    }
    
    $errors = @()
    
    switch ($DtoType) {
        "AuthenticationResponse" {
            # Check for either 'token' (simplified) or 'accessToken' (full DTO)
            if (-not $AuthResponse.token -and -not $AuthResponse.accessToken) { 
                $errors += "Missing required field: token or accessToken" 
            }
            
            # If using full AuthenticationResponse DTO structure
            if ($AuthResponse.accessToken) {
                if (-not $AuthResponse.refreshToken) { $errors += "Missing field: refreshToken" }
                if (-not $AuthResponse.expiresAt) { $errors += "Missing field: expiresAt" }
                if (-not $AuthResponse.tokenType) { $errors += "Missing field: tokenType" }
                if (-not $AuthResponse.user) { $errors += "Missing field: user" }
                
                # Validate nested user object
                if ($AuthResponse.user) {
                    $userValidation = Test-AuthenticationDtoStructure -AuthResponse $AuthResponse.user -DtoType "UserInfo"
                    if (-not $userValidation.IsValid) {
                        $errors += $userValidation.Errors | ForEach-Object { "user.$_" }
                    }
                }
            }
        }
        
        "UserInfo" {
            if (-not $AuthResponse.id) { $errors += "Missing required field: id" }
            if (-not $AuthResponse.email) { $errors += "Missing required field: email" }
            if (-not $AuthResponse.firstName) { $errors += "Missing required field: firstName" }
            if (-not $AuthResponse.lastName) { $errors += "Missing required field: lastName" }
            if (-not $AuthResponse.displayName) { $errors += "Missing required field: displayName" }
            if (-not $AuthResponse.roles) { $errors += "Missing required field: roles" }
            if ($AuthResponse.isActive -eq $null) { $errors += "Missing required field: isActive" }
        }
        
        "LoginRequest" {
            if (-not $AuthResponse.email) { $errors += "Missing required field: email" }
            if (-not $AuthResponse.password) { $errors += "Missing required field: password" }
        }
        
        "RegisterRequest" {
            if (-not $AuthResponse.email) { $errors += "Missing required field: email" }
            if (-not $AuthResponse.password) { $errors += "Missing required field: password" }
            if (-not $AuthResponse.firstName) { $errors += "Missing required field: firstName" }
            if (-not $AuthResponse.lastName) { $errors += "Missing required field: lastName" }
        }
    }
    
    return @{
        IsValid = $errors.Count -eq 0
        Errors = $errors
        ValidatedFields = @() # Could be enhanced to return which fields were validated
    }
}

# Generic DTO validation helper
function Test-ApiResponseStructure {
    param(
        [object]$Response,
        [string]$ExpectedType,
        [string]$TestDescription = "API Response"
    )
    
    $validation = switch ($ExpectedType) {
        "Customer" { Test-CustomerDtoStructure -Customer $Response }
        "CustomerDetail" { Test-CustomerDtoStructure -Customer $Response -DtoType "Detail" }
        "Product" { Test-ProductDtoStructure -Product $Response }
        "Order" { Test-OrderDtoStructure -Order $Response }
        "AuthenticationResponse" { Test-AuthenticationDtoStructure -AuthResponse $Response }
        "UserInfo" { Test-AuthenticationDtoStructure -AuthResponse $Response -DtoType "UserInfo" }
        default {
            @{ IsValid = $false; Errors = @("Unknown DTO type: $ExpectedType") }
        }
    }
    
    if ($validation.IsValid) {
        Write-Host "✅ $TestDescription - DTO structure valid ($ExpectedType)" -ForegroundColor Green
        return @{ Status = "Pass"; Type = "DTO Validation"; Details = "All required fields present" }
    } else {
        Write-Host "❌ $TestDescription - DTO structure invalid ($ExpectedType)" -ForegroundColor Red
        foreach ($error in $validation.Errors) {
            Write-Host "   • $error" -ForegroundColor Red
        }
        return @{ Status = "Fail"; Type = "DTO Validation"; Error = "DTO validation failed"; Details = $validation.Errors -join "; " }
    }
}

# Collection validation helper
function Test-ApiCollectionStructure {
    param(
        [array]$Collection,
        [string]$ExpectedItemType,
        [string]$TestDescription = "API Collection",
        [int]$MaxItemsToValidate = 3
    )
    
    $results = @()
    
    if (-not $Collection -or $Collection.Count -eq 0) {
        Write-Host "⚠️  $TestDescription - Collection is empty" -ForegroundColor Yellow
        return @{ Status = "Warning"; Type = "Collection Validation"; Details = "Empty collection" }
    }
    
    Write-Host "🔍 Validating $TestDescription ($($Collection.Count) items, checking first $MaxItemsToValidate)" -ForegroundColor Cyan
    
    $itemsToCheck = [Math]::Min($Collection.Count, $MaxItemsToValidate)
    $validItems = 0
    
    for ($i = 0; $i -lt $itemsToCheck; $i++) {
        $item = $Collection[$i]
        $itemValidation = Test-ApiResponseStructure -Response $item -ExpectedType $ExpectedItemType -TestDescription "Item $($i + 1)"
        
        if ($itemValidation.Status -eq "Pass") {
            $validItems++
        }
        
        $results += $itemValidation
    }
    
    if ($validItems -eq $itemsToCheck) {
        Write-Host "✅ $TestDescription - All checked items valid ($validItems/$itemsToCheck)" -ForegroundColor Green
        return @{ Status = "Pass"; Type = "Collection Validation"; Details = "All checked items valid ($validItems/$itemsToCheck)" }
    } else {
        Write-Host "❌ $TestDescription - Some items invalid ($validItems/$itemsToCheck)" -ForegroundColor Red
        return @{ Status = "Fail"; Type = "Collection Validation"; Details = "Some items invalid ($validItems/$itemsToCheck)" }
    }
}
