namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Provides an abstract base record for browser-specific settings,
/// defining common configurable properties for WebDriver initialization as immutable values.
/// </summary>
/// <remarks>
/// Concrete browser settings records (e.g., for Chrome, Firefox) should inherit from this record.
/// As a record with `init`-only properties, instances of this type are immutable once created,
/// ensuring that browser configuration remains consistent after being loaded from sources like appsettings.json.
/// </remarks>
public abstract record BaseBrowserSettings
{
    /// <summary>
    /// Gets a value indicating whether the browser should be launched in headless mode
    /// (without a visible UI).
    /// </summary>
    public bool Headless { get; init; }

    /// <summary>
    /// Gets the default timeout in seconds for various WebDriver operations.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Range(0, 180, ErrorMessage = "TimeoutSeconds must be between 0 and 300.")]
    public int TimeoutSeconds { get; init; } = 10;

    /// <summary>
    /// Gets the desired width of the browser window in pixels.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowWidth, if specified, must be a positive integer.")]
    public int? WindowWidth { get; init; }

    /// <summary>
    /// Gets the desired height of the browser window in pixels.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "WindowHeight, if specified, must be a positive integer.")]
    public int? WindowHeight { get; init; }

    /// <summary>
    /// Gets a value indicating whether the browser instance should be left open
    /// after a test run completes.
    /// </summary>
    public bool LeaveBrowserOpenAfterTest { get; init; }

    /// <summary>
    /// Gets the URL for a remote Selenium Grid Hub.
    /// If this value is set, the framework will attempt to create a <see cref="OpenQA.Selenium.Remote.RemoteWebDriver"/>
    /// instance pointing to this URL instead of a local driver.
    /// </summary>
    public string? SeleniumGridUrl { get; init; }
}
