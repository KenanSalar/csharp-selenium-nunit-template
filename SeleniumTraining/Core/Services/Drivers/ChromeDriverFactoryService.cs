using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Chrome;
    protected override BrowserType ConcreteBrowserType => BrowserType.Chrome;
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    public ChromeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
    }

    private string GetChromeExecutablePathInternal()
    {
        Logger.LogDebug("Searching for Chrome executable...");
        string? chromePathEnv = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(chromePathEnv) && File.Exists(chromePathEnv))
        {
            Logger.LogInformation("Using Chrome executable from CHROME_EXECUTABLE_PATH: {ChromePathEnv}", chromePathEnv);
            return chromePathEnv;
        }

        string[] pathsToTry;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            pathsToTry =
            [
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
            Logger.LogWarning("Unsupported OS platform for Chrome path detection: {OSPlatform}", RuntimeInformation.OSDescription);
            return string.Empty;
        }

        foreach (string path in pathsToTry)
        {
            if (File.Exists(path))
            {
                Logger.LogInformation("Found Chrome executable at: {ChromePath}", path);
                return path;
            }
            Logger.LogDebug("Chrome executable not found at: {PathToTry}", path);
        }
        
        Logger.LogWarning("Chrome executable not found in standard installation paths for OS {OSPlatform}.", RuntimeInformation.OSDescription);
        return string.Empty;
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not ChromeSettings settings)
        {
            var ex = new ArgumentException(
                $"Invalid settings type provided. Expected {nameof(ChromeSettings)}, got {settingsBase.GetType().Name}.",
                nameof(settingsBase)
            );
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(ChromeDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating {BrowserType} WebDriver. Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type,
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        string chromeExecutablePath = GetChromeExecutablePathInternal();
        
        if (string.IsNullOrEmpty(chromeExecutablePath)) 
        {
            Logger.LogWarning("Chrome executable path not found. WebDriverManager will use default detection if possible, and Selenium will rely on PATH.");
        } 
        else 
        {
            Logger.LogInformation("Chrome executable path determined to be: {ChromePath}", chromeExecutablePath);
        }

        // try
        // {
        //     Logger.LogInformation("Attempting to set up ChromeDriver using WebDriverManager's default behavior for ChromeConfig (will attempt auto-detection).");
        //     _ = new DriverManager().SetUpDriver(new ChromeConfig());
        //     Logger.LogInformation("WebDriverManager successfully completed default ChromeDriver setup for Chrome.");
        // }
        // catch (Exception ex)
        // {
        //     Logger.LogError(ex, "WebDriverManager failed to set up ChromeDriver using default ChromeConfig.");
        //     throw;
        // }

        ChromeOptions chromeOptions = ConfigureCommonChromeOptions(settings, options, out List<string> appliedOptionsForLog);
        
        if (!string.IsNullOrEmpty(chromeExecutablePath) && File.Exists(chromeExecutablePath))
        {
            chromeOptions.BinaryLocation = chromeExecutablePath;
        }
        else
        {
            Logger.LogDebug("Chrome binary location not explicitly set in options; Selenium will use default system path or detected driver's expectation.");
        }

        Logger.LogInformation(
            "ChromeOptions configured for {BrowserType}. BinaryLocation: {BinaryLocation}. Effective arguments: [{EffectiveArgs}]",
            Type, chromeOptions.BinaryLocation ?? "Default/System PATH",
            string.Join(", ",
            appliedOptionsForLog.Distinct())
        );

        return CreateDriverInstanceWithChecks(chromeOptions);
    }
}
