using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FabrikamApi.Data;

namespace FabrikamTests.Helpers;

/// <summary>
/// Custom WebApplicationFactory for testing that ensures consistent test environment
/// Forces in-memory database and testing configuration regardless of environment settings
/// </summary>
/// <typeparam name="TStartup"></typeparam>
public class FabrikamTestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set testing environment
            context.HostingEnvironment.EnvironmentName = "Testing";
            
            // Add testing configuration that forces Disabled authentication mode to avoid conflicts
            config.AddJsonFile("appsettings.Testing.json", optional: true)
                  .AddInMemoryCollection(new Dictionary<string, string?>
                  {
                      {"Database:Provider", "InMemory"},
                      {"SeedData:EnableSeedOnStartup", "true"},
                      {"SeedData:Method", "Json"},
                      {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                      {"Authentication:Mode", "Disabled"}, // Force disabled authentication for integration tests
                      {"Authentication:AspNetIdentity:Password:RequireDigit", "false"},
                      {"Authentication:AspNetIdentity:Password:RequiredLength", "6"},
                      {"Authentication:AspNetIdentity:Password:RequireNonAlphanumeric", "false"},
                      {"Authentication:AspNetIdentity:Password:RequireUppercase", "false"},
                      {"Authentication:AspNetIdentity:Password:RequireLowercase", "false"}
                  });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FabrikamIdentityDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique database per test class
            // but shared within test class to avoid seeding conflicts
            var databaseName = $"TestDatabase_{GetType().Name}_{Environment.CurrentManagedThreadId}";
            services.AddDbContext<FabrikamIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging(); // Useful for debugging tests
            });
        });
    }
}

/// <summary>
/// Custom WebApplicationFactory for testing EntraExternalId authentication
/// Forces EntraExternalId authentication mode with mock configuration
/// </summary>
/// <typeparam name="TStartup"></typeparam>
public class EntraExternalIdTestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set testing environment
            context.HostingEnvironment.EnvironmentName = "Testing";
            
            // Add testing configuration that forces EntraExternalId authentication mode
            config.AddJsonFile("appsettings.Testing.json", optional: true)
                  .AddInMemoryCollection(new Dictionary<string, string?>
                  {
                      {"Database:Provider", "InMemory"},
                      {"SeedData:EnableSeedOnStartup", "true"},
                      {"SeedData:Method", "Json"},
                      {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                      {"Authentication:Mode", "EntraExternalId"}, // Force EntraExternalId authentication for these tests
                      {"Authentication:EntraExternalId:Authority", "https://test.microsoft.com/v2.0"},
                      {"Authentication:EntraExternalId:Audience", "test-audience"},
                      {"Authentication:EntraExternalId:ClientId", "test-client-id"},
                      {"Authentication:EntraExternalId:TenantId", "test-tenant-id"},
                      {"Authentication:AspNetIdentity:Password:RequireDigit", "false"},
                      {"Authentication:AspNetIdentity:Password:RequiredLength", "6"},
                      {"Authentication:AspNetIdentity:Password:RequireNonAlphanumeric", "false"},
                      {"Authentication:AspNetIdentity:Password:RequireUppercase", "false"},
                      {"Authentication:AspNetIdentity:Password:RequireLowercase", "false"}
                  });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FabrikamIdentityDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique database per test class
            var databaseName = $"TestDatabase_{GetType().Name}_{Environment.CurrentManagedThreadId}";
            services.AddDbContext<FabrikamIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging(); // Useful for debugging tests
            });
        });
    }
}
