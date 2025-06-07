namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies that a standard user can successfully fill in their information
    /// on the first step of the checkout process and proceed to the overview page.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Adds an item to the cart (e.g., "Sauce Labs Backpack").</description></item>
    ///   <item><description>Navigates to the ShoppingCartPage.</description></item>
    ///   <item><description>Clicks the "Checkout" button.</description></item>
    ///   <item><description>Verifies landing on the CheckoutStepOnePage.</description></item>
    ///   <item><description>Fills in valid First Name, Last Name, and Zip/Postal Code.</description></item>
    ///   <item><description>Clicks "Continue".</description></item>
    ///   <item><description>Asserts navigation to the CheckoutStepTwoPage (Overview page).</description></item>
    /// </list>
    /// Performance of the checkout information step is measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user successfully fills checkout information")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that the standard user can complete the first step of checkout (entering personal information).")]
    public void ShouldSuccessfullyFillCheckoutInformationForStandardUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        string firstName = "Test";
        string lastName = "User";
        string postalCode = "12345";
        string itemToAddToCart = "Sauce Labs Backpack";

        // --- Login and Add Item to Cart (Setup) ---
        var setupTimer = new PerformanceTimer("TestStep_Setup_LoginAndAddItemForCheckout", TestLogger, resourceMonitor: ResourceMonitor);
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
        InventoryPage inventoryPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click)
            .ShouldBeOfType<InventoryPage>("Login should lead to Inventory Page.");

        InventoryItemComponent? item = inventoryPage.GetInventoryItems().FirstOrDefault(i => i.ItemName == itemToAddToCart);
        _ = item.ShouldNotBeNull($"Item '{itemToAddToCart}' must be present to add to cart.");
        item.ClickActionButton();
        TestLogger.LogInformation("Item '{ItemName}' added to cart for checkout.", itemToAddToCart);
        setupTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 10000);

        // --- Navigate to Cart and Proceed to Checkout ---
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink()
            .ShouldBeOfType<ShoppingCartPage>();

        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();
        TestLogger.LogInformation("Navigated to Checkout Step One page.");

        // --- Fill Information and Continue ---
        var fillInfoTimer = new PerformanceTimer("TestStep_FillCheckoutStepOneInfo", TestLogger, resourceMonitor: ResourceMonitor);
        CheckoutStepTwoPage checkoutStepTwoPage;
        try
        {
            checkoutStepTwoPage = checkoutStepOnePage
                .EnterFirstName(firstName)
                .EnterLastName(lastName)
                .EnterPostalCode(postalCode)
                .ClickContinue();

            _ = checkoutStepTwoPage.ShouldNotBeNull("Proceeding from checkout step one should lead to step two.");

            CheckoutCompletePage checkoutCompletePage = checkoutStepTwoPage.ClickFinish();
            _ = checkoutCompletePage.ShouldNotBeNull("Checkout should lead to checkout complete page.");

            TestLogger.LogInformation("Checkout information filled and continued. Expected to be on Overview page.");
        }
        finally
        {
            fillInfoTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
            fillInfoTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName}. Checkout information successfully submitted.", currentTestName);
    }

    /// <summary>
    /// Verifies that a standard user can review their order on the checkout overview page
    /// and successfully complete the purchase.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Adds "Sauce Labs Backpack" and "Sauce Labs Bike Light" to the cart.</description></item>
    ///   <item><description>Proceeds through the first step of checkout (entering user information).</description></item>
    ///   <item><description>Verifies landing on the CheckoutStepTwoPage (Overview).</description></item>
    ///   <item><description>Verifies that the correct items are listed in the overview (e.g., by checking count and names).</description></item>
    ///   <item><description>Optionally, verifies pricing details (subtotal, tax, total) - (Simplified for this example).</description></item>
    ///   <item><description>Clicks the "Finish" button.</description></item>
    ///   <item><description>Asserts navigation to the CheckoutCompletePage.</description></item>
    /// </list>
    /// Performance of the overview and finish steps are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user completes purchase from overview")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that the standard user can review and finish the order from the checkout overview page.")]
    public void ShouldSuccessfullyCompletePurchaseFromOverviewForStandardUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        string firstName = "Checkout";
        string lastName = "UserTwo";
        string postalCode = "54321";
        var itemsToOrder = new List<string> { "Sauce Labs Backpack", "Sauce Labs Bike Light" };

        // --- Setup: Login, Add Items, Reach Checkout Step One ---
        var setupTimer = new PerformanceTimer("TestStep_Setup_ReachCheckoutOverview", TestLogger, resourceMonitor: ResourceMonitor);
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
        InventoryPage inventoryPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click)
            .ShouldBeOfType<InventoryPage>();

        var allInventoryItems = inventoryPage.GetInventoryItems().ToList();
        foreach (string itemName in itemsToOrder)
        {
            allInventoryItems.First(i => i.ItemName == itemName).ClickActionButton();
        }
        inventoryPage.GetShoppingCartBadgeCount().ShouldBe(itemsToOrder.Count);

        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();

        CheckoutStepTwoPage checkoutStepTwoPage = checkoutStepOnePage
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode)
            .ClickContinue();

        setupTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 15000);
        TestLogger.LogInformation("Setup complete, on Checkout Overview page.");

        // --- Verify Overview and Finish ---
        var overviewAndFinishTimer = new PerformanceTimer("TestStep_VerifyOverviewAndFinish", TestLogger, resourceMonitor: ResourceMonitor);
        CheckoutCompletePage checkoutCompletePage;
        try
        {
            var overviewItems = checkoutStepTwoPage.GetItemsInOverview().ToList();
            overviewItems.Count.ShouldBe(itemsToOrder.Count, $"Expected {itemsToOrder.Count} items in overview.");
            foreach (string itemName in itemsToOrder)
            {
                overviewItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be in overview.");
            }
            TestLogger.LogInformation("Items verified on overview page.");

            TestLogger.LogInformation("Subtotal: {Subtotal}", checkoutStepTwoPage.GetSubtotalText());
            TestLogger.LogInformation("Total: {Total}", checkoutStepTwoPage.GetTotalText());

            checkoutCompletePage = checkoutStepTwoPage.ClickFinish();
            _ = checkoutCompletePage.ShouldNotBeNull("Finishing order should lead to complete page.");
            TestLogger.LogInformation("Clicked 'Finish'. Expected to be on Checkout Complete page.");
        }
        finally
        {
            overviewAndFinishTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
            overviewAndFinishTimer.Dispose();
        }

        TestLogger.LogInformation("Finished test: {TestName}. Purchase completed from overview.", currentTestName);
    }

    /// <summary>
    /// Verifies the purchase confirmation page details and the "Back Home" functionality
    /// for a standard user after completing a purchase.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as standard_user, adds items, and completes all checkout steps to reach the CheckoutCompletePage.</description></item>
    ///   <item><description>On CheckoutCompletePage: Verifies the confirmation header text (e.g., "THANK YOU FOR YOUR ORDER").</description></item>
    ///   <item><description>Verifies the presence of the "Pony Express" image.</description></item>
    ///   <item><description>Clicks the "Back Home" button.</description></item>
    ///   <item><description>Asserts navigation back to the InventoryPage.</description></item>
    ///   <item><description>Asserts that the shopping cart is now empty (badge count is 0).</description></item>
    /// </list>
    /// Performance of the confirmation page interaction and navigation is measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user verifies purchase confirmation and returns home")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies the order completion page details and successful return to inventory with an empty cart.")]
    public void ShouldDisplayCorrectConfirmationAndReturnHomeForStandardUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);

        string firstName = "ThankYou";
        string lastName = "User";
        string postalCode = "98765";
        string itemToOrder = "Sauce Labs Fleece Jacket";

        // --- Setup: Login, Add Item, Complete Checkout to reach Confirmation Page ---
        var setupTimer = new PerformanceTimer("TestStep_Setup_CompletePurchase", TestLogger, resourceMonitor: ResourceMonitor);
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
        InventoryPage inventoryPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click)
            .ShouldBeOfType<InventoryPage>();

        inventoryPage.GetInventoryItems().First(i => i.ItemName == itemToOrder).ClickActionButton();

        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();

        CheckoutStepTwoPage checkoutStepTwoPage = checkoutStepOnePage
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode)
            .ClickContinue();

        CheckoutCompletePage checkoutCompletePage = checkoutStepTwoPage.ClickFinish();

        setupTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 20000);
        TestLogger.LogInformation("Setup complete, on Checkout Complete page.");

        // --- Verify Confirmation Page and Return Home ---
        var confirmationTimer = new PerformanceTimer("TestStep_VerifyConfirmationAndReturnHome", TestLogger, resourceMonitor: ResourceMonitor);
        InventoryPage finalInventoryPage;
        try
        {
            checkoutCompletePage.GetConfirmationHeaderText().ShouldBe("Thank you for your order!", StringCompareShould.IgnoreCase);
            checkoutCompletePage.IsPonyExpressImageDisplayed().ShouldBeTrue("Pony Express image should be displayed.");
            TestLogger.LogInformation("Confirmation page details verified.");

            finalInventoryPage = checkoutCompletePage.ClickBackHome();
            _ = finalInventoryPage.ShouldNotBeNull("Clicking 'Back Home' should return to Inventory Page.");
            TestLogger.LogInformation("Navigated back to Inventory page.");

            finalInventoryPage.GetShoppingCartBadgeCount().ShouldBe(0, "Shopping cart should be empty after completing an order and returning home.");
            TestLogger.LogInformation("Shopping cart badge is correctly empty on Inventory page.");
        }
        finally
        {
            confirmationTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
            confirmationTimer.Dispose();
        }
        TestLogger.LogInformation("Finished test: {TestName}. Purchase confirmation and empty cart verified.", currentTestName);
    }
}
