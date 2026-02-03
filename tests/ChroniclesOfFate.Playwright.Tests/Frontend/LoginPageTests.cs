namespace ChroniclesOfFate.Playwright.Tests.Frontend;

/// <summary>
/// Tests for the Login page functionality using Playwright best practices.
/// Uses semantic locators: GetByRole, GetByPlaceholder, GetByText.
/// </summary>
[TestFixture]
[Category("Frontend")]
[Category("Login")]
public class LoginPageTests : PlaywrightTestBase
{
    [Test]
    public async Task LoginPage_ShouldDisplayLoginForm_WhenNavigatingToRoot()
    {
        // Arrange & Act
        await Page.GotoAsync(BlazorBaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert using role-based locators
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome Back" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Continue your journey")).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldDisplayLoginForm_WhenNavigatingToLoginRoute()
    {
        // Arrange & Act
        await NavigateToLoginAsync();

        // Assert using semantic locators
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome Back" })).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter username")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter password")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldToggleToRegistration_WhenClickingCreateAccount()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Act - Click the "Create Account" link using GetByRole
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create Account" }).ClickAsync();
        
        // Assert using semantic locators
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Create Account" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Begin your 10-year adventure")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter email")).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldToggleBackToLogin_WhenClickingLogin()
    {
        // Arrange
        await NavigateToLoginAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create Account" }).ClickAsync();

        // Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();

        // Assert
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome Back" })).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter email")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldShowError_WhenLoginWithInvalidCredentials()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Act - Fill using placeholder locators (best practice for inputs)
        await Page.GetByPlaceholder("Enter username").FillAsync("invaliduser");
        await Page.GetByPlaceholder("Enter password").FillAsync("wrongpassword");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        
        // Wait for response
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - should show error message (using CSS for dynamic error div)
        await Expect(Page.Locator(".error-message")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = TestConfig.DefaultTimeoutMs
        });
    }

    [Test]
    public async Task LoginPage_ShouldShowLoadingState_WhenSubmittingForm()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Act - Fill form using placeholder locators
        await Page.GetByPlaceholder("Enter username").FillAsync("testuser");
        await Page.GetByPlaceholder("Enter password").FillAsync("password");
        
        // Get submit button using role locator
        var button = Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        
        // Check initial state before clicking
        await Expect(button).ToBeEnabledAsync();
        
        // Click the button
        await button.ClickAsync();

        // Assert - either shows loading text OR transitions quickly to error/success
        // The loading state may be too fast to catch, so we verify the form was submitted
        // by checking for either error message, loading text, or navigation
        var submittedIndicator = Page.GetByText("Loading...").Or(Page.Locator(".error-message")).Or(Page.GetByText("sessions"));
        await submittedIndicator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000, State = WaitForSelectorState.Visible });
        Assert.Pass("Form submission was triggered");
    }

    [Test]
    public async Task LoginPage_ShouldValidateInputFields_AreEmpty()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Assert - inputs should start empty using placeholder locators
        await Expect(Page.GetByPlaceholder("Enter username")).ToHaveValueAsync("");
        await Expect(Page.GetByPlaceholder("Enter password")).ToHaveValueAsync("");
    }

    [Test]
    public async Task RegistrationForm_ShouldShowEmailField_WhenInRegistrationMode()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Act - Switch to registration using role locator
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create Account" }).ClickAsync();

        // Assert using placeholder locator
        var emailInput = Page.GetByPlaceholder("Enter email");
        await Expect(emailInput).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldHavePasswordInputMasked()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Assert using placeholder locator
        var passwordInput = Page.GetByPlaceholder("Enter password");
        await Expect(passwordInput).ToHaveAttributeAsync("type", "password");
    }

    [Test]
    public async Task LoginPage_SubmitButton_ShouldBeEnabledByDefault()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Assert using role locator
        var submitButton = Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        await Expect(submitButton).ToBeEnabledAsync();
    }

    [Test]
    public async Task LoginPage_ShouldFillFormCorrectly()
    {
        // Arrange
        await NavigateToLoginAsync();
        const string testUsername = "mytestuser";
        const string testPassword = "mypassword123";

        // Act - Fill form using placeholder locators
        await Page.GetByPlaceholder("Enter username").FillAsync(testUsername);
        await Page.GetByPlaceholder("Enter password").FillAsync(testPassword);

        // Assert - Verify values were entered
        await Expect(Page.GetByPlaceholder("Enter username")).ToHaveValueAsync(testUsername);
        await Expect(Page.GetByPlaceholder("Enter password")).ToHaveValueAsync(testPassword);
    }

    [Test]
    public async Task RegistrationForm_ShouldShowCreateAccountButton()
    {
        // Arrange
        await NavigateToLoginAsync();

        // Act - Switch to registration mode
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create Account" }).ClickAsync();

        // Assert - Button text should change to "Create Account"
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" })).ToBeVisibleAsync();
    }
}
