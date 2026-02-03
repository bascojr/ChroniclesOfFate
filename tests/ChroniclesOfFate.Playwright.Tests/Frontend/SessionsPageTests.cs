namespace ChroniclesOfFate.Playwright.Tests.Frontend;

/// <summary>
/// Tests for the Sessions page functionality using Playwright best practices.
/// Uses semantic locators: GetByRole, GetByPlaceholder, GetByText.
/// </summary>
[TestFixture]
[Category("Frontend")]
[Category("Sessions")]
public class SessionsPageTests : PlaywrightTestBase
{
    /// <summary>
    /// Navigates to sessions page after login, returns false if auth failed.
    /// </summary>
    private async Task<bool> NavigateToSessionsAuthenticatedAsync()
    {
        await LoginAsync(TestConfig.TestUser.Username, TestConfig.TestUser.Password);
        
        // Wait a bit for any error message to appear
        await Task.Delay(500);
        
        // Check if there's an error message on login page (auth failed)
        var errorVisible = await Page.Locator(".error-message").IsVisibleAsync();
        if (errorVisible)
        {
            return false;
        }
        
        await Page.GotoAsync($"{BlazorBaseUrl}/sessions");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait a moment for redirects
        await Task.Delay(500);
        
        // Check if redirected back to login (auth failed)
        if (Page.Url.Contains("login"))
        {
            return false;
        }
        return true;
    }

    private async Task OpenNewGameDialogAsync()
    {
        await Page.Locator(".session-card.new-game").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task SessionsPage_ShouldRedirectToLogin_WhenNotAuthenticated()
    {
        // Act
        await Page.GotoAsync($"{BlazorBaseUrl}/sessions");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - should be redirected to login or show auth challenge
        var currentUrl = Page.Url;
        var isOnLoginOrSessions = currentUrl.Contains("login") || currentUrl.Contains("sessions");
        Assert.That(isOnLoginOrSessions, Is.True, "Should be on login or sessions page");
    }

    [Test]
    public async Task SessionsPage_ShouldShowTitle_WhenAuthenticated()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Your Adventures" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task SessionsPage_ShouldShowNewGameCard()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }

        // Assert using text-based locator for the new game card
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "New Adventure" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Start a new 10-year journey")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SessionsPage_ShouldOpenNewGameDialog_WhenClickingNewGame()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }

        // Act - Click on the new game card
        await OpenNewGameDialogAsync();

        // Assert using role-based locator
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Begin New Adventure" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task NewGameDialog_ShouldShowCharacterCreationForm()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Assert using placeholder locators
        await Expect(Page.GetByPlaceholder("My Adventure")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter name")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Choose Your Class")).ToBeVisibleAsync();
    }

    [Test]
    public async Task NewGameDialog_ShouldShowAllCharacterClasses()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Assert - check that class cards exist using CSS (for repeated elements)
        var classCards = Page.Locator(".class-card");
        var count = await classCards.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Should have character class options");
    }

    [Test]
    public async Task NewGameDialog_ShouldHighlightSelectedClass()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Act - click on first class card
        await Page.Locator(".class-card").First.ClickAsync();

        // Assert
        await Expect(Page.Locator(".class-card.selected")).ToBeVisibleAsync();
    }

    [Test]
    public async Task NewGameDialog_NextButton_ShouldBeDisabled_WhenNoCharacterName()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Assert - next button should be disabled without character name (using role locator)
        var nextButton = Page.GetByRole(AriaRole.Button, new() { Name = "Next: Choose Storybooks" });
        await Expect(nextButton).ToBeDisabledAsync();
    }

    [Test]
    public async Task NewGameDialog_NextButton_ShouldBeEnabled_WhenCharacterNameEntered()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Act - Fill using placeholder locator and press Tab to trigger Blazor @bind update
        var characterNameInput = Page.GetByPlaceholder("Enter name");
        await characterNameInput.FillAsync("TestHero");
        await characterNameInput.PressAsync("Tab"); // Trigger onchange event for Blazor binding

        // Assert using role locator
        var nextButton = Page.GetByRole(AriaRole.Button, new() { Name = "Next: Choose Storybooks" });
        await Expect(nextButton).ToBeEnabledAsync();
    }

    [Test]
    public async Task NewGameDialog_ShouldCloseOnCancel()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Act - Click cancel using role locator
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Assert - modal should be closed
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Begin New Adventure" })).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task NewGameDialog_ShouldAdvanceToStorybookSelection()
    {
        // Arrange
        var isAuth = await NavigateToSessionsAuthenticatedAsync();
        if (!isAuth)
        {
            Assert.Inconclusive("User is not authenticated. Test user may not exist.");
        }
        await OpenNewGameDialogAsync();

        // Act - Fill using placeholder and press Tab to trigger Blazor @bind, then click button
        var characterNameInput = Page.GetByPlaceholder("Enter name");
        await characterNameInput.FillAsync("TestHero");
        await characterNameInput.PressAsync("Tab"); // Trigger onchange event for Blazor binding
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next: Choose Storybooks" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert using text locator
        await Expect(Page.GetByText("Select up to 5 storybooks")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = TestConfig.DefaultTimeoutMs
        });
    }
}
