namespace SeleniumTraining.Core.Enums;

/// <summary>
/// Defines the types of web browsers supported by the Selenium test framework.
/// </summary>
/// <remarks>
/// This enumeration is used to specify which browser a test should run against,
/// and to select appropriate WebDriver configurations and factories.
/// Currently supports Chrome and Firefox. Brave is commented out as a potential future addition.
/// </remarks>
public enum BrowserType
{
    // Brave,

    /// <summary>
    /// Represents the Google Chrome browser.
    /// This is typically associated with ChromeDriver.
    /// </summary>
    Chrome = 0,

    Edge = 1,

    /// <summary>
    /// Represents the Mozilla Firefox browser.
    /// This is typically associated with GeckoDriver.
    /// </summary>
    Firefox = 2
}
