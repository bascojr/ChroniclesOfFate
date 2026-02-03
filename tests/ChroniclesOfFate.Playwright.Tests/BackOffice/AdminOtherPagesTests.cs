namespace ChroniclesOfFate.Playwright.Tests.BackOffice;

/// <summary>
/// Tests for the Admin Storybooks management page.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("Storybooks")]
public class AdminStorybooksTests : PlaywrightTestBase
{
    private async Task NavigateToStorybooksPageAsync()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/storybooks");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task StorybooksPage_ShouldDisplayTitle()
    {
        // Arrange & Act
        await NavigateToStorybooksPageAsync();

        // Assert
        await Expect(Page.Locator("h2")).ToContainTextAsync("Storybooks");
    }

    [Test]
    public async Task StorybooksPage_ShouldShowCreateButton()
    {
        // Arrange & Act
        await NavigateToStorybooksPageAsync();

        // Assert
        var createButton = Page.Locator("button.btn-create");
        await Expect(createButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task StorybooksPage_ShouldDisplayDataTable()
    {
        // Arrange & Act
        await NavigateToStorybooksPageAsync();

        // Assert
        await Expect(Page.Locator("table.admin-table")).ToBeVisibleAsync();
    }

    [Test]
    public async Task StorybooksPage_ShouldOpenCreateModal_WhenClickingCreate()
    {
        // Arrange
        await NavigateToStorybooksPageAsync();

        // Act
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
    }

    [Test]
    public async Task StorybooksTable_ShouldHaveEditAndDeleteActions()
    {
        // Arrange
        await NavigateToStorybooksPageAsync();
        var tableRows = Page.Locator("table.admin-table tbody tr");
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount > 0)
        {
            // Assert
            await Expect(tableRows.First.Locator("button.btn-edit")).ToBeVisibleAsync();
            await Expect(tableRows.First.Locator("button.btn-delete")).ToBeVisibleAsync();
        }
    }
}

/// <summary>
/// Tests for the Admin Random Events management page.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("RandomEvents")]
public class AdminRandomEventsTests : PlaywrightTestBase
{
    private async Task NavigateToEventsPageAsync()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/events");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task EventsPage_ShouldDisplayTitle()
    {
        // Arrange & Act
        await NavigateToEventsPageAsync();

        // Assert
        await Expect(Page.Locator("h2")).ToContainTextAsync("Random Events");
    }

    [Test]
    public async Task EventsPage_ShouldShowCreateButton()
    {
        // Arrange & Act
        await NavigateToEventsPageAsync();

        // Assert
        var createButton = Page.Locator("button.btn-create");
        await Expect(createButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task EventsPage_ShouldDisplayDataTable()
    {
        // Arrange & Act
        await NavigateToEventsPageAsync();

        // Assert
        await Expect(Page.Locator("table.admin-table")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EventsPage_ShouldOpenCreateModal_WhenClickingCreate()
    {
        // Arrange
        await NavigateToEventsPageAsync();

        // Act
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
    }
}

/// <summary>
/// Tests for the Admin Training Scenarios management page.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("TrainingScenarios")]
public class AdminTrainingTests : PlaywrightTestBase
{
    private async Task NavigateToTrainingPageAsync()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/training");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task TrainingPage_ShouldDisplayTitle()
    {
        // Arrange & Act
        await NavigateToTrainingPageAsync();

        // Assert
        await Expect(Page.Locator("h2")).ToContainTextAsync("Training");
    }

    [Test]
    public async Task TrainingPage_ShouldShowCreateButton()
    {
        // Arrange & Act
        await NavigateToTrainingPageAsync();

        // Assert
        var createButton = Page.Locator("button.btn-create");
        await Expect(createButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task TrainingPage_ShouldDisplayDataTable()
    {
        // Arrange & Act
        await NavigateToTrainingPageAsync();

        // Assert
        await Expect(Page.Locator("table.admin-table")).ToBeVisibleAsync();
    }

    [Test]
    public async Task TrainingPage_ShouldOpenCreateModal_WhenClickingCreate()
    {
        // Arrange
        await NavigateToTrainingPageAsync();

        // Act
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
    }
}
