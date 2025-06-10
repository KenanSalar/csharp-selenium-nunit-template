namespace SeleniumTraining.Tests.SauceDemoTests;

public partial class SauceDemoTests : BaseTest
{
    /// <summary>
    /// Logs in as the standard user by navigating to the login page, entering credentials,
    /// and clicking the login button. It verifies that the navigation to the Inventory Page is successful.
    /// </summary>
    /// <returns>A fully initialized <see cref="InventoryPage"/> instance after a successful login.</returns>
    /// <remarks>
    /// This method encapsulates the entire standard login flow. It uses a performance timer
    /// to measure the login duration but does not attach it to the Allure report by default,
    /// as it's considered a setup step for other tests.
    /// It relies on <see cref="Shouldly"/> to assert a successful navigation to the InventoryPage.
    /// </remarks>
    private InventoryPage LoginAsStandardUser()
    {
        TestLogger.LogInformation("Executing helper method: LoginAsStandardUser");

        var loginTimer = new PerformanceTimer(
            "TestSetupHelper_LoginAsStandardUser",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        LoginPage loginPage = new(WebDriverManager.GetDriver(), PageObjectLoggerFactory, SettingsProvider, RetryService);
        BasePage resultPage = loginPage
            .EnterUsername(_sauceDemoSettings.LoginUsernameStandardUser)
            .EnterPassword(_sauceDemoSettings.LoginPassword)
            .LoginAndExpectNavigation(LoginMode.Click);

        InventoryPage inventoryPage = resultPage.ShouldBeOfType<InventoryPage>("Login helper should successfully navigate to the Inventory Page.");

        loginTimer.StopAndLog(attachToAllure: false);
        TestLogger.LogInformation("Finished helper method: LoginAsStandardUser");

        return inventoryPage;
    }

    /// <summary>
    /// A comprehensive setup helper that first logs in as the standard user and then
    /// adds a specified list of items to the shopping cart from the inventory page.
    /// </summary>
    /// <param name="itemsToAddToCart">An enumerable of strings, where each string is the exact name of an inventory item to add to the cart.</param>
    /// <returns>A fully initialized <see cref="InventoryPage"/> instance after the login and add-to-cart actions are complete.</returns>
    /// <remarks>
    /// This method first calls the <see cref="LoginAsStandardUser"/> helper. It then iterates through the
    /// provided item names, finds each corresponding item component on the inventory page, clicks its "Add to cart" button,
    /// and waits for the button text to update to "Remove" to confirm the action. Finally, it asserts that the
    /// shopping cart badge count matches the number of items added.
    /// </remarks>
    private InventoryPage LoginAndAddItemsToCart(IEnumerable<string> itemsToAddToCart)
    {
        TestLogger.LogInformation("Executing helper method: LoginAndAddItemsToCart for items: {Items}", string.Join(", ", itemsToAddToCart));
        InventoryPage inventoryPage = LoginAsStandardUser();

        var addItemsTimer = new PerformanceTimer(
            "TestSetupHelper_AddItemsToCart",
            TestLogger,
            resourceMonitor: ResourceMonitor
        );

        var allInventoryItems = inventoryPage.GetInventoryItems().ToList();
        var wait = new WebDriverWait(WebDriverManager.GetDriver(), TimeSpan.FromSeconds(10));

        foreach (string itemName in itemsToAddToCart)
        {
            InventoryItemComponent? itemComponent = allInventoryItems.FirstOrDefault(i => i.ItemName == itemName);
            _ = itemComponent.ShouldNotBeNull($"Item '{itemName}' should be found on the inventory page during test setup.");

            itemComponent.ClickActionButton();
            TestLogger.LogDebug("Setup Helper: Added '{ItemName}' to cart.", itemName);

            _ = wait.Until(d => itemComponent.GetActionButtonText() == "Remove");
        }

        inventoryPage.GetShoppingCartBadgeCount().ShouldBe(itemsToAddToCart.Count());
        addItemsTimer.StopAndLog(attachToAllure: false);
        TestLogger.LogInformation("Finished helper method: LoginAndAddItemsToCart");

        return inventoryPage;
    }
}
