namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the "Checkout: Your Information" page (Step One) of the SauceDemo application.
/// This page object allows entering user details required to proceed with the checkout.
/// </summary>
/// <remarks>
/// Inherits from <see cref="BasePage"/>. Uses locators from <see cref="CheckoutStepOnePageMap"/>.
/// </remarks>
public class CheckoutStepOnePage : BasePage
{
    /// <inheritdoc/>
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => CheckoutStepOnePageMap.CheckoutStepOnePageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckoutStepOnePage"/> class.
    /// Verifies that the current page is indeed the checkout step one page by checking URL and title.
    /// </summary>
    public CheckoutStepOnePage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        string expectedPath = CheckoutStepOnePageMap.PageUrlPath;
        bool urlCorrect = Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase));
        urlCorrect.ShouldBeTrue($"Landed on Checkout Step One page, but URL was expected to contain '{expectedPath}'. Current URL: '{Driver.Url}'.");

        IWebElement titleElement = FindElementOnPage(CheckoutStepOnePageMap.PageTitle);
        titleElement.Text.ShouldBe(CheckoutStepOnePageMap.PageTitleText, $"Page title should be '{CheckoutStepOnePageMap.PageTitleText}'.");
        PageLogger.LogInformation("{PageName} loaded and URL/Title verified.", PageName);
    }

    /// <summary>
    /// Enters the first name into the corresponding input field.
    /// </summary>
    /// <param name="firstName">The first name to enter.</param>
    /// <returns>The current <see cref="CheckoutStepOnePage"/> instance for fluent chaining.</returns>
    [AllureStep("Enter First Name: {firstName}")]
    public CheckoutStepOnePage EnterFirstName(string firstName)
    {
        PageLogger.LogInformation("Entering First Name: {FirstName}", firstName);
        HighlightIfEnabled(CheckoutStepOnePageMap.FirstNameInput).SendKeys(firstName);
        return this;
    }

    /// <summary>
    /// Enters the last name into the corresponding input field.
    /// </summary>
    /// <param name="lastName">The last name to enter.</param>
    /// <returns>The current <see cref="CheckoutStepOnePage"/> instance for fluent chaining.</returns>
    [AllureStep("Enter Last Name: {lastName}")]
    public CheckoutStepOnePage EnterLastName(string lastName)
    {
        PageLogger.LogInformation("Entering Last Name: {LastName}", lastName);
        HighlightIfEnabled(CheckoutStepOnePageMap.LastNameInput).SendKeys(lastName);
        return this;
    }

    /// <summary>
    /// Enters the postal code into the corresponding input field.
    /// </summary>
    /// <param name="postalCode">The postal code to enter.</param>
    /// <returns>The current <see cref="CheckoutStepOnePage"/> instance for fluent chaining.</returns>
    [AllureStep("Enter Postal Code: {postalCode}")]
    public CheckoutStepOnePage EnterPostalCode(string postalCode)
    {
        PageLogger.LogInformation("Entering Postal Code: {PostalCode}", postalCode);
        HighlightIfEnabled(CheckoutStepOnePageMap.PostalCodeInput).SendKeys(postalCode);
        return this;
    }

    /// <summary>
    /// Fills all information fields (First Name, Last Name, Postal Code) and clicks the "Continue" button.
    /// </summary>
    /// <param name="firstName">The first name to enter.</param>
    /// <param name="lastName">The last name to enter.</param>
    /// <param name="postalCode">The postal code to enter.</param>
    /// <returns>A new <see cref="CheckoutStepTwoPage"/> instance representing the next page in the flow.</returns>
    [AllureStep("Fill checkout information and continue. Name: {firstName} {lastName}, Zip: {postalCode}")]
    public CheckoutStepTwoPage FillInformationAndContinue(string firstName, string lastName, string postalCode)
    {
        _ = EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterPostalCode(postalCode);
        return ClickContinue();
    }

    /// <summary>
    /// Clicks the "Continue" button to proceed to the next step of checkout.
    /// </summary>
    /// <returns>A new <see cref="CheckoutStepTwoPage"/> instance.</returns>
    [AllureStep("Click 'Continue' button on checkout step one")]
    public CheckoutStepTwoPage ClickContinue()
    {
        PageLogger.LogInformation("Clicking 'Continue' button.");
        HighlightIfEnabled(CheckoutStepOnePageMap.ContinueButton).Click();
        return new CheckoutStepTwoPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }

    /// <summary>
    /// Clicks the "Cancel" button to return to the shopping cart.
    /// </summary>
    /// <returns>A new <see cref="ShoppingCartPage"/> instance.</returns>
    [AllureStep("Click 'Cancel' button on checkout step one")]
    public ShoppingCartPage ClickCancel()
    {
        PageLogger.LogInformation("Clicking 'Cancel' button.");
        HighlightIfEnabled(CheckoutStepOnePageMap.CancelButton).Click();
        return new ShoppingCartPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
