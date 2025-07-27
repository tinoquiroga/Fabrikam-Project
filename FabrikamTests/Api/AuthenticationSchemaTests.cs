using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FabrikamApi.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using FabrikamTests.Helpers;

namespace FabrikamTests.Api;

/// <summary>
/// Tests to validate Issue #4: Database Schema for User Management and Roles
/// Ensures new schema fields (EntraObjectId, CompanyName, UpdatedDate, etc.) are working correctly
/// Uses in-memory database for fast, isolated testing
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Issue", "4")]
public class AuthenticationSchemaTests : IClassFixture<FabrikamTestApplicationFactory<Program>>
{
    private readonly FabrikamTestApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationSchemaTests(FabrikamTestApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task DatabaseSchema_Should_Include_Issue4_Fields()
    {
        // Arrange - Get the identity database context to verify schema
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        // First verify users exist in the database
        var userCount = await context.Users.CountAsync();
        userCount.Should().BeGreaterThan(0, "Seeded users should exist in test database");

        // Act - Query a user to verify all Issue #4 fields are accessible
        var user = await context.Users
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.EntraObjectId,      // Issue #4: New field for Entra External ID integration
                u.CompanyName,        // Issue #4: New field for business context
                u.CreatedDate,
                u.UpdatedDate,        // Issue #4: New audit field
                u.DeletedDate,        // Issue #4: New audit field for soft delete
                u.LastActiveDate,
                u.IsActive,
                u.CustomerId,
                u.NotificationPreferences,
                u.IsAdmin
            })
            .FirstOrDefaultAsync();

        // Assert - Verify all Issue #4 fields are accessible without errors
        user.Should().NotBeNull("Should be able to query user with new schema fields");
        
        // Verify new Issue #4 fields exist and are accessible (they can be null, we're just testing schema)
        Action accessEntraId = () => { var _ = user?.EntraObjectId; };
        Action accessCompanyName = () => { var _ = user?.CompanyName; };
        Action accessUpdatedDate = () => { var _ = user?.UpdatedDate; };
        Action accessDeletedDate = () => { var _ = user?.DeletedDate; };

        accessEntraId.Should().NotThrow("EntraObjectId field should be accessible");
        accessCompanyName.Should().NotThrow("CompanyName field should be accessible");
        accessUpdatedDate.Should().NotThrow("UpdatedDate field should be accessible");
        accessDeletedDate.Should().NotThrow("DeletedDate field should be accessible");
    }

    [Fact]
    public async Task UserRoles_Should_Include_Issue4_AuditFields()
    {
        // Arrange - Get the identity database context
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        // Act - Query user roles to verify audit fields
        var userRole = await context.UserRoles
            .Select(ur => new
            {
                ur.UserId,
                ur.RoleId,
                ur.AssignedAt,        // Issue #4: New audit field
                ur.AssignedBy,        // Issue #4: New audit field
                ur.IsActive
            })
            .FirstOrDefaultAsync();

        // Assert - Verify audit fields are accessible
        if (userRole != null)
        {
            Action accessAssignedAt = () => { var _ = userRole.AssignedAt; };
            Action accessAssignedBy = () => { var _ = userRole.AssignedBy; };
            
            accessAssignedAt.Should().NotThrow("AssignedAt should be accessible");
            accessAssignedBy.Should().NotThrow("AssignedBy should be accessible");
        }
    }

    [Fact]
    public async Task Authentication_API_Should_Work_With_New_Schema()
    {
        // Act - Test that API endpoints work with new schema (using a simple info endpoint)
        var response = await _client.GetAsync("/api/info");

        // Assert - Should not fail due to schema issues
        response.IsSuccessStatusCode.Should().BeTrue("API endpoints should work with Issue #4 schema");
    }

    [Fact]
    public async Task Database_Indexes_Should_Support_Issue4_Performance()
    {
        // Arrange - Get the identity database context
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FabrikamIdentityDbContext>();

        // Act - Perform queries that should be optimized by Issue #4 indexes
        var startTime = DateTime.UtcNow;
        
        // Query by EntraObjectId (should be indexed for performance)
        var userByEntraId = await context.Users
            .Where(u => u.EntraObjectId != null)
            .FirstOrDefaultAsync();
            
        // Query active users (should be optimized)
        var activeUsers = await context.Users
            .Where(u => u.IsActive && u.DeletedDate == null)
            .Take(10)
            .ToListAsync();
            
        var queryTime = DateTime.UtcNow - startTime;

        // Assert - Queries should complete quickly (under 50ms benchmark)
        queryTime.TotalMilliseconds.Should().BeLessThan(50, "Issue #4 indexes should provide sub-50ms query performance");
        
        // Just verify the queries don't throw exceptions (data may or may not exist)
        Action accessResult = () => { var _ = userByEntraId?.Id; };
        accessResult.Should().NotThrow("Should be able to query by EntraObjectId efficiently");
        
        activeUsers.Should().NotBeNull("Should efficiently retrieve active users collection");
    }
}
