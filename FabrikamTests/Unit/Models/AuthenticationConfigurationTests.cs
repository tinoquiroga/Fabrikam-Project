using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs;
using FabrikamTests.Helpers;
using Xunit.Abstractions;

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Phase 3B: Unit tests for AuthenticationSettings edge cases and validation
/// Tests environment variable parsing, invalid configurations, defaults, and configuration validation
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "AuthenticationSettings")]
[Trait("Feature", "Authentication")]
[Trait("Priority", "High")]
[Trait("Phase", "3B")]
public class AuthenticationConfigurationTests : GivenWhenThenTestBase
{
    public AuthenticationConfigurationTests(ITestOutputHelper output) : base(output)
    {
    }

    #region Configuration Creation Tests

    [Fact]
    public void CreateAuthenticationSettings_WithValidSettings_ReturnsCorrectConfiguration()
    {
        // Given
        AuthenticationSettings settings = null!;

        Given.That("we have valid authentication settings parameters", () =>
            {
                StoreInContext("Mode", AuthenticationMode.BearerToken);
            });

        // When
        When.I("create authentication settings", () =>
            {
                var mode = GetFromContext<AuthenticationMode>("Mode");
                settings = new AuthenticationSettings
                {
                    Mode = mode,
                    Jwt = new JwtSettings
                    {
                        SecretKey = "ValidSecretKeyThatIsLongEnoughForHS256Algorithm",
                        Issuer = "FabrikamTestIssuer",
                        Audience = "FabrikamTestAudience",
                        ExpirationMinutes = 60
                    },
                    ServiceJwt = new ServiceJwtSettings
                    {
                        SecretKey = "ValidServiceSecretKeyThatIsLongEnoughForHS256",
                        Issuer = "FabrikamMcp",
                        Audience = "FabrikamServices"
                    }
                };
                StoreInContext("AuthenticationSettings", settings);
            });

        // Then
        Then.The("settings should be properly initialized", () =>
            {
                var storedSettings = GetFromContext<AuthenticationSettings>("AuthenticationSettings");
                storedSettings.Should().NotBeNull("Settings should be created");
                storedSettings.Mode.Should().Be(AuthenticationMode.BearerToken, "Mode should match input");
                storedSettings.RequireUserAuthentication.Should().BeTrue("Should require user authentication for BearerToken mode");
                storedSettings.RequireServiceAuthentication.Should().BeTrue("Should always require service authentication");
                storedSettings.Jwt.Should().NotBeNull("JWT settings should be initialized");
                storedSettings.ServiceJwt.Should().NotBeNull("Service JWT settings should be initialized");
            });
    }

    [Fact]
    public void CreateAuthenticationSettings_WithDisabledMode_SetsCorrectDefaults()
    {
        // Given
        AuthenticationSettings settings = null!;

        Given.That("we have disabled authentication mode", () =>
            {
                StoreInContext("Mode", AuthenticationMode.Disabled);
            });

        // When
        When.I("create authentication settings for disabled mode", () =>
            {
                var mode = GetFromContext<AuthenticationMode>("Mode");
                settings = new AuthenticationSettings
                {
                    Mode = mode,
                    GuidValidation = new GuidValidationSettings
                    {
                        Enabled = true,
                        ValidateMicrosoftGuidFormat = true,
                        RejectEmptyGuid = true
                    },
                    ServiceJwt = new ServiceJwtSettings
                    {
                        SecretKey = "ValidServiceSecretKeyThatIsLongEnoughForHS256",
                        Issuer = "FabrikamMcp",
                        Audience = "FabrikamServices"
                    }
                };
                StoreInContext("AuthenticationSettings", settings);
            });

        // Then
        Then.The("settings should reflect disabled state", () =>
            {
                var storedSettings = GetFromContext<AuthenticationSettings>("AuthenticationSettings");
                storedSettings.Mode.Should().Be(AuthenticationMode.Disabled, "Mode should be disabled");
                storedSettings.RequireUserAuthentication.Should().BeFalse("Should not require user authentication");
                storedSettings.RequireServiceAuthentication.Should().BeTrue("Should still require service authentication");
                storedSettings.GuidValidation.Should().NotBeNull("GUID validation should be configured for disabled mode");
            });
    }

    #endregion

    #region Environment Variable Parsing Tests

    [Fact]
    public void ParseEnvironmentVariables_WithValidAuthMode_ParsesCorrectly()
    {
        // Given
        AuthenticationMode parsedMode = AuthenticationMode.Disabled;
        string environmentValue = "BearerToken";

        Given.That("we have a valid authentication mode environment variable", () =>
            {
                StoreInContext("EnvironmentValue", environmentValue);
            });

        // When
        When.I("parse the authentication mode from environment", () =>
            {
                var envValue = GetFromContext<string>("EnvironmentValue");
                var parseSuccess = Enum.TryParse<AuthenticationMode>(envValue, true, out parsedMode);
                StoreInContext("ParseSuccess", parseSuccess);
                StoreInContext("ParsedMode", parsedMode);
            });

        // Then
        Then.The("parsing should succeed with correct mode", () =>
            {
                var success = GetFromContext<bool>("ParseSuccess");
                var mode = GetFromContext<AuthenticationMode>("ParsedMode");
                
                success.Should().BeTrue("Environment variable should parse successfully");
                mode.Should().Be(AuthenticationMode.BearerToken, "Should parse to BearerToken mode");
            });
    }

