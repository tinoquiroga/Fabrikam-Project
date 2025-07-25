using FabrikamApi.Data;
using FabrikamApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FabrikamApi.Services;

public class DataSeedService : ISeedService
{
    private readonly FabrikamDbContext _context;
    private readonly ILogger<DataSeedService> _logger;

    public DataSeedService(FabrikamDbContext context, ILogger<DataSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.Customers.AnyAsync())
            {
                _logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            _logger.LogInformation("Starting database seed...");

            SeedProducts();
            SeedCustomers();
            await SeedOrders();
            await SeedSupportTickets();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    public async Task ForceReseedAsync()
    {
        try
        {
            _logger.LogInformation("Starting force re-seed of database...");

            // Clear existing data
            _context.Orders.RemoveRange(_context.Orders);
            _context.Customers.RemoveRange(_context.Customers);
            _context.Products.RemoveRange(_context.Products);
            _context.SupportTickets.RemoveRange(_context.SupportTickets);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared existing data, now re-seeding...");

            SeedProducts();
            SeedCustomers();
            await SeedOrders();
            await SeedSupportTickets();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Force re-seed completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during force re-seed");
            throw;
        }
    }

    private void SeedProducts()
    {
        var products = new List<Product>
        {
            // Single Family Homes
            new Product
            {
                Name = "Cozy Cottage 1200",
                Description = "A charming 1,200 sq ft single-family modular home perfect for first-time buyers",
                ModelNumber = "CC-1200",
                Category = ProductCategory.SingleFamily,
                Price = 89500,
                StockQuantity = 25,
                ReorderLevel = 5,
                Dimensions = "40x30",
                SquareFeet = 1200,
                Bedrooms = 2,
                Bathrooms = 2,
                DeliveryDaysEstimate = 45,
                IsActive = true
            },
            new Product
            {
                Name = "Family Haven 1800",
                Description = "Spacious 1,800 sq ft family home with open concept living",
                ModelNumber = "FH-1800",
                Category = ProductCategory.SingleFamily,
                Price = 145000,
                StockQuantity = 18,
                ReorderLevel = 3,
                Dimensions = "45x40",
                SquareFeet = 1800,
                Bedrooms = 3,
                Bathrooms = 2,
                DeliveryDaysEstimate = 60,
                IsActive = true
            },
            new Product
            {
                Name = "Executive Manor 2500",
                Description = "Luxury 2,500 sq ft executive home with premium finishes",
                ModelNumber = "EM-2500",
                Category = ProductCategory.SingleFamily,
                Price = 225000,
                StockQuantity = 8,
                ReorderLevel = 2,
                Dimensions = "50x50",
                SquareFeet = 2500,
                Bedrooms = 4,
                Bathrooms = 3,
                DeliveryDaysEstimate = 75,
                IsActive = true
            },

            // Duplex Units
            new Product
            {
                Name = "Twin Vista Duplex",
                Description = "Modern duplex with two 1,000 sq ft units",
                ModelNumber = "TVD-2000",
                Category = ProductCategory.Duplex,
                Price = 175000,
                StockQuantity = 12,
                ReorderLevel = 2,
                Dimensions = "40x50",
                SquareFeet = 2000,
                Bedrooms = 4,
                Bathrooms = 4,
                DeliveryDaysEstimate = 65,
                IsActive = true
            },

            // Accessory Units
            new Product
            {
                Name = "Backyard Studio 400",
                Description = "Perfect ADU for rental income or home office",
                ModelNumber = "BS-400",
                Category = ProductCategory.Accessory,
                Price = 45000,
                StockQuantity = 35,
                ReorderLevel = 10,
                Dimensions = "20x20",
                SquareFeet = 400,
                Bedrooms = 1,
                Bathrooms = 1,
                DeliveryDaysEstimate = 30,
                IsActive = true
            },

            // Commercial Units
            new Product
            {
                Name = "Retail Flex 1500",
                Description = "Flexible retail/office space with storefront design",
                ModelNumber = "RF-1500",
                Category = ProductCategory.Commercial,
                Price = 125000,
                StockQuantity = 6,
                ReorderLevel = 1,
                Dimensions = "30x50",
                SquareFeet = 1500,
                DeliveryDaysEstimate = 90,
                IsActive = true
            },

            // Components
            new Product
            {
                Name = "Premium Kitchen Package",
                Description = "Upgraded kitchen with granite countertops and stainless appliances",
                ModelNumber = "PKP-001",
                Category = ProductCategory.Components,
                Price = 15000,
                StockQuantity = 50,
                ReorderLevel = 15,
                DeliveryDaysEstimate = 14,
                IsActive = true
            },
            new Product
            {
                Name = "Solar Power System",
                Description = "Complete solar installation with 20-year warranty",
                ModelNumber = "SPS-001",
                Category = ProductCategory.Components,
                Price = 25000,
                StockQuantity = 20,
                ReorderLevel = 5,
                DeliveryDaysEstimate = 21,
                IsActive = true
            }
        };

        _context.Products.AddRange(products);
        _logger.LogInformation("Added {Count} products to seed data", products.Count);
    }

    private void SeedCustomers()
    {
        var customers = new List<Customer>
        {
            new Customer
            {
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@email.com",
                Phone = "(555) 123-4567",
                Address = "123 Maple Street",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Region = "Pacific Northwest"
            },
            new Customer
            {
                FirstName = "Michael",
                LastName = "Chen",
                Email = "michael.chen@email.com",
                Phone = "(555) 234-5678",
                Address = "456 Oak Avenue",
                City = "Portland",
                State = "OR",
                ZipCode = "97201",
                Region = "Pacific Northwest"
            },
            new Customer
            {
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@email.com",
                Phone = "(555) 345-6789",
                Address = "789 Pine Road",
                City = "Austin",
                State = "TX",
                ZipCode = "73301",
                Region = "Southwest"
            },
            new Customer
            {
                FirstName = "David",
                LastName = "Rodriguez",
                Email = "david.rodriguez@email.com",
                Phone = "(555) 456-7890",
                Address = "321 Cedar Lane",
                City = "Denver",
                State = "CO",
                ZipCode = "80201",
                Region = "Mountain West"
            },
            new Customer
            {
                FirstName = "Jennifer",
                LastName = "Wilson",
                Email = "jennifer.wilson@email.com",
                Phone = "(555) 567-8901",
                Address = "654 Birch Court",
                City = "Atlanta",
                State = "GA",
                ZipCode = "30301",
                Region = "Southeast"
            },
            new Customer
            {
                FirstName = "Robert",
                LastName = "Anderson",
                Email = "robert.anderson@email.com",
                Phone = "(555) 678-9012",
                Address = "987 Elm Street",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                Region = "Midwest"
            },
            new Customer
            {
                FirstName = "Lisa",
                LastName = "Thompson",
                Email = "lisa.thompson@email.com",
                Phone = "(555) 789-0123",
                Address = "147 Willow Drive",
                City = "Phoenix",
                State = "AZ",
                ZipCode = "85001",
                Region = "Southwest"
            },
            new Customer
            {
                FirstName = "James",
                LastName = "Martinez",
                Email = "james.martinez@email.com",
                Phone = "(555) 890-1234",
                Address = "258 Spruce Avenue",
                City = "Miami",
                State = "FL",
                ZipCode = "33101",
                Region = "Southeast"
            }
        };

        _context.Customers.AddRange(customers);
        _logger.LogInformation("Added {Count} customers to seed data", customers.Count);
    }

    private async Task SeedOrders()
    {
        await _context.SaveChangesAsync(); // Save customers and products first

        var customers = await _context.Customers.ToListAsync();
        var products = await _context.Products.ToListAsync();

        var orders = new List<Order>();
        var random = new Random();

        // Create 15 sample orders
        for (int i = 0; i < 15; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));

            var order = new Order
            {
                CustomerId = customer.Id,
                OrderNumber = $"ORD{orderDate:yyyyMMdd}{(i + 1):D4}",
                Status = (OrderStatus)random.Next(1, 9),
                OrderDate = orderDate,
                ShippingAddress = customer.Address,
                ShippingCity = customer.City,
                ShippingState = customer.State,
                ShippingZipCode = customer.ZipCode,
                Region = customer.Region,
                LastUpdated = orderDate
            };

            // Add 1-3 items to each order
            var itemCount = random.Next(1, 4);
            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;

            for (int j = 0; j < itemCount; j++)
            {
                var product = products[random.Next(products.Count)];
                var quantity = random.Next(1, 3);

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * quantity
                };

                orderItems.Add(orderItem);
                subtotal += orderItem.LineTotal;
            }

