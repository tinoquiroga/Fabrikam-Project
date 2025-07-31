using System;
using Xunit;
using FluentAssertions;
using FabrikamContracts.DTOs;
using FabrikamTests.Helpers;
using Xunit.Abstractions;

namespace FabrikamTests.Unit.Models;

/// <summary>
/// Enhanced unit tests for AuthenticationMode enumeration using Given/When/Then pattern
/// Tests enum values, string conversions, and parsing functionality with clear test structure
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "AuthenticationMode")]
[Trait("Feature", "Authentication")]
public class EnhancedAuthenticationModeTests : GivenWhenThenTestBase
{
    public EnhancedAuthenticationModeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AuthenticationMode_ShouldHaveExpectedEnumValues()
    {
        // Given
        var expectedValues = new[]
        {
            AuthenticationMode.Disabled,
            AuthenticationMode.BearerToken,
            AuthenticationMode.EntraExternalId
        };

        // When & Then
        Given.That("we have the AuthenticationMode enumeration", () =>
            {
                // All expected values should be defined
                foreach (var expectedValue in expectedValues)
                {
                    Enum.IsDefined(typeof(AuthenticationMode), expectedValue).Should().BeTrue(
                        $"AuthenticationMode should contain value {expectedValue}");
                }
            })
            .And("we check the total number of enum values", () =>
            {
                var allValues = Enum.GetValues<AuthenticationMode>();
                allValues.Should().HaveCount(3, "AuthenticationMode should have exactly 3 values");
            });
    }

    [Theory]
    [InlineData(AuthenticationMode.Disabled, "Disabled")]
    [InlineData(AuthenticationMode.BearerToken, "BearerToken")]
    [InlineData(AuthenticationMode.EntraExternalId, "EntraExternalId")]
    [Trait("TestType", "Theory")]
    public void AuthenticationMode_ShouldConvertToCorrectString(AuthenticationMode mode, string expectedString)
    {
        // Given
        AuthenticationMode givenMode = default;
        string actualResult = string.Empty;

        Given.That($"we have AuthenticationMode.{mode}", () =>
            {
                givenMode = mode;
            });

        // When
        When.I("convert the enum to string", () =>
            {
                actualResult = givenMode.ToString();
            });

        // Then
        Then.The("result should match the expected string representation", () =>
            {
                actualResult.Should().Be(expectedString, 
                    $"AuthenticationMode.{mode} should convert to string '{expectedString}'");
            });
    }

    [Theory]
    [InlineData("Disabled", AuthenticationMode.Disabled)]
    [InlineData("BearerToken", AuthenticationMode.BearerToken)]
    [InlineData("EntraExternalId", AuthenticationMode.EntraExternalId)]
    [Trait("TestType", "Theory")]
    public void AuthenticationMode_ShouldParseFromValidString(string stringValue, AuthenticationMode expectedMode)
    {
        // Given
        string givenString = string.Empty;
        AuthenticationMode parsedResult = default;
        bool parseSuccess = false;

        Given.That($"we have the string '{stringValue}'", () =>
            {
                givenString = stringValue;
            });

        // When
        When.I("attempt to parse it to AuthenticationMode", () =>
            {
                parseSuccess = Enum.TryParse<AuthenticationMode>(givenString, out parsedResult);
            });

        // Then
        Then.The("parsing should succeed", () =>
            {
                parseSuccess.Should().BeTrue($"'{stringValue}' should parse successfully");
            })
            .And("the result should be the expected enum value", () =>
            {
                parsedResult.Should().Be(expectedMode, $"'{stringValue}' should parse to {expectedMode}");
            })
            .And("direct parsing should also work", () =>
            {
                var directParseResult = Enum.Parse<AuthenticationMode>(givenString);
                directParseResult.Should().Be(expectedMode);
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    [InlineData("disabled")] // Case sensitive
    [InlineData("DISABLED")] // Case sensitive
    [InlineData("None")]
    [InlineData("Token")]
    [Trait("TestType", "Theory")]
    public void AuthenticationMode_ShouldFailToParseInvalidString(string invalidString)
    {
        // Given
        string givenInvalidString = string.Empty;
        AuthenticationMode parsedResult = default;
        bool parseSuccess = false;

        Given.That($"we have an invalid string '{invalidString}'", () =>
            {
                givenInvalidString = invalidString;
            });

        // When
        When.I("attempt to parse it to AuthenticationMode", () =>
            {
                parseSuccess = Enum.TryParse<AuthenticationMode>(givenInvalidString, out parsedResult);
            });

        // Then
        Then.The("parsing should fail", () =>
            {
                parseSuccess.Should().BeFalse($"'{invalidString}' should not parse successfully");
            })
            .And("the result should be the default value", () =>
            {
                parsedResult.Should().Be(default(AuthenticationMode));
            });
    }

    [Fact]
    public void AuthenticationMode_ShouldSupportCaseInsensitiveParsing()
    {
        // Given
        var testCases = new[]
        {
            ("disabled", AuthenticationMode.Disabled),
            ("BEARERTOKEN", AuthenticationMode.BearerToken),
            ("entraexternalid", AuthenticationMode.EntraExternalId),
            ("Disabled", AuthenticationMode.Disabled),
            ("BearerToken", AuthenticationMode.BearerToken)
        };

        foreach (var (input, expected) in testCases)
        {
            // Given
            string givenInput = string.Empty;
            AuthenticationMode result = default;
            bool parseSuccess = false;

            Given.That($"we have the string '{input}' in various cases", () =>
                {
                    givenInput = input;
                });

            // When
            When.I("parse it with case-insensitive option", () =>
                {
                    parseSuccess = Enum.TryParse<AuthenticationMode>(givenInput, ignoreCase: true, out result);
                });

            // Then
            Then.The("parsing should succeed regardless of case", () =>
                {
                    parseSuccess.Should().BeTrue($"'{input}' should parse successfully with ignoreCase=true");
                })
                .And("the result should be correct", () =>
                {
                    result.Should().Be(expected, $"'{input}' should parse to {expected} when ignoring case");
                });
        }
    }

    [Fact]
    public void AuthenticationMode_ShouldHaveConsistentBehaviorWithTestDataBuilders()
    {
        // Given
        AuthenticationSettings disabledSettings = null!;
        AuthenticationSettings bearerSettings = null!;
        AuthenticationSettings entraSettings = null!;

        Given.That("we create authentication settings using test data builders", () =>
            {
                disabledSettings = TestDataBuilders.Given.AuthenticationSettings()
                    .AsDisabled()
                    .Build();

                bearerSettings = TestDataBuilders.Given.AuthenticationSettings()
                    .AsBearerToken()
                    .Build();

                entraSettings = TestDataBuilders.Given.AuthenticationSettings()
                    .AsEntraExternalId()
                    .Build();
            });

        // When & Then
        Then.The("authentication modes should be set correctly", () =>
            {
                disabledSettings.Mode.Should().Be(AuthenticationMode.Disabled);
                bearerSettings.Mode.Should().Be(AuthenticationMode.BearerToken);
                entraSettings.Mode.Should().Be(AuthenticationMode.EntraExternalId);
            })
            .And("they should convert to strings properly", () =>
            {
                disabledSettings.Mode.ToString().Should().Be("Disabled");
                bearerSettings.Mode.ToString().Should().Be("BearerToken");
                entraSettings.Mode.ToString().Should().Be("EntraExternalId");
            });
    }
}
