namespace SeleniumTraining.Pages.SauceDemo;

public class LoginPage : BasePage
{
    protected override IEnumerable<By> CriticalElementsToEnsureVisible => LoginPageMap.LoginPageElements;

    public LoginPage(IWebDriver driver, ILoggerFactory loggerFactory)
        : base(driver, loggerFactory)
    {
        Logger.LogDebug("Performing LoginPage-specific initialization checks for {PageName}.", PageName);

        try
        {
            Wait.WaitForPageTitle(Driver, Logger, PageName, LoginPageMap.PageTitle);
            Logger.LogInformation("Page title 'Swag Labs' verified for {PageName}.", PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            Logger.LogError(ex, "{PageName} did not load with the expected title 'Swag Labs'.", PageName);
            throw;
        }

        Logger.LogDebug("{PageName} instance fully created and validated.", PageName);
    }

    [AllureStep("Entering username: {username}")]
    public LoginPage EnterUsername(string username)
    {
        Logger.LogInformation("Entering username '{UsernameValue}' into username field on {PageName}.", username, PageName);
        LoginPageMap.UsernameInput.EnterUsername(username, Driver, Wait);

        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        Logger.LogInformation("Entering password into password field on {PageName}.", PageName);
        LoginPageMap.PasswordInput.EnterPassword(password, Driver, Wait);

        return this;
    }

    public BasePage LoginAndExpectNavigation(LoginMode mode = LoginMode.Submit)
    {
        Logger.LogInformation("Attempting login on {PageName} using {LoginMode} mode.", PageName, mode);

        if (mode == LoginMode.Submit)
        {
            Logger.LogDebug("Submitting login form via password field on {PageName}.", PageName);
            Wait.WaitForElement(Logger, PageName, LoginPageMap.PasswordInput).Submit();
        }
        else
        {
            Logger.LogDebug("Clicking login button on {PageName}.", PageName);
            Wait.WaitForElement(Logger, PageName, LoginPageMap.LoginButton).Click();
        }

        try
        {
            Wait.EnsureElementIsVisible(Logger, PageName, InventoryPageMap.InventoryContainer);
            Logger.LogInformation("Login successful on {PageName}. Confirmed navigation to InventoryPage.", PageName);

            return new InventoryPage(Driver, LoggerFactory);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Login action on {PageName} did not result in navigation to InventoryPage. User likely remained on {PageName}.",
                PageName,
                PageName
            );

            return this;
        }
    }

    [AllureStep("Getting error message from login page")]
    public string GetErrorMessage()
    {
        Logger.LogInformation("Attempting to retrieve error message from {PageName}.", PageName);
        try
        {
            IWebElement errorMessageElement = Wait.WaitForElement(Logger, PageName, LoginPageMap.ErrorMessageContainer);
            string errorMessage = errorMessageElement.Text;
            Logger.LogInformation("Retrieved error message from {PageName}: '{ErrorMessage}'", PageName, errorMessage);
            return errorMessage;
        }
        catch (WebDriverTimeoutException ex)
        {
            Logger.LogError(ex, "Error message element not found or not visible on {PageName} within the timeout period.", PageName);
            throw new NoSuchElementException($"Error message element defined by {LoginPageMap.ErrorMessageContainer} was not found on {PageName}.", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred while trying to retrieve the error message from {PageName}.", PageName);
            throw;
        }
    }
}
