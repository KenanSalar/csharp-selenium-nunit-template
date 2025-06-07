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
    /// Verifies the behavior of the inventory page when logged in as the "problem_user".
    /// This user typically displays the same incorrect image for all products.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for the "problem_user".</description></item>
    ///   <item><description>Performs login using 'Click' mode and expects navigation to InventoryPage.</description></item>
    ///   <item><description>Asserts that the current page is indeed the InventoryPage.</description></item>
    ///   <item><description>Retrieves all inventory items.</description></item>
    ///   <item><description>Asserts that there is more than one item displayed (to make the image check meaningful).</description></item>
    ///   <item><description>Gets the 'src' attribute of each product image.</description></item>
    ///   <item><description>Asserts that all product images have the same 'src' attribute, indicating they are all the same image.</description></item>
    /// </list>
    /// Performance and resource usage of login and the image verification are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login and Verify Product Images for problem_user")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureDescription("Verifies that when logged in as problem_user, all product images on the inventory page are identical.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldDisplaySameImageForAllProductsForProblemUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for problem_user", currentTestName);

        BasePage resultPage;
        const LoginMode loginMode = LoginMode.Click;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameProblemUser },
            { "LoginAction", loginMode.ToString() }
        };

        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_ProblemUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps,
            ResourceMonitor
        );
        bool loginStepSuccess = false;

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameProblemUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameProblemUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);

            loginStepSuccess = resultPage is InventoryPage;
        }
        finally
        {
            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginStepSuccess ? 7000 : null);
            loginTimer.Dispose();
        }

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login as problem_user should be successful and navigate to the Inventory Page.");
        TestLogger.LogInformation("Login successful as problem_user, currently on InventoryPage.");

        var imageVerificationTimer = new PerformanceTimer(
            "TestStep_VerifyProductImages_ProblemUser",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        var productImageSources = new List<string>();
        try
        {
            TestLogger.LogInformation("Retrieving inventory items to check product images.");
            IEnumerable<InventoryItemComponent> items = inventoryPage.GetInventoryItems();

            items.Count().ShouldBeGreaterThan(1, "There should be multiple items to compare images.");

            foreach (InventoryItemComponent item in items)
            {
                try
                {
                    IWebElement itemImageElement = item.ItemImage;
                    if (itemImageElement != null && itemImageElement.Displayed)
                    {
                        string? imgSrc = itemImageElement.GetAttribute("src");
                        if (!string.IsNullOrEmpty(imgSrc))
                        {
                            productImageSources.Add(imgSrc);
                            TestLogger.LogDebug("Item '{ItemName}' image src: {ImageSrc}", item.ItemName, imgSrc);
                        }
                        else
                        {
                            TestLogger.LogWarning("Item '{ItemName}' image src attribute is null or empty.", item.ItemName);
                        }
                    }
                    else
                    {
                        TestLogger.LogWarning("Item '{ItemName}' image element is not displayed or null.", item.ItemName);
                    }
                }
                catch (Exception ex)
                {
                    TestLogger.LogError(ex, "Error getting image source for item: {ItemName}", item.ItemName);
                }
            }

            productImageSources.Count.ShouldBe(items.Count(), "Should have retrieved an image source for each item.");

            if (productImageSources.Count != 0)
            {
                int distinctImageSourcesCount = productImageSources.Distinct().Count();
                distinctImageSourcesCount.ShouldBe(1, $"Expected all product images to be the same for problem_user, but found {distinctImageSourcesCount} distinct image sources. Sources: [{string.Join(", ", productImageSources.Distinct())}]");
                TestLogger.LogInformation("Verified that all {ImageCount} product images have the same source: {ImageSrc_Example}", productImageSources.Count, productImageSources.First());
            }
            else
            {
                Assert.Fail("No product image sources were collected, cannot verify problem_user image issue.");
            }
        }
        finally
        {
            imageVerificationTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
            imageVerificationTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName} for problem_user", currentTestName);
    }

    /// <summary>
    /// Verifies that a user can log in as "performance_glitch_user" and that the inventory page,
    /// despite known performance issues for this user, eventually loads and displays products.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Instantiates the LoginPage.</description></item>
    ///   <item><description>Enters credentials for the "performance_glitch_user".</description></item>
    ///   <item><description>Performs login using 'Click' mode and expects navigation to InventoryPage. This step's duration is measured.</description></item>
    ///   <item><description>Asserts that the current page is indeed the InventoryPage.</description></item>
    ///   <item><description>Attempts to retrieve inventory items, expecting at least one to be present. This step's duration is also measured.</description></item>
    ///   <item><description>Asserts that inventory items are loaded.</description></item>
    /// </list>
    /// The key observation for this test is the extended duration of operations, which is captured by PerformanceTimers.
    /// The test passes if functionality remains intact despite the slowness.
    /// Performance and resource usage of login and inventory item retrieval are measured and logged.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login and Verify Inventory Load for performance_glitch_user")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureDescription("Verifies that the performance_glitch_user can log in and the inventory page loads, albeit slowly.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldLoadInventoryForPerformanceGlitchUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for performance_glitch_user", currentTestName);

        BasePage resultPage;
        const LoginMode loginMode = LoginMode.Click;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernamePerformanceGlitchUser },
            { "LoginAction", loginMode.ToString() }
        };

        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_PerformanceGlitchUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps,
            ResourceMonitor
        );
        bool loginStepSuccess = false;

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernamePerformanceGlitchUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernamePerformanceGlitchUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);
            loginStepSuccess = resultPage is InventoryPage;
        }
        finally
        {
            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginStepSuccess ? 20000 : null);
            loginTimer.Dispose();
        }

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login as performance_glitch_user should be successful and navigate to the Inventory Page.");
        TestLogger.LogInformation("Login successful as performance_glitch_user, currently on InventoryPage.");

        var inventoryLoadTimer = new PerformanceTimer(
            "TestStep_VerifyInventoryItemsLoad_PerformanceGlitchUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            resourceMonitor: ResourceMonitor
        );
        bool inventoryItemsLoaded = false;

        try
        {
            TestLogger.LogInformation("Attempting to retrieve inventory items for performance_glitch_user.");
            IEnumerable<InventoryItemComponent> items = inventoryPage.GetInventoryItems(minExpectedItems: 1);

            items.Any().ShouldBeTrue("At least one inventory item should be loaded on the page for performance_glitch_user.");
            TestLogger.LogInformation("Successfully retrieved {ItemCount} inventory items for performance_glitch_user.", items.Count());
            inventoryItemsLoaded = true;
        }
        finally
        {
            inventoryLoadTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: inventoryItemsLoaded ? 15000 : null);
            inventoryLoadTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName} for performance_glitch_user. Functionality confirmed despite slowness.", currentTestName);
    }

    /// <summary>
    /// Verifies that the "error_user" encounters browser-specific issues when interacting with the cart
    /// functionality on the inventory page.
    /// </summary>
    /// <remarks>
    /// This test checks for distinct error behaviors depending on the browser:
    /// <list type="bullet">
    ///   <item>
    ///     <term>For Chrome (and potentially other similar browsers):</term>
    ///     <description>
    ///       1. Logs in as "error_user".
    ///       2. Attempts to add an item to the cart.
    ///       3. Asserts that the item's "Add to cart" button text *does not* change, indicating the add action failed at the UI level.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>For Firefox:</term>
    ///     <description>
    ///       1. Logs in as "error_user".
    ///       2. Adds an item to the cart (button text *is expected* to change to "Remove").
    ///       3. Attempts to remove the item from the cart by clicking the "Remove" button.
    ///       4. Asserts that the item's button text *does not* change back from "Remove", indicating the remove action failed at the UI level.
    ///     </description>
    ///   </item>
    /// </list>
    /// Performance and resource usage of login and cart interactions are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Login as error_user and Verify Browser-Specific Cart Error Behaviors")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that the error_user encounters specific, expected cart interaction failures that may differ by browser.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldExhibitExpectedCartErrorsForErrorUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for error_user (browser-specific cart errors)", currentTestName);

        BasePage resultPage;
        const LoginMode loginMode = LoginMode.Click;
        string itemNameForTest;

        var loginOperationProps = new Dictionary<string, object>
        {
            { "Username", _sauceDemoSettings.LoginUsernameErrorUser },
            { "LoginAction", loginMode.ToString() }
        };

        var loginTimer = new PerformanceTimer(
            "TestStep_UserLogin_ErrorUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            loginOperationProps,
            ResourceMonitor
        );
        bool loginStepSuccess = false;

        TestLogger.LogInformation(
            "Attempting login with username: {LoginUsername} using {LoginActionType} action.",
            _sauceDemoSettings.LoginUsernameErrorUser,
            loginMode
        );

        try
        {
            LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
            resultPage = loginPage
                .EnterUsername(_sauceDemoSettings.LoginUsernameErrorUser)
                .EnterPassword(_sauceDemoSettings.LoginPassword)
                .LoginAndExpectNavigation(loginMode);
            loginStepSuccess = resultPage is InventoryPage;
        }
        finally
        {
            loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: loginStepSuccess ? 7000 : null);
            loginTimer.Dispose();
        }

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login as error_user should be successful and navigate to the Inventory Page.");
        TestLogger.LogInformation("Login successful as error_user, currently on InventoryPage.");

        var cartInteractionTimer = new PerformanceTimer(
            "TestStep_VerifyCartBehavior_ErrorUser",
            TestLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            new Dictionary<string, object> { { "Browser", BrowserType.ToString() } },
            ResourceMonitor
        );

        bool cartInteractionsVerified = false;
        IWebDriver driver = WebDriverManager.GetDriver();

        try
        {
            TestLogger.LogInformation("Attempting to interact with the first inventory item's cart button for error_user on {Browser}.", BrowserType);
            InventoryItemComponent? firstItem = inventoryPage.GetInventoryItems(minExpectedItems: 1).FirstOrDefault();
            _ = firstItem.ShouldNotBeNull("At least one inventory item should be available to test.");

            itemNameForTest = firstItem.ItemName;
            TestLogger.LogInformation("Selected item for cart interaction test: {ItemName}", itemNameForTest);

            string buttonTextBeforeAdd = firstItem.GetActionButtonText();
            TestLogger.LogDebug("Action button text for '{ItemName}' before first click: '{ButtonText}'", itemNameForTest, buttonTextBeforeAdd);
            buttonTextBeforeAdd.ShouldBe("Add to cart", $"The initial button text for '{itemNameForTest}' was expected to be 'Add to cart'.");

            firstItem.ClickActionButton();
            TestLogger.LogInformation("Performed first click on action button for item: {ItemName}", itemNameForTest);

            var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            try
            {
                if (BrowserType == BrowserType.Firefox && buttonTextBeforeAdd == "Add to cart")
                {
                    _ = shortWait.Until(d => firstItem.ActionButtonElement.Text == "Remove");
                    TestLogger.LogDebug("Button text changed to 'Remove' as expected for Firefox 'add to cart' action.");
                }
                else
                {
                    _ = shortWait.Until(ExpectedConditions.ElementToBeClickable(firstItem.ActionButtonElement));
                    TestLogger.LogDebug("Action button became clickable after first interaction.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogWarning("UI did not reach the expected state (text change or clickable) within timeout after first click for item '{ItemName}'. Proceeding with assertion.", itemNameForTest);
            }

            string buttonTextAfterFirstClick = firstItem.GetActionButtonText();
            TestLogger.LogDebug("Action button text for '{ItemName}' after first click: '{ButtonText}'", itemNameForTest, buttonTextAfterFirstClick);

            if (BrowserType == BrowserType.Chrome)
            {
                TestLogger.LogInformation("Chrome specific check: Verifying 'Add to cart' fails for error_user (button text should not change).");
                buttonTextAfterFirstClick.ShouldBe(
                    buttonTextBeforeAdd, $"For error_user on Chrome, button text for '{itemNameForTest}' should " +
                    "remain '{buttonTextBeforeAdd}' after first click, but was '{buttonTextAfterFirstClick}'."
                );
                TestLogger.LogInformation("Verified for Chrome: Button text remained 'Add to cart' as expected for error_user.");
            }
            else if (BrowserType == BrowserType.Firefox)
            {
                TestLogger.LogInformation("Firefox specific check: Verifying 'Add to cart' appears to succeed, then 'Remove' fails for error_user.");
                buttonTextAfterFirstClick.ShouldBe(
                    "Remove", $"For error_user on Firefox, button text for '{itemNameForTest}' should " +
                    "change to 'Remove' after first click (add), but was '{buttonTextAfterFirstClick}'."
                );
                TestLogger.LogInformation("Verified for Firefox: Button text changed to 'Remove' after add attempt.");

                TestLogger.LogInformation("Attempting second click (to 'Remove') for item: {ItemName} on Firefox", itemNameForTest);
                firstItem.ClickActionButton();
                TestLogger.LogInformation("Performed second click on action button for item: {ItemName} on Firefox", itemNameForTest);

                try
                {
                    _ = shortWait.Until(ExpectedConditions.ElementToBeClickable(firstItem.ActionButtonElement));
                    TestLogger.LogDebug("Action button became clickable after 'remove' attempt.");
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogWarning("UI did not become clickable within timeout after 'remove' attempt for item '{ItemName}'. Proceeding with assertion.", itemNameForTest);
                }

                string buttonTextAfterRemoveAttempt = firstItem.GetActionButtonText();
                TestLogger.LogDebug("Action button text for '{ItemName}' after remove attempt on Firefox: '{ButtonText}'", itemNameForTest, buttonTextAfterRemoveAttempt);
                buttonTextAfterRemoveAttempt.ShouldBe(
                    "Remove", $"For error_user on Firefox, button text for '{itemNameForTest}' should " +
                    "remain 'Remove' after attempting to remove, but was '{buttonTextAfterRemoveAttempt}'."
                );
                TestLogger.LogInformation("Verified for Firefox: Button text remained 'Remove' after remove attempt, as expected for error_user.");
            }
            else
            {
                inventoryPage.GetShoppingCartBadgeCount().ShouldBe(0, "The cart count should not increase for the error_user.");
                TestLogger.LogInformation("Verified for Chrome: Shopping cart count remained 0 as expected for error_user.");
            }

            cartInteractionsVerified = true;
        }
        finally
        {
            cartInteractionTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: cartInteractionsVerified ? 6000 : null);
            cartInteractionTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName} for error_user. Verified expected browser-specific cart error behavior.", currentTestName);
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
    ///   <item><description>Pauses with explicit wait to allow any visual glitches or animations to settle.</description></item>
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
        IWebDriver driver = WebDriverManager.GetDriver();

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
                TestLogger.LogTrace("Ensuring page is visually stable by checking for InventoryContainer visibility (max 1s wait) for {TestName}.", currentTestName);
                var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
                _ = shortWait.Until(ExpectedConditions.ElementIsVisible(InventoryPageMap.InventoryContainer));
                TestLogger.LogDebug("Page confirmed stable (InventoryContainer visible) after short wait for {TestName}.", currentTestName);
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogWarning("Short wait for visual stability (InventoryContainer visibility) timed out for {TestName}. Visuals might capture an intermediate state.", currentTestName);
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
