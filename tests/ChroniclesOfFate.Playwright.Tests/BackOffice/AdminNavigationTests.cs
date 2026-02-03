namespace ChroniclesOfFate.Playwright.Tests.BackOffice;

/// <summary>
/// Tests for the Admin Layout navigation and structure.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("Navigation")]
public class AdminNavigationTests : PlaywrightTestBase
{
    [Test]
    public async Task AdminLayout_ShouldShowNavigationLinks()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Assert - Check for navigation links in sidebar/header
        // The exact selectors depend on AdminLayout.razor structure
        await Expect(Page.Locator("a[href='/admin']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminNavigation_ShouldNavigateBetweenPages()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act & Assert - Navigate through all admin pages
        var adminPages = new[]
        {
            "/admin",
            "/admin/training",
            "/admin/enemies",
            "/admin/storybooks",
            "/admin/events",
            "/admin/skills"
        };

        foreach (var pagePath in adminPages)
        {
            await Page.GotoAsync($"{BlazorBaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Verify page loaded (no error)
            Assert.That(Page.Url, Does.Contain(pagePath), $"Should be on {pagePath}");
        }
    }

    [Test]
    public async Task AdminPages_ShouldMaintainLayoutConsistency()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act - Visit each page and verify consistent layout
        var pagesToCheck = new[]
        {
            "/admin/enemies",
            "/admin/skills",
            "/admin/storybooks"
        };

        foreach (var pagePath in pagesToCheck)
        {
            await Page.GotoAsync($"{BlazorBaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Consistent elements should exist
            await Expect(Page.Locator(".admin-table-container")).ToBeVisibleAsync();
            await Expect(Page.Locator("button.btn-create")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task AdminPages_ShouldHaveConsistentTableStructure()
    {
        // Arrange
        await LoginAsAdminAsync();

        var pagesToCheck = new[]
        {
            "/admin/enemies",
            "/admin/skills"
        };

        foreach (var pagePath in pagesToCheck)
        {
            await Page.GotoAsync($"{BlazorBaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Table structure should be consistent
            await Expect(Page.Locator("table.admin-table thead")).ToBeVisibleAsync();
            await Expect(Page.Locator("table.admin-table tbody")).ToBeVisibleAsync();
        }
    }
}
