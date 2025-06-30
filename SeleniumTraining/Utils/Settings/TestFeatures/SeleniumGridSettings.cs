namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines settings for connecting to a remote Selenium Grid as an immutable record.
/// </summary>
/// <remarks>
/// This record is bound to the "SeleniumGrid" section of the configuration (e.g., appsettings.json).
/// It allows the framework to conditionally create RemoteWebDriver instances that point to a Selenium Hub,
/// which is essential for running tests in a distributed or containerized environment like Docker.
/// Its properties are immutable after being loaded.
/// </remarks>
public record SeleniumGridSettings
{
    /// <summary>
    /// Gets a value indicating whether testing against the Selenium Grid is enabled.
    /// If false, the framework will create local WebDriver instances.
    /// If true, it will use the specified <see cref="Url"/> to create RemoteWebDriver instances.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets the URL of the Selenium Hub.
    /// This should be the full address including the port and path (e.g., "http://localhost:4444").
    /// In a Docker Compose environment, this will typically use the service name (e.g., "http://selenium-hub:4444").
    /// </summary>
    public string Url { get; init; } = "http://localhost:4444";
}
