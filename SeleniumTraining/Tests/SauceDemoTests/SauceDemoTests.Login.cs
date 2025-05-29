namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    [Test]
    [Retry(2)]
    [AllureStep("Login with Login Button Click and Sort Products for standard_user")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies user login with the standard_user, using the Click action and then sorts products by all available options.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldLoginSuccessfullyWithStandardUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        Logger.LogInformation("Starting test: {TestName}", currentTestName);

        Logger.LogDebug("Instantiating LoginPage.");
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory);

        const LoginMode loginMode = LoginMode.Click;

        Logger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameStandardUser,
            loginMode
        );
        BasePage resultPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(loginMode);
        Logger.LogInformation("Login successful, currently on InventoryPage.");

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login should be successful and navigate to the Inventory Page.");

        Logger.LogInformation(
            "Starting verification of product sorting options. Number of options to check: {SortOptionCount}",
            _inventoryProductsDropdownOptions.Count
        );
        foreach (KeyValuePair<SortByType, string> option in _inventoryProductsDropdownOptions)
        {
            inventoryPage = inventoryPage.SortProducts(option.Key, option.Value);

            if (option.Key == SortByType.Text)
                inventoryPage.GetSelectedSortText().ShouldBe(option.Value);
            else if (option.Key == SortByType.Value)
                inventoryPage.GetSelectedSortValue().ShouldBe(option.Value);
        }
        Logger.LogInformation("All product sorting options verified successfully.");

        Logger.LogInformation("Finished test: {TestName}", currentTestName);
    }

    [Test]
    [Retry(2)]
    [AllureStep("Login with Submit for locked_out_user")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies user login with the locked_out_user, using the Submit action.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldNotLoginSuccessfullyWithLockedOutUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        Logger.LogInformation("Starting test: {TestName}", currentTestName);

        Logger.LogDebug("Instantiating LoginPage.");
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory);

        const LoginMode loginMode = LoginMode.Submit;

        Logger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameLockedOutUser,
            loginMode
        );
        BasePage resultPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameLockedOutUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(loginMode);

        _ = resultPage.ShouldBeOfType<LoginPage>("User should have remained on the Login Page.");

        const string expectedErrorMessage = "Epic sadface: Sorry, this user has been locked out.";
        string actualErrorMessage = ((LoginPage)resultPage).GetErrorMessage();
        actualErrorMessage.ShouldBe(expectedErrorMessage, $"Error message should be: {expectedErrorMessage}");

        Logger.LogInformation("Login not successful, currently on LoginPage.");

        Logger.LogInformation("Finished test: {TestName}", currentTestName);
    }
}
