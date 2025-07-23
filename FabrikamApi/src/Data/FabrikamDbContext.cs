using Microsoft.EntityFrameworkCore;
using FabrikamApi.Models;

namespace FabrikamApi.Data;

public class FabrikamDbContext : DbContext
{
    public FabrikamDbContext(DbContextOptions<FabrikamDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketNote> TicketNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision for financial fields
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Tax)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Shipping)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.LineTotal)
            .HasPrecision(18, 2);

        // Configure relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.Customer)
            .WithMany(c => c.SupportTickets)
            .HasForeignKey(st => st.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.Order)
            .WithMany()
            .HasForeignKey(st => st.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TicketNote>()
            .HasOne(tn => tn.Ticket)
            .WithMany(st => st.Notes)
            .HasForeignKey(tn => tn.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraints
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.TicketNumber)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.ModelNumber)
            .IsUnique();

        // Configure indexes for better query performance
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.CustomerId, o.OrderDate });

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => new { st.CustomerId, st.Status });

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.CreatedDate);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Category);
    }
}
