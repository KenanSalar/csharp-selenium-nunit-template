using OpenQA.Selenium.BiDi.Modules.Input;

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
    /// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Must not be null.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Must not be null.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Must not be null.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Must not be null.</param>
    public CheckoutStepOnePage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
    }

    /// <summary>
    /// Asserts that the CheckoutStepOnePage is fully loaded by performing base checks and
    /// verifying the page URL and title.
    /// </summary>
    /// <returns>The current CheckoutStepOnePage instance for fluent chaining.</returns>
    public override CheckoutStepOnePage AssertPageIsLoaded()
    {
        _ = base.AssertPageIsLoaded();

        PageLogger.LogDebug("Performing {PageName}-specific validation (URL and Title check).", PageName);
        string expectedPath = CheckoutStepOnePageMap.PageUrlPath;
        bool urlCorrect = Wait.Until(d => d.Url.Contains(expectedPath, StringComparison.OrdinalIgnoreCase));
        urlCorrect.ShouldBeTrue($"Landed on Checkout Step One page, but URL was expected to contain '{expectedPath}'. Current URL: '{Driver.Url}'.");

        IWebElement titleElement = FindElementOnPage(CheckoutStepOnePageMap.PageTitle);
        titleElement.Text.ShouldBe(CheckoutStepOnePageMap.PageTitleText, $"Page title should be '{CheckoutStepOnePageMap.PageTitleText}'.");
        PageLogger.LogInformation("{PageName} URL and Title verified.", PageName);

        return this;
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
        try
        {
            IWebElement firstNameInput = FindElementOnPage(CheckoutStepOnePageMap.FirstNameInput);
            _ = Wait.Until(_ => firstNameInput.Displayed && firstNameInput.Enabled);
            _ = HighlightIfEnabled(firstNameInput);

            firstNameInput.Clear();
            firstNameInput.SendKeys(firstName);
        }
        catch (ElementNotInteractableException ex)
        {
            PageLogger.LogError(ex, "Failed to enter First Name - element was not interactable.");
            throw;
        }
        catch (WebDriverException ex) // Catches StaleElement, NoSuchElement, etc.
        {
            PageLogger.LogError(ex, "A WebDriver error occurred while trying to enter the First Name.");
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while entering the First Name.");
            throw;
        }
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
        try
        {
            IWebElement lastNameInput = FindElementOnPage(CheckoutStepOnePageMap.LastNameInput);
            _ = Wait.Until(_ => lastNameInput.Displayed && lastNameInput.Enabled);
            _ = HighlightIfEnabled(lastNameInput);

            lastNameInput.Clear();
            lastNameInput.SendKeys(lastName);
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to enter Last Name.");
            throw;
        }
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
        try
        {
            IWebElement postalCodeInput = FindElementOnPage(CheckoutStepOnePageMap.PostalCodeInput);
            _ = Wait.Until(_ => postalCodeInput.Displayed && postalCodeInput.Enabled);
            _ = HighlightIfEnabled(postalCodeInput);

            postalCodeInput.Clear();
            postalCodeInput.SendKeys(postalCode);
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to enter Postal Code.");
            throw;
        }

        return this;
    }

    /// <summary>
    /// Clicks the "Continue" button to proceed to the next step of checkout.
    /// </summary>
    /// <returns>A new <see cref="CheckoutStepTwoPage"/> instance.</returns>
    [AllureStep("Click 'Continue' button on checkout step one")]
    public CheckoutStepTwoPage ClickContinue()
    {
        PageLogger.LogInformation("Clicking 'Continue' button.");
        try
        {
            FindElementOnPage(CheckoutStepOnePageMap.ContinueButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Continue' button using JavaScript.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Continue' button. The test will likely fail on the next page validation.");

            throw;
        }

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
        try
        {
            FindElementOnPage(CheckoutStepOnePageMap.CancelButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);

            PageLogger.LogInformation("Successfully clicked 'Cancel' button using JavaScript.");
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Could not click the 'Cancel' button.");
            throw;
        }

        return new ShoppingCartPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }
}
