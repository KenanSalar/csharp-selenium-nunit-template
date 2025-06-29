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
    public static void ClickStandard(
        this IWebElement element,
        IWebDriver driver,
        WebDriverWait wait,
        ILogger logger,
        TestFrameworkSettings settings
    )
    {
        string elementDesc = WebElementHighlightingExtensions.GetElementDescription(element);
        try
        {
            logger.LogDebug("Attempting standard click on element: {ElementDescription}", elementDesc);

            _ = wait.Until(ExpectedConditions.ElementToBeClickable(element));

            if (settings.HighlightElementsOnInteraction)
            {
                _ = element.HighlightElement(driver, logger, settings.HighlightDurationMs);
            }

            element.Click();

            logger.LogInformation("Successfully performed standard click on element: {ElementDescription}", elementDesc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Standard click failed for element: {ElementDescription}", elementDesc);
            throw;
        }
    }
}
