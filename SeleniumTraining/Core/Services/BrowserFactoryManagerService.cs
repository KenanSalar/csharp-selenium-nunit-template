namespace SeleniumTraining.Core.Services;

/// <summary>
/// Manages and orchestrates the creation of WebDriver instances by selecting
/// and utilizing appropriate browser-specific factories.
/// </summary>
/// <remarks>
/// This service implements <see cref="IBrowserFactoryManagerService"/> ([2]) and acts as a central dispatcher
/// for WebDriver creation requests. It maintains a collection of registered
/// <see cref="IBrowserDriverFactoryService"/> instances and delegates the actual driver
/// instantiation to the factory corresponding to the requested <see cref="BrowserType"/>.
/// This class inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class BrowserFactoryManagerService : BaseService, IBrowserFactoryManagerService
{
    private readonly Dictionary<BrowserType, IBrowserDriverFactoryService> _factories;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserFactoryManagerService"/> class.
    /// </summary>
    /// <param name="factories">An enumerable collection of <see cref="IBrowserDriverFactoryService"/> instances,
    /// each responsible for a specific browser type. These will be registered with the manager. Must not be null.</param>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="factories"/> or <paramref name="loggerFactory"/> is null.</exception>
    /// <remarks>
    /// Upon construction, this service indexes the provided factories by their supported <see cref="BrowserType"/>.
    /// It logs the count of registered factories and details about each one. A warning is logged if no factories are provided,
    /// as this would prevent any WebDriver creation.
    /// </remarks>
    public BrowserFactoryManagerService(IEnumerable<IBrowserDriverFactoryService> factories, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _factories = factories.ToDictionary(f => f.Type);
        ServiceLogger.LogInformation(
            "{ServiceName} initialized with {FactoryCount} browser driver factories.",
            nameof(BrowserFactoryManagerService),
            _factories.Count
        );

        if (factories.Any())
        {
            foreach (KeyValuePair<BrowserType, IBrowserDriverFactoryService> factoryEntry in _factories)
            {
                ServiceLogger.LogDebug(
                    "Registered factory: BrowserType={RegisteredBrowserType}, FactoryType={RegisteredFactoryType}",
                    factoryEntry.Key,
                    factoryEntry.Value.GetType().Name
                );
            }
        }
        else
        {
            ServiceLogger.LogWarning("{ServiceName} initialized with ZERO browser driver factories. WebDriver creation will fail.", nameof(BrowserFactoryManagerService));
        }
    }

    /// <inheritdoc cref="IBrowserFactoryManagerService.UseBrowserDriver(BrowserType, BaseBrowserSettings, DriverOptions)" />
    /// <remarks>
    /// This implementation first logs the request details. It then attempts to find a registered
    /// <see cref="IBrowserDriverFactoryService"/> that matches the requested <paramref name="browserType"/>.
    /// If a matching factory is found, its <c>CreateDriver</c> method is invoked to instantiate the WebDriver.
    /// If no factory is registered for the requested <paramref name="browserType"/>, an <see cref="ArgumentException"/> is thrown.
    /// All significant steps and errors during the process are logged.
    /// </remarks>
    public IWebDriver UseBrowserDriver(BrowserType browserType, BaseBrowserSettings settings, DriverOptions? options = null)
    {
        ServiceLogger.LogInformation(
            "Requesting WebDriver creation for {BrowserTypeRequested}. Headless requested: {IsHeadlessRequested}, WindowSize requested: {RequestedWindowSize}",
            browserType,
            settings.Headless,
            (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
                ? $"{settings.WindowWidth.Value}x{settings.WindowHeight.Value}"
                : "Default (not specified)"
        );

        if (_factories.TryGetValue(browserType, out IBrowserDriverFactoryService? factory))
        {
            ServiceLogger.LogDebug(
                "Found factory {SelectedFactoryType} for requested browser {BrowserTypeRequested}.",
                factory.GetType().Name,
                browserType
            );
            try
            {
                IWebDriver driver = factory.CreateDriver(settings, options);

                ServiceLogger.LogInformation(
                    "Successfully created WebDriver for {BrowserTypeRequested} using factory {SelectedFactoryType}. Driver hash: {DriverHashCode}",
                    browserType,
                    factory.GetType().Name,
                    driver.GetHashCode()
                );
                return driver;
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(
                    ex,
                    "Error creating WebDriver for {BrowserTypeRequested} using factory {SelectedFactoryType}.",
                    browserType,
                    factory.GetType().Name
                );
                throw;
            }
        }
        else
        {
            string availableFactories = string.Join(", ", _factories.Keys.Select(k => k.ToString()));
            ServiceLogger.LogError(
                "Unsupported browser: {BrowserTypeRequested}. No factory registered for this browser type. Available factories: [{RegisteredFactories}]",
                browserType,
                string.IsNullOrEmpty(availableFactories)
                    ? "None"
                    : availableFactories
            );

            throw new ArgumentException($"Unsupported browser: {browserType}. No factory registered. Available types: [{availableFactories}]", nameof(browserType));
        }
    }
}