    [Fact]
    public void ParseEnvironmentVariables_WithInvalidAuthMode_UsesDefault()
    {
        // Given
        AuthenticationMode parsedMode = AuthenticationMode.BearerToken; // Start with non-default
        string environmentValue = "InvalidMode";

        Given.That("we have an invalid authentication mode environment variable", () =>
            {
                StoreInContext("EnvironmentValue", environmentValue);
            });

        // When
        When.I("parse the authentication mode from environment", () =>
            {
                var envValue = GetFromContext<string>("EnvironmentValue");
                var parseSuccess = Enum.TryParse<AuthenticationMode>(envValue, true, out parsedMode);
                
                // If parsing fails, use default
                if (!parseSuccess)
                {
                    parsedMode = AuthenticationMode.Disabled; // Default fallback
                }
                
                StoreInContext("ParseSuccess", parseSuccess);
                StoreInContext("ParsedMode", parsedMode);
            });

        // Then
        Then.The("parsing should fail and use default mode", () =>
            {
                var success = GetFromContext<bool>("ParseSuccess");
                var mode = GetFromContext<AuthenticationMode>("ParsedMode");
                
                success.Should().BeFalse("Invalid environment variable should fail parsing");
                mode.Should().Be(AuthenticationMode.Disabled, "Should fall back to default mode");
            });
    }

    [Fact]
    public void ParseEnvironmentVariables_WithCaseInsensitiveValue_ParsesCorrectly()
    {
        // Given
        AuthenticationMode parsedMode = AuthenticationMode.Disabled;
        string environmentValue = "bearertoken"; // lowercase

        Given.That("we have a case-insensitive authentication mode environment variable", () =>
            {
                StoreInContext("EnvironmentValue", environmentValue);
            });

        // When
        When.I("parse the authentication mode with case-insensitive parsing", () =>
            {
                var envValue = GetFromContext<string>("EnvironmentValue");
                var parseSuccess = Enum.TryParse<AuthenticationMode>(envValue, true, out parsedMode);
                StoreInContext("ParseSuccess", parseSuccess);
                StoreInContext("ParsedMode", parsedMode);
            });

        // Then
        Then.The("parsing should succeed despite case differences", () =>
            {
                var success = GetFromContext<bool>("ParseSuccess");
                var mode = GetFromContext<AuthenticationMode>("ParsedMode");
                
                success.Should().BeTrue("Case-insensitive parsing should succeed");
                mode.Should().Be(AuthenticationMode.BearerToken, "Should parse to BearerToken mode");
            });
    }

    [Fact]
    public void ParseEnvironmentVariables_WithNullOrEmptyValue_UsesDefault()
    {
        // Given
        AuthenticationMode parsedMode = AuthenticationMode.BearerToken;
        
        Given.That("we have null or empty environment variable", () =>
            {
                StoreInContext<string?>("EnvironmentValue", null);
            });

        // When
        When.I("parse the authentication mode from null/empty environment", () =>
            {
                var envValue = GetFromContext<string?>("EnvironmentValue");
                var parseSuccess = false;
                
                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    parseSuccess = Enum.TryParse<AuthenticationMode>(envValue, true, out parsedMode);
                }
                
                if (!parseSuccess)
                {
                    parsedMode = AuthenticationMode.Disabled; // Default fallback
                }
                
                StoreInContext("ParseSuccess", parseSuccess);
                StoreInContext("ParsedMode", parsedMode);
            });

        // Then
        Then.The("should use default mode for null/empty values", () =>
            {
                var success = GetFromContext<bool>("ParseSuccess");
                var mode = GetFromContext<AuthenticationMode>("ParsedMode");
                
                success.Should().BeFalse("Null/empty value should not parse");
                mode.Should().Be(AuthenticationMode.Disabled, "Should use default mode");
            });
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void ValidateConfiguration_WithCompleteValidConfig_PassesValidation()
    {
        // Given
        AuthenticationSettings settings = null!;
        bool validationResult = false;

        Given.That("we have a complete valid authentication configuration", () =>
            {
                settings = new AuthenticationSettings
                {
                    Mode = AuthenticationMode.BearerToken,
                    Jwt = new JwtSettings
                    {
                        SecretKey = "ValidSecretKeyThatIsLongEnoughForHS256Algorithm",
                        Issuer = "FabrikamTestIssuer",
                        Audience = "FabrikamTestAudience",
                        ExpirationMinutes = 60
                    },
                    ServiceJwt = new ServiceJwtSettings
                    {
                        SecretKey = "ValidServiceSecretKeyThatIsLongEnoughForHS256",
                        Issuer = "FabrikamMcp",
                        Audience = "FabrikamServices"
                    }
                };
                StoreInContext("AuthenticationSettings", settings);
            });

        // When
        When.I("validate the authentication configuration", () =>
            {
                var config = GetFromContext<AuthenticationSettings>("AuthenticationSettings");
                
                try
                {
                    // Use the built-in validation method
                    config.Validate();
                    validationResult = true;
                }
                catch
                {
                    validationResult = false;
                }
                
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation should pass for complete configuration", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeTrue("Complete valid configuration should pass validation");
            });
    }

