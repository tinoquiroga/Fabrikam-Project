namespace FabrikamApi.Services;

/// <summary>
/// Configuration options for data seeding
/// </summary>
public class SeedDataOptions
{
    public const string SectionName = "SeedData";

    /// <summary>
    /// The method to use for seeding data: "Json" or "Hardcoded"
    /// </summary>
    public string Method { get; set; } = "Json";

    /// <summary>
    /// Whether to enable seed data on startup
    /// </summary>
    public bool EnableSeedOnStartup { get; set; } = true;

    /// <summary>
    /// Path to JSON seed data files (relative to ContentRoot)
    /// </summary>
    public string JsonSeedDataPath { get; set; } = "Data/SeedData";
}
