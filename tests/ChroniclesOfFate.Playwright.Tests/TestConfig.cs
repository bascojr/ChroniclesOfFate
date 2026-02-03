namespace ChroniclesOfFate.Playwright.Tests;

/// <summary>
/// Test configuration constants for the Chronicles of Fate Playwright tests.
/// </summary>
public static class TestConfig
{
    /// <summary>
    /// The base URL for the Blazor frontend.
    /// </summary>
    public const string BlazorBaseUrl = "https://localhost:57778";

    /// <summary>
    /// The base URL for the API backend.
    /// </summary>
    public const string ApiBaseUrl = "https://localhost:57777";

    /// <summary>
    /// Test user credentials for authentication tests.
    /// Uses the admin user from seed data since no separate test user exists.
    /// </summary>
    public static class TestUser
    {
        public const string Username = "admin";
        public const string Password = "Admin123!";
        public const string Email = "admin@chroniclesoffate.com";
    }

    /// <summary>
    /// Admin user credentials for back office tests.
    /// Matches seed data in DataSeeder.cs
    /// </summary>
    public static class AdminUser
    {
        public const string Username = "admin";
        public const string Password = "Admin123!";
    }

    /// <summary>
    /// Default timeout for page loads and navigation.
    /// </summary>
    public const int DefaultTimeoutMs = 30000;

    /// <summary>
    /// Short timeout for quick element checks.
    /// </summary>
    public const int ShortTimeoutMs = 5000;
}
