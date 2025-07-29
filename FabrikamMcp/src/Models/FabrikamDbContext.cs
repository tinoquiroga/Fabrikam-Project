using Microsoft.EntityFrameworkCore;
using FabrikamContracts.DTOs;

namespace FabrikamMcp.Models;

/// <summary>
/// Entity Framework database context for Fabrikam MCP server
/// Handles user registration and audit data across all authentication modes
/// </summary>
public class FabrikamDbContext : DbContext
{
    public FabrikamDbContext(DbContextOptions<FabrikamDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Disabled mode users with user-generated GUIDs
    /// </summary>
    public DbSet<DisabledModeUser> DisabledModeUsers { get; set; }

    /// <summary>
    /// Authenticated users with service-generated audit GUIDs
    /// </summary>
    public DbSet<AuthenticatedUser> AuthenticatedUsers { get; set; }

    /// <summary>
    /// OAuth users with service-generated audit GUIDs
    /// </summary>
    public DbSet<OAuthUser> OAuthUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DisabledModeUser
        modelBuilder.Entity<DisabledModeUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Organization).HasMaxLength(200);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.AuthenticationMode).HasConversion<string>();
        });

        // Configure AuthenticatedUser
        modelBuilder.Entity<AuthenticatedUser>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Roles).HasMaxLength(500);
            entity.Property(e => e.AuthenticationMode).HasConversion<string>();
            entity.HasIndex(e => e.AuditGuid).IsUnique();
        });

        // Configure OAuthUser
        modelBuilder.Entity<OAuthUser>(entity =>
        {
            entity.HasKey(e => e.AzureObjectId);
            entity.Property(e => e.AzureObjectId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TenantId).HasMaxLength(100);
            entity.Property(e => e.GrantedScopes).HasMaxLength(1000);
            entity.Property(e => e.AuthenticationMode).HasConversion<string>();
            entity.HasIndex(e => e.AuditGuid).IsUnique();
        });

        // Add indexes for performance
        modelBuilder.Entity<DisabledModeUser>().HasIndex(e => e.AuditGuid);
        modelBuilder.Entity<AuthenticatedUser>().HasIndex(e => e.AuditGuid);
        modelBuilder.Entity<OAuthUser>().HasIndex(e => e.AuditGuid);
    }
}
