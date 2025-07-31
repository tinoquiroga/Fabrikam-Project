using System;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs; // Use consolidated authentication models

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Unit tests for AuthenticationSettings class and related configuration models
/// Tests configuration properties, defaults, and validation logic
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Component", "AuthenticationSettings")]
public class AuthenticationSettingsTests
{
    [Fact]
    public void AuthenticationSettings_DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var settings = new AuthenticationSettings();

        // Assert
        settings.Mode.Should().Be(AuthenticationMode.BearerToken, "Default mode should be BearerToken for security");
        settings.Jwt.Should().NotBeNull("JWT settings should be initialized");
        settings.ServiceJwt.Should().NotBeNull("Service JWT settings should be initialized");
        settings.GuidValidation.Should().NotBeNull("GUID validation settings should be initialized");
        settings.EnableAuditLogging.Should().BeTrue("Audit logging should be enabled by default");
        settings.RequireUserAuthentication.Should().BeTrue("User authentication should be required by default");
        settings.RequireServiceAuthentication.Should().BeTrue("Service authentication should always be required");
    }

    [Fact]
    public void AuthenticationSettings_SectionName_IsCorrect()
    {
        // Assert
        AuthenticationSettings.SectionName.Should().Be("Authentication", "Section name should match configuration");
    }

    [Fact]
    public void AuthenticationSettings_RequireUserAuthentication_DependsOnMode()
    {
        // Test different authentication modes
        var testCases = new[]
        {
            (AuthenticationMode.Disabled, false, "Disabled mode should not require user authentication"),
            (AuthenticationMode.BearerToken, true, "BearerToken mode should require user authentication"),
            (AuthenticationMode.EntraExternalId, true, "EntraExternalId mode should require user authentication")
        };

        foreach (var (mode, expectedUserAuth, description) in testCases)
        {
            // Arrange
            var settings = new AuthenticationSettings { Mode = mode };

            // Act
            var requiresUserAuth = settings.RequireUserAuthentication;

            // Assert
            requiresUserAuth.Should().Be(expectedUserAuth, description);
        }
    }

    [Fact]
    public void AuthenticationSettings_RequireServiceAuthentication_AlwaysTrue()
    {
        // Test that service authentication is always required regardless of mode
        foreach (AuthenticationMode mode in Enum.GetValues<AuthenticationMode>())
        {
            // Arrange
            var settings = new AuthenticationSettings { Mode = mode };

            // Act
            var requiresServiceAuth = settings.RequireServiceAuthentication;

            // Assert
            requiresServiceAuth.Should().BeTrue($"Service authentication should always be required for mode {mode}");
        }
    }

    [Fact]
    public void AuthenticationSettings_CanBeConfigured_WithAllProperties()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ClockSkewInMinutes = 10
        };

        var serviceJwtSettings = new ServiceJwtSettings
        {
            ServiceIdentity = "test-service",
            ExpirationHours = 12,
            EnableCaching = false
        };

        var guidValidationSettings = new GuidValidationSettings
        {
            RejectEmptyGuid = true,
            ValidateInDatabase = false,
            ValidationCacheMinutes = 120
        };

        // Act
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.EntraExternalId,
            Jwt = jwtSettings,
            ServiceJwt = serviceJwtSettings,
            GuidValidation = guidValidationSettings,
            EnableAuditLogging = false
        };

        // Assert
        settings.Mode.Should().Be(AuthenticationMode.EntraExternalId);
        settings.Jwt.Should().Be(jwtSettings);
        settings.ServiceJwt.Should().Be(serviceJwtSettings);
        settings.GuidValidation.Should().Be(guidValidationSettings);
        settings.EnableAuditLogging.Should().BeFalse();
    }

    #region JwtSettings Tests

    [Fact]
    public void JwtSettings_DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var jwtSettings = new JwtSettings();

        // Assert
        jwtSettings.SecretKey.Should().Be("");
        jwtSettings.Issuer.Should().Be("https://localhost:7297");
        jwtSettings.Audience.Should().Be("FabrikamApi");
        jwtSettings.ValidateIssuer.Should().BeTrue();
        jwtSettings.ValidateAudience.Should().BeTrue();
        jwtSettings.ValidateLifetime.Should().BeTrue();
        jwtSettings.ValidateIssuerSigningKey.Should().BeTrue();
        jwtSettings.ClockSkewInMinutes.Should().Be(5);
        jwtSettings.RequireHttpsMetadata.Should().BeFalse();
    }

    [Fact]
    public void JwtSettings_CanBeConfigured_WithCustomValues()
    {
        // Arrange
        var customSecretKey = "my-custom-secret-key-that-is-long-enough-for-security";
        var customIssuer = "MyIssuer";
        var customAudience = "MyAudience";
        var customClockSkew = 10;

        // Act
        var jwtSettings = new JwtSettings
        {
            SecretKey = customSecretKey,
            Issuer = customIssuer,
            Audience = customAudience,
            ClockSkewInMinutes = customClockSkew,
            ValidateIssuer = false,
            RequireHttpsMetadata = true
        };

        // Assert
        jwtSettings.SecretKey.Should().Be(customSecretKey);
        jwtSettings.Issuer.Should().Be(customIssuer);
        jwtSettings.Audience.Should().Be(customAudience);
        jwtSettings.ClockSkewInMinutes.Should().Be(customClockSkew);
        jwtSettings.ValidateIssuer.Should().BeFalse();
        jwtSettings.RequireHttpsMetadata.Should().BeTrue();
    }

    #endregion

    #region ServiceJwtSettings Tests

    [Fact]
    public void ServiceJwtSettings_DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var serviceJwtSettings = new ServiceJwtSettings();

        // Assert
        serviceJwtSettings.ExpirationHours.Should().Be(24);
        serviceJwtSettings.ServiceIdentity.Should().Be("fabrikam-mcp-service");
        serviceJwtSettings.EnableCaching.Should().BeTrue();
        serviceJwtSettings.CacheRefreshThresholdMinutes.Should().Be(60);
    }

    [Fact]
    public void ServiceJwtSettings_CanBeConfigured_WithCustomValues()
    {
        // Arrange
        var customIdentity = "custom-service";
        var customExpiration = 12;

        // Act
        var serviceJwtSettings = new ServiceJwtSettings
        {
            ServiceIdentity = customIdentity,
            ExpirationHours = customExpiration,
            EnableCaching = false,
            CacheRefreshThresholdMinutes = 30
        };

        // Assert
        serviceJwtSettings.ServiceIdentity.Should().Be(customIdentity);
        serviceJwtSettings.ExpirationHours.Should().Be(customExpiration);
        serviceJwtSettings.EnableCaching.Should().BeFalse();
        serviceJwtSettings.CacheRefreshThresholdMinutes.Should().Be(30);
    }

    #endregion

    #region GuidValidationSettings Tests

    [Fact]
    public void GuidValidationSettings_DefaultConstruction_HasCorrectDefaults()
    {
        // Act
        var guidSettings = new GuidValidationSettings();

        // Assert
        guidSettings.GuidPattern.Should().Be("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
        guidSettings.RejectEmptyGuid.Should().BeTrue("Empty GUIDs should be rejected by default");
        guidSettings.ValidateInDatabase.Should().BeTrue("Database validation should be enabled by default");
        guidSettings.ValidationCacheMinutes.Should().Be(60);
    }

    [Fact]
    public void GuidValidationSettings_CanBeConfigured_WithCustomValues()
    {
        // Arrange
        var customPattern = "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$";

        // Act
        var guidSettings = new GuidValidationSettings
        {
            GuidPattern = customPattern,
            RejectEmptyGuid = false,
            ValidateInDatabase = false,
            ValidationCacheMinutes = 30
        };

        // Assert
        guidSettings.GuidPattern.Should().Be(customPattern);
        guidSettings.RejectEmptyGuid.Should().BeFalse();
        guidSettings.ValidateInDatabase.Should().BeFalse();
        guidSettings.ValidationCacheMinutes.Should().Be(30);
    }

    #endregion

    #region Configuration Scenarios Tests

    [Fact]
    public void AuthenticationSettings_DisabledMode_Configuration()
    {
        // Arrange - Simulate configuration for Disabled mode
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.Disabled,
            GuidValidation = new GuidValidationSettings
            {
                RejectEmptyGuid = true,
                ValidateInDatabase = true,
                ValidationCacheMinutes = 60
            },
            ServiceJwt = new ServiceJwtSettings
            {
                ServiceIdentity = "disabled-mode-service",
                ExpirationHours = 1
            },
            EnableAuditLogging = true
        };

        // Assert
        settings.RequireUserAuthentication.Should().BeFalse("Disabled mode doesn't require user authentication");
        settings.RequireServiceAuthentication.Should().BeTrue("Service authentication always required");
        settings.GuidValidation.ValidateInDatabase.Should().BeTrue("Database validation enabled for disabled mode");
    }

    [Fact]
    public void AuthenticationSettings_BearerTokenMode_Configuration()
    {
        // Arrange - Simulate configuration for Bearer Token mode
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.BearerToken,
            Jwt = new JwtSettings
            {
                Issuer = "FabrikamMcp",
                Audience = "FabrikamApi",
                ClockSkewInMinutes = 5
            },
            ServiceJwt = new ServiceJwtSettings
            {
                ServiceIdentity = "service-identity",
                ExpirationHours = 1
            }
        };

        // Assert
        settings.RequireUserAuthentication.Should().BeTrue("Bearer token mode requires user authentication");
        settings.RequireServiceAuthentication.Should().BeTrue("Service authentication always required");
        settings.Jwt.Should().NotBeNull("JWT settings required for bearer token mode");
    }

    [Fact]
    public void AuthenticationSettings_EntraExternalIdMode_Configuration()
    {
        // Arrange - Simulate configuration for Entra External ID mode
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.EntraExternalId,
            ServiceJwt = new ServiceJwtSettings
            {
                ServiceIdentity = "entra-service-identity",
                ExpirationHours = 2
            },
            EnableAuditLogging = true
        };

        // Assert
        settings.RequireUserAuthentication.Should().BeTrue("Entra External ID mode requires user authentication");
        settings.RequireServiceAuthentication.Should().BeTrue("Service authentication always required");
        settings.EnableAuditLogging.Should().BeTrue("Audit logging should be enabled for Entra mode");
    }

    #endregion

    #region Validation and Edge Cases

    [Fact]
    public void AuthenticationSettings_DefaultSubSettings_AreNotNull()
    {
        // Arrange & Act
        var settings = new AuthenticationSettings();

        // Assert - Default sub-settings should be initialized
        settings.Jwt.Should().NotBeNull("JwtSettings should be initialized by default");
        settings.ServiceJwt.Should().NotBeNull("ServiceJwtSettings should be initialized by default");
        settings.GuidValidation.Should().NotBeNull("GuidValidationSettings should be initialized by default");

        // Verify default values
        settings.Mode.Should().Be(AuthenticationMode.BearerToken);
        settings.RequireUserAuthentication.Should().BeTrue();
        settings.RequireServiceAuthentication.Should().BeTrue();
    }

    [Fact]
    public void AuthenticationSettings_Immutability_OfConstants()
    {
        // Assert - Verify that constants are truly constant
        AuthenticationSettings.SectionName.Should().Be("Authentication");
        
        // Verify we can't accidentally modify the constant (compile-time check)
        var sectionName = AuthenticationSettings.SectionName;
        sectionName.Should().Be("Authentication");
    }

    [Fact]
    public void AuthenticationSettings_SecurityByDefault_Architecture()
    {
        // Test the security-by-default architecture principles
        var settings = new AuthenticationSettings();

        // Assert - Security-by-default principles
        settings.RequireServiceAuthentication.Should().BeTrue("Service authentication always required");
        settings.EnableAuditLogging.Should().BeTrue("Audit logging enabled by default");
        settings.Mode.Should().Be(AuthenticationMode.BearerToken, "Default to secure mode");
        settings.GuidValidation.RejectEmptyGuid.Should().BeTrue("Reject empty GUIDs by default");
        settings.GuidValidation.ValidateInDatabase.Should().BeTrue("Database validation by default");
    }

    #endregion
}
