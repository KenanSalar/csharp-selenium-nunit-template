namespace SeleniumTraining.Pages.SauceDemo;

public class LoginPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => LoginPageMap.LoginPageElements;

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

    [AllureStep("Entering username: {username}")]
    public LoginPage EnterUsername(string username)
    {
        PageLogger.LogInformation("Entering username '{UsernameValue}' into username field on {PageName}.", username, PageName);
        LoginPageMap.UsernameInput.EnterUsername(username, Driver, Wait, PageLogger, FrameworkSettings);

        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        PageLogger.LogInformation("Entering password into password field on {PageName}.", PageName);
        LoginPageMap.PasswordInput.EnterPassword(password, Driver, Wait, PageLogger, FrameworkSettings);

        return this;
    }

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
