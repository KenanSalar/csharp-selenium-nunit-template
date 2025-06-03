namespace SeleniumTraining.Core.Services.Drivers.Contracts;

/// <summary>
/// Defines the contract for a factory service responsible for creating <see cref="IWebDriver"/> instances
/// for a specific browser type (e.g., Chrome, Firefox).
/// </summary>
/// <remarks>
/// Each implementation of this interface will be specialized for a single browser.
/// For example, a <c>ChromeDriverFactoryService</c> would implement this interface to produce
/// Chrome WebDriver instances. This pattern allows for browser-specific setup and configuration logic
/// to be encapsulated within the respective factory.
/// </remarks>
public interface IBrowserDriverFactoryService
{
    // <summary>
    /// Gets the specific <see cref="BrowserType"/> that this factory is responsible for creating drivers for.
    /// </summary>
    /// <value>The <see cref="BrowserType"/> supported by this factory implementation.</value>
    public BrowserType Type { get; }

    /// <summary>
    /// Creates and returns a new <see cref="IWebDriver"/> instance for the browser type supported by this factory,
    /// configured with the provided settings and options.
    /// </summary>
    /// <param name="settingsBase">The browser-specific settings (subclass of <see cref="BaseBrowserSettings"/>)
    /// containing configurations like headless mode, timeouts, and browser arguments. Must not be null.</param>
    /// <param name="options">Optional, pre-configured <see cref="DriverOptions"/> specific to the browser type.
    /// If null, the factory will generate new options based on the <paramref name="settingsBase"/>.</param>
    /// <returns>A new, configured <see cref="IWebDriver"/> instance for the factory's specific browser type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settingsBase"/> is null.</exception>
    /// <exception cref="WebDriverException">Thrown if WebDriver instantiation fails due to issues like
    /// driver executable mismatch, browser not installed, or invalid options.</exception>
    /// <remarks>
    /// Implementations should handle the creation of appropriate <see cref="DriverOptions"/> if not provided,
    /// apply settings from <paramref name="settingsBase"/>, and instantiate the correct WebDriver
    /// (e.g., <c>ChromeDriver</c>, <c>FirefoxDriver</c>).
    /// </remarks>
    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null);
}
