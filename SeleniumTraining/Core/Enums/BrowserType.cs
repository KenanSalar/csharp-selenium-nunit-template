namespace SeleniumTraining.Core.Enums;

/// <summary>
/// Defines the types of web browsers supported by the Selenium test framework.
/// </summary>
/// <remarks>
/// This enumeration is used to specify which browser a test should run against,
/// and to select appropriate WebDriver configurations and factories.
/// Currently supports Chrome, Edge and Firefox.
/// </remarks>
public enum BrowserType
{
    /// <summary>
    /// Represents the Google Chrome browser.
    /// This is typically associated with ChromeDriver.
    /// </summary>
    Chrome = 0,

    /// <summary>
    /// Represents the Microsoft Edge browser.
    /// This is typically associated with EdgeDriver.
    /// </summary>
    Edge = 1,

    /// <summary>
    /// Represents the Mozilla Firefox browser.
    /// This is typically associated with GeckoDriver.
    /// </summary>
    Firefox = 2
}
