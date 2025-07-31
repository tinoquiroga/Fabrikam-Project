using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using FabrikamApi.Services.Authentication;
using FabrikamApi.Models.Authentication;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Unit.Services.Authentication;

public class EntraExternalIdAuthenticationServiceTests
{
    private readonly Mock<IAuthenticationService> _mockBaseAuthService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EntraExternalIdAuthenticationService>> _mockLogger;
    private readonly EntraExternalIdAuthenticationService _service;

    public EntraExternalIdAuthenticationServiceTests()
    {
        _mockBaseAuthService = new Mock<IAuthenticationService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EntraExternalIdAuthenticationService>>();

        // Setup configuration
        var entraSection = new Mock<IConfigurationSection>();
        entraSection.Setup(x => x["TenantId"]).Returns("test-tenant");
        entraSection.Setup(x => x["ClientId"]).Returns("test-client-id");
        entraSection.Setup(x => x["ClientSecret"]).Returns("test-secret");
        entraSection.Setup(x => x["Authority"]).Returns("https://test-tenant.b2clogin.com/test-tenant.onmicrosoft.com/v2.0");

        _mockConfiguration.Setup(x => x.GetSection("Authentication:EntraExternalId"))
            .Returns(entraSection.Object);

        _service = new EntraExternalIdAuthenticationService(
            _mockBaseAuthService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldInitialize()
    {
        // Arrange & Act - done in constructor

        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public void Constructor_WithMissingConfiguration_ShouldThrowException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        var emptySection = new Mock<IConfigurationSection>();
        mockConfig.Setup(x => x.GetSection("Authentication:EntraExternalId"))
            .Returns(emptySection.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new EntraExternalIdAuthenticationService(
            _mockBaseAuthService.Object,
            mockConfig.Object,
            _mockLogger.Object));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password",
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.LoginAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "refresh-token"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "old-password",
            NewPassword = "new-password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.ChangePasswordAsync("user-id", request));
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.RequestPasswordResetAsync(request));
    }

    [Fact]
    public async Task ConfirmPasswordResetAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ConfirmPasswordResetRequest
        {
            Token = "reset-token",
            NewPassword = "new-password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.ConfirmPasswordResetAsync(request));
    }

    [Fact]
    public async Task ValidateOAuthTokenAsync_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var accessToken = "valid-oauth-token";

        // Act
        var result = await _service.ValidateOAuthTokenAsync(accessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("oauth-validated", result.Id);
        Assert.Equal("oauth-user@example.com", result.Email);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task ValidateOAuthTokenAsync_WithEmptyToken_ShouldReturnNull()
    {
        // Arrange
        var accessToken = "";

        // Act
        var result = await _service.ValidateOAuthTokenAsync(accessToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithAdminRole_ShouldReturnAdminRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("roles", "administrator"),
            new("email", "admin@fabrikam.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Admin", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithSalesRole_ShouldReturnSalesRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("roles", "sales"),
            new("email", "sales@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Sales", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithSupportRole_ShouldReturnCustomerServiceRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("roles", "support"),
            new("email", "support@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("CustomerService", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithGroupClaim_ShouldMapGroupToRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("groups", "admin-group"),
            new("email", "user@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Admin", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithFabrikamEmail_ShouldReturnAdminRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("email", "admin@fabrikam.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Admin", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithSalesEmail_ShouldReturnSalesRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("email", "sales.person@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Sales", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithCustomExtensionRole_ShouldReturnCustomRole()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("extension_fabrikam_role", "CustomRole"),
            new("email", "user@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("CustomRole", result);
    }

    [Fact]
    public async Task MapOAuthClaimsToRolesAsync_WithNoClaims_ShouldReturnUserRole()
    {
        // Arrange
        var claims = new List<Claim>();

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Single(result);
        Assert.Contains("User", result);
    }

    [Fact]
    public async Task CreateOrUpdateUserFromOAuthAsync_WithValidClaims_ShouldCreateUserInfo()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("oid", "12345678-1234-1234-1234-123456789012"),
            new("email", "test@example.com"),
            new("given_name", "Test"),
            new("family_name", "User"),
            new("name", "Test User"),
            new("roles", "admin")
        };

        // Act
        var result = await _service.CreateOrUpdateUserFromOAuthAsync(claims);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("Test User", result.DisplayName);
        Assert.Contains("Admin", result.Roles);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateOrUpdateUserFromOAuthAsync_WithMissingObjectId_ShouldThrowException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("email", "test@example.com"),
            new("given_name", "Test")
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateOrUpdateUserFromOAuthAsync(claims));
    }

    [Fact]
    public async Task CreateOrUpdateUserFromOAuthAsync_WithMissingEmail_ShouldThrowException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("oid", "12345678-1234-1234-1234-123456789012"),
            new("given_name", "Test")
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateOrUpdateUserFromOAuthAsync(claims));
    }

    [Fact]
    public async Task CreateOrUpdateUserFromOAuthAsync_WithMinimalClaims_ShouldCreateUserWithDefaults()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("oid", "12345678-1234-1234-1234-123456789012"),
            new("email", "test@example.com")
        };

        // Act
        var result = await _service.CreateOrUpdateUserFromOAuthAsync(claims);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("OAuth", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task GetUserInfoAsync_ShouldDelegateToBaseService()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedUserInfo = new UserInfo
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        _mockBaseAuthService.Setup(x => x.GetUserInfoAsync(userId))
            .ReturnsAsync(expectedUserInfo);

        // Act
        var result = await _service.GetUserInfoAsync(userId);

        // Assert
        Assert.Equal(expectedUserInfo, result);
        _mockBaseAuthService.Verify(x => x.GetUserInfoAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserInfoByEmailAsync_ShouldDelegateToBaseService()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUserInfo = new UserInfo
        {
            Id = "test-user-id",
            Email = email,
            FirstName = "Test",
            LastName = "User"
        };

        _mockBaseAuthService.Setup(x => x.GetUserInfoByEmailAsync(email))
            .ReturnsAsync(expectedUserInfo);

        // Act
        var result = await _service.GetUserInfoByEmailAsync(email);

        // Assert
        Assert.Equal(expectedUserInfo, result);
        _mockBaseAuthService.Verify(x => x.GetUserInfoByEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData("administrator", "Admin")]
    [InlineData("admin", "Admin")]
    [InlineData("global_admin", "Admin")]
    [InlineData("sales", "Sales")]
    [InlineData("sales_manager", "Sales")]
    [InlineData("sales_rep", "Sales")]
    [InlineData("support", "CustomerService")]
    [InlineData("customer_service", "CustomerService")]
    [InlineData("helpdesk", "CustomerService")]
    [InlineData("user", "User")]
    [InlineData("member", "User")]
    [InlineData("unknown_role", "User")]
    public async Task MapOAuthClaimsToRolesAsync_WithVariousRoles_ShouldMapCorrectly(string inputRole, string expectedRole)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("roles", inputRole),
            new("email", "test@example.com")
        };

        // Act
        var result = await _service.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains(expectedRole, result);
    }
}
