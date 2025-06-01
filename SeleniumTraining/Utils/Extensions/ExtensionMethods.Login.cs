using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    [AllureStep("Entering username: {userName}")]
    public static void EnterUsername(
        this By locator,
        string userName,
        IWebDriver driver,
        WebDriverWait wait,
        ILogger logger,
        TestFrameworkSettings settings
    )
    {
        IWebElement element = wait.WaitForElement(logger, "LoginPage", locator);

        if (settings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(driver, logger, settings.HighlightDurationMs);

        bool foundElement = wait.Until(driver => driver.FindElement(locator).Displayed);

        if (foundElement)
        {
            element.Clear();
            element.SendKeys(userName);
            logger.LogInformation("Entered username '{UsernameValue}' into element: {Locator}", userName, locator);
        }
        else
        {
            logger.LogWarning("Element with locator '{Locator}' not found or not displayed.", locator);
        }
    }

    public static void EnterPassword(
        this By locator,
        string password,
        IWebDriver driver,
        WebDriverWait wait,
        ILogger logger,
        TestFrameworkSettings settings
    )
    {
        IWebElement element = wait.WaitForElement(logger, "LoginPage", locator);

        if (settings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(driver, logger, settings.HighlightDurationMs);

        bool foundElement = wait.Until(driver => driver.FindElement(locator).Displayed);

        if (foundElement)
        {
            element.Clear();
            element.SendKeys(password);
            logger.LogInformation("Entered password into element: {Locator}", locator);
        }
        else
        {
            logger.LogWarning("Element with locator '{Locator}' not found or not displayed.", locator);
        }
    }
}
