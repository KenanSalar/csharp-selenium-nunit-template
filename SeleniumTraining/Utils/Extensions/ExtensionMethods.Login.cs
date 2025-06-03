using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides extension methods related to login actions, typically for interacting
/// with username and password input fields identified by <see cref="By"/> locators.
/// </summary>
/// <remarks>
/// This partial class centralizes common sequences of actions required to enter text
/// into login form fields, such as waiting for the element to be interactable,
/// optionally highlighting it, clearing existing content, and then sending the keys.
/// These extensions are designed to be used within Page Object methods to enhance
/// readability and maintainability.
/// This is part of a larger <c>ExtensionMethods</c> static class.
/// </remarks>
public static partial class ExtensionMethods
{
    /// <summary>
    /// Extends <see cref="By"/> locators to provide a standardized way to enter a username
    /// into an input field. It waits for the element, optionally highlights it,
    /// clears existing content, and then types the username.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator strategy for the username input field.</param>
    /// <param name="userName">The username string to enter.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance.</param>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance for explicit waits.</param>
    /// <param name="logger">The <see cref="ILogger"/> for logging actions and warnings.</param>
    /// <param name="settings">The <see cref="TestFrameworkSettings"/> containing configuration
    /// like highlight preferences and duration.</param>
    /// <remarks>
    /// This method first uses <c>wait.WaitForElement()</c> (assumed to be another extension method)
    /// to ensure the element is present and interactable.
    /// If <see cref="TestFrameworkSettings.HighlightElementsOnInteraction"/> is true, the element is highlighted
    /// using an assumed <c>HighlightElement()</c> extension method.
    /// It then verifies the element is displayed before clearing and sending keys.
    /// Logs information about the action or a warning if the element is not found/displayed.
    /// The Allure step includes the username for traceability in reports.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown by <c>wait.WaitForElement()</c> or the internal <c>wait.Until()</c> if the element is not found or interactable within the timeout.</exception>
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

    /// <summary>
    /// Extends <see cref="By"/> locators to provide a standardized way to enter a password
    /// into an input field. It waits for the element, optionally highlights it,
    /// clears existing content, and then types the password.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator strategy for the password input field.</param>
    /// <param name="password">The password string to enter. For security, this is not included in the Allure step name.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance.</param>
    /// <param name="wait">The <see cref="WebDriverWait"/> instance for explicit waits.</param>
    /// <param name="logger">The <see cref="ILogger"/> for logging actions and warnings.</param>
    /// <param name="settings">The <see cref="TestFrameworkSettings"/> containing configuration
    /// like highlight preferences and duration.</param>
    /// <remarks>
    /// This method operates similarly to <see cref="EnterUsername(By, string, IWebDriver, WebDriverWait, ILogger, TestFrameworkSettings)"/>.
    /// It ensures the element is present and interactable, highlights if configured,
    /// verifies display, then clears and sends the password.
    /// Logs information or warnings based on the outcome.
    /// The Allure step name does not include the password value for security.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown by <c>wait.WaitForElement()</c> or the internal <c>wait.Until()</c> if the element is not found or interactable within the timeout.</exception>
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
