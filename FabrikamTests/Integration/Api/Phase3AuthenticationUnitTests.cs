using FluentAssertions;
using FabrikamContracts.DTOs;
using FabrikamApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace FabrikamTests.Integration.Api;

/// <summary>
/// Phase 3 Authentication Unit Tests - Individual Service Tests
/// Tests ServiceJwtSettings validation, AuthenticationConfiguration edge cases,
/// environment variable parsing, error handling, and configuration validation
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Component", "Phase3UnitTests")]
[Trait("TestType", "UnitTest")]
public class Phase3AuthenticationUnitTests
{
    #region ServiceJwtSettings Validation Tests

    [Fact]
    public void ServiceJwtSettings_ValidateSecretKey_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Record.Exception(() => settings.Validate());
        exception.Should().BeNull("Valid settings should not throw an exception");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateSecretKey_WithShortKey_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "TooShort", // Less than 32 characters
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("SecretKey must be at least 32 characters");
        exception.ParamName.Should().Be("SecretKey");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateSecretKey_WithEmptyKey_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "", // Empty string
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("SecretKey cannot be empty");
        exception.ParamName.Should().Be("SecretKey");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateSecretKey_WithNullKey_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = null!, // Null value
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("SecretKey cannot be empty");
        exception.ParamName.Should().Be("SecretKey");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateIssuer_WithEmptyIssuer_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "", // Empty issuer
            Audience = "TestAudience",
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("Issuer cannot be empty");
        exception.ParamName.Should().Be("Issuer");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateAudience_WithEmptyAudience_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "", // Empty audience
            ServiceIdentity = "TestService"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("Audience cannot be empty");
        exception.ParamName.Should().Be("Audience");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateServiceIdentity_WithEmptyIdentity_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "" // Empty service identity
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("ServiceIdentity cannot be empty");
        exception.ParamName.Should().Be("ServiceIdentity");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateExpirationMinutes_WithZeroValue_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService",
            ExpirationMinutes = 0 // Invalid expiration
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be between 1 and 10080 minutes");
        exception.ParamName.Should().Be("ExpirationMinutes");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateExpirationMinutes_WithExcessiveValue_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService",
            ExpirationMinutes = 20160 // More than 1 week (10080 minutes)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be between 1 and 10080 minutes");
        exception.ParamName.Should().Be("ExpirationMinutes");
    }

    [Fact]
    public void ServiceJwtSettings_ValidateCacheRefreshThreshold_WithZeroValue_ThrowsException()
    {
        // Arrange
        var settings = new ServiceJwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ServiceIdentity = "TestService",
            CacheRefreshThresholdMinutes = 0 // Invalid threshold
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be at least 1 minute");
        exception.ParamName.Should().Be("CacheRefreshThresholdMinutes");
    }

    #endregion

    #region JwtSettings Validation Tests

    [Fact]
    public void JwtSettings_ValidateSecretKey_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        // Act & Assert
        var exception = Record.Exception(() => settings.Validate());
        exception.Should().BeNull("Valid settings should not throw an exception");
    }

    [Fact]
    public void JwtSettings_ValidateSecretKey_WithShortKey_ThrowsException()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "TooShort", // Less than 32 characters
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("SecretKey must be at least 32 characters for security");
        exception.ParamName.Should().Be("SecretKey");
    }

    [Fact]
    public void JwtSettings_ValidateExpirationMinutes_WithExcessiveValue_ThrowsException()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 2000 // More than 24 hours (1440 minutes)
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be between 1 and 1440 minutes");
        exception.ParamName.Should().Be("ExpirationMinutes");
    }

    [Fact]
    public void JwtSettings_ValidateClockSkew_WithNegativeValue_ThrowsException()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ClockSkewInMinutes = -1 // Negative clock skew
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be between 0 and 60 minutes");
        exception.ParamName.Should().Be("ClockSkewInMinutes");
    }

    [Fact]
    public void JwtSettings_ValidateClockSkew_WithExcessiveValue_ThrowsException()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ClockSkewInMinutes = 120 // More than 60 minutes
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        exception.Message.Should().Contain("Must be between 0 and 60 minutes");
        exception.ParamName.Should().Be("ClockSkewInMinutes");
    }

    #endregion

    #region Environment Variable Parsing Tests

    [Theory]
    [InlineData("Test", AuthenticationMode.Disabled)]
    [InlineData("Testing", AuthenticationMode.Disabled)]
    [InlineData("Development", AuthenticationMode.BearerToken)]
    [InlineData("Production", AuthenticationMode.BearerToken)]
    [InlineData("Staging", AuthenticationMode.BearerToken)]
    [InlineData(null, AuthenticationMode.BearerToken)]
    [InlineData("", AuthenticationMode.BearerToken)]
    public void AuthenticationSettings_GetDefaultAuthenticationMode_WithEnvironmentValues_ReturnsCorrectMode(string? environment, AuthenticationMode expectedMode)
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            // Set environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);

            // Act
            var settings = new AuthenticationSettings();

            // Assert
            settings.Mode.Should().Be(expectedMode, $"Environment '{environment}' should result in {expectedMode} mode");
        }
        finally
        {
            // Restore original environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Theory]
    [InlineData("ContainsTestInName", AuthenticationMode.Disabled)]
    [InlineData("MyTestEnvironment", AuthenticationMode.Disabled)]
    [InlineData("TestStage", AuthenticationMode.Disabled)]
    [InlineData("UnitTest", AuthenticationMode.Disabled)]
    [InlineData("IntegrationTest", AuthenticationMode.Disabled)]
    public void AuthenticationSettings_GetDefaultAuthenticationMode_WithTestInEnvironmentName_ReturnsDisabled(string environment, AuthenticationMode expectedMode)
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            // Set environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);

            // Act
            var settings = new AuthenticationSettings();

            // Assert
            settings.Mode.Should().Be(expectedMode, $"Environment '{environment}' containing 'Test' should result in {expectedMode} mode");
        }
        finally
        {
            // Restore original environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public void AuthenticationSettings_GetDefaultAuthenticationMode_WithDotnetEnvironmentFallback_UsesCorrectValue()
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            // Clear ASPNETCORE_ENVIRONMENT and set DOTNET_ENVIRONMENT
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");

            // Act
            var settings = new AuthenticationSettings();

            // Assert
            settings.Mode.Should().Be(AuthenticationMode.Disabled, "Should fall back to DOTNET_ENVIRONMENT when ASPNETCORE_ENVIRONMENT is not set");
        }
        finally
        {
            // Restore original environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    #endregion

    #region AuthenticationConfiguration Edge Cases Tests

    [Fact]
    public void AuthenticationConfiguration_GetEffectiveAuthenticationMode_WithValidSettings_ReturnsCorrectMode()
    {
        // Arrange
        var settings = new AuthenticationSettings { Mode = AuthenticationMode.BearerToken };

        // Act
        var effectiveMode = AuthenticationConfiguration.GetEffectiveAuthenticationMode(settings);

        // Assert
        effectiveMode.Should().Be(AuthenticationMode.BearerToken);
    }

    [Fact]
    public void AuthenticationConfiguration_IsUserAuthenticationRequired_WithDisabledMode_ReturnsFalse()
    {
        // Arrange
        var settings = new AuthenticationSettings { Mode = AuthenticationMode.Disabled };

        // Act
        var isRequired = AuthenticationConfiguration.IsUserAuthenticationRequired(settings);

        // Assert
        isRequired.Should().BeFalse("User authentication should not be required in Disabled mode");
    }

    [Fact]
    public void AuthenticationConfiguration_IsUserAuthenticationRequired_WithBearerTokenMode_ReturnsTrue()
    {
        // Arrange
        var settings = new AuthenticationSettings { Mode = AuthenticationMode.BearerToken };

        // Act
        var isRequired = AuthenticationConfiguration.IsUserAuthenticationRequired(settings);

        // Assert
        isRequired.Should().BeTrue("User authentication should be required in BearerToken mode");
    }

    [Fact]
    public void AuthenticationConfiguration_IsUserAuthenticationRequired_WithEntraExternalIdMode_ReturnsTrue()
    {
        // Arrange
        var settings = new AuthenticationSettings { Mode = AuthenticationMode.EntraExternalId };

        // Act
        var isRequired = AuthenticationConfiguration.IsUserAuthenticationRequired(settings);

        // Assert
        isRequired.Should().BeTrue("User authentication should be required in EntraExternalId mode");
    }

    [Fact]
    public void AuthenticationConfiguration_IsServiceAuthenticationRequired_AlwaysReturnsTrue()
    {
        // Arrange
        var disabledSettings = new AuthenticationSettings { Mode = AuthenticationMode.Disabled };
        var bearerSettings = new AuthenticationSettings { Mode = AuthenticationMode.BearerToken };
        var entraSettings = new AuthenticationSettings { Mode = AuthenticationMode.EntraExternalId };

        // Act & Assert
        AuthenticationConfiguration.IsServiceAuthenticationRequired(disabledSettings).Should().BeTrue("Service authentication should always be required");
        AuthenticationConfiguration.IsServiceAuthenticationRequired(bearerSettings).Should().BeTrue("Service authentication should always be required");
        AuthenticationConfiguration.IsServiceAuthenticationRequired(entraSettings).Should().BeTrue("Service authentication should always be required");
    }

    #endregion

    #region Error Handling Scenarios Tests

    [Fact]
    public void AuthenticationSettings_Validate_WithMissingJwtSecretInBearerTokenMode_ThrowsException()
    {
        // Arrange
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.BearerToken,
            Jwt = new JwtSettings
            {
                SecretKey = "", // Missing secret key
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => settings.Validate());
        exception.Message.Should().Contain("JWT SecretKey is required for BearerToken mode");
    }

    [Fact]
    public void AuthenticationSettings_Validate_WithMissingEntraSettingsInEntraMode_ThrowsException()
    {
        // Arrange
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.EntraExternalId,
            EntraExternalId = null // Missing Entra settings
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => settings.Validate());
        exception.Message.Should().Contain("EntraExternalId settings are required for EntraExternalId mode");
    }

    [Fact]
    public void AuthenticationSettings_Validate_WithInvalidServiceJwtSettings_ThrowsException()
    {
        // Arrange
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.Disabled,
            ServiceJwt = new ServiceJwtSettings
            {
                SecretKey = "TooShort", // Invalid secret key
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ServiceIdentity = "TestService"
            }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => settings.Validate());
        exception.Message.Should().Contain("SecretKey must be at least 32 characters");
    }

    [Fact]
    public void AuthenticationSettings_ConfigureEnvironmentAwareAuthentication_WithNullConfiguration_HandlesGracefully()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        
        var configBuilder = new ConfigurationBuilder();
        var config = configBuilder.Build();

        // Act & Assert
        var exception = Record.Exception(() => 
            services.ConfigureEnvironmentAwareAuthentication(config, mockEnv.Object));
        
        exception.Should().BeNull("Should handle missing authentication configuration gracefully");
    }

    #endregion

    #region Configuration Validation Logic Tests

    [Fact]
    public void AuthenticationSettings_RequireUserAuthentication_ReflectsMode()
    {
        // Arrange & Act & Assert
        var disabledSettings = new AuthenticationSettings { Mode = AuthenticationMode.Disabled };
        disabledSettings.RequireUserAuthentication.Should().BeFalse("Disabled mode should not require user authentication");

        var bearerSettings = new AuthenticationSettings { Mode = AuthenticationMode.BearerToken };
        bearerSettings.RequireUserAuthentication.Should().BeTrue("BearerToken mode should require user authentication");

        var entraSettings = new AuthenticationSettings { Mode = AuthenticationMode.EntraExternalId };
        entraSettings.RequireUserAuthentication.Should().BeTrue("EntraExternalId mode should require user authentication");
    }

    [Fact]
    public void AuthenticationSettings_RequireServiceAuthentication_AlwaysTrue()
    {
        // Arrange & Act & Assert
        var settings1 = new AuthenticationSettings { Mode = AuthenticationMode.Disabled };
        var settings2 = new AuthenticationSettings { Mode = AuthenticationMode.BearerToken };
        var settings3 = new AuthenticationSettings { Mode = AuthenticationMode.EntraExternalId };

        settings1.RequireServiceAuthentication.Should().BeTrue("Service authentication should always be required");
        settings2.RequireServiceAuthentication.Should().BeTrue("Service authentication should always be required");
        settings3.RequireServiceAuthentication.Should().BeTrue("Service authentication should always be required");
    }

    [Fact]
    public void AuthenticationSettings_SectionName_IsCorrect()
    {
        // Act & Assert
        AuthenticationSettings.SectionName.Should().Be("Authentication", "Section name should match configuration section");
    }

    [Fact]
    public void JwtSettings_SectionName_IsCorrect()
    {
        // Act & Assert
        JwtSettings.SectionName.Should().Be("Authentication:Jwt", "Section name should match configuration section");
    }

    [Fact]
    public void AuthenticationSettings_DefaultSubSettings_AreInitialized()
    {
        // Act
        var settings = new AuthenticationSettings();

        // Assert
        settings.Jwt.Should().NotBeNull("JWT settings should be initialized");
        settings.ServiceJwt.Should().NotBeNull("Service JWT settings should be initialized");
        settings.GuidValidation.Should().NotBeNull("GUID validation settings should be initialized");
        settings.EnableAuditLogging.Should().BeTrue("Audit logging should be enabled by default");
    }

    [Fact]
    public void AuthenticationSettings_DefaultValues_AreSecure()
    {
        // Act
        var settings = new AuthenticationSettings();

        // Assert
        settings.Mode.Should().Be(AuthenticationMode.BearerToken, "Default mode should be secure (BearerToken)");
        settings.RequireUserAuthentication.Should().BeTrue("Should require user authentication by default");
        settings.RequireServiceAuthentication.Should().BeTrue("Should require service authentication by default");
        settings.EnableAuditLogging.Should().BeTrue("Should enable audit logging by default");
    }

    #endregion

    #region Complex Integration Scenarios

    [Fact]
    public void AuthenticationSettings_CompleteValidation_WithValidBearerTokenConfiguration_Succeeds()
    {
        // Arrange
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.BearerToken,
            Jwt = new JwtSettings
            {
                SecretKey = "ValidSecretKeyThatIsAtLeast32CharactersLongForSecurity",
                Issuer = "https://api.fabrikam.com",
                Audience = "FabrikamApiClients",
                ExpirationMinutes = 60,
                ClockSkewInMinutes = 5,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            },
            ServiceJwt = new ServiceJwtSettings
            {
                SecretKey = "ValidServiceSecretKeyThatIsAtLeast32CharactersLongForSecurity",
                Issuer = "FabrikamMcp",
                Audience = "FabrikamServices",
                ServiceIdentity = "fabrikam-mcp-service",
                ExpirationMinutes = 1440,
                EnableCaching = true,
                CacheRefreshThresholdMinutes = 60
            }
        };

        // Act & Assert
        var exception = Record.Exception(() => settings.Validate());
        exception.Should().BeNull("Complete valid configuration should not throw an exception");
    }

    [Fact]
    public void AuthenticationSettings_CompleteValidation_WithValidDisabledConfiguration_Succeeds()
    {
        // Arrange
        var settings = new AuthenticationSettings
        {
            Mode = AuthenticationMode.Disabled,
            GuidValidation = new GuidValidationSettings
            {
                Enabled = true,
                ValidateMicrosoftGuidFormat = true,
                ValidationRules = new[] { "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$" }
            },
            ServiceJwt = new ServiceJwtSettings
            {
                SecretKey = "ValidServiceSecretKeyThatIsAtLeast32CharactersLongForSecurity",
                Issuer = "FabrikamMcp",
                Audience = "FabrikamServices",
                ServiceIdentity = "fabrikam-mcp-service",
                ExpirationMinutes = 1440
            }
        };

        // Act & Assert
        var exception = Record.Exception(() => settings.Validate());
        exception.Should().BeNull("Complete valid disabled configuration should not throw an exception");
    }

    #endregion
}
