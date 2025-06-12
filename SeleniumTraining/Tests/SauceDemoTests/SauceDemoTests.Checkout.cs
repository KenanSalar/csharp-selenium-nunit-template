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
    ///   <item><description>Uses a helper method to log in as the standard_user and add an item to the cart.</description></item>
    ///   <item><description>Navigates from the inventory page to the ShoppingCartPage and clicks "Checkout".</description></item>
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
        var itemsToOrder = new List<string> { "Sauce Labs Backpack" };

        // --- Login and Add Item to Cart (Setup) ---
        InventoryPage inventoryPage = LoginAndAddItemsToCart(itemsToOrder);

        // --- ACT ---
        // Navigate from inventory to the first checkout step
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
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

        // --- ASSERT ---
        // Verify we landed on the correct page after filling the form
        _ = checkoutStepTwoPage.ShouldNotBeNull("Proceeding from checkout step one should lead to step two.");
        TestLogger.LogInformation("Successfully submitted checkout info and landed on the overview page.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }

    /// <summary>
    /// Verifies that a standard user can review their order on the checkout overview page
    /// and successfully complete the purchase.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Uses a helper method to log in as the standard_user and add two specific items to the cart.</description></item>
    ///   <item><description>Proceeds through the first step of checkout by filling in user information.</description></item>
    ///   <item><description>Verifies landing on the CheckoutStepTwoPage (Overview).</description></item>
    ///   <item><description>Verifies that the correct items are listed in the overview by checking count and names.</description></item>
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

        // --- ARRANGE ---
        // Define test data
        string firstName = "Checkout";
        string lastName = "UserTwo";
        string postalCode = "54321";
        var itemsToOrder = new List<string> { "Sauce Labs Backpack", "Sauce Labs Bike Light" };

        // --- Setup: Login, Add Items, Reach Checkout Step One ---
        InventoryPage inventoryPage = LoginAndAddItemsToCart(itemsToOrder);

        // Navigate to the checkout overview page
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();
        CheckoutStepTwoPage checkoutStepTwoPage = checkoutStepOnePage
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode)
            .ClickContinue();
        TestLogger.LogInformation("Setup complete, on Checkout Overview page.");

        // --- ACT & ASSERT ---
        var overviewAndFinishTimer = new PerformanceTimer("TestStep_VerifyOverviewAndFinish", TestLogger, resourceMonitor: ResourceMonitor);
        CheckoutCompletePage checkoutCompletePage;
        try
        {
            // Verify items in overview
            var overviewItems = checkoutStepTwoPage.GetItemsInOverview().ToList();
            overviewItems.Count.ShouldBe(itemsToOrder.Count, $"Expected {itemsToOrder.Count} items in overview.");
            foreach (string itemName in itemsToOrder)
            {
                overviewItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be in overview.");
            }
            TestLogger.LogInformation("Items verified on overview page.");

            // Verify pricing details (optional)
            TestLogger.LogInformation("Subtotal: {Subtotal}", checkoutStepTwoPage.GetSubtotalText());
            TestLogger.LogInformation("Total: {Total}", checkoutStepTwoPage.GetTotalText());

            // Finish the purchase
            checkoutCompletePage = checkoutStepTwoPage.ClickFinish();
        }
        finally
        {
            overviewAndFinishTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
            overviewAndFinishTimer.Dispose();
        }

        // Verify landing on the final page
        _ = checkoutCompletePage.ShouldNotBeNull("Finishing order should lead to complete page.");
        TestLogger.LogInformation("Clicked 'Finish'. Verified navigation to Checkout Complete page.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }

    /// <summary>
    /// Verifies the purchase confirmation page details and the "Back Home" functionality
    /// for a standard user after completing a purchase.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Uses a helper method to log in as standard_user and add an item to the cart.</description></item>
    ///   <item><description>Completes all checkout steps to reach the CheckoutCompletePage.</description></item>
    ///   <item><description>On CheckoutCompletePage, verifies the confirmation header text and the presence of the "Pony Express" image.</description></item>
    ///   <item><description>Clicks the "Back Home" button.</description></item>
    ///   <item><description>Asserts navigation back to the InventoryPage.</description></item>
    ///   <item><description>Asserts that the shopping cart is now empty by checking the badge count is 0.</description></item>
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
        var itemsToOrder = new List<string> { "Sauce Labs Fleece Jacket" };

        // --- Setup: Login, Add Item, Complete Checkout to reach Confirmation Page ---
        InventoryPage inventoryPage = LoginAndAddItemsToCart(itemsToOrder);

        // Complete checkout process to get to the confirmation page
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();
        CheckoutStepTwoPage checkoutStepTwoPage = checkoutStepOnePage
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode)
            .ClickContinue();
        CheckoutCompletePage checkoutCompletePage = checkoutStepTwoPage.ClickFinish();
        TestLogger.LogInformation("Setup complete, on Checkout Complete page.");

        // --- ACT & ASSERT ---
        var confirmationTimer = new PerformanceTimer("TestStep_VerifyConfirmationAndReturnHome", TestLogger, resourceMonitor: ResourceMonitor);
        InventoryPage finalInventoryPage;
        try
        {
            // Verify confirmation page details
            checkoutCompletePage.GetConfirmationHeaderText().ShouldBe("Thank you for your order!", StringCompareShould.IgnoreCase);
            checkoutCompletePage.IsPonyExpressImageDisplayed().ShouldBeTrue("Pony Express image should be displayed.");
            TestLogger.LogInformation("Confirmation page details verified.");

            // Click back home
            finalInventoryPage = checkoutCompletePage.ClickBackHome();
        }
        finally
        {
            confirmationTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
            confirmationTimer.Dispose();
        }

        // Verify state after returning to inventory
        _ = finalInventoryPage.ShouldNotBeNull("Clicking 'Back Home' should return to Inventory Page.");
        finalInventoryPage.GetShoppingCartBadgeCount().ShouldBe(0, "Shopping cart should be empty after completing an order and returning home.");
        TestLogger.LogInformation("Verified cart is empty on Inventory page.");
        TestLogger.LogInformation("Finished test: {TestName}", currentTestName);
    }
}
