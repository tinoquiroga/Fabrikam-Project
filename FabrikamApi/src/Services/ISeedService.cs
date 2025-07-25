namespace FabrikamApi.Services;

/// <summary>
/// Interface for data seeding services, enabling flexible seed data approaches
/// </summary>
public interface ISeedService
{
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    Task SeedDataAsync();
}
