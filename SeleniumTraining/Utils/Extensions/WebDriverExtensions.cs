namespace SeleniumTraining.Utils.Extensions;

public static class WebDriverExtensions
{
    public static void QuitSafely(this IWebDriver driver, ILogger logger, string contextMessage)
    {
        if (driver == null)
            return;

        try
        {
            driver.Quit();
            logger.LogDebug("WebDriver QuitSafely successful for context: {Context}", contextMessage);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception during WebDriver QuitSafely ({Context}). Driver might not have been fully initialized or already closed.", contextMessage);
        }
    }
}
