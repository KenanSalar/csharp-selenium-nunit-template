namespace SeleniumTraining.Core.Services;

public class BrowserFactoryManagerService : BaseService, IBrowserFactoryManagerService
{
    private readonly Dictionary<BrowserType, IBrowserDriverFactoryService> _factories;

    public BrowserFactoryManagerService(IEnumerable<IBrowserDriverFactoryService> factories, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _factories = factories.ToDictionary(f => f.Type);
        Logger.LogInformation(
            "{ServiceName} initialized with {FactoryCount} browser driver factories.",
            nameof(BrowserFactoryManagerService),
            _factories.Count
        );

        if (factories.Any())
        {
            foreach (KeyValuePair<BrowserType, IBrowserDriverFactoryService> factoryEntry in _factories)
            {
                Logger.LogDebug(
                    "Registered factory: BrowserType={RegisteredBrowserType}, FactoryType={RegisteredFactoryType}",
                    factoryEntry.Key,
                    factoryEntry.Value.GetType().Name
                );
            }
        }
        else
        {
            Logger.LogWarning("{ServiceName} initialized with ZERO browser driver factories. WebDriver creation will fail.", nameof(BrowserFactoryManagerService));
        }
    }

    public IWebDriver UseBrowserDriver(BrowserType browserType, BaseBrowserSettings settings, DriverOptions? options = null)
    {
        Logger.LogInformation(
            "Requesting WebDriver creation for {BrowserTypeRequested}. Headless requested: {IsHeadlessRequested}, WindowSize requested: {RequestedWindowSize}",
            browserType,
            settings.Headless,
            (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
                ? $"{settings.WindowWidth.Value}x{settings.WindowHeight.Value}"
                : "Default (not specified)"
        );

        if (_factories.TryGetValue(browserType, out IBrowserDriverFactoryService? factory))
        {
            Logger.LogDebug(
                "Found factory {SelectedFactoryType} for requested browser {BrowserTypeRequested}.",
                factory.GetType().Name,
                browserType
            );
            try
            {
                IWebDriver driver = factory.CreateDriver(settings, options);

                Logger.LogInformation(
                    "Successfully created WebDriver for {BrowserTypeRequested} using factory {SelectedFactoryType}. Driver hash: {DriverHashCode}",
                    browserType,
                    factory.GetType().Name,
                    driver.GetHashCode()
                );
                return driver;
            }
            catch (Exception ex)
            {
                Logger.LogError(
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
            Logger.LogError(
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
