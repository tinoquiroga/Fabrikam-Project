namespace FabrikamTests.Helpers;

/// <summary>
/// Standardized test traits for consistent categorization and filtering
/// Supports Phase 3 test quality enhancement with systematic categorization
/// </summary>
public static class TestTraits
{
    /// <summary>
    /// Test category traits - primary classification
    /// </summary>
    public static class Category
    {
        public const string Unit = "Unit";
        public const string Integration = "Integration";
        public const string EndToEnd = "EndToEnd";
        public const string Performance = "Performance";
        public const string Security = "Security";
    }

    /// <summary>
    /// Component traits - what component is being tested
    /// </summary>
    public static class Component
    {
        // API Controllers
        public const string CustomersController = "CustomersController";
        public const string OrdersController = "OrdersController";
        public const string ProductsController = "ProductsController";
        public const string InfoController = "InfoController";
        public const string SupportTicketsController = "SupportTicketsController";
        public const string AuthController = "AuthController";

        // Models and DTOs
        public const string AuthenticationMode = "AuthenticationMode";
        public const string AuthenticationSettings = "AuthenticationSettings";
        public const string CustomerModels = "CustomerModels";
        public const string OrderModels = "OrderModels";
        public const string ProductModels = "ProductModels";

        // Services
        public const string ServiceJwtService = "ServiceJwtService";
        public const string DisabledAuthenticationService = "DisabledAuthenticationService";
        public const string UserRegistrationService = "UserRegistrationService";
        public const string AuthenticationSeedService = "AuthenticationSeedService";
        public const string JsonDataSeedService = "JsonDataSeedService";

        // Infrastructure
        public const string Database = "Database";
        public const string Configuration = "Configuration";
        public const string TestInfrastructure = "TestInfrastructure";
    }

    /// <summary>
    /// Feature traits - what business feature is being tested
    /// </summary>
    public static class Feature
    {
        public const string Authentication = "Authentication";
        public const string CustomerManagement = "CustomerManagement";
        public const string OrderManagement = "OrderManagement";
        public const string ProductCatalog = "ProductCatalog";
        public const string SupportTickets = "SupportTickets";
        public const string BusinessIntelligence = "BusinessIntelligence";
        public const string DataSeeding = "DataSeeding";
        public const string ApiSecurity = "ApiSecurity";
        public const string UserRegistration = "UserRegistration";
    }

    /// <summary>
    /// Priority traits - test execution priority
    /// </summary>
    public static class Priority
    {
        public const string Critical = "Critical";    // Must pass - blocks deployment
        public const string High = "High";           // Important - should pass
        public const string Medium = "Medium";       // Desirable - nice to pass
        public const string Low = "Low";             // Optional - can fail temporarily
    }

    /// <summary>
    /// Speed traits - test execution speed expectations
    /// </summary>
    public static class Speed
    {
        public const string Fast = "Fast";           // < 100ms
        public const string Medium = "Medium";       // 100ms - 1s
        public const string Slow = "Slow";          // 1s - 10s
        public const string VerySlow = "VerySlow";   // > 10s
    }

    /// <summary>
    /// Test type traits - testing approach
    /// </summary>
    public static class TestType
    {
        public const string Theory = "Theory";
        public const string Fact = "Fact";
        public const string Smoke = "Smoke";         // Basic functionality check
        public const string Regression = "Regression"; // Prevents known bugs
        public const string EdgeCase = "EdgeCase";   // Boundary conditions
        public const string ErrorHandling = "ErrorHandling"; // Exception scenarios
    }

    /// <summary>
    /// Environment traits - where test should run
    /// </summary>
    public static class Environment
    {
        public const string Development = "Development";
        public const string CI = "CI";              // Continuous Integration
        public const string Staging = "Staging";
        public const string Production = "Production"; // Production-safe tests only
    }

    /// <summary>
    /// Data traits - test data requirements
    /// </summary>
    public static class Data
    {
        public const string InMemory = "InMemory";   // Uses in-memory database
        public const string SeedData = "SeedData";  // Requires seeded test data
        public const string Isolated = "Isolated";  // No external dependencies
        public const string External = "External";  // Requires external services
    }
}

/// <summary>
/// Common trait combinations for easy application
/// </summary>
public static class CommonTraitCombinations
{
    /// <summary>
    /// Fast unit test for models/DTOs
    /// </summary>
    public static (string, string)[] FastUnit => new[]
    {
        ("Category", TestTraits.Category.Unit),
        ("Speed", TestTraits.Speed.Fast),
        ("Priority", TestTraits.Priority.High),
        ("Data", TestTraits.Data.Isolated)
    };

    /// <summary>
    /// Service layer unit test
    /// </summary>
    public static (string, string)[] ServiceUnit => new[]
    {
        ("Category", TestTraits.Category.Unit),
        ("Speed", TestTraits.Speed.Medium),
        ("Priority", TestTraits.Priority.High),
        ("Data", TestTraits.Data.InMemory)
    };

    /// <summary>
    /// API integration test
    /// </summary>
    public static (string, string)[] ApiIntegration => new[]
    {
        ("Category", TestTraits.Category.Integration),
        ("Speed", TestTraits.Speed.Slow),
        ("Priority", TestTraits.Priority.Critical),
        ("Data", TestTraits.Data.SeedData),
        ("Environment", TestTraits.Environment.CI)
    };

    /// <summary>
    /// Database integration test
    /// </summary>
    public static (string, string)[] DatabaseIntegration => new[]
    {
        ("Category", TestTraits.Category.Integration),
        ("Component", TestTraits.Component.Database),
        ("Speed", TestTraits.Speed.Medium),
        ("Priority", TestTraits.Priority.High),
        ("Data", TestTraits.Data.InMemory)
    };

    /// <summary>
    /// Critical smoke test
    /// </summary>
    public static (string, string)[] CriticalSmoke => new[]
    {
        ("Category", TestTraits.Category.Integration),
        ("TestType", TestTraits.TestType.Smoke),
        ("Priority", TestTraits.Priority.Critical),
        ("Speed", TestTraits.Speed.Fast),
        ("Environment", TestTraits.Environment.CI)
    };
}

/// <summary>
/// Extension methods for easy trait application
/// </summary>
public static class TraitExtensions
{
    /// <summary>
    /// Apply multiple traits to a test class or method
    /// </summary>
    public static void ApplyTraits(this object testInstance, params (string name, string value)[] traits)
    {
        // This is a helper for documentation - actual traits are applied via attributes
        // Usage: [Trait("Category", TestTraits.Category.Unit)]
    }
}
