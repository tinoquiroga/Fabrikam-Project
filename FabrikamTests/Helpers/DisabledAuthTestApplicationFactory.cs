using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FabrikamApi;
using FabrikamApi.Data;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Helpers;

/// <summary>
/// Test application factory specifically for Phase 3 tests with authentication disabled
/// Forces authentication mode to Disabled to test environment-aware authorization policies
/// </summary>
public class DisabledAuthTestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set testing environment
            context.HostingEnvironment.EnvironmentName = "Testing";
            
            var configValues = new Dictionary<string, string?>
            {
                {"Database:Provider", "InMemory"},
                {"SeedData:EnableSeedOnStartup", "true"},
                {"SeedData:Method", "Json"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                
                // Explicitly force authentication to Disabled mode for Phase 3 tests
                {"Authentication:Mode", "Disabled"},
                {"Authentication:RequireUserAuthentication", "false"},
                {"Authentication:RequireServiceAuthentication", "true"},
                
                // JWT configuration (not used in disabled mode but kept for consistency)
                {"Authentication:Jwt:Issuer", "FabrikamTestIssuer"},
                {"Authentication:Jwt:Audience", "FabrikamTestAudience"},
                {"Authentication:Jwt:SecretKey", "TestSecretKeyForFabrikamAuthenticationTestsOnly_MustBeAtLeast32Characters!"}
            };

            config.AddInMemoryCollection(configValues);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration for replacement with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FabrikamIdentityDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<FabrikamIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase("FabrikamTestDb_DisabledAuth");
                options.EnableSensitiveDataLogging();
            });
        });
    }
}
