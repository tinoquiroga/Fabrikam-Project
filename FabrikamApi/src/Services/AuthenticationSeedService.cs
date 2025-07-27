using FabrikamApi.Data;
using FabrikamApi.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace FabrikamApi.Services;

public class AuthenticationSeedService
{
    private readonly UserManager<FabrikamUser> _userManager;
    private readonly RoleManager<FabrikamRole> _roleManager;
    private readonly ILogger<AuthenticationSeedService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _instanceId;
    private readonly Dictionary<string, string> _instancePasswords;

    public AuthenticationSeedService(
        UserManager<FabrikamUser> userManager,
        RoleManager<FabrikamRole> roleManager,
        ILogger<AuthenticationSeedService> logger,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
        
        // Generate instance identifier and passwords
        _instanceId = GetInstanceIdentifier();
        _instancePasswords = GenerateInstancePasswords(_instanceId);
    }

    /// <summary>
    /// Gets a unique identifier for this instance based on environment variables or configuration
    /// </summary>
    private string GetInstanceIdentifier()
    {
        // Try to get instance ID from various sources in order of preference
        var instanceId = 
            // Azure App Service instance ID
            Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ??
            // Azure environment name
            _configuration["AZURE_ENV_NAME"] ??
            // Custom instance identifier
            _configuration["InstanceId"] ??
            // Machine name as fallback
            Environment.MachineName ??
            // Final fallback - generate from user profile
            Environment.UserName;

        // For development, use a more readable identifier
        if (_environment.IsDevelopment())
        {
            var devInstanceId = _configuration["DevelopmentInstanceId"] ?? "dev-local";
            instanceId = $"{devInstanceId}-{Environment.UserName}";
        }

        // Normalize the instance ID (remove special characters, limit length)
        var normalizedId = new string(instanceId
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Take(20)
            .ToArray())
            .ToLowerInvariant();

        return string.IsNullOrEmpty(normalizedId) ? "default" : normalizedId;
    }

    /// <summary>
    /// Generates unique but deterministic passwords for this instance
    /// </summary>
    private Dictionary<string, string> GenerateInstancePasswords(string instanceId)
    {
        var passwords = new Dictionary<string, string>();
        var roles = new[] { "Admin", "Read-Write", "Read-Only", "Future-Role-A", "Future-Role-B", "Future-Role-C", "Future-Role-D" };

        foreach (var role in roles)
        {
            var password = GenerateRolePassword(role, instanceId);
            passwords[role] = password;
        }

        return passwords;
    }

    /// <summary>
    /// Generates a deterministic but unique password for a specific role and instance
    /// </summary>
    private string GenerateRolePassword(string role, string instanceId)
    {
        // Create a deterministic seed from role + instance ID
        var seed = $"Fabrikam-{role}-{instanceId}";
        
        // Use SHA256 to create a deterministic hash
        using var sha256 = SHA256.Create();
        var seedBytes = Encoding.UTF8.GetBytes(seed);
        var hashBytes = sha256.ComputeHash(seedBytes);
        
        // Convert hash to base64 and extract characters for uniqueness
        var hashString = Convert.ToBase64String(hashBytes);
        var alphaNumeric = new string(hashString.Where(char.IsLetterOrDigit).Take(8).ToArray());
        
        // Ensure we have at least one digit by taking hash value modulo
        var hashValue = BitConverter.ToUInt32(hashBytes, 0);
        var guaranteedDigit = (hashValue % 10).ToString(); // Ensures 0-9
        
        // Build password components
        var roleName = role.Replace("-", "");
        var uniquePart = alphaNumeric.Length >= 6 ? alphaNumeric.Substring(0, 6) : alphaNumeric.PadRight(6, 'X');
        
        // Create password: RoleName + unique chars + guaranteed digit + special char
        var password = $"{roleName}{uniquePart}{guaranteedDigit}!";
        
        // Ensure minimum length requirement (ASP.NET Identity default: 8 characters)
        if (password.Length < 8)
        {
            password += "2025!";
        }
        
        return password;
    }

    /// <summary>
    /// Gets the current instance passwords for external access (e.g., demo scripts)
    /// </summary>
    public Dictionary<string, string> GetInstancePasswords()
    {
        return new Dictionary<string, string>(_instancePasswords);
    }

    /// <summary>
    /// Gets the current instance identifier
    /// </summary>
    public string GetCurrentInstanceId()
    {
        return _instanceId;
    }

    public async Task SeedAuthenticationDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting authentication data seeding...");

            // First, ensure roles exist
            await EnsureRolesExistAsync();

            // Then seed users from JSON file
            await SeedUsersFromJsonAsync();

            _logger.LogInformation("Authentication data seeding completed successfully");

