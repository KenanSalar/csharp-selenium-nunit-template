namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the "Checkout: Overview" page (Step Two) of the SauceDemo application.
/// This page allows the user to review their order before finalizing the purchase.
/// </summary>
/// <remarks>
/// Inherits from <see cref="BasePage"/>. Uses locators from <see cref="CheckoutStepTwoPageMap"/>.
/// It can also leverage <see cref="CartItemComponent"/> to represent items in the overview if the HTML structure is similar to the cart page.
/// </remarks>
public class CheckoutStepTwoPage : BasePage
{
    /// <inheritdoc/>
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => CheckoutStepTwoPageMap.CheckoutStepTwoPageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckoutStepTwoPage"/> class.
    /// Verifies that the current page is indeed the checkout overview page.
    /// </summary>
    public CheckoutStepTwoPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        string expectedPath = CheckoutStepTwoPageMap.PageUrlPath;
        bool urlCorrect = Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase));
        urlCorrect.ShouldBeTrue($"Landed on Checkout Step Two page, but URL was expected to contain '{expectedPath}'. Current URL: '{Driver.Url}'.");

        IWebElement titleElement = FindElementOnPage(CheckoutStepTwoPageMap.PageTitle);
        titleElement.Text.ShouldBe(CheckoutStepTwoPageMap.PageTitleText, $"Page title should be '{CheckoutStepTwoPageMap.PageTitleText}'.");
        PageLogger.LogInformation("{PageName} loaded and URL/Title verified.", PageName);
    }

    /// <summary>
    /// Retrieves all items currently displayed in the checkout overview.
    /// Uses the same component as the cart page as the HTML structure for items is similar.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CartItemComponent"/> representing the items.</returns>
    [AllureStep("Get all items from checkout overview")]
    public IEnumerable<CartItemComponent> GetItemsInOverview()
    {
        PageLogger.LogDebug("Attempting to find all cart item elements on overview page using locator: {CartItemLocator}", CheckoutStepTwoPageMap.CartItem);
        IEnumerable<IWebElement> itemElements = Driver.FindElements(CheckoutStepTwoPageMap.CartItem); // Same locator as cart

        if (!itemElements.Any())
        {
            PageLogger.LogInformation("No items found in the checkout overview.");
            return [];
        }

        PageLogger.LogInformation("Found {Count} item elements in overview. Creating components.", itemElements.Count());
        return itemElements.Select(element => new CartItemComponent(element, Driver, LoggerFactory, PageSettingsProvider, Retry)).ToList();
    }

    /// <summary>
    /// Gets the displayed subtotal (Item total) text from the order summary.
    /// </summary>
    /// <returns>The text of the subtotal label (e.g., "Item total: $XX.YY").</returns>
    public string GetSubtotalText()
    {
        return FindElementOnPage(CheckoutStepTwoPageMap.SubtotalLabel).Text;
    }

    /// <summary>
    /// Gets the displayed tax text from the order summary.
    /// </summary>
    /// <returns>The text of the tax label (e.g., "Tax: $X.YZ").</returns>
    public string GetTaxText()
    {
        return FindElementOnPage(CheckoutStepTwoPageMap.TaxLabel).Text;
    }

    /// <summary>
    /// Gets the displayed total price text from the order summary.
    /// </summary>
    /// <returns>The text of the total label (e.g., "Total: $XX.YY").</returns>
    public string GetTotalText()
    {
        return FindElementOnPage(CheckoutStepTwoPageMap.TotalLabel).Text;
    }


    /// <summary>
    /// Clicks the "Finish" button to complete the purchase.
    /// </summary>
    /// <returns>A new <see cref="CheckoutCompletePage"/> instance.</returns>
    [AllureStep("Click 'Finish' button on checkout overview")]
    public CheckoutCompletePage ClickFinish()
    {
        PageLogger.LogInformation("Clicking 'Finish' button.");
        HighlightIfEnabled(CheckoutStepTwoPageMap.FinishButton).Click();
        return new CheckoutCompletePage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }

    /// <summary>
    /// Clicks the "Cancel" button to return to the inventory page.
    /// </summary>
    /// <returns>A new <see cref="InventoryPage"/> instance.</returns>
    [AllureStep("Click 'Cancel' button on checkout overview")]
    public InventoryPage ClickCancel()
    {
        PageLogger.LogInformation("Clicking 'Cancel' button.");
        HighlightIfEnabled(CheckoutStepTwoPageMap.CancelButton).Click();
        return new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
