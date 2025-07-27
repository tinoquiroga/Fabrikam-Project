# 🏭 Business Simulator - Detailed Requirements & Metrics

> **GitHub Issue**: [#9 - Business Simulator: Real-time Order & Event Generation](https://github.com/davebirr/Fabrikam-Project/issues/9)

## 📊 **Current Business Metrics Foundation**

Based on our seed data analysis and realistic business model for a $500M modular home manufacturer:

### **Revenue & Order Patterns**

```
Current Sample Data (41 orders):
- Total Revenue: $8,373,000
- Average Order Value: $204,219
- Order Distribution:
  - Delivered: 35 orders ($7.1M) - 85% completion rate
  - In Production: 3 orders ($528K) - Active manufacturing
  - Pending: 3 orders ($742K) - Order pipeline

Target Annual Scale:
- Annual Revenue: $500,000,000
- Estimated Annual Orders: ~2,450 orders
- Daily Order Rate: 6-7 orders per business day
- Monthly Volume: ~204 orders
```

### **Product Portfolio Performance**

```json
{
  "products": {
    "singleFamily": {
      "cozyCotagge1200": {
        "price": 115000,
        "marketShare": "25%",
        "category": "Entry"
      },
      "familyHaven1800": {
        "price": 185000,
        "marketShare": "25%",
        "category": "Mid"
      },
      "executiveManor2500": {
        "price": 295000,
        "marketShare": "15%",
        "category": "Luxury"
      }
    },
    "multiFamily": {
      "twinVistaDuplex": {
        "price": 225000,
        "marketShare": "20%",
        "category": "Investment"
      }
    },
    "commercial": {
      "retailFlex1500": {
        "price": 165000,
        "marketShare": "10%",
        "category": "Business"
      }
    },
    "accessory": {
      "backyardStudio400": {
        "price": 58000,
        "marketShare": "15%",
        "category": "ADU"
      }
    },
    "components": {
      "luxuryBathroom": {
        "price": 35000,
        "margin": "60%",
        "attachRate": "25%"
      },
      "solarPower": { "price": 32000, "margin": "55%", "attachRate": "40%" },
      "energyEfficiency": {
        "price": 28000,
        "margin": "50%",
        "attachRate": "35%"
      },
      "premiumFlooring": {
        "price": 22500,
        "margin": "45%",
        "attachRate": "30%"
      },
      "premiumKitchen": {
        "price": 18500,
        "margin": "45%",
        "attachRate": "45%"
      },
      "smartHome": { "price": 12500, "margin": "60%", "attachRate": "35%" }
    }
  }
}
```

### **Customer Distribution Patterns**

```json
{
  "regions": {
    "west": {
      "share": "25%",
      "avgOrderValue": 220000,
      "preferences": ["luxury", "solar"]
    },
    "southeast": {
      "share": "20%",
      "avgOrderValue": 190000,
      "preferences": ["adu", "efficiency"]
    },
    "midwest": {
      "share": "25%",
      "avgOrderValue": 195000,
      "preferences": ["balanced"]
    },
    "northeast": {
      "share": "15%",
      "avgOrderValue": 210000,
      "preferences": ["commercial"]
    },
    "southwest": {
      "share": "15%",
      "avgOrderValue": 185000,
      "preferences": ["adu", "solar"]
    }
  },
  "customerTypes": {
    "newCustomer": { "percentage": 70, "conversionRate": "85%" },
    "repeatCustomer": { "percentage": 20, "avgOrderIncrease": "15%" },
    "referralCustomer": { "percentage": 10, "avgOrderIncrease": "8%" }
  }
}
```

## 🎯 **Simulation Parameters**

### **Order Generation Logic**

```csharp
// Realistic business patterns for simulator
public class BusinessSimulationConfig
{
    // Daily order targets (6-7 per day = 2,450 annually)
    public int MinDailyOrders = 5;
    public int MaxDailyOrders = 9;
    public int TargetAnnualOrders = 2450;

    // Order value distribution
    public decimal MinOrderValue = 58000;    // Backyard Studio
    public decimal MaxOrderValue = 359000;   // Executive Manor + Components
    public decimal TargetAvgOrderValue = 204219;

    // Product mix probabilities
    public ProductMixConfig ProductMix = new()
    {
        SingleFamily = 0.65m,  // 65% of orders
        Duplex = 0.20m,        // 20% of orders
        Commercial = 0.10m,    // 10% of orders
        AccessoryUnit = 0.15m  // 15% of orders (overlaps with others)
    };

    // Component attachment rates
    public ComponentAttachmentRates ComponentRates = new()
    {
        PremiumKitchen = 0.45m,      // 45% attach rate
        SolarPower = 0.40m,          // 40% attach rate
        EnergyEfficiency = 0.35m,    // 35% attach rate
        SmartHome = 0.35m,           // 35% attach rate
        PremiumFlooring = 0.30m,     // 30% attach rate
        LuxuryBathroom = 0.25m       // 25% attach rate
    };
}
```

### **Seasonal & Business Patterns**

```csharp
public class SeasonalPatterns
{
    // Construction industry seasonality
    public Dictionary<int, decimal> MonthlyMultipliers = new()
    {
        { 1, 0.7m },   // January - Slow winter
        { 2, 0.8m },   // February - Planning season
        { 3, 1.2m },   // March - Spring startup
        { 4, 1.3m },   // April - Peak begins
        { 5, 1.4m },   // May - Peak season
        { 6, 1.3m },   // June - High activity
        { 7, 1.2m },   // July - Summer busy
        { 8, 1.1m },   // August - Continued activity
        { 9, 1.0m },   // September - Normal
        { 10, 0.9m },  // October - Slowdown begins
        { 11, 0.7m },  // November - Winter prep
        { 12, 0.6m }   // December - Holiday slow
    };

    // Business hours patterns
    public TimePattern BusinessHours = new()
    {
        StartHour = 8,     // 8 AM
        EndHour = 17,      // 5 PM
        TimeZone = "PST",  // Pacific (HQ timezone)
        WeekendMultiplier = 0.1m,  // 10% weekend activity
        HolidayMultiplier = 0.05m  // 5% holiday activity
    };
}
```

### **Regional Distribution**

```csharp
public class RegionalConfig
{
    public Dictionary<string, RegionProfile> Regions = new()
    {
        ["West"] = new()
        {
            States = ["CA", "WA", "OR", "NV"],
            MarketShare = 0.25m,
            AvgOrderValue = 220000,
            PreferredProducts = ["ExecutiveManor", "Solar", "SmartHome"],
            GrowthRate = 1.15m  // 15% above average
        },
        ["Southeast"] = new()
        {
            States = ["FL", "GA", "NC", "SC", "TN"],
            MarketShare = 0.20m,
            AvgOrderValue = 190000,
            PreferredProducts = ["BackyardStudio", "EnergyEfficiency"],
            GrowthRate = 1.20m  // 20% above average (ADU boom)
        },
        ["Midwest"] = new()
        {
            States = ["IL", "OH", "MI", "IN", "WI"],
            MarketShare = 0.25m,
            AvgOrderValue = 195000,
            PreferredProducts = ["FamilyHaven", "PremiumKitchen"],
            GrowthRate = 1.05m  // 5% above average
        },
        ["Northeast"] = new()
        {
            States = ["NY", "MA", "PA", "NJ", "CT"],
            MarketShare = 0.15m,
            AvgOrderValue = 210000,
            PreferredProducts = ["RetailFlex", "LuxuryBathroom"],
            GrowthRate = 1.08m  // 8% above average
        },
        ["Southwest"] = new()
        {
            States = ["TX", "AZ", "CO", "NM"],
            MarketShare = 0.15m,
            AvgOrderValue = 185000,
            PreferredProducts = ["BackyardStudio", "Solar"],
            GrowthRate = 1.25m  // 25% above average (fastest growing)
        }
    };
}
```

## 🚀 **Implementation Architecture**

### **Simulator Service Structure**

```csharp
// Proposed service architecture
public interface IBusinessSimulator
{
    Task StartSimulationAsync(SimulationConfig config);
    Task StopSimulationAsync();
    Task<SimulationMetrics> GetCurrentMetricsAsync();
    Task<BusinessScenario> ApplyScenarioAsync(string scenarioName);
}

public class BusinessSimulatorService : BackgroundService, IBusinessSimulator
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ISupportTicketService _supportService;
    private readonly ILogger<BusinessSimulatorService> _logger;

    // Core simulation engines
    private readonly OrderGenerationEngine _orderEngine;
    private readonly CustomerLifecycleEngine _customerEngine;
    private readonly SupportTicketEngine _supportEngine;
    private readonly SeasonalPatternEngine _seasonalEngine;
}
```

### **Order Generation Engine**

```csharp
public class OrderGenerationEngine
{
    public async Task<Order> GenerateRealisticOrderAsync()
    {
        // 1. Select customer (70% new, 20% repeat, 10% referral)
        var customer = await SelectOrCreateCustomerAsync();

        // 2. Determine region and preferences
        var region = GetCustomerRegion(customer);
        var preferences = GetRegionalPreferences(region);

        // 3. Select primary product based on regional preferences
        var primaryProduct = SelectProductByPreferences(preferences);

        // 4. Calculate component attachments
        var components = GenerateComponentAttachments(primaryProduct, preferences);

        // 5. Apply seasonal pricing adjustments
        var pricing = ApplySeasonalPricing(primaryProduct, components);

        // 6. Create realistic order
        return new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = BuildOrderItems(primaryProduct, components, pricing),
            ShippingAddress = customer.Address,
            // ... other properties
        };
    }
}
```

### **Support Ticket Generation**

```csharp
public class SupportTicketEngine
{
    // Generate 1-2 tickets per 10 orders (10-20% rate)
    public async Task<SupportTicket> GenerateTicketAsync(Order relatedOrder)
    {
        var scenarios = new[]
        {
            new TicketScenario
            {
                Type = "Delivery Inquiry",
                Probability = 0.4m,
                Priority = Priority.Low,
                EstimatedResolutionDays = 1
            },
            new TicketScenario
            {
                Type = "Customization Request",
                Probability = 0.3m,
                Priority = Priority.Medium,
                EstimatedResolutionDays = 3
            },
            new TicketScenario
            {
                Type = "Technical Issue",
                Probability = 0.2m,
                Priority = Priority.High,
                EstimatedResolutionDays = 2
            },
            new TicketScenario
            {
                Type = "Installation Question",
                Probability = 0.1m,
                Priority = Priority.Medium,
                EstimatedResolutionDays = 2
            }
        };

        var scenario = SelectScenarioByProbability(scenarios);
        return GenerateTicketFromScenario(scenario, relatedOrder);
    }
}
```

## 📈 **Business Scenarios**

### **Predefined Simulation Scenarios**

```json
{
  "scenarios": {
    "normalOperations": {
      "name": "Normal Business Operations",
      "orderVelocityMultiplier": 1.0,
      "seasonalPatternsEnabled": true,
      "supportTicketRate": 0.15,
      "description": "Steady-state business with seasonal patterns"
    },
    "growthPhase": {
      "name": "Rapid Growth Phase",
      "orderVelocityMultiplier": 1.3,
      "newCustomerRate": 0.8,
      "marketExpansion": ["Nevada", "Utah", "Montana"],
      "description": "30% growth with new market expansion"
    },
    "seasonalPeak": {
      "name": "Spring/Summer Construction Boom",
      "orderVelocityMultiplier": 1.5,
      "componentAttachmentBonus": 0.1,
      "premiumProductPreference": 0.2,
      "description": "Peak construction season with higher-value orders"
    },
    "economicDownturn": {
      "name": "Economic Challenges",
      "orderVelocityMultiplier": 0.7,
      "budgetProductPreference": 0.3,
      "supportTicketRate": 0.25,
      "description": "Reduced volume, budget-conscious customers"
    },
    "productLaunch": {
      "name": "New Product Launch",
      "featuredProduct": "EcoSmart2000",
      "launchDiscount": 0.1,
      "marketingBoost": 0.2,
      "description": "New eco-friendly home model introduction"
    }
  }
}
```

## ⚙️ **Configuration & Controls**

### **Simulation Speed Controls**

```csharp
public enum SimulationSpeed
{
    RealTime = 1,        // 1x - Normal business pace
    Accelerated = 10,    // 10x - 10 times faster
    SuperFast = 100,     // 100x - Rapid testing
    TestMode = 1000      // 1000x - Instant validation
}
```

### **Data Persistence Options**

```csharp
public enum PersistenceMode
{
    InMemory,       // Testing/development
    Database,       // Full persistence
    Hybrid,         // Key data persisted, bulk in-memory
    ReadOnly        // No writes, metrics only
}
```

## 🎯 **Success Metrics & KPIs**

### **Simulator Performance**

- **Accuracy**: Generated patterns match business model (±5%)
- **Performance**: <50ms average order generation time
- **Reliability**: 99.9% uptime during extended runs
- **Resource Usage**: <100MB memory footprint

### **Business Realism**

- **Revenue Distribution**: Monthly targets within ±10%
- **Regional Balance**: Geographic distribution accuracy
- **Product Mix**: Maintain target percentages ±15%
- **Customer Behavior**: Realistic repeat/referral patterns

### **Demo & Testing Value**

- **Data Freshness**: Always current, relevant data
- **Scalability**: Support 10x to 100x acceleration
- **Scenario Flexibility**: Quick switching between business conditions
- **Analytics**: Real-time business intelligence

---

## 🛠️ **Implementation Checklist**

When ready to implement Issue #9:

### **Phase 1: Foundation**

- [ ] Create `BusinessSimulatorService` base class
- [ ] Implement basic order generation logic
- [ ] Add configuration system
- [ ] Create simple console logging

### **Phase 2: Business Logic**

- [ ] Add seasonal patterns
- [ ] Implement regional distribution
- [ ] Create customer lifecycle management
- [ ] Add support ticket generation

### **Phase 3: Scenarios**

- [ ] Build scenario engine
- [ ] Create predefined business scenarios
- [ ] Add real-time metrics
- [ ] Implement performance monitoring

### **Phase 4: Advanced Features**

- [ ] Cloud deployment options
- [ ] Advanced analytics dashboard
- [ ] Machine learning patterns
- [ ] Enterprise scaling

---

**This document provides the complete foundation for implementing a realistic business simulator that will bring Fabrikam to life with dynamic, accurate business operations.**