            // Log demo credentials for development
            if (_environment.IsDevelopment())
            {
                LogDemoCredentials();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during authentication data seeding");
            throw;
        }
    }

    private async Task EnsureRolesExistAsync()
    {
        var roles = new[] { "Admin", "Read-Write", "Read-Only", "User" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new FabrikamRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = GetRoleDescription(roleName),
                    IsActive = true,
                    Priority = GetRolePriority(roleName),
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogWarning("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedUsersFromJsonAsync()
    {
        var authUsersPath = Path.Combine(_environment.ContentRootPath, "Data", "SeedData", "auth-users.json");

        if (!File.Exists(authUsersPath))
        {
            _logger.LogWarning("Auth users seed file not found at: {Path}", authUsersPath);
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(authUsersPath);
        var seedUsers = JsonSerializer.Deserialize<List<AuthUserSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (seedUsers == null || !seedUsers.Any())
        {
            _logger.LogWarning("No auth users found in seed file");
            return;
        }

        foreach (var seedUser in seedUsers)
        {
            await CreateUserIfNotExistsAsync(seedUser);
        }
    }

    private async Task CreateUserIfNotExistsAsync(AuthUserSeedData seedUser)
    {
        // Get password for the role
        var password = _instancePasswords.GetValueOrDefault(seedUser.Role, GenerateRolePassword("Default", _instanceId));

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(seedUser.Email);
        if (existingUser != null)
        {
            // Update existing user's password to match current instance
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            var updateResult = await _userManager.ResetPasswordAsync(existingUser, token, password);
            if (updateResult.Succeeded)
            {
                _logger.LogInformation("Updated password for existing user: {Email} with role: {Role}", seedUser.Email, seedUser.Role);
            }
            else
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to update password for user {Email}: {Errors}", seedUser.Email, errors);
            }
            return;
        }

        // Create the user
        var user = new FabrikamUser
        {
            UserName = seedUser.Email,
            Email = seedUser.Email,
            NormalizedEmail = seedUser.Email.ToUpperInvariant(),
            NormalizedUserName = seedUser.Email.ToUpperInvariant(),
            FirstName = seedUser.FirstName,
            LastName = seedUser.LastName,
            EmailConfirmed = true, // For demo purposes
            IsActive = seedUser.IsActive,
            CreatedDate = DateTime.Parse(seedUser.CreatedDate),
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Created user: {Email} with role: {Role}", seedUser.Email, seedUser.Role);

            // Add user to role
            if (await _roleManager.RoleExistsAsync(seedUser.Role))
            {
                await _userManager.AddToRoleAsync(user, seedUser.Role);
                _logger.LogDebug("Added user {Email} to role {Role}", seedUser.Email, seedUser.Role);
            }
            else
            {
                // If role doesn't exist, add to User role as fallback
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogWarning("Role {Role} doesn't exist, added user {Email} to User role",
                    seedUser.Role, seedUser.Email);
            }
        }
        else
        {
            _logger.LogError("Failed to create user {Email}: {Errors}",
                seedUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private void LogDemoCredentials()
    {
        _logger.LogInformation("");
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("🔐 DEMO AUTHENTICATION CREDENTIALS");
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("");
        _logger.LogInformation("� INSTANCE-SPECIFIC DEMO CREDENTIALS (Instance: {InstanceId})", _instanceId);
        _logger.LogInformation("═══════════════════════════════════════");
        _logger.LogInformation("");
        _logger.LogInformation("�📧 Admin User:");
        _logger.LogInformation("   Email: lee.gu@fabrikam.levelupcsp.com");
        _logger.LogInformation("   Password: {Password}", _instancePasswords["Admin"]);
        _logger.LogInformation("   Role: Admin (Full system access)");
        _logger.LogInformation("");
        _logger.LogInformation("📧 Read-Write User:");
        _logger.LogInformation("   Email: alex.wilber@fabrikam.levelupcsp.com");
        _logger.LogInformation("   Password: {Password}", _instancePasswords["Read-Write"]);
        _logger.LogInformation("   Role: Read-Write (View and modify data)");
        _logger.LogInformation("");
        _logger.LogInformation("📧 Read-Only User:");
        _logger.LogInformation("   Email: henrietta.mueller@fabrikam.levelupcsp.com");
        _logger.LogInformation("   Password: {Password}", _instancePasswords["Read-Only"]);
        _logger.LogInformation("   Role: Read-Only (View access only)");
        _logger.LogInformation("");
        _logger.LogInformation("🔗 Test these credentials at: https://localhost:7297/swagger");
        _logger.LogInformation("📋 Use the /api/auth/login endpoint to get JWT tokens");
        _logger.LogInformation("");
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("");
    }

    private static string GetRoleDescription(string roleName) => roleName switch
    {
        "Admin" => "Full system access and administrative privileges",
        "Read-Write" => "Can view and modify business data",
        "Read-Only" => "Can view business data but cannot modify",
        "User" => "Basic user access",
        _ => $"Role: {roleName}"
    };

    private static int GetRolePriority(string roleName) => roleName switch
    {
        "Admin" => 1,
        "Read-Write" => 2,
        "Read-Only" => 3,
        "User" => 4,
        _ => 99
    };
}

// DTO class for deserializing the JSON seed data
public class AuthUserSeedData
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? AzureObjectId { get; set; }
    public string CreatedDate { get; set; } = string.Empty;
    public string? LastLoginDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
