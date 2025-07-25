using FabrikamApi.Data;
using FabrikamApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with In-Memory database for development
builder.Services.AddDbContext<FabrikamDbContext>(options =>
    options.UseInMemoryDatabase("FabrikamDb"));

// Add data seeding services - JSON as primary, hardcoded as fallback
builder.Services.AddScoped<JsonDataSeedService>();
builder.Services.AddScoped<DataSeedService>();

// Configure seed data options
builder.Services.Configure<SeedDataOptions>(
    builder.Configuration.GetSection(SeedDataOptions.SectionName));

// Use JSON seed service as the primary implementation (configurable)
builder.Services.AddScoped<ISeedService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var seedMethod = configuration.GetValue<string>("SeedData:Method", "Json");

    return seedMethod.ToLowerInvariant() switch
    {
        "hardcoded" => provider.GetRequiredService<DataSeedService>(),
        "json" or _ => provider.GetRequiredService<JsonDataSeedService>()
    };
});

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
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

// Seed data on startup (configurable)
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var enableSeed = configuration.GetValue<bool>("SeedData:EnableSeedOnStartup", true);

    if (enableSeed)
    {
        var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
        await seedService.SeedDataAsync();
    }
}

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
