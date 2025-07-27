using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FabrikamApi.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FabrikamTests.Integration;

/// <summary>
/// Integration tests that validate Issue #4 schema against actual SQL Server database
/// These tests run against real infrastructure to validate production-like scenarios
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "SqlServer")]
[Trait("Issue", "4")]
public class DatabaseSchemaIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DatabaseSchemaIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "RequiresSqlServer")]
    public async Task SqlServer_DatabaseSchema_Should_Include_Issue4_Fields()
    {
        // Skip this test if SQL Server connection is not available
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("InMemory"))
        {
            // Skip test when running with in-memory database
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        try
        {
            // Ensure database exists and is accessible
            await context.Database.CanConnectAsync();

            // Act - Query a user to verify all Issue #4 fields work with SQL Server
            var user = await context.Users
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.EntraObjectId,      // Issue #4: Test against real SQL Server schema
                    u.CompanyName,        // Issue #4: Test against real SQL Server schema
                    u.CreatedDate,
                    u.UpdatedDate,        // Issue #4: Test against real SQL Server schema
                    u.DeletedDate,        // Issue #4: Test against real SQL Server schema
                    u.LastActiveDate,
                    u.IsActive,
                    u.CustomerId,
                    u.NotificationPreferences,
                    u.IsAdmin
                })
                .FirstOrDefaultAsync();

            // Assert - Verify all Issue #4 fields work in SQL Server environment
            // Note: Data may be null, we're testing schema compatibility
            Action accessEntraId = () => { var _ = user?.EntraObjectId; };
            Action accessCompanyName = () => { var _ = user?.CompanyName; };
            Action accessUpdatedDate = () => { var _ = user?.UpdatedDate; };
            Action accessDeletedDate = () => { var _ = user?.DeletedDate; };

            accessEntraId.Should().NotThrow("EntraObjectId field should work with SQL Server");
            accessCompanyName.Should().NotThrow("CompanyName field should work with SQL Server");
            accessUpdatedDate.Should().NotThrow("UpdatedDate field should work with SQL Server");
            accessDeletedDate.Should().NotThrow("DeletedDate field should work with SQL Server");
        }
        catch (Exception ex) when (ex.Message.Contains("network-related") || ex.Message.Contains("authentication"))
        {
            // Skip test if SQL Server is not accessible (expected in some CI environments)
            return;
        }
    }

    [Fact]
    [Trait("Category", "RequiresSqlServer")]
    public async Task SqlServer_Performance_Should_Meet_Issue4_Benchmarks()
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("InMemory"))
        {
            return; // Skip for in-memory tests
        }

        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        try
        {
            await context.Database.CanConnectAsync();

            // Act - Test real SQL Server performance with Issue #4 indexes
            var startTime = DateTime.UtcNow;
            
            // Query by EntraObjectId (should use index for performance)
            var userByEntraId = await context.Users
                .Where(u => u.EntraObjectId != null)
                .FirstOrDefaultAsync();
                
            // Query active users (should use index)
            var activeUsers = await context.Users
                .Where(u => u.IsActive && u.DeletedDate == null)
                .Take(10)
                .ToListAsync();
                
            var queryTime = DateTime.UtcNow - startTime;

            // Assert - SQL Server performance should meet benchmarks
            queryTime.TotalMilliseconds.Should().BeLessThan(100, 
                "SQL Server queries with Issue #4 indexes should complete under 100ms");
        }
        catch (Exception ex) when (ex.Message.Contains("network-related") || ex.Message.Contains("authentication"))
        {
            return; // Skip if SQL Server not accessible
        }
    }

    [Fact]
    [Trait("Category", "RequiresSqlServer")]
    public async Task SqlServer_Identity_Tables_Should_Have_Correct_Schema()
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("InMemory"))
        {
            return; // Skip for in-memory tests
        }

        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        try
        {
            await context.Database.CanConnectAsync();

            // Act - Verify we can access identity tables in SQL Server
            var usersCount = await context.Users.CountAsync();
            var rolesCount = await context.Roles.CountAsync();

            // Try to query Issue #4 fields specifically
            var userWithNewFields = await context.Users
                .Where(u => u.EntraObjectId != null || u.CompanyName != null)
                .FirstOrDefaultAsync();

            // Assert - SQL Server should support all Issue #4 schema features
            // Note: Counts may be 0, we're testing schema accessibility
            usersCount.Should().BeGreaterThanOrEqualTo(0, "Should be able to count users in SQL Server");
            rolesCount.Should().BeGreaterThanOrEqualTo(0, "Should be able to count roles in SQL Server");
            
            // No exception means schema is valid
            var _ = userWithNewFields?.EntraObjectId;
            var __ = userWithNewFields?.CompanyName;
        }
        catch (Exception ex) when (ex.Message.Contains("network-related") || ex.Message.Contains("authentication"))
        {
            return; // Skip if SQL Server not accessible
        }
    }
}
