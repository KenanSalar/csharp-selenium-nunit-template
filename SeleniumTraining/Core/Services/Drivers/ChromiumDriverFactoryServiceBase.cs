using OpenQA.Selenium.Chromium;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Provides a base class for creating WebDriver instances for Chromium-based browsers (e.g., Chrome, Edge).
/// It encapsulates logic that is 100% common to all Chromium browsers, like handling command-line arguments.
/// </summary>
/// <remarks>
/// This class implements <see cref="IBrowserDriverFactoryService"/> and requires derived classes to provide
/// an implementation for the abstract <see cref="Type"/> and <see cref="CreateDriver"/> members.
/// </remarks>
public abstract class ChromiumDriverFactoryServiceBase : DriverFactoryServiceBase, IBrowserDriverFactoryService
{
    /// <inheritdoc/>
    public abstract BrowserType Type { get; }

    /// <summary>
    /// Gets the minimum supported version for the specific Chromium-based browser handled by this factory.
    /// </summary>
    protected abstract Version MinimumSupportedVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumDriverFactoryServiceBase"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.</param>
    protected ChromiumDriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {

    }

    /// <inheritdoc/>
    public abstract IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null);

    /// <summary>
    /// Creates a new WebDriver instance using a provided factory function and performs standard checks.
    /// </summary>
    /// <typeparam name="TDriver">The type of WebDriver to create (e.g., ChromeDriver).</typeparam>
    /// <typeparam name="TOptions">The driver options type (e.g., ChromeOptions).</typeparam>
    /// <param name="driverOptions">The configured driver options.</param>
    /// <param name="driverFactory">A function that takes the options and returns a new driver instance.</param>
    /// <returns>A new, checked WebDriver instance.</returns>
    protected TDriver CreateDriverInstanceWithChecks<TDriver, TOptions>(
        TOptions driverOptions,
        Func<TOptions, TDriver> driverFactory
    )
        where TDriver : ChromiumDriver
        where TOptions : ChromiumOptions
    {
        ServiceLogger.LogDebug("Attempting to instantiate new {DriverType} with configured options.", typeof(TDriver).Name);
        TDriver driver;
        try
        {
            driver = driverFactory(driverOptions);
            ServiceLogger.LogInformation(
                "{BrowserType} WebDriver ({DriverType}) instance created successfully. Driver hash: {DriverHashCode}",
                Type, typeof(TDriver).Name, driver.GetHashCode()
            );
            PerformVersionCheck(driver, Type.ToString(), MinimumSupportedVersion);
            return driver;
        }
        catch (Exception ex)
        {
            LogAndThrowWebDriverCreationError(ex, Type, driverOptions, $"While creating {Type} driver.");
            throw;
        }
    }

    /// <summary>
    /// Configures common <see cref="ChromiumOptions"/> applicable to all Chromium-based browsers.
    /// </summary>
    /// <typeparam name="TChromiumOptions">The specific type of ChromiumOptions to configure (e.g., ChromeOptions).</typeparam>
    /// <param name="settings">The browser settings containing configuration values.</param>
    /// <param name="baseOptions">Optional existing options to build upon.</param>
    /// <param name="appliedOptionsForLog">An output list of strings representing the applied options for logging.</param>
    /// <returns>A configured instance of the specified ChromiumOptions type.</returns>
    protected TChromiumOptions ConfigureCommonChromiumOptions<TChromiumOptions>(
        ChromiumBasedSettings settings,
        DriverOptions? baseOptions,
        out List<string> appliedOptionsForLog
    )
        where TChromiumOptions : ChromiumOptions, new()
    {
        appliedOptionsForLog = [];
        TChromiumOptions chromiumOptions = baseOptions as TChromiumOptions ?? new TChromiumOptions();
        ServiceLogger.LogDebug("Initialized {OptionsType} for {BrowserType}.", typeof(TChromiumOptions).Name, Type);

        string windowSizeArgument = GetWindowSizeArgumentInternal(settings);
        if (!string.IsNullOrEmpty(windowSizeArgument))
        {
            chromiumOptions.AddArgument(windowSizeArgument);
            appliedOptionsForLog.Add(windowSizeArgument);
        }

        if (settings.Headless && !string.IsNullOrEmpty(settings.ChromeHeadlessArgument))
        {
            chromiumOptions.AddArgument(settings.ChromeHeadlessArgument);
            appliedOptionsForLog.Add(settings.ChromeHeadlessArgument);
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            chromiumOptions.AddAdditionalOption("detach", true);
            ServiceLogger.LogWarning("DEBUGGING: {BrowserType} browser will be left running after the test.", Type);
        }

        if (settings.ChromeArguments != null && settings.ChromeArguments.Count != 0)
        {
            ServiceLogger.LogDebug("Applying {ArgCount} custom Chrome arguments from settings.", settings.ChromeArguments.Count);
            foreach (string arg in settings.ChromeArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    chromiumOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                }
            }
        }

        return chromiumOptions;
    }

    /// <summary>
    /// Generates the window size command-line argument string (e.g., "--window-size=1920,1080").
    /// </summary>
    /// <param name="settings">The settings containing width and height.</param>
    /// <returns>The formatted window size argument string, or an empty string if not specified.</returns>
    protected static string GetWindowSizeArgumentInternal(BaseBrowserSettings settings)
    {
        return settings.WindowWidth.HasValue && settings.WindowHeight.HasValue
            ? $"--window-size={settings.WindowWidth.Value},{settings.WindowHeight.Value}"
            : string.Empty;
    }
}
