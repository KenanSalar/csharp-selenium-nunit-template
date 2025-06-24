namespace SeleniumTraining.Pages.SauceDemo;

/// <summary>
/// Represents the Login Page (e.g., for saucedemo.com).
/// This class provides methods for interacting with login form elements,
/// performing login actions, and retrieving error messages.
/// </summary>
/// <remarks>
/// This page object inherits from <see cref="BasePage"/> to gain common page functionalities.
/// It defines critical elements specific to the login page and implements actions
/// like entering username/password and submitting the login form.
/// Login actions can be performed either by submitting the form (e.g., hitting Enter)
/// or by clicking the login button, controlled by the <see cref="LoginMode"/> enum.
/// </remarks>
public class LoginPage : BasePage
{
    /// <summary>
    /// Gets the collection of locators for critical elements that must be visible
    /// for the Login Page to be considered properly loaded.
    /// These include the username input, password input, and login button.
    /// </summary>
    /// <inheritdoc cref="BasePage.CriticalElementsToEnsureVisible" />
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => LoginPageMap.LoginPageElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage"/> class.
    /// /// Call <see cref="AssertPageIsLoaded()"/> to perform validation after instantiation.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Passed to base.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Passed to base.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Passed to base.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Passed to base.</param>
    /// <exception cref="WebDriverTimeoutException">Thrown if the page title does not match the expected title within the timeout period during initialization.</exception>
    /// <exception cref="ArgumentNullException">Thrown by the base constructor if any of the required service parameters are null.</exception>
    public LoginPage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, IRetryService retryService)
        : base(driver, loggerFactory, settingsProvider, retryService)
    {
        PageLogger.LogDebug("{PageName} instance created. Call AssertPageIsLoaded() to verify.", PageName);
    }

    /// <summary>
    /// Asserts that the LoginPage is fully loaded by performing base checks and
    /// verifying the page title.
    /// </summary>
    /// <returns>The current LoginPage instance for fluent chaining.</returns>
    public override LoginPage AssertPageIsLoaded()
    {
        _ = base.AssertPageIsLoaded();

        PageLogger.LogDebug("Performing LoginPage-specific validation (Page Title).");
        try
        {
            Wait.WaitForPageTitle(Driver, PageLogger, PageName, LoginPageMap.PageTitle);
            PageLogger.LogInformation("Page title '{PageTitle}' verified for {PageName}.", LoginPageMap.PageTitle, PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(ex, "{PageName} did not load with the expected title '{PageTitle}'.", PageName, LoginPageMap.PageTitle);
            throw;
        }

        return this;
    }

    /// <summary>
    /// Enters the provided username into the username input field on the login page.
    /// </summary>
    /// <param name="username">The username string to enter. Should not be null or empty for a valid login attempt.</param>
    /// <returns>The current instance of the <see cref="LoginPage"/>, allowing for fluent method chaining.</returns>
    /// <remarks>
    /// This method utilizes an extension method <c>EnterUsername</c> (defined on <see cref="By"/> locators)
    /// to perform the actual find, clear, and send keys operations, including highlighting if enabled.
    /// </remarks>
    [AllureStep("Entering username: {username}")]
    public LoginPage EnterUsername(string username)
    {
        PageLogger.LogInformation("Entering username '{UsernameValue}' into username field on {PageName}.", username, PageName);
        LoginPageMap.UsernameInput.EnterUsername(username, Driver, Wait, PageLogger, FrameworkSettings);

        return this;
    }

    /// <summary>
    /// Enters the provided password into the password input field on the login page.
    /// </summary>
    /// <param name="password">The password string to enter. Should not be null or empty for a valid login attempt.</param>
    /// <returns>The current instance of the <see cref="LoginPage"/>, allowing for fluent method chaining.</returns>
    /// <remarks>
    /// This method utilizes an extension method <c>EnterPassword</c> (defined on <see cref="By"/> locators)
    /// to perform the actual find, clear, and send keys operations, including highlighting if enabled.
    /// </remarks>
    public LoginPage EnterPassword(string password)
    {
        PageLogger.LogInformation("Entering password into password field on {PageName}.", PageName);
        LoginPageMap.PasswordInput.EnterPassword(password, Driver, Wait, PageLogger, FrameworkSettings);

        return this;
    }

    /// <summary>
    /// Attempts to log in and expects a successful navigation to a new page, typically the inventory page.
    /// This method should only be used for positive test scenarios where login is expected to succeed.
    /// </summary>
    /// <param name="mode">The <see cref="LoginMode"/> to use for the login attempt. Defaults to <see cref="LoginMode.Submit"/>.</param>
    /// <returns>A new instance of the <see cref="InventoryPage"/> upon successful navigation.</returns>
    /// <remarks>
    /// This method implements a "fail-fast" strategy. After performing the login action (submit or click),
    /// it explicitly waits for a critical element on the destination page (<see cref="InventoryPageMap.InventoryContainer"/>)
    /// to become visible. If navigation does not occur within the configured timeout, this method will
    /// throw a <see cref="WebDriverTimeoutException"/>, immediately failing the test at the root cause of the failure.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if navigation to the InventoryPage does not complete within the wait timeout.</exception>
    public BasePage LoginAndExpectNavigation(LoginMode mode = LoginMode.Submit)
    {
        PageLogger.LogInformation("Attempting login on {PageName} using {LoginMode} mode, expecting successful navigation.", PageName, mode);

        if (mode == LoginMode.Submit)
        {
            PageLogger.LogDebug("Submitting login form via password field on {PageName}.", PageName);
            IWebElement passwordInput = FindElementOnPage(LoginPageMap.PasswordInput);
            _ = HighlightIfEnabled(passwordInput);
            passwordInput.Submit();
        }
        else
        {
            PageLogger.LogDebug("Clicking login button on {PageName}.", PageName);
            FindElementOnPage(LoginPageMap.LoginButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);
        }

        Wait.EnsureElementIsVisible(PageLogger, PageName, InventoryPageMap.InventoryContainer);
        PageLogger.LogInformation("Login successful. Confirmed navigation to InventoryPage.");

        return new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
    }

    /// <summary>
    /// Attempts a login action but does not wait for or verify navigation. This method is specifically
    /// designed for negative test scenarios where the login is expected to fail and remain on the LoginPage.
    /// </summary>
    /// <param name="mode">The <see cref="LoginMode"/> to use for the login attempt. Defaults to <see cref="LoginMode.Submit"/>.</param>
    /// <returns>The current <see cref="LoginPage"/> instance, allowing for subsequent assertions on the page (e.g., verifying an error message).</returns>
    /// <remarks>
    /// Use this method to test invalid credentials, locked-out users, or any other case where the
    /// expected outcome is to stay on the login page. It performs the login action and immediately
    /// returns the page object for the test to continue its verifications.
    /// </remarks>
    [AllureStep("Attempting login, expecting failure")]
    public LoginPage LoginAndExpectFailure(LoginMode mode = LoginMode.Submit)
    {
        PageLogger.LogInformation("Attempting login on {PageName} using {LoginMode} mode, expecting to remain on the page.", PageName, mode);

        if (mode == LoginMode.Submit)
        {
            IWebElement passwordInput = FindElementOnPage(LoginPageMap.PasswordInput);
            _ = HighlightIfEnabled(passwordInput);
            passwordInput.Submit();
        }
        else
        {
            FindElementOnPage(LoginPageMap.LoginButton)
                .ClickStandard(Driver, Wait, PageLogger, FrameworkSettings);
        }

        return this;
    }

    /// <summary>
    /// Retrieves the text of the error message displayed on the login page, if any.
    /// This method uses a retry policy to handle cases where the error message might appear with a delay.
    /// </summary>
    /// <returns>The text of the error message. Returns an empty string if no error message is found or if the text is empty after retries.</returns>
    /// <exception cref="Exception">Re-throws exceptions that occur if all retry attempts to get the error message fail (e.g., <see cref="WebDriverTimeoutException"/>).</exception>
    /// <remarks>
    /// It utilizes the <see cref="IRetryService.ExecuteWithRetry{TResult}(Func{TResult}, int, TimeSpan?, ILogger?, Func{TResult, bool}?)"/>
    /// method from the <see cref="Retry"/> service (available via <see cref="BasePage"/>).
    /// The retry policy is configured to retry if the error message element is not found or if its text is initially empty.
    /// Performance of this operation is measured.
    /// </remarks>
    [AllureStep("Getting error message from login page")]
    public string GetErrorMessage()
    {
        var timer = new PerformanceTimer($"GetErrorMessage_{PageName}", PageLogger); // Timer from your code
        bool success = false;
        string errorMessageText;

        try
        {
            PageLogger.LogInformation("Attempting to retrieve error message from {PageName}.", PageName);

            errorMessageText = Retry.ExecuteWithRetry(() =>
                {
                    IWebElement errorMessageElement = Wait.WaitForElement(PageLogger, PageName, LoginPageMap.ErrorMessageContainer);
                    string text = errorMessageElement.Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        PageLogger.LogWarning("Error message text is empty, will retry if attempts remain.");
                    }
                    return text;
                },
                maxRetryAttempts: 2,
                initialDelay: TimeSpan.FromMilliseconds(200),
                actionLogger: PageLogger,
                resultCondition: (text) => !string.IsNullOrWhiteSpace(text)
            );

            PageLogger.LogInformation("Retrieved error message from {PageName}: '{ErrorMessage}'", PageName, errorMessageText);
            success = true;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to get error message from {PageName} after retries.", PageName);

            throw;
        }
        finally
        {
            timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: success ? 1000 : null);
        }

        return errorMessageText;
    }
}
