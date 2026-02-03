namespace ChroniclesOfFate.Playwright.Tests.BackOffice;

/// <summary>
/// Tests for the Admin Skills management page.
/// </summary>
[TestFixture]
[Category("BackOffice")]
[Category("Skills")]
public class AdminSkillsTests : PlaywrightTestBase
{
    private async Task NavigateToSkillsPageAsync()
    {
        await LoginAsAdminAsync();
        await Page.GotoAsync($"{BlazorBaseUrl}/admin/skills");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task SkillsPage_ShouldDisplayTitle()
    {
        // Arrange & Act
        await NavigateToSkillsPageAsync();

        // Assert
        await Expect(Page.Locator("h2")).ToContainTextAsync("Skills");
    }

    [Test]
    public async Task SkillsPage_ShouldShowCreateButton()
    {
        // Arrange & Act
        await NavigateToSkillsPageAsync();

        // Assert
        await Expect(Page.Locator("button.btn-create")).ToContainTextAsync("New Skill");
    }

    [Test]
    public async Task SkillsPage_ShouldDisplayDataTable()
    {
        // Arrange & Act
        await NavigateToSkillsPageAsync();

        // Assert
        await Expect(Page.Locator("table.admin-table")).ToBeVisibleAsync();
        
        // Check table headers
        await Expect(Page.Locator("th:has-text('Name')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Type')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Rarity')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Effect')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Status')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Actions')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SkillsPage_ShouldOpenCreateModal_WhenClickingNewSkill()
    {
        // Arrange
        await NavigateToSkillsPageAsync();

        // Act
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
        await Expect(Page.Locator(".modal-content h2")).ToContainTextAsync("Create Skill");
    }

    [Test]
    public async Task CreateSkillModal_ShouldShowBasicFormFields()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator("input[placeholder='Skill name']")).ToBeVisibleAsync();
        await Expect(Page.Locator("textarea[placeholder='Describe the skill...']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[placeholder='https://...']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateSkillModal_ShouldShowSkillTypeDropdown()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator("label:has-text('Skill Type')")).ToBeVisibleAsync();
        var skillTypeSelect = Page.Locator("select").First;
        await Expect(skillTypeSelect).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateSkillModal_ShouldShowRarityDropdown()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        await Page.ClickAsync("button.btn-create");

        // Assert
        await Expect(Page.Locator("label:has-text('Rarity')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateSkillModal_ShouldShowPassiveFields_WhenPassiveTypeSelected()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        await Page.ClickAsync("button.btn-create");

        // Act - select Passive type (depends on current default)
        await Page.SelectOptionAsync("select >> nth=0", new[] { "Passive" });
        
        // Assert
        await Expect(Page.Locator("label:has-text('Passive Effect')")).ToBeVisibleAsync();
        await Expect(Page.Locator("label:has-text('Passive Value')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateSkillModal_ShouldShowActiveFields_WhenActiveTypeSelected()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        await Page.ClickAsync("button.btn-create");

        // Act - select Active type
        await Page.SelectOptionAsync("select >> nth=0", new[] { "Active" });
        
        // Assert
        await Expect(Page.Locator("label:has-text('Trigger Chance')")).ToBeVisibleAsync();
        await Expect(Page.Locator("label:has-text('Base Damage')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SkillsTable_ShouldShowRarityBadges()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        var tableRows = Page.Locator("table.admin-table tbody tr");
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount == 0)
        {
            Assert.Ignore("No skills exist to test rarity badges");
            return;
        }

        // Assert - check for rarity badges
        var rarityBadges = Page.Locator("table.admin-table tbody .badge");
        await Expect(rarityBadges.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task SkillsTable_ShouldShowEditAndDeleteButtons()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        var tableRows = Page.Locator("table.admin-table tbody tr");
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount > 0)
        {
            // Assert
            await Expect(tableRows.First.Locator("button.btn-edit")).ToBeVisibleAsync();
            await Expect(tableRows.First.Locator("button.btn-delete")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task SkillsTable_EditButton_ShouldOpenEditModal()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        var tableRows = Page.Locator("table.admin-table tbody tr");
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount == 0)
        {
            Assert.Ignore("No skills exist to test edit functionality");
            return;
        }

        // Act
        await tableRows.First.Locator("button.btn-edit").ClickAsync();

        // Assert
        await Expect(Page.Locator(".modal-overlay")).ToBeVisibleAsync();
        await Expect(Page.Locator(".modal-content h2")).ToContainTextAsync("Edit Skill");
    }

    [Test]
    public async Task SkillsPage_ShouldShowSkillTypes_InTable()
    {
        // Arrange
        await NavigateToSkillsPageAsync();
        var tableRows = Page.Locator("table.admin-table tbody tr");
        var rowCount = await tableRows.CountAsync();
        
        if (rowCount == 0)
        {
            Assert.Ignore("No skills exist to verify skill types in table");
            return;
        }

        // Assert - Type column should show skill types
        var firstRowTypeCells = tableRows.First.Locator("td >> nth=1");
        var typeText = await firstRowTypeCells.TextContentAsync();
        Assert.That(typeText, Is.Not.Empty, "Skill type should be displayed");
    }
}
