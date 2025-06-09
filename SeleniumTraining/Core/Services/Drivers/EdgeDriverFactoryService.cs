using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Edge;
using System.Runtime.InteropServices;

namespace SeleniumTraining.Core.Services.Drivers;

public class EdgeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Edge;
    protected override BrowserType ConcreteBrowserType => BrowserType.Edge;
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    public EdgeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(EdgeDriverFactoryService), Type);
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not EdgeSettings settings)
        {
            var ex = new ArgumentException(
                $"Invalid settings type provided. Expected {nameof(EdgeSettings)}, got {settingsBase.GetType().Name}.",
                nameof(settingsBase)
            );
            ServiceLogger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(EdgeDriverFactoryService));
            throw ex;
        }

        ServiceLogger.LogInformation(
            "Creating {BrowserType} WebDriver. Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type,
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        EdgeOptions edgeOptions = options as EdgeOptions ?? new EdgeOptions();
        var appliedOptionsForLog = new List<string>();

        string windowSizeArgument = GetWindowSizeArgumentInternal(settings);
        if (!string.IsNullOrEmpty(windowSizeArgument))
        {
            edgeOptions.AddArgument(windowSizeArgument);
            appliedOptionsForLog.Add(windowSizeArgument);
        }

        if (settings.Headless && !string.IsNullOrEmpty(settings.ChromeHeadlessArgument))
        {
            edgeOptions.AddArgument(settings.ChromeHeadlessArgument);
            appliedOptionsForLog.Add(settings.ChromeHeadlessArgument);
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            ServiceLogger.LogWarning("LeaveBrowserOpenAfterTest=true is not directly supported by EdgeOptions.");
        }

        if (settings.ChromeArguments != null && settings.ChromeArguments.Count != 0)
        {
            edgeOptions.AddArguments(settings.ChromeArguments);
            appliedOptionsForLog.AddRange(settings.ChromeArguments);
        }

        string edgeExecutablePath = GetEdgeExecutablePathInternal();
        if (!string.IsNullOrEmpty(edgeExecutablePath) && File.Exists(edgeExecutablePath))
        {
            edgeOptions.BinaryLocation = edgeExecutablePath;
        }

        ServiceLogger.LogInformation(
            "EdgeOptions configured for {BrowserType}. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]",
            Type,
            edgeOptions.BinaryLocation ?? "Default/System PATH",
            string.Join(", ", appliedOptionsForLog.Distinct())
        );

        if (string.IsNullOrEmpty(settings.SeleniumGridUrl))
        {
            ServiceLogger.LogInformation("Creating local EdgeDriver instance.");
            ServiceLogger.LogDebug("Attempting to set up EdgeDriver using WebDriverManager (EdgeConfig).");
            try
            {
                _ = new DriverManager().SetUpDriver(new EdgeConfig());
                ServiceLogger.LogInformation("WebDriverManager successfully completed EdgeDriver setup (EdgeConfig).");
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "WebDriverManager failed to set up EdgeDriver (EdgeConfig).");
                throw;
            }

            var localDriver = new EdgeDriver(edgeOptions);
            PerformVersionCheck(localDriver, Type.ToString(), MinimumSupportedVersion);
            return localDriver;
        }
        else
        {
            ServiceLogger.LogInformation("Creating RemoteWebDriver instance for Edge Grid at {GridUrl}", settings.SeleniumGridUrl);
            var remoteDriver = new RemoteWebDriver(new Uri(settings.SeleniumGridUrl), edgeOptions);
            PerformVersionCheck(remoteDriver, Type.ToString(), MinimumSupportedVersion);
            return remoteDriver;
        }
    }

    private string GetEdgeExecutablePathInternal()
    {
        ServiceLogger.LogDebug("Searching for Edge executable...");
        string? edgePathEnv = Environment.GetEnvironmentVariable("EDGE_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(edgePathEnv) && File.Exists(edgePathEnv))
        {
            ServiceLogger.LogInformation("Using Edge executable from EDGE_EXECUTABLE_PATH: {EdgePathEnv}", edgePathEnv);
            return edgePathEnv;
        }

        string[] pathsToTry;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            pathsToTry = [Path.Combine(progFilesX86, "Microsoft", "Edge", "Application", "msedge.exe")];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pathsToTry = ["/usr/bin/microsoft-edge-stable", "/usr/bin/microsoft-edge-dev"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            pathsToTry = ["/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge"];
        }
        else
        {
            ServiceLogger.LogWarning("Unsupported OS platform for Edge path detection: {OSPlatform}", RuntimeInformation.OSDescription);
            return string.Empty;
        }

        foreach (string path in pathsToTry)
        {
            if (File.Exists(path))
            {
                ServiceLogger.LogInformation("Found Edge executable at: {EdgePath}", path);
                return path;
            }
            ServiceLogger.LogDebug("Edge executable not found at: {PathToTry}", path);
        }

        ServiceLogger.LogWarning("Edge executable not found in standard installation paths for OS {OSPlatform}.", RuntimeInformation.OSDescription);
        return string.Empty;
    }
}
