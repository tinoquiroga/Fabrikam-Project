using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;
using FabrikamMcp.Services;
using FabrikamContracts.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace FabrikamTests.Unit.Services;

[Trait("Category", "Authentication")]
[Trait("Component", "ServiceJwtService")]
public class ServiceJwtServiceTests
{
    private readonly Mock<IUserRegistrationService> _mockUserRegistrationService;
    private readonly Mock<ILogger<ServiceJwtService>> _mockLogger;
    private readonly ServiceJwtService _jwtService;
    private readonly ServiceJwtSettings _serviceJwtSettings;
    private readonly Guid _validUserGuid = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
    private readonly Guid _invalidUserGuid = Guid.Parse("987fcdeb-51a2-43d7-b123-456789abcdef");

    public ServiceJwtServiceTests()
    {
        _mockUserRegistrationService = new Mock<IUserRegistrationService>();
        _mockLogger = new Mock<ILogger<ServiceJwtService>>();

        // Configure test settings
        _serviceJwtSettings = new ServiceJwtSettings
        {
            ServiceName = "FabrikamMcp",
            SecretKey = "ThisIsATestSecretKeyForJwtTesting123456789",
            Issuer = "FabrikamMcp",
            Audience = "FabrikamServices",
            ExpirationMinutes = 60,
            AllowedModes = new[] { AuthenticationMode.Disabled, AuthenticationMode.BearerToken }
        };

        var authSettings = new AuthenticationSettings
        {
            ServiceJwt = _serviceJwtSettings
        };

        var mockAuthSettings = new Mock<IOptions<AuthenticationSettings>>();
        mockAuthSettings.Setup(x => x.Value).Returns(authSettings);

        // Setup default user registration service behavior
        var testUser = new DisabledModeUser
        {
            Id = _validUserGuid,
            AuditGuid = _validUserGuid,
            Name = "Test User",
            Email = "test@fabrikam.com",
            AuthenticationMode = AuthenticationMode.Disabled,
            RegistrationDate = DateTime.UtcNow
        };

        _mockUserRegistrationService
            .Setup(x => x.ValidateUserGuidAsync(_validUserGuid))
            .ReturnsAsync(true);

        _mockUserRegistrationService
            .Setup(x => x.GetUserByGuidAsync(_validUserGuid))
            .ReturnsAsync(testUser);

        _mockUserRegistrationService
            .Setup(x => x.ValidateUserGuidAsync(_invalidUserGuid))
            .ReturnsAsync(false);

        _mockUserRegistrationService
            .Setup(x => x.GetUserByGuidAsync(_invalidUserGuid))
            .ReturnsAsync((BaseUserRegistration?)null);

        _jwtService = new ServiceJwtService(mockAuthSettings.Object, _mockUserRegistrationService.Object, _mockLogger.Object);
    }

    #region GenerateServiceTokenAsync Tests

