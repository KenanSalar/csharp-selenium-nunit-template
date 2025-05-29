using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeleniumTraining.Utils.Extensions;

public static class WebDriverExtensions
{
    public static void QuitSafely(this IWebDriver driver, ILogger logger, string contextMessage)
    {
        try
        {
            driver?.Quit();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception during WebDriver Quit ({Context}). Driver might not have been fully initialized or already closed.", contextMessage);
        }
    }
}
