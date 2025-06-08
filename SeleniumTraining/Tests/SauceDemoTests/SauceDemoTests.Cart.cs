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
    [Retry(2)]
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

        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAsStandardUserAndNavigateToInventoryPage();

        // --- Add Items to Cart Step ---
        using (var addItemsTimer = new PerformanceTimer("TestStep_AddItemsToCart", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var allInventoryItems = inventoryPage.GetInventoryItems().ToList();
            foreach (string itemName in itemsToAdd)
            {
                InventoryItemComponent? itemComponent = allInventoryItems.FirstOrDefault(i => i.ItemName == itemName);
                _ = itemComponent.ShouldNotBeNull($"Item '{itemName}' should be found on the inventory page.");

                itemComponent.ClickActionButton();
                TestLogger.LogInformation("Clicked 'Add to cart' for '{ItemName}'. Waiting for UI to update.", itemName);

                try
                {
                    _ = wait.Until(d => itemComponent.GetActionButtonText() == "Remove");
                    TestLogger.LogInformation("Button text for '{ItemName}' successfully changed to 'Remove'.", itemName);
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogError("Timeout waiting for button text to change to 'Remove' for item '{ItemName}'.", itemName);
                }
                itemComponent.GetActionButtonText().ShouldBe("Remove", $"Button for '{itemName}' should change to 'Remove' after adding.");
            }
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(itemsToAdd.Count, "Shopping cart badge count should be updated after adding items.");
            addItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 15000);
        }

        // --- Navigate to Cart and Verify Items ---
        TestLogger.LogInformation("Navigating to shopping cart page.");
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink()
            .ShouldBeOfType<ShoppingCartPage>("Clicking cart icon should lead to ShoppingCartPage.");

        using (var verifyCartTimer = new PerformanceTimer("TestStep_VerifyCartContentsBeforeRemove", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var cartItems = shoppingCartPage.GetCartItems().ToList();
            cartItems.Count.ShouldBe(itemsToAdd.Count, $"Expected {itemsToAdd.Count} items in cart, but found {cartItems.Count}.");
            foreach (string itemName in itemsToAdd)
            {
                cartItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be present in the cart.");
            }
            TestLogger.LogInformation("Verified all {ItemCount} added items are present in the cart.", itemsToAdd.Count);
            verifyCartTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        // --- Remove Items from Cart Step ---
        using (var removeItemsTimer = new PerformanceTimer("TestStep_RemoveItemsFromCart", TestLogger, resourceMonitor: ResourceMonitor))
        {
            foreach (string itemName in itemsToRemove)
            {
                _ = shoppingCartPage.RemoveItemByName(itemName);
                TestLogger.LogInformation("Clicked 'Remove' for '{ItemName}'. Waiting for item to disappear from cart.", itemName);

                try
                {
                    _ = wait.Until(d => !shoppingCartPage.GetCartItems().Any(i => i.ItemName == itemName));
                    TestLogger.LogInformation("Verified item '{ItemName}' is no longer visible in the cart.", itemName);
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogError("Item '{ItemName}' was still visible in the cart after the remove timeout.", itemName);
                }
            }

            var remainingCartItems = shoppingCartPage.GetCartItems().ToList();
            remainingCartItems.Count.ShouldBe(1, "Expected 1 item to remain in the cart.");
            remainingCartItems.First().ItemName.ShouldBe(itemToRemain, $"Expected '{itemToRemain}' to be the only item left.");
            TestLogger.LogInformation("Verified item removal. '{ItemName}' is the only item remaining.", itemToRemain);

            inventoryPage = shoppingCartPage.ClickContinueShopping();
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge count should be 1 after removals.");
            removeItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 8000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Cart add and remove functionality verified.", currentTestName);
    }

    /// <summary>
    /// Verifies that a standard user can successfully add a single item to the cart.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Navigates to the InventoryPage.</description></item>
    ///   <item><description>Defines a specific item to add (e.g., "Sauce Labs Backpack").</description></item>
    ///   <item><description>Finds the item component on the InventoryPage.</description></item>
    ///   <item><description>Clicks the "Add to cart" button for that item.</description></item>
    ///   <item><description>Verifies that the button text for the item changes to "Remove".</description></item>
    ///   <item><description>Verifies that the shopping cart badge count on the InventoryPage updates to 1.</description></item>
    /// </list>
    /// Performance and resource usage of these operations are measured using <see cref="PerformanceTimer"/>.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user adds a single item to cart")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that a standard user can add a single item to the cart and the UI updates correctly.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void TestStandardUserCanAddItemToCart()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for standard_user add single item to cart", currentTestName);

        string itemToAdd = "Sauce Labs Backpack";
        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAsStandardUserAndNavigateToInventoryPage();

        // --- Add Item to Cart Step ---
        using (var addItemTimer = new PerformanceTimer($"TestStep_AddItemToCart_{itemToAdd.Replace(" ", "")}", TestLogger, resourceMonitor: ResourceMonitor))
        {
            InventoryItemComponent? itemComponent = inventoryPage.GetInventoryItems().FirstOrDefault(i => i.ItemName == itemToAdd);
            _ = itemComponent.ShouldNotBeNull($"Item '{itemToAdd}' should be found on the inventory page.");

            itemComponent.ClickActionButton();
            TestLogger.LogInformation("Clicked 'Add to cart' for '{ItemName}'. Waiting for UI to update.", itemToAdd);

            try
            {
                _ = wait.Until(d => itemComponent.GetActionButtonText() == "Remove");
                TestLogger.LogInformation("Button text for '{ItemName}' successfully changed to 'Remove'.", itemToAdd);
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogError("Timeout waiting for button text to change to 'Remove' for item '{ItemName}'.", itemToAdd);
                // Re-fetch the text to ensure the assertion uses the latest state, even if timeout occurred.
                string currentButtonText = itemComponent.GetActionButtonText();
                currentButtonText.ShouldBe("Remove", $"Button for '{itemToAdd}' should change to 'Remove' after adding. Current text: '{currentButtonText}'.");
            }
            itemComponent.GetActionButtonText().ShouldBe("Remove", $"Button for '{itemToAdd}' should change to 'Remove' after adding.");

            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge count should be 1 after adding a single item.");
            TestLogger.LogInformation("Verified shopping cart badge count is 1.");

            addItemTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Single item add to cart functionality verified.", currentTestName);
    }

    /// <summary>
    /// Verifies that a standard user can add an item to the cart, navigate to the cart page,
    /// and see the correct item listed.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Navigates to the InventoryPage.</description></item>
    ///   <item><description>Defines "Sauce Labs Bike Light" as the item to add.</description></item>
    ///   <item><description>Finds the item on InventoryPage and clicks "Add to cart".</description></item>
    ///   <item><description>Waits for the button text to change to "Remove" and cart badge to update.</description></item>
    ///   <item><description>Clicks the shopping cart icon to navigate to ShoppingCartPage.</description></item>
    ///   <item><description>Verifies the current page is ShoppingCartPage.</description></item>
    ///   <item><description>Retrieves items from the cart.</description></item>
    ///   <item><description>Verifies "Sauce Labs Bike Light" is present in the cart and is the only item.</description></item>
    /// </list>
    /// Performance and resource usage are measured using <see cref="PerformanceTimer"/>.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user navigates to cart and verifies item")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that a standard user can add an item, navigate to the cart, and verify the item's presence.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void TestStandardUserCanNavigateToCartAndVerifyItem()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for standard_user navigate to cart and verify item", currentTestName);

        string itemToTest = "Sauce Labs Bike Light";
        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAsStandardUserAndNavigateToInventoryPage();

        // --- Add Item to Cart Step on Inventory Page ---
        using (var addItemTimer = new PerformanceTimer($"TestStep_AddItemToCart_{itemToTest.Replace(" ", "")}_ForCartNavigation", TestLogger, resourceMonitor: ResourceMonitor))
        {
            InventoryItemComponent? itemComponent = inventoryPage.GetInventoryItems().FirstOrDefault(i => i.ItemName == itemToTest);
            _ = itemComponent.ShouldNotBeNull($"Item '{itemToTest}' should be found on the inventory page.");

            itemComponent.ClickActionButton();
            TestLogger.LogInformation("Clicked 'Add to cart' for '{ItemName}'. Waiting for UI to update.", itemToTest);

            try
            {
                _ = wait.Until(d => itemComponent.GetActionButtonText() == "Remove");
                TestLogger.LogInformation("Button text for '{ItemName}' successfully changed to 'Remove'.", itemToTest);
                _ = wait.Until(d => inventoryPage.GetShoppingCartBadgeCount() == 1);
                TestLogger.LogInformation("Shopping cart badge count updated to 1.");
            }
            catch (WebDriverTimeoutException ex)
            {
                TestLogger.LogError(ex, "Timeout waiting for item '{ItemName}' to be added to cart or badge to update. Button text: {ButtonText}, Badge: {BadgeCount}",
                    itemToTest, itemComponent.GetActionButtonText(), inventoryPage.GetShoppingCartBadgeCount());
                // Assertions to ensure failure if critical UI updates didn't happen
                itemComponent.GetActionButtonText().ShouldBe("Remove", $"Button for '{itemToTest}' should change to 'Remove'.");
                inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge should be 1.");
            }
            addItemTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        // --- Navigate to Cart and Verify Item ---
        TestLogger.LogInformation("Navigating to shopping cart page.");
        ShoppingCartPage shoppingCartPage;
        using (var navigationTimer = new PerformanceTimer("TestStep_NavigateToShoppingCartPage", TestLogger, resourceMonitor: ResourceMonitor))
        {
            shoppingCartPage = inventoryPage.ClickShoppingCartLink()
                .ShouldBeOfType<ShoppingCartPage>("Clicking cart icon should lead to ShoppingCartPage.");
            navigationTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }
        TestLogger.LogInformation("Successfully navigated to ShoppingCartPage.");

        using (var verifyCartTimer = new PerformanceTimer($"TestStep_VerifyItemInCart_{itemToTest.Replace(" ", "")}", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var cartItems = shoppingCartPage.GetCartItems().ToList();
            cartItems.Count.ShouldBe(1, $"Expected 1 item in cart, but found {cartItems.Count}.");
            TestLogger.LogDebug("Found {CartItemCount} items in cart. Expected 1.", cartItems.Count);

            CartItemComponent? cartItem = cartItems.FirstOrDefault(ci => ci.ItemName == itemToTest);
            _ = cartItem.ShouldNotBeNull($"Item '{itemToTest}' should be present in the cart.");
            TestLogger.LogInformation("Verified item '{ItemName}' is present in the cart.", itemToTest);

            // Optionally, verify other details like quantity if necessary, though not explicitly requested.
            // cartItem.Quantity.ShouldBe(1, $"Quantity for '{itemToTest}' should be 1.");

            verifyCartTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 2000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Cart navigation and item verification successful.", currentTestName);
    }

    /// <summary>
    /// Verifies that a standard user can add multiple items to the cart, then remove one item,
    /// and the cart updates correctly.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Navigates to the InventoryPage.</description></item>
    ///   <item><description>Defines two items to add: "Sauce Labs Backpack" and "Sauce Labs Bike Light".</description></item>
    ///   <item><description>Adds both items to the cart from InventoryPage, verifying UI changes (button text, cart badge).</description></item>
    ///   <item><description>Navigates to the ShoppingCartPage.</description></item>
    ///   <item><description>Verifies both added items are present in the cart.</description></item>
    ///   <item><description>Defines "Sauce Labs Bike Light" as the item to remove.</description></item>
    ///   <item><description>Clicks the "Remove" button for "Sauce Labs Bike Light" on ShoppingCartPage.</description></item>
    ///   <item><description>Verifies "Sauce Labs Bike Light" is no longer listed in the cart.</description></item>
    ///   <item><description>Verifies "Sauce Labs Backpack" is still listed in the cart.</description></item>
    ///   <item><description>Verifies the cart item count is 1.</description></item>
    ///   <item><description>Verifies the shopping cart badge on the header also updates to 1 (after returning to inventory page or by checking directly if possible).</description></item>
    /// </list>
    /// Performance and resource usage are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Standard user removes an item from cart")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies that a standard user can add multiple items and then successfully remove one from the cart page.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void TestStandardUserCanRemoveItemFromCart()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for standard_user remove item from cart", currentTestName);

        var itemsToInitiallyAdd = new List<string> { "Sauce Labs Backpack", "Sauce Labs Bike Light" };
        string itemToKeep = "Sauce Labs Backpack";
        string itemToRemove = "Sauce Labs Bike Light";

        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAsStandardUserAndNavigateToInventoryPage();

        // --- Add Multiple Items to Cart Step ---
        using (var addItemsTimer = new PerformanceTimer("TestStep_AddMultipleItems_ForRemoveItemTest", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var inventoryItemsOnPage = inventoryPage.GetInventoryItems().ToList();
            int itemsAddedCount = 0;
            foreach (string itemName in itemsToInitiallyAdd)
            {
                InventoryItemComponent? itemComponent = inventoryItemsOnPage.FirstOrDefault(i => i.ItemName == itemName);
                _ = itemComponent.ShouldNotBeNull($"Item '{itemName}' should be found on the inventory page.");

                itemComponent.ClickActionButton();
                TestLogger.LogInformation("Clicked 'Add to cart' for '{ItemName}'.", itemName);
                itemsAddedCount++;

                try
                {
                    _ = wait.Until(d => itemComponent.GetActionButtonText() == "Remove");
                    TestLogger.LogInformation("Button text for '{ItemName}' changed to 'Remove'.", itemName);
                }
                catch (WebDriverTimeoutException)
                {
                    TestLogger.LogError("Timeout waiting for button text to change for '{ItemName}'. Current: {Text}", itemName, itemComponent.GetActionButtonText());
                    itemComponent.GetActionButtonText().ShouldBe("Remove", $"Button for '{itemName}' should be 'Remove'.");
                }
            }
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(itemsToInitiallyAdd.Count, $"Shopping cart badge should be {itemsToInitiallyAdd.Count}.");
            TestLogger.LogInformation("Successfully added {Count} items to cart. Badge count verified.", itemsToInitiallyAdd.Count);
            addItemsTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 10000);
        }

        // --- Navigate to Cart and Verify Initial State ---
        TestLogger.LogInformation("Navigating to shopping cart page.");
        ShoppingCartPage shoppingCartPage;
        using (var navTimer = new PerformanceTimer("TestStep_NavigateToCart_ForRemoveItemTest", TestLogger, resourceMonitor: ResourceMonitor))
        {
            shoppingCartPage = inventoryPage.ClickShoppingCartLink()
                .ShouldBeOfType<ShoppingCartPage>("Clicking cart icon should lead to ShoppingCartPage.");
            navTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        using (var verifyInitialCartTimer = new PerformanceTimer("TestStep_VerifyInitialCart_ForRemoveItemTest", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var cartItems = shoppingCartPage.GetCartItems().ToList();
            cartItems.Count.ShouldBe(itemsToInitiallyAdd.Count, $"Expected {itemsToInitiallyAdd.Count} items in cart initially.");
            foreach (string itemName in itemsToInitiallyAdd)
            {
                cartItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be in cart initially.");
            }
            TestLogger.LogInformation("Verified all {Count} initially added items are in the cart.", itemsToInitiallyAdd.Count);
            verifyInitialCartTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        // --- Remove Specific Item from Cart ---
        using (var removeItemTimer = new PerformanceTimer($"TestStep_RemoveItem_{itemToRemove.Replace(" ", "")}", TestLogger, resourceMonitor: ResourceMonitor))
        {
            _ = shoppingCartPage.RemoveItemByName(itemToRemove)
                .ShouldNotBeNull($"Remove button for '{itemToRemove}' should be found and clicked.");
            TestLogger.LogInformation("Clicked 'Remove' for item '{ItemName}'. Waiting for cart to update.", itemToRemove);

            try
            {
                _ = wait.Until(d => !shoppingCartPage.GetCartItems().Any(i => i.ItemName == itemToRemove));
                TestLogger.LogInformation("Item '{ItemName}' successfully removed from cart.", itemToRemove);
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogError("Timeout waiting for item '{ItemName}' to disappear from cart.", itemToRemove);
                shoppingCartPage.GetCartItems().ShouldNotContain(i => i.ItemName == itemToRemove, $"Item '{itemToRemove}' should no longer be in cart.");
            }
            removeItemTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        // --- Verify Cart State After Removal ---
        using (var verifyAfterRemovalTimer = new PerformanceTimer("TestStep_VerifyCartAfterRemoval", TestLogger, resourceMonitor: ResourceMonitor))
        {
            var remainingCartItems = shoppingCartPage.GetCartItems().ToList();
            remainingCartItems.Count.ShouldBe(1, "Cart should contain 1 item after removal.");
            TestLogger.LogDebug("Cart item count is 1 as expected.");

            remainingCartItems.ShouldContain(item => item.ItemName == itemToKeep, $"Item '{itemToKeep}' should still be in cart.");
            TestLogger.LogInformation("Verified item '{ItemName}' is still present in the cart.", itemToKeep);

            remainingCartItems.ShouldNotContain(item => item.ItemName == itemToRemove, $"Item '{itemToRemove}' should no longer be in cart.");
            TestLogger.LogInformation("Verified item '{ItemName}' is no longer present in the cart.", itemToRemove);

            // Verify header cart badge. Need to navigate back or have a common header component.
            // For simplicity, we'll check badge on inventory page after returning.
            inventoryPage = shoppingCartPage.ClickContinueShopping();
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge on header should be 1 after removal and returning to inventory.");
            TestLogger.LogInformation("Verified shopping cart badge on header is 1 after returning to inventory page.");

            verifyAfterRemovalTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 4000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Item removal from cart functionality verified.", currentTestName);
    }

    /// <summary>
    /// Verifies that the shopping cart badge count on the InventoryPage updates correctly
    /// as items are added and removed through the full cart interaction cycle.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Logs in as the standard_user.</description></item>
    ///   <item><description>Navigates to the InventoryPage.</description></item>
    ///   <item><description>Defines two items: "Sauce Labs Bolt T-Shirt" (item1) and "Sauce Labs Fleece Jacket" (item2).</description></item>
    ///   <item><description>Adds item1 to the cart. Verifies InventoryPage cart badge is 1.</description></item>
    ///   <item><description>Adds item2 to the cart. Verifies InventoryPage cart badge is 2.</description></item>
    ///   <item><description>Navigates to the ShoppingCartPage.</description></item>
    ///   <item><description>Removes item1 from the cart.</description></item>
    ///   <item><description>Verifies ShoppingCartPage shows 1 item (item2).</description></item>
    ///   <item><description>Clicks "Continue Shopping" to return to InventoryPage.</description></item>
    ///   <item><description>Verifies InventoryPage cart badge is 1.</description></item>
    /// </list>
    /// Performance and resource usage are measured.
    /// </remarks>
    [Test]
    [Retry(2)]
    [AllureStep("Verify cart item count updates on Inventory Page")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureDescription("Verifies that the cart badge count on the Inventory Page correctly reflects items being added and removed.")]
    [AllureLink("SauceDemo Site", "https://www.saucedemo.com")]
    public void TestStandardUserCartItemCountUpdatesCorrectlyOnInventoryPage()
    {
        string currentTestName = TestContext.CurrentContext.Test.Name;
        TestLogger.LogInformation("Starting test: {TestName} for cart badge count updates on Inventory Page", currentTestName);

        string item1_NameToAddAndRemove = "Sauce Labs Bolt T-Shirt";
        string item2_NameToAddAndKeep = "Sauce Labs Fleece Jacket";

        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAsStandardUserAndNavigateToInventoryPage();

        // --- Add First Item and Verify Badge ---
        using (var addItem1Timer = new PerformanceTimer($"TestStep_AddItem1_{item1_NameToAddAndRemove.Replace(" ", "")}", TestLogger, resourceMonitor: ResourceMonitor))
        {
            InventoryItemComponent? itemComponent1 = inventoryPage.GetInventoryItems().FirstOrDefault(i => i.ItemName == item1_NameToAddAndRemove);
            _ = itemComponent1.ShouldNotBeNull($"Item '{item1_NameToAddAndRemove}' should be found.");
            itemComponent1.ClickActionButton();
            TestLogger.LogInformation("Added '{ItemName}' to cart.", item1_NameToAddAndRemove);
            _ = wait.Until(d => itemComponent1.GetActionButtonText() == "Remove");
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Cart badge should be 1 after adding first item.");
            TestLogger.LogInformation("Inventory page cart badge is 1, as expected.");
            addItem1Timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        // --- Add Second Item and Verify Badge ---
        using (var addItem2Timer = new PerformanceTimer($"TestStep_AddItem2_{item2_NameToAddAndKeep.Replace(" ", "")}", TestLogger, resourceMonitor: ResourceMonitor))
        {
            InventoryItemComponent? itemComponent2 = inventoryPage.GetInventoryItems().FirstOrDefault(i => i.ItemName == item2_NameToAddAndKeep);
            _ = itemComponent2.ShouldNotBeNull($"Item '{item2_NameToAddAndKeep}' should be found.");
            itemComponent2.ClickActionButton();
            TestLogger.LogInformation("Added '{ItemName}' to cart.", item2_NameToAddAndKeep);
            _ = wait.Until(d => itemComponent2.GetActionButtonText() == "Remove");
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(2, "Cart badge should be 2 after adding second item.");
            TestLogger.LogInformation("Inventory page cart badge is 2, as expected.");
            addItem2Timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        // --- Navigate to ShoppingCartPage ---
        ShoppingCartPage shoppingCartPage;
        using (var navToCartTimer = new PerformanceTimer("TestStep_NavigateToCart_ForInventoryBadgeTest", TestLogger, resourceMonitor: ResourceMonitor))
        {
            shoppingCartPage = inventoryPage.ClickShoppingCartLink()
                .ShouldBeOfType<ShoppingCartPage>("Clicking cart icon should lead to ShoppingCartPage.");
            TestLogger.LogInformation("Navigated to ShoppingCartPage.");
            navToCartTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        // --- Remove First Item from ShoppingCartPage ---
        using (var removeItemTimer = new PerformanceTimer($"TestStep_RemoveItem1_{item1_NameToAddAndRemove.Replace(" ", "")}_FromCartPage", TestLogger, resourceMonitor: ResourceMonitor))
        {
            _ = shoppingCartPage.RemoveItemByName(item1_NameToAddAndRemove)
                .ShouldNotBeNull($"Remove button for '{item1_NameToAddAndRemove}' should be clicked.");
            TestLogger.LogInformation("Removed '{ItemName}' from cart page.", item1_NameToAddAndRemove);
             _ = wait.Until(d => !shoppingCartPage.GetCartItems().Any(i => i.ItemName == item1_NameToAddAndRemove));
            shoppingCartPage.GetCartItems().ToList().Count.ShouldBe(1, "Cart should show 1 item after removal.");
            shoppingCartPage.GetCartItems().First().ItemName.ShouldBe(item2_NameToAddAndKeep, $"Item '{item2_NameToAddAndKeep}' should be the one remaining in cart.");
            TestLogger.LogInformation("Cart page shows 1 item ('{ItemName}') as expected.", item2_NameToAddAndKeep);
            removeItemTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 5000);
        }

        // --- Click "Continue Shopping" to go back to InventoryPage ---
        using (var continueShoppingTimer = new PerformanceTimer("TestStep_ContinueShopping_ForInventoryBadgeTest", TestLogger, resourceMonitor: ResourceMonitor))
        {
            inventoryPage = shoppingCartPage.ClickContinueShopping()
                .ShouldBeOfType<InventoryPage>("Clicking 'Continue Shopping' should lead to InventoryPage.");
            TestLogger.LogInformation("Navigated back to InventoryPage.");
            continueShoppingTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 3000);
        }

        // --- Verify Shopping Cart Badge on InventoryPage is 1 ---
        using (var verifyBadgeTimer = new PerformanceTimer("TestStep_VerifyFinalBadge_InventoryPage", TestLogger, resourceMonitor: ResourceMonitor))
        {
            inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Cart badge on InventoryPage should be 1 after returning.");
            TestLogger.LogInformation("Inventory page cart badge is 1, as expected after removal and return.");
            verifyBadgeTimer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: 1000);
        }

        TestLogger.LogInformation("Finished test: {TestName}. Cart item count updates correctly on Inventory Page.", currentTestName);
    }
}
