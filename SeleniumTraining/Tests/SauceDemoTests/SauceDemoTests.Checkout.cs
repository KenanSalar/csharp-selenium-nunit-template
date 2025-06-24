namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Verifies that a standard user can successfully complete the entire checkout process
    /// using data provided from an external source.
    /// </summary>
    /// <remarks>
    /// Test Steps:
    /// <list type="number">
    ///   <item><description>Uses a helper method to log in and add specified items to the cart.</description></item>
    ///   <item><description>Navigates to the shopping cart page.</description></item>
    ///   <item><description>Clicks 'Checkout' and casts the resulting <see cref="BasePage"/> to a <see cref="CheckoutStepOnePage"/>.</description></item>
    ///   <item><description>Fills the user information form and continues.</description></item>
    ///   <item><description>Verifies the correct items are in the order overview.</description></item>
    ///   <item><description>Finishes the purchase and verifies the confirmation page.</description></item>
    ///   <item><description>Returns to the inventory page and confirms the cart is empty.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="firstName">The first name to use during checkout.</param>
    /// <param name="lastName">The last name to use during checkout.</param>
    /// <param name="postalCode">The postal code to use during checkout.</param>
    /// <param name="itemsToOrder">The list of item names to add to the cart before checkout.</param>
    [Test]
    [Retry(2)]
    [TestCaseSource(typeof(SauceDemoTests), nameof(CheckoutScenarios))]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureDescription("Verifies the full checkout flow with externally provided data.")]
    public void ShouldSuccessfullyCompleteFullCheckoutFlow(string firstName, string lastName, string postalCode, List<string> itemsToOrder)
    {
        string currentTestName = TestContext.CurrentContext.Test.Name; // This will now use the name from SetName()
        TestLogger.LogInformation("Starting test: {TestName}", currentTestName);
        TestLogger.LogInformation("Test data: Items='{Items}', User='{User}', PostalCode='{Code}'", string.Join(", ", itemsToOrder), $"{firstName} {lastName}", postalCode);

        // --- ARRANGE & ACT ---
        // 1. Setup: Login and add the specified items to the cart.
        InventoryPage inventoryPage = LoginAndAddItemsToCart(itemsToOrder);

        // 2. Navigate through the checkout process.
        ShoppingCartPage shoppingCartPage = inventoryPage.ClickShoppingCartLink();
        var checkoutStepOnePage = (CheckoutStepOnePage)shoppingCartPage.ClickCheckout();

        CheckoutStepTwoPage checkoutStepTwoPage = checkoutStepOnePage
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode)
            .ClickContinue();

        // 3. Verify items in the overview.
        var overviewItems = checkoutStepTwoPage.GetItemsInOverview().ToList();
        overviewItems.Count.ShouldBe(itemsToOrder.Count, $"Expected {itemsToOrder.Count} items in overview.");
        foreach (string itemName in itemsToOrder)
        {
            overviewItems.ShouldContain(item => item.ItemName == itemName, $"Item '{itemName}' should be in overview.");
        }
        TestLogger.LogInformation("Items verified on checkout overview page.");

        // 4. Finish the purchase.
        CheckoutCompletePage checkoutCompletePage = checkoutStepTwoPage.ClickFinish();

        // --- ASSERT ---
        // 5. Verify the confirmation page details.
        checkoutCompletePage.GetConfirmationHeaderText().ShouldBe("Thank you for your order!", StringCompareShould.IgnoreCase);
        checkoutCompletePage.IsPonyExpressImageDisplayed().ShouldBeTrue("Pony Express image should be displayed.");
        TestLogger.LogInformation("Confirmation page details verified.");

        // 6. Go back home and verify the cart is empty.
        InventoryPage finalInventoryPage = checkoutCompletePage.ClickBackHome();
        finalInventoryPage.GetShoppingCartBadgeCount().ShouldBe(0, "Shopping cart should be empty after completing an order.");
        TestLogger.LogInformation("Verified cart is empty on Inventory page.");
        TestLogger.LogInformation("Finished test successfully: {TestName}", currentTestName);
    }
}
