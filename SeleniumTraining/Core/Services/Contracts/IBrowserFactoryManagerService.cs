namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service responsible for creating and managing WebDriver instances
/// based on browser-specific factories.
/// </summary>
/// <remarks>
/// This service acts as a higher-level manager that utilizes specific browser factories
/// (e.g., ChromeFactory, FirefoxFactory) to instantiate WebDriver instances.
/// It centralizes the logic for selecting the appropriate factory and passing
/// necessary configurations.
/// </remarks>
public interface IBrowserFactoryManagerService
{
    /// <summary>
    /// Creates and returns a WebDriver instance for the specified browser type using the provided settings and options.
    /// </summary>
    /// <param name="browserType">The type of browser for which to create the WebDriver instance (e.g., Chrome, Firefox).</param>
    /// <param name="settings">The browser-specific settings (e.g., headless mode, timeouts) to apply. Must not be null.</param>
    /// <param name="options">Optional, pre-configured DriverOptions to use. If null, new options will be generated based on settings.</param>
    /// <returns>An <see cref="IWebDriver"/> instance configured for the specified browser and settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified <paramref name="browserType"/> is not supported by any registered factory.</exception>
    /// <exception cref="WebDriverException">Thrown if WebDriver instantiation fails for other reasons (e.g., driver executable not found, browser not installed).</exception>
    public IWebDriver UseBrowserDriver(BrowserType browserType, BaseBrowserSettings settings, DriverOptions? options = null);
}
