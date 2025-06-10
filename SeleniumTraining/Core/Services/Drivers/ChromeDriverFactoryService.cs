using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Factory service specifically for creating and configuring <see cref="ChromeDriver"/> instances.
/// </summary>
/// <remarks>
/// This service handles the Chrome-specific setup, including applying user preferences
/// and locating the Chrome executable before creating the driver instance.
/// </remarks>
public class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase
{
    /// <inheritdoc/>
    public override BrowserType Type => BrowserType.Chrome;

    /// <inheritdoc/>
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriverFactoryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.</param>
    public ChromeDriverFactoryService(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
    }

    /// <inheritdoc/>
    public override IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not ChromeSettings settings)
        {
            throw new ArgumentException($"Invalid settings type. Expected {nameof(ChromeSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
        }

        ChromeOptions chromeOptions = ConfigureCommonChromiumOptions<ChromeOptions>(settings, options, out _);

        if (settings.UserProfilePreferences != null && settings.UserProfilePreferences.Count != 0)
        {
            ServiceLogger.LogDebug(
                "Applying {PrefCount} user profile preferences via 'prefs' experimental option.",
                settings.UserProfilePreferences.Count
            );

            foreach (KeyValuePair<string, object> pref in settings.UserProfilePreferences)
            {
                try
                {
                    string key = pref.Key;
                    string? stringValue = pref.Value?.ToString();

                    if (stringValue is null)
                    {
                        ServiceLogger.LogWarning("Skipping user profile preference '{PrefKey}' because its value is null.", key);
                        continue;
                    }

                    object finalValue;

                    if (bool.TryParse(stringValue, out bool boolResult))
                    {
                        finalValue = boolResult;
                    }
                    else if (int.TryParse(stringValue, out int intResult))
                    {
                        finalValue = intResult;
                    }
                    else
                    {
                        finalValue = stringValue;
                    }

                    chromeOptions.AddUserProfilePreference(key, finalValue);
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError(ex, "Failed to apply user profile preference '{PrefKey}' with value '{PrefValue}'.", pref.Key, pref.Value);
                }
            }
        }

        string chromeExecutablePath = GetChromeExecutablePathInternal();
        if (!string.IsNullOrEmpty(chromeExecutablePath))
        {
            chromeOptions.BinaryLocation = chromeExecutablePath;
        }

        if (string.IsNullOrEmpty(settings.SeleniumGridUrl))
        {
            ServiceLogger.LogInformation("Creating local ChromeDriver. Selenium Manager will ensure the driver is available.");
            return CreateDriverInstanceWithChecks(chromeOptions, opts => new ChromeDriver(opts));
        }
        else
        {
            ServiceLogger.LogInformation("Creating RemoteWebDriver for Chrome Grid at {GridUrl}", settings.SeleniumGridUrl);
            return new RemoteWebDriver(new Uri(settings.SeleniumGridUrl), chromeOptions);
        }
    }

    /// <summary>
    /// Attempts to locate the Google Chrome executable on the current system.
    /// It checks environment variables and standard installation paths for different operating systems.
    /// </summary>
    /// <returns>The full path to the Chrome executable if found; otherwise, an empty string.</returns>
    /// <remarks>
    /// The search order is:
    /// <list type="number">
    ///   <item><description>Environment variable <c>CHROME_EXECUTABLE_PATH</c>.</description></item>
    ///   <item><description>Standard installation paths for Windows (Program Files, Program Files (x86)).</description></item>
    ///   <item><description>Standard installation paths for Linux.</description></item>
    ///   <item><description>Standard installation path for macOS.</description></item>
    /// </list>
    /// Logs information about the search process and the outcome.
    /// </remarks>
    private string GetChromeExecutablePathInternal()
    {
        ServiceLogger.LogDebug("Searching for Chrome executable...");
        string? chromePathEnv = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(chromePathEnv) && File.Exists(chromePathEnv))
        {
            ServiceLogger.LogInformation("Using Chrome executable from CHROME_EXECUTABLE_PATH: {ChromePathEnv}", chromePathEnv);
            return chromePathEnv;
        }

        string[] pathsToTry;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            pathsToTry = [
                Path.Combine(progFiles, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(progFilesX86, "Google", "Chrome", "Application", "chrome.exe")
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pathsToTry = ["/usr/bin/google-chrome-stable", "/usr/bin/google-chrome", "/opt/google/chrome/chrome"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            pathsToTry = ["/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"];
        }
        else
        {
            ServiceLogger.LogWarning("Unsupported OS platform for Chrome path detection: {OSPlatform}", RuntimeInformation.OSDescription);
            return string.Empty;
        }

        foreach (string path in pathsToTry)
        {
            if (File.Exists(path))
            {
                ServiceLogger.LogInformation("Found Chrome executable at: {ChromePath}", path);
                return path;
            }
            ServiceLogger.LogDebug("Chrome executable not found at: {PathToTry}", path);
        }

        ServiceLogger.LogWarning("Chrome executable not found in standard installation paths for OS {OSPlatform}.", RuntimeInformation.OSDescription);
        return string.Empty;
    }
}
