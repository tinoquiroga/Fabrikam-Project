using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs; // Use consolidated authentication models

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Unit tests for AuthenticationContext class
/// Tests context creation, role checking, and display functionality
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Component", "AuthenticationContext")]
public class AuthenticationContextTests
{
    [Fact]
    public void AuthenticationContext_Creation_SetsAllProperties()
    {
        // Arrange
        var userId = "user123";
        var userName = "John Doe";
        var roles = new List<string> { "Admin", "User", "Manager" };

        // Act
        var context = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = userId,
            UserName = userName,
            Roles = roles
        };

        // Assert
        context.IsAuthenticated.Should().BeTrue();
        context.UserId.Should().Be(userId);
        context.UserName.Should().Be(userName);
        context.Roles.Should().BeEquivalentTo(roles);
        context.Roles.Should().HaveCount(3);
    }

    [Fact]
    public void AuthenticationContext_DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var context = new AuthenticationContext();

        // Assert
        context.IsAuthenticated.Should().BeFalse("Default IsAuthenticated should be false");
        context.UserId.Should().BeNull("Default UserId should be null");
        context.UserName.Should().BeNull("Default UserName should be null");
        context.Roles.Should().NotBeNull("Roles should never be null");
        context.Roles.Should().BeEmpty("Default Roles should be empty");
    }

    [Fact]
    public void AuthenticationContext_WithRoles_HandlesRoleCollection()
    {
        // Arrange
        var roles = new List<string> { "Admin", "User", "PowerUser", "Manager" };
        var context = new AuthenticationContext { Roles = roles };

        // Act & Assert
        context.Roles.Should().HaveCount(4);
        context.Roles.Should().Contain("Admin");
        context.Roles.Should().Contain("User");
        context.Roles.Should().Contain("PowerUser");
        context.Roles.Should().Contain("Manager");
    }

    [Fact]
    public void AuthenticationContext_HasRole_WorksCorrectly()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string> { "Admin", "User", "Manager" }
        };

        // Act & Assert - Test existing roles
        context.HasRole("Admin").Should().BeTrue("Should find existing role 'Admin'");
        context.HasRole("User").Should().BeTrue("Should find existing role 'User'");
        context.HasRole("Manager").Should().BeTrue("Should find existing role 'Manager'");

        // Test non-existing roles
        context.HasRole("SuperAdmin").Should().BeFalse("Should not find non-existing role 'SuperAdmin'");
        context.HasRole("Guest").Should().BeFalse("Should not find non-existing role 'Guest'");
        context.HasRole("").Should().BeFalse("Should not find empty role");
    }

    [Fact]
    public void AuthenticationContext_HasRole_IsCaseInsensitive()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string> { "Admin", "User", "PowerUser" }
        };

        // Act & Assert - Test case insensitive matching
        context.HasRole("admin").Should().BeTrue("Should match 'admin' case-insensitively");
        context.HasRole("ADMIN").Should().BeTrue("Should match 'ADMIN' case-insensitively");
        context.HasRole("AdMiN").Should().BeTrue("Should match 'AdMiN' case-insensitively");
        context.HasRole("user").Should().BeTrue("Should match 'user' case-insensitively");
        context.HasRole("USER").Should().BeTrue("Should match 'USER' case-insensitively");
        context.HasRole("poweruser").Should().BeTrue("Should match 'poweruser' case-insensitively");
        context.HasRole("POWERUSER").Should().BeTrue("Should match 'POWERUSER' case-insensitively");
    }

    [Fact]
    public void AuthenticationContext_HasAnyRole_WorksCorrectly()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string> { "User", "Manager" }
        };

        // Act & Assert - Test with multiple roles where some match
        context.HasAnyRole("Admin", "User").Should().BeTrue("Should match when one role exists");
        context.HasAnyRole("User", "SuperAdmin").Should().BeTrue("Should match when one role exists");
        context.HasAnyRole("Manager", "Admin", "PowerUser").Should().BeTrue("Should match when one role exists");

        // Test with no matching roles
        context.HasAnyRole("Admin", "SuperAdmin").Should().BeFalse("Should not match when no roles exist");
        context.HasAnyRole("Guest", "PowerUser").Should().BeFalse("Should not match when no roles exist");

        // Test with empty array
        context.HasAnyRole().Should().BeFalse("Should not match with empty role array");

        // Test with all matching roles
        context.HasAnyRole("User", "Manager").Should().BeTrue("Should match when all roles exist");
    }

    [Fact]
    public void AuthenticationContext_HasAnyRole_HandlesNullAndEmpty()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string> { "User", "Admin" }
        };

        // Act & Assert
        context.HasAnyRole(null!).Should().BeFalse("Should handle null role gracefully");
        context.HasAnyRole("").Should().BeFalse("Should handle empty role gracefully");
        context.HasAnyRole("User", null!, "Admin").Should().BeTrue("Should match existing roles despite null");
        context.HasAnyRole("", "User").Should().BeTrue("Should match existing roles despite empty string");
    }

    [Fact]
    public void AuthenticationContext_GetDisplayName_ReturnsCorrectValues()
    {
        // Test authenticated user with UserName
        var context1 = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = "user123",
            UserName = "John Doe"
        };
        context1.GetDisplayName().Should().Be("John Doe", "Should prefer UserName when available");

        // Test authenticated user with UserId but no UserName
        var context2 = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = "user123",
            UserName = null
        };
        context2.GetDisplayName().Should().Be("user123", "Should use UserId when UserName is null");

        // Test authenticated user with neither UserId nor UserName
        var context3 = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = null,
            UserName = null
        };
        context3.GetDisplayName().Should().Be("Unknown User", "Should return 'Unknown User' when no identifiers");

        // Test unauthenticated user
        var context4 = new AuthenticationContext
        {
            IsAuthenticated = false,
            UserId = "user123",
            UserName = "John Doe"
        };
        context4.GetDisplayName().Should().Be("Anonymous", "Should return 'Anonymous' for unauthenticated users");
    }

    [Fact]
    public void AuthenticationContext_IsAuthenticated_ReflectsState()
    {
        // Test authenticated state
        var authenticatedContext = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = "user123"
        };
        authenticatedContext.IsAuthenticated.Should().BeTrue();

        // Test unauthenticated state
        var unauthenticatedContext = new AuthenticationContext
        {
            IsAuthenticated = false
        };
        unauthenticatedContext.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void AuthenticationContext_RoleManagement_SupportsModification()
    {
        // Arrange
        var context = new AuthenticationContext();

        // Act - Add roles dynamically
        context.Roles.Add("User");
        context.Roles.Add("Admin");

        // Assert
        context.Roles.Should().HaveCount(2);
        context.HasRole("User").Should().BeTrue();
        context.HasRole("Admin").Should().BeTrue();

        // Act - Remove role
        context.Roles.Remove("User");

        // Assert
        context.Roles.Should().HaveCount(1);
        context.HasRole("User").Should().BeFalse();
        context.HasRole("Admin").Should().BeTrue();
    }

    [Fact]
    public void AuthenticationContext_WithEmptyRoles_HandlesGracefully()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string>()
        };

        // Act & Assert
        context.HasRole("AnyRole").Should().BeFalse("Should handle empty roles list");
        context.HasAnyRole("Role1", "Role2").Should().BeFalse("Should handle empty roles list");
        context.Roles.Should().NotBeNull("Roles should never be null");
        context.Roles.Should().BeEmpty("Roles should be empty");
    }

    [Fact]
    public void AuthenticationContext_WithDuplicateRoles_HandlesCorrectly()
    {
        // Arrange
        var context = new AuthenticationContext
        {
            Roles = new List<string> { "Admin", "User", "Admin", "Manager", "User" }
        };

        // Act & Assert
        context.Roles.Should().HaveCount(5, "Should preserve duplicate roles if added");
        context.HasRole("Admin").Should().BeTrue("Should find role regardless of duplicates");
        context.HasRole("User").Should().BeTrue("Should find role regardless of duplicates");
        context.HasRole("Manager").Should().BeTrue("Should find role regardless of duplicates");
    }

    [Fact]
    public void AuthenticationContext_SerializationSupport_WorksCorrectly()
    {
        // Arrange - Create a context with various properties set
        var originalContext = new AuthenticationContext
        {
            IsAuthenticated = true,
            UserId = "user123",
            UserName = "John Doe",
            Roles = new List<string> { "Admin", "User", "Manager" }
        };

        // Act - Simulate serialization/deserialization by copying properties
        var serializedContext = new AuthenticationContext
        {
            IsAuthenticated = originalContext.IsAuthenticated,
            UserId = originalContext.UserId,
            UserName = originalContext.UserName,
            Roles = new List<string>(originalContext.Roles)
        };

        // Assert
        serializedContext.IsAuthenticated.Should().Be(originalContext.IsAuthenticated);
        serializedContext.UserId.Should().Be(originalContext.UserId);
        serializedContext.UserName.Should().Be(originalContext.UserName);
        serializedContext.Roles.Should().BeEquivalentTo(originalContext.Roles);
        serializedContext.GetDisplayName().Should().Be(originalContext.GetDisplayName());
        serializedContext.HasRole("Admin").Should().Be(originalContext.HasRole("Admin"));
    }

    [Fact]
    public void AuthenticationContext_EdgeCases_HandleCorrectly()
    {
        // Test with very long role names
        var context = new AuthenticationContext();
        var longRoleName = new string('A', 1000);
        context.Roles.Add(longRoleName);
        context.HasRole(longRoleName).Should().BeTrue("Should handle very long role names");

        // Test with special characters in roles
        var specialRoles = new[] { "Role@Company", "Role#123", "Role$Admin", "Role&Manager" };
        foreach (var role in specialRoles)
        {
            context.Roles.Add(role);
            context.HasRole(role).Should().BeTrue($"Should handle role with special characters: {role}");
        }

        // Test with Unicode characters
        var unicodeRole = "Rôle_Ādmin_测试";
        context.Roles.Add(unicodeRole);
        context.HasRole(unicodeRole).Should().BeTrue("Should handle Unicode characters in role names");
    }
}
