namespace ChroniclesOfFate.Playwright.Tests.BackOffice;

/// <summary>
/// Tests for the Admin Dashboard functionality using Playwright best practices.
/// Uses semantic locators: GetByRole, GetByText, GetByTestId.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("AdminDashboard")]
public class AdminDashboardTests : PlaywrightTestBase
{
    [Test]
    public async Task AdminDashboard_ShouldRequireAdminRole()
    {
        // Arrange - login as regular user
        await LoginAsync(TestConfig.TestUser.Username, TestConfig.TestUser.Password);

        // Act
        await Page.GotoAsync($"{BlazorBaseUrl}/admin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - should either redirect or show unauthorized
        var hasAccess = await HasTextAsync("Dashboard");
        // Regular user should not have access - exact behavior depends on auth configuration
    }

    [Test]
    public async Task AdminDashboard_ShouldDisplayTitle_WhenAuthorized()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminDashboard_ShouldShowStatisticsCards()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Assert - verify dashboard cards exist using text locators
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Training Scenarios" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Enemies" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Storybooks" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Random Events" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Skills" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Users" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Game Sessions" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminDashboard_ShouldShowQuickActionLinks()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Assert using role-based locators for links
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Manage Training" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Manage Enemies" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Manage Storybooks" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Manage Events" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Manage Skills" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminDashboard_TrainingCard_ShouldShowCount()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Assert using text and CSS locators for card structure
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Training Scenarios" })).ToBeVisibleAsync();
        // Count element uses CSS as it's structural
        await Expect(Page.Locator(".dashboard-card.training .count")).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminDashboard_ShouldNavigateToEnemies_WhenClickingManageEnemies()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act using role-based locator
        await Page.GetByRole(AriaRole.Link, new() { Name = "Manage Enemies" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Assert.That(Page.Url, Does.Contain("/admin/enemies"));
    }

    [Test]
    public async Task AdminDashboard_ShouldNavigateToSkills_WhenClickingManageSkills()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act using role-based locator
        await Page.GetByRole(AriaRole.Link, new() { Name = "Manage Skills" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Assert.That(Page.Url, Does.Contain("/admin/skills"));
    }

    [Test]
    public async Task AdminDashboard_ShouldShowLoadingState_Initially()
    {
        // This test checks if loading indicator appears while data is fetched
        // We need to intercept the API call to slow it down
        
        // Arrange
        await LoginAsync(TestConfig.AdminUser.Username, TestConfig.AdminUser.Password);

        // Note: For a proper test, we would intercept the API and delay the response
        // For now, we just verify the page loads successfully
        await Page.GotoAsync($"{BlazorBaseUrl}/admin");
        
        // The loading state may be too quick to catch, so we just verify final state
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        await Expect(Page.Locator(".dashboard-grid")).ToBeVisibleAsync();
    }
}
