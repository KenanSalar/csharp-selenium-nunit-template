namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies that a standard user can successfully add multiple items to the cart,
    /// view them in the cart, remove some items, and see the cart updated correctly.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Navigates to the InventoryPage.</description></item>
    ///   <item><description>Adds three specific items ("Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt") to the cart, verifying button text changes and cart badge updates.</description></item>
    ///   <item><description>Navigates to the ShoppingCartPage.</description></item>
    ///   <item><description>Verifies all three added items are present in the cart by name and count.</description></item>
    ///   <item><description>Removes two specific items ("Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt") from the cart.</description></item>
    ///   <item><description>Verifies that only one item ("Sauce Labs Backpack") remains in the cart by checking item count and name.</description></item>
    ///   <item><description>Navigates back to InventoryPage using "Continue Shopping" and verifies the cart badge count is 1.</description></item>
    /// </list>
    /// Performance and resource usage of these operations are measured using <see cref="PerformanceTimer"/>.
    /// </remarks>
    [Test]
    [Retry(1)]
    [AllureStep("Standard user adds and removes items from cart")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies the full add-to-cart and remove-from-cart flow for a standard user.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void ShouldAddAndRemoveItemsFromCartSuccessfullyForStandardUser()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for standard_user cart interactions", currentTestName);

        var itemsToAdd = new List<string> { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        var itemsToRemove = new List<string> { "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        string itemToRemain = "Sauce Labs Backpack";
        IWebDriver driver = WebDriverManager.GetDriver();

        // --- Login Step ---
        var loginTimer = new PerformanceTimer("TestStep_Login_StandardUser_ForCartTest", TestLogger, resourceMonitor: ResourceMonitor);
        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
        InventoryPage inventoryPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click)
            .ShouldBeOfType<InventoryPage>("Login should lead to Inventory Page.");
        loginTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 7000);
        TestLogger.LogInformation("Login successful, on Inventory Page.");

        // --- Add Items to Cart Step ---
        var addItemsTimer = new PerformanceTimer("TestStep_AddItemsToCart", TestLogger, resourceMonitor: ResourceMonitor);
        try
        {
            var allInventoryItems = inventoryPage.GetInventoryItems().ToList();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            foreach (string itemName in itemsToAdd)
            {
                InventoryItemComponent? itemComponent = allInventoryItems.FirstOrDefault(i => i.ItemName == itemName);
                _ = itemComponent.ShouldNotBeNull($"Item '{itemName}' should be found on the inventory page.");

                itemComponent.GetActionButtonText().ShouldBe("Add to cart", $"Button for '{itemName}' should initially be 'Add to cart'.");
                itemComponent.ClickActionButton();

                IWebElement buttonElement = itemComponent.ActionButtonElement;

                string expectedButtonText = "Remove";
                try
                {
                    _ = wait.Until(d =>
                    {
                        try
                        {
                            return buttonElement.Text == expectedButtonText;
                        }
                        catch (StaleElementReferenceException)
                        {
                            TestLogger.LogTrace("Stale element encountered while waiting for button text of '{ItemName}' to become '{ExpectedText}'. Retrying.", itemName, expectedButtonText);
                            return false;
                        }
                    });
                    TestLogger.LogInformation("Button text for '{ItemName}' successfully changed to '{ExpectedText}'.", itemName, expectedButtonText);
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogError("Timeout: Button text for '{ItemName}' did not change to '{ExpectedText}' within the timeout. Current text: {CurrentText}", itemName, expectedButtonText, buttonElement.Text);
                }

                itemComponent.GetActionButtonText().ShouldBe("Remove", $"Button for '{itemName}' should change to 'Remove' after adding.");
                TestLogger.LogInformation("Added '{ItemName}' to cart.", itemName);
            }
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(itemsToAdd.Count, "Shopping cart badge count should be updated after adding items.");
        }
        finally
        {
            addItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 10000);
        }

        // --- Navigate to Cart and Verify Items ---
        TestLogger.LogInformation("Navigating to shopping cart page.");
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink()
            .ShouldBeOfType<ShoppingCartPage>("Clicking cart icon should lead to ShoppingCartPage.");

        var verifyCartTimer = new PerformanceTimer("TestStep_VerifyCartContentsBeforeRemove", TestLogger, resourceMonitor: ResourceMonitor);
        try
        {
            var cartItems = shoppingCartPage.GetCartItems().ToList();
            cartItems.Count.ShouldBe(itemsToAdd.Count, $"Expected {itemsToAdd.Count} items in cart, but found {cartItems.Count}.");
            foreach (string itemName in itemsToAdd)
            {
                cartItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be present in the cart.");
            }
            TestLogger.LogInformation("Verified all {ItemCount} added items are present in the cart.", itemsToAdd.Count);
        }
        finally
        {
            verifyCartTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        // --- Remove Items from Cart Step ---
        var removeItemsTimer = new PerformanceTimer("TestStep_RemoveItemsFromCart", TestLogger, resourceMonitor: ResourceMonitor);
        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            foreach (string itemName in itemsToRemove)
            {
                _ = shoppingCartPage.RemoveItemByName(itemName);
                TestLogger.LogInformation("Removed '{ItemName}' from cart.", itemName);

                try
                {
                    _ = wait.Until(d => !shoppingCartPage.GetCartItems().Any(i => i.ItemName == itemName));
                    TestLogger.LogInformation("Verified item '{ItemName}' is no longer visible in the cart.", itemName);
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogError("Item '{ItemName}' was still visible in the cart after the remove timeout.", itemName);
                    // Let the test continue and likely fail on the final count assertion, which is more descriptive.
                }
            }

            var remainingCartItems = shoppingCartPage.GetCartItems().ToList();
            remainingCartItems.Count.ShouldBe(1, "Expected 1 item to remain in the cart.");
            remainingCartItems.First().ItemName.ShouldBe(itemToRemain, $"Expected '{itemToRemain}' to be the only item left.");
            TestLogger.LogInformation("Verified item removal. '{ItemName}' is the only item remaining.", itemToRemain);

            inventoryPage = shoppingCartPage.ClickContinueShopping();
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge count should be 1 after removals.");

        }
        finally
        {
            removeItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 8000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Cart add and remove functionality verified.", currentTestName);
    }
}
