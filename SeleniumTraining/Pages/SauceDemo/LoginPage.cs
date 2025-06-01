namespace SeleniumTraining.Pages.SauceDemo;

public class LoginPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => LoginPageMap.LoginPageElements;

    public LoginPage(IWebDriver driver, ILoggerFactory loggerFactory)
        : base(driver, loggerFactory)
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
        LoginPageMap.UsernameInput.EnterUsername(username, Driver, Wait);

        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        PageLogger.LogInformation("Entering password into password field on {PageName}.", PageName);
        LoginPageMap.PasswordInput.EnterPassword(password, Driver, Wait);

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
            Wait.WaitForElement(PageLogger, PageName, LoginPageMap.PasswordInput).Submit();
        }
        else
        {
            PageLogger.LogDebug("Clicking login button on {PageName}.", PageName);
            Wait.WaitForElement(PageLogger, PageName, LoginPageMap.LoginButton).Click();
        }

        try
        {
            Wait.EnsureElementIsVisible(PageLogger, PageName, InventoryPageMap.InventoryContainer);
            PageLogger.LogInformation("Login successful on {PageName}. Confirmed navigation to InventoryPage.", PageName);

            loginSuccessful = true;
            nextPage = new InventoryPage(Driver, LoggerFactory);
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
        string errorMessageText;
        var timer = new PerformanceTimer($"GetErrorMessage_{PageName}", PageLogger);
        bool success;

        try
        {
            PageLogger.LogInformation("Attempting to retrieve error message from {PageName}.", PageName);
            IWebElement errorMessageElement = Wait.WaitForElement(PageLogger, PageName, LoginPageMap.ErrorMessageContainer);

            errorMessageText = errorMessageElement.Text;
            PageLogger.LogInformation("Retrieved error message from {PageName}: '{ErrorMessage}'", PageName, errorMessageText);

            success = true;
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(ex, "Error message element not found or not visible on {PageName} within the timeout period.", PageName);

            timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
            timer.Dispose();

            throw new NoSuchElementException($"Error message element defined by {LoginPageMap.ErrorMessageContainer} was not found on {PageName}.", ex);
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while trying to retrieve the error message from {PageName}.", PageName);

            timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: null);
            timer.Dispose();

            throw;
        }

        timer.StopAndLog(attachToAllure: true, expectedMaxMilliseconds: success ? 1000 : null);
        timer.Dispose();
        return errorMessageText;
    }
}
