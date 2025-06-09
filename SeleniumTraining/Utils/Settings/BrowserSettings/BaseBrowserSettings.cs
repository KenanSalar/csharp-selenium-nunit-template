namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Provides an abstract base class for browser-specific settings,
/// defining common configurable properties for WebDriver initialization.
/// </summary>
/// <remarks>
/// Concrete browser settings classes (e.g., for Chrome, Firefox) should inherit from this class
/// to gain common properties like <see cref="Headless"/> mode, <see cref="TimeoutSeconds"/>,
/// <see cref="WindowWidth"/>, <see cref="WindowHeight"/>, and <see cref="LeaveBrowserOpenAfterTest"/>.
/// These settings are typically bound from configuration files (e.g., appsettings.json) and
/// influence how WebDriver instances are created and behave.
/// Data annotations like <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> are used for validating some settings.
/// In CI/CD environments, certain settings like <see cref="Headless"/> mode might be
/// overridden or defaulted to specific values for automated runs.
/// </remarks>
public abstract class BaseBrowserSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the browser should be launched in headless mode
    /// (without a visible UI).
    /// </summary>
    /// <value>
    /// <c>true</c> to run in headless mode; otherwise, <c>false</c>.
    /// The default value depends on the configuration source.
    /// </value>
    /// <remarks>
    /// Headless mode is often preferred for automated tests running in CI/CD environments
    /// as it can be faster and does not require a display server.
    /// </remarks>
    public bool Headless { get; set; }

    /// <summary>
    /// Gets or sets the default timeout in seconds for various WebDriver operations,
    /// such as implicit waits or command timeouts, depending on how it's used by the WebDriver factory.
    /// </summary>
    /// <value>
    /// The timeout duration in seconds. Must be between 0 and 180, inclusive (as per the <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/>).
    /// Defaults to 10 seconds if not otherwise specified in configuration.
    /// </value>
    /// <remarks>
    /// The <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> ensures this value is within a practical range.
    /// This timeout influences how long Selenium will wait for certain conditions before throwing an exception.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Range(0, 180, ErrorMessage = "TimeoutSeconds must be between 0 and 300.")]
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the desired width of the browser window in pixels.
    /// If null, the browser's default width or a width determined by other means (e.g., maximized) may be used.
    /// </summary>
    /// <value>
    /// The desired window width in pixels, or <c>null</c> for default behavior.
    /// If specified, must be a positive integer (as per the <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/>).
    /// </value>
    /// <remarks>
    /// Setting specific window dimensions can be important for consistent test execution,
    /// especially for applications with responsive designs.
    /// The <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> ensures this value, if set, is positive.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowWidth, if specified, must be a positive integer.")]
    public int? WindowWidth { get; set; }

    /// <summary>
    /// Gets or sets the desired height of the browser window in pixels.
    /// If null, the browser's default height or a height determined by other means (e.g., maximized) may be used.
    /// </summary>
    /// <value>
    /// The desired window height in pixels, or <c>null</c> for default behavior.
    /// If specified, must be a positive integer (as per the <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/>).
    /// </value>
    /// <remarks>
    /// Paired with <see cref="WindowWidth"/>, this allows for precise control over the viewport size.
    /// The <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> ensures this value, if set, is positive.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowHeight, if specified, must be a positive integer.")]
    public int? WindowHeight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the browser instance should be left open
    /// after a test run completes.
    /// </summary>
    /// <value>
    /// <c>true</c> to leave the browser open after the test; otherwise, <c>false</c> to close it.
    /// The default value depends on the configuration source.
    /// </value>
    /// <remarks>
    /// This setting is primarily intended for debugging purposes, allowing developers to inspect
    /// the final state of the application or diagnose issues. It should typically be set to <c>false</c>
    /// for automated runs, especially in CI/CD environments, to ensure proper resource cleanup.
    /// Note that not all WebDriver implementations might directly support this setting through options
    /// (e.g., FirefoxDriver has no direct "LeaveBrowserRunning" option like ChromeDriver).
    /// </remarks>
    public bool LeaveBrowserOpenAfterTest { get; set; }

    /// <summary>
    /// Gets or sets the URL for a remote Selenium Grid Hub.
    /// If this value is set, the framework will attempt to create a <see cref="OpenQA.Selenium.Remote.RemoteWebDriver"/>
    /// instance pointing to this URL instead of a local driver.
    /// </summary>
    /// <value>The URL of the Selenium Grid Hub, or <c>null</c> to use a local driver.</value>
    public string? SeleniumGridUrl { get; set; }
}
