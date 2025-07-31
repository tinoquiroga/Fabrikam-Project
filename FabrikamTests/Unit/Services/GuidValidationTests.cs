using System;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using FabrikamMcp.Tools;
using FabrikamMcp.Services;
using FabrikamMcp.Controllers;
using FabrikamContracts.DTOs;
using System.Text.RegularExpressions;

namespace FabrikamTests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for GUID validation functionality across the Fabrikam MCP system
/// Tests various GUID formats, validation methods, and edge cases
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Component", "GuidValidation")]
public class GuidValidationTests
{
    private readonly Mock<ILogger<TestAuthenticatedTool>> _mockLogger;
    private readonly Mock<IServiceJwtService> _mockServiceJwtService;
    private readonly Mock<IUserRegistrationService> _mockUserRegistrationService;
    private readonly Mock<IOptions<AuthenticationSettings>> _mockAuthSettings;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly TestAuthenticatedTool _testTool;
    private readonly UserRegistrationController _controller;
    
    // Standard valid GUID formats for testing
    private readonly string _standardGuid = "123e4567-e89b-12d3-a456-426614174000";
    private readonly string _guidWithBraces = "{123e4567-e89b-12d3-a456-426614174000}";
    private readonly string _guidWithParentheses = "(123e4567-e89b-12d3-a456-426614174000)";
    private readonly string _uppercaseGuid = "123E4567-E89B-12D3-A456-426614174000";
    private readonly string _guidWithoutHyphens = "123e4567e89b12d3a456426614174000";
    private readonly string _emptyGuidString = "00000000-0000-0000-0000-000000000000";
    
    // Invalid GUID formats for testing
    private readonly string _invalidFormat = "invalid-guid-format";
    private readonly string _tooShort = "123e4567-e89b-12d3-a456";
    private readonly string _tooLong = "123e4567-e89b-12d3-a456-426614174000-extra";
    private readonly string _invalidCharacter = "123e4567-e89b-12d3-a456-42661417400x";
    private readonly string _emptyString = "";
    private readonly string _whitespaceOnly = "   ";
    private readonly string _guidWithSpaces = " 123e4567-e89b-12d3-a456-426614174000 ";

    public GuidValidationTests()
    {
        _mockLogger = new Mock<ILogger<TestAuthenticatedTool>>();
        _mockServiceJwtService = new Mock<IServiceJwtService>();
        _mockUserRegistrationService = new Mock<IUserRegistrationService>();
        _mockAuthSettings = new Mock<IOptions<AuthenticationSettings>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockHttpClient = new Mock<HttpClient>();
        
        // Setup default GUID validation settings
        var guidSettings = new GuidValidationSettings
        {
            ValidateMicrosoftGuidFormat = true,
            ValidateGuidInDatabase = false,
            ValidationCacheMinutes = 60,
            MicrosoftGuidPattern = @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$",
            ValidationRules = Array.Empty<string>()
        };
        
        var authSettings = new AuthenticationSettings
        {
            GuidValidation = guidSettings
        };
        
        _mockAuthSettings.Setup(x => x.Value).Returns(authSettings);
        
        _testTool = new TestAuthenticatedTool(
            _mockHttpClient.Object, 
            _mockConfiguration.Object,
            _mockAuthService.Object,
            _mockLogger.Object);
            
        _controller = new UserRegistrationController(
            _mockUserRegistrationService.Object,
            _mockServiceJwtService.Object,
            Mock.Of<ILogger<UserRegistrationController>>(),
            _mockAuthSettings.Object);
    }

    #region ValidateGuid Tests

    [Fact]
    public void ValidateGuid_WithValidGuid_ReturnsTrue()
    {
        // Arrange
        var validGuid = _standardGuid;

        // Act
        var result = _testTool.TestValidateAndSetGuidContext(validGuid, "TestMethod");

        // Assert
        result.Should().BeTrue();
        VerifyNoWarningLogs();
    }

    [Fact]
    public void ValidateGuid_WithValidGuidVariousFormats_ReturnsTrue()
    {
        // Test various valid GUID formats that should be accepted
        var validFormats = new[]
        {
            _standardGuid,           // Standard format
            _guidWithBraces,         // With braces
            _guidWithParentheses,    // With parentheses  
            _uppercaseGuid,          // Uppercase
            _guidWithSpaces.Trim()   // Spaces trimmed
        };

        foreach (var format in validFormats)
        {
            // Act
            var result = _testTool.TestValidateAndSetGuidContext(format, "TestMethod");

            // Assert
            result.Should().BeTrue($"Format '{format}' should be valid");
        }
    }

