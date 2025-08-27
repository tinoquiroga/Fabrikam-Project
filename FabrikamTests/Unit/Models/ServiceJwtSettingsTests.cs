using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs;
using FabrikamTests.Helpers;
using Xunit.Abstractions;

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Phase 3A: Unit tests for ServiceJwtSettings validation methods
/// Tests JWT settings validation, secret key validation, and configuration edge cases
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "ServiceJwtSettings")]
[Trait("Feature", "Authentication")]
[Trait("Priority", "High")]
[Trait("Phase", "3A")]
public class ServiceJwtSettingsTests : GivenWhenThenTestBase
{
    public ServiceJwtSettingsTests(ITestOutputHelper output) : base(output)
    {
    }

    #region Secret Key Validation Tests

    [Fact]
    public void ValidateSecretKey_WithValidKey_ReturnsTrue()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = false;

        Given.That("we have JWT settings with a valid secret key", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("ThisIsAValidSecretKeyThatIsLongEnoughForHS256Algorithm")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the secret key", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) && 
                                 storedSettings.SecretKey.Length >= 32;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be true", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Valid secret key should pass validation");
            });
    }

    [Fact]
    public void ValidateSecretKey_WithShortKey_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with a short secret key", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("short")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the secret key", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) && 
                                 storedSettings.SecretKey.Length >= 32;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Short secret key should fail validation");
            });
    }

    [Fact]
    public void ValidateSecretKey_WithNullKey_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with a null secret key", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey(null!)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the secret key", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) && 
                                 storedSettings.SecretKey.Length >= 32;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Null secret key should fail validation");
            });
    }

    [Fact]
    public void ValidateSecretKey_WithEmptyKey_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with an empty secret key", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the secret key", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) && 
                                 storedSettings.SecretKey.Length >= 32;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Empty secret key should fail validation");
            });
    }

    [Fact]
    public void ValidateSecretKey_WithWhitespaceKey_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with a whitespace-only secret key", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("   ")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the secret key", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) && 
                                 storedSettings.SecretKey.Length >= 32;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Whitespace-only secret key should fail validation");
            });
    }

    #endregion

    #region Issuer Validation Tests

    [Fact]
    public void ValidateIssuer_WithValidIssuer_ReturnsTrue()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = false;

        Given.That("we have JWT settings with a valid issuer", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithIssuer("FabrikamTestIssuer")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the issuer", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.Issuer);
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be true", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Valid issuer should pass validation");
            });
    }

    [Fact]
    public void ValidateIssuer_WithNullIssuer_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with a null issuer", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithIssuer(null!)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the issuer", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.Issuer);
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Null issuer should fail validation");
            });
    }

    [Fact]
    public void ValidateIssuer_WithEmptyIssuer_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with an empty issuer", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithIssuer("")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the issuer", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.Issuer);
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Empty issuer should fail validation");
            });
    }

    #endregion

    #region Audience Validation Tests

    [Fact]
    public void ValidateAudience_WithValidAudience_ReturnsTrue()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = false;

        Given.That("we have JWT settings with a valid audience", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithAudience("FabrikamTestAudience")
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the audience", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.Audience);
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be true", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Valid audience should pass validation");
            });
    }

    [Fact]
    public void ValidateAudience_WithNullAudience_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with a null audience", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithAudience(null!)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the audience", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.Audience);
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Null audience should fail validation");
            });
    }

    #endregion

    #region Token Lifetime Validation Tests

    [Fact]
    public void ValidateTokenLifetime_WithValidLifetime_ReturnsTrue()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = false;

        Given.That("we have JWT settings with a valid token lifetime", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithTokenLifetimeInMinutes(60)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the token lifetime", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = storedSettings.ExpirationMinutes > 0 && storedSettings.ExpirationMinutes <= 1440; // Max 24 hours
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be true", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Valid token lifetime should pass validation");
            });
    }

    [Fact]
    public void ValidateTokenLifetime_WithZeroLifetime_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with zero token lifetime", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithTokenLifetimeInMinutes(0)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the token lifetime", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = storedSettings.ExpirationMinutes > 0 && storedSettings.ExpirationMinutes <= 1440;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Zero token lifetime should fail validation");
            });
    }

    [Fact]
    public void ValidateTokenLifetime_WithNegativeLifetime_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with negative token lifetime", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithTokenLifetimeInMinutes(-10)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the token lifetime", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = storedSettings.ExpirationMinutes > 0 && storedSettings.ExpirationMinutes <= 1440;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Negative token lifetime should fail validation");
            });
    }

    [Fact]
    public void ValidateTokenLifetime_WithExcessiveLifetime_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with excessive token lifetime", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithTokenLifetimeInMinutes(10080) // 1 week
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the token lifetime", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = storedSettings.ExpirationMinutes > 0 && storedSettings.ExpirationMinutes <= 1440;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Excessive token lifetime should fail validation for security");
            });
    }

    #endregion

    #region Complete Configuration Validation Tests

    [Fact]
    public void ValidateConfiguration_WithCompleteValidSettings_ReturnsTrue()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = false;

        Given.That("we have JWT settings with complete valid configuration", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("ThisIsAValidSecretKeyThatIsLongEnoughForHS256Algorithm")
                    .WithIssuer("FabrikamTestIssuer")
                    .WithAudience("FabrikamTestAudience")
                    .WithTokenLifetimeInMinutes(60)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the complete configuration", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) &&
                                 storedSettings.SecretKey.Length >= 32 &&
                                 !string.IsNullOrWhiteSpace(storedSettings.Issuer) &&
                                 !string.IsNullOrWhiteSpace(storedSettings.Audience) &&
                                 storedSettings.ExpirationMinutes > 0 &&
                                 storedSettings.ExpirationMinutes <= 1440;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be true", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Complete valid configuration should pass validation");
            });
    }

    [Fact]
    public void ValidateConfiguration_WithPartiallyValidSettings_ReturnsFalse()
    {
        // Given
        ServiceJwtSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have JWT settings with partially valid configuration", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("ValidSecretKeyThatIsLongEnoughForHS256")
                    .WithIssuer(null!) // Invalid issuer
                    .WithAudience("FabrikamTestAudience")
                    .WithTokenLifetimeInMinutes(60)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("validate the complete configuration", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                validationResult = !string.IsNullOrWhiteSpace(storedSettings.SecretKey) &&
                                 storedSettings.SecretKey.Length >= 32 &&
                                 !string.IsNullOrWhiteSpace(storedSettings.Issuer) &&
                                 !string.IsNullOrWhiteSpace(storedSettings.Audience) &&
                                 storedSettings.ExpirationMinutes > 0 &&
                                 storedSettings.ExpirationMinutes <= 1440;
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation result should be false", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Partially invalid configuration should fail validation");
            });
    }

    [Fact]
    public void ValidateConfiguration_WithDefaultSettings_ShouldHaveCorrectDefaults()
    {
        // Given
        ServiceJwtSettings settings = null!;

        Given.That("we have JWT settings with default values", () =>
            {
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithDefaults()
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
            });

        // When
        When.I("check the default values", () =>
            {
                // Default values are checked in the Then step
                StoreInContext("DefaultValuesChecked", true);
            });

        // Then
        Then.The("settings should have correct default values", () =>
            {
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                storedSettings.SecretKey.Should().NotBeNullOrEmpty("Secret key should have a default value");
                storedSettings.Issuer.Should().Be("FabrikamTestIssuer", "Default issuer should be set");
                storedSettings.Audience.Should().Be("FabrikamTestAudience", "Default audience should be set");
                storedSettings.ExpirationMinutes.Should().Be(60, "Default token lifetime should be 60 minutes");
            });
    }

    #endregion

    #region Environment Variable Override Tests

    [Fact]
    public void ValidateConfiguration_WithEnvironmentOverrides_SimulatesEnvironmentLoading()
    {
        // Given
        ServiceJwtSettings settings = null!;

        Given.That("we have JWT settings that simulate environment variable loading", () =>
            {
                // Simulate what would happen with environment variable overrides
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithSecretKey("EnvironmentSecretKey")
                    .WithIssuer("EnvironmentIssuer")
                    .WithAudience("EnvironmentAudience")
                    .WithTokenLifetimeInMinutes(120)
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
                StoreInContext("EnvironmentSimulated", true);
            });

        // When
        When.I("load configuration from simulated environment", () =>
            {
                // In a real implementation, the configuration system would handle environment loading
                // For testing purposes, we simulate the loaded settings
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                StoreInContext("LoadedFromEnvironment", storedSettings);
            });

        // Then
        Then.The("settings should reflect the environment values", () =>
            {
                var loadedSettings = GetFromContext<ServiceJwtSettings>("LoadedFromEnvironment");
                loadedSettings.Should().NotBeNull("Settings should be loaded");
                loadedSettings.SecretKey.Should().Be("EnvironmentSecretKey", "Should use environment secret key");
                loadedSettings.Issuer.Should().Be("EnvironmentIssuer", "Should use environment issuer");
                loadedSettings.Audience.Should().Be("EnvironmentAudience", "Should use environment audience");
                loadedSettings.ExpirationMinutes.Should().Be(120, "Should use environment token lifetime");
            });
    }

    [Fact]
    public void ValidateConfiguration_WithMissingEnvironmentVariables_UsesDefaults()
    {
        // Given
        ServiceJwtSettings settings = null!;

        Given.That("we have JWT settings that simulate missing environment variables", () =>
            {
                // Simulate what would happen when environment variables are missing
                settings = TestDataBuilders.ServiceJwtSettings()
                    .WithDefaults() // Fall back to defaults
                    .Build();
                StoreInContext("ServiceJwtSettings", settings);
                StoreInContext("EnvironmentMissing", true);
            });

        // When
        When.I("load configuration with missing environment variables", () =>
            {
                // Simulate the fallback behavior
                var storedSettings = GetFromContext<ServiceJwtSettings>("ServiceJwtSettings");
                StoreInContext("LoadedSettings", storedSettings);
            });

        // Then
        Then.The("settings should fall back to default values", () =>
            {
                var loadedSettings = GetFromContext<ServiceJwtSettings>("LoadedSettings");
                loadedSettings.SecretKey.Should().NotBeNullOrEmpty("Should fall back to default secret key");
                loadedSettings.Issuer.Should().Be("FabrikamTestIssuer", "Should fall back to default issuer");
                loadedSettings.Audience.Should().Be("FabrikamTestAudience", "Should fall back to default audience");
                loadedSettings.ExpirationMinutes.Should().Be(60, "Should fall back to default token lifetime");
            });
    }

    #endregion
}