            order.OrderItems = orderItems;
            order.Subtotal = subtotal;
            order.Tax = subtotal * 0.085m; // 8.5% tax
            order.Shipping = subtotal > 1000 ? 0 : 100; // Free shipping over $1000
            order.Total = order.Subtotal + order.Tax + order.Shipping;

            // Set shipped/delivered dates for shipped orders
            if (order.Status >= OrderStatus.Shipped)
            {
                order.ShippedDate = orderDate.AddDays(random.Next(14, 45));
                if (order.Status == OrderStatus.Delivered)
                {
                    order.DeliveredDate = order.ShippedDate?.AddDays(random.Next(1, 14));
                }
            }

            orders.Add(order);
        }

        _context.Orders.AddRange(orders);
        _logger.LogInformation("Added {Count} orders to seed data", orders.Count);
    }

    private async Task SeedSupportTickets()
    {
        await _context.SaveChangesAsync(); // Save orders first

        var customers = await _context.Customers.ToListAsync();
        var orders = await _context.Orders.ToListAsync();

        var tickets = new List<SupportTicket>();
        var random = new Random();

        var sampleIssues = new[]
        {
            ("Delivery Delay", "My order was supposed to arrive last week but I haven't received any updates", TicketCategory.DeliveryIssue),
            ("Installation Question", "Need help with foundation requirements for my modular home", TicketCategory.Installation),
            ("Product Defect", "Found a crack in one of the wall panels upon delivery", TicketCategory.ProductDefect),
            ("Order Status", "Can you provide an update on order #ORD20241215001?", TicketCategory.OrderInquiry),
            ("Billing Issue", "I was charged twice for my recent order", TicketCategory.Billing),
            ("Customization Request", "Want to upgrade the kitchen package before delivery", TicketCategory.General)
        };

        // Create 20 sample support tickets
        for (int i = 0; i < 20; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var createdDate = DateTime.UtcNow.AddDays(-random.Next(1, 60));
            var issue = sampleIssues[random.Next(sampleIssues.Length)];

            var ticket = new SupportTicket
            {
                TicketNumber = $"TKT{createdDate:yyyyMMdd}{(i + 1):D4}",
                CustomerId = customer.Id,
                OrderId = random.Next(0, 3) == 0 ? orders[random.Next(orders.Count)].Id : null, // 33% chance of having an order
                Subject = issue.Item1,
                Description = issue.Item2,
                Status = (TicketStatus)random.Next(1, 7),
                Priority = (TicketPriority)random.Next(1, 5),
                Category = issue.Item3,
                Region = customer.Region,
                CreatedDate = createdDate,
                LastUpdated = createdDate
            };

            // Assign some tickets
            if (random.Next(0, 2) == 0)
            {
                var agents = new[] { "John Smith", "Maria Garcia", "Alex Kim", "Sarah Connor" };
                ticket.AssignedTo = agents[random.Next(agents.Length)];
            }

            // Set resolved date for resolved tickets
            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            {
                ticket.ResolvedDate = createdDate.AddDays(random.Next(1, 7));
                ticket.LastUpdated = ticket.ResolvedDate.Value;
            }

            tickets.Add(ticket);
        }

        _context.SupportTickets.AddRange(tickets);
        _logger.LogInformation("Added {Count} support tickets to seed data", tickets.Count);

        // Add some sample notes to tickets
        await _context.SaveChangesAsync(); // Save tickets first

        var ticketNotes = new List<TicketNote>();
        foreach (var ticket in tickets.Take(10)) // Add notes to first 10 tickets
        {
            var noteCount = random.Next(1, 4);
            for (int i = 0; i < noteCount; i++)
            {
                var noteDate = ticket.CreatedDate.AddHours(random.Next(1, 48));
                var isInternal = random.Next(0, 2) == 0;

                var note = new TicketNote
                {
                    TicketId = ticket.Id,
                    Note = isInternal ? "Internal note: Customer called for update" : "Thank you for contacting support. We're looking into this issue.",
                    CreatedBy = ticket.AssignedTo ?? "Support Agent",
                    IsInternal = isInternal,
                    CreatedDate = noteDate
                };

                ticketNotes.Add(note);
            }
        }

        _context.TicketNotes.AddRange(ticketNotes);
        _logger.LogInformation("Added {Count} ticket notes to seed data", ticketNotes.Count);
    }
}
