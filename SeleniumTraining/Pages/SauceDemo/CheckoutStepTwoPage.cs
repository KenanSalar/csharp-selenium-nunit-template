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
    /// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Must not be null.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Must not be null.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Must not be null.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Must not be null.</param>
    public CheckoutStepTwoPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        PageLogger.LogDebug("{PageName} instance created. Call AssertPageIsLoaded() to verify.", PageName);
    }

    /// <summary>
    /// Asserts that the CheckoutStepTwoPage is fully loaded by performing base checks and
    /// verifying the page URL and title.
    /// </summary>
    /// <returns>The current CheckoutStepTwoPage instance for fluent chaining.</returns>
    public override CheckoutStepTwoPage AssertPageIsLoaded()
    {
        _ = base.AssertPageIsLoaded();

        PageLogger.LogDebug("Performing {PageName}-specific validation (URL and Title check).", PageName);
        string expectedPath = CheckoutStepTwoPageMap.PageUrlPath;
        bool urlCorrect = Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase));
        urlCorrect.ShouldBeTrue($"Landed on Checkout Step Two page, but URL was expected to contain '{expectedPath}'. Current URL: '{Driver.Url}'.");

        IWebElement titleElement = FindElementOnPage(CheckoutStepTwoPageMap.PageTitle);
        titleElement.Text.ShouldBe(CheckoutStepTwoPageMap.PageTitleText, $"Page title should be '{CheckoutStepTwoPageMap.PageTitleText}'.");
        PageLogger.LogInformation("{PageName} URL and Title verified.", PageName);

        return this;
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
        try
        {
            FindElementOnPage(CheckoutStepTwoPageMap.FinishButton)
                .ClickStandard(Driver,Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Finish' button using JavaScript.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Finish' button.");
            throw;
        }

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
        try
        {
            FindElementOnPage(CheckoutStepTwoPageMap.CancelButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Cancel' button using JavaScript.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Cancel' button.");
            throw;
        }

        return new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
