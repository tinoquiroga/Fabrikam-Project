# 🔄 **Fabrikam DTO Architecture Schema**

## 📋 **Core Principle: DTOs Must Match API Responses Exactly**

This document defines the DTO strategy for the Fabrikam project to ensure alignment between API responses, MCP tools, and GitHub Copilot expectations.

---

## 🎯 **Schema Design Rules**

### **Rule 1: 1:1 Property Mapping**
- DTO properties must match API response properties **exactly**
- Property names must use **same casing** as API responses
- No additional properties that don't exist in API responses
- No missing properties that exist in API responses

### **Rule 2: Simple Structure Alignment**
- API returns simple anonymous objects → DTOs should be simple classes
- Avoid deep nesting unless API response has deep nesting
- Use nullable properties only when API can return null

### **Rule 3: Documentation Standards**
```csharp
/// <summary>
/// [Clear description matching API endpoint documentation]
/// This DTO aligns exactly with [ControllerName].[MethodName]() API response
/// </summary>
public class [Endpoint]Dto
{
    /// <summary>
    /// [Property description] - matches API property '[propertyName]'
    /// </summary>
    public [Type] PropertyName { get; set; } = [default];
}
```

---

## 📊 **Current API Response Structures**

### **Sales Analytics** (`/api/orders/analytics`)
```json
{
  "summary": {
    "totalOrders": 42,
    "totalRevenue": 156789.50,
    "averageOrderValue": 3733.08,
    "period": {
      "fromDate": "2024-01-01",
      "toDate": "2024-12-31"
    }
  },
  "byStatus": [
    { "status": "Pending", "count": 15, "revenue": 45230.75 }
  ],
  "byRegion": [
    { "region": "West", "count": 12, "revenue": 38940.25 }
  ],
  "recentTrends": [
    { "date": "2024-07-24", "orders": 3, "revenue": 8750.00 }
  ]
}
```

### **Customer List** (`/api/customers`)
```json
[
  {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "(555) 123-4567",
    "city": "Seattle",
    "state": "WA",
    "region": "West",
    "createdDate": "2024-01-15T08:30:00Z",
    "orderCount": 3,
    "totalSpent": 15750.00
  }
]
```

### **Product List** (`/api/products`)
```json
[
  {
    "id": 1,
    "name": "Cozy Cottage 1200",
    "modelNumber": "CC-1200",
    "category": "SingleFamily",
    "price": 89500,
    "stockQuantity": 25,
    "reorderLevel": 5,
    "stockStatus": "In Stock",
    "dimensions": "40x30",
    "squareFeet": 1200,
    "bedrooms": 2,
    "bathrooms": 2,
    "deliveryDaysEstimate": 45
  }
]
```

---

## 🛠️ **Implementation Strategy**

### **Phase 1: Create Aligned DTOs**
1. Replace existing complex DTOs with simple ones matching API responses
2. Ensure exact property name and type matching
3. Add clear documentation linking to API endpoints

### **Phase 2: Update MCP Tools**
1. Replace JsonDocument parsing with DTO deserialization
2. Maintain all business logic and formatting
3. Add error handling for DTO deserialization failures

### **Phase 3: Schema Validation**
1. Add unit tests ensuring DTO-API alignment
2. Create CI checks to validate DTO structure matches API responses
3. Documentation updates

---

## 🤖 **GitHub Copilot Guidelines**

When working with DTOs in this project:

1. **Always check this schema document first**
2. **Use DTOs that match API responses exactly**
3. **Do not create complex nested structures unless API returns them**
4. **Follow the 1:1 property mapping rule**
5. **Include XML documentation referencing the API endpoint**

### **Example: Correct DTO Creation**
```csharp
/// <summary>
/// Sales analytics response structure
/// This DTO aligns exactly with OrdersController.GetSalesAnalytics() API response
/// </summary>
public class SalesAnalyticsDto
{
    /// <summary>
    /// Summary statistics - matches API property 'summary'
    /// </summary>
    public SalesSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Status breakdown - matches API property 'byStatus'
    /// </summary>
    public List<SalesByStatusDto> ByStatus { get; set; } = new();
    
    /// <summary>
    /// Region breakdown - matches API property 'byRegion'
    /// </summary>
    public List<SalesByRegionDto> ByRegion { get; set; } = new();
    
    /// <summary>
    /// Daily trends - matches API property 'recentTrends'
    /// </summary>
    public List<SalesTrendDto> RecentTrends { get; set; } = new();
}
```

---

## ✅ **Validation Checklist**

Before committing DTO changes:

- [ ] DTO properties match API response exactly
- [ ] Property names use same casing as API
- [ ] No extra properties not in API response  
- [ ] No missing properties from API response
- [ ] XML documentation references correct API endpoint
- [ ] MCP tools can deserialize successfully
- [ ] Unit tests pass
- [ ] Integration tests pass

---

## 🔄 **Migration Status**

- ✅ **SalesAnalyticsDto**: Needs alignment (RecentTrends vs DailyTrends)
- ❌ **CustomerInfoDto**: Needs complete replacement (overly complex)
- ❌ **ProductCatalogDto**: Needs complete replacement (overly complex)
- ❌ **InventoryStatusDto**: Needs alignment check
- ❌ **OrderDto**: Needs alignment check

---

This schema ensures GitHub Copilot and developers have clear guidance on DTO architecture and prevents future misalignment issues.