    [Fact]
    public async Task GenerateServiceTokenAsync_WithValidGuid_ReturnsValidJwt()
    {
        // Arrange
        var mode = AuthenticationMode.Disabled;
        var sessionId = "test-session-123";

        // Act
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, mode, sessionId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Verify token structure
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(_serviceJwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_serviceJwtSettings.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == _validUserGuid.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "fabrikam_auth_mode" && c.Value == mode.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "fabrikam_service_jwt" && c.Value == "true");
        jwtToken.Claims.Should().Contain(c => c.Type == "fabrikam_session_id" && c.Value == sessionId);
        
        // Verify expiration
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_serviceJwtSettings.ExpirationMinutes), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GenerateServiceTokenAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var mode = AuthenticationMode.Disabled;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _jwtService.GenerateServiceTokenAsync(emptyGuid, mode));
        
        exception.ParamName.Should().Be("userGuid");
        exception.Message.Should().Contain("Valid user GUID is required");
    }

    [Fact]
    public async Task GenerateServiceTokenAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var mode = AuthenticationMode.Disabled;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _jwtService.GenerateServiceTokenAsync(_invalidUserGuid, mode));
        
        exception.Message.Should().Contain($"User GUID {_invalidUserGuid} not found in registry");
    }

    [Fact]
    public async Task GenerateServiceTokenAsync_WithoutSessionId_GeneratesTokenWithoutSessionClaim()
    {
        // Arrange
        var mode = AuthenticationMode.BearerToken;

        // Act
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, mode);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().NotContain(c => c.Type == "fabrikam_session_id");
        jwtToken.Claims.Should().Contain(c => c.Type == "fabrikam_auth_mode" && c.Value == mode.ToString());
    }

    [Fact]
    public async Task GenerateServiceTokenAsync_LogsDebugMessage()
    {
        // Arrange
        var mode = AuthenticationMode.Disabled;

        // Act
        await _jwtService.GenerateServiceTokenAsync(_validUserGuid, mode);

        // Assert
        VerifyLogContains(LogLevel.Debug, $"Generated service JWT for user {_validUserGuid} in {mode} mode");
    }

    #endregion

    #region ValidateServiceTokenAsync Tests

    [Fact]
    public async Task ValidateServiceTokenAsync_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, AuthenticationMode.Disabled);

        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(_validUserGuid.ToString());
        principal.FindFirst("fabrikam_service_jwt")?.Value.Should().Be("true");
        principal.FindFirst("fabrikam_auth_mode")?.Value.Should().Be(AuthenticationMode.Disabled.ToString());
    }

    [Fact]
    public async Task ValidateServiceTokenAsync_WithNullToken_ReturnsNull()
    {
        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync(null!);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateServiceTokenAsync_WithEmptyToken_ReturnsNull()
    {
        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync("");

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateServiceTokenAsync_WithWhitespaceToken_ReturnsNull()
    {
        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync("   ");

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateServiceTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync(invalidToken);

        // Assert
        principal.Should().BeNull();
        VerifyLogContains(LogLevel.Error, "Unexpected error validating service JWT");
    }

    [Fact]
    public async Task ValidateServiceTokenAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange - Generate a token while user exists
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, AuthenticationMode.Disabled);

        // Now simulate the user no longer existing by overriding the mock
        _mockUserRegistrationService
            .Setup(x => x.ValidateUserGuidAsync(_validUserGuid))
            .ReturnsAsync(false);

        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync(token);

        // Assert
        principal.Should().BeNull();
        VerifyLogContains(LogLevel.Warning, "no longer exists in registry");
    }

    #endregion

    #region ExtractUserGuidFromTokenAsync Tests

    [Fact]
    public async Task ExtractUserGuidFromTokenAsync_WithValidToken_ReturnsUserGuid()
    {
        // Arrange
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, AuthenticationMode.Disabled);

        // Act
        var extractedGuid = await _jwtService.ExtractUserGuidFromTokenAsync(token);

        // Assert
        extractedGuid.Should().Be(_validUserGuid);
    }

    [Fact]
    public async Task ExtractUserGuidFromTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var extractedGuid = await _jwtService.ExtractUserGuidFromTokenAsync(invalidToken);

        // Assert
        extractedGuid.Should().BeNull();
    }

    [Fact]
    public async Task ExtractUserGuidFromTokenAsync_WithNullToken_ReturnsNull()
    {
        // Act
        var extractedGuid = await _jwtService.ExtractUserGuidFromTokenAsync(null!);

        // Assert
        extractedGuid.Should().BeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ValidateServiceTokenAsync_WithUnexpectedError_ReturnsNullAndLogsError()
    {
        // Arrange - Generate a token while user exists
        var token = await _jwtService.GenerateServiceTokenAsync(_validUserGuid, AuthenticationMode.Disabled);

        // Now setup user registration service to throw exception during validation
        _mockUserRegistrationService
            .Setup(x => x.ValidateUserGuidAsync(_validUserGuid))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var principal = await _jwtService.ValidateServiceTokenAsync(token);

        // Assert
        principal.Should().BeNull();
        VerifyLogContains(LogLevel.Error, "Unexpected error validating service JWT");
    }

    [Fact]
    public async Task GenerateServiceTokenAsync_WithUserRegistrationServiceException_ThrowsException()
    {
        // Arrange
        var testGuid = Guid.NewGuid();
        _mockUserRegistrationService
            .Setup(x => x.ValidateUserGuidAsync(testGuid))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _jwtService.GenerateServiceTokenAsync(testGuid, AuthenticationMode.Disabled));
        
        exception.Message.Should().Be("Database connection failed");
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
