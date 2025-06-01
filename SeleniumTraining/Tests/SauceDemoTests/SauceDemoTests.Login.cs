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
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        BasePage resultPage;

        const LoginMode loginMode = LoginMode.Click;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() }
        };
        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_Standard",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps
        );
        bool loginStepSuccess = false;

        TestLogger.LogInformation(
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
        TestLogger.LogInformation("Login successful, currently on InventoryPage.");

        var sortLoopTimer = new PerformanceTimer("TestStep_VerifyAllSortOptions", TestLogger);

        TestLogger.LogInformation(
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
        TestLogger.LogInformation("All product sorting options verified successfully.");

        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
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
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        const LoginMode loginMode = LoginMode.Submit;

        var loginAttemptProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameStandardUser },
            { "LoginAction", LoginMode.Click.ToString() }
        };
        var loginAttemptTimer = new PerformanceTimer(
            "TestStep_UserLogin_Standard",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginAttemptProps
        );
        bool loginAttemptSuccessAsExpected = false;

        BasePage resultPage;
        try
        {
            TestLogger.LogDebug("Instantiating LoginPage.");
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

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameLockedOutUser,
            loginMode
        );

        LoginPage loginPageInstance = resultPage.ShouldBeOfType<LoginPage>("User should have remained on the Login Page.");

        const string expectedErrorMessage = "Epic sadface: Sorry, this user has been locked out.";
        var errorMsgTimer = new PerformanceTimer("TestStep_GetLoginErrorMessage_LockedOut", TestLogger);
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

        TestLogger.LogInformation("Login not successful, currently on LoginPage.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }

    [Test]
    [Retry(2)]
    [AllureStep("Verify Visuals on Inventory Page for visual_user")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureDescription("Verifies the visual appearance of the inventory page when logged in as the visual_user, checking for known or unexpected visual discrepancies.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldDetectVisualAnomaliesForVisualUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting visual test: {TestName} for visual_user", currentTestName);

        // 1. Instantiate LoginPage
        TestLogger.LogDebug("Instantiating LoginPage for {TestName}.", currentTestName);
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory);

        // 2. Login as visual_user
        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} (visual_user) using Click action for {TestName}.",
            _sauceDemoSettings.LoginUsernameVisualUser,
            currentTestName
        );
        BasePage resultPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameVisualUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click);

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login as visual_user should navigate to the Inventory Page.");
        TestLogger.LogInformation("Successfully logged in as visual_user and navigated to InventoryPage for {TestName}.", currentTestName);

        try
        {
            TestLogger.LogTrace("Pausing briefly to allow visual_user glitches to manifest (500ms) for {TestName}.", currentTestName);
            Thread.Sleep(1000);
        }
        catch (ThreadInterruptedException tie)
        {
            TestLogger.LogWarning(tie, "Thread.Sleep was interrupted during visual test setup for {TestName}.", currentTestName);
            Thread.CurrentThread.Interrupt();
        }

        string fullPageBaselineId = "InventoryPage_VisualUser_FullPage";
        TestLogger.LogInformation(
            "Performing full-page visual assertion for ID '{BaselineID}' in test '{TestName}'.",
            fullPageBaselineId,
            currentTestName
        );
        VisualTester.AssertVisualMatch(
            baselineIdentifier: fullPageBaselineId,
            testName: TestName,
            browserType: BrowserType
        );
        TestLogger.LogInformation("Full-page visual assertion completed for ID '{BaselineID}' in {TestName}.", fullPageBaselineId, currentTestName);

        string boltTShirtImageBaselineId = "InventoryPage_VisualUser_BoltTShirtImage";
        try
        {

            TestLogger.LogDebug("Attempting to locate the 'Sauce Labs Bolt T-Shirt' image element for visual check in {TestName}.", currentTestName);

            InventoryItemComponent? boltTShirtComponent = inventoryPage.GetInventoryItems()
                .FirstOrDefault(item => item.ItemName == "Sauce Labs Bolt T-Shirt");

            By boltTShirtImageLocator = SmartLocators.DataTest("inventory-item-sauce-labs-bolt-t-shirt-img");
            IWebElement? boltTShirtImageElement = null;
            try
            {
                boltTShirtImageElement = WebDriverManager.GetDriver().FindElement(boltTShirtImageLocator);
            }
            catch (NoSuchElementException nseEx)
            {
                TestLogger.LogWarning(nseEx, "Could not find the 'Sauce Labs Bolt T-Shirt' image using locator {Locator} for specific visual check in {TestName}. Skipping element-specific visual check.", boltTShirtImageLocator, currentTestName);
            }


            if (boltTShirtImageElement != null && boltTShirtImageElement.Displayed)
            {
                TestLogger.LogInformation(
                    "Performing element-specific visual assertion for ID '{BaselineID}' (Bolt T-Shirt Image) in test '{TestName}'.",
                    boltTShirtImageBaselineId,
                    currentTestName
                );
                VisualTester.AssertVisualMatch(
                    baselineIdentifier: boltTShirtImageBaselineId,
                    testName: TestName,
                    browserType: BrowserType,
                    elementToCapture: boltTShirtImageElement
                );
                TestLogger.LogInformation(
                    "Element-specific visual assertion completed for ID '{BaselineID}' in {TestName}.",
                    boltTShirtImageBaselineId,
                    currentTestName
                );
            }
            else if (boltTShirtImageElement != null && !boltTShirtImageElement.Displayed)
            {
                TestLogger.LogWarning("'Sauce Labs Bolt T-Shirt' image found but was not displayed. Skipping element-specific visual check for {BaselineID} in {TestName}.", boltTShirtImageBaselineId, currentTestName);
            }
        }
        catch (Exception ex)
        {
            TestLogger.LogError(
                ex,
                "An error occurred during the element-specific visual check for '{BaselineID}' in {TestName}. This check will be skipped.",
                boltTShirtImageBaselineId,
                currentTestName
            );
        }

        TestLogger.LogInformation("Finished visual test: {TestName} for visual_user", currentTestName);
    }
}
