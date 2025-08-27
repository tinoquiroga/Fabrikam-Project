using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using FabrikamApi.Services.Authentication;
using FabrikamApi.Models.Authentication;
using FabrikamApi.Models;
using FabrikamApi.Data;
using FabrikamTests.Helpers;
using Xunit.Abstractions;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Unit.Services;

/// <summary>
/// Phase 3C: Unit tests for AuthenticationService (API layer)
/// Tests user registration, login, token management, password operations, and user info retrieval
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "AuthenticationService")]
[Trait("Feature", "Authentication")]
[Trait("Priority", "High")]
[Trait("Phase", "3C")]
public class AuthenticationServiceTests : GivenWhenThenTestBase, IDisposable
{
    private readonly Mock<UserManager<FabrikamUser>> _mockUserManager;
    private readonly Mock<SignInManager<FabrikamUser>> _mockSignInManager;
    private readonly Mock<RoleManager<FabrikamRole>> _mockRoleManager;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly FabrikamIdentityDbContext _context;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests(ITestOutputHelper output) : base(output)
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<FabrikamIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FabrikamIdentityDbContext(options);

        // Setup mock dependencies
        _mockUserManager = MockHelpers.CreateMockUserManager<FabrikamUser>();
        _mockSignInManager = MockHelpers.CreateMockSignInManager(_mockUserManager.Object);
        _mockRoleManager = MockHelpers.CreateMockRoleManager<FabrikamRole>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        _authService = new AuthenticationService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockRoleManager.Object,
            _mockJwtService.Object,
            _context,
            _mockLogger.Object);
    }

    public override void Dispose()
    {
        _context?.Dispose();
        base.Dispose();
    }

    #region User Registration Tests

    [Fact]
    public Task RegisterAsync_WithValidRequest_CreatesUserSuccessfully()
    {
        // Given
        RegisterRequest request = null!;
        AuthenticationResponse response = null!;
        FabrikamUser createdUser = null!;

        Given.That("we have a valid registration request", () =>
            {
                request = new RegisterRequest
                {
                    Email = "newuser@test.com",
                    Password = "TestPassword123!",
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "+1-555-123-4567"
                };
                StoreInContext("RegisterRequest", request);
            });

        // When
        When.I("register a new user", async () =>
            {
                var req = GetFromContext<RegisterRequest>("RegisterRequest");
                
                // Setup mocks for successful registration
                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync((FabrikamUser?)null);

                _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<FabrikamUser>(), req.Password))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<FabrikamUser, string>((user, password) => createdUser = user);

                _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<FabrikamUser>(), "User"))
                    .ReturnsAsync(IdentityResult.Success);

                _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<FabrikamUser>()))
                    .ReturnsAsync(new List<string> { "User" });

                _mockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<FabrikamUser>()))
                    .ReturnsAsync(new List<Claim>());

                _mockJwtService.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<FabrikamUser>(), It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
                    .ReturnsAsync("mock-access-token");

                _mockJwtService.Setup(x => x.GenerateRefreshToken())
                    .Returns("mock-refresh-token");

                _mockJwtService.Setup(x => x.GetTokenExpiration())
                    .Returns(DateTime.UtcNow.AddHours(1));

                response = await _authService.RegisterAsync(req);
                StoreInContext("AuthenticationResponse", response);
                StoreInContext("CreatedUser", createdUser);
            });

        // Then
        Then.The("registration should succeed with valid response", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                var user = GetFromContext<FabrikamUser>("CreatedUser");
                var req = GetFromContext<RegisterRequest>("RegisterRequest");

                resp.Should().NotBeNull("Response should be created");
                resp.Success.Should().BeTrue("Registration should succeed");
                resp.AccessToken.Should().Be("mock-access-token", "Should return access token");
                resp.RefreshToken.Should().Be("mock-refresh-token", "Should return refresh token");
                resp.User.Should().NotBeNull("Should return user info");
                resp.User.Email.Should().Be(req.Email, "User email should match request");
                
                user.Should().NotBeNull("User should be created");
                user.Email.Should().Be(req.Email, "Created user email should match");
                user.FirstName.Should().Be(req.FirstName, "First name should match");
                user.LastName.Should().Be(req.LastName, "Last name should match");
                user.IsActive.Should().BeTrue("New user should be active");
            });
            
            return Task.CompletedTask;
        }

    [Fact]
    public Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Given
        RegisterRequest request = null!;
        AuthenticationResponse response = null!;

        Given.That("we have a registration request with existing email", () =>
            {
                request = new RegisterRequest
                {
                    Email = "existing@test.com",
                    Password = "TestPassword123!",
                    FirstName = "John",
                    LastName = "Doe"
                };
                StoreInContext("RegisterRequest", request);
            });

        // When
        When.I("attempt to register with existing email", async () =>
            {
                var req = GetFromContext<RegisterRequest>("RegisterRequest");

                // Setup mock to return existing user
                var existingUser = new FabrikamUser { Email = req.Email };
                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(existingUser);

                response = await _authService.RegisterAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("registration should fail with appropriate error", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Registration should fail for existing email");
                resp.ErrorMessage.Should().Be("User with this email already exists", "Should return appropriate error message");
                resp.AccessToken.Should().BeNullOrEmpty("Should not return access token on failure");
            });
            
            return Task.CompletedTask;
        }

    [Fact]
    public Task RegisterAsync_WithInvalidCustomerId_ReturnsFailure()
    {
        // Given
        RegisterRequest request = null!;
        AuthenticationResponse response = null!;

        Given.That("we have a registration request with invalid customer ID", () =>
            {
                request = new RegisterRequest
                {
                    Email = "newuser@test.com",
                    Password = "TestPassword123!",
                    FirstName = "John",
                    LastName = "Doe",
                    CustomerId = 999 // Non-existent customer ID
                };
                StoreInContext("RegisterRequest", request);
            });

        // When
        When.I("attempt to register with invalid customer ID", async () =>
            {
                var req = GetFromContext<RegisterRequest>("RegisterRequest");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync((FabrikamUser?)null);

                // Ensure the context is clean (no customer with this ID exists)
                var existingCustomer = await _context.Customers.FindAsync(req.CustomerId);
                if (existingCustomer != null)
                {
                    _context.Customers.Remove(existingCustomer);
                    await _context.SaveChangesAsync();
                }

                response = await _authService.RegisterAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("registration should fail with customer error", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Registration should fail for invalid customer ID");
                resp.ErrorMessage.Should().Be("Invalid customer ID", "Should return appropriate error message");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task RegisterAsync_WithIdentityFailure_ReturnsFailure()
    {
        // Given
        RegisterRequest request = null!;
        AuthenticationResponse response = null!;

        Given.That("we have a registration request that will fail identity validation", () =>
            {
                request = new RegisterRequest
                {
                    Email = "newuser@test.com",
                    Password = "weak", // Weak password
                    FirstName = "John",
                    LastName = "Doe"
                };
                StoreInContext("RegisterRequest", request);
            });

        // When
        When.I("attempt to register with invalid data", async () =>
            {
                var req = GetFromContext<RegisterRequest>("RegisterRequest");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync((FabrikamUser?)null);

                var identityErrors = new[]
                {
                    new IdentityError { Code = "PasswordTooShort", Description = "Password too short" },
                    new IdentityError { Code = "PasswordRequiresDigit", Description = "Password requires digit" }
                };

                _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<FabrikamUser>(), req.Password))
                    .ReturnsAsync(IdentityResult.Failed(identityErrors));

                response = await _authService.RegisterAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("registration should fail with identity errors", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Registration should fail for identity errors");
                resp.ErrorMessage.Should().Contain("Registration failed", "Should contain registration failure message");
                resp.ErrorMessage.Should().Contain("Password too short", "Should contain specific error");
                resp.ErrorMessage.Should().Contain("Password requires digit", "Should contain specific error");
            });

            return Task.CompletedTask;
    }

    #endregion

    #region User Login Tests

    [Fact]
    public Task LoginAsync_WithValidCredentials_ReturnsSuccessfulResponse()
    {
        // Given
        LoginRequest request = null!;
        AuthenticationResponse response = null!;
        FabrikamUser user = null!;

        Given.That("we have valid login credentials", () =>
            {
                request = new LoginRequest
                {
                    Email = "user@test.com",
                    Password = "ValidPassword123!"
                };

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = request.Email,
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true
                };

                StoreInContext("LoginRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("attempt to login with valid credentials", async () =>
            {
                var req = GetFromContext<LoginRequest>("LoginRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(usr);

                _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(usr, req.Password, true))
                    .ReturnsAsync(SignInResult.Success);

                _mockUserManager.Setup(x => x.UpdateAsync(usr))
                    .ReturnsAsync(IdentityResult.Success);

                _mockUserManager.Setup(x => x.GetRolesAsync(usr))
                    .ReturnsAsync(new List<string> { "User" });

                _mockUserManager.Setup(x => x.GetClaimsAsync(usr))
                    .ReturnsAsync(new List<Claim>());

                _mockJwtService.Setup(x => x.GenerateAccessTokenAsync(usr, It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
                    .ReturnsAsync("access-token");

                _mockJwtService.Setup(x => x.GenerateRefreshToken())
                    .Returns("refresh-token");

                _mockJwtService.Setup(x => x.GetTokenExpiration())
                    .Returns(DateTime.UtcNow.AddHours(1));

                response = await _authService.LoginAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("login should succeed with tokens", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeTrue("Login should succeed");
                resp.AccessToken.Should().Be("access-token", "Should return access token");
                resp.RefreshToken.Should().Be("refresh-token", "Should return refresh token");
                resp.User.Should().NotBeNull("Should return user info");
                resp.User.Email.Should().Be("user@test.com", "Should return correct user email");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Given
        LoginRequest request = null!;
        AuthenticationResponse response = null!;

        Given.That("we have login request for non-existent user", () =>
            {
                request = new LoginRequest
                {
                    Email = "nonexistent@test.com",
                    Password = "AnyPassword123!"
                };
                StoreInContext("LoginRequest", request);
            });

        // When
        When.I("attempt to login with non-existent user", async () =>
            {
                var req = GetFromContext<LoginRequest>("LoginRequest");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync((FabrikamUser?)null);

                response = await _authService.LoginAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("login should fail with appropriate error", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Login should fail for non-existent user");
                resp.ErrorMessage.Should().Be("Invalid email or password", "Should not reveal user existence");
                resp.AccessToken.Should().BeNullOrEmpty("Should not return access token");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task LoginAsync_WithInactiveUser_ReturnsFailure()
    {
        // Given
        LoginRequest request = null!;
        AuthenticationResponse response = null!;
        FabrikamUser inactiveUser = null!;

        Given.That("we have login request for inactive user", () =>
            {
                request = new LoginRequest
                {
                    Email = "inactive@test.com",
                    Password = "ValidPassword123!"
                };

                inactiveUser = new FabrikamUser
                {
                    Id = "inactive-user",
                    Email = request.Email,
                    IsActive = false // User is inactive
                };

                StoreInContext("LoginRequest", request);
                StoreInContext("InactiveUser", inactiveUser);
            });

        // When
        When.I("attempt to login with inactive user", async () =>
            {
                var req = GetFromContext<LoginRequest>("LoginRequest");
                var user = GetFromContext<FabrikamUser>("InactiveUser");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(user);

                response = await _authService.LoginAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("login should fail for inactive user", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Login should fail for inactive user");
                resp.ErrorMessage.Should().Be("User account is inactive", "Should indicate account is inactive");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task LoginAsync_WithLockedOutUser_ReturnsFailure()
    {
        // Given
        LoginRequest request = null!;
        AuthenticationResponse response = null!;
        FabrikamUser lockedUser = null!;

        Given.That("we have login request for locked out user", () =>
            {
                request = new LoginRequest
                {
                    Email = "locked@test.com",
                    Password = "ValidPassword123!"
                };

                lockedUser = new FabrikamUser
                {
                    Id = "locked-user",
                    Email = request.Email,
                    IsActive = true
                };

                StoreInContext("LoginRequest", request);
                StoreInContext("LockedUser", lockedUser);
            });

        // When
        When.I("attempt to login with locked out user", async () =>
            {
                var req = GetFromContext<LoginRequest>("LoginRequest");
                var user = GetFromContext<FabrikamUser>("LockedUser");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(user);

                _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, req.Password, true))
                    .ReturnsAsync(SignInResult.LockedOut);

                response = await _authService.LoginAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("login should fail for locked out user", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Login should fail for locked out user");
                resp.ErrorMessage.Should().Be("Account is locked due to multiple failed login attempts", "Should indicate lockout");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Given
        LoginRequest request = null!;
        AuthenticationResponse response = null!;
        FabrikamUser user = null!;

        Given.That("we have login request with invalid password", () =>
            {
                request = new LoginRequest
                {
                    Email = "user@test.com",
                    Password = "WrongPassword123!"
                };

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = request.Email,
                    IsActive = true
                };

                StoreInContext("LoginRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("attempt to login with invalid password", async () =>
            {
                var req = GetFromContext<LoginRequest>("LoginRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(usr);

                _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(usr, req.Password, true))
                    .ReturnsAsync(SignInResult.Failed);

                response = await _authService.LoginAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("login should fail for invalid password", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Login should fail for invalid password");
                resp.ErrorMessage.Should().Be("Invalid email or password", "Should not reveal password invalidity specifically");
            });

            return Task.CompletedTask;
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Given
        RefreshTokenRequest request = null!;
        AuthenticationResponse response = null!;
        ClaimsPrincipal principal = null!;
        FabrikamUser user = null!;

        Given.That("we have a valid refresh token request", () =>
            {
                request = new RefreshTokenRequest
                {
                    AccessToken = "expired-access-token",
                    RefreshToken = "valid-refresh-token"
                };

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "user-123"),
                    new(ClaimTypes.Email, "user@test.com")
                };
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = "user@test.com",
                    IsActive = true
                };

                StoreInContext("RefreshTokenRequest", request);
                StoreInContext("Principal", principal);
                StoreInContext("User", user);
            });

        // When
        When.I("attempt to refresh tokens", async () =>
            {
                var req = GetFromContext<RefreshTokenRequest>("RefreshTokenRequest");
                var prin = GetFromContext<ClaimsPrincipal>("Principal");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockJwtService.Setup(x => x.GetPrincipalFromExpiredToken(req.AccessToken))
                    .Returns(prin);

                _mockUserManager.Setup(x => x.FindByIdAsync("user-123"))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.GetRolesAsync(usr))
                    .ReturnsAsync(new List<string> { "User" });

                _mockUserManager.Setup(x => x.GetClaimsAsync(usr))
                    .ReturnsAsync(new List<Claim>());

                _mockJwtService.Setup(x => x.GenerateAccessTokenAsync(usr, It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
                    .ReturnsAsync("new-access-token");

                _mockJwtService.Setup(x => x.GenerateRefreshToken())
                    .Returns("new-refresh-token");

                _mockJwtService.Setup(x => x.GetTokenExpiration())
                    .Returns(DateTime.UtcNow.AddHours(1));

                response = await _authService.RefreshTokenAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("token refresh should succeed with new tokens", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeTrue("Token refresh should succeed");
                resp.AccessToken.Should().Be("new-access-token", "Should return new access token");
                resp.RefreshToken.Should().Be("new-refresh-token", "Should return new refresh token");
                resp.User.Should().NotBeNull("Should return user info");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task RefreshTokenAsync_WithInvalidAccessToken_ReturnsFailure()
    {
        // Given
        RefreshTokenRequest request = null!;
        AuthenticationResponse response = null!;

        Given.That("we have a refresh request with invalid access token", () =>
            {
                request = new RefreshTokenRequest
                {
                    AccessToken = "invalid-token",
                    RefreshToken = "some-refresh-token"
                };
                StoreInContext("RefreshTokenRequest", request);
            });

        // When
        When.I("attempt to refresh with invalid access token", async () =>
            {
                var req = GetFromContext<RefreshTokenRequest>("RefreshTokenRequest");

                _mockJwtService.Setup(x => x.GetPrincipalFromExpiredToken(req.AccessToken))
                    .Returns((ClaimsPrincipal?)null);

                response = await _authService.RefreshTokenAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("token refresh should fail for invalid token", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Token refresh should fail for invalid token");
                resp.ErrorMessage.Should().Be("Invalid access token", "Should indicate invalid token");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task RefreshTokenAsync_WithMissingUserIdClaim_ReturnsFailure()
    {
        // Given
        RefreshTokenRequest request = null!;
        AuthenticationResponse response = null!;
        ClaimsPrincipal principal = null!;

        Given.That("we have a refresh request with token missing user ID claim", () =>
            {
                request = new RefreshTokenRequest
                {
                    AccessToken = "token-without-userid",
                    RefreshToken = "some-refresh-token"
                };

                // Principal without NameIdentifier claim
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Email, "user@test.com")
                };
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

                StoreInContext("RefreshTokenRequest", request);
                StoreInContext("Principal", principal);
            });

        // When
        When.I("attempt to refresh with missing user ID claim", async () =>
            {
                var req = GetFromContext<RefreshTokenRequest>("RefreshTokenRequest");
                var prin = GetFromContext<ClaimsPrincipal>("Principal");

                _mockJwtService.Setup(x => x.GetPrincipalFromExpiredToken(req.AccessToken))
                    .Returns(prin);

                response = await _authService.RefreshTokenAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("token refresh should fail for missing claims", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Token refresh should fail for missing user ID");
                resp.ErrorMessage.Should().Be("Invalid token claims", "Should indicate invalid claims");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task RefreshTokenAsync_WithInactiveUser_ReturnsFailure()
    {
        // Given
        RefreshTokenRequest request = null!;
        AuthenticationResponse response = null!;
        ClaimsPrincipal principal = null!;
        FabrikamUser inactiveUser = null!;

        Given.That("we have a refresh request for inactive user", () =>
            {
                request = new RefreshTokenRequest
                {
                    AccessToken = "token-for-inactive-user",
                    RefreshToken = "some-refresh-token"
                };

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "inactive-user-123")
                };
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

                inactiveUser = new FabrikamUser
                {
                    Id = "inactive-user-123",
                    Email = "inactive@test.com",
                    IsActive = false
                };

                StoreInContext("RefreshTokenRequest", request);
                StoreInContext("Principal", principal);
                StoreInContext("InactiveUser", inactiveUser);
            });

        // When
        When.I("attempt to refresh token for inactive user", async () =>
            {
                var req = GetFromContext<RefreshTokenRequest>("RefreshTokenRequest");
                var prin = GetFromContext<ClaimsPrincipal>("Principal");
                var usr = GetFromContext<FabrikamUser>("InactiveUser");

                _mockJwtService.Setup(x => x.GetPrincipalFromExpiredToken(req.AccessToken))
                    .Returns(prin);

                _mockUserManager.Setup(x => x.FindByIdAsync("inactive-user-123"))
                    .ReturnsAsync(usr);

                response = await _authService.RefreshTokenAsync(req);
                StoreInContext("AuthenticationResponse", response);
            });

        // Then
        Then.The("token refresh should fail for inactive user", () =>
            {
                var resp = GetFromContext<AuthenticationResponse>("AuthenticationResponse");
                
                resp.Success.Should().BeFalse("Token refresh should fail for inactive user");
                resp.ErrorMessage.Should().Be("User not found or inactive", "Should indicate user issue");
            });

            return Task.CompletedTask;
    }

    #endregion

    #region Password Change Tests

    [Fact]
    public Task ChangePasswordAsync_WithValidRequest_ReturnsSuccess()
    {
        // Given
        string userId = "user-123";
        ChangePasswordRequest request = null!;
        bool result = false;
        FabrikamUser user = null!;

        Given.That("we have a valid password change request", () =>
            {
                request = new ChangePasswordRequest
                {
                    CurrentPassword = "CurrentPassword123!",
                    NewPassword = "NewPassword456!"
                };

                user = new FabrikamUser
                {
                    Id = userId,
                    Email = "user@test.com"
                };

                StoreInContext("UserId", userId);
                StoreInContext("ChangePasswordRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("attempt to change password", async () =>
            {
                var uid = GetFromContext<string>("UserId");
                var req = GetFromContext<ChangePasswordRequest>("ChangePasswordRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByIdAsync(uid))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.ChangePasswordAsync(usr, req.CurrentPassword, req.NewPassword))
                    .ReturnsAsync(IdentityResult.Success);

                result = await _authService.ChangePasswordAsync(uid, req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password change should succeed", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeTrue("Password change should succeed");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task ChangePasswordAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Given
        string userId = "non-existent-user";
        ChangePasswordRequest request = null!;
        bool result = true; // Start with true to ensure test changes it

        Given.That("we have a password change request for non-existent user", () =>
            {
                request = new ChangePasswordRequest
                {
                    CurrentPassword = "CurrentPassword123!",
                    NewPassword = "NewPassword456!"
                };

                StoreInContext("UserId", userId);
                StoreInContext("ChangePasswordRequest", request);
            });

        // When
        When.I("attempt to change password for non-existent user", async () =>
            {
                var uid = GetFromContext<string>("UserId");
                var req = GetFromContext<ChangePasswordRequest>("ChangePasswordRequest");

                _mockUserManager.Setup(x => x.FindByIdAsync(uid))
                    .ReturnsAsync((FabrikamUser?)null);

                result = await _authService.ChangePasswordAsync(uid, req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password change should fail for non-existent user", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeFalse("Password change should fail for non-existent user");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task ChangePasswordAsync_WithInvalidCurrentPassword_ReturnsFailure()
    {
        // Given
        string userId = "user-123";
        ChangePasswordRequest request = null!;
        bool result = true;
        FabrikamUser user = null!;

        Given.That("we have a password change request with invalid current password", () =>
            {
                request = new ChangePasswordRequest
                {
                    CurrentPassword = "WrongCurrentPassword!",
                    NewPassword = "NewPassword456!"
                };

                user = new FabrikamUser
                {
                    Id = userId,
                    Email = "user@test.com"
                };

                StoreInContext("UserId", userId);
                StoreInContext("ChangePasswordRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("attempt to change password with wrong current password", async () =>
            {
                var uid = GetFromContext<string>("UserId");
                var req = GetFromContext<ChangePasswordRequest>("ChangePasswordRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByIdAsync(uid))
                    .ReturnsAsync(usr);

                var identityErrors = new[]
                {
                    new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password" }
                };

                _mockUserManager.Setup(x => x.ChangePasswordAsync(usr, req.CurrentPassword, req.NewPassword))
                    .ReturnsAsync(IdentityResult.Failed(identityErrors));

                result = await _authService.ChangePasswordAsync(uid, req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password change should fail for wrong current password", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeFalse("Password change should fail for wrong current password");
            });

            return Task.CompletedTask;
    }

    #endregion

    #region User Info Retrieval Tests

    [Fact]
    public Task GetUserInfoAsync_WithValidUserId_ReturnsUserInfo()
    {
        // Given
        string userId = "user-123";
        UserInfo? userInfo = null;
        FabrikamUser user = null!;

        Given.That("we have a valid user ID", () =>
            {
                user = new FabrikamUser
                {
                    Id = userId,
                    Email = "user@test.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    IsAdmin = false,
                    CustomerId = 123
                };

                StoreInContext("UserId", userId);
                StoreInContext("User", user);
            });

        // When
        When.I("request user info by ID", async () =>
            {
                var uid = GetFromContext<string>("UserId");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByIdAsync(uid))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.GetRolesAsync(usr))
                    .ReturnsAsync(new List<string> { "User", "Manager" });

                _mockUserManager.Setup(x => x.GetClaimsAsync(usr))
                    .ReturnsAsync(new List<Claim>
                    {
                        new("permission", "read"),
                        new("permission", "write")
                    });

                userInfo = await _authService.GetUserInfoAsync(uid);
                StoreInContext("UserInfo", userInfo);
            });

        // Then
        Then.The("user info should be returned correctly", () =>
            {
                var info = GetFromContext<UserInfo?>("UserInfo");
                var usr = GetFromContext<FabrikamUser>("User");
                
                info.Should().NotBeNull("User info should be returned");
                info!.Id.Should().Be(usr.Id, "User ID should match");
                info.Email.Should().Be(usr.Email, "Email should match");
                info.FirstName.Should().Be(usr.FirstName, "First name should match");
                info.LastName.Should().Be(usr.LastName, "Last name should match");
                info.DisplayName.Should().Be(usr.DisplayName, "Display name should match");
                info.IsActive.Should().Be(usr.IsActive, "Active status should match");
                info.IsAdmin.Should().Be(usr.IsAdmin, "Admin status should match");
                info.CustomerId.Should().Be(usr.CustomerId, "Customer ID should match");
                info.Roles.Should().Contain(new[] { "User", "Manager" }, "Should contain user roles");
                info.Permissions.Should().Contain("permission", "Should contain user permissions");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task GetUserInfoAsync_WithNonExistentUser_ReturnsNull()
    {
        // Given
        string userId = "non-existent-user";
        UserInfo? userInfo = null!;

        Given.That("we have a non-existent user ID", () =>
            {
                StoreInContext("UserId", userId);
            });

        // When
        When.I("request user info for non-existent user", async () =>
            {
                var uid = GetFromContext<string>("UserId");

                _mockUserManager.Setup(x => x.FindByIdAsync(uid))
                    .ReturnsAsync((FabrikamUser?)null);

                userInfo = await _authService.GetUserInfoAsync(uid);
                StoreInContext("UserInfo", userInfo);
            });

        // Then
        Then.The("user info should be null for non-existent user", () =>
            {
                var info = GetFromContext<UserInfo?>("UserInfo");
                
                info.Should().BeNull("Should return null for non-existent user");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task GetUserInfoByEmailAsync_WithValidEmail_ReturnsUserInfo()
    {
        // Given
        string email = "user@test.com";
        UserInfo? userInfo = null;
        FabrikamUser user = null!;

        Given.That("we have a valid email address", () =>
            {
                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = email,
                    FirstName = "Jane",
                    LastName = "Smith",
                    IsActive = true
                };

                StoreInContext("Email", email);
                StoreInContext("User", user);
            });

        // When
        When.I("request user info by email", async () =>
            {
                var em = GetFromContext<string>("Email");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(em))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.GetRolesAsync(usr))
                    .ReturnsAsync(new List<string> { "User" });

                _mockUserManager.Setup(x => x.GetClaimsAsync(usr))
                    .ReturnsAsync(new List<Claim>());

                userInfo = await _authService.GetUserInfoByEmailAsync(em);
                StoreInContext("UserInfo", userInfo);
            });

        // Then
        Then.The("user info should be returned by email correctly", () =>
            {
                var info = GetFromContext<UserInfo?>("UserInfo");
                var usr = GetFromContext<FabrikamUser>("User");
                
                info.Should().NotBeNull("User info should be returned");
                info!.Email.Should().Be(usr.Email, "Email should match");
                info.FirstName.Should().Be(usr.FirstName, "First name should match");
                info.LastName.Should().Be(usr.LastName, "Last name should match");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task GetUserInfoByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Given
        string email = "nonexistent@test.com";
        UserInfo? userInfo = null!;

        Given.That("we have a non-existent email address", () =>
            {
                StoreInContext("Email", email);
            });

        // When
        When.I("request user info for non-existent email", async () =>
            {
                var em = GetFromContext<string>("Email");

                _mockUserManager.Setup(x => x.FindByEmailAsync(em))
                    .ReturnsAsync((FabrikamUser?)null);

                userInfo = await _authService.GetUserInfoByEmailAsync(em);
                StoreInContext("UserInfo", userInfo);
            });

        // Then
        Then.The("user info should be null for non-existent email", () =>
            {
                var info = GetFromContext<UserInfo?>("UserInfo");
                
                info.Should().BeNull("Should return null for non-existent email");
            });

            return Task.CompletedTask;
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public Task RequestPasswordResetAsync_WithValidEmail_ReturnsSuccess()
    {
        // Given
        ResetPasswordRequest request = null!;
        bool result = false;
        FabrikamUser user = null!;

        Given.That("we have a valid password reset request", () =>
            {
                request = new ResetPasswordRequest
                {
                    Email = "user@test.com"
                };

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = request.Email
                };

                StoreInContext("ResetPasswordRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("request password reset", async () =>
            {
                var req = GetFromContext<ResetPasswordRequest>("ResetPasswordRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(usr))
                    .ReturnsAsync("reset-token-123");

                result = await _authService.RequestPasswordResetAsync(req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password reset request should succeed", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeTrue("Password reset request should succeed");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task RequestPasswordResetAsync_WithNonExistentEmail_ReturnsSuccessForSecurity()
    {
        // Given
        ResetPasswordRequest request = null!;
        bool result = false;

        Given.That("we have a password reset request for non-existent email", () =>
            {
                request = new ResetPasswordRequest
                {
                    Email = "nonexistent@test.com"
                };

                StoreInContext("ResetPasswordRequest", request);
            });

        // When
        When.I("request password reset for non-existent email", async () =>
            {
                var req = GetFromContext<ResetPasswordRequest>("ResetPasswordRequest");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync((FabrikamUser?)null);

                result = await _authService.RequestPasswordResetAsync(req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password reset should succeed for security reasons", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                // Returns true for security - don't reveal if user exists
                res.Should().BeTrue("Should return true to not reveal user existence");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task ConfirmPasswordResetAsync_WithValidToken_ReturnsSuccess()
    {
        // Given
        ConfirmPasswordResetRequest request = null!;
        bool result = false;
        FabrikamUser user = null!;

        Given.That("we have a valid password reset confirmation request", () =>
            {
                request = new ConfirmPasswordResetRequest
                {
                    Email = "user@test.com",
                    Token = "valid-reset-token",
                    NewPassword = "NewSecurePassword123!"
                };

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = request.Email
                };

                StoreInContext("ConfirmPasswordResetRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("confirm password reset with valid token", async () =>
            {
                var req = GetFromContext<ConfirmPasswordResetRequest>("ConfirmPasswordResetRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(usr);

                _mockUserManager.Setup(x => x.ResetPasswordAsync(usr, req.Token, req.NewPassword))
                    .ReturnsAsync(IdentityResult.Success);

                result = await _authService.ConfirmPasswordResetAsync(req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password reset confirmation should succeed", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeTrue("Password reset confirmation should succeed");
            });

            return Task.CompletedTask;
    }

    [Fact]
    public Task ConfirmPasswordResetAsync_WithInvalidToken_ReturnsFailure()
    {
        // Given
        ConfirmPasswordResetRequest request = null!;
        bool result = true; // Start with true to ensure test changes it
        FabrikamUser user = null!;

        Given.That("we have a password reset confirmation request with invalid token", () =>
            {
                request = new ConfirmPasswordResetRequest
                {
                    Email = "user@test.com",
                    Token = "invalid-reset-token",
                    NewPassword = "NewSecurePassword123!"
                };

                user = new FabrikamUser
                {
                    Id = "user-123",
                    Email = request.Email
                };

                StoreInContext("ConfirmPasswordResetRequest", request);
                StoreInContext("User", user);
            });

        // When
        When.I("confirm password reset with invalid token", async () =>
            {
                var req = GetFromContext<ConfirmPasswordResetRequest>("ConfirmPasswordResetRequest");
                var usr = GetFromContext<FabrikamUser>("User");

                _mockUserManager.Setup(x => x.FindByEmailAsync(req.Email))
                    .ReturnsAsync(usr);

                var identityErrors = new[]
                {
                    new IdentityError { Code = "InvalidToken", Description = "Invalid token" }
                };

                _mockUserManager.Setup(x => x.ResetPasswordAsync(usr, req.Token, req.NewPassword))
                    .ReturnsAsync(IdentityResult.Failed(identityErrors));

                result = await _authService.ConfirmPasswordResetAsync(req);
                StoreInContext("Result", result);
            });

        // Then
        Then.The("password reset confirmation should fail for invalid token", () =>
            {
                var res = GetFromContext<bool>("Result");
                
                res.Should().BeFalse("Password reset confirmation should fail for invalid token");
            });

            return Task.CompletedTask;
    }

    #endregion
}
