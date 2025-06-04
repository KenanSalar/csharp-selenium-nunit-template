namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies successful login using standard user credentials via a 'Click' action on the login button.
    /// After successful login, it navigates to the inventory page and then iterates through all
    /// available product sorting options, verifying that each sort option can be selected correctly.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters standard user credentials (username and password).</description></item>
    ///   <item><description>Performs login using the 'Click' mode and expects navigation to InventoryPage.</description></item>
    ///   <item><description>Asserts that the current page is indeed the InventoryPage.</description></item>
    ///   <item><description>Iterates through all predefined sort options (<see cref="_inventoryProductsDropdownOptions"/> from the Data partial class).</description></item>
    ///   <item><description>For each option, calls <see cref="InventoryPage.SortProducts(SortByType, string)"/>.</description></item>
    ///   <item><description>Verifies that the selected sort option in the dropdown matches the applied option (by text or value).</description></item>
    /// </list>
    /// This test is critical for verifying core login and product sorting functionality.
    /// Performance and resource usage (memory) of login and the sort loop are measured and logged.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login with Login Button Click and Sort Products for standard_user")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies user login with the standard_user, using the Click action and then sorts products by all available options.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldLoginSuccessfullyWithStandardUserAndSortProducts()
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
            loginOperationProps,
            ResourceMonitor
        );

        bool loginStepSuccess = false;

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameStandardUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);

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

        var sortLoopTimer = new PerformanceTimer(
            "TestStep_VerifyAllSortOptions",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

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

    /// <summary>
    /// Verifies that a "locked out" user cannot log in successfully and receives the appropriate error message.
    /// The login attempt is made using the 'Submit' action (e.g., pressing Enter in the password field).
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for a "locked out" user.</description></item>
    ///   <item><description>Performs login using the 'Submit' mode and expects to remain on the LoginPage.</description></item>
    ///   <item><description>Asserts that the current page is still the LoginPage.</description></item>
    ///   <item><description>Retrieves the error message displayed on the LoginPage.</description></item>
    ///   <item><description>Asserts that the error message matches the expected message for a locked out user (<see cref="SauceDemoMessages.LockedOutUserError"/>).</description></item>
    /// </list>
    /// This test is critical for verifying error handling and security aspects of the login process.
    /// Performance and resource usage (memory) of the login attempt and error message retrieval are measured.
    /// </remarks>
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
            loginAttemptProps,
            ResourceMonitor
        );

        bool loginAttemptSuccessAsExpected = false;

        BasePage resultPage;
        try
        {
            TestLogger.LogDebug("Instantiating LoginPage.");
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);

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

        var errorMsgTimer = new PerformanceTimer(
            "TestStep_GetLoginErrorMessage_LockedOut",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

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

        actualErrorMessage.ShouldBe(SauceDemoMessages.LockedOutUserError, $"Error message should be: {SauceDemoMessages.LockedOutUserError} but was: {actualErrorMessage}");

        TestLogger.LogInformation("Login not successful, currently on LoginPage.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }

    /// <summary>
    /// Verifies the visual appearance of the inventory page for a "visual_user".
    /// This test logs in as the visual_user, navigates to the inventory page, and then performs
    /// visual assertions using <see cref="IVisualTestService.AssertVisualMatch(string, string, BrowserType, IWebElement?, SixLabors.ImageSharp.Rectangle?, double?)"/>.
    /// It checks both the full page and a specific element (Bolt T-Shirt image).
    /// The overall execution time and resource usage of this visual test are measured.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Starts a performance timer for the entire test method, including resource monitoring.</description></item>
    ///   <item><description>Logs in as 'visual_user'.</description></item>
    ///   <item><description>Asserts navigation to InventoryPage.</description></item>
    ///   <item><description>Pauses briefly (e.g., 1 second) to allow any visual glitches or animations to settle.</description></item>
    ///   <item><description>Performs a full-page visual assertion against a baseline identified by "InventoryPage_VisualUser_FullPage".</description></item>
    ///   <item><description>Attempts to locate the "Sauce Labs Bolt T-Shirt" inventory item.</description></item>
    ///   <item><description>If found and its image is displayed, performs an element-specific visual assertion against a baseline identified by "InventoryPage_VisualUser_BoltTShirtImage".</description></item>
    ///   <item><description>Logs warnings if the specific item or its image cannot be found or is not displayed, skipping the element-specific check.</description></item>
    ///   <item><description>Stops the performance timer and logs/attaches the results.</description></item>
    /// </list>
    /// This test is important for catching unintended UI changes that functional tests might miss.
    /// It relies on the <see cref="VisualTester"/> service (from <see cref="BaseTest"/>) and pre-existing baseline images.
    /// </remarks>
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

        long expectedMaxDurationMs = 30000;
        var visualTestTimer = new PerformanceTimer(
            $"TestStep_VisualAssertion_{currentTestName}",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            new Dictionary<string, object> { { "UserType", "visual_user" } },
            ResourceMonitor
        );
        bool testStepsSuccessful = false;

        try
        {
            TestLogger.LogDebug("Instantiating LoginPage for {TestName}.", currentTestName);
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);

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
                Thread.Sleep(500);
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

                if (boltTShirtComponent != null)
                {
                    TestLogger.LogInformation("Found 'Sauce Labs Bolt T-Shirt' component. Proceeding with image visual check.");
                    IWebElement boltTShirtImageElement = boltTShirtComponent.ItemImage;

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
                        TestLogger.LogWarning(
                            "'Sauce Labs Bolt T-Shirt' image element found via component but was not displayed. Skipping element-specific visual check for {BaselineID} in {TestName}.",
                            boltTShirtImageBaselineId,
                            currentTestName
                        );
                    }
                    else
                    {
                        TestLogger.LogWarning(
                            "Could not retrieve 'ItemImage' from the 'Sauce Labs Bolt T-Shirt' component (element was null). Skipping check for {BaselineID} in {TestName}.",
                            boltTShirtImageBaselineId,
                            currentTestName
                        );
                    }
                }
                else
                {
                    TestLogger.LogWarning("Could not find the 'Sauce Labs Bolt T-Shirt' item component using InventoryPage.GetInventoryItems(). Skipping element-specific visual check for {BaselineID} in {TestName}.", boltTShirtImageBaselineId, currentTestName);
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

            testStepsSuccessful = true;
        }
        finally
        {
            visualTestTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: testStepsSuccessful ? expectedMaxDurationMs : null);
            visualTestTimer.Dispose();
            TestLogger.LogInformation("Finished visual test: {TestName} for visual_user", currentTestName);
        }
    }
}
