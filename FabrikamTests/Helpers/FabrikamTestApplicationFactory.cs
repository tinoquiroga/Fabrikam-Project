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
            
            // Add testing configuration
            config.AddJsonFile("appsettings.Testing.json", optional: true)
                  .AddInMemoryCollection(new Dictionary<string, string?>
                  {
                      {"Database:Provider", "InMemory"},
                      {"SeedData:EnableSeedOnStartup", "true"},
                      {"SeedData:Method", "Json"},
                      {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"}
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
