using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FabrikamApi.Models.Authentication;
using System.Security.Claims;

namespace FabrikamTests.Helpers;

/// <summary>
/// Helper class for creating mock instances of ASP.NET Identity services
/// Provides standardized mocking patterns for UserManager, SignInManager, and RoleManager
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// Creates a mock UserManager with the necessary setup for testing
    /// </summary>
    /// <typeparam name="TUser">The user type (typically FabrikamUser)</typeparam>
    /// <returns>Mock UserManager instance</returns>
    public static Mock<UserManager<TUser>> CreateMockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<TUser>>();
        var userValidators = new List<IUserValidator<TUser>>();
        var passwordValidators = new List<IPasswordValidator<TUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<TUser>>>();

        var mockUserManager = new Mock<UserManager<TUser>>(
            store.Object, 
            optionsAccessor.Object, 
            passwordHasher.Object,
            userValidators, 
            passwordValidators, 
            keyNormalizer.Object,
            errors.Object, 
            services.Object, 
            logger.Object);

        // Setup default behaviors for common operations
        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.DeleteAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.RemoveFromRoleAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<TUser>()))
            .ReturnsAsync(new List<string> { "User" });

        mockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<TUser>()))
            .ReturnsAsync(new List<Claim>());

        mockUserManager.Setup(x => x.AddClaimAsync(It.IsAny<TUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.RemoveClaimAsync(It.IsAny<TUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.ChangePasswordAsync(It.IsAny<TUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<TUser>()))
            .ReturnsAsync("reset-token");

        mockUserManager.Setup(x => x.ResetPasswordAsync(It.IsAny<TUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<TUser>()))
            .ReturnsAsync("confirmation-token");

        mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        return mockUserManager;
    }

    /// <summary>
    /// Creates a mock SignInManager with the necessary setup for testing
    /// </summary>
    /// <param name="userManager">The UserManager instance (can be mocked)</param>
    /// <returns>Mock SignInManager instance</returns>
    public static Mock<SignInManager<FabrikamUser>> CreateMockSignInManager(UserManager<FabrikamUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<FabrikamUser>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<FabrikamUser>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var userConfirmation = new Mock<IUserConfirmation<FabrikamUser>>();

        var mockSignInManager = new Mock<SignInManager<FabrikamUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            optionsAccessor.Object,
            logger.Object,
            schemes.Object,
            userConfirmation.Object);

        // Setup default behaviors for common sign-in operations
        mockSignInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        mockSignInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<FabrikamUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<FabrikamUser>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        mockSignInManager.Setup(x => x.SignInAsync(It.IsAny<FabrikamUser>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mockSignInManager.Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        mockSignInManager.Setup(x => x.RefreshSignInAsync(It.IsAny<FabrikamUser>()))
            .Returns(Task.CompletedTask);

        mockSignInManager.Setup(x => x.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);

        mockSignInManager.Setup(x => x.CanSignInAsync(It.IsAny<FabrikamUser>()))
            .ReturnsAsync(true);

        return mockSignInManager;
    }

    /// <summary>
    /// Creates a mock RoleManager with the necessary setup for testing
    /// </summary>
    /// <typeparam name="TRole">The role type (typically FabrikamRole)</typeparam>
    /// <returns>Mock RoleManager instance</returns>
    public static Mock<RoleManager<TRole>> CreateMockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        var roleValidators = new List<IRoleValidator<TRole>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<TRole>>>();

        var mockRoleManager = new Mock<RoleManager<TRole>>(
            store.Object,
            roleValidators,
            keyNormalizer.Object,
            errors.Object,
            logger.Object);

        // Setup default behaviors for common role operations
        mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<TRole>()))
            .ReturnsAsync(IdentityResult.Success);

        mockRoleManager.Setup(x => x.UpdateAsync(It.IsAny<TRole>()))
            .ReturnsAsync(IdentityResult.Success);

        mockRoleManager.Setup(x => x.DeleteAsync(It.IsAny<TRole>()))
            .ReturnsAsync(IdentityResult.Success);

        mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        mockRoleManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((TRole?)null);

        mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((TRole?)null);

        mockRoleManager.Setup(x => x.GetRoleIdAsync(It.IsAny<TRole>()))
            .ReturnsAsync("role-id");

        mockRoleManager.Setup(x => x.GetRoleNameAsync(It.IsAny<TRole>()))
            .ReturnsAsync("role-name");

        mockRoleManager.Setup(x => x.SetRoleNameAsync(It.IsAny<TRole>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return mockRoleManager;
    }

    /// <summary>
    /// Creates a test FabrikamUser with default values
    /// </summary>
    /// <param name="email">User email (defaults to test@example.com)</param>
    /// <param name="firstName">User first name (defaults to Test)</param>
    /// <param name="lastName">User last name (defaults to User)</param>
    /// <returns>FabrikamUser instance for testing</returns>
    public static FabrikamUser CreateTestUser(
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User")
    {
        return new FabrikamUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CompanyName = "Test Company",
            NotificationPreferences = "email"
        };
    }

    /// <summary>
    /// Creates a test FabrikamRole with default values
    /// </summary>
    /// <param name="name">Role name (defaults to User)</param>
    /// <param name="description">Role description</param>
    /// <returns>FabrikamRole instance for testing</returns>
    public static FabrikamRole CreateTestRole(
        string name = "User",
        string description = "Standard user role")
    {
        return new FabrikamRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            Priority = 100
        };
    }

    /// <summary>
    /// Creates an IdentityResult.Failed with specific error codes
    /// </summary>
    /// <param name="errorCodes">Error codes to include</param>
    /// <returns>Failed IdentityResult</returns>
    public static IdentityResult CreateFailedResult(params string[] errorCodes)
    {
        var errors = errorCodes.Select(code => new IdentityError
        {
            Code = code,
            Description = $"Error: {code}"
        }).ToArray();

        return IdentityResult.Failed(errors);
    }

    /// <summary>
    /// Creates a SignInResult.Failed with specific properties
    /// </summary>
    /// <param name="isLockedOut">Whether account is locked out</param>
    /// <param name="isNotAllowed">Whether sign-in is not allowed</param>
    /// <param name="requiresTwoFactor">Whether two-factor is required</param>
    /// <returns>Failed SignInResult</returns>
    public static SignInResult CreateFailedSignInResult(
        bool isLockedOut = false,
        bool isNotAllowed = false,
        bool requiresTwoFactor = false)
    {
        if (isLockedOut) return SignInResult.LockedOut;
        if (isNotAllowed) return SignInResult.NotAllowed;
        if (requiresTwoFactor) return SignInResult.TwoFactorRequired;
        return SignInResult.Failed;
    }

    /// <summary>
    /// Creates test claims for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roles">User roles</param>
    /// <param name="customClaims">Additional custom claims</param>
    /// <returns>List of claims</returns>
    public static List<Claim> CreateTestClaims(
        string userId,
        IEnumerable<string>? roles = null,
        Dictionary<string, string>? customClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "test@example.com"),
            new(ClaimTypes.Email, "test@example.com")
        };

        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        if (customClaims != null)
        {
            claims.AddRange(customClaims.Select(kvp => new Claim(kvp.Key, kvp.Value)));
        }

        return claims;
    }
}
