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
    ///   <item><description>Uses a helper method to log in as the standard_user and add three specific items to the cart.</description></item>
    ///   <item><description>Navigates to the ShoppingCartPage.</description></item>
    ///   <item><description>Verifies all three added items are present in the cart by name and count.</description></item>
    ///   <item><description>Removes two specific items from the cart, waiting for each to disappear from the view.</description></item>
    ///   <item><description>Verifies that only the one expected item remains in the cart.</description></item>
    ///   <item><description>Navigates back to the InventoryPage using "Continue Shopping".</description></item>
    ///   <item><description>Verifies the cart badge count on the InventoryPage is now 1.</description></item>
    /// </list>
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

        // --- ARRANGE ---
        var itemsToAdd = new List<string> { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        var itemsToRemove = new List<string> { "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        const string itemToRemain = "Sauce Labs Backpack";
        var wait = new WebDriverWait(LifecycleManager.WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        // --- Login Step ---
        InventoryPage inventoryPage = LoginAndAddItemsToCart(itemsToAdd);

        // --- ACT ---
        // 1. Navigate to cart and verify initial state
        TestLogger.LogInformation("Navigating to shopping cart page.");
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();

        var cartItems = shoppingCartPage.GetCartItems().ToList();
        cartItems.Count.ShouldBe(itemsToAdd.Count, $"Expected {itemsToAdd.Count} items in cart, but found {cartItems.Count}.");
        foreach (string itemName in itemsToAdd)
        {
            cartItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be present in the cart.");
        }
        TestLogger.LogInformation("Verified all {ItemCount} added items are present in the cart.", itemsToAdd.Count);

        // 2. Remove items from the cart
        foreach (string itemName in itemsToRemove)
        {
            _ = shoppingCartPage.RemoveItemByName(itemName);
            TestLogger.LogInformation("Clicked 'Remove' for '{ItemName}'. Waiting for item to disappear from cart.", itemName);

            // Explicitly wait for the item to be gone from the DOM
            _ = wait.Until(_ => !shoppingCartPage.GetCartItems().Any(i => i.ItemName == itemName));
            TestLogger.LogInformation("Verified item '{ItemName}' is no longer visible in the cart.", itemName);
        }

        // --- ASSERT ---
        // 3. Verify the final state of the cart
        var remainingCartItems = shoppingCartPage.GetCartItems().ToList();
        remainingCartItems.Count.ShouldBe(1, "Expected 1 item to remain in the cart.");
        remainingCartItems.First().ItemName.ShouldBe(itemToRemain, $"Expected '{itemToRemain}' to be the only item left.");
        TestLogger.LogInformation("Verified item removal. '{ItemName}' is the only item remaining.", itemToRemain);

        // 4. Verify state after returning to inventory page
        inventoryPage = shoppingCartPage.ClickContinueShopping();
        inventoryPage.GetShoppingCartBadgeCount().ShouldBe(1, "Shopping cart badge count should be 1 after removals.");

        TestLogger.LogInformation("Finished test: {TestName}. Cart add and remove functionality verified.", currentTestName);
    }
}
