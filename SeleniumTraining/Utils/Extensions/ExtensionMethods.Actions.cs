using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    /// <summary>
    /// Performs a standard, reliable click by first waiting for the element to be clickable.
    /// This should be the default click method for most interactions.
    /// </summary>
    /// <param name="element">The IWebElement to click.</param>
    /// <param name="wait">The WebDriverWait instance.</param>
    /// <param name="logger">The logger for logging actions and errors.</param>
    [AllureStep("Performing standard click on element: {element}")]
    public static void ClickStandard(this IWebElement element, WebDriverWait wait, ILogger logger)
    {
        string elementDesc = WebElementHighlightingExtensions.GetElementDescription(element);
        try
        {
            logger.LogDebug("Attempting standard click on element: {ElementDescription}", elementDesc);

            _ = wait.Until(ExpectedConditions.ElementToBeClickable(element));
            element.Click();

            logger.LogInformation("Successfully performed standard click on element: {ElementDescription}", elementDesc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Standard click failed for element: {ElementDescription}", elementDesc);
            throw;
        }
    }

    /// <summary>
    /// Performs a robust click on an element by first waiting for it to be clickable,
    /// and then using a JavaScript click, which is more reliable in CI/headless environments.
    /// </summary>
    /// <param name="element">The IWebElement to click.</param>
    /// <param name="driver">The IWebDriver instance.</param>
    /// <param name="wait">The WebDriverWait instance.</param>
    /// <param name="logger">The logger for logging actions and errors.</param>
    [AllureStep("Performing robust click on element: {element}")]
    public static void ClickRobustly(this IWebElement element, IWebDriver driver, WebDriverWait wait, ILogger logger)
    {
        string elementDesc = WebElementHighlightingExtensions.GetElementDescription(element);
        try
        {
            logger.LogDebug("Attempting robust click on element: {ElementDescription}", elementDesc);

            _ = wait.Until(ExpectedConditions.ElementToBeClickable(element));

            _ = ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);

            logger.LogInformation("Successfully performed robust click on element: {ElementDescription}", elementDesc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Robust click failed for element: {ElementDescription}", elementDesc);

            throw;
        }
    }
}
