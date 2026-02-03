namespace ChroniclesOfFate.Playwright.Tests.BackOffice;

/// <summary>
/// Tests for the Admin Enemies management page using Playwright best practices.
/// Uses semantic locators: GetByRole, GetByPlaceholder, GetByText, GetByLabel.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("Enemies")]
public class AdminEnemiesTests : PlaywrightTestBase
{
    private async Task NavigateToEnemiesPageAsync()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/enemies");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task EnemiesPage_ShouldDisplayTitle()
    {
        // Arrange & Act
        await NavigateToEnemiesPageAsync();

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Enemies" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesPage_ShouldShowCreateButton()
    {
        // Arrange & Act
        await NavigateToEnemiesPageAsync();

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesPage_ShouldDisplayDataTable()
    {
        // Arrange & Act
        await NavigateToEnemiesPageAsync();

        // Assert using role-based locators for table structure
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
        
        // Check table headers using role and column headers
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Name" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Type" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Tier" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Stats" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Rewards" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Status" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Actions" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesPage_ShouldOpenCreateModal_WhenClickingNewEnemy()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();

        // Act using role-based locator
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" }).ClickAsync();

        // Assert using role-based locator for modal heading
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Create Enemy" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateEnemyModal_ShouldShowAllFormFields()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" }).ClickAsync();

        // Assert using placeholder locators
        await Expect(Page.GetByPlaceholder("Enemy name")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Normal, Boss, etc.")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enemy description...")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("https://...")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateEnemyModal_ShouldShowStatFields()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" }).ClickAsync();

        // Assert - check for stat input fields using GetByText for labels
        await Expect(Page.GetByText("Strength")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Agility")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Intelligence")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Endurance")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Health")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateEnemyModal_ShouldShowRewardFields()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" }).ClickAsync();

        // Assert using GetByText for labels
        await Expect(Page.GetByText("Experience Reward")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Gold Reward")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Reputation Reward")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateEnemyModal_ShouldClose_WhenClickingOverlay()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ New Enemy" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Create Enemy" })).ToBeVisibleAsync();

        // Act - click on overlay (outside modal content) using CSS for overlay
        await Page.Locator(".modal-overlay").ClickAsync(new LocatorClickOptions
        {
            Position = new Position { X = 10, Y = 10 }
        });

        // Assert - modal should be closed
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Create Enemy" })).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesTable_ShouldShowEditAndDeleteButtons()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();

        // Assert - check if any enemies exist and have action buttons
        var tableRows = Page.GetByRole(AriaRole.Row);
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount > 1) // More than header row
        {
            // Use role-based locators for buttons
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).First).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task EnemiesTable_EditButton_ShouldOpenEditModal()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        var editButtons = Page.GetByRole(AriaRole.Button, new() { Name = "Edit" });
        var buttonCount = await editButtons.CountAsync();
        
        if (buttonCount == 0)
        {
            Assert.Ignore("No enemies exist to test edit functionality");
            return;
        }

        // Act - Click first Edit button
        await editButtons.First.ClickAsync();

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Edit Enemy" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesTable_DeleteButton_ShouldShowConfirmation()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        var deleteButtons = Page.GetByRole(AriaRole.Button, new() { Name = "Delete" });
        var buttonCount = await deleteButtons.CountAsync();
        
        if (buttonCount == 0)
        {
            Assert.Ignore("No enemies exist to test delete functionality");
            return;
        }

        // Act - Click first Delete button
        await deleteButtons.First.ClickAsync();

        // Assert - should show delete confirmation modal
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnemiesTable_ShouldDisplayStatusBadges()
    {
        // Arrange
        await NavigateToEnemiesPageAsync();
        var tableRows = Page.GetByRole(AriaRole.Row);
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount <= 1) // Only header row
        {
            Assert.Ignore("No enemies exist to test status badges");
            return;
        }

        // Assert - check for status badges (Active or Inactive text)
        var activeOrInactive = Page.GetByText("Active").Or(Page.GetByText("Inactive"));
        await Expect(activeOrInactive.First).ToBeVisibleAsync();
    }
}
