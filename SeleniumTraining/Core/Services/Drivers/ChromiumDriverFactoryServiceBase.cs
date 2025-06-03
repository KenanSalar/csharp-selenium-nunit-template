using OpenQA.Selenium.Chrome;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Provides a base class for creating WebDriver instances for Chromium-based browsers (e.g., Chrome, Edge).
/// It encapsulates common logic for configuring <see cref="ChromeOptions"/> and performing
/// version checks specific to Chromium browsers.
/// </summary>
/// <remarks>
/// Derived classes must implement <see cref="ConcreteBrowserType"/> and <see cref="MinimumSupportedVersion"/>
/// to specify the particular Chromium browser and its minimum version requirement.
/// This class inherits from <see cref="DriverFactoryServiceBase"/>, which in turn provides
/// logging and potentially other base factory functionalities.
/// </remarks>
public abstract class ChromiumDriverFactoryServiceBase : DriverFactoryServiceBase
{
    /// <summary>
    /// Gets the specific <see cref="BrowserType"/> (e.g., Chrome, Edge) that the derived factory implementation handles.
    /// This must be implemented by concrete derived classes.
    /// </summary>
    /// <value>The <see cref="BrowserType"/> for this specific Chromium-based browser factory.</value>
    protected abstract BrowserType ConcreteBrowserType { get; }

