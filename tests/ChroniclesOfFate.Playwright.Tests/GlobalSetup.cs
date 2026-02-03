using ChroniclesOfFate.Playwright.Tests.Fixtures;

[assembly: LevelOfParallelism(1)]

namespace ChroniclesOfFate.Playwright.Tests;

/// <summary>
/// Global setup fixture that starts servers once before all tests run.
/// Uses NUnit's SetUpFixture to manage server lifecycle.
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    private static ServerFixture? _serverFixture;
    
    /// <summary>
    /// Gets whether the servers are ready for testing.
    /// </summary>
    public static bool ServersReady => 
        _serverFixture?.IsApiRunning == true && 
        _serverFixture?.IsBlazorRunning == true;

    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        Console.WriteLine("=== Global Test Setup ===");
        Console.WriteLine($"Starting at: {DateTime.Now}");
        
        _serverFixture = new ServerFixture();
        
        try
        {
            await _serverFixture.StartServersAsync();
            Console.WriteLine("=== Servers Ready for Testing ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start servers: {ex.Message}");
            Console.WriteLine("Tests will attempt to run against already-running servers.");
        }
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        Console.WriteLine("=== Global Test Teardown ===");
        Console.WriteLine($"Completing at: {DateTime.Now}");
        
        _serverFixture?.Dispose();
        _serverFixture = null;
        
        Console.WriteLine("=== Test Run Complete ===");
    }
}
