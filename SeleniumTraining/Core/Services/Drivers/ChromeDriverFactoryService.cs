using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Factory service specifically for creating and configuring <see cref="ChromeDriver"/> instances.
/// </summary>
/// <remarks>
/// This service handles the Chrome-specific setup, including locating the Chrome executable,
/// configuring <see cref="ChromeOptions"/> with common and Chrome-specific settings,
/// and instantiating the <see cref="ChromeDriver"/>. It implements <see cref="IBrowserDriverFactoryService"/>
/// and inherits common Chromium configurations from <see cref="ChromiumDriverFactoryServiceBase"/>.
/// </remarks>
public class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Chrome;

    /// <summary>
    /// Gets the browser type this factory is responsible for, which is always <see cref="BrowserType.Chrome"/>.
    /// </summary>
    /// <inheritdoc cref="IBrowserDriverFactoryService.Type" />
    /// 
    /// /// <summary>
    /// Gets the specific <see cref="BrowserType"/> (Chrome) that this factory implementation handles.
    /// </summary>
    /// <inheritdoc cref="ChromiumDriverFactoryServiceBase.ConcreteBrowserType" />
    protected override BrowserType ConcreteBrowserType => BrowserType.Chrome;

    /// <summary>
    /// Gets the minimum supported version for the Google Chrome browser handled by this factory.
    /// </summary>
    /// <inheritdoc cref="ChromiumDriverFactoryServiceBase.MinimumSupportedVersion" />
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriverFactoryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers, passed to the base class.</param>
    public ChromeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
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

    /// <inheritdoc cref="IBrowserDriverFactoryService.CreateDriver(BaseBrowserSettings, DriverOptions)" />
    /// <summary>
    /// Creates and configures a WebDriver instance for Chrome, supporting both local and remote (Selenium Grid) execution.
    /// </summary>
    /// <remarks>
    /// This method orchestrates the creation of a Chrome WebDriver by following these steps:
    /// <list type="number">
    ///   <item><description>Validates that the provided settings are of type <see cref="ChromeSettings"/>.</description></item>
    ///   <item><description>Configures <see cref="ChromeOptions"/> with common settings (headless, window size, etc.) and any custom arguments.</description></item>
    ///   <item><description>Checks if a <see cref="BaseBrowserSettings.SeleniumGridUrl"/> is specified in the settings.</description></item>
    ///   <item><description><b>If a Grid URL is present</b>, it creates a <see cref="RemoteWebDriver"/> instance pointing to the grid.</description></item>
    ///   <item><description><b>If no Grid URL is present</b>, it uses <see cref="WebDriverManager"/> to set up the local `chromedriver.exe` and then creates a local <see cref="ChromeDriver"/> instance.</description></item>
    ///   <item><description>Performs a browser version check against the minimum supported version.</description></item>
    /// </list>
    /// </remarks>
    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not ChromeSettings settings)
        {
            var ex = new ArgumentException(
                $"Invalid settings type provided. Expected {nameof(ChromeSettings)}, got {settingsBase.GetType().Name}.",
                nameof(settingsBase)
            );
            ServiceLogger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(ChromeDriverFactoryService));
            throw ex;
        }

        ServiceLogger.LogInformation(
            "Creating {BrowserType} WebDriver. Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type,
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        ChromeOptions chromeOptions = ConfigureCommonChromeOptions(settings, options, out List<string> appliedOptionsForLog);

        string chromeExecutablePath = GetChromeExecutablePathInternal();
        if (!string.IsNullOrEmpty(chromeExecutablePath) && File.Exists(chromeExecutablePath))
            chromeOptions.BinaryLocation = chromeExecutablePath;
        else
            ServiceLogger.LogDebug("Chrome binary location not explicitly set in options; Selenium will use default system path or detected driver's expectation.");

        ServiceLogger.LogInformation(
            "ChromeOptions configured for {BrowserType}. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]",
            Type, chromeOptions.BinaryLocation ?? "Default/System PATH",
            string.Join(", ",
            appliedOptionsForLog.Distinct())
        );

        if (string.IsNullOrEmpty(settings.SeleniumGridUrl))
        {
            ServiceLogger.LogInformation("Creating local ChromeDriver instance.");

            ServiceLogger.LogDebug("Attempting to set up ChromeDriver using WebDriverManager (ChromeConfig).");
            try
            {
                _ = new DriverManager().SetUpDriver(new ChromeConfig());
                ServiceLogger.LogInformation("WebDriverManager successfully completed ChromeDriver setup (ChromeConfig).");
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "WebDriverManager failed to set up ChromeDriver (ChromeConfig).");
                throw;
            }

            return CreateDriverInstanceWithChecks(chromeOptions);
        }
        else
        {
            ServiceLogger.LogInformation("Creating RemoteWebDriver instance for Chrome Grid at {GridUrl}", settings.SeleniumGridUrl);
            var remoteDriver = new RemoteWebDriver(new Uri(settings.SeleniumGridUrl), chromeOptions);
            PerformVersionCheck(remoteDriver, Type.ToString(), MinimumSupportedVersion);
            return remoteDriver;
        }
    }
}
