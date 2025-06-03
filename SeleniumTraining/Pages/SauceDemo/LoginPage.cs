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
    /// It calls the base constructor and then performs LoginPage-specific initialization checks,
    /// such as verifying the page title.
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

        PageLogger.LogDebug("Performing LoginPage-specific initialization checks for {PageName}.", PageName);

        try
        {
            Wait.WaitForPageTitle(Driver, PageLogger, PageName, LoginPageMap.PageTitle);
            PageLogger.LogInformation("Page title 'Swag Labs' verified for {PageName}.", PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(ex, "{PageName} did not load with the expected title 'Swag Labs'.", PageName);
            throw;
        }

        PageLogger.LogDebug("{PageName} instance fully created and validated.", PageName);
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
    /// Attempts to log in using the specified <see cref="LoginMode"/> (either by submitting the form
    /// or clicking the login button) and expects navigation to a new page, typically the inventory page.
    /// </summary>
    /// <param name="mode">The <see cref="LoginMode"/> to use for the login attempt. Defaults to <see cref="LoginMode.Submit"/>.</param>
    /// <returns>
    /// If login and navigation are successful, returns a new instance of the target page (e.g., <see cref="InventoryPage"/>).
    /// If login or navigation fails, returns the current <see cref="LoginPage"/> instance, allowing further interaction or error checking.
    /// </returns>
    /// <remarks>
    /// This method measures the performance of the login operation.
    /// After performing the login action (submit or click), it waits for a specific element on the expected
    /// next page (<see cref="InventoryPageMap.InventoryContainer"/>) to become visible to confirm navigation.
    /// If navigation is not confirmed, an error is logged.
    /// It utilizes the <see cref="Retry"/> service implicitly through the <c>Wait</c> operations.
    /// Element highlighting is applied to the password field (for submit) or login button (for click) if enabled.
    /// </remarks>
    public BasePage LoginAndExpectNavigation(LoginMode mode = LoginMode.Submit)
    {
        PageLogger.LogInformation("Attempting login on {PageName} using {LoginMode} mode.", PageName, mode);

        string operationName = $"LoginAndNavigate_{PageName}_{mode}";
        var additionalProps = new Dictionary<string, object>
        {
            { "LoginMode", mode.ToString() }
        };
        long expectedMaxLoginTimeMs = 5000;
        bool loginSuccessful = false;

        var timer = new PerformanceTimer(
            operationName,
            PageLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            additionalProps
        );

        BasePage nextPage;
        if (mode == LoginMode.Submit)
        {
            PageLogger.LogDebug("Submitting login form via password field on {PageName}.", PageName);
            _ = HighlightIfEnabled(LoginPageMap.PasswordInput);
            Wait.WaitForElement(PageLogger, PageName, LoginPageMap.PasswordInput).Submit();
        }
        else
        {
            PageLogger.LogDebug("Clicking login button on {PageName}.", PageName);
            _ = HighlightIfEnabled(LoginPageMap.LoginButton);
            Wait.WaitForElement(PageLogger, PageName, LoginPageMap.LoginButton).Click();
        }

        try
        {
            Wait.EnsureElementIsVisible(PageLogger, PageName, InventoryPageMap.InventoryContainer);
            PageLogger.LogInformation("Login successful on {PageName}. Confirmed navigation to InventoryPage.", PageName);

            loginSuccessful = true;
            nextPage = new InventoryPage(Driver, LoggerFactory, PageSettingsProvider, Retry);
        }
        catch (Exception ex)
        {
            PageLogger.LogError(
                ex,
                "Login action on {PageName} did not result in navigation to InventoryPage. User likely remained on {PageName}.",
                PageName,
                PageName
            );

            loginSuccessful = false;
            nextPage = this;
        }
        finally
        {
            timer.StopAndLog(
                attachToAllure: true,
                expectedMaxMilliseconds: loginSuccessful ? expectedMaxLoginTimeMs : null
            );
            timer.Dispose();
        }

        return nextPage;
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
