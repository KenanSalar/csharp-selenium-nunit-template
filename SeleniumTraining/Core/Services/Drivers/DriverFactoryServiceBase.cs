namespace SeleniumTraining.Core.Services.Drivers;

public abstract class DriverFactoryServiceBase : BaseService
{
    protected DriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {

    }

    protected void PerformVersionCheck(IWebDriver driver, string browserNameForLog, Version minimumSupportedVersion)
    {
        try
        {
            ICapabilities capabilities = ((IHasCapabilities)driver).Capabilities;

            string? browserVersionString = capabilities.GetCapability("browserVersion")?.ToString();

            if (string.IsNullOrWhiteSpace(browserVersionString))
            {
                Logger.LogWarning("Could not determine {BrowserName} version from capabilities. Skipping version check.", browserNameForLog);
                return;
            }

            Logger.LogInformation("Detected {BrowserName} version string: {BrowserVersionString}", browserNameForLog, browserVersionString);

            IEnumerable<string> versionNumericParts = browserVersionString.Split('.')
                .TakeWhile(part => part.All(char.IsDigit) || (part.Contains('-') && part[..part.IndexOf('-')].All(char.IsDigit)))
                .Select(part => part.Contains('-') ? part[..part.IndexOf('-')] : part)
                .Take(3);

            string normalizedVersionString = string.Join(".", versionNumericParts);

            if (Version.TryParse(normalizedVersionString, out Version? detectedVersion) && detectedVersion != null)
            {
                Logger.LogInformation("Parsed {BrowserName} version: {ParsedVersion}", browserNameForLog, detectedVersion);
                if (detectedVersion < minimumSupportedVersion)
                {
                    Logger.LogError("Unsupported {BrowserName} version. Detected: {DetectedVersion}, Minimum Required: {MinimumVersion}", browserNameForLog, detectedVersion, minimumSupportedVersion);
                    driver.QuitSafely(Logger, $"Unsupported version {detectedVersion} for {browserNameForLog}");
                    throw new UnsupportedBrowserVersionException(browserNameForLog, detectedVersion.ToString(), minimumSupportedVersion.ToString());
                }
                Logger.LogInformation("{BrowserName} version {ParsedVersion} meets minimum requirement of {MinVersion}.", browserNameForLog, detectedVersion, minimumSupportedVersion);
            }
            else
            {
                Logger.LogWarning("Could not parse {BrowserName} version from string '{RawVersionString}' (normalized to '{NormalizedString}'). Skipping version check.", browserNameForLog, browserVersionString, normalizedVersionString);
            }
        }
        catch (UnsupportedBrowserVersionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred during browser version check for {BrowserName}. Proceeding without version verification.", browserNameForLog);
        }
    }

    protected void LogAndThrowWebDriverCreationError(Exception ex, BrowserType browserType, DriverOptions driverOptions, string additionalContext = "")
    {
        string optionsJson = "Could not serialize options";
        try
        {
            optionsJson = JsonConvert.SerializeObject(driverOptions.ToCapabilities(), Formatting.Indented);
        }
        catch (Exception serializationEx)
        {
            Logger.LogWarning(
                serializationEx,
                "Failed to serialize driver options to JSON during error logging for {BrowserType}. Using default message. Context: {Context}",
                browserType,
                additionalContext
            );
        }

        string baseErrorMessage = $"Error instantiating {browserType} WebDriver. {additionalContext}".Trim();

        if (ex is WebDriverException wdEx)
            Logger.LogError(wdEx, "{BaseErrorMessage} WebDriverException. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
        else if (ex is InvalidOperationException ioEx)
            Logger.LogError(ioEx, "{BaseErrorMessage} InvalidOperationException. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
        else
            Logger.LogError(ex, "{BaseErrorMessage} Unexpected Exception. Options: {ConfiguredOptions}", baseErrorMessage, optionsJson);
    }
}

public static class WebDriverQuitHelper
{
    public static void QuitSafely(this IWebDriver? driver, ILogger logger, string contextMessage)
    {
        if (driver == null) return;
        try
        {
            driver.Quit();
            logger.LogDebug("WebDriver QuitSafely successful for context: {Context}", contextMessage);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception during WebDriver QuitSafely ({Context}). Driver might not have been fully initialized or already closed.", contextMessage);
        }
    }
}
