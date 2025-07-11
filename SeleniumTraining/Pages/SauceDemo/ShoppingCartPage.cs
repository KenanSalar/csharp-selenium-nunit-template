namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the Shopping Cart page of the saucedemo.com application.
/// This page object provides functionalities for viewing and managing items in the cart,
/// and for proceeding to checkout or returning to the inventory.
/// </summary>
/// <remarks>
/// This page inherits from <see cref="BasePage"/> to leverage common page functionalities
/// like WebDriver access, waits, logging, and settings.
/// It utilizes locators defined in <see cref="ShoppingCartPageMap"/> and interacts with
/// <see cref="CartItemComponent"/> instances to represent individual items within the cart.
/// The page's readiness is confirmed by checking critical elements and the URL path.
/// </remarks>
public class ShoppingCartPage : BasePage
{
    /// <inheritdoc cref="BasePage.CriticalElementsToEnsureVisible"/>
    /// <remarks>
    /// For the Shopping Cart Page, critical elements include the checkout and continue shopping buttons.
    /// </remarks>
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => ShoppingCartPageMap.ShoppingCartPageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCartPage"/> class.
    /// It ensures that the browser has navigated to the correct cart page URL.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance used for browser interaction.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic.</param>
    public ShoppingCartPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
    }

    /// <summary>
    /// Asserts that the ShoppingCartPage is fully loaded by performing base checks and
    /// verifying the page URL.
    /// </summary>
    /// <returns>The current ShoppingCartPage instance for fluent chaining.</returns>
    public override ShoppingCartPage AssertPageIsLoaded()
    {
        _ = base.AssertPageIsLoaded();

        PageLogger.LogDebug("Performing ShoppingCartPage-specific validation (URL check).");

        string expectedPath = ShoppingCartPageMap.PageUrlPath;
        Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase))
            .ShouldBeTrue($"Expected URL to contain '{expectedPath}' but was '{Driver.Url}'.");

        PageLogger.LogInformation("{PageName} URL verified.", PageName);

        return this;
    }

    /// <summary>
    /// Retrieves all items currently displayed in the shopping cart as a list of <see cref="CartItemComponent"/>s.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="CartItemComponent"/> representing the items in the cart.
    /// Returns an empty list if no items are found.</returns>
    [AllureStep("Get all items from the shopping cart")]
    public List<CartItemComponent> GetCartItems()
    {
        PageLogger.LogDebug("Attempting to find all cart item elements.");

        IEnumerable<IWebElement> itemElements = Driver.FindElements(ShoppingCartPageMap.CartItem);

        if (!itemElements.Any())
        {
            PageLogger.LogInformation("No cart items found on the page.");
            return [];
        }

        PageLogger.LogInformation("Found {Count} cart item elements. Creating components.", itemElements.Count());
        return itemElements.Select(element => new CartItemComponent(element, Driver, LoggerFactory, PageSettingsProvider, Retry)).ToList();
    }

    /// <summary>
    /// Removes a specified item from the shopping cart by its displayed name.
    /// </summary>
    /// <param name="itemName">The name of the item to remove from the cart.</param>
    /// <returns>The current <see cref="ShoppingCartPage"/> instance, allowing for fluent method chaining.</returns>
    /// <exception cref="NoSuchElementException">Thrown if no item with the specified <paramref name="itemName"/> is found in the cart.</exception>
    [AllureStep("Remove item '{itemName}' from cart")]
    public ShoppingCartPage RemoveItemByName(string itemName)
    {
        CartItemComponent? itemToRemove = GetCartItems().FirstOrDefault(item => item.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (itemToRemove == null)
        {
            PageLogger.LogError("Item '{ItemNameToRemove}' not found in the cart to remove.", itemName);
            throw new NoSuchElementException($"Item '{itemName}' not found in the cart.");
        }

        PageLogger.LogInformation("Attempting to remove item: {ItemName}", itemName);
        itemToRemove.ClickRemoveButton();
        PageLogger.LogInformation("Clicked 'Remove' for item: {ItemName}. Page might refresh or item disappear.", itemName);

        return this;
    }

    /// <summary>
    /// Clicks the "Checkout" button to proceed to the first step of the checkout process.
    /// </summary>
    /// <remarks>
    /// This method returns a generic <see cref="BasePage"/> to provide flexibility.
    /// The caller is responsible for casting the returned object to the expected page type,
    /// for example: `(CheckoutStepOnePage)shoppingCartPage.ClickCheckout();`.
    /// </remarks>
    /// <returns>A new <see cref="BasePage"/> instance representing the loaded checkout page, which needs to be cast to the correct type.</returns>
    [AllureStep("Click 'Checkout' button")]
    public BasePage ClickCheckout()
    {
        PageLogger.LogInformation("Clicking 'Checkout' button.");
        try
        {
            FindElementOnPage(ShoppingCartPageMap.CheckoutButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Checkout' button.");
        }
        catch (ElementClickInterceptedException ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Checkout' button because it was intercepted by another element.");
            throw;
        }
        catch (WebDriverException ex)
        {
            PageLogger.LogError(ex, "A WebDriver error occurred while trying to click the 'Checkout' button.");
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while clicking the 'Checkout' button.");
            throw;
        }

        return new CheckoutStepOnePage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }

    /// <summary>
    /// Clicks the "Continue Shopping" button to navigate back to the inventory page.
    /// </summary>
    /// <returns>A new <see cref="InventoryPage"/> instance.</returns>
    [AllureStep("Click 'Continue Shopping' button")]
    public InventoryPage ClickContinueShopping()
    {
        PageLogger.LogInformation("Clicking 'Continue Shopping' button.");
        try
        {
            FindElementOnPage(ShoppingCartPageMap.ContinueShoppingButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Continue Shopping' button.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Continue Shopping' button. The test will likely fail on the next page validation.");

            throw;
        }

        return new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
