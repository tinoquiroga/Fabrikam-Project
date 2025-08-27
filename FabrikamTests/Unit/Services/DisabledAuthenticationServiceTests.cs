using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using FabrikamMcp.Services;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Unit.Services;

[Trait("Category", "Authentication")]
[Trait("Component", "DisabledAuthenticationService")]
public class DisabledAuthenticationServiceTests
{
    private readonly Mock<IServiceJwtService> _mockServiceJwtService;
    private readonly Mock<ILogger<DisabledAuthenticationService>> _mockLogger;
    private readonly DisabledAuthenticationService _authService;
    private readonly string _validGuid = "123e4567-e89b-12d3-a456-426614174000";
    private readonly string _invalidGuid = "invalid-guid-format";
    private readonly string _emptyGuid = "";
    private readonly string _validJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token";

    public DisabledAuthenticationServiceTests()
    {
        _mockServiceJwtService = new Mock<IServiceJwtService>();
        _mockLogger = new Mock<ILogger<DisabledAuthenticationService>>();
        _authService = new DisabledAuthenticationService(_mockServiceJwtService.Object, _mockLogger.Object);
    }

    #region SetUserGuidContext Tests

    [Fact]
    public void SetUserGuidContext_WithValidGuid_SetsContext()
    {
        // Act
        _authService.SetUserGuidContext(_validGuid);

        // Assert
        _authService.IsAuthenticated().Should().BeTrue();
        _authService.GetCurrentUserId().Should().Be($"disabled-user-{_validGuid}");
    }

    [Fact]
    public void SetUserGuidContext_WithInvalidGuid_LogsWarning()
    {
        // Act
        _authService.SetUserGuidContext(_invalidGuid);

        // Assert
        _authService.IsAuthenticated().Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Invalid GUID format provided");
    }

    [Fact]
    public void SetUserGuidContext_WithEmptyGuid_LogsWarning()
    {
        // Act
        _authService.SetUserGuidContext(_emptyGuid);

        // Assert
        _authService.IsAuthenticated().Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Attempted to set empty user GUID context");
    }

    [Fact]
    public void SetUserGuidContext_WithNullGuid_LogsWarning()
    {
        // Act
        _authService.SetUserGuidContext(null!);

        // Assert
        _authService.IsAuthenticated().Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Attempted to set empty user GUID context");
    }

    [Fact]
    public void SetUserGuidContext_WithWhitespaceGuid_LogsWarning()
    {
        // Act
        _authService.SetUserGuidContext("   ");

        // Assert
        _authService.IsAuthenticated().Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Attempted to set empty user GUID context");
    }

    #endregion

    #region GetCurrentJwtTokenAsync Tests

