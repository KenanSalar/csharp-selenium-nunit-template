namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Provides a base class for browser-specific WebDriver factory services.
/// It includes common functionalities like browser version checking and standardized error logging for driver creation.
/// </summary>
/// <remarks>
/// Derived classes (e.g., <c>ChromeDriverFactoryService</c>, <c>FirefoxDriverFactoryService</c>)
/// should implement the <see cref="IBrowserDriverFactoryService"/> interface and can leverage
/// the protected methods provided by this base class to ensure consistent behavior.
/// This class inherits from <see cref="BaseService"/> to provide logging capabilities.
/// </remarks>
public abstract class DriverFactoryServiceBase : BaseService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriverFactoryServiceBase"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.
    /// This is typically passed to the base <see cref="BaseService"/> constructor.</param>
    protected DriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {

    }

    /// <summary>
    /// Performs a check to ensure the detected version of the browser associated with the provided WebDriver instance
    /// meets a specified minimum supported version.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance whose browser version is to be checked.
    /// Must implement <see cref="IHasCapabilities"/>.</param>
    /// <param name="browserNameForLog">A user-friendly name of the browser (e.g., "Chrome", "Firefox") for logging purposes.</param>
    /// <param name="minimumSupportedVersion">The minimum <see cref="Version"/> that is supported by the framework or application.</param>
    /// <exception cref="UnsupportedBrowserVersionException">
    /// Thrown if the detected browser version is older than the <paramref name="minimumSupportedVersion"/>.
    /// The driver is also quit before this exception is thrown.
    /// </exception>
    /// <remarks>
    /// This method attempts to parse the browser version from the driver's capabilities.
    /// If the version cannot be determined or parsed, a warning is logged, and the check is skipped.
    /// It handles common version string formats (e.g., "100.0.1234.56", "101.0.2345-beta").
    /// </remarks>
    protected void PerformVersionCheck(IWebDriver driver, string browserNameForLog, Version minimumSupportedVersion)
    {
        try
        {
            ICapabilities capabilities = ((IHasCapabilities)driver).Capabilities;

            string? browserVersionString = capabilities.GetCapability("browserVersion")?.ToString();

            if (string.IsNullOrWhiteSpace(browserVersionString))
            {
                ServiceLogger.LogWarning("Could not determine {BrowserName} version from capabilities. Skipping version check.", browserNameForLog);
                return;
            }

            ServiceLogger.LogInformation("Detected {BrowserName} version string: {BrowserVersionString}", browserNameForLog, browserVersionString);

            IEnumerable<string> versionNumericParts = browserVersionString.Split('.')
                .TakeWhile(part => part.All(char.IsDigit) || (part.Contains('-') && part[..part.IndexOf('-')].All(char.IsDigit)))
                .Select(part => part.Contains('-') ? part[..part.IndexOf('-')] : part)
                .Take(3);

            string normalizedVersionString = string.Join(".", versionNumericParts);

            if (Version.TryParse(normalizedVersionString, out Version? detectedVersion) && detectedVersion != null)
            {
                ServiceLogger.LogInformation("Parsed {BrowserName} version: {ParsedVersion}", browserNameForLog, detectedVersion);
                if (detectedVersion < minimumSupportedVersion)
                {
                    ServiceLogger.LogError("Unsupported {BrowserName} version. Detected: {DetectedVersion}, Minimum Required: {MinimumVersion}", browserNameForLog, detectedVersion, minimumSupportedVersion);
                    driver.QuitSafely(ServiceLogger, $"Unsupported version {detectedVersion} for {browserNameForLog}");
                    throw new UnsupportedBrowserVersionException(browserNameForLog, detectedVersion.ToString(), minimumSupportedVersion.ToString());
                }
                ServiceLogger.LogInformation("{BrowserName} version {ParsedVersion} meets minimum requirement of {MinVersion}.", browserNameForLog, detectedVersion, minimumSupportedVersion);
            }
            else
            {
                ServiceLogger.LogWarning("Could not parse {BrowserName} version from string '{RawVersionString}' (normalized to '{NormalizedString}'). Skipping version check.", browserNameForLog, browserVersionString, normalizedVersionString);
            }
        }
        catch (UnsupportedBrowserVersionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ServiceLogger.LogError(ex, "An error occurred during browser version check for {BrowserName}. Proceeding without version verification.", browserNameForLog);
        }
    }

    /// <summary>
    /// Logs detailed information about an error encountered during WebDriver creation and then throws the original exception.
    /// </summary>
    /// <param name="ex">The exception that occurred during WebDriver instantiation.</param>
    /// <param name="browserType">The <see cref="BrowserType"/> for which the WebDriver creation failed.</param>
    /// <param name="driverOptions">The <see cref="DriverOptions"/> that were used in the attempt to create the WebDriver.
    /// These options will be serialized to JSON for logging.</param>
    /// <param name="additionalContext">Optional. Any additional context or information relevant to the failure scenario.</param>
    /// <exception cref="Exception">Re-throws the original <paramref name="ex"/> after logging.</exception>
    /// <remarks>
    /// This method serializes the <paramref name="driverOptions"/> to JSON to provide a detailed snapshot
    /// of the configuration at the time of failure, aiding in debugging.
    /// </remarks>
    protected void LogAndThrowWebDriverCreationError(Exception ex, BrowserType browserType, DriverOptions driverOptions, string additionalContext = "")
    {
        string optionsJson = "Could not serialize options";
        try
        {
            optionsJson = JsonConvert.SerializeObject(driverOptions.ToCapabilities(), Formatting.Indented);
        }
        catch (Exception serializationEx)
        {
            ServiceLogger.LogWarning(
                serializationEx,
                "Failed to serialize driver options to JSON during error logging for {BrowserType}. Using default message. Context: {Context}",
                browserType,
                additionalContext
            );
        }

        string baseErrorMessage = $"Error instantiating {browserType} WebDriver. {additionalContext}".Trim();

        if (ex is WebDriverException wdEx)
            ServiceLogger.LogError(wdEx, "{BaseErrorMessage} WebDriverException. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
        else if (ex is InvalidOperationException ioEx)
            ServiceLogger.LogError(ioEx, "{BaseErrorMessage} InvalidOperationException. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
        else
            ServiceLogger.LogError(ex, "{BaseErrorMessage} Unexpected Exception. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
    }
}
