using System;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs; // Use consolidated authentication models

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Unit tests for AuthenticationMode enumeration
/// Tests enum values, string conversions, and parsing functionality
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Component", "AuthenticationMode")]
public class AuthenticationModeTests
{
    [Fact]
    public void AuthenticationMode_Enum_HasExpectedValues()
    {
        // Assert - Verify all expected enum values exist
        var expectedValues = new[]
        {
            AuthenticationMode.Disabled,
            AuthenticationMode.BearerToken,
            AuthenticationMode.EntraExternalId
        };

        foreach (var expectedValue in expectedValues)
        {
            Enum.IsDefined(typeof(AuthenticationMode), expectedValue).Should().BeTrue(
                $"AuthenticationMode should contain value {expectedValue}");
        }

        // Verify enum has exactly the expected number of values
        var allValues = Enum.GetValues<AuthenticationMode>();
        allValues.Should().HaveCount(3, "AuthenticationMode should have exactly 3 values");
    }

    [Fact]
    public void AuthenticationMode_ToString_ReturnsCorrectStrings()
    {
        // Test string representation of each enum value
        var testCases = new[]
        {
            (AuthenticationMode.Disabled, "Disabled"),
            (AuthenticationMode.BearerToken, "BearerToken"),
            (AuthenticationMode.EntraExternalId, "EntraExternalId")
        };

        foreach (var (mode, expectedString) in testCases)
        {
            // Act
            var result = mode.ToString();

            // Assert
            result.Should().Be(expectedString, $"AuthenticationMode.{mode} should convert to string '{expectedString}'");
        }
    }

    [Fact]
    public void AuthenticationMode_Parsing_FromString_WorksCorrectly()
    {
        // Test parsing valid string values
        var testCases = new[]
        {
            ("Disabled", AuthenticationMode.Disabled),
            ("BearerToken", AuthenticationMode.BearerToken),
            ("EntraExternalId", AuthenticationMode.EntraExternalId)
        };

        foreach (var (stringValue, expectedMode) in testCases)
        {
            // Act
            var parseSuccess = Enum.TryParse<AuthenticationMode>(stringValue, out var result);
            
            // Assert
            parseSuccess.Should().BeTrue($"'{stringValue}' should parse successfully");
            result.Should().Be(expectedMode, $"'{stringValue}' should parse to {expectedMode}");

            // Also test Enum.Parse (throws on failure)
            var directParseResult = Enum.Parse<AuthenticationMode>(stringValue);
            directParseResult.Should().Be(expectedMode);
        }
    }

    [Fact]
    public void AuthenticationMode_CaseInsensitiveParsing_WorksCorrectly()
    {
        // Test case-insensitive parsing
        var testCases = new[]
        {
            ("disabled", AuthenticationMode.Disabled),
            ("DISABLED", AuthenticationMode.Disabled),
            ("bearertoken", AuthenticationMode.BearerToken),
            ("BEARERTOKEN", AuthenticationMode.BearerToken),
            ("entraexternalid", AuthenticationMode.EntraExternalId),
            ("ENTRAEXTERNALID", AuthenticationMode.EntraExternalId),
            ("entrAexternAlid", AuthenticationMode.EntraExternalId) // Mixed case
        };

        foreach (var (stringValue, expectedMode) in testCases)
        {
            // Act
            var parseSuccess = Enum.TryParse<AuthenticationMode>(stringValue, ignoreCase: true, out var result);
            
            // Assert
            parseSuccess.Should().BeTrue($"'{stringValue}' should parse successfully (case-insensitive)");
            result.Should().Be(expectedMode, $"'{stringValue}' should parse to {expectedMode}");
        }
    }

