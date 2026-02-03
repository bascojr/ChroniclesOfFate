# Chronicles of Fate - Playwright UI Tests

This project contains end-to-end UI tests for the Chronicles of Fate application using Playwright with NUnit, following Playwright best practices.

## Playwright Best Practices Used

This test project follows the [official Playwright best practices](https://playwright.dev/dotnet/docs/locators):

### Semantic Locators (Recommended)
- **`GetByRole()`** - Locate by ARIA role (buttons, headings, links, etc.)
- **`GetByText()`** - Locate by visible text content
- **`GetByLabel()`** - Locate form controls by label
- **`GetByPlaceholder()`** - Locate inputs by placeholder text
- **`GetByTestId()`** - Locate by `data-testid` attribute

### Why Semantic Locators?
1. **Resilient to DOM changes** - Tests won't break if CSS classes change
2. **Accessible by design** - Tests verify the page is accessible
3. **User-centric** - Tests interact like real users do
4. **Auto-waiting** - Playwright waits for elements automatically

### Example Usage
```csharp
// Good - Semantic locators
await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
await Page.GetByPlaceholder("Enter username").FillAsync("user");
await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })).ToBeVisibleAsync();

// Avoid - CSS selectors (when possible)
await Page.Locator(".btn-primary").ClickAsync();
await Page.Locator("#username").FillAsync("user");
```

## Prerequisites

1. **.NET 8 SDK** - Required to build and run the tests
2. **Playwright browsers** - Automatically installed on first run

## Setup

### 1. Install Playwright Browsers

After building the project, run the Playwright install command:

```powershell
# Navigate to the test project
cd tests/ChroniclesOfFate.Playwright.Tests

# Build the project
dotnet build

# Install Playwright browsers
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### 2. Start the Application

Before running tests, ensure both the API and Blazor applications are running:

```powershell
# Terminal 1 - Start API
cd src/ChroniclesOfFate.API
dotnet run

# Terminal 2 - Start Blazor
cd src/ChroniclesOfFate.Blazor
dotnet run
```

Default URLs:
- **API**: https://localhost:57777
- **Blazor**: https://localhost:57778

### 3. Configure Test Users

Make sure the following users exist in your database:
- **Test User**: `testuser` / `TestPassword123!`
- **Admin User**: `admin` / `Admin123!`

## Running Tests

### Run All Tests

```powershell
dotnet test
```

### Run Tests with Specific Category

```powershell
# Frontend tests only
dotnet test --filter "Category=Frontend"

# Back office tests only
dotnet test --filter "Category=BackOffice"

# Integration tests only
dotnet test --filter "Category=Integration"
```

### Run Tests in Headed Mode (Visible Browser)

```powershell
$env:HEADED="1"
dotnet test
```

### Run with Slow Motion (for debugging)

```powershell
$env:SLOWMO="500"
dotnet test
```

## Test Structure

```
ChroniclesOfFate.Playwright.Tests/
├── Frontend/                    # Frontend UI tests
│   ├── LoginPageTests.cs       # Login/Registration page tests
│   └── SessionsPageTests.cs    # Sessions page tests
├── BackOffice/                  # Admin panel tests
│   ├── AdminDashboardTests.cs  # Dashboard tests
│   ├── AdminEnemiesTests.cs    # Enemies management tests
│   ├── AdminSkillsTests.cs     # Skills management tests
│   ├── AdminNavigationTests.cs # Navigation tests
│   └── AdminOtherPagesTests.cs # Storybooks, Events, Training tests
├── Integration/                 # End-to-end workflow tests
│   └── UserWorkflowTests.cs    # Complete user journey tests
├── PlaywrightTestBase.cs       # Base test class with utilities
├── TestConfig.cs               # Test configuration constants
└── playwright.runsettings      # Playwright run settings
```

## Test Categories

- **Frontend** - Tests for user-facing pages (Login, Sessions, Game)
- **BackOffice** - Tests for admin panel functionality
- **Integration** - End-to-end workflow tests
- **Login** - Specific to login functionality
- **Sessions** - Specific to sessions page
- **AdminDashboard** - Admin dashboard tests
- **Enemies** - Enemy management tests
- **Skills** - Skill management tests
- **Navigation** - Navigation and layout tests

## Configuration

Edit `TestConfig.cs` to change:
- Base URLs for API and Blazor
- Test user credentials
- Timeout settings

## Troubleshooting

### Browser Installation Issues

```powershell
# Force reinstall browsers
pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
```

### SSL Certificate Errors

The tests are configured to ignore HTTPS errors for local development. If you still encounter issues, ensure you trust the development certificates:

```powershell
dotnet dev-certs https --trust
```

### Tests Timing Out

Increase timeouts in `TestConfig.cs` or check that the application is running and accessible.

## Writing New Tests

1. Inherit from `PlaywrightTestBase`
2. Use semantic locators (`GetByRole`, `GetByPlaceholder`, `GetByText`, etc.)
3. Use Playwright's `Expect()` assertions
4. Add appropriate `[Category]` attributes

Example:

```csharp
[TestFixture]
[Category("Frontend")]
public class MyNewTests : PlaywrightTestBase
{
    [Test]
    public async Task MyTest_ShouldWork()
    {
        await Page.GotoAsync(BlazorBaseUrl);
        
        // Use semantic locators
        await Page.GetByPlaceholder("Enter username").FillAsync("user");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        
        // Use Expect for assertions
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome" })).ToBeVisibleAsync();
    }
}
```

## Locator Priority Guide

When choosing locators, follow this priority:

1. **`GetByRole()`** - Best for buttons, links, headings, inputs
2. **`GetByLabel()`** - Best for form inputs with labels
3. **`GetByPlaceholder()`** - Best for inputs without labels
4. **`GetByText()`** - Best for non-interactive elements (divs, spans)
5. **`GetByTestId()`** - Use when other methods don't work
6. **CSS/XPath** - Last resort, use only when necessary