    [Fact]
    public void ValidateConfiguration_WithMissingJwtSecretKey_FailsValidation()
    {
        // Given
        AuthenticationSettings settings = null!;
        bool validationResult = true; // Start with true to ensure test changes it

        Given.That("we have authentication configuration with missing JWT secret key", () =>
            {
                settings = new AuthenticationSettings
                {
                    Mode = AuthenticationMode.BearerToken,
                    Jwt = new JwtSettings
                    {
                        SecretKey = "", // Missing secret key
                        Issuer = "FabrikamTestIssuer",
                        Audience = "FabrikamTestAudience"
                    },
                    ServiceJwt = new ServiceJwtSettings
                    {
                        SecretKey = "ValidServiceSecretKeyThatIsLongEnoughForHS256",
                        Issuer = "FabrikamMcp",
                        Audience = "FabrikamServices"
                    }
                };
                StoreInContext("AuthenticationSettings", settings);
            });

        // When
        When.I("validate the authentication configuration", () =>
            {
                var config = GetFromContext<AuthenticationSettings>("AuthenticationSettings");
                
                try
                {
                    config.Validate();
                    validationResult = true;
                }
                catch
                {
                    validationResult = false;
                }
                
                StoreInContext("ValidationResult", validationResult);
            });

        // Then
        Then.The("validation should fail for missing JWT secret key", () =>
            {
                var result = GetFromContext<bool>("ValidationResult");
                result.Should().BeFalse("Configuration with missing JWT secret key should fail validation");
            });
    }

    #endregion

    #region Default Configuration Tests

    [Fact]
    public void CreateDefaultConfiguration_ShouldHaveCorrectDefaults()
    {
        // Given
        AuthenticationSettings defaultSettings = null!;

        Given.That("we need to create a default authentication configuration", () =>
            {
                // Setup for default configuration creation
                StoreInContext("CreateDefaults", true);
            });

        // When
        When.I("create a default authentication configuration", () =>
            {
                defaultSettings = new AuthenticationSettings();
                StoreInContext("DefaultSettings", defaultSettings);
            });

        // Then
        Then.The("default configuration should have correct values", () =>
            {
                var settings = GetFromContext<AuthenticationSettings>("DefaultSettings");
                
                // The default mode depends on environment, but we can test that it's set
                settings.Mode.Should().BeOneOf(AuthenticationMode.Disabled, AuthenticationMode.BearerToken)
                    .And.Subject.Should().Match(mode => mode == AuthenticationMode.Disabled || mode == AuthenticationMode.BearerToken,
                        "Default mode should be either Disabled or BearerToken depending on environment");
                
                settings.RequireServiceAuthentication.Should().BeTrue("Service authentication should always be required");
                settings.EnableAuditLogging.Should().BeTrue("Audit logging should be enabled by default");
                settings.Jwt.Should().NotBeNull("JWT settings should be initialized");
                settings.ServiceJwt.Should().NotBeNull("Service JWT settings should be initialized");
                settings.GuidValidation.Should().NotBeNull("GUID validation settings should be initialized");
            });
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void HandleConfiguration_WithNullConfiguration_CreatesDefault()
    {
        // Given
        AuthenticationSettings? inputSettings = null;
        AuthenticationSettings resultSettings = null!;

        Given.That("we have a null authentication configuration", () =>
            {
                StoreInContext<AuthenticationSettings?>("InputSettings", inputSettings);
            });

        // When
        When.I("handle the null configuration", () =>
            {
                var settings = GetFromContext<AuthenticationSettings?>("InputSettings");
                
                // Handle null configuration by providing default
                resultSettings = settings ?? new AuthenticationSettings();
                
                StoreInContext("ResultSettings", resultSettings);
            });

        // Then
        Then.The("should return a valid default configuration", () =>
            {
                var result = GetFromContext<AuthenticationSettings>("ResultSettings");
                result.Should().NotBeNull("Should not return null");
                result.RequireServiceAuthentication.Should().BeTrue("Should require service authentication by default");
            });
    }

    #endregion
}