    [Fact]
    public async Task GetCurrentJwtTokenAsync_WithValidGuid_ReturnsJwt()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);
        _mockServiceJwtService
            .Setup(x => x.GenerateServiceTokenAsync(It.IsAny<Guid>(), AuthenticationMode.Disabled, null))
            .ReturnsAsync(_validJwtToken);

        // Act
        var result = await _authService.GetCurrentJwtTokenAsync();

        // Assert
        result.Should().Be(_validJwtToken);
        _mockServiceJwtService.Verify(x => x.GenerateServiceTokenAsync(
            Guid.Parse(_validGuid), 
            AuthenticationMode.Disabled, 
            null), Times.Once);
    }

    [Fact]
    public async Task GetCurrentJwtTokenAsync_WithoutGuid_ReturnsNull()
    {
        // Act
        var result = await _authService.GetCurrentJwtTokenAsync();

        // Assert
        result.Should().BeNull();
        VerifyLogContains(LogLevel.Warning, "No user GUID context set");
        _mockServiceJwtService.Verify(x => x.GenerateServiceTokenAsync(
            It.IsAny<Guid>(), It.IsAny<AuthenticationMode>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentJwtTokenAsync_WithInvalidGuid_ReturnsNull()
    {
        // Arrange
        _authService.SetUserGuidContext(_invalidGuid);

        // Act
        var result = await _authService.GetCurrentJwtTokenAsync();

        // Assert
        result.Should().BeNull();
        VerifyLogContains(LogLevel.Warning, "No user GUID context set");
    }

    [Fact]
    public async Task GetCurrentJwtTokenAsync_ServiceFailure_ReturnsNull()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);
        _mockServiceJwtService
            .Setup(x => x.GenerateServiceTokenAsync(It.IsAny<Guid>(), AuthenticationMode.Disabled, null))
            .ThrowsAsync(new InvalidOperationException("Service failure"));

        // Act
        var result = await _authService.GetCurrentJwtTokenAsync();

        // Assert
        result.Should().BeNull();
        VerifyLogContains(LogLevel.Error, "Failed to generate service JWT");
    }

    #endregion

    #region Authentication State Tests

    [Fact]
    public void IsAuthenticated_WithGuidSet_ReturnsTrue()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);

        // Act & Assert
        _authService.IsAuthenticated().Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithoutGuid_ReturnsFalse()
    {
        // Act & Assert
        _authService.IsAuthenticated().Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_AfterInvalidGuid_ReturnsFalse()
    {
        // Arrange
        _authService.SetUserGuidContext(_invalidGuid);

        // Act & Assert
        _authService.IsAuthenticated().Should().BeFalse();
    }

    #endregion

    #region User Identity Tests

    [Fact]
    public void GetCurrentUserId_WithGuid_ReturnsFormattedId()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);

        // Act
        var result = _authService.GetCurrentUserId();

        // Assert
        result.Should().Be($"disabled-user-{_validGuid}");
    }

    [Fact]
    public void GetCurrentUserId_WithoutGuid_ReturnsSystem()
    {
        // Act
        var result = _authService.GetCurrentUserId();

        // Assert
        result.Should().Be("system");
    }

    [Fact]
    public void GetCurrentUserRoles_ReturnsExpectedRoles()
    {
        // Act
        var roles = _authService.GetCurrentUserRoles();

        // Assert
        roles.Should().Contain("DisabledModeUser");
        roles.Should().Contain("ServiceUser");
        roles.Should().HaveCount(2);
    }

    [Fact]
    public void HasRole_AlwaysReturnsTrue()
    {
        // Act & Assert
        _authService.HasRole("Admin").Should().BeTrue();
        _authService.HasRole("User").Should().BeTrue();
        _authService.HasRole("NonExistentRole").Should().BeTrue();
        _authService.HasRole("").Should().BeTrue();
        _authService.HasRole(null!).Should().BeTrue();
    }

    #endregion

    #region Authentication Context Tests

    [Fact]
    public void CreateAuthenticationContext_WithGuid_ReturnsPopulatedContext()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);

        // Act
        var context = _authService.CreateAuthenticationContext();

        // Assert
        context.Should().NotBeNull();
        context.UserId.Should().Be($"disabled-user-{_validGuid}");
        context.UserName.Should().Contain("Disabled Mode User");
        context.UserName.Should().Contain(_validGuid);
        context.IsAuthenticated.Should().BeTrue();
        context.Roles.Should().Contain("DisabledModeUser");
        context.Roles.Should().Contain("ServiceUser");
    }

    [Fact]
    public void CreateAuthenticationContext_WithoutGuid_ReturnsSystemContext()
    {
        // Act
        var context = _authService.CreateAuthenticationContext();

        // Assert
        context.Should().NotBeNull();
        context.UserId.Should().Be("system");
        context.UserName.Should().Be("System User");
        context.IsAuthenticated.Should().BeFalse();
        context.Roles.Should().Contain("DisabledModeUser");
        context.Roles.Should().Contain("ServiceUser");
    }

    #endregion

    #region Synchronous Token Methods Tests

    [Fact]
    public void GetCurrentJwtToken_SyncWrapper_CallsAsyncMethod()
    {
        // Arrange
        _authService.SetUserGuidContext(_validGuid);
        _mockServiceJwtService
            .Setup(x => x.GenerateServiceTokenAsync(It.IsAny<Guid>(), AuthenticationMode.Disabled, null))
            .ReturnsAsync(_validJwtToken);

        // Act
        var result = _authService.GetCurrentJwtToken();

        // Assert
        result.Should().Be(_validJwtToken);
        _mockServiceJwtService.Verify(x => x.GenerateServiceTokenAsync(
            Guid.Parse(_validGuid), 
            AuthenticationMode.Disabled, 
            null), Times.Once);
    }

    [Fact]
    public void GetCurrentJwtToken_WithoutGuid_ReturnsNull()
    {
        // Act
        var result = _authService.GetCurrentJwtToken();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void SetUserGuidContext_WithEmptyGuidConstant_LogsWarning()
    {
        // Arrange
        var emptyGuidString = Guid.Empty.ToString();

        // Act
        _authService.SetUserGuidContext(emptyGuidString);

        // Assert
        _authService.IsAuthenticated().Should().BeFalse();
        VerifyLogContains(LogLevel.Warning, "Invalid GUID format provided");
    }

    [Fact]
    public void SetUserGuidContext_WithGuidInBraces_SetsContext()
    {
        // Arrange
        var guidWithBraces = $"{{{_validGuid}}}";

        // Act
        _authService.SetUserGuidContext(guidWithBraces);

        // Assert - Should succeed because Guid.TryParse accepts braces
        _authService.IsAuthenticated().Should().BeTrue();
        _authService.GetCurrentUserId().Should().Be($"disabled-user-{guidWithBraces}");
    }

    [Fact]
    public void SetUserGuidContext_WithUppercaseGuid_SetsContext()
    {
        // Arrange
        var uppercaseGuid = _validGuid.ToUpper();

        // Act
        _authService.SetUserGuidContext(uppercaseGuid);

        // Assert
        _authService.IsAuthenticated().Should().BeTrue();
        _authService.GetCurrentUserId().Should().Be($"disabled-user-{uppercaseGuid}");
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
