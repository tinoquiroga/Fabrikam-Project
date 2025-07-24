using FabrikamApi.Data;
using FabrikamApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with In-Memory database for development
builder.Services.AddDbContext<FabrikamDbContext>(options =>
    options.UseInMemoryDatabase("FabrikamDb"));

// Add data seeding service
builder.Services.AddScoped<DataSeedService>();

// Configure OpenAPI/Swagger
builder.Services.AddOpenApi();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "Fabrikam API v1");
        c.RoutePrefix = ""; // Serve Swagger UI at root
    });
}

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a health check endpoint
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedDataAsync();
}

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
