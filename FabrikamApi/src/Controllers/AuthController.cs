using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FabrikamApi.Models.Authentication;
using FabrikamApi.Services.Authentication;
using FabrikamApi.Services;

namespace FabrikamApi.Controllers;

/// <summary>
/// Authentication controller handling user registration, login, and token management
/// Provides JWT-based authentication for the Fabrikam API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly AuthenticationSeedService _seedService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthenticationService authenticationService, 
        AuthenticationSeedService seedService,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment)
    {
        _authenticationService = authenticationService;
        _seedService = seedService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">User registration information</param>
    /// <returns>Authentication response with JWT token or error</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid registration data</response>
    /// <response code="409">User already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 409)]
    public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid registration request: {Errors}", errors);
                return BadRequest($"Invalid registration data: {errors}");
            }

            var response = await _authenticationService.RegisterAsync(request);

            if (!response.Success)
            {
                _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, response.ErrorMessage);

                if (response.ErrorMessage?.Contains("already exists") == true)
                {
                    return Conflict(response.ErrorMessage);
                }

                return BadRequest(response.ErrorMessage);
            }

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return StatusCode(500, "An error occurred during registration");
        }
    }

    /// <summary>
    /// Authenticate user login
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <returns>Authentication response with JWT token or error</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid login data</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="423">Account locked</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 401)]
    [ProducesResponseType(typeof(string), 423)]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid login request: {Errors}", errors);
                return BadRequest($"Invalid login data: {errors}");
            }

            var response = await _authenticationService.LoginAsync(request);

            if (!response.Success)
            {
                _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, response.ErrorMessage);

                if (response.ErrorMessage?.Contains("locked") == true)
                {
                    return StatusCode(423, response.ErrorMessage);
                }

                return Unauthorized(response.ErrorMessage);
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for {Email}", request.Email);
            return StatusCode(500, "An error occurred during login");
        }
    }

    /// <summary>
    /// Refresh JWT access token using refresh token
    /// </summary>
    /// <param name="request">Token refresh request</param>
    /// <returns>New authentication response with refreshed tokens</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid token data</response>
    /// <response code="401">Invalid or expired tokens</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 401)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid refresh token request: {Errors}", errors);
                return BadRequest($"Invalid token data: {errors}");
            }

            var response = await _authenticationService.RefreshTokenAsync(request);

            if (!response.Success)
            {
                _logger.LogWarning("Token refresh failed: {Error}", response.ErrorMessage);
                return Unauthorized(response.ErrorMessage);
            }

            _logger.LogDebug("Token refreshed successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, "An error occurred during token refresh");
        }
    }

    /// <summary>
    /// Change user password (requires authentication)
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success status</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Invalid password data</response>
    /// <response code="401">Unauthorized access</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 401)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid change password request: {Errors}", errors);
                return BadRequest($"Invalid password data: {errors}");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Change password failed: No user ID in token");
                return Unauthorized("Invalid user token");
            }

            var success = await _authenticationService.ChangePasswordAsync(userId, request);

            if (!success)
            {
                _logger.LogWarning("Password change failed for user {UserId}", userId);
                return BadRequest("Password change failed. Please check your current password.");
            }

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, "An error occurred during password change");
        }
    }

    /// <summary>
    /// Request password reset (sends reset email)
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Success status</returns>
    /// <response code="200">Reset email sent (if user exists)</response>
    /// <response code="400">Invalid email data</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<ActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid reset password request: {Errors}", errors);
                return BadRequest($"Invalid email data: {errors}");
            }

            await _authenticationService.RequestPasswordResetAsync(request);

            // Always return success for security (don't reveal if user exists)
            _logger.LogInformation("Password reset requested for email: {Email}", request.Email);
            return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for {Email}", request.Email);
            return StatusCode(500, "An error occurred during password reset request");
        }
    }

    /// <summary>
    /// Confirm password reset with token
    /// </summary>
    /// <param name="request">Password reset confirmation</param>
    /// <returns>Success status</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid reset data or token</response>
    [HttpPost("confirm-reset")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<ActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                _logger.LogWarning("Invalid confirm password reset request: {Errors}", errors);
                return BadRequest($"Invalid reset data: {errors}");
            }

            var success = await _authenticationService.ConfirmPasswordResetAsync(request);

            if (!success)
            {
                _logger.LogWarning("Password reset confirmation failed for email: {Email}", request.Email);
                return BadRequest("Password reset failed. Please check your reset token and try again.");
            }

            _logger.LogInformation("Password reset confirmed for email: {Email}", request.Email);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset confirmation for {Email}", request.Email);
            return StatusCode(500, "An error occurred during password reset confirmation");
        }
    }

    /// <summary>
    /// Get current user information (requires authentication)
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="404">User not found</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), 200)]
    [ProducesResponseType(typeof(string), 401)]
    [ProducesResponseType(typeof(string), 404)]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Get current user failed: No user ID in token");
                return Unauthorized("Invalid user token");
            }

            var userInfo = await _authenticationService.GetUserInfoAsync(userId);
            if (userInfo == null)
            {
                _logger.LogWarning("User information not found for user {UserId}", userId);
                return NotFound("User not found");
            }

            _logger.LogDebug("Retrieved user information for user {UserId}", userId);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return StatusCode(500, "An error occurred while retrieving user information");
        }
    }

    /// <summary>
    /// Logout (invalidate tokens - for future enhancement)
    /// </summary>
    /// <returns>Success status</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} logged out", userId);

            // In a production system, you would invalidate the refresh token here
            // For now, we'll just return a success response
            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }

    /// <summary>
    /// Get demo user credentials for the current instance
    /// Only available in development environment for security
    /// </summary>
    /// <returns>Demo user credentials with instance-specific passwords</returns>
    [HttpGet("demo-credentials")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(403)]
    public ActionResult GetDemoCredentials()
    {
        try
        {
            // Allow in development environment OR dev environment (for Azure deployments)
            if (!_environment.IsDevelopment() && !_environment.EnvironmentName.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "Demo credentials are only available in development or dev environments" });
            }

            var instanceId = _seedService.GetCurrentInstanceId();
            var passwords = _seedService.GetInstancePasswords();

            var demoUsers = new[]
            {
                new {
                    Name = "Lee Gu (Admin)",
                    Email = "lee.gu@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Admin", ""),
                    Role = "Admin",
                    Description = "Full system access - Licensed mailbox"
                },
                new {
                    Name = "Alex Wilber (Read-Write)",
                    Email = "alex.wilber@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Read-Write", ""),
                    Role = "Read-Write",
                    Description = "Can view and modify data - Licensed mailbox"
                },
                new {
                    Name = "Henrietta Mueller (Read-Only)",
                    Email = "henrietta.mueller@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Read-Only", ""),
                    Role = "Read-Only",
                    Description = "View access only - Licensed mailbox"
                },
                new {
                    Name = "Pradeep Gupta (Future Role A)",
                    Email = "pradeep.gupta@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Future-Role-A", ""),
                    Role = "Future-Role-A",
                    Description = "Reserved for future role implementation - Licensed mailbox"
                },
                new {
                    Name = "Lidia Holloway (Future Role B)",
                    Email = "lidia.holloway@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Future-Role-B", ""),
                    Role = "Future-Role-B",
                    Description = "Reserved for future role implementation - Licensed mailbox"
                },
                new {
                    Name = "Joni Sherman (Future Role C)",
                    Email = "joni.sherman@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Future-Role-C", ""),
                    Role = "Future-Role-C",
                    Description = "Reserved for future role implementation - Licensed mailbox"
                },
                new {
                    Name = "Miriam Graham (Future Role D)",
                    Email = "miriam.graham@fabrikam.levelupcsp.com",
                    Password = passwords.GetValueOrDefault("Future-Role-D", ""),
                    Role = "Future-Role-D",
                    Description = "Reserved for future role implementation - Licensed mailbox"
                }
            };

            return Ok(new 
            { 
                instanceId = instanceId,
                demoUsers = demoUsers,
                apiUrl = $"{Request.Scheme}://{Request.Host}",
                note = "These passwords are unique to this instance for security"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving demo credentials");
            return StatusCode(500, "An error occurred retrieving demo credentials");
        }
    }
}
