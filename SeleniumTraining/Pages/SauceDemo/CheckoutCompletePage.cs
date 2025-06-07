namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the "Checkout: Complete!" page of the SauceDemo application,
/// displayed after a successful purchase.
/// </summary>
/// <remarks>
/// Inherits from <see cref="BasePage"/>. Uses locators from <see cref="CheckoutCompletePageMap"/>.
/// </remarks>
public class CheckoutCompletePage : BasePage
{
    /// <inheritdoc/>
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => CheckoutCompletePageMap.CheckoutCompletePageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckoutCompletePage"/> class.
    /// Verifies that the current page is indeed the checkout complete page.
    /// </summary>
    public CheckoutCompletePage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        string expectedPath = CheckoutCompletePageMap.PageUrlPath;
        bool urlCorrect = Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase));
        urlCorrect.ShouldBeTrue($"Landed on Checkout Complete page, but URL was expected to contain '{expectedPath}'. Current URL: '{Driver.Url}'.");

        IWebElement titleElement = FindElementOnPage(CheckoutCompletePageMap.PageTitle);
        titleElement.Text.ShouldBe(CheckoutCompletePageMap.PageTitleText, $"Page title should be '{CheckoutCompletePageMap.PageTitleText}'.");
        PageLogger.LogInformation("{PageName} loaded and URL/Title verified.", PageName);
    }

    /// <summary>
    /// Gets the main confirmation header text (e.g., "THANK YOU FOR YOUR ORDER").
    /// </summary>
    /// <returns>The text of the complete header.</returns>
    public string GetConfirmationHeaderText()
    {
        return FindElementOnPage(CheckoutCompletePageMap.CompleteHeader).Text;
    }

    /// <summary>
    /// Gets the descriptive completion text.
    /// </summary>
    /// <returns>The text of the completion message body.</returns>
    public string GetCompletionText()
    {
        return FindElementOnPage(CheckoutCompletePageMap.CompleteText).Text;
    }

    /// <summary>
    /// Checks if the "Pony Express" image is displayed on the page.
    /// </summary>
    /// <returns><c>true</c> if the image is displayed; otherwise, <c>false</c>.</returns>
    public bool IsPonyExpressImageDisplayed()
    {
        try
        {
            return FindElementOnPage(CheckoutCompletePageMap.PonyExpressImage).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Clicks the "Back Home" button to return to the inventory page.
    /// </summary>
    /// <returns>A new <see cref="InventoryPage"/> instance.</returns>
    [AllureStep("Click 'Back Home' button")]
    public InventoryPage ClickBackHome()
    {
        PageLogger.LogInformation("Clicking 'Back Home' button.");
        try
        {
            IWebElement backHomeButton = FindElementOnPage(CheckoutCompletePageMap.BackHomeButton);

            _ = Wait.Until(ExpectedConditions.ElementToBeClickable(backHomeButton));

            _ = HighlightIfEnabled(backHomeButton);

            backHomeButton.ClickStandard(Wait, PageLogger);

            PageLogger.LogInformation("Successfully clicked 'Back Home' button using JavaScript.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Back Home' button.");
            throw;
        }

        return new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
