using System.Net.Http;

namespace ChroniclesOfFate.Playwright.Tests;

/// <summary>
/// Base class for all Playwright tests providing common setup and utilities.
/// Uses Playwright best practices: GetByRole, GetByLabel, GetByPlaceholder, GetByTestId.
/// </summary>
public abstract class PlaywrightTestBase : PageTest
{
    protected string BlazorBaseUrl => TestConfig.BlazorBaseUrl;
    protected string ApiBaseUrl => TestConfig.ApiBaseUrl;

    /// <summary>
    /// Setup that runs before each test to verify servers are accessible.
    /// </summary>
    [SetUp]
    public async Task BaseSetUp()
    {
        // Verify servers are reachable before running tests
        await WaitForServersAsync();
    }

    private async Task WaitForServersAsync()
    {
        var maxRetries = 30;
        var retryDelay = TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            if (await IsServerReadyAsync(BlazorBaseUrl))
            {
                return;
            }
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
        }
        
        Assert.Fail($"Servers not ready after {maxRetries} seconds. Ensure the API and Blazor servers are running.");
    }

    private static async Task<bool> IsServerReadyAsync(string url)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await client.GetAsync(url);
            return true; // Any response means server is up
        }
        catch
        {
            return false;
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true, // For local development with self-signed certs
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        };
    }

    /// <summary>
    /// Navigates to the login page and waits for it to load.
    /// </summary>
    protected async Task NavigateToLoginAsync()
    {
        await Page.GotoAsync($"{BlazorBaseUrl}/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Performs a login with the specified credentials using semantic locators.
    /// </summary>
    protected async Task LoginAsync(string username, string password)
    {
        await NavigateToLoginAsync();
        
        // Wait for the login form to be visible using role-based locator
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome Back" }).WaitForAsync();
        
        // Fill in credentials using placeholder-based locators (best practice for inputs without labels)
        await Page.GetByPlaceholder("Enter username").FillAsync(username);
        await Page.GetByPlaceholder("Enter password").FillAsync(password);
        
        // Click login button using role-based locator
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        
        // Wait for navigation after login
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Performs an admin login and navigates to the admin dashboard.
    /// </summary>
    protected async Task LoginAsAdminAsync()
    {
        await LoginAsync(TestConfig.AdminUser.Username, TestConfig.AdminUser.Password);
        
        // Navigate to admin after login
        await Page.GotoAsync($"{BlazorBaseUrl}/admin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Waits for a specific text to appear on the page using GetByText.
    /// </summary>
    protected async Task WaitForTextAsync(string text, int timeoutMs = TestConfig.DefaultTimeoutMs)
    {
        await Page.GetByText(text).WaitForAsync(new LocatorWaitForOptions
        {
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// Checks if an element with specific text exists on the page.
    /// </summary>
    protected async Task<bool> HasTextAsync(string text)
    {
        return await Page.GetByText(text).CountAsync() > 0;
    }

    /// <summary>
    /// Takes a screenshot with a descriptive name for debugging.
    /// </summary>
    protected async Task TakeScreenshotAsync(string name)
    {
        var screenshotPath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            "screenshots",
            $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
    }
}