    /// <summary>
    /// Gets the minimum supported version for the specific Chromium-based browser handled by this factory.
    /// This must be implemented by concrete derived classes.
    /// </summary>
    /// <value>The minimum <see cref="Version"/> that is supported.</value>
    protected abstract Version MinimumSupportedVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumDriverFactoryServiceBase"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers, passed to the base <see cref="DriverFactoryServiceBase"/>.</param>
    protected ChromiumDriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    /// <summary>
    /// Configures common <see cref="ChromeOptions"/> applicable to Chromium-based browsers
    /// using the provided settings. This method can be overridden by derived classes
    /// to add further browser-specific options.
    /// </summary>
    /// <param name="settings">The <see cref="ChromiumBasedSettings"/> containing configurations like headless mode,
    /// window size, and custom arguments. Must not be null.</param>
    /// <param name="baseOptions">Optional existing <see cref="DriverOptions"/>. If provided and is a <see cref="ChromeOptions"/> instance,
    /// it will be used as a base; otherwise, new <see cref="ChromeOptions"/> are created.</param>
    /// <param name="appliedOptionsForLog">An output list that will be populated with strings representing the options
    /// that were actually applied, for logging purposes.</param>
    /// <returns>A configured <see cref="ChromeOptions"/> instance.</returns>
    /// <remarks>
    /// This method handles standard options such as window size, headless mode,
    /// <c>LeaveBrowserRunning</c> (for debugging), and custom arguments specified in <paramref name="settings"/>.
    /// </remarks>
    protected virtual ChromeOptions ConfigureCommonChromeOptions(
        ChromiumBasedSettings settings,
        DriverOptions? baseOptions,
        out List<string> appliedOptionsForLog
    )
    {
        appliedOptionsForLog = [];
        ChromeOptions chromeOptions = baseOptions as ChromeOptions ?? new ChromeOptions();
        ServiceLogger.LogDebug(
            "Initialized ChromeOptions for {BrowserType}. Base options type: {OptionsBaseType}",
            ConcreteBrowserType,
            chromeOptions.GetType().BaseType?.Name ?? chromeOptions.GetType().Name
        );

        string windowSizeArgument = GetWindowSizeArgumentInternal(settings);
        if (!string.IsNullOrEmpty(windowSizeArgument))
        {
            chromeOptions.AddArgument(windowSizeArgument);
            appliedOptionsForLog.Add(windowSizeArgument);
            ServiceLogger.LogDebug("Applied window size argument for {BrowserType}: '{WindowSizeArgument}'", ConcreteBrowserType, windowSizeArgument);
        }

        if (settings.Headless && !string.IsNullOrEmpty(settings.ChromeHeadlessArgument))
        {
            chromeOptions.AddArgument(settings.ChromeHeadlessArgument);
            appliedOptionsForLog.Add(settings.ChromeHeadlessArgument);
            ServiceLogger.LogDebug("Applied headless argument for {BrowserType}: '{HeadlessArgument}'", ConcreteBrowserType, settings.ChromeHeadlessArgument);
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            chromeOptions.LeaveBrowserRunning = true;
            ServiceLogger.LogWarning("DEBUGGING: {BrowserType} browser will be left running after the test due to LeaveBrowserOpenAfterTest=true setting.", ConcreteBrowserType);
        }

        if (settings.ChromeArguments != null && settings.ChromeArguments.Count != 0)
        {
            ServiceLogger.LogDebug(
                "Applying {ArgCount} custom Chrome arguments from configuration settings for {BrowserType}.",
                settings.ChromeArguments.Count,
                ConcreteBrowserType
            );
            foreach (string arg in settings.ChromeArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    chromeOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                    ServiceLogger.LogTrace("Applied Chrome argument from settings for {BrowserType}: '{ChromeArgument}'", ConcreteBrowserType, arg);
                }
            }
        }
        return chromeOptions;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ChromeDriver"/> using the provided <see cref="ChromeOptions"/>
    /// and performs standard checks such as browser version verification.
    /// </summary>
    /// <param name="chromeOptions">The fully configured <see cref="ChromeOptions"/> to use for instantiating the ChromeDriver.</param>
    /// <returns>A new <see cref="ChromeDriver"/> instance.</returns>
    /// <exception cref="Exception">Re-throws any exception that occurs during driver creation after logging it
    /// via <see cref="DriverFactoryServiceBase.LogAndThrowWebDriverCreationError"/> (if that method throws).</exception>
    /// <remarks>
    /// This method centralizes the instantiation of the ChromeDriver and the subsequent call to
    /// <see cref="DriverFactoryServiceBase.PerformVersionCheck"/>. If driver creation fails,
    /// detailed error information is logged.
    /// </remarks>
    protected ChromeDriver CreateDriverInstanceWithChecks(ChromeOptions chromeOptions)
    {
        ServiceLogger.LogDebug("Attempting to instantiate new ChromeDriver (for {BrowserType}) with configured options.", ConcreteBrowserType);
        ChromeDriver driver;
        try
        {
            driver = new ChromeDriver(chromeOptions);

            ServiceLogger.LogInformation(
                "{BrowserType} WebDriver (via ChromeDriver) instance created successfully. Driver hash: {DriverHashCode}",
                ConcreteBrowserType,
                driver.GetHashCode()
            );

            PerformVersionCheck(driver, ConcreteBrowserType.ToString(), MinimumSupportedVersion);
            return driver;
        }
        catch (Exception ex)
        {
            LogAndThrowWebDriverCreationError(ex, ConcreteBrowserType, chromeOptions, $"While creating {ConcreteBrowserType} driver.");
            throw;
        }
    }

    /// <summary>
    /// Generates the window size command-line argument string (e.g., "--window-size=1920,1080")
    /// based on the width and height specified in the settings.
    /// </summary>
    /// <param name="settings">The <see cref="BaseBrowserSettings"/> (or derived type like <see cref="ChromiumBasedSettings"/>)
    /// containing <see cref="BaseBrowserSettings.WindowWidth"/> and <see cref="BaseBrowserSettings.WindowHeight"/>.</param>
    /// <returns>The formatted window size argument string, or an empty string if width or height is not specified.</returns>
    protected static string GetWindowSizeArgumentInternal(BaseBrowserSettings settings)
    {
        return settings.WindowWidth.HasValue && settings.WindowHeight.HasValue
            ? $"--window-size={settings.WindowWidth.Value},{settings.WindowHeight.Value}"
            : string.Empty;
    }
}
