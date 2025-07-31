using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using FabrikamApi.Models.Authentication;
using FabrikamApi.Data;

namespace FabrikamApi.Services.Authentication;

/// <summary>
/// Service interface for authentication operations
/// Handles user registration, login, and token management
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration request data</param>
    /// <returns>Authentication response with token or error</returns>
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user login
    /// </summary>
    /// <param name="request">Login request data</param>
    /// <returns>Authentication response with token or error</returns>
    Task<AuthenticationResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication response or error</returns>
    Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// Changes a user's password
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="request">Change password request</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    /// <summary>
    /// Initiates password reset process
    /// </summary>
    /// <param name="request">Reset password request</param>
    /// <returns>True if reset email sent, false otherwise</returns>
    Task<bool> RequestPasswordResetAsync(ResetPasswordRequest request);

    /// <summary>
    /// Completes password reset with token
    /// </summary>
    /// <param name="request">Confirm password reset request</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ConfirmPasswordResetAsync(ConfirmPasswordResetRequest request);

    /// <summary>
    /// Gets user information by ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>User information or null if not found</returns>
    Task<UserInfo?> GetUserInfoAsync(string userId);

    /// <summary>
    /// Gets user information by email address (for test token compatibility)
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User information or null if not found</returns>
    Task<UserInfo?> GetUserInfoByEmailAsync(string email);
}

/// <summary>
/// Authentication service implementation using ASP.NET Identity
/// Integrates with FabrikamUser and JWT token generation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<FabrikamUser> _userManager;
    private readonly SignInManager<FabrikamUser> _signInManager;
    private readonly RoleManager<FabrikamRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly FabrikamIdentityDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<FabrikamUser> userManager,
        SignInManager<FabrikamUser> signInManager,
        RoleManager<FabrikamRole> roleManager,
        IJwtService jwtService,
        FabrikamIdentityDbContext context,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "User with this email already exists"
                };
            }

            // Validate customer ID if provided
            if (request.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    _logger.LogWarning("Registration failed: Invalid customer ID {CustomerId}", request.CustomerId);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid customer ID"
                    };
                }
            }

            // Create new user
            var user = new FabrikamUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CustomerId = request.CustomerId,
                EmailConfirmed = true, // Auto-confirm for demo purposes
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, errors);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = $"Registration failed: {errors}"
                };
            }

            // Assign default "User" role
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User {UserId} registered successfully with email {Email}", user.Id, request.Email);

            // Generate token for immediate login
            return await GenerateAuthenticationResponseAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            return new AuthenticationResponse
            {
                Success = false,
                ErrorMessage = "An error occurred during registration"
            };
        }
    }

    /// <inheritdoc />
    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User account {UserId} is inactive", user.Id);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "User account is inactive"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: User {UserId} is locked out", user.Id);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Account is locked due to multiple failed login attempts"
                };
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Update last active date
            user.LastActiveDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return await GenerateAuthenticationResponseAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            return new AuthenticationResponse
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    /// <inheritdoc />
    public async Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid access token"
                };
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid token claims"
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "User not found or inactive"
                };
            }

            // In a production system, you would validate the refresh token against stored values
            // For this demo, we'll generate a new token

            return await GenerateAuthenticationResponseAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthenticationResponse
            {
                Success = false,
                ErrorMessage = "An error occurred during token refresh"
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Change password failed: User {UserId} not found", userId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return true;
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, errors);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RequestPasswordResetAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal whether the user exists for security
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
                return true;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // In a production system, you would send the reset email here
            _logger.LogInformation("Password reset token generated for user {UserId}", user.Id);

            // For demo purposes, log the token (don't do this in production!)
            _logger.LogDebug("Password reset token for {Email}: {Token}", request.Email, token);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email {Email}", request.Email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmPasswordResetAsync(ConfirmPasswordResetRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset confirmation failed: User not found for email {Email}", request.Email);
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
                return true;
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming password reset for email {Email}", request.Email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserInfoAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Roles = roles.ToList(),
                Permissions = claims.Select(c => c.Type).ToList(),
                IsAdmin = user.IsAdmin,
                CustomerId = user.CustomerId,
                IsActive = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserInfoByEmailAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Roles = roles.ToList(),
                Permissions = claims.Select(c => c.Type).ToList(),
                IsAdmin = user.IsAdmin,
                CustomerId = user.CustomerId,
                IsActive = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for email {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Generates authentication response with JWT tokens and user info
    /// </summary>
    private async Task<AuthenticationResponse> GenerateAuthenticationResponseAsync(FabrikamUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles, claims);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            TokenType = "Bearer",
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Roles = roles.ToList(),
                Permissions = claims.Select(c => c.Type).ToList(),
                IsAdmin = user.IsAdmin,
                CustomerId = user.CustomerId,
                IsActive = user.IsActive
            },
            Success = true
        };
    }
}