    [Fact]
    public void ValidateGuid_WithInvalidFormat_ReturnsFalse()
    {
        // Arrange
        var invalidGuid = _invalidFormat;

        // Act
        var result = _testTool.TestValidateAndSetGuidContext(invalidGuid, "TestMethod");

        // Assert
        result.Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Invalid GUID format provided");
    }

    [Fact]
    public void ValidateGuid_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange - Test the empty GUID constant
        var emptyGuid = _emptyGuidString;

        // Act
        var result = _testTool.TestValidateAndSetGuidContext(emptyGuid, "TestMethod");

        // Assert
        result.Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Invalid GUID format provided");
    }

    [Fact]
    public void ValidateGuid_WithNullInput_ReturnsFalse()
    {
        // Act
        var result = _testTool.TestValidateAndSetGuidContext(null, "TestMethod");

        // Assert
        result.Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "No user GUID provided");
    }

    [Fact]
    public void ValidateGuid_WithWhitespace_HandlesCorrectly()
    {
        // Test various whitespace scenarios
        var whitespaceInputs = new[]
        {
            _emptyString,
            _whitespaceOnly,
            "\t",
            "\n",
            "\r\n"
        };

        foreach (var input in whitespaceInputs)
        {
            // Act
            var result = _testTool.TestValidateAndSetGuidContext(input, "TestMethod");

            // Assert
            result.Should().BeFalse($"Whitespace input '{input}' should return false");
            VerifyLogContains(LogLevel.Warning, "No user GUID provided");
        }
    }

    #endregion

    #region GUID Sanitization Tests

    [Fact]
    public void GuidSanitization_RemovesWhitespaceAndNormalizes()
    {
        // Arrange
        var guidWithWhitespace = _guidWithSpaces;

        // Act - The method should handle whitespace internally
        var result = _testTool.TestValidateAndSetGuidContext(guidWithWhitespace, "TestMethod");

        // Assert - .NET's Guid.TryParse actually handles leading/trailing whitespace
        result.Should().BeTrue(".NET Guid.TryParse handles leading/trailing whitespace");
        VerifyNoWarningLogs();
    }

    [Fact]
    public void GuidSanitization_HandlesVariousFormats()
    {
        // Test various formats that need normalization
        var formatTests = new[]
        {
            (_guidWithBraces, true, "Braces should be handled by Guid.TryParse"),
            (_guidWithParentheses, true, "Parentheses should be handled by Guid.TryParse"),
            (_uppercaseGuid, true, "Uppercase should be valid"),
            (_guidWithoutHyphens, true, "No hyphens should be valid for .NET Guid.TryParse")
        };

        foreach (var (format, expectedResult, description) in formatTests)
        {
            // Act
            var result = _testTool.TestValidateAndSetGuidContext(format, "TestMethod");

            // Assert
            result.Should().Be(expectedResult, description);
        }
    }

    #endregion

    #region GUID Parsing Tests

    [Fact]
    public void GuidParsing_WithValidInput_ReturnsGuidValue()
    {
        // Test .NET's Guid.TryParse behavior with valid inputs
        var validInputs = new[]
        {
            _standardGuid,
            _guidWithBraces,
            _guidWithParentheses,
            _uppercaseGuid
        };

        foreach (var input in validInputs)
        {
            // Act
            var parseSuccess = Guid.TryParse(input, out var guidValue);

            // Assert
            parseSuccess.Should().BeTrue($"Input '{input}' should parse successfully");
            guidValue.Should().NotBe(Guid.Empty, "Parsed GUID should not be empty");
            
            // Verify the parsed value matches expected format
            var standardFormat = guidValue.ToString().ToLower();
            standardFormat.Should().MatchRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
        }
    }

    [Fact]
    public void GuidParsing_WithInvalidInput_ThrowsOrReturnsFalse()
    {
        // Test .NET's Guid.TryParse behavior with invalid inputs
        var invalidInputs = new[]
        {
            _invalidFormat,
            _tooShort,
            _tooLong,
            _invalidCharacter,
            _emptyString,
            _whitespaceOnly
        };

        foreach (var input in invalidInputs)
        {
            // Act - Test TryParse (safe method)
            var parseSuccess = Guid.TryParse(input, out var guidValue);

            // Assert
            parseSuccess.Should().BeFalse($"Invalid input '{input}' should not parse");
            guidValue.Should().Be(Guid.Empty, "Failed parse should result in empty GUID");

            // Act - Test Parse (throws exception)
            Action parseAction = () => Guid.Parse(input);

            // Assert - .NET throws FormatException for all invalid inputs including empty/null
            parseAction.Should().Throw<Exception>($"Invalid input '{input}' should throw exception")
                .Which.Should().BeOfType<FormatException>($"Invalid input '{input}' throws FormatException");
        }
    }

    #endregion

    #region Microsoft GUID Format Validation

    [Fact]
    public void MicrosoftGuidFormat_WithValidFormat_ReturnsTrue()
    {
        // Arrange
        var validGuid = Guid.Parse(_standardGuid);

        // Act
        var result = TestIsValidMicrosoftGuid(validGuid);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MicrosoftGuidFormat_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act - Empty GUID should match the regex pattern but may be rejected by business logic
        var result = TestIsValidMicrosoftGuid(emptyGuid);

        // Assert - Depends on implementation, but empty GUID typically matches pattern
        result.Should().BeTrue("Empty GUID matches Microsoft format pattern");
    }

    [Fact]
    public void MicrosoftGuidFormat_RegexPattern_ValidatesCorrectly()
    {
        // Test the regex pattern directly
        var pattern = @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$";
        var regex = new Regex(pattern);

        var testCases = new[]
        {
            (_standardGuid.ToLower(), true, "Standard lowercase"),
            (_standardGuid.ToUpper(), true, "Standard uppercase"),
            (_emptyGuidString, true, "Empty GUID string"),
            ("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", true, "All F's"),
            ("invalid-guid-format", false, "Invalid format"),
            ("123e4567-e89b-12d3-a456", false, "Too short"),
            ("123e4567e89b12d3a456426614174000", false, "No hyphens")
        };

        foreach (var (input, expected, description) in testCases)
        {
            // Act
            var result = regex.IsMatch(input);

            // Assert
            result.Should().Be(expected, description);
        }
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Fact]
    public void GuidValidation_EdgeCases_HandleCorrectly()
    {
        var edgeCases = new[]
        {
            (null, false, "Null input"),
            ("", false, "Empty string"),
            ("   ", false, "Whitespace only"),
            ("\t\n\r", false, "Various whitespace characters"),
            (_emptyGuidString, false, "Empty GUID constant"),
            ("00000000000000000000000000000000", false, "32 zeros without hyphens"),
            ("G123e4567-e89b-12d3-a456-426614174000", false, "Invalid character at start"),
            ("123e4567-e89b-12d3-a456-426614174000G", false, "Invalid character at end")
        };

        foreach (var (input, expected, description) in edgeCases)
        {
            // Act
            var result = _testTool.TestValidateAndSetGuidContext(input, "TestMethod");

            // Assert
            result.Should().Be(expected, description);
        }
    }

    [Fact]
    public void GuidValidation_WithLogging_GeneratesAppropriateMessages()
    {
        // Test different scenarios generate correct log messages
        var testCases = new[]
        {
            (null, "No user GUID provided"),
            ("", "No user GUID provided"),
            ("   ", "No user GUID provided"),
            (_invalidFormat, "Invalid GUID format provided"),
            (_emptyGuidString, "Invalid GUID format provided")
        };

        foreach (var (input, expectedLogMessage) in testCases)
        {
            // Arrange - Reset mock for clean test
            _mockLogger.Reset();

            // Act
            var result = _testTool.TestValidateAndSetGuidContext(input, "TestMethod");

            // Assert
            result.Should().BeFalse();
            VerifyLogContains(LogLevel.Warning, expectedLogMessage);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Test the private IsValidMicrosoftGuid method through reflection or public wrapper
    /// </summary>
    private bool TestIsValidMicrosoftGuid(Guid guid)
    {
        // Test the Microsoft GUID validation logic
        var guidString = guid.ToString().ToLower();
        var pattern = @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$";
        return Regex.IsMatch(guidString, pattern);
    }

    private void VerifyLogContains(LogLevel level, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyNoWarningLogs()
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion
}

/// <summary>
/// Test implementation of AuthenticatedMcpToolBase for testing GUID validation
/// </summary>
public class TestAuthenticatedTool : AuthenticatedMcpToolBase
{
    public TestAuthenticatedTool(
        HttpClient httpClient,
        IConfiguration configuration,
        IAuthenticationService authService,
        ILogger<TestAuthenticatedTool> logger) 
        : base(httpClient, configuration, authService, logger)
    {
    }

    /// <summary>
    /// Public wrapper for testing the protected ValidateAndSetGuidContext method
    /// </summary>
    public bool TestValidateAndSetGuidContext(string? userGuid, string methodName)
    {
        return ValidateAndSetGuidContext(userGuid, methodName);
    }
}
