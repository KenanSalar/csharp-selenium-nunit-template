namespace SeleniumTraining.Pages.SauceDemo.Components;

/// <summary>
/// Represents a single item displayed within the shopping cart on the SauceDemo page.
/// This component provides access to the item's details such as its name, description, price, and quantity,
/// as well as the action to remove it from the cart.
/// </summary>
/// <remarks>
/// This component inherits from <see cref="BasePageComponent"/> to gain common functionalities
/// like a scoped root element, WebDriver access, logging, and element caching.
/// It uses locators defined in <see cref="CartItemComponentMap"/> to find its sub-elements relative
/// to its root <see cref="IWebElement"/>.
/// </remarks>
public class CartItemComponent : BasePageComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CartItemComponent"/> class.
    /// </summary>
    /// <param name="rootElement">The root <see cref="IWebElement"/> that encapsulates this cart item. This element is typically one of the items found by <see cref="ShoppingCartPageMap.CartItem"/>.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance, passed to the base component.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers, passed to the base component.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations, passed to the base component.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic, passed to the base component.</param>
    public CartItemComponent(
        IWebElement rootElement,
        IWebDriver driver,
        ILoggerFactory loggerFactory,
        ISettingsProviderService settingsProvider,
        IRetryService retryService)
        : base(rootElement, driver, loggerFactory, settingsProvider, retryService)
    {
        ComponentLogger.LogDebug("CartItemComponent initialized for element.");
    }

    /// <summary>
    /// Gets the name of this cart item as displayed on the page.
    /// </summary>
    /// <value>The text content of the item's name element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item name element cannot be found within this component.</exception>
    public string ItemName => FindElement(CartItemComponentMap.ItemName).Text;

    /// <summary>
    /// Gets the description of this cart item as displayed on the page.
    /// </summary>
    /// <value>The text content of the item's description element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item description element cannot be found within this component.</exception>
    public string ItemDescription => FindElement(CartItemComponentMap.ItemDescription).Text;

    /// <summary>
    /// Gets the price of this cart item as displayed on the page (e.g., "$29.99").
    /// </summary>
    /// <value>The text content of the item's price element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item price element cannot be found within this component.</exception>
    public string ItemPrice => FindElement(CartItemComponentMap.ItemPrice).Text;

    /// <summary>
    /// Gets the quantity of this item in the cart as an integer.
    /// </summary>
    /// <value>The displayed quantity of the item. Returns 0 if the quantity text cannot be parsed or the element is not found.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item quantity element cannot be found within this component.</exception>
    public int Quantity
    {
        get
        {
            string quantityText = FindElement(CartItemComponentMap.ItemQuantity).Text;
            return int.TryParse(quantityText, out int quantity) ? quantity : 0;
        }
    }

    /// <summary>
    /// Clicks the "Remove" button associated with this specific cart item.
    /// This action will remove the item from the shopping cart.
    /// </summary>
    /// <remarks>
    /// After clicking, the component cache is cleared as the item (and thus this component's root element)
    /// will likely be removed from the DOM or the page will refresh.
    /// The calling code (e.g., in <see cref="ShoppingCartPage"/>) should handle re-fetching the list of cart items.
    /// </remarks>
    [AllureStep("Click 'Remove' button for cart item")]
    public void ClickRemoveButton()
    {
        IWebElement removeButton = FindElement(CartItemComponentMap.RemoveButton);
        _ = HighlightIfEnabled(removeButton);
        ComponentLogger.LogInformation("Clicking 'Remove' button for item: {ItemName}", ItemName);
        removeButton.Click();
        
        ClearComponentElementCache();
    }
}
