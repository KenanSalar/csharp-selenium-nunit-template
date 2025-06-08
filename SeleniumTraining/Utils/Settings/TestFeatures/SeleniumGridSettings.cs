namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines settings for connecting to a remote Selenium Grid.
/// </summary>
/// <remarks>
/// This class is bound to the "SeleniumGrid" section of the configuration (e.g., appsettings.json).
/// It allows the framework to conditionally create RemoteWebDriver instances that point to a Selenium Hub,
/// which is essential for running tests in a distributed or containerized environment like Docker.
/// </remarks>
public class SeleniumGridSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether testing against the Selenium Grid is enabled.
    /// If false, the framework will create local WebDriver instances.
    /// If true, it will use the specified <see cref="Url"/> to create RemoteWebDriver instances.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the URL of the Selenium Hub.
    /// This should be the full address including the port and path (e.g., "http://localhost:4444").
    /// In a Docker Compose environment, this will typically use the service name (e.g., "http://selenium-hub:4444").
    /// </summary>
    public string Url { get; set; } = "http://localhost:4444";
}