    [Fact]
    public void AuthenticationMode_InvalidValues_FailToParse()
    {
        // Test invalid string values
        var invalidValues = new[]
        {
            "",
            "   ",
            "Invalid",
            "OAuth",
            "JWT",
            "Windows",
            "BasicAuth",
            "DisabledMode",  // Close but not exact
            "Bearer",       // Partial match
            "Entra"         // Partial match
        };

        foreach (var invalidValue in invalidValues)
        {
            // Act
            var parseSuccess = Enum.TryParse<AuthenticationMode>(invalidValue, out var result);
            
            // Assert
            parseSuccess.Should().BeFalse($"'{invalidValue}' should not parse successfully");
            result.Should().Be(default(AuthenticationMode), "Failed parse should return default value");

            // Test that Enum.Parse throws for invalid values
            if (!string.IsNullOrWhiteSpace(invalidValue))
            {
                Action parseAction = () => Enum.Parse<AuthenticationMode>(invalidValue);
                parseAction.Should().Throw<ArgumentException>($"'{invalidValue}' should throw ArgumentException");
            }
        }
    }

    [Fact]
    public void AuthenticationMode_Numeric_ValuesAreStable()
    {
        // Verify that enum numeric values are stable (important for serialization/storage)
        ((int)AuthenticationMode.Disabled).Should().Be(0, "Disabled should have numeric value 0");
        ((int)AuthenticationMode.BearerToken).Should().Be(1, "BearerToken should have numeric value 1");
        ((int)AuthenticationMode.EntraExternalId).Should().Be(2, "EntraExternalId should have numeric value 2");
    }

    [Fact]
    public void AuthenticationMode_DefaultValue_IsDisabled()
    {
        // Act
        var defaultValue = default(AuthenticationMode);
        
        // Assert
        defaultValue.Should().Be(AuthenticationMode.Disabled, "Default AuthenticationMode should be Disabled");
    }

    [Fact]
    public void AuthenticationMode_CanBeUsedInSwitch_AllCasesCovered()
    {
        // This test ensures all enum values can be used in switch statements
        // and helps catch when new values are added without updating switch statements
        
        foreach (AuthenticationMode mode in Enum.GetValues<AuthenticationMode>())
        {
            // Act - Use switch expression to process each mode
            var description = mode switch
            {
                AuthenticationMode.Disabled => "GUID tracking with service JWT authentication",
                AuthenticationMode.BearerToken => "JWT Bearer token authentication",
                AuthenticationMode.EntraExternalId => "Entra External ID OAuth 2.0 authentication",
                _ => throw new InvalidOperationException($"Unhandled AuthenticationMode: {mode}")
            };

            // Assert
            description.Should().NotBeNullOrEmpty($"All AuthenticationMode values should have descriptions");
        }
    }

    [Fact]
    public void AuthenticationMode_SecurityImplications_AreCorrect()
    {
        // Test the security implications of each mode based on documentation
        var securityTests = new[]
        {
            (AuthenticationMode.Disabled, false, true, "Disabled mode: no user auth, service auth enabled"),
            (AuthenticationMode.BearerToken, true, true, "BearerToken mode: user auth enabled, service auth enabled"),
            (AuthenticationMode.EntraExternalId, true, true, "EntraExternalId mode: user auth enabled, service auth enabled")
        };

        foreach (var (mode, expectedUserAuth, expectedServiceAuth, description) in securityTests)
        {
            // Act - Simulate the authentication requirements logic
            var requiresUserAuth = mode != AuthenticationMode.Disabled;
            var requiresServiceAuth = true; // Always true per security-by-default architecture

            // Assert
            requiresUserAuth.Should().Be(expectedUserAuth, description);
            requiresServiceAuth.Should().Be(expectedServiceAuth, description);
        }
    }

    [Fact]
    public void AuthenticationMode_ConfigurationCompatibility_WithStrings()
    {
        // Test that enum can be used with configuration systems that work with strings
        // This is important for appsettings.json and environment variable configuration
        
        foreach (AuthenticationMode mode in Enum.GetValues<AuthenticationMode>())
        {
            // Act - Simulate configuration round-trip (enum -> string -> enum)
            var configString = mode.ToString();
            var parsedBack = Enum.Parse<AuthenticationMode>(configString);

            // Assert
            parsedBack.Should().Be(mode, $"AuthenticationMode should survive configuration round-trip");
        }
    }
}
