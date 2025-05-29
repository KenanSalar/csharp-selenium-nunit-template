using OpenQA.Selenium;

namespace SeleniumTraining.Utils.Extensions;

public static partial class ExtensionMethods
{
    [AllureStep("Entering username: {text}")]
    public static void EnterUsername(this By locator, string userName, IWebDriver driver, WebDriverWait wait)
    {
        _ = wait.Until(driver => driver.FindElement(locator).Displayed);
        driver.FindElement(locator).Clear();
        driver.FindElement(locator).SendKeys(userName);
    }

    public static void EnterPassword(this By locator, string password, IWebDriver driver, WebDriverWait wait)
    {
        _ = wait.Until(driver => driver.FindElement(locator).Displayed);
        driver.FindElement(locator).Clear();
        driver.FindElement(locator).SendKeys(password);
    }
}
