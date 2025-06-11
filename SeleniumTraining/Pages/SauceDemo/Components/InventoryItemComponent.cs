namespace SeleniumTraining.Pages.SauceDemo.Components;

/// <summary>
/// Represents a single inventory item (product card) as displayed on the SauceDemo inventory page.
/// This component provides access to the item's image, name, description, price, and action button (e.g., "Add to cart").
/// </summary>
/// <remarks>
/// This component inherits from <see cref="BasePageComponent"/> to gain common component functionalities,
/// such as a scoped <see cref="BasePageComponent.RootElement"/>, WebDriver access, logging, and retry capabilities.
/// It uses locators defined in <see cref="InventoryItemComponentMap"/> to find its sub-elements.
/// Actions like clicking the "Add to cart" or "Remove" button are performed via the <see cref="ClickActionButton"/> method,
/// which includes retry logic.
/// </remarks>
public class InventoryItemComponent : BasePageComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItemComponent"/> class.
    /// </summary>
    /// <param name="rootElement">The root <see cref="IWebElement"/> that encapsulates this inventory item. Passed to base.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance. Passed to base.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Passed to base.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Passed to base.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Passed to base.</param>
    /// <exception cref="ArgumentNullException">Thrown by the base constructor if any of the required parameters are null.</exception>
    /// <remarks>
    /// Upon construction, it calls the base constructor and logs a debug message including a snippet
    /// of the root element's outer HTML for identification purposes.
    /// </remarks>
    public InventoryItemComponent(
        IWebElement rootElement,
        IWebDriver driver,
        ILoggerFactory loggerFactory,
        ISettingsProviderService settingsProvider,
        IRetryService retryService
    )
        : base(rootElement, driver, loggerFactory, settingsProvider, retryService)
    {
        string outerHtml = RootElement.GetAttribute("outerHTML") ?? string.Empty;
        ComponentLogger.LogDebug(
            "InventoryItemComponent initialized for element: {RootElementId}",
            outerHtml[..Math.Min(100, outerHtml.Length)]
        );
    }

    /// <summary>
    /// Gets the <see cref="IWebElement"/> representing the image of this inventory item.
    /// </summary>
    /// <value>The item's image web element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item image element cannot be found within this component.</exception>
    public IWebElement ItemImage
    {
        get
        {
            ComponentLogger.LogTrace(
                "Attempting to find ItemImage using locator: {Locator}",
                InventoryItemComponentMap.ItemImage
            );
            return FindElement(InventoryItemComponentMap.ItemImage);
        }
    }

    /// <summary>
    /// Gets the name (title) of this inventory item as displayed on the page.
    /// </summary>
    /// <value>The text content of the item's name element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item name element cannot be found within this component.</exception>
    public string ItemName
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemName element using locator: {Locator}", InventoryItemComponentMap.ItemName);
            string name = FindElement(InventoryItemComponentMap.ItemName).Text;
            ComponentLogger.LogDebug("Retrieved ItemName: {Name}", name);

            return name;
        }
    }

    /// <summary>
    /// Gets the description text of this inventory item.
    /// </summary>
    /// <value>The text content of the item's description element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item description element cannot be found within this component.</exception>
    public string ItemDescription
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemDescription element using locator: {Locator}", InventoryItemComponentMap.ItemDescription);
            string description = FindElement(InventoryItemComponentMap.ItemDescription).Text;
            ComponentLogger.LogDebug("Retrieved ItemDescription: {Description}", description);
            return description;
        }
    }

    /// <summary>
    /// Gets the price of this inventory item as displayed on the page (e.g., "$29.99").
    /// </summary>
    /// <value>The text content of the item's price element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the item price element cannot be found within this component.</exception>
    public string ItemPrice
    {
        get
        {
            ComponentLogger.LogTrace("Attempting to find ItemPrice element using locator: {Locator}", InventoryItemComponentMap.ItemPrice);
            string price = FindElement(InventoryItemComponentMap.ItemPrice).Text;
            ComponentLogger.LogDebug("Retrieved ItemPrice: {Price}", price);
            return price;
        }
    }

    /// <summary>
    /// Clicks the action button for this inventory item (e.g., "Add to cart" or "Remove").
    /// This action is performed with a retry policy to handle transient UI issues.
    /// </summary>
    /// <remarks>
    /// This method utilizes the <see cref="IRetryService.ExecuteWithRetry(Action, int, TimeSpan?, ILogger?)"/>
    /// from the <see cref="BasePageComponent.Retry"/> service. Element highlighting is applied to the button
    /// before clicking if enabled in framework settings.
    /// The operation is logged with the button text and item name.
    /// After clicking, the component's internal element cache is cleared via <see cref="BasePageComponent.ClearComponentElementCache()"/>
    /// as the button's state (e.g., text) is expected to change.
    /// </remarks>
    /// <exception cref="ElementClickInterceptedException">May be thrown by the retry mechanism if the click is consistently intercepted.</exception>
    /// <exception cref="NoSuchElementException">May be thrown by the retry mechanism if the button is not found after retries, or by <see cref="ItemName"/> if the item name cannot be found for logging.</exception>
    /// <exception cref="AggregateException">Thrown by Polly if all retry attempts fail, containing the exceptions from all attempts.</exception>
    [AllureStep("Click item action button")]
    public void ClickActionButton()
    {
        Retry.ExecuteWithRetry(() =>
            {
                ComponentLogger.LogTrace("Attempting to find ActionButton element using locator: {Locator}", InventoryItemComponentMap.ActionButton);
                IWebElement button = FindElement(InventoryItemComponentMap.ActionButton);
                string buttonText = button.Text;

                _ = HighlightIfEnabled(button);

                ComponentLogger.LogInformation("Clicking action button with text '{ButtonText}' for item '{ItemName}'.", buttonText, ItemName);
                button.Click();
                ComponentLogger.LogInformation("Successfully clicked action button with text '{ButtonText}' for item '{ItemName}'.", buttonText, ItemName);
            },
            maxRetryAttempts: 2,
            initialDelay: TimeSpan.FromMilliseconds(500),
            actionLogger: ComponentLogger
        );
    }

    /// <summary>
    /// Gets the current text of the action button for this inventory item (e.g., "Add to cart", "Remove").
    /// </summary>
    /// <returns>The text content of the action button.</returns>
    /// <exception cref="NoSuchElementException">Thrown if the action button cannot be found within this component.</exception>
    public string GetActionButtonText()
    {
        ComponentLogger.LogTrace("Attempting to find ActionButton element to get text, using locator: {Locator}", InventoryItemComponentMap.ActionButton);
        string text = ActionButtonElement.Text;
        ComponentLogger.LogDebug("Retrieved ActionButton text: {Text}", text);
        return text;
    }

    /// <summary>
    /// Gets the <see cref="IWebElement"/> representing the action button for this inventory item
    /// (e.g., "Add to cart" or "Remove" button).
    /// </summary>
    /// <value>The action button web element.</value>
    /// <exception cref="NoSuchElementException">Thrown if the action button element cannot be found within this component.</exception>
    public IWebElement ActionButtonElement
    {
        get
        {
            ComponentLogger.LogTrace(
                "Attempting to find ActionButtonElement using locator: {Locator}",
                InventoryItemComponentMap.ActionButton
            );
            return FindElement(InventoryItemComponentMap.ActionButton);
        }
    }
}
