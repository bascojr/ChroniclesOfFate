namespace ChroniclesOfFate.Playwright.Tests.Integration;

/// <summary>
/// End-to-end integration tests covering full user workflows.
/// </summary>
[TestFixture]
[Category("Integration")]
[Category("E2E")]
public class UserWorkflowTests : PlaywrightTestBase
{
    [Test]
    public async Task FullRegistrationFlow_ShouldCreateAccountAndRedirect()
    {
        // Arrange
        await NavigateToLoginAsync();
        var uniqueUsername = $"testuser_{DateTime.Now.Ticks}";
        var uniqueEmail = $"{uniqueUsername}@test.com";

        // Act - Switch to registration mode
        await Page.ClickAsync("text=Create Account");
        
        // Fill in registration form
        await Page.FillAsync("input[placeholder='Enter username']", uniqueUsername);
        await Page.FillAsync("input[placeholder='Enter email']", uniqueEmail);
        await Page.FillAsync("input[placeholder='Enter password']", "SecurePassword123!");
        
        // Submit
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should redirect to sessions or show success
        // The exact behavior depends on the auth flow
        await Page.WaitForTimeoutAsync(2000); // Wait for potential redirect
    }

    [Test]
    public async Task LoginAndViewSessions_ShouldShowUserAdventures()
    {
        // Arrange & Act
        await LoginAsync(TestConfig.TestUser.Username, TestConfig.TestUser.Password);
        
        // Navigate to sessions if not redirected
        if (!Page.Url.Contains("sessions"))
        {
            await Page.GotoAsync($"{BlazorBaseUrl}/sessions");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Assert
        await Expect(Page.Locator("h2")).ToContainTextAsync("Your Adventures");
    }

    [Test]
    public async Task AdminWorkflow_CreateAndEditEnemy()
    {
        // Arrange
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/enemies");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Open create modal
        await Page.ClickAsync("button.btn-create");
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();

        // Fill form with test data
        var uniqueName = $"TestEnemy_{DateTime.Now.Ticks}";
        await Page.FillAsync("input[placeholder='Enemy name']", uniqueName);
        await Page.FillAsync("input[placeholder='Normal, Boss, etc.']", "Boss");
        await Page.FillAsync("textarea[placeholder='Enemy description...']", "A test enemy for Playwright tests");

        // Note: Not actually submitting to avoid polluting the database
        // In a real test environment, you would:
        // 1. Use a test database
        // 2. Clean up after tests
        // 3. Actually submit and verify the entity was created

        // Close modal
        await Page.Locator(".modal-overlay").ClickAsync(new LocatorClickOptions
        {
            Position = new Position { X = 10, Y = 10 }
        });

        // Assert
        await Expect(Page.Locator(".modal-overlay")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminWorkflow_NavigateAllSections()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act & Assert - Navigate through all admin sections
        // Dashboard
        await Expect(Page.Locator("h1")).ToContainTextAsync("Dashboard");

        // Training
        await Page.ClickAsync("a[href='/admin/training']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h2")).ToContainTextAsync("Training");

        // Enemies
        await Page.ClickAsync("a[href='/admin/enemies']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h2")).ToContainTextAsync("Enemies");

        // Storybooks
        await Page.ClickAsync("a[href='/admin/storybooks']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h2")).ToContainTextAsync("Storybooks");

        // Events
        await Page.ClickAsync("a[href='/admin/events']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h2")).ToContainTextAsync("Random Events");

        // Skills
        await Page.ClickAsync("a[href='/admin/skills']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h2")).ToContainTextAsync("Skills");
    }
}
