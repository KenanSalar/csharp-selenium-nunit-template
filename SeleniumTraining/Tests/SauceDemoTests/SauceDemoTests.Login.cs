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

        BasePage resultPage;

        const LoginMode loginMode = LoginMode.Click;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() }
        };
        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_Standard",
            Logger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps
        );
        bool loginStepSuccess = false;

        Logger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameStandardUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory);

            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);

            loginStepSuccess = resultPage is InventoryPage;
        }
        finally
        {
            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginStepSuccess ? 7000 : null);
            loginTimer.Dispose();
        }

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login should be successful and navigate to the Inventory Page.");
        Logger.LogInformation("Login successful, currently on InventoryPage.");

        var sortLoopTimer = new PerformanceTimer("TestStep_VerifyAllSortOptions", Logger);

        Logger.LogInformation(
            "Starting verification of product sorting options. Number of options to check: {SortOptionCount}",
            _inventoryProductsDropdownOptions.Count
        );

        try
        {
            foreach (KeyValuePair<SortByType, string> option in _inventoryProductsDropdownOptions)
            {
                inventoryPage = inventoryPage.SortProducts(option.Key, option.Value);

                if (option.Key == SortByType.Text)
                    inventoryPage.GetSelectedSortText().ShouldBe(option.Value);
                else if (option.Key == SortByType.Value)
                    inventoryPage.GetSelectedSortValue().ShouldBe(option.Value);
            }
        }
        finally
        {
            sortLoopTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 10000);
            sortLoopTimer.Dispose();
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

        const LoginMode loginMode = LoginMode.Submit;

        var loginAttemptProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() }
        };
        var loginAttemptTimer = new PerformanceTimer(
            "TestStep_UserLogin_Standard",
            Logger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginAttemptProps
        );
        bool loginAttemptSuccessAsExpected = false;

        BasePage resultPage;
        try
        {
            Logger.LogDebug("Instantiating LoginPage.");
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory);

            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameLockedOutUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);

            loginAttemptSuccessAsExpected = resultPage is LoginPage;
        }
        finally
        {
            loginAttemptTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginAttemptSuccessAsExpected ? 5000 : null);
            loginAttemptTimer.Dispose();
        }

        Logger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameLockedOutUser,
            loginMode
        );

        LoginPage loginPageInstance = resultPage.ShouldBeOfType<LoginPage>("User should have remained on the Login Page.");

        const string expectedErrorMessage = "Epic sadface: Sorry, this user has been locked out.";
        var errorMsgTimer = new PerformanceTimer("TestStep_GetLoginErrorMessage_LockedOut", Logger);
        string actualErrorMessage;
        try
        {
            actualErrorMessage = loginPageInstance.GetErrorMessage();
        }
        finally
        {
            errorMsgTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 1000);
            errorMsgTimer.Dispose();
        }

        actualErrorMessage.ShouldBe(expectedErrorMessage, $"Error message should be: {expectedErrorMessage} but was: {actualErrorMessage}");

        Logger.LogInformation("Login not successful, currently on LoginPage.");
        Logger.LogInformation("Finished test: {TestName}", currentTestName);
    }
}
